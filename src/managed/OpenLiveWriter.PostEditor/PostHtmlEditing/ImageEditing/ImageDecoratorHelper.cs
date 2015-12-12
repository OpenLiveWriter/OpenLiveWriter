// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageDecoratorHelper.
    /// </summary>
    internal class ImageDecoratorHelper
    {
        private ImageDecoratorHelper()
        {
            //no instances
        }

        /// <summary>
        /// Displays the editor (if one exists) for an image decorator in a dialog.
        /// </summary>
        /// <param name="imageDecorator"></param>
        /// <param name="ImageInfo"></param>
        /// <param name="applyCallback"></param>
        /// <param name="owner"></param>
        internal static DialogResult ShowImageDecoratorEditorDialog(ImageDecorator imageDecorator, ImagePropertiesInfo ImageInfo, ApplyDecoratorCallback applyCallback, IUndoUnitFactory undoUnitFactory, object state, IImageTargetEditor targetEditor, CommandManager commandManager)
        {
            ImageDecoratorEditor editor = imageDecorator.CreateEditor(commandManager);
            if (editor != null)
            {
                IProperties settings = ImageInfo.ImageDecorators.GetImageDecoratorSettings(imageDecorator);
                editor.LoadEditor(new ImageDecoratorEditorContextImpl(settings, new ApplyDecoratorCallback(applyCallback), ImageInfo, undoUnitFactory, commandManager), state, targetEditor);
                using (ImageDecoratorEditorForm editorForm = new ImageDecoratorEditorForm())
                {
                    editorForm.ImageDecoratorEditor = editor;

                    // for automation
                    editorForm.Name = imageDecorator.Id + "EditorForm";
                    return editorForm.ShowDialog();
                }
            }
            return DialogResult.Abort;
        }
    }
}
