// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace OpenLiveWriter.CoreServices.Settings
{
    public class RegistrySettingsPersister : ISettingsPersister
    {
        private RegistryKey rootNode;
        private string keyName;

        /// <param name="rootNode">One of the root-level nodes (HKLM, HKCU, etc.)</param>
        /// <param name="keyName">A relative path from the rootNode to the key for this instance</param>
        public RegistrySettingsPersister(RegistryKey rootNode, string keyName)
        {
            // set members
            this.rootNode = rootNode;
            this.keyName = keyName;
        }

        #region ISettingsPersister Members

        public string[] GetNames()
        {
            using (RegistryKey key = GetKey(false))
            {
                if (key == null)
                    return null;
                else
                    return key.GetValueNames();
            }
        }

        public object Get(string name, Type desiredType, object defaultValue)
        {
            object savedValue = null;
            try
            {
                // first try to get it from storage
                savedValue = Get(name, desiredType);
            }
            catch (Exception e)
            {
                // Since this could get called frequently, try to only log once
                if (!haveLoggedFailedRead)
                {
                    //Debug.Fail("The setting " + name + " could not be retrieved: " + e.ToString());
                    Trace.WriteLine("The setting " + name + " could not be retrieved: " + e.ToString());
                    haveLoggedFailedRead = true;
                }
            }

            if (savedValue != null)
            {
                // value was successfully retrieved
                return savedValue;
            }
            else
            {
                // we want to use the default value and make sure it's persisted
                if (defaultValue != null)
                {
                    try
                    {
                        Set(name, defaultValue);
                    }
                    catch
                    {
                        // Since this could get called frequently, try to only log once
                        if (!haveLoggedFailedDefault)
                        {
                            //Debug.Fail("Wasn't able to persist a default value for " + name);
                            Trace.WriteLine("Wasn't able to persist a default value for " + name);
                            haveLoggedFailedDefault = true;
                        }
                    }
                }

                // all code paths that do not result in successful retrieval should end up here
                return defaultValue;
            }
        }
        private bool haveLoggedFailedRead = false;
        private bool haveLoggedFailedDefault = false;
        private bool haveLoggedFailedGetKey = false;

        /// <summary>
        /// Low-level get (returns null if the value doesn't exist). 
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>value (null if not present)</returns>
        public object Get(string name)
        {
            using (RegistryKey key = GetKey(false))
            {
                object obj = key.GetValue(name);

                // HACK: Roundtripping of serializable objects is broken if this isn't included.
                // For example, this code would assert:
                // byte[] inBytes = new byte[] {1,2,3,4,5};
                // Set("foo", inBytes);
                // Set("foo", Get("foo"));
                // byte[] outBytes = Get("foo", typeof(byte[]));
                // Trace.Assert(ArrayHelper.Compare(inBytes, outBytes));
                if (obj is byte[])
                    obj = RegistryCodec.SerializableCodec.Deserialize((byte[])obj);

                return obj;
            }
        }

        /// <summary>
        /// This used to be public, for symmetry with <c>Set</c>; but it's a bad
        /// idea to use this method because there is no fallback position if an exception
        /// occurs when retrieving from the persisted state.  The overloaded version
        /// of this that takes a default value is much safer.
        /// </summary>
        protected object Get(string name, Type desiredType)
        {
            using (RegistryKey key = GetKey(false))
            {
                if (key == null)
                    return null;

                object registryData = key.GetValue(name);
                if (registryData != null)
                    return RegistryCodec.Instance.Decode(registryData, desiredType);
                else
                    return null;
            }
        }

        public void Set(string name, object val)
        {
            //	If the value is null, remove the key.  Otherwise, set it.
            if (val == null)
                Unset(name);
            else
            {
                using (RegistryKey key = GetKey(true))
                {
                    if (key != null)
                    {
                        object registryData = RegistryCodec.Instance.Encode(val);
                        key.SetValue(name, registryData);
                    }
                }
            }
        }

        public void Unset(string name)
        {
            using (RegistryKey key = GetKey(true))
            {
                if (key != null)
                    key.DeleteValue(name, false);
            }
        }

        public void UnsetSubSettingsTree(string name)
        {
            using (RegistryKey key = GetKey(true))
            {
                if (key != null)
                    key.DeleteSubKeyTree(name);
            }
        }

        public IDisposable BatchUpdate()
        {
            return null;
        }

        /// <summary>
        /// Determine whether the specified sub-settings exists
        /// </summary>
        /// <param name="subSettingsName">name of sub-settings to check for</param>
        /// <returns>true if it has them, otherwise false</returns>
        public bool HasSubSettings(string subSettingsName)
        {
            using (RegistryKey key = GetKey(false))
            {
                if (key == null)
                {
                    return false;
                }
                using (RegistryKey subSettingsKey = key.OpenSubKey(subSettingsName))
                {
                    return subSettingsKey != null;
                }
            }
        }

        /// <summary>
        /// Get a subsetting object that is rooted in the current ISettingsPersister
        /// </summary>
        /// <param name="subKeyName">name of subkey</param>
        /// <returns>settings persister</returns>
        public ISettingsPersister GetSubSettings(string subSettingsName)
        {
            return new RegistrySettingsPersister(this.rootNode, keyName + "\\" + subSettingsName);
        }

        /// <summary>
        /// Enumerate the available sub-settings
        /// </summary>
        /// <returns></returns>
        public string[] GetSubSettings()
        {
            using (RegistryKey key = GetKey(false))
            {
                if (key == null)
                    return null;
                else
                    return key.GetSubKeyNames();
            }
        }

        /// <summary>
        /// Dispose the settings persister
        /// </summary>
        public void Dispose()
        {
            rootNode.Close();
        }

        #endregion

        /// <summary>
        /// Gets the base key associated with this instance.
        /// </summary>
        protected virtual RegistryKey GetKey(bool writable)
        {
            try
            {
                if (writable)
                {
                    return rootNode.CreateSubKey(keyName);
                }
                else
                {
                    // CreateSubKey returns the subkey in read-write mode.  We want to
                    // do the creation but return in read-only mode

                    RegistryKey key = rootNode.OpenSubKey(keyName, false);
                    if (key != null)
                    {
                        // key already exists
                        return key;
                    }
                    else
                    {
                        // key does not exist; create it, close it, open it
                        rootNode.CreateSubKey(keyName).Close();
                        return rootNode.OpenSubKey(keyName, false);
                    }
                }
            }
            catch (Exception)
            {
                // Since this could get called frequently, try to only log once
                if (!haveLoggedFailedGetKey)
                {
                    if (keyName != null && keyName.IndexOf("XDefMan", StringComparison.OrdinalIgnoreCase) >= 0)
                        Trace.WriteLine("Wasn't able to get key for " + keyName);
                    haveLoggedFailedGetKey = true;
                }

                return null;
            }
        }
    }
}
