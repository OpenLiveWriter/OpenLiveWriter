// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Host form for image decorators editors.
    /// </summary>
    public class ImageDecoratorEditorForm : BaseForm
    {
        private Panel panelEditor;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        /*
        private Size offsetFromOwner;
        */

        public ImageDecoratorEditorForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                /*
                (this.Owner as MainFrameSatelliteWindow).LocationFixed -=new EventHandler(Owner_LocationChanged);
                (this.Owner as MainFrameSatelliteWindow).CloseFixed -=new EventHandler(Owner_Closed);
                */
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelEditor = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            //
            // panelEditor
            //
            this.panelEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelEditor.Location = new System.Drawing.Point(4, 8);
            this.panelEditor.Name = "panelEditor";
            this.panelEditor.Size = new System.Drawing.Size(284, 252);
            this.panelEditor.TabIndex = 0;
            //
            // ImageDecoratorEditorForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.panelEditor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageDecoratorEditorForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Image Decorator";
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Set the image decorator editor to display in this form.
        /// </summary>
        public ImageDecoratorEditor ImageDecoratorEditor
        {
            set
            {
                SuspendLayout();
                try
                {
                    this.panelEditor.Controls.Clear();
                    if (value != null)
                    {
                        this.panelEditor.Controls.Add(value);
                        value.Dock = DockStyle.Fill;

                        //resize the form to the editor's preferred size;
                        Size preferredSize = value.GetPreferredSize();
                        int diffHeight = preferredSize.Height - panelEditor.Height;
                        int diffWidth = preferredSize.Width - panelEditor.Width;
                        this.Size = new Size(Width + diffWidth, Height + diffHeight);
                        Trace.Assert(!string.IsNullOrEmpty(value.Text), "Title of ImageDecoratorEditor cannot be null or empty.");
                        this.Text = value.Text;

                        FormBorderStyle = value.FormBorderStyle;
                    }
                }
                finally
                {
                    ResumeLayout();
                }

                PerformLayout();
                Invalidate();
            }
        }

        /*
        public void SetOwner(Form owner)
        {
            this.Owner = owner;
            this.Location = new Point(owner.Location.X + 100, owner.Location.Y + 100);
            (owner as MainFrameSatelliteWindow).LocationFixed +=new EventHandler(Owner_LocationChanged);
            (owner as MainFrameSatelliteWindow).CloseFixed +=new EventHandler(Owner_Closed);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            // update frame offset
            Point frameTopRight = GetFrameTopRight() ;
            offsetFromOwner = new Size(Left - frameTopRight.X, Top - frameTopRight.Y );
        }

        private Point GetFrameTopRight()
        {
            Point frameTopRight = this.Owner.Location ;
            frameTopRight.Offset(this.Owner.Size.Width,0) ;
            return frameTopRight ;
        }

        private void Owner_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Owner_LocationChanged(object sender, EventArgs e)
        {
            Point location = GetFrameTopRight() ;
            Size frameOffset = offsetFromOwner ;
            location.Offset(frameOffset.Width, frameOffset.Height);

            Location = EnsureOnScreen(location) ;
        }

        private Point EnsureOnScreen(Point location)
        {
            int left = 0;
            int right = 0;
            foreach (Screen currentScreen in Screen.AllScreens)
            {
                Rectangle currentScreenBounds = currentScreen.Bounds;
                left = Math.Min(left, currentScreenBounds.Left);
                right = Math.Max(right, currentScreenBounds.Right);
            }

            if (location.X > right)
                location.X = right - Width;
            else if (location.X < left)
                location.X = left;

            return location;
        }
        */
    }
}
