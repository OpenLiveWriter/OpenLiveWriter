// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Extensibility.ImageServices
{
    /// <summary>
    /// Cross-platform interface for image file upload settings editors.
    /// The WinForms implementation (ImageFileUploadSettingsEditor : UserControl) lives in Platform.Windows.
    /// </summary>
    public interface IImageFileUploadSettingsEditor
    {
        void LoadEditor(IImageUploadSettingsEditorContext context);
    }
}
