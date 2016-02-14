// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32.SafeHandles;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Interop.Windows;
using Microsoft.Win32;

namespace OpenLiveWriter.CoreServices
{
    #region RegistryKey Event and Delegate
    public class RegistryKeyEventArgs : EventArgs
    {
        private string _key; //identifies the registry key
        private string _fullKey; //identifies the registry key
        private UIntPtr _hKey;
        public RegistryKeyEventArgs(UIntPtr hkey, string key, string fullKey)
        {
            _hKey = hkey;
            _key = key;
            _fullKey = fullKey;
        }

        /// <summary>
        /// The path of the registry key (without the HKEY value).
        /// </summary>
        public string Key
        {
            get
            {
                return _key;
            }
        }

        /// <summary>
        /// The HKEY base of the registry key.
        /// </summary>
        public UIntPtr HKey
        {
            get
            {
                return _hKey;
            }
        }

        /// <summary>
        /// A string representation of the full registry key.
        /// </summary>
        public string FullKey
        {
            get
            {
                return _fullKey;
            }
        }
    }

    /// <summary>
    /// Delegate method fpr handling RegistryKey events.
    /// </summary>
    public delegate void RegistryKeyEventHandler(object sender, RegistryKeyEventArgs evt);
    #endregion

    /// <summary>
    /// Summary description for RegistryMonitor.
    /// </summary>
    public class RegistryMonitor
    {
        //the maximum number of handles supported by the WaitForMultipleObjects call
        public static readonly int MAX_MONITOR_HANDLES = 64;

        #region Members/Initialization
        private RegistryMonitor()
        {
            keyMonitors = new SortedList();
            workerQueue = new BackgroundWorkerQueue(3);

            //initialize the fixed set of monitor events.
            InitFixedMonitorEvents();
        }
        private SortedList keyMonitors;
        private BackgroundWorkerQueue workerQueue;
        private Thread monitorThread;

        /// <summary>
        /// Initializes the fixed set of monitor events.
        /// </summary>
        private void InitFixedMonitorEvents()
        {
            monitorHandlesUpdatedEvent = new ManualResetEvent(false);
            monitorAbortEvent = new ManualResetEvent(false);
            FIXED_MONITOR_EVENTS = new ManualResetEvent[] { monitorHandlesUpdatedEvent, monitorAbortEvent };
        }
        private ManualResetEvent monitorHandlesUpdatedEvent;
        private ManualResetEvent monitorAbortEvent;
        private readonly int MONITOR_HANDLES_UPDATED_INDEX = 0;
        private readonly int MONITOR_ABORT_INDEX = 1;
        private ManualResetEvent[] FIXED_MONITOR_EVENTS;

