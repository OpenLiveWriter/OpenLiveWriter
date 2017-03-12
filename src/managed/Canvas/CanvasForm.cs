// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

using OpenLiveWriter.SpellChecker;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.Interop.Com;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Test
{
    public partial class CanvasForm : Form
    {
        private readonly ContentEditorFactory factory;
        private readonly IContentEditor editor;

        public CanvasForm()
        {
            InitializeComponent();

            factory = new ContentEditorFactory();
            factory.Initialize(@"SOFTWARE\Microsoft\Windows Live\Test Canvas", new CanvasLogger(), new CanvasTarget(), new CanvasSettingsProvider());
            editor = factory.CreateEditor(new CanvasSite(panelCanvas), null, "<html><head></head><body><div>{post-body}</div></body></html>", MshtmlOptions.DEFAULT_DLCTL);
            editor.DisableSpelling();

            panelCanvas.SizeChanged += new EventHandler(panelCanvas_SizeChanged);
        }

        void panelCanvas_SizeChanged(object sender, EventArgs e)
        {
            editor.SetSize(panelCanvas.Size.Width, panelCanvas.Size.Height);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            editor.Dispose();
            factory.Shutdown();
        }

        public class CanvasLogger : IContentEditorLogger
        {

            #region IContentEditorLogger Members

            public void WriteLine(string message, int level)
            {
                Console.WriteLine(message);
            }

            #endregion
        }

        public class CanvasTarget : IContentTarget
        {
            #region IContentTarget Members

            public string ProductName
            {
                get { return "CanvasForm"; }
            }

            public bool SupportsFeature(ContentEditorFeature featureName)
            {
                switch (featureName)
                {
                    case ContentEditorFeature.ImageClickThroughs:
                        return false;
                    default:
                        return true;
                }
            }

            #endregion
        }

        public class CanvasSettingsProvider : ISettingsProvider
        {
            #region ISettingsProvider Members

            public ManagedPropVariant GetSetting(ContentEditorSetting setting)
            {
                return new ManagedPropVariant();
            }

            #endregion
        }

        public class CanvasSite : IContentEditorSite
        {
            private Panel _panel;
            public CanvasSite(Panel panel)
            {
                _panel = panel;
            }

            #region IContentEditorSite Members

            public IntPtr GetWindowHandle()
            {
                return _panel.Handle;
            }

            public void OnDocumentComplete()
            {

            }

            public void SetStatusBarMessage(string message, string wordCountValue)
            {

            }

            public void WriteEvent(string eventName)
            {

            }

            public void OnKeyboardLanguageChanged()
            {
            }

            public string GetDocumentTitle()
            {
                return null;
            }

            public void OnGotFocus()
            {
            }

            public void OnLostFocus()
            {
            }

            public void OnIsPhotoMailChanged(bool fNewIsPhotoMailValue)
            {
            }

            public IWordRange GetSubjectSpellcheckWordRange()
            {
                return null;
            }

            public void CloseSubjectSpellcheckWordRange()
            {
            }

            #endregion

            #region IUIFramework Members

            public int Destroy()
            {
                return 0;
            }

            public int FlushPendingInvalidations()
            {
                return 0;
            }

            public int GetUICommandProperty(uint commandId, ref PropertyKey key, out PropVariant value)
            {
                value = PropVariant.Empty;
                return 0;
            }

            public int GetView(uint viewId, ref Guid riid, out object ppv)
            {
                ppv = IntPtr.Zero;
                return 0;
            }

            public int Initialize(IntPtr frameWnd, OpenLiveWriter.Interop.Com.Ribbon.IUIApplication application)
            {
                return 0;
            }

            public int InvalidateUICommand(uint commandId, OpenLiveWriter.Interop.Com.Ribbon.CommandInvalidationFlags flags, IntPtr keyPtr)
            {
                return 0;
            }

            public int LoadUI(IntPtr instance, string resourceName)
            {
                return 0;
            }

            public int SetModes(int iModes)
            {
                return 0;
            }

            public int SetUICommandProperty(uint commandId, ref PropertyKey key, ref PropVariant value)
            {
                return 0;
            }

            #endregion

            #region IDropTarget Members

            public void DragEnter(OpenLiveWriter.Interop.Com.IOleDataObject pDataObj, OpenLiveWriter.Interop.Windows.MK grfKeyState, OpenLiveWriter.Interop.Windows.POINT pt, ref OpenLiveWriter.Interop.Com.DROPEFFECT pdwEffect)
            {
                pdwEffect = DROPEFFECT.NONE;
            }

            public void DragLeave()
            {

            }

            public void DragOver(OpenLiveWriter.Interop.Windows.MK grfKeyState, OpenLiveWriter.Interop.Windows.POINT pt, ref OpenLiveWriter.Interop.Com.DROPEFFECT pdwEffect)
            {
                pdwEffect = DROPEFFECT.NONE;
            }

            public void Drop(OpenLiveWriter.Interop.Com.IOleDataObject pDataObj, OpenLiveWriter.Interop.Windows.MK grfKeyState, OpenLiveWriter.Interop.Windows.POINT pt, ref OpenLiveWriter.Interop.Com.DROPEFFECT pdwEffect)
            {
                pdwEffect = DROPEFFECT.NONE;
            }

            #endregion
        }

        private void buttonSource_Click(object sender, EventArgs e)
        {
            editor.ChangeView(EditingMode.Source);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            editor.ChangeView(EditingMode.Wysiwyg);
        }

        private void buttonPreview_Click(object sender, EventArgs e)
        {
            editor.ChangeView(EditingMode.Preview);
        }

        private void buttonPlain_Click(object sender, EventArgs e)
        {
            editor.ChangeView(EditingMode.PlainText);
        }

    [DllImport("KERNEL32.DLL", CharSet=CharSet.Auto, EntryPoint="LoadLibrary")]
    public static extern IntPtr LoadLibrary(String lpFileName);
    }
}
