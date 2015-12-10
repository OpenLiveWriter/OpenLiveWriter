// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Api;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    /// <summary>
    /// Summary description for IDefaultSettingsCustomizer.
    /// </summary>
    public interface IImageDecoratorDefaultSettingsCustomizer
    {
        /// <summary>
        /// Hook for saving the default settings for a decorator based on the current editor settings.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="defaultSettings">An empty settings object that should be populated with the default settings</param>
        void CustomizeDefaultSettingsBeforeSave(ImageDecoratorEditorContext context, IProperties defaultSettings);
    }
}
