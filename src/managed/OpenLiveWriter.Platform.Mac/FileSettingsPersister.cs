// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.Platform.Mac
{
    /// <summary>
    /// JSON file-backed <see cref="ISettingsPersister"/> for macOS. Each root instance
    /// maps to one file under the platform-resolved Settings directory; nested
    /// <see cref="GetSubSettings"/> views share the file and navigate into child objects.
    /// </summary>
    public sealed class FileSettingsPersister : ISettingsPersister
    {
        private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly string _filePath;
        private readonly string[] _segments;
        private JsonObject _root;
        private int _batchDepth;

        private FileSettingsPersister(string filePath, string[] segments, JsonObject root)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _segments = segments ?? Array.Empty<string>();
            _root = root ?? new JsonObject();
        }

        /// <summary>Creates a root persister for <paramref name="subKey"/> under <paramref name="settingsDirectory"/>.</summary>
        public static FileSettingsPersister Create(string settingsDirectory, string subKey)
        {
            if (string.IsNullOrEmpty(settingsDirectory))
                throw new ArgumentNullException(nameof(settingsDirectory));
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException(nameof(subKey));

            Directory.CreateDirectory(settingsDirectory);
            string path = Path.Combine(settingsDirectory, SanitizeSubKey(subKey) + ".json");
            return OpenFile(path);
        }

        /// <summary>Opens (or creates) a persister rooted at the given JSON file. Used by tests with temp paths.</summary>
        public static FileSettingsPersister OpenFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            JsonObject root;
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    root = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
                }
                catch (JsonException)
                {
                    root = new JsonObject();
                }
            }
            else
            {
                root = new JsonObject();
            }

            return new FileSettingsPersister(filePath, Array.Empty<string>(), root);
        }

        /// <summary>The on-disk JSON file this tree persists to.</summary>
        public string FilePath => _filePath;

        public string[] GetNames()
        {
            JsonObject container = GetContainer(create: false);
            if (container == null)
                return Array.Empty<string>();

            return container
                .Where(p => p.Value is not JsonObject)
                .Select(p => p.Key)
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToArray();
        }

        public object Get(string name, Type desiredType, object defaultValue)
        {
            if (desiredType == null)
                throw new ArgumentNullException(nameof(desiredType));

            try
            {
                object saved = Get(name);
                if (saved != null)
                    return Coerce(saved, desiredType);
            }
            catch
            {
                // Fall through to default + persist.
            }

            if (defaultValue != null)
            {
                try { Set(name, defaultValue); }
                catch { /* best effort */ }
            }

            return defaultValue;
        }

        public object Get(string name)
        {
            if (name == null)
                return null;

            JsonObject container = GetContainer(create: false);
            if (container == null || !container.TryGetPropertyValue(name, out JsonNode node))
                return null;

            if (node is JsonObject)
                return null;

            return NodeToObject(node);
        }

        public void Set(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            JsonObject container = GetContainer(create: true);
            if (value == null)
            {
                container.Remove(name);
            }
            else
            {
                container[name] = ObjectToNode(value);
            }

            SaveIfNeeded();
        }

        public void Unset(string name)
        {
            if (name == null)
                return;

            JsonObject container = GetContainer(create: false);
            if (container == null)
                return;

            if (container.Remove(name))
                SaveIfNeeded();
        }

        public void UnsetSubSettingsTree(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            JsonObject container = GetContainer(create: false);
            if (container == null)
                return;

            if (container.Remove(name))
                SaveIfNeeded();
        }

        public IDisposable BatchUpdate()
        {
            _batchDepth++;
            return new BatchScope(this);
        }

        public bool HasSubSettings(string subSettingsName)
        {
            JsonObject container = GetContainer(create: false);
            return container != null
                && container.TryGetPropertyValue(subSettingsName, out JsonNode node)
                && node is JsonObject;
        }

        public ISettingsPersister GetSubSettings(string subSettingsName)
        {
            if (string.IsNullOrEmpty(subSettingsName))
                throw new ArgumentNullException(nameof(subSettingsName));

            var childSegments = new string[_segments.Length + 1];
            Array.Copy(_segments, childSegments, _segments.Length);
            childSegments[_segments.Length] = subSettingsName;

            JsonObject container = GetContainer(create: true);
            if (!container.TryGetPropertyValue(subSettingsName, out JsonNode node) || node is not JsonObject child)
            {
                child = new JsonObject();
                container[subSettingsName] = child;
                SaveIfNeeded();
            }

            return new FileSettingsPersister(_filePath, childSegments, _root);
        }

        public string[] GetSubSettings()
        {
            JsonObject container = GetContainer(create: false);
            if (container == null)
                return Array.Empty<string>();

            return container
                .Where(p => p.Value is JsonObject)
                .Select(p => p.Key)
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToArray();
        }

        public void Dispose()
        {
            Flush();
        }

        private JsonObject GetContainer(bool create)
        {
            JsonObject current = _root;
            foreach (string segment in _segments)
            {
                if (!current.TryGetPropertyValue(segment, out JsonNode node) || node is not JsonObject child)
                {
                    if (!create)
                        return null;

                    child = new JsonObject();
                    current[segment] = child;
                }

                current = child;
            }

            return current;
        }

        private void SaveIfNeeded()
        {
            if (_batchDepth == 0)
                Flush();
        }

        private void Flush()
        {
            string dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            string json = _root.ToJsonString(WriteOptions);
            string temp = _filePath + ".tmp";
            File.WriteAllText(temp, json);
            if (File.Exists(_filePath))
                File.Replace(temp, _filePath, null);
            else
                File.Move(temp, _filePath);
        }

        private static string SanitizeSubKey(string subKey)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                subKey = subKey.Replace(c, '_');
            return subKey;
        }

        private static JsonNode ObjectToNode(object value)
        {
            switch (value)
            {
                case bool b:
                    return JsonValue.Create(b);
                case int i:
                    return JsonValue.Create(i);
                case long l:
                    return JsonValue.Create(l);
                case float f:
                    return JsonValue.Create(f);
                case double d:
                    return JsonValue.Create(d);
                case string s:
                    return JsonValue.Create(s);
                case string[] arr:
                    var array = new JsonArray();
                    foreach (string item in arr)
                        array.Add(item);
                    return array;
                case Enum e:
                    return JsonValue.Create(e.ToString());
                default:
                    return JsonValue.Create(Convert.ToString(value, CultureInfo.InvariantCulture));
            }
        }

        private static object NodeToObject(JsonNode node)
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<bool>(out bool b)) return b;
                if (value.TryGetValue<int>(out int i)) return i;
                if (value.TryGetValue<long>(out long l)) return l;
                if (value.TryGetValue<double>(out double d)) return d;
                if (value.TryGetValue<string>(out string s)) return s;
            }

            if (node is JsonArray array)
            {
                var list = new List<string>();
                foreach (JsonNode item in array)
                {
                    if (item is JsonValue v && v.TryGetValue<string>(out string s))
                        list.Add(s);
                }
                return list.ToArray();
            }

            return node?.ToJsonString();
        }

        private static object Coerce(object value, Type desiredType)
        {
            if (value == null)
                return null;

            Type target = Nullable.GetUnderlyingType(desiredType) ?? desiredType;

            if (target.IsInstanceOfType(value))
                return value;

            if (target == typeof(string))
                return Convert.ToString(value, CultureInfo.InvariantCulture);

            if (target == typeof(bool))
            {
                if (value is string s)
                    return bool.Parse(s);
                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }

            if (target == typeof(int))
            {
                if (value is long l)
                    return (int)l;
                if (value is string s)
                    return int.Parse(s, CultureInfo.InvariantCulture);
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }

            if (target == typeof(string[]) && value is string[] arr)
                return arr;

            if (target.IsEnum)
            {
                if (value is string es)
                    return Enum.Parse(target, es, ignoreCase: true);
                return Enum.ToObject(target, value);
            }

            return Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
        }

        private sealed class BatchScope : IDisposable
        {
            private readonly FileSettingsPersister _owner;
            private bool _disposed;

            public BatchScope(FileSettingsPersister owner) => _owner = owner;

            public void Dispose()
            {
                if (_disposed)
                    return;
                _disposed = true;
                _owner._batchDepth--;
                if (_owner._batchDepth == 0)
                    _owner.Flush();
            }
        }
    }
}
