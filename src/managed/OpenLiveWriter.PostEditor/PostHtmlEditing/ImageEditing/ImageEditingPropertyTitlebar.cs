// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class ImageEditingPropertyTitlebar : Panel
    {
        private const int TOP_INSET = 2;

        public ImageEditingPropertyTitlebar()
        {
            // enable double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // set height based on system metrics
            Height = SystemInformation.ToolWindowCaptionHeight ;

            // initialize down button
            closeButton = new BitmapButton() ;
            closeButton.BitmapDisabled = closeButtonDisabled ;
            closeButton.BitmapEnabled = closeButtonEnabled ;
            closeButton.BitmapPushed = closeButtonPushed ;
            closeButton.BitmapSelected = closeButtonSelected ;
            closeButton.ButtonStyle = ButtonStyle.Bitmap ;
            closeButton.ToolTip = "Hide Image Properties" ;
            closeButton.Width = closeButtonEnabled.Width ;
            closeButton.Height = closeButtonEnabled.Height;
            closeButton.Top = TOP_INSET - 1;
            closeButton.Left = Width - closeButton.Width - TOP_INSET ;
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right ;
            closeButton.Click +=new EventHandler(closeButton_Click);
            Controls.Add(closeButton) ;

            // manage appearance
            ApplicationStyleManager.ApplicationStyleChanged += new EventHandler(ApplicationManager_ApplicationStyleChanged);
            UpdateAppearance() ;
        }

        private void UpdateAppearance()
        {
            closeButton.BackColor = ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor ;

            PerformLayout();
            Invalidate();
        }

        /*
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {

        }
        */

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);

            // draw the background
            using ( Brush brush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor) )
                e.Graphics.FillRectangle( brush, ClientRectangle ) ;

            // draw the border
            using ( Pen pen = new Pen(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor) )
                e.Graphics.DrawLine(pen, 0, Height-1, Width-1, Height-1);

            // draw the text
            int TEXT_TOP_INSET = TOP_INSET ;
            using ( Brush brush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTextColor) )
                e.Graphics.DrawString( "Image Properties",ApplicationManager.ApplicationStyle.NormalApplicationFont, brush, new PointF(1,TEXT_TOP_INSET) ) ;

        }

        /// <summary>
        /// Close event (indicates that the user has hit the close button and wants
        /// the tray hidden)
        /// </summary>
        public event EventHandler HideTitleBarClicked ;

        /// <summary>
        /// Fires the Close event
        /// </summary>
        /// <param name="ea">event args</param>
        protected virtual void OnHideTitleBarClicked( EventArgs ea )
        {
            if ( HideTitleBarClicked != null )
                HideTitleBarClicked( this, ea ) ;
        }

        /// <summary>
        /// Handle close button click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            OnHideTitleBarClicked( EventArgs.Empty ) ;
        }

        /// <summary>
        /// Handle appearance preference changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplicationManager_ApplicationStyleChanged(object sender, EventArgs e)
        {
            UpdateAppearance() ;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
                ApplicationStyleManager.ApplicationStyleChanged -= new EventHandler(ApplicationManager_ApplicationStyleChanged);
            }
            base.Dispose( disposing );
        }

        // tray compontents
        private BitmapButton closeButton ;

        /// <summary>
        /// Embedded components
        /// </summary>
        private Container components = new Container();

        // close button images
        private const string TRAY_IMAGE_PATH = "Images." ;
        private Bitmap closeButtonDisabled = ResourceHelper.LoadAssemblyResourceBitmap( TRAY_IMAGE_PATH + "CloseButtonDisabled.png") ;
        private Bitmap closeButtonEnabled = ResourceHelper.LoadAssemblyResourceBitmap( TRAY_IMAGE_PATH + "CloseButtonEnabled.png") ;
        private Bitmap closeButtonPushed = ResourceHelper.LoadAssemblyResourceBitmap( TRAY_IMAGE_PATH + "CloseButtonPushed.png") ;
        private Bitmap closeButtonSelected = ResourceHelper.LoadAssemblyResourceBitmap( TRAY_IMAGE_PATH + "CloseButtonSelected.png") ;
    }

}
