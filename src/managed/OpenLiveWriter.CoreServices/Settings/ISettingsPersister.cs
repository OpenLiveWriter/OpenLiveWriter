// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices.Settings
{
    /// <summary>
    /// Defines an interface for persistent settings.
    /// </summary>
    public interface ISettingsPersister : IDisposable
    {
        /// <summary>
        /// Get the list of existing settings.
        /// </summary>
        string[] GetNames();

        /// <summary>
        /// Get a persisted value.  The defaultValue will be used if the value does
        /// not exist or if any type of exception occurs.
        /// </summary>
        /// <param name="name">The name of the setting value to get.</param>
        /// <param name="desiredType">The desired type to return.</param>
        /// <param name="defaultValue">The default value to return if the value does not exist or an exception occurs.</param>
        /// <returns>The value of the setting, or the defaul value.</returns>
        object Get(string name, Type desiredType, object defaultValue);

        /// <summary>
        /// Low-level get (returns null if the value doesn't exist).
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>value (null if not present)</returns>
        object Get(string name);

        /// <summary>
        /// Persists a value.  The type of the object being persisted should be
        /// <b>precisely</b> the same as what will be passed into the <c>Get</c>
        /// method as <c>desiredType</c>, otherwise bad things may happen.
        /// </summary>
        /// <param name="name">The name of the setting value to set.</param>
        /// <param name="value">The value to set.</param>
        void Set(string name, object value);

        /// <summary>
        /// Deletes a value.  Does not delete subsettings.
        /// </summary>
        /// <param name="name"></param>
        void Unset(string name);

        /// <summary>
        /// Deletes a subsettings tree.
        /// </summary>
        /// <param name="name"></param>
        void UnsetSubSettingsTree(string name);

        /// <summary>
        /// Provides a hint to the settings persister that many
        /// changes are happening, and that the persister may
        /// want to batch up the changes for performance.
        ///
        /// This makes no guarantees about atomicity, nor whether
        /// the changes will actually be batched--it's up to the
        /// ISettingsPersister implementation to decide the best
        /// behavior.
        /// </summary>
        /// <example>
        /// using (settingsPersister.BatchUpdate())
        /// {
        ///     // do lots of updates here
        /// }
        /// </example>
        /// <returns>An IDisposable that should be disposed when
        /// the batch operation completes, OR NULL.</returns>
        IDisposable BatchUpdate();

        /// <summary>
        /// Determine whether the specified sub-settings exists
        /// </summary>
        /// <param name="subSettingsName">name of sub-settings to check for</param>
        /// <returns>true if it has them, otherwise false</returns>
        bool HasSubSettings(string subSettingsName);

        /// <summary>
        /// Get a subsetting object that is rooted in the current ISettingsPersister
        /// </summary>
        /// <param name="subKeyName">name of subkey</param>
        /// <returns>settings persister</returns>
        ISettingsPersister GetSubSettings(string subSettingsName);

        /// <summary>
        /// Enumerate the available sub-settings
        /// </summary>
        /// <returns></returns>
        string[] GetSubSettings();
    }

}
