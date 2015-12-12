// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;
using System.Threading;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;
using Timer = System.Threading.Timer;

namespace OpenLiveWriter.PostEditor.BlogProviderButtons
{

    public class BlogProviderButtonNotificationSink : IDisposable
    {
        public BlogProviderButtonNotificationSink(Control synchronizeInvokeControl)
        {
            // save reference to sync invoke for firing events back on the UI thread
            _synchronizeInvokeControl = synchronizeInvokeControl;
        }

        // Must be called *after* hooking up to BlogProviderButtonNotificationReceived event
        public void CheckForNotifications()
        {
            // fire up a background thread to poll for notifications
            // (create disabled and only enable when we have notifications to check for)
            TimerCallback timerCallback = new TimerCallback(CheckForNotifications);
            _pollingTimer = new Timer(timerCallback, null, _disablePollingInterval, _disablePollingInterval);
        }

        public void Dispose()
        {
            try
            {
                Detach();

                lock (this)
                    _pollingTimer.Dispose();
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception disposing notification sink: " + ex.ToString());
            }
        }

        public void Attach(Blog blog)
        {
            // detach from any existing work
            Detach();

            // record context
            lock (this)
            {
                _blogId = blog.Id;
                _hostBlogId = blog.HostBlogId;
                _homepageUrl = blog.HomepageUrl;
                _postApiUrl = blog.PostApiUrl;
                _buttonIds = new ArrayList();

                foreach (IBlogProviderButtonDescription buttonDescription in blog.ButtonDescriptions)
                {
                    if (buttonDescription.SupportsNotification)
                        _buttonIds.Add(buttonDescription.Id);
                }

                // if we have notifications to poll for then force an immediate polling
                // (we will adjust the polling interval within the callback to the
                // standard interval after processing the first call)
                if (_buttonIds.Count > 0)
                {
                    _pollingTimer.Change(_immediatePollingInterval, _disablePollingInterval);
                }
            }
        }

        public void Detach()
        {
            try
            {
                lock (this)
                {
                    _blogId = null;
                    _hostBlogId = null;
                    _homepageUrl = null;
                    _buttonIds = null;
                    _pollingTimer.Change(_disablePollingInterval, _disablePollingInterval);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpeted exception detaching from blog provider button notification context: " + ex.ToString());
            }

        }

        private void CheckForNotifications(object state)
        {
            try
            {
                // check for notifications
                foreach (BlogProviderButton button in GetButtons())
                    button.CheckForNotification();

                // set timer to standard internval
                lock (this)
                {
                    if (_buttonIds != null)
                        _pollingTimer.Change(_standardPollingInterval, _standardPollingInterval);
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception in BlogProviderButtonNotificationSink.Main: " + ex.ToString());
            }
        }

        private BlogProviderButton[] GetButtons()
        {
            ArrayList buttons = new ArrayList();
            lock (this)
            {
                if (_buttonIds != null)
                {
                    foreach (string buttonId in _buttonIds)
                        buttons.Add(new BlogProviderButton(_blogId, _hostBlogId, _homepageUrl, _postApiUrl, buttonId));
                }
            }
            return buttons.ToArray(typeof(BlogProviderButton)) as BlogProviderButton[];
        }

        // NOTE: Special handling of this event to ensure that all windows currently listening
        // for button notification events receive the notification
        public event BlogProviderButtonNotificationReceivedHandler BlogProviderButtonNotificationReceived
        {
            add
            {
                RegisterButtonNotificationListener(_synchronizeInvokeControl, value);
            }
            remove
            {
                UnregisterButtonNotificationListener(_synchronizeInvokeControl, value);
            }
        }

        // fired by the button when its state changes
        internal static void FireNotificationEvent(string blogId, string buttonId)
        {
            try
            {
                lock (_buttonNotificationListeners)
                {
                    // now notify all of the listeners asynchronously
                    foreach (DictionaryEntry listener in _buttonNotificationListeners)
                    {
                        Control control = listener.Value as Control;
                        if (ControlHelper.ControlCanHandleInvoke(control))
                        {
                            control.BeginInvoke(listener.Key as BlogProviderButtonNotificationReceivedHandler, new object[] { blogId, buttonId });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception firing button notification event: " + ex.ToString());
            }
        }

        private static void RegisterButtonNotificationListener(Control controlContext, BlogProviderButtonNotificationReceivedHandler listener)
        {
            lock (_buttonNotificationListeners)
            {
                _buttonNotificationListeners[listener] = controlContext;
            }
        }
        private static void UnregisterButtonNotificationListener(Control controlContext, BlogProviderButtonNotificationReceivedHandler listener)
        {
            lock (_buttonNotificationListeners)
            {
                _buttonNotificationListeners.Remove(listener);
            }
        }

        private static Hashtable _buttonNotificationListeners = new Hashtable();

        private string _blogId;
        private string _hostBlogId;
        private string _homepageUrl;
        private string _postApiUrl;
        private ArrayList _buttonIds;

        private Timer _pollingTimer;
        private readonly TimeSpan _disablePollingInterval = TimeSpan.FromMilliseconds(-1);
        private readonly TimeSpan _immediatePollingInterval = TimeSpan.FromMilliseconds(100);
        private readonly TimeSpan _standardPollingInterval = TimeSpan.FromSeconds(61);
        // logically, 60 seconds should work however we set to 61 just
        // for superstition's sake :-)

        private readonly Control _synchronizeInvokeControl;

    }

    public delegate void BlogProviderButtonNotificationReceivedHandler(string blogId, string buttonId);
}
