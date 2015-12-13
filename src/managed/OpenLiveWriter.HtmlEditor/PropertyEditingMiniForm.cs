// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Project31.CoreServices;
using Project31.Controls;
using Project31.Interop.Windows;
using Project31.ApplicationFramework;

namespace Onfolio.Core.HtmlEditor
{

    public class PropertyEditingMiniForm : MiniForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private BitmapButton _closeButton ;

        private Bitmap _closeButtonEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.CloseEnabled.png") ;
        private Bitmap _closeButtonSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.CloseSelected.png") ;
        private Bitmap _closeButtonPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.ClosePushed.png") ;
        private Bitmap _closeButtonDisabled = ResourceHelper.LoadAssemblyResourceBitmap(Images.CloseInactive.png") ;

        public PropertyEditingMiniForm()
            : this(new DesignModeMainFrameWindow())
        {
        }

        public PropertyEditingMiniForm(IMainFrameWindow mainFrameWindow)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // hide when user clicks away
            DismissOnDeactivate = true ;

            // initialize close button
            _closeButton = new BitmapButton() ;
            _closeButton.BitmapDisabled = _closeButtonDisabled ;
            _closeButton.BitmapEnabled = _closeButtonEnabled ;
            _closeButton.BitmapPushed = _closeButtonPushed ;
            _closeButton.BitmapSelected = _closeButtonSelected ;
            _closeButton.ButtonStyle = ButtonStyle.Bitmap ;
            _closeButton.ToolTip = "Close" ;
            _closeButton.Width = _closeButtonEnabled.Width ;
            _closeButton.Height = _closeButtonEnabled.Height;
            _closeButton.Top = 2 ;
            _closeButton.Left = Width - _closeButton.Width - 1 ;
            _closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right ;
            _closeButton.Click +=new EventHandler(_closeButton_Click);
            Controls.Add(_closeButton) ;

            // subscribe to appearance changed
            ApplicationManager.ApplicationStyleChanged +=new EventHandler(ApplicationManager_ApplicationStyleChanged);

            // update appearance
            SyncAppearanceToApplicationStyle() ;
        }

        private void _closeButton_Click( object sender, EventArgs ea )
        {
            Close() ;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);

            // draw the background
            e.Graphics.FillRectangle(_backgroundBrush, ClientRectangle);

            // calculate title metrics
            const int TEXT_LEFT_OFFSET = 2 ;
            const int TEXT_TOP_OFFSET = 2 ;
            float fontHeight = ApplicationManager.ApplicationStyle.NormalApplicationFont.GetHeight(e.Graphics) ;
            int titleHeight = Convert.ToInt32(fontHeight) + TEXT_TOP_OFFSET + 1;

            // draw the title area
            e.Graphics.FillRectangle( _titleBarBrush, new Rectangle( 1, 1, Width-2, titleHeight ) );

            // draw the border
            e.Graphics.DrawRectangle( _borderPen, new Rectangle(0,0, Width-1, Height-1) ) ;
            e.Graphics.DrawLine( _borderPen, 1, titleHeight + 1, Width-2, titleHeight + 1  );

            // draw the text
            e.Graphics.DrawString( Text, ApplicationManager.ApplicationStyle.NormalApplicationFont, _textBrush, new PointF(TEXT_LEFT_OFFSET,TEXT_TOP_OFFSET) ) ;
        }

        private void ApplicationManager_ApplicationStyleChanged(object sender, EventArgs e)
        {
            SyncAppearanceToApplicationStyle() ;
            PerformLayout() ;
            Invalidate() ;
        }

        private void SyncAppearanceToApplicationStyle()
        {
            _closeButton.BackColor = ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor;

            if ( _backgroundBrush != null )
                _backgroundBrush.Dispose();
            _backgroundBrush = new SolidBrush(Color.FromArgb(246,243,240)) ;

            if ( _titleBarBrush != null )
                _titleBarBrush.Dispose();
            _titleBarBrush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor) ;

            if ( _borderPen != null )
                _borderPen.Dispose();
            _borderPen = new Pen(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor) ;

            if ( _textBrush != null )
                _textBrush.Dispose();
            _textBrush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTextColor) ;
        }
        private Brush _backgroundBrush ;
        private Brush _titleBarBrush ;
        private Pen _borderPen ;
        private Brush _textBrush ;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if ( _backgroundBrush != null )
                    _backgroundBrush.Dispose();
                if ( _titleBarBrush != null )
                    _titleBarBrush.Dispose();
                if ( _borderPen != null )
                    _borderPen.Dispose();
                if ( _textBrush != null )
                    _textBrush.Dispose();

                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        /*
        private class ParentFrameEventMonitor : NativeWindow, IDisposable
        {
            public ParentFrameEventMonitor( IWin32Window parentWindow )
            {
                AssignHandle(parentWindow.Handle);
            }

            public void Dispose()
            {
                ReleaseHandle() ;
            }

            protected override void WndProc(ref Message m)
            {
                // always allow default processing
                base.WndProc (ref m);

                if ( m.Msg == WM.ACTIVATE )
                {

                    // detect switching between the two forms
                    if ( lParam == Handle )
                        switchedBetweenForms = true ;
                    else
                        switchedBetweenForms = false ;

                }

            }
        }
        */

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Size = new System.Drawing.Size(300,300);
            this.Text = "PropertyEditingMiniForm";
        }
        #endregion

    }
}
