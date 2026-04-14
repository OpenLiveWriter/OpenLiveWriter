// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    /// <summary>
    /// Cross-platform interface for image decorator editors.
    /// The WinForms implementation (ImageDecoratorEditor : UserControl) lives in Platform.Windows.
    /// </summary>
    public interface IImageDecoratorEditor
    {
        void LoadEditor(ImageDecoratorEditorContext context, object state, IImageTargetEditor imageTargetEditor);
        Size GetPreferredSize();
    }
}
