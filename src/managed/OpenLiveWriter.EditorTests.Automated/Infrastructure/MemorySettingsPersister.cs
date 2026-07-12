// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.EditorTests.Automated.Infrastructure
{
    /// <summary>
    /// In-memory <see cref="ISettingsPersister"/> for headless preference round-trip tests.
    /// </summary>
    internal sealed class MemorySettingsPersister : ISettingsPersister
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.Ordinal);
        private readonly Dictionary<string, MemorySettingsPersister> _children =
            new Dictionary<string, MemorySettingsPersister>(StringComparer.Ordinal);

        public string[] GetNames()
        {
            var names = new string[_values.Count];
            _values.Keys.CopyTo(names, 0);
            return names;
        }

        public object Get(string name, Type desiredType, object defaultValue)
        {
            if (_values.TryGetValue(name, out object val))
            {
                if (desiredType != null && val != null && val.GetType() != desiredType)
                {
                    try { return Convert.ChangeType(val, desiredType, System.Globalization.CultureInfo.InvariantCulture); }
                    catch { return defaultValue; }
                }
                return val;
            }
            return defaultValue;
        }

        public object Get(string name) => _values.TryGetValue(name, out object v) ? v : null;

        public void Set(string name, object value)
        {
            if (value == null) { Unset(name); return; }
            _values[name] = value;
        }

        public void Unset(string name) => _values.Remove(name);

        public void UnsetSubSettingsTree(string name) => _children.Remove(name);

        public IDisposable BatchUpdate() => new BatchScope(this);

        public bool HasSubSettings(string subSettingsName) => _children.ContainsKey(subSettingsName);

        public ISettingsPersister GetSubSettings(string subSettingsName)
        {
            if (!_children.TryGetValue(subSettingsName, out MemorySettingsPersister child))
            {
                child = new MemorySettingsPersister();
                _children[subSettingsName] = child;
            }
            return child;
        }

        public string[] GetSubSettings()
        {
            var names = new string[_children.Count];
            _children.Keys.CopyTo(names, 0);
            return names;
        }

        public void Dispose() { }

        private sealed class BatchScope : IDisposable
        {
            private readonly MemorySettingsPersister _owner;
            public BatchScope(MemorySettingsPersister owner) => _owner = owner;
            public void Dispose() { }
        }
    }
}
