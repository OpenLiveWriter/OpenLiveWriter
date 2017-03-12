// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// An ISettingsPersister-based ISettings implementation
    /// </summary>
    internal class PluginSettingsAdaptor : IProperties
    {
        public PluginSettingsAdaptor(SettingsPersisterHelper settingsHelper)
        {
            SettingsHelper = settingsHelper;
        }

        public string this[string key]
        {
            get
            {
                return GetString(key, null);
            }
            set
            {
                SetString(key, value);
            }
        }

        public int GetInt(string key, int defaultValue)
        {
            string val = GetString(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            return Int32.Parse(val, CultureInfo.InvariantCulture);
        }
        public void SetInt(string key, int value)
        {
            SetString(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            string val = GetString(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            return bool.Parse(val);
        }
        public void SetBoolean(string key, bool value)
        {
            SetString(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public float GetFloat(string key, float defaultValue)
        {
            string val = GetString(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            return float.Parse(val, CultureInfo.InvariantCulture);
        }
        public void SetFloat(string key, float value)
        {
            SetString(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public decimal GetDecimal(string key, decimal defaultValue)
        {
            string val = GetString(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            return decimal.Parse(val, CultureInfo.InvariantCulture);
        }
        public void SetDecimal(string key, decimal value)
        {
            SetString(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public string GetString(string key, string defaultValue)
        {
            return SettingsHelper.GetString(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            SettingsHelper.SetString(key, value);
        }

        public void Remove(string key)
        {
            SettingsHelper.Unset(key);
        }

        public void RemoveAll()
        {
            foreach (string propertyName in Names)
                Remove(propertyName);

            foreach (string subPropertyName in SubPropertyNames)
                RemoveSubProperties(subPropertyName);
        }

        public string[] Names
        {
            get { return SettingsHelper.GetNames(); }
        }

        public bool Contains(string key)
        {
            return Array.IndexOf(SettingsHelper.GetNames(), key) > -1;
        }

        public string[] SubPropertyNames
        {
            get
            {
                return SettingsHelper.GetSubSettingNames();
            }
        }

        public IProperties GetSubProperties(string key)
        {
            return new PluginSettingsAdaptor(SettingsHelper.GetSubSettings(key));
        }

        public void RemoveSubProperties(string key)
        {
            if (ContainsSubProperties(key))
                SettingsHelper.UnsetSubsettingTree(key);
        }

        public bool ContainsSubProperties(string key)
        {
            return SettingsHelper.SettingsPersister.HasSubSettings(key);
        }

        protected SettingsPersisterHelper SettingsHelper;
    }
}
