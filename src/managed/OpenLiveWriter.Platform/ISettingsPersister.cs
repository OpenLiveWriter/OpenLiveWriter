// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Defines an interface for persistent settings.
    /// </summary>
    public interface ISettingsPersister : IDisposable
    {
        string[] GetNames();
        object Get(string name, Type desiredType, object defaultValue);
        object Get(string name);
        void Set(string name, object value);
        void Unset(string name);
        void UnsetSubSettingsTree(string name);
        IDisposable BatchUpdate();
        bool HasSubSettings(string subSettingsName);
        ISettingsPersister GetSubSettings(string subSettingsName);
        string[] GetSubSettings();
    }
}
