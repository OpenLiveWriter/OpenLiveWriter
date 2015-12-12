// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Wraps OpenFileDialog components to create an "Open Image" dialog that defaults
    /// to the shell Thumbnail view
    /// </summary>
    public class OpenImageDialog : IDisposable
    {
        public OpenImageDialog()
        {
            // create the open file dialog
            _openFileDialog = new OpenFileDialog();

            // set some defaults appropriate for images
            _openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            _openFileDialog.Filter = "Images Files (*.gif, *.jpg, *.jpeg, *.png)|*.gif;*.jpg;*.jpeg;*.png|All Files (*.*)|*.*" ;
            _openFileDialog.CheckFileExists = true ;
            _openFileDialog.CheckPathExists = true ;
            _openFileDialog.DereferenceLinks = true ;
            _openFileDialog.Multiselect = false ;
            _openFileDialog.ValidateNames = true ;
        }

        public void Dispose()
        {
            if ( _openFileDialog != null )
            {
                _openFileDialog.Dispose();
                _openFileDialog = null ;
            }
        }

        /// <summary>
        /// Provide passthrough for accessing properties of FileDialog
        /// </summary>
        public OpenFileDialog FileDialog
        {
            get
            {
                return _openFileDialog ;
            }
        }

        /// <summary>
        /// Show the dialog w/ appropriate hooks to change size and default view
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public DialogResult ShowDialog(IWin32Window parent)
        {
            // show the dialog (customizations happen in the DialogCreated event handler)
            using (OpenFileDialogCreationListener creationListener = new OpenFileDialogCreationListener(parent) )
            {
                // listen for creation of the dialog, when this happens we customize the
                // appearance and behavior of the dialog as desired
                creationListener.DialogCreated +=new EventHandler(creationListener_DialogCreated);

                // show the dialog and return the result
                return _openFileDialog.ShowDialog(parent) ;
            }
        }

        private void creationListener_DialogCreated(object sender, EventArgs e)
        {
            // get the dialog's handle
            IntPtr hDialog = (sender as OpenFileDialogCreationListener).DialogHandle ;

            // switch to thumbnail view
            SwitchToThumbnailView( hDialog ) ;

            // make the dialog larger so it shows 3 rows of thumbnails
            PositionDialog( hDialog ) ;
        }

        private void SwitchToThumbnailView( IntPtr hDialog )
        {
            // This hack is based on the knowledge that within the standard open file dialog
            // there is a control w/ class "SHELLDLL_DefView" which implements standard
            // shell right-pane viewing. Documentation for the existence of this window can
            // be found at: http://msdn.microsoft.com/msdnmag/issues/04/03/CQA/

            // get handle of the special control
            IntPtr hListView = User32.FindWindowEx(hDialog, IntPtr.Zero, "SHELLDLL_DefView", "");

            // This control implements a set of WM_COMMAND messages which correspond to
            // menu items on its view menu. By using Spy++ you can reverse engineer this
            // enumeration (this is also described in the article referenced above).

            // send the message
            User32.SendMessage(hListView, WM_COMMAND, FCIDM_SHVIEW.THUMBNAIL, IntPtr.Zero);

            // NOTE: there is at least one report from a developer of this technique not working
            // (see comments at http://www.thecodeproject.com/cs/miscctrl/FileDialogExtender.asp).
            // it is very likely that this technique is fragile accross OS version and/or
            // installed shell customizations.
        }

        private void PositionDialog( IntPtr hDialog )
        {
            // desired dialog dimensions
            const int DIALOG_HEIGHT = 565 ;
            const int DIALOG_WIDTH = 650 ;

            // get existing dimensions
            RECT dialogRect = new RECT();
            User32.GetWindowRect(hDialog, ref dialogRect ) ;

            // grow window size (note: will result in slightly off-center window however
            // if we try to center the window it will flash/flicker while being moved)
            User32.MoveWindow(hDialog, dialogRect.left, dialogRect.top,
                DIALOG_WIDTH,
                DIALOG_HEIGHT,
                true ) ;
        }

        private OpenFileDialog _openFileDialog = null ;

        // wm command message
        private const uint WM_COMMAND = 0x0111 ;

        // reverse-engineered command codes for SHELLDLL_DefView
        private class FCIDM_SHVIEW
        {
            public static readonly UIntPtr LARGEICON = new UIntPtr(0x7029) ;
            public static readonly UIntPtr SMALLICON = new UIntPtr(0x702A) ;
            public static readonly UIntPtr LIST = new UIntPtr(0x702B) ;
            public static readonly UIntPtr REPORT = new UIntPtr(0x702C) ;
            public static readonly UIntPtr THUMBNAIL = new UIntPtr(0x702D) ;
            public static readonly UIntPtr TILE = new UIntPtr(0x702E) ;
        }


        /// <summary>
        /// Hook to detect the creation and window handle of the dialog.
        /// </summary>
        private class OpenFileDialogCreationListener : IDisposable
        {
            public OpenFileDialogCreationListener( IWin32Window parent )
            {
                _subClasser = new WindowSubClasser(parent, new WndProcDelegate(WndProc));
                _subClasser.Install();
            }

            public IntPtr WndProc( IntPtr hWnd, uint uMsg, UIntPtr wParam, IntPtr lParam )
            {
                // detect the dialog's creation and record its window handle
                if ( uMsg == WM_ENTERIDLE && wParam == MSGF_DIALOGBOX )
                {
                    // only do this once
                    if ( _dialogHandle == IntPtr.Zero )
                    {
                        _dialogHandle = lParam ;

                        if ( DialogCreated != null )
                            DialogCreated( this, EventArgs.Empty ) ;
                    }
                }

                return _subClasser.CallBaseWindowProc(hWnd, uMsg, wParam, lParam);
            }

            private WindowSubClasser _subClasser;

            public void Dispose()
            {
                _subClasser.Remove();
            }

            public event EventHandler DialogCreated ;

            public IntPtr DialogHandle
            {
                get
                {
                    return _dialogHandle ;
                }
            }
            private IntPtr _dialogHandle = IntPtr.Zero ;

            private const int WM_ENTERIDLE = 0x0121 ;
            private static readonly UIntPtr MSGF_DIALOGBOX = UIntPtr.Zero ;
        }

    }
}
