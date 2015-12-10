// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class ImageEditingElementBehavior : HtmlEditorElementBehavior
    {
        private bool _disposed;

        public ImageEditingElementBehavior(IHtmlEditorComponentContext editorContext)
            : base(editorContext)
        {
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();

            EditorContext.CommandKey += new KeyEventHandler(EditorContext_CommandKey);
            EditorContext.DocumentEvents.DoubleClick += new HtmlEventHandler(EditorContext_DoubleClick);
        }

        protected override void OnSelectedChanged()
        {
        }

        public override void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo)
        {
            // ensure we paint above everything (including selection handles)
            pInfo.lFlags = (int)_HTML_PAINTER.HTMLPAINTER_OPAQUE;
            pInfo.lZOrder = (int)_HTML_PAINT_ZORDER.HTMLPAINT_ZORDER_WINDOW_TOP;

            // expand to the right to accomodate our widget
            pInfo.rcExpand.top = 0;
            pInfo.rcExpand.bottom = 0;
            pInfo.rcExpand.left = 0;
            pInfo.rcExpand.right = 0;
        }

        private void EditorContext_CommandKey(object sender, KeyEventArgs e)
        {
            if (Selected && (e.KeyCode == Keys.Back))
            {
                EditorContext.Clear();
                e.Handled = true;
            }
        }

        private void EditorContext_DoubleClick(object sender, HtmlEventArgs e)
        {
            if (Selected)
                EditorContext.CommandManager.Execute(CommandId.ActivateContextualTab);
        }

        protected override bool QueryElementSelected()
        {
            IHTMLElement selectedImage = EditorContext.Selection.SelectedImage as IHTMLElement;
            if (selectedImage != null)
            {
                return selectedImage.sourceIndex == HTMLElement.sourceIndex;
            }
            else
            {
                return false;
            }
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    Debug.Assert(EditorContext != null);
                    EditorContext.CommandKey -= new KeyEventHandler(EditorContext_CommandKey);
                    EditorContext.DocumentEvents.DoubleClick -= new HtmlEventHandler(EditorContext_DoubleClick);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
    }
}
