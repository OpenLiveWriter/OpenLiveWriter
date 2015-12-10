// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{

    internal class MapPushpinForm : MiniForm
    {
        private string UNTITLED_PUSHPIN = Res.Get(StringId.PushpinUntitled);
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Label labelNotes;
        private System.Windows.Forms.TextBox textBoxNotes;
        private System.Windows.Forms.TextBox textBoxPhotoUrl;
        private System.Windows.Forms.Label labelPhotoUrl;
        private System.Windows.Forms.TextBox textBoxMoreInfoUrl;
        private System.Windows.Forms.Label labelMoreInfoUrl;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private MapPushpinEditedHandler _pushpinEditedHandler;

        public MapPushpinForm(IWin32Window parentFrame, Point location, MapPushpinEditedHandler pushpinEditedHandler)
            : this(parentFrame, location, pushpinEditedHandler, null)
        {
        }

        public MapPushpinForm(IWin32Window parentFrame, Point location, MapPushpinEditedHandler pushpinEditedHandler, MapPushpinInfo pushpinInfo)
            : base(parentFrame)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            if (SystemInformation.HighContrast)
                this.BackColor = SystemColors.Control;

            this.labelTitle.Text = Res.Get(StringId.PushpinTitle);
            this.textBoxTitle.Text = Res.Get(StringId.PushpinUntitled);
            this.labelNotes.Text = Res.Get(StringId.PushpinNotes);
            this.labelPhotoUrl.Text = Res.Get(StringId.PushpinPhotoUrl);
            this.labelMoreInfoUrl.Text = Res.Get(StringId.PushpinMoreInfoUrl);
            this.buttonSave.Text = Res.Get(StringId.SaveButton);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);

            _pushpinEditedHandler = pushpinEditedHandler;

            if (pushpinInfo == null)
                pushpinInfo = new MapPushpinInfo(String.Empty);

            textBoxTitle.Text = pushpinInfo.Title;
            if (textBoxTitle.Text == String.Empty)
                textBoxTitle.Text = UNTITLED_PUSHPIN;

            textBoxNotes.Text = pushpinInfo.Notes;
            textBoxPhotoUrl.Text = pushpinInfo.PhotoUrl;
            textBoxMoreInfoUrl.Text = pushpinInfo.MoreInfoUrl;

            if (pushpinInfo.Title == String.Empty)
            {
                Text = Res.Get(StringId.PushpinAdd);
            }
            else
            {
                Text = Res.Get(StringId.PushpinEdit);
            }

            Location = location;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LayoutHelper.FixupOKCancel(buttonSave, buttonCancel);
        }

        private void buttonSave_Click(object sender, System.EventArgs e)
        {
            string title = StringHelper.Ellipsis(textBoxTitle.Text.Trim(), 600);
            if (title == String.Empty)
            {
                title = textBoxTitle.Text = UNTITLED_PUSHPIN;
            }

            // initialize the pushpin info
            MapPushpinInfo pushpinInfo = new MapPushpinInfo(title);

            // treat an empty title as the same as "Cancel" -- since this dialog dismisses
            // on deactivation we can't really throw up a message-box
            if (pushpinInfo.Title != String.Empty)
            {
                pushpinInfo.Notes = StringHelper.Ellipsis(textBoxNotes.Text.Trim(), 600);
                pushpinInfo.PhotoUrl = StringHelper.RestrictLength(UrlHelper.FixUpUrl(textBoxPhotoUrl.Text), 600);
                pushpinInfo.MoreInfoUrl = StringHelper.RestrictLength(UrlHelper.FixUpUrl(textBoxMoreInfoUrl.Text), 600);

                _pushpinEditedHandler(pushpinInfo);
            }

            Close();
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.ResetClip();
            BidiGraphics g = new BidiGraphics(e.Graphics, Bounds, false);

            using (Brush brush = new SolidBrush(SystemColors.ActiveCaption))
            {
                g.FillRectangle(brush, new Rectangle(0, 0, Width - 1, SystemInformation.ToolWindowCaptionHeight));
            }

            using (Pen borderPen = new Pen(Color.FromArgb(127, 157, 185)))
            {
                g.DrawRectangle(borderPen, new Rectangle(0, 0, Width - 1, Height - 1));
                g.DrawLine(borderPen, 0, SystemInformation.ToolWindowCaptionHeight, Width - 1, SystemInformation.ToolWindowCaptionHeight);
            }

            using (Font boldFont = new Font(Font, FontStyle.Bold))
                g.DrawText(Text, boldFont, new Rectangle(4, 2, Width - 2, SystemInformation.ToolWindowCaptionHeight - 1), SystemColors.ActiveCaptionText);

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
            this.labelTitle = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.textBoxNotes = new System.Windows.Forms.TextBox();
            this.labelNotes = new System.Windows.Forms.Label();
            this.textBoxPhotoUrl = new System.Windows.Forms.TextBox();
            this.labelPhotoUrl = new System.Windows.Forms.Label();
            this.textBoxMoreInfoUrl = new System.Windows.Forms.TextBox();
            this.labelMoreInfoUrl = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // labelTitle
            //
            this.labelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTitle.Location = new System.Drawing.Point(8, 23);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(185, 14);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "&Title:";
            //
            // textBoxTitle
            //
            this.textBoxTitle.Location = new System.Drawing.Point(8, 39);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(220, 21);
            this.textBoxTitle.TabIndex = 2;
            this.textBoxTitle.Text = "Untitled pushpin";
            //
            // textBoxNotes
            //
            this.textBoxNotes.AcceptsReturn = true;
            this.textBoxNotes.Location = new System.Drawing.Point(8, 81);
            this.textBoxNotes.Multiline = true;
            this.textBoxNotes.Name = "textBoxNotes";
            this.textBoxNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxNotes.Size = new System.Drawing.Size(220, 67);
            this.textBoxNotes.TabIndex = 4;
            this.textBoxNotes.Text = "";
            //
            // labelNotes
            //
            this.labelNotes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelNotes.Location = new System.Drawing.Point(8, 66);
            this.labelNotes.Name = "labelNotes";
            this.labelNotes.Size = new System.Drawing.Size(215, 14);
            this.labelNotes.TabIndex = 3;
            this.labelNotes.Text = "&Notes:";
            //
            // textBoxPhotoUrl
            //
            this.textBoxPhotoUrl.Location = new System.Drawing.Point(8, 168);
            this.textBoxPhotoUrl.Name = "textBoxPhotoUrl";
            this.textBoxPhotoUrl.Size = new System.Drawing.Size(220, 21);
            this.textBoxPhotoUrl.TabIndex = 6;
            this.textBoxPhotoUrl.Text = "";
            //
            // labelPhotoUrl
            //
            this.labelPhotoUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPhotoUrl.Location = new System.Drawing.Point(8, 153);
            this.labelPhotoUrl.Name = "labelPhotoUrl";
            this.labelPhotoUrl.Size = new System.Drawing.Size(211, 14);
            this.labelPhotoUrl.TabIndex = 5;
            this.labelPhotoUrl.Text = "&Photo URL:";
            //
            // textBoxMoreInfoUrl
            //
            this.textBoxMoreInfoUrl.Location = new System.Drawing.Point(8, 211);
            this.textBoxMoreInfoUrl.Name = "textBoxMoreInfoUrl";
            this.textBoxMoreInfoUrl.Size = new System.Drawing.Size(220, 21);
            this.textBoxMoreInfoUrl.TabIndex = 8;
            this.textBoxMoreInfoUrl.Text = "";
            //
            // labelMoreInfoUrl
            //
            this.labelMoreInfoUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelMoreInfoUrl.Location = new System.Drawing.Point(8, 195);
            this.labelMoreInfoUrl.Name = "labelMoreInfoUrl";
            this.labelMoreInfoUrl.Size = new System.Drawing.Size(216, 14);
            this.labelMoreInfoUrl.TabIndex = 7;
            this.labelMoreInfoUrl.Text = "&URL for more info:";
            //
            // buttonSave
            //
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSave.Location = new System.Drawing.Point(73, 238);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.TabIndex = 9;
            this.buttonSave.Text = "&Save";
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(154, 238);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // MapPushpinForm
            //
            this.AcceptButton = this.buttonSave;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(236, 268);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.textBoxMoreInfoUrl);
            this.Controls.Add(this.textBoxPhotoUrl);
            this.Controls.Add(this.textBoxNotes);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.labelMoreInfoUrl);
            this.Controls.Add(this.labelPhotoUrl);
            this.Controls.Add(this.labelNotes);
            this.Controls.Add(this.labelTitle);
            this.Name = "MapPushpinForm";
            this.Text = "MapPushpinForm";
            this.ResumeLayout(false);

        }
        #endregion

    }

    internal delegate void MapPushpinEditedHandler(MapPushpinInfo pushpinInfo);

    internal class MapPushpinInfo
    {
        public MapPushpinInfo(string title)
        {
            _title = title;
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        private string _title;

        public string Notes
        {
            get { return _notes; }
            set { _notes = value; }
        }
        private string _notes = String.Empty;

        public string PhotoUrl
        {
            get { return _photoUrl; }
            set { _photoUrl = value; }
        }
        private string _photoUrl = String.Empty;

        public string MoreInfoUrl
        {
            get { return _moreInfoUrl; }
            set { _moreInfoUrl = value; }
        }
        private string _moreInfoUrl = String.Empty;

    }
}
