// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Interop.Com;
using Microsoft.Win32;
using OpenLiveWriter.CoreServices.Settings;
using System.Security.Principal;

namespace OpenLiveWriter.CoreServices
{
    [Flags]
    public enum RunningObjectTableFlags
    {
        /// <summary>
        /// When set, indicates a strong registration for the object.
        /// </summary>
        /// <remarks>
        /// When an object is registered, the ROT always calls AddRef on the object. For a weak registration
        /// (ROTFLAGS_REGISTRATIONKEEPSALIVE not set), the ROT will release the object whenever the last strong
        /// reference to the object is released. For a strong registration (ROTFLAGS_REGISTRATIONKEEPSALIVE set), the
        /// ROT prevents the object from being destroyed until the object's registration is explicitly revoked.
        /// </remarks>
        RegistrationKeepsAlive = 0x1,

        /// <summary>
        /// When set, any client can connect to the running object through its entry in the ROT. When not set, only
        /// clients in the window station that registered the object can connect to it.
        /// </summary>
        /// <remarks>
        /// A server registered as either LocalService or RunAs can set the ROTFLAGS_ALLOWANYCLIENT flag in its call
        /// to Register to allow any client to connect to it. A server setting this bit must have its executable name
        /// in the AppID section of the registry that refers to the AppID for the executable. An "activate as
        /// activator" server (not registered as LocalService or RunAs) must not set this flag in its call to Register.
        /// </remarks>
        AllowAnyClient = 0x2,
    }

    /// <summary>
    /// Handy wrapper for the COM Running Object Table. The scope
    /// for this table is session-wide.
    /// </summary>
    public class RunningObjectTable : IDisposable
    {
        private static readonly string APP_ID = "{6DC87AF3-AA96-47AD-9F42-E440C61CE4FB}";
        private static readonly string EXE_REGISTRY_PATH = @"Software\Classes\AppID\OpenLiveWriter.exe";
        private static readonly string APP_ID_REGISTRY_PATH = @"Software\Classes\AppID\" + APP_ID;
        private static readonly string ALLOW_ANY_CLIENT = "AllowAnyClient";

        public static void EnsureComRegistration()
        {
            // Are we running with Administrator privileges? If so, we need to write to HKLM. Otherwise, need to write to HKCU.
            RegistryKey rootKey = IsInAdministratorRole ? Registry.LocalMachine : Registry.CurrentUser;

            try
            {
                // A client calling IRunningObjectTable::Register with the ROTFLAGS_ALLOWANYCLIENT bit must have its
                // executable name in the AppID section of the registry that refers to the AppID for the executable.
                var exeRegistrySettings = new RegistrySettingsPersister(rootKey, EXE_REGISTRY_PATH);
                exeRegistrySettings.Set("AppID", APP_ID);

                // A RunAs entry must be present because the system prohibits "activate as activator" processes from registering in
                // the ROT with ROTFLAGS_ALLOWANYCLIENT.This is done for security reasons.
                var appIdRegistrySettings = new RegistrySettingsPersister(rootKey, APP_ID_REGISTRY_PATH);
                appIdRegistrySettings.Set("", "OpenLiveWriter.exe");
                appIdRegistrySettings.Set("RunAs", "Interactive User");

                if (IsInAdministratorRole)
                {
                    // If we wrote to HKLM, even medium integrity processes can now use ROTFLAGS_ALLOWANYCLIENT.
                    // However, medium integrity processes can't read from HKLM to see if it's set or not, so we
                    // need a duplicate setting to store this information.
                    exeRegistrySettings = new RegistrySettingsPersister(Registry.CurrentUser, EXE_REGISTRY_PATH);
                    exeRegistrySettings.Set("AllowAnyClient", true);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception thrown while setting RunningObjectTable COM registration: \r\n" + ex);
                if (!RegistryHelper.IsRegistryException(ex))
                    throw;
            }
        }

        private IRunningObjectTable _rot;

        public RunningObjectTable()
        {
            int result = Ole32.GetRunningObjectTable(0, out _rot);
            if (result != 0)
                throw new Win32Exception(result);
        }

        ~RunningObjectTable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases the Running Object Table instance.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
                if (_rot != null)
                    Marshal.ReleaseComObject(_rot);
                _rot = null;
            }
        }

