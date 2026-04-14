// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Api;

namespace OpenLiveWriter.Extensibility.ImageServices
{
    /// <summary>
    /// Cross-platform interface for image service settings editors.
    /// The WinForms implementation (ImageServiceSettingsEditor : UserControl) lives in Platform.Windows.
    /// </summary>
    public interface IImageServiceSettingsEditor
    {
        void LoadEditor(IProperties imageServiceSettings);
    }
}