        /// <summary>
        /// Returns the RegistryMonitor singleton instance.
        /// </summary>
        public static RegistryMonitor Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RegistryMonitor();
                return _instance;
            }
        }
        private static RegistryMonitor _instance;

        #endregion

        #region Public Methods

        public void AddRegistryChangeListener(UIntPtr hkey, string key, RegistryKeyEventHandler callback)
        {
            AddRegistryChangeListener(hkey, key, callback, false);
        }
        /// <summary>
        /// Register a method for callbacks when the specified registry key is changed.
        /// Note: this operation does not recursively register for notifications from subkeys!
        /// </summary>
        /// <param name="hkey">the HKEY constant (HKEY.CURRENT_USER or HKEY.CLASSES_ROOT)</param>
        /// <param name="key">the registry key path (example: @"Software\AppDataLow\Software\Onfolio\Preferences\Appearance"</param>
        /// <param name="callback">the callback delegate that is invoked when the registry key changes</param>
        public void AddRegistryChangeListener(UIntPtr hkey, string key, RegistryKeyEventHandler callback, bool autoCreateKey)
        {
            if (!autoCreateKey && !RegistryHelper.KeyExists(hkey, key))
                return;

            lock (this)
            {
                RegistryKeyMonitor keyMonitor = GetRegistryKeyMonitor(hkey, key, autoCreateKey);
                keyMonitor.AddChangeListener(callback);

                //launch the monitor thread if it hasn't been started.
                if (monitorThread == null)
                {
                    monitorThread = new Thread(new ThreadStart(this.MonitorChanges));
                    monitorThread.IsBackground = true;
                    monitorThread.Name = "Registry Monitor";
                    monitorThread.Start();
                }
            }
        }

        /// <summary>
        /// Deregister for registry change events.
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        public void RemoveRegistryChangeListener(UIntPtr hkey, string key, RegistryKeyEventHandler callback)
        {
            lock (this)
            {
                RegistryKeyMonitor keyMonitor = GetRegistryKeyMonitor(hkey, key);
                keyMonitor.RemoveChangeListener(callback);

                if (!keyMonitor.HasListeners())
                {
                    //TODO: deregister for change key events
                    //Debug.WriteLine("stop listening for changes to registry key: " + keyMonitor.FullKey, "RegistryMonitor");
                    keyMonitors.Remove(keyMonitor.FullKey);
                    keyMonitor.settingsChangedEvent.Close();

                    //Reset the list of monitor handles.
                    ResetMonitorHandles();

                    //if there are no more listeners, abort the monitor thread.
                    if (keyMonitors.Count == 0)
                    {
                        try
                        {
                            monitorAbortEvent.Set();
                        }
                        catch (ObjectDisposedException)
                        {
                            //occurs when removing change listeners while shutting down.
                        }
                    }
                }
            }
        }
        #endregion

        #region Private Methods

        private RegistryKeyMonitor GetRegistryKeyMonitor(UIntPtr hkey, string key)
        {
            return GetRegistryKeyMonitor(hkey, key, false);
        }
        /// <summary>
        /// Retrieves the RegistryKeyMonitor associated with the specified registry key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private RegistryKeyMonitor GetRegistryKeyMonitor(UIntPtr hkey, string key, bool autoCreateKey)
        {
            string fullKey = RegistryHelper.GetKeyString(hkey, key);
            RegistryKeyMonitor monitor = keyMonitors[fullKey] as RegistryKeyMonitor;
            if (monitor == null)
            {
                monitor = CreateRegistryKeyMonitor(hkey, key, autoCreateKey);
                keyMonitors[monitor.FullKey] = monitor;
                ResetMonitorHandles();
                //Debug.WriteLine("start listening for changes to registry key: " + monitor.FullKey, "RegistryMonitor");
            }
            return monitor;
        }

        /// <summary>
        /// Creates a key monitor object and registers it with the OS for registry change events.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private RegistryKeyMonitor CreateRegistryKeyMonitor(UIntPtr hkey, string key, bool autoCreateKey)
        {
            if (autoCreateKey)
            {
                RegistryHelper.CreateKey(hkey, key);
            }
            RegistryKeyMonitor monitor = new RegistryKeyMonitor(this, hkey, key);
            ConfigureChangeMonitoring(monitor, false);
            return monitor;
        }

        /// <summary>
        /// Configure change monitoring for this monitor object
        /// </summary>
        /// <param name="subKey"></param>
        private void ConfigureChangeMonitoring(RegistryKeyMonitor monitor, bool throwOnNotExist)
        {
            string key = monitor.Key;
            // assert preconditions
            Debug.Assert(monitor.hRawKey == UIntPtr.Zero);
            Debug.Assert(monitor.settingsChangedEvent == null);

            // open handle to registry key
            int result = Advapi32.RegOpenKeyEx(
                monitor.HKey,
                key,
                0, KEY.READ, out monitor.hRawKey);
            if (result != ERROR.SUCCESS)
            {
                if (throwOnNotExist)
                    throw new Exception("Failed to open registry key :" + key);
                else
                {
                    Debug.WriteLine("ConfigureChangeMonitoring error: " + result);
                    return;
                }
            }

            // create settings changed event
            monitor.settingsChangedEvent = new ManualResetEvent(false);

            // start monitoring changes
            RegisterForRegistryKeyChanged(monitor);
        }

        /// <summary>
        /// The list of Handles used to listen for ManualResetEvent events.
        /// </summary>
        private SafeWaitHandle[] MonitorHandles
        {
            get
            {
                lock (this)
                {
                    if (_monitorHandles == null)
                    {
                        ResetMonitorHandles();
                    }
                    return _monitorHandles;
                }
            }
        }
        SafeWaitHandle[] _monitorHandles;

        /// <summary>
        /// Rebuild the list of handles we are monitoring based on the keyMonitor table.
        /// </summary>
        private void ResetMonitorHandles()
        {
            //merge the FIXED_MONITOR_EVENTS and keyMonitor handles into a single array
            SafeWaitHandle[] monitorHandles = new SafeWaitHandle[FIXED_MONITOR_EVENTS.Length + keyMonitors.Count];

            //Assert if the number of handles exceeds the MAX number of handles supported by WaitForMultipleObjects
            //Trace.Assert(monitorHandles < MAX_MONITOR_HANDLES, "Too many registry keys are being monitored");

            int index;
            //add the fixed monitor events to the handles array
            for (index = 0; index < FIXED_MONITOR_EVENTS.Length; index++)
                monitorHandles[index] = FIXED_MONITOR_EVENTS[index].SafeWaitHandle;

            //add the registry monitor events to the handles array
            foreach (RegistryKeyMonitor keyMonitor in keyMonitors.Values)
            {
                monitorHandles[index++] = keyMonitor.settingsChangedEvent.SafeWaitHandle;
            }
            //replace the old list with the new list
            _monitorHandles = monitorHandles;

            //Signal the monitor thread to wake up so that the new handles will be monitored.
            //Note: Making this call will cause the registry monitor thread to pop out of its call to
            //WaitForMultipleObjects, where it will notice the the monitorHandlesUpdatedEvent was signaled.
            try
            {
                monitorHandlesUpdatedEvent.Set();
            }
            catch (ObjectDisposedException)
            {
                //occurs when removing change listeners while shutting down.
            }
        }

        private class SafeHandleArrayHelper : IDisposable
        {
            private SafeHandle[] _safeHandles;
            private IntPtr[] _intPtrs;

            public SafeHandleArrayHelper(SafeHandle[] handles)
            {
                _safeHandles = new SafeHandle[handles.Length];
                _intPtrs = new IntPtr[handles.Length];

                try
                {
                    for (int i = 0; i < handles.Length; i++)
                    {
                        SafeHandle handle = handles[i];
                        bool release = false;
                        handle.DangerousAddRef(ref release);
                        if (!release)
                            throw new InvalidOperationException("Unable to Add a reference to a SafeHandle");

                        _safeHandles[i] = handle;
                        _intPtrs[i] = handle.DangerousGetHandle();
                    }
                }
                catch
                {
                    ReleaseSafeHandles();
                    throw;
                }
            }

            private void ReleaseSafeHandles()
            {
                foreach (SafeHandle handle in _safeHandles)
                    if (handle != null)
                        handle.DangerousRelease();
                _safeHandles = new SafeHandle[0];
                _intPtrs = new IntPtr[0];
            }

            public IntPtr[] IntPtrs
            {
                get
                {
                    return _intPtrs;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                Debug.Assert(disposing, "You must dispose the SafeHandleArrayHelper");
                ReleaseSafeHandles();
            }
        }

        /// <summary>
        /// Monitor changes in the registry.
        /// Note: This operation is the main Run() loop for the Registry Monitor Thread.
        /// </summary>
        private void MonitorChanges()
        {
            //use a hashtable to keep track of bulk changes to the registry so that we only fire one event each
            //if multiple changes are made to the registry at once (like when a key is renamed)
            Hashtable regChangeList = new Hashtable();
            while (true)
            {
                SafeHandle[] monitorHandles;
                RegistryKeyMonitor[] monitors;

                lock (this) //lock here so that we don't encounter race conditions if registry listeners are being added/removed
                {
                    monitorHandles = MonitorHandles;
                    monitors = (RegistryKeyMonitor[])ArrayHelper.CollectionToArray(keyMonitors.Values, (typeof(RegistryKeyMonitor)));
                }

                using (SafeHandleArrayHelper safeHandleArrayHelper = new SafeHandleArrayHelper(monitorHandles))
                {
                    uint result =
                        Kernel32.WaitForMultipleObjects((uint)monitorHandles.Length, safeHandleArrayHelper.IntPtrs, false,
                                                        Kernel32.INFINITE);
                    if (result >= 0 && result < FIXED_MONITOR_EVENTS.Length)
                    {
                        //a FIXED_MONITOR_EVENT occurred, so handle it.
                        if (result == MONITOR_HANDLES_UPDATED_INDEX)
                        {
                            //the monitorHandlesUpdatedEvent was signalled, so reset it and re-loop
                            //so that we get the latest list of events to monitor.
                            monitorHandlesUpdatedEvent.Reset();
                        }
                        else if (result == MONITOR_ABORT_INDEX)
                        {
                            //the monitorAbortEvent was signaled, so exit the method.
                            //Debug.WriteLine("registry monitor stopped", "RegistryMonitor");
                            monitorAbortEvent.Reset();
                            return;
                        }
                        else
                        {
                            Debug.Fail("unknown FIXED_MONITOR_EVENT detected!");
                        }
                    }
                    else if (result >= WAIT.OBJECT_0 && result <= (WAIT.OBJECT_0 + monitorHandles.Length - 1))
                    {
                        //one or more registry keys have changed, so loop to gather up all of the signaled events
                        uint nextResult = result;
                        while ((result >= WAIT.OBJECT_0 && result <= (WAIT.OBJECT_0 + monitorHandles.Length - 1))
                               && !(result >= 0 && result < FIXED_MONITOR_EVENTS.Length))
                        {
                            //a registry change event occurred, so add the monitor to the notification list.
                            RegistryKeyMonitor keyMonitor = monitors[result - FIXED_MONITOR_EVENTS.Length];
                            regChangeList[keyMonitor.FullKey] = keyMonitor;

                            //re-register the monitor for change events from the OS
                            RegisterForRegistryKeyChanged(keyMonitor);

                            //get the next pending event from the kernel (without blocking)
                            result =
                                Kernel32.WaitForMultipleObjects((uint)monitorHandles.Length, safeHandleArrayHelper.IntPtrs, false, 0);
                        }

                        //notify the listeners the keys have changed (use the backgroundWorkerQueue so that this thread can't get hosed!)
                        RegistryKeyMonitor[] changedMonitors =
                            (RegistryKeyMonitor[])
                            ArrayHelper.CollectionToArray(regChangeList.Values, typeof(RegistryKeyMonitor));
                        workerQueue.AddWorker(new WaitCallback(backgroundWorker_HandleRegistryKeyChanged),
                                              changedMonitors);

                        //clear the change list.
                        regChangeList.Clear();
                    }
                    else if (result == WAIT.FAILED)
                    {
                        Debug.Fail("Wait failed: " + Marshal.GetLastWin32Error());
                    }
                    else if (result == WAIT.TIMEOUT)
                    {
                        //just re-loop so that we have the latest list of events to monitor.
                    }
                    else
                    {
                        Debug.Fail("unexpected WaitForMultipleObjects() return code!: " + result);
                    }
                }
            }
        }

        /// <summary>
        /// Monitor changes in the registry key.
        /// </summary>
        private static void RegisterForRegistryKeyChanged(RegistryKeyMonitor monitor)
        {
            // reset the settings changed event so it will not be considered signaled until
            // another change is made to the specified key
            monitor.settingsChangedEvent.Reset();

            // request that the event be signaled when the registry key changes
            int result = Advapi32.RegNotifyChangeKeyValue(monitor.hRawKey,
                false, REG_NOTIFY_CHANGE.LAST_SET, monitor.settingsChangedEvent.SafeWaitHandle, true);
            if (result != ERROR.SUCCESS)
            {
                Trace.WriteLine("Unexpeced failure to monitor reg key (Error code: " + result);
            }
        }

        /// <summary>
        /// Handles a set of registry key changes.
        /// </summary>
        private static void HandleRegistryKeyChanged(IEnumerable<RegistryKeyMonitor> monitors)
        {
            foreach (RegistryKeyMonitor monitor in monitors)
            {
                try
                {
                    //notify listeners that the key changed
                    //Debug.WriteLine(String.Format("notify registry key changed: {0}", monitor.FullKey), "RegistryMonitor");
                    monitor.FireChange();
                }
                catch (Exception e)
                {
                    Trace.WriteLine("error handling registry update: " + e.Message);
                    Trace.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// BackgroundWorkerQueue-compatible implementation of the HandleRegistryKeyChanged method.
        /// </summary>
        /// <param name="param"></param>
        private void backgroundWorker_HandleRegistryKeyChanged(object param)
        {
            RegistryKeyMonitor[] monitors = (RegistryKeyMonitor[])param;
            HandleRegistryKeyChanged(monitors);
        }

        #endregion

        #region RegistryKeyMonitor Helper class

        /// <summary>
        /// Utility for holding data associated with a particular registry that is being monitored.
        /// </summary>
        internal class RegistryKeyMonitor
        {
            private RegistryKeyEventHandler keyHandlers = null;
            private RegistryMonitor _monitor;
            private string _key;
            private string _fullKey;
            private UIntPtr _hKey;

            /// <summary>
            /// Handle to underlying registry key used by this monitor (we need this in order
            /// to register for change notifications)
            /// </summary>
            internal UIntPtr hRawKey = UIntPtr.Zero;

            //the OS-level registry event.
            internal ManualResetEvent settingsChangedEvent;

            internal RegistryKeyMonitor(RegistryMonitor monitor, UIntPtr hkey, string key)
            {
                _monitor = monitor;
                _key = key;
                _hKey = hkey;
                _fullKey = RegistryHelper.GetKeyString(hkey, key);
            }

            /// <summary>
            /// Add a listener for changes to this object's registry key.
            /// </summary>
            /// <param name="handler"></param>
            internal void AddChangeListener(RegistryKeyEventHandler handler)
            {
                keyHandlers = (RegistryKeyEventHandler)Delegate.Combine(keyHandlers, handler);
            }

            /// <summary>
            /// Remove a listener for changes to this object's registry key.
            /// </summary>
            /// <param name="handler"></param>
            internal void RemoveChangeListener(RegistryKeyEventHandler handler)
            {
                keyHandlers = (RegistryKeyEventHandler)Delegate.Remove(keyHandlers, handler);
            }

            /// <summary>
            /// Notify listeners that this monitor's registry key has changed.
            /// </summary>
            internal virtual void FireChange()
            {
                FireChange(new RegistryKeyEventArgs(HKey, Key, FullKey));
            }

            /// <summary>
            /// Notify listeners that this monitor's registry key has changed.
            /// </summary>
            internal virtual void FireChange(RegistryKeyEventArgs e)
            {
                if (keyHandlers != null)
                {
                    keyHandlers(this, e);
                }
            }

            internal bool HasListeners()
            {
                return keyHandlers != null;
            }

            public string Key
            {
                get { return _key; }
            }

            public string FullKey
            {
                get { return _fullKey; }
            }

            public UIntPtr HKey
            {
                get { return _hKey; }
            }
        }
        #endregion
    }
}
