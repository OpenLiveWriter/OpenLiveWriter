// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor
{

    public class BlogPostSettingsBag : IProperties, ICloneable
    {
        Hashtable settings = new Hashtable();
        Hashtable subsettings = new Hashtable();
        ArrayList settingsOrderedKeyList = new ArrayList(); //maintains settings keys in insertion order
        ArrayList subsettingsOrderedKeyList = new ArrayList(); //maintains subsettings keys in insertion order

        public BlogPostSettingsBag()
        {

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

        public BlogPostSettingsBag GetSubSettings(string key)
        {
            BlogPostSettingsBag val = (BlogPostSettingsBag)subsettings[key];
            return val;
        }

        public BlogPostSettingsBag CreateSubSettings(string key)
        {
            BlogPostSettingsBag val = GetSubSettings(key);
            if (val == null)
            {
                val = new BlogPostSettingsBag();
                subsettings[key] = val;
                subsettingsOrderedKeyList.Add(key);
            }
            return val;
        }

        public void RemoveSubSettings(string key)
        {
            subsettings.Remove(key);
            subsettingsOrderedKeyList.Remove(key);
        }

        public void AddSettings(IProperties settings)
        {
            foreach (string key in settings.Names)
            {
                SetString(key, settings[key]);
            }
        }

        public string GetString(string key, string defaultValue)
        {
            string val = (string)settings[key];
            if (val == null)
                val = defaultValue;
            return val;
        }
        public void SetString(string key, string value)
        {
            if (!settings.ContainsKey(key))
                settingsOrderedKeyList.Add(key);
            settings[key] = value;
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

        public void Remove(string key)
        {
            settings.Remove(key);
            settingsOrderedKeyList.Remove(key);
        }

        public void RemoveAll()
        {
            foreach (string propertyName in Names)
                Remove(propertyName);

            foreach (string subPropertyName in SubPropertyNames)
                RemoveSubProperties(subPropertyName);
        }

        public string[] Names { get { return settingsOrderedKeyList.ToArray(typeof(string)) as string[]; } }
        public bool Contains(string key) { return settings.ContainsKey(key); }
        public void Clear() { settings.Clear(); subsettings.Clear(); }

        public IEnumerable SubsettingNames { get { return subsettingsOrderedKeyList; } }
        public bool ContainsSubsetting(string key) { return subsettings.ContainsKey(key); }
        public int Count { get { return settings.Count; } }
        public int SubSettingsCount { get { return subsettings.Count; } }

        public string[] SubPropertyNames
        {
            get
            {
                return (string[])subsettingsOrderedKeyList.ToArray(typeof(string));
            }
        }

        public IProperties GetSubProperties(string key)
        {
            IProperties props = GetSubSettings(key);
            if (props == null)
                props = CreateSubSettings(key);
            return props;
        }

        public void RemoveSubProperties(string key)
        {
            RemoveSubSettings(key);
        }

        public bool ContainsSubProperties(string key)
        {
            return subsettings.ContainsKey(key);
        }

        #region ICloneable Members

        public object Clone()
        {
            BlogPostSettingsBag clone = new BlogPostSettingsBag();
            foreach (string key in Names)
            {
                string value = this[key];
                clone[key] = value;
            }

            foreach (string name in SubsettingNames)
            {
                BlogPostSettingsBag sub = GetSubSettings(name);
                clone.subsettings[name] = (BlogPostSettingsBag)sub.Clone();
                clone.settingsOrderedKeyList = (ArrayList)settingsOrderedKeyList.Clone();
                clone.subsettingsOrderedKeyList = (ArrayList)subsettingsOrderedKeyList.Clone();
            }
            return clone;
        }

        #endregion
    }
}