        /// <summary>
        /// Registers the COM object under the given name. The returned
        /// IDisposable must be disposed in order to remove the object
        /// from the table, otherwise resource leaks may result (I think).
        /// </summary>
        public IDisposable Register(string itemName, object obj)
        {
            IMoniker mk = CreateMoniker(itemName);

            // The ALLOW_ANY_CLIENT setting will be set to true if we've been launched via 'Run as administrator' at
            // least once because this means we wrote the HKLM registry entries to allow passing the
            // ROTFLAGS_ALLOWANYCLIENT flag.
            var exeRegistrySettings = new RegistrySettingsPersister(Registry.CurrentUser, EXE_REGISTRY_PATH);
            bool allowAnyClient = (bool)exeRegistrySettings.Get(ALLOW_ANY_CLIENT, typeof(bool), false);
            int rotRegistrationFlags = allowAnyClient ?
                (int)(RunningObjectTableFlags.RegistrationKeepsAlive | RunningObjectTableFlags.AllowAnyClient) :
                (int)(RunningObjectTableFlags.RegistrationKeepsAlive);

            try
            {
                int registration = _rot.Register(rotRegistrationFlags, obj, mk);
                return new RegistrationHandle(this, registration);
            }
            catch (COMException ex)
            {
                Trace.Fail("Exception thrown from IRunningObjectTable::Register: \r\n" + ex);

                // If registration failed, try again without ROTFLAGS_ALLOWANYCLIENT because the AllowAnyClient setting might be out of date.
                exeRegistrySettings.Set(ALLOW_ANY_CLIENT, false);
                rotRegistrationFlags = (int)(RunningObjectTableFlags.RegistrationKeepsAlive);
                int registration = _rot.Register(rotRegistrationFlags, obj, mk);
                return new RegistrationHandle(this, registration);
            }
        }

        private static IMoniker CreateMoniker(string itemName)
        {
            IMoniker mk;
            int result = Ole32.CreateItemMoniker("!", itemName, out mk);
            if (result != 0)
                throw new Win32Exception(result);
            return mk;
        }

        /// <summary>
        /// Attempts to retrieve an item from the ROT; returns null if not found.
        /// </summary>
        public object GetObject(string itemName)
        {
            try
            {
                IMoniker mk = CreateMoniker(itemName);

                object obj;
                int hr = _rot.GetObject(mk, out obj);

                if (hr != HRESULT.S_OK)
                {
                    Trace.WriteLine(String.Format("ROT.GetObject returned HRESULT 0x{0:x}.", hr));
                    return null;
                }
                return obj;
            }
            catch (COMException e)
            {
                if (e.ErrorCode == RPC_E.CALL_FAILED_DNE)
                    return null;

                throw;
            }
        }

        private void Revoke(int registration)
        {
            _rot.Revoke(registration);
        }

        public static bool IsInAdministratorRole
        {
            get
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private class RegistrationHandle : IDisposable
        {
            private RunningObjectTable _rot;
            private int _registration;

            public RegistrationHandle(RunningObjectTable rot, int registration)
            {
                _rot = rot;
                _registration = registration;
            }

            ~RegistrationHandle()
            {
                Debug.Fail("RegistrationHandle was not disposed!!");

                // Can't use normal Dispose() because the _rot may already
                // have been garbage collected by this point
                IRunningObjectTable rot;
                int result = Ole32.GetRunningObjectTable(0, out rot);
                if (result == 0)
                {
                    rot.Revoke(_registration);
                    Marshal.ReleaseComObject(rot);
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                _rot.Revoke(_registration);
            }
        }
    }
}
