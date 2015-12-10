// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using Microsoft.Win32;
using System.Collections;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    // constants and utility functions useful in working with the registry
    public class RegistryHelper
    {
        /// <summary>
        /// CLSID registry key
        /// </summary>
        public static readonly string CLSID = "CLSID";

        /// <summary>
        /// InprocServer32 registry key
        /// </summary>
        public static readonly string INPROC_SERVER_32 = "InprocServer32";

        /// <summary>
        /// Return the passed guid as a string in registry format (i.e. w/ {})
        /// </summary>
        /// <param name="guid">Guid to return reg format for</param>
        /// <returns></returns>
        public static string GuidRegFmt(Guid guid)
        {
            return guid.ToString("B");
        }

        /// <summary>
        /// Helper function to recursively delete a sub-key (swallows errors in the
        /// case of the sub-key not existing
        /// </summary>
        /// <param name="root">Root to delete key from</param>
        /// <param name="subKey">Name of key to delete</param>
        public static void DeleteSubKeyTree(RegistryKey root, string subKey)
        {
            // delete the specified sub-key if if exists (swallow the error if the
            // sub-key does not exist)
            try { root.DeleteSubKeyTree(subKey); }
            catch (ArgumentException) { }
        }

        /// <summary>
        /// Returns the root RegistryKey associated with the HKey value.
        /// </summary>
        /// <param name="hkey"></param>
        /// <returns></returns>
        private static RegistryKey GetRootRegistryKey(UIntPtr hkey)
        {
            if (hkey == HKEY.CLASSES_ROOT)
                return Registry.ClassesRoot;
            else if (hkey == HKEY.CURRENT_USER)
                return Registry.CurrentUser;
            else if (hkey == HKEY.LOCAL_MACHINE)
                return Registry.LocalMachine;
            else if (hkey == HKEY.USERS)
                return Registry.Users;
            else if (hkey == HKEY.PERFORMANCE_DATA)
                return Registry.PerformanceData;
            else if (hkey == HKEY.CURRENT_CONFIG)
                return Registry.CurrentConfig;
            else if (hkey == HKEY.DYN_DATA)
            {
                throw new Exception("Unsupported HKEY value: " + hkey);
            }
            else
            {
                throw new Exception("Unknown HKEY value: " + hkey);
            }
        }

        /// <summary>
        /// Create the specified registry key.
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        public static void CreateKey(UIntPtr hkey, string key)
        {
            RegistryKey regKey = GetKey(hkey, key);
            regKey.Close();
        }

        public static bool KeyExists(UIntPtr hkey, string key)
        {
            RegistryKey currentKey = GetRootRegistryKey(hkey);
            using (RegistryKey subkey = currentKey.OpenSubKey(key, false))
                return subkey != null;
        }

        public static string GetAppUserModelID(string progId)
        {
            try
            {
                using (RegistryKey progIdKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\" + progId))
                {
                    if (progIdKey != null)
                        return progIdKey.GetValue("AppUserModelID") as string;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception thrown while getting the AppUserModelID for " + progId);
                if (!IsRegistryException(ex))
                    throw;
            }

            return null;
        }

        private static void DumpKey(RegistryKey key, int depth)
        {
            try
            {
                string indent = new string(' ', depth * 2);
                Trace.WriteLine(indent + key);

                // Enumerate values
                foreach (string valueName in key.GetValueNames())
                {
                    if (String.IsNullOrEmpty(valueName))
                    {
                        Trace.WriteLine(indent + "Default Value: " + key.GetValue(valueName));
                    }
                    else
                    {
                        Trace.WriteLine(indent + "Name: " + valueName + " --> Value: " + key.GetValue(valueName));
                    }
                }

                // Enumerate subkeys
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        DumpKey(subKey, depth + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception thrown while dumping registry key: " + key + "\r\n" + ex);
                if (IsRegistryException(ex))
                    return;

                throw;
            }

        }

        public static bool IsRegistryException(Exception ex)
        {
            return (ex is SecurityException ||
                    ex is UnauthorizedAccessException ||
                    ex is ObjectDisposedException ||
                    ex is IOException ||
                    ex is ArgumentNullException);
        }

        public static void DumpKey(RegistryKey key, string subkey)
        {
            try
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                if (subkey == null)
                    throw new ArgumentNullException("subkey");

                using (RegistryKey extensionKey = key.OpenSubKey(subkey))
                {
                    if (extensionKey == null)
                        Trace.WriteLine("Failed to open " + key + @"\" + subkey);
                    else
                        DumpKey(extensionKey);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception thrown while dumping registry key: " + key + "\r\n" + ex);
                if (IsRegistryException(ex))
                    return;

                throw;
            }
        }

        public static void DumpKey(RegistryKey key)
        {
            DumpKey(key, 0);
        }

        /// <summary>
        /// Get the specified registry key.
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        public static RegistryKey GetKey(UIntPtr hkey, string key)
        {
            ArrayList keyList = new ArrayList();
            try
            {
                RegistryKey currentKey = GetRootRegistryKey(hkey);
                keyList.Add(currentKey);
                foreach (string subKeyName in key.Split('\\'))
                {
                    currentKey = currentKey.CreateSubKey(subKeyName);
                    keyList.Add(currentKey);
                }
            }
            finally
            {
                //close all of the open registry keys
                for (int i = 0; i < keyList.Count - 1; i++)
                {
                    RegistryKey regKey = (RegistryKey)keyList[i];
                    regKey.Close();
                }
            }
            return keyList[keyList.Count - 1] as RegistryKey;
        }

        /// <summary>
        /// Returns the path of the specified registry key.
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetKeyString(UIntPtr hkey, string key)
        {
            return String.Format(CultureInfo.InvariantCulture, @"{0}\{1}", GetHKeyString(hkey), key);
        }

        private static string GetHKeyString(UIntPtr hkey)
        {
            if (hkey == HKEY.CLASSES_ROOT)
                return "HKEY_CLASSES_ROOT";
            else if (hkey == HKEY.CURRENT_USER)
                return "HKEY_CURRENT_USER";
            else if (hkey == HKEY.LOCAL_MACHINE)
                return "HKEY_LOCAL_MACHINE";
            else if (hkey == HKEY.USERS)
                return "HKEY_USERS";
            else if (hkey == HKEY.PERFORMANCE_DATA)
                return "HKEY_PERFORMANCE_DATA";
            else if (hkey == HKEY.CURRENT_CONFIG)
                return "HKEY_CURRENT_CONFIG";
            else if (hkey == HKEY.DYN_DATA)
                return "HKEY_DYN_DATA";
            else
            {
                Debug.Fail("unknown HKEY value");
                return "HKEY_UNKNOWN";
            }
        }
    }
}
