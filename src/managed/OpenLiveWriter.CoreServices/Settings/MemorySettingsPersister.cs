// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.CoreServices.Settings
{
    /// <summary>
    /// Keeps settings in memory (i.e. doesn't actually persist them)
    /// </summary>
    public class MemorySettingsPersister : ISettingsPersister
    {
        private object defaultVal;
        private Hashtable data;
        private DefaultHashtable children;

        public MemorySettingsPersister()
        {
            defaultVal = null;
            data = new Hashtable();
            children = new DefaultHashtable(new DefaultHashtable.DefaultValuePump(MemorySettingsPersisterPump));
        }

        private object MemorySettingsPersisterPump(object key)
        {
            return new MemorySettingsPersister();
        }

        public string[] GetNames()
        {
            return CollectionToSortedStringArray(data.Keys);
        }

        public object Get(string name, Type desiredType, object defaultValue)
        {
            if (name == null)
            {
                if (this.defaultVal == null)
                    this.defaultVal = defaultValue;
                return this.defaultVal;
            }

            object val = data[name];
            if (val != null && desiredType.IsAssignableFrom(val.GetType()))
                return val;
            data[name] = defaultValue;
            return defaultValue;
        }

        public object Get(string name)
        {
            if (name == null)
                return this.defaultVal;

            return data[name];
        }

        public void Set(string name, object value)
        {
            if (name == null)
                defaultVal = value;
            else
                data[name] = value;
        }

        public void Unset(string name)
        {
            if (name == null)
                defaultVal = null;

            data.Remove(name);
        }

        public void UnsetSubSettingsTree(string name)
        {
            children.Remove(name);
        }

        public IDisposable BatchUpdate()
        {
            return null;
        }

        public bool HasSubSettings(string subSettingsName)
        {
            return children.ContainsKey(subSettingsName);
        }

        public ISettingsPersister GetSubSettings(string subSettingsName)
        {
            return (ISettingsPersister)children[subSettingsName];
        }

        public string[] GetSubSettings()
        {
            return CollectionToSortedStringArray(children.Keys);
        }

        public void Dispose()
        {
        }

        private string[] CollectionToSortedStringArray(ICollection foo)
        {
            string[] names = new string[data.Count];
            int i = 0;
            foreach (string key in foo)
            {
                names[i++] = key;
            }
            Array.Sort(names);
            return (string[])ArrayHelper.Compact(names);
        }
    }
}
