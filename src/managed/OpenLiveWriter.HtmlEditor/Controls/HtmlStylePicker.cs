// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.HtmlEditor.Controls
{
    // @SharedCanvas - what happens when this is part of the ribbon?
    public interface IHtmlStylePicker
    {
        event EventHandler HtmlStyleChanged;
        IHtmlFormattingStyle SelectedStyle { get; }
        bool Enabled { get; set; }

        void SelectStyleByElementName(string p);
    }

    /// <summary>
    /// Summary description for HtmlStylePicker.
    /// </summary>
    public class HtmlStylePicker : UserControl, IHtmlStylePicker
    {
        private ComboBox styleComboBox;
        private System.ComponentModel.IContainer components;
        private ToolTip2 toolTip;

        /// <summary>
        /// The editor command source.
        /// </summary>
        IHtmlEditorCommandSource _commandSource;

        public HtmlStylePicker(IHtmlEditorCommandSource commandSource)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            styleComboBox.AccessibleName = Res.Get(StringId.TextStyle);
            this.AccessibleName = Res.Get(StringId.TextStyle);
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.ComboBox;

            _commandSource = commandSource;

            this.toolTip.SetToolTip(this.styleComboBox, Res.Get(StringId.ChangeTextStyle));

            this.SetStyles(defaultStyles);

            using (new AutoGrow(this, AnchorStyles.Right, true))
                DisplayHelper.AutoFitSystemCombo(styleComboBox, 0, int.MaxValue, false);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            Height = styleComboBox.Bottom;
        }

        private static IHtmlFormattingStyle[] defaultStyles = new IHtmlFormattingStyle[] {
                                                                                             new HtmlElementFormattingStyle(Res.Get(StringId.Heading1), "h1", _ELEMENT_TAG_ID.TAGID_H1),
                                                                                             new HtmlElementFormattingStyle(Res.Get(StringId.Heading2), "h2", _ELEMENT_TAG_ID.TAGID_H2),
                                                                                             new HtmlElementFormattingStyle(Res.Get(StringId.Heading3), "h3", _ELEMENT_TAG_ID.TAGID_H3),
                                                                                             new HtmlElementFormattingStyle(Res.Get(StringId.Heading4), "h4", _ELEMENT_TAG_ID.TAGID_H4),
                                                                                             new HtmlElementFormattingStyle(Res.Get(StringId.Heading5), "h5", _ELEMENT_TAG_ID.TAGID_H5),
                                                                                             new HtmlElementFormattingStyle(Res.Get(StringId.Heading6), "h6", _ELEMENT_TAG_ID.TAGID_H6),
                                                                                             new HtmlElementFormattingStyle(Res.Get(StringId.Paragraph), "p", _ELEMENT_TAG_ID.TAGID_P)
                                                                                         };

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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.styleComboBox = new System.Windows.Forms.ComboBox();
            this.toolTip = new ToolTip2(this.components);
            this.SuspendLayout();
            //
            // styleComboBox
            //
            this.styleComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.styleComboBox.DisplayMember = "Change Text Style";
            this.styleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleComboBox.Location = new System.Drawing.Point(0, 0);
            this.styleComboBox.MaxDropDownItems = 15;
            this.styleComboBox.Name = "styleComboBox";
            this.styleComboBox.Size = new System.Drawing.Size(76, 21);
            this.styleComboBox.TabIndex = 0;
            this.toolTip.SetToolTip(this.styleComboBox, "Change Text Style");
            this.styleComboBox.SelectionChangeCommitted += new System.EventHandler(this.styleComboBox_SelectionChangeCommitted);
            //
            // HtmlStylePicker
            //
            this.Controls.Add(this.styleComboBox);
            this.Name = "HtmlStylePicker";
            this.Size = new System.Drawing.Size(76, 21);
            this.ResumeLayout(false);

        }
        #endregion

        public event EventHandler HtmlStyleChanged;
        protected virtual void OnHtmlStyleChanged(EventArgs evt)
        {
            if (HtmlStyleChanged != null)
            {
                HtmlStyleChanged(this, evt);
            }
        }

        public void SetStyles(IHtmlFormattingStyle[] styles)
        {
            //clear any existing styles
            styleComboBox.Items.Clear();
            styleTable.Clear();

            //add the new styles
            foreach (IHtmlFormattingStyle style in styles)
            {
                StyleItem styleItem = new StyleItem(style);
                styleComboBox.Items.Add(styleItem);
                styleTable[style.ElementName] = styleItem;
            }
        }
        Hashtable styleTable = new Hashtable();

        public void SelectStyleByElementName(string name)
        {
            StyleItem styleItem = name != null ? (StyleItem)styleTable[name] : null;
            styleComboBox.SelectedItem = styleItem;
        }

        private void styleComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnHtmlStyleChanged(EventArgs.Empty);
        }

        public IHtmlFormattingStyle SelectedStyle
        {
            get
            {
                StyleItem styleItem = (StyleItem)styleComboBox.SelectedItem;
                if (styleItem != null)
                    return styleItem._style;
                else
                    return null;
            }
        }

        private class StyleItem
        {
            internal IHtmlFormattingStyle _style;
            public StyleItem(IHtmlFormattingStyle style)
            {
                _style = style;
            }

            public override string ToString()
            {
                return _style.DisplayName;
            }
        }

        public class HtmlElementFormattingStyle : IHtmlFormattingStyle
        {
            private string _name;
            private string _element;
            private _ELEMENT_TAG_ID _elementTagId;
            public HtmlElementFormattingStyle(string name, string element, _ELEMENT_TAG_ID elementTagId)
            {
                _name = name;
                _element = element;
                _elementTagId = elementTagId;
            }

            public string DisplayName
            {
                get
                {
                    return _name;
                }
            }

            public string ElementName
            {
                get
                {
                    return _element;
                }
            }

            public _ELEMENT_TAG_ID ElementTagId
            {
                get { return _elementTagId; }
            }
        }
    }
}
