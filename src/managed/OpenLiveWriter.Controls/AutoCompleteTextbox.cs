// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls
{
    public partial class AutoCompleteTextbox : TextBox
    {

        private readonly AutoCompleteForm tagSuggestForm;
        private AutoCompleteSource tagSource;
        private BitmapButton button;

        private Font normalFont;
        private Font cueFont;

        public AutoCompleteTextbox()
        {
            tagSuggestForm = new AutoCompleteForm();
            tagSuggestForm.CreateControl();
            tagSuggestForm.TagSelected += TagSelected;
            CreateButton();

            GotFocus += new EventHandler(AutoCompleteTextbox_Enter);
            LostFocus += new EventHandler(AutoCompleteTextbox_Leave);

            normalFont = Res.DefaultFont;
            cueFont = Res.DefaultFont;
        }

        private bool rtlFixedUp = false;
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!BidiHelper.IsRightToLeft || RightToLeft == RightToLeft.No)
                User32.SendMessage(Handle, WM.EM_SETMARGINS, (UIntPtr)EC.RIGHTMARGIN, new IntPtr((BUTTON_WIDTH + BUTTON_RIGHT_OFFSET) << 16));
            else
            {
                if (!rtlFixedUp)
                {
                    rtlFixedUp = true;

                    int leftMargin = Margin.Left;
                    int rightMargin = Margin.Right;
                    BidiHelper.RtlLayoutFixup(this);
                    Margin = new Padding(leftMargin, Padding.Top, rightMargin, Padding.Bottom);
                }
            }
        }

        void AutoCompleteTextbox_Leave(object sender, EventArgs e)
        {
            if (Text == String.Empty)
            {
                _isDirty = false;
                SetDefaultText();
            }
            tagSuggestForm.Dismissed = false;
        }

        void AutoCompleteTextbox_Enter(object sender, EventArgs e)
        {
            if (!_isDirty)
            {
                _isDirty = true;
                Text = String.Empty;
                ForeColor = SystemColors.WindowText;
                Font = normalFont;
            }
        }

        public bool ShowButton
        {
            get
            {
                return button.Visible;
            }
            set
            {
                button.Visible = value;
            }
        }
        private const int BUTTON_WIDTH = 16;
        private const int BUTTON_RIGHT_OFFSET = 8;
        private void CreateButton()
        {
            button = new BitmapButton();
            button.BitmapEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.refresh.png");
            button.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.refreshPushed.png");
            button.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.refreshSelected.png");
            button.Anchor = AnchorStyles.Right;
            button.Height = 16;
            button.Width = BUTTON_WIDTH;
            button.Cursor = Cursors.Hand;
            button.Left = Width - button.Width - BUTTON_RIGHT_OFFSET;
            button.AccessibleName = Res.Get(StringId.CategoryRefreshList);
            button.ToolTip = Res.Get(StringId.CategoryRefreshList);
            Controls.Add(button);
        }

        private bool _isDirty = false;
        public string _defaultText;
        public string DefaultText
        {
            get
            {
                return _defaultText;
            }
            set
            {
                _defaultText = value;
                AccessibleName = _defaultText;
                if (!_isDirty)
                {
                    SetDefaultText();
                }
            }
        }

        private void SetDefaultText()
        {
            base.Text = _defaultText;
            ForeColor = SystemColors.GrayText;
            Font = cueFont;
        }

        public string TextValue
        {
            get { return _isDirty ? Text : ""; }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value) && !Focused && !button.Focused)
                {
                    ClearTextbox();
                    return;
                }

                _isDirty = true;
                ForeColor = SystemColors.WindowText;
                Font = normalFont;
                base.Text = value;
            }
        }

        public void ClearTextbox()
        {
            _isDirty = false;
            SetDefaultText();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize = base.GetPreferredSize(proposedSize);

            // WinLive 118369: Windows Forms doesn't take into account that the refresh button may be drawn over
            // text, so we add extra padding to leave enough room to fit the text and the button.
            if (ShowButton)
                preferredSize.Width += BUTTON_WIDTH + BUTTON_RIGHT_OFFSET * 2;

            return preferredSize;
        }

        public event EventHandler ButtonClicked
        {
            add
            {
                button.Click += value;
            }
            remove
            {
                button.Click -= value;
            }
        }

        public event EventHandler DirtyChanged;
        public void FireDirtyChanged()
        {
            if (DirtyChanged != null)
                DirtyChanged(this, EventArgs.Empty);
        }

        public AutoCompleteSource TagSource
        {
            set { tagSource = value; }
        }

        private void TagSelected(object sender, EventArgs e)
        {
            int pos;
            int len;
            string prefix = tagSource.GetPrefix(this, out pos, out len);
            Trace.Assert(!string.IsNullOrEmpty(prefix));

            Select(pos, len);
            SelectedText = tagSuggestForm.SelectedTag;
            Select(SelectionStart + SelectionLength, 0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                tagSuggestForm.Dispose();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            tagSuggestForm.Owner = FindForm();
            tagSuggestForm.Font = normalFont;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (tagSuggestForm != null)
                tagSuggestForm.Font = normalFont;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (!_isDirty)
                return;

            FireDirtyChanged();

            ShowSuggestionForm();

            base.OnTextChanged(e);
        }

        private void ShowSuggestionForm()
        {
            int pos;
            int segmentLength;

            if (tagSource == null)
            {
                tagSuggestForm.Hide();
                return;
            }

            string prefix = tagSource.GetPrefix(this, out pos, out segmentLength);

            if (string.IsNullOrEmpty(prefix))
            {
                tagSuggestForm.Hide();
            }
            else
            {
                prefix = prefix.ToLower(CultureInfo.CurrentUICulture);

                List<string> listChoices = new List<string>();
                tagSource.PopulateTags(prefix, listChoices);

                IntPtr p = User32.SendMessage(Handle, WM.EM_POSFROMCHAR, new IntPtr(pos), IntPtr.Zero);
                int x = p.ToInt32() & 0xFFFF;
                tagSuggestForm.Origin = PointToScreen(new Point(x, ClientSize.Height - 2));
                tagSuggestForm.SetSuggestions(FindForm(), listChoices);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!tagSuggestForm.ContainsFocus)
                tagSuggestForm.Visible = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    tagSuggestForm.MoveSelectionUp();
                    return true;
                case Keys.Down:
                    tagSuggestForm.MoveSelectionDown();
                    return true;
                case Keys.Escape:
                    if (!tagSuggestForm.Visible)
                        break;
                    tagSuggestForm.Dismissed = true;
                    return true;
                case Keys.Enter:
                case Keys.Tab:
                    if (tagSuggestForm.CanAcceptSelection)
                    {
                        tagSuggestForm.AcceptSelection();
                        return true;
                    }
                    break;
                case Keys.Control | Keys.Space:
                    tagSuggestForm.Dismissed = false;
                    ShowSuggestionForm();
                    return true;
                case Keys.Control | Keys.A:
                    SelectAll();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == ',')
            {
                //                if (tagSuggestForm.CanAcceptSelection)
                //                {
                //                    tagSuggestForm.AcceptSelection();
                //                    SelectedText = ", ";
                //                    e.Handled = true;
                //                }
                tagSuggestForm.Dismissed = false;
            }
        }
    }

    public abstract class AutoCompleteSource
    {
        public virtual string GetPrefix(AutoCompleteTextbox tstb, out int pos, out int segmentLength)
        {
            if (tstb.SelectionLength == 0)
            {
                string s = tstb.Text;
                int selPos = tstb.SelectionStart;
                Match m = Regex.Match(s, @"[^,\s][^,]*");
                while (m.Success)
                {
                    if (m.Index <= selPos && m.Index + m.Length >= selPos)
                    {
                        pos = m.Index;
                        segmentLength = m.Length;
                        return s.Substring(m.Index, selPos - m.Index);
                    }

                    m = m.NextMatch();
                }
            }
            pos = -1;
            segmentLength = -1;
            return null;
        }

        public abstract void PopulateTags(string prefix, List<string> tagList);
    }

    public class SimpleTagSource : AutoCompleteSource
    {
        private readonly List<string> tags;

        public SimpleTagSource(IEnumerable<string> tags)
        {
            this.tags = new List<string>(tags);
        }

        public override void PopulateTags(string prefix, List<string> tagList)
        {
            foreach (string str in tags)
            {
                if (str.ToLower(CultureInfo.CurrentUICulture).StartsWith(prefix, StringComparison.CurrentCulture))
                    tagList.Add(str);
            }

        }

    }

    public class AutoCompleteForm : Form
    {
        /// <summary>
        /// The position of this form relative to the owner form.
        /// </summary>
        private Point relativeLocation = Point.Empty;

        /// <summary>
        /// The desired position of the lower-left (or lower-right, in RTL) corner of the form.
        /// </summary>
        private Point origin;

        private bool dismissed;

        public AutoCompleteForm()
        {
            InitializeComponent();
            VisibleChanged += TagSuggestForm_VisibleChanged;
            lstSuggest.Resize += new EventHandler(lstSuggest_Resize);

            RightToLeft = BidiHelper.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
        }

        void lstSuggest_Resize(object sender, EventArgs e)
        {
            Size = lstSuggest.Size;
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public event EventHandler TagSelected;

        public bool CanAcceptSelection
        {
            get { return Visible && lstSuggest.SelectedIndex >= 0; }
        }

        public string SelectedTag
        {
            get { return lstSuggest.SelectedItem.ToString(); }
        }

        public void SetSuggestions(Form owner, IEnumerable<string> suggestions)
        {
            if (dismissed)
            {
                return;
            }

            lstSuggest.BeginUpdate();
            try
            {
                lstSuggest.Items.Clear();
                foreach (string sugg in suggestions)
                {
                    lstSuggest.Items.Add(sugg);
                }
                if (lstSuggest.Items.Count == 0)
                {
                    Visible = false;
                }
                else
                {
                    AdjustListSize();
                    if (!Visible)
                        Show(owner);

                    lstSuggest.SelectedIndex = 0;

                }
            }
            finally
            {
                lstSuggest.EndUpdate();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            AdjustListSize();

            base.OnShown(e);
        }

        private void AdjustListSize()
        {
            int width = 0;
            using (Graphics g = CreateGraphics())
            {
                foreach (string s in lstSuggest.Items)
                    width = Math.Max(width, TextRenderer.MeasureText(g, s, Font, Size.Empty, TextFormatFlags.NoPrefix).Width);
            }
            width += 3;
            int preferredHeight = lstSuggest.PreferredHeight;
            int height = Math.Min(preferredHeight, lstSuggest.ItemHeight * 11);
            if (height < preferredHeight)
                width += SystemInformation.VerticalScrollBarWidth;

            lstSuggest.Size = new Size(width, height);
        }

        public void MoveSelectionUp()
        {
            MoveSelection(true);
        }

        public void MoveSelectionDown()
        {
            MoveSelection(false);
        }

        private void MoveSelection(bool up)
        {
            int c = lstSuggest.Items.Count;
            if (c <= 1)
                return;

            int offset = up ? -1 : 1;
            lstSuggest.SelectedIndex = (lstSuggest.SelectedIndex + c + offset) % c;
        }

        private void lstSuggest_Click(object sender, EventArgs e)
        {
            AcceptSelection();
        }

        public void AcceptSelection()
        {
            if (TagSelected != null)
                TagSelected(this, EventArgs.Empty);
            Visible = false;
        }

        public bool Dismissed
        {
            set
            {
                if (value)
                    Visible = false;
                dismissed = value;
                Debug.WriteLine("dismissed: " + dismissed);
            }
        }

        public Point Origin
        {
            get { return origin; }
            set
            {
                if (origin != value)
                {
                    origin = value;
                    RepositionForm();
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            RepositionForm();
        }

        private void RepositionForm()
        {
            if (!BidiHelper.IsRightToLeft)
                Location = new Point(origin.X, origin.Y);
            else
                Location = new Point(origin.X - Width, origin.Y);
        }

        private void TagSuggestForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                VisibleChanged -= TagSuggestForm_VisibleChanged;
                if (Owner != null)
                    Owner.LocationChanged += ParentForm_LocationChanged;
            }

        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (Owner != null)
            {
                Point p = Owner.Location;
                relativeLocation = Point.Subtract(Location, new Size(p.X, p.Y));
            }
        }

        private void ParentForm_LocationChanged(object sender, EventArgs e)
        {
            Point p = Owner.Location;
            p.Offset(relativeLocation);
            Location = p;
        }

        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.lstSuggest = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            //
            // lstSuggest
            //
            this.lstSuggest.Location = new System.Drawing.Point(0, 0);
            this.lstSuggest.Name = "lstSuggest";
            this.lstSuggest.Size = new System.Drawing.Size(148, 108);
            this.lstSuggest.TabIndex = 0;
            this.lstSuggest.Click += new System.EventHandler(this.lstSuggest_Click);
            //
            // TagSuggestForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(148, 108);
            this.Controls.Add(this.lstSuggest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TagSuggestForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TagSuggestForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstSuggest;
    }
}

