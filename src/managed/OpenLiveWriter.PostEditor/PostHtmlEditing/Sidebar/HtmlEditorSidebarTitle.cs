// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    internal class HtmlEditorSidebarTitle : Panel
    {
        private const int TOP_INSET = 2;

        private BitmapButton buttonChevron;
        private SidebarTitleUITheme _uiTheme;

        public HtmlEditorSidebarTitle()
        {
            // enable double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            Height = 30;

            buttonChevron = new BitmapButton(this.components);
            buttonChevron.ButtonStyle = ButtonStyle.Bitmap;
            buttonChevron.ButtonText = Res.Get(StringId.HideSidebar);
            buttonChevron.ToolTip = Res.Get(StringId.HideSidebar);
            buttonChevron.Click += new EventHandler(ClickHandler);
            buttonChevron.AccessibleName = Res.Get(StringId.HideSidebar);
            buttonChevron.TabStop = true;
            buttonChevron.TabIndex = 0;
            buttonChevron.RightToLeft = (BidiHelper.IsRightToLeft ? RightToLeft.No : RightToLeft.Yes);
            buttonChevron.AllowMirroring = true;
            Controls.Add(buttonChevron);

            Click += new EventHandler(ClickHandler);

            //create the UI theme
            _uiTheme = new SidebarTitleUITheme(this);

            buttonChevron.Bounds =
                RectangleHelper.Center(_uiTheme.bmpChevronRight.Size, new Rectangle(0, 0, 20, ClientSize.Height), false);
        }

        public void UpdateTitle( string titleText )
        {
            Text = titleText ;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);
            using (Brush b = new SolidBrush(ColorizedResources.Instance.BorderDarkColor))
                g.FillRectangle(b, ClientRectangle);

            TextFormatFlags tf = TextFormatFlags.VerticalCenter;
            int width = g.MeasureText(Text, Font).Width;
            g.DrawText(Text, Font, new Rectangle(20, -1, width, ClientSize.Height), _uiTheme.TextColor, tf);
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
        private void ClickHandler(object sender, EventArgs e)
        {
            OnHideTitleBarClicked( EventArgs.Empty ) ;
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
            }
            base.Dispose( disposing );
        }

        private class SidebarTitleUITheme : ControlUITheme
        {
            private Bitmap bmpChevronRightBase = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Sidebar.Images.ChevronRight.png");
            internal Bitmap bmpChevronRight;
            private Bitmap bmpChevronRightHover = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Sidebar.Images.ChevronRightHover.png");
            private Color _textColor;
            private HtmlEditorSidebarTitle _sidebarTitleControl;
            public SidebarTitleUITheme(HtmlEditorSidebarTitle sidebarTitleControl) : base(sidebarTitleControl, false)
            {
                _sidebarTitleControl = sidebarTitleControl;
                ColorizedResources.Instance.ColorizationChanged += new EventHandler(ColorizationChanged);
                ApplyTheme();
            }

            protected override void Dispose()
            {
                ColorizedResources.Instance.ColorizationChanged -= new EventHandler(ColorizationChanged);
            }

            protected override void ApplyTheme(bool highContrast)
            {
                if(bmpChevronRight != null && bmpChevronRight != bmpChevronRightBase)
                    bmpChevronRight.Dispose();

                if(highContrast)
                {
                    _textColor = SystemColors.ControlText;
                    _sidebarTitleControl.buttonChevron.BackColor = SystemColors.Control;

                    //convert the chevon's White color to the Window Text color (fixes bug 437444)
                    bmpChevronRight = new Bitmap(bmpChevronRightBase);
                    ColorMap colorMap = new ColorMap();
                    colorMap.OldColor = Color.White;
                    colorMap.NewColor = SystemColors.WindowText;
                    ImageHelper.ConvertColors(bmpChevronRight, colorMap);
                }
                else
                {
                    _textColor = Color.White;
                    _sidebarTitleControl.buttonChevron.BackColor = ColorizedResources.Instance.SidebarHeaderBackgroundColor;
                    bmpChevronRight = bmpChevronRightBase;
                }

                _sidebarTitleControl.buttonChevron.BackColor = ColorizedResources.Instance.SidebarHeaderBackgroundColor;
                _sidebarTitleControl.buttonChevron.BitmapEnabled = bmpChevronRight;
                _sidebarTitleControl.buttonChevron.BitmapSelected = bmpChevronRightHover;
                _sidebarTitleControl.buttonChevron.BitmapPushed = bmpChevronRightHover;
            }

            private void ColorizationChanged(object sender, EventArgs e)
            {
                _sidebarTitleControl.buttonChevron.BackColor = ColorizedResources.Instance.SidebarHeaderBackgroundColor;
            }

            public Color TextColor { get { return _textColor; }}
        }

        /// <summary>
        /// Embedded components
        /// </summary>
        private Container components = new Container();

        // close button images
        private const string IMAGE_RESOURCE_PATH = "Images." ;
        private Bitmap closeButtonDisabled = ResourceHelper.LoadAssemblyResourceBitmap( IMAGE_RESOURCE_PATH + "CloseButtonDisabled.png") ;
        private Bitmap closeButtonEnabled = ResourceHelper.LoadAssemblyResourceBitmap( IMAGE_RESOURCE_PATH + "CloseButtonEnabled.png") ;
        private Bitmap closeButtonPushed = ResourceHelper.LoadAssemblyResourceBitmap( IMAGE_RESOURCE_PATH + "CloseButtonPushed.png") ;
        private Bitmap closeButtonSelected = ResourceHelper.LoadAssemblyResourceBitmap( IMAGE_RESOURCE_PATH + "CloseButtonSelected.png") ;
    }

}
