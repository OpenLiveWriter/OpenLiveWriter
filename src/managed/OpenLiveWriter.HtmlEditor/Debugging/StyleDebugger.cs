// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Debugging
{

    /// <summary>
    /// Summary description for StyleDebugger.
    /// </summary>
    public class StyleDebugger : Form
    {

#if DEBUG_STYLES

        private ColumnHeader columnHeaderStyleName;
        private ColumnHeader columnHeaderStyleValue;
        private ListView listViewStyle;
        private Button buttonLoad;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public StyleDebugger()
        {
            Init();
        }

        /*public StyleDebugger(IWin32Window parentFrame) : base(parentFrame)
        {
            Init();

        }*/

        private void Init()
        {
            InitializeComponent();
            InitStyleItems();
        }

        public static void ShowDebugger(IWin32Window parentFrame, MshtmlEditorEx editor)
        {
            StyleDebugger styleDebugger = new StyleDebugger();
            styleDebugger.MshtmlEditor = editor;
            styleDebugger.Location = new Point(0, 0);
            styleDebugger.Show();
            styleDebugger.BringToFront();
        }

        #region Style list items
        private void InitStyleItems()
        {
            //load the style list items
            AddStyleItem("color", new StyleExtractor(GetColor));
            AddStyleItem("font-size", new StyleExtractor(GetFontSize));
            AddStyleItem("font-family", new StyleExtractor(GetFontFamily));
            AddStyleItem("font-style", new StyleExtractor(GetFontStyle));
            AddStyleItem("font-variant", new StyleExtractor(GetFontVariant));
            AddStyleItem("font-weight", new StyleExtractor(GetFontWeight));
            AddStyleItem("padding", new StyleExtractor(GetPadding));
            AddStyleItem("margin", new StyleExtractor(GetMargin));
        }
        private delegate object StyleExtractor(IHTMLCurrentStyle style);

        private object GetColor(IHTMLCurrentStyle style){ return style.color; }
        private object GetFontSize(IHTMLCurrentStyle style){ return style.fontSize; }
        private object GetFontFamily(IHTMLCurrentStyle style){ return style.fontFamily; }
        private object GetFontStyle(IHTMLCurrentStyle style){ return style.fontStyle; }
        private object GetFontVariant(IHTMLCurrentStyle style){ return style.fontVariant; }
        private object GetFontWeight(IHTMLCurrentStyle style){ return style.fontWeight; }
        private object GetPadding(IHTMLCurrentStyle style){ return style.padding; }
        private object GetMargin(IHTMLCurrentStyle style){ return style.margin; }

        private void AddStyleItem(string name, StyleExtractor extractor)
        {
            listViewStyle.Items.Add(new StyleListItem(name, extractor));
        }

        private void RefreshStyleItems(IHTMLCurrentStyle style)
        {
            //listViewStyle.BeginUpdate();
            foreach(StyleListItem item in listViewStyle.Items)
                item.RefreshStyle(style);
            //listViewStyle.EndUpdate();
        }

        #endregion

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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listViewStyle = new System.Windows.Forms.ListView();
            this.columnHeaderStyleName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderStyleValue = new System.Windows.Forms.ColumnHeader();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // listViewStyle
            //
            this.listViewStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewStyle.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                            this.columnHeaderStyleName,
                                                                                            this.columnHeaderStyleValue});
            this.listViewStyle.Location = new System.Drawing.Point(4, 4);
            this.listViewStyle.Name = "listViewStyle";
            this.listViewStyle.Size = new System.Drawing.Size(284, 332);
            this.listViewStyle.TabIndex = 0;
            this.listViewStyle.View = System.Windows.Forms.View.Details;
            //
            // columnHeaderStyleName
            //
            this.columnHeaderStyleName.Text = "Attribute";
            this.columnHeaderStyleName.Width = 98;
            //
            // columnHeaderStyleValue
            //
            this.columnHeaderStyleValue.Text = "Value";
            this.columnHeaderStyleValue.Width = 149;
            //
            // buttonLoad
            //
            this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoad.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonLoad.Location = new System.Drawing.Point(204, 344);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.TabIndex = 1;
            this.buttonLoad.Text = "Load Style";
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            //
            // StyleDebugger
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(292, 374);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.listViewStyle);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.Name = "StyleDebugger";
            this.Text = "StyleDebugger";
            this.ResumeLayout(false);

        }
        #endregion

        public MshtmlEditorEx MshtmlEditor
        {
            get { return mshtmlEditor; }
            set { mshtmlEditor = value; }
        }
        private MshtmlEditorEx mshtmlEditor;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            MarkupRange selection = GetSelectedMarkupRange();
            if(selection.IsEmpty())
            {
                this.listViewStyle.Enabled = true;

                IHTMLElement2 element = (IHTMLElement2)selection.Start.CurrentScope;
                IHTMLCurrentStyle style = element.currentStyle;
                RefreshStyleItems(style);
            }
            else
            {
                this.listViewStyle.Enabled = false;
            }
        }

        private MshtmlMarkupServices MarkupServices
        {
            get
            {
                return mshtmlEditor.MshtmlControl.MarkupServices;
            }
        }

        private IHTMLDocument2 HTMLDocument
        {
            get
            {
                return mshtmlEditor.HTMLDocument;
            }
        }

        /// <summary>
        /// Get current selection pointers
        /// </summary>
        /// <returns>selection pointers</returns>
        public MarkupRange GetSelectedMarkupRange()
        {
            return MarkupServices.CreateMarkupRange( SelectedRange ) ;
        }

        /// <summary>
        /// Currently selected range (null if there is no selection)
        /// </summary>
        private IHTMLTxtRange SelectedRange
        {
            get
            {
                // get hte selection
                IHTMLSelectionObject selection = HTMLDocument.selection ;
                if ( selection == null )
                {
                    return null ;
                }

                // see what type of range is selected
                object range = selection.createRange() ;
                if ( range is IHTMLTxtRange )
                {
                    return range as IHTMLTxtRange ;
                }
                else if ( range is IHTMLControlRange )
                {
                    // we only support single-selection so a "control-range" can always
                    // be converted into a single-element text range
                    IHTMLControlRange controlRange = range as IHTMLControlRange ;
                    if ( controlRange.length == 1)
                    {
                        //bug fix 1793: use markup services to select the range of markup because the
                        //IHTMLTxtRange.moveToElementText() operation doesn't create a reasonable
                        //selection range for <img> selections within an anchor (thumbnails, etc)
                        IHTMLElement selectedElement = controlRange.item(0);
                        MarkupRange markupRange = MarkupServices.CreateMarkupRange(selectedElement);
                        if(selectedElement.parentElement.tagName == "A")
                        {
                            //expand the selection to include the anchor if there is no content between
                            //the selected element and its anchor.
                            markupRange.MoveOutwardIfNoContent();
                        }

                        //return the precisely positioned text range
                        return markupRange.ToTextRange();
                    }
                    else
                    {
                        Debug.Fail( "Length of control range not equal to 1 (value was " + controlRange.length.ToString() ) ;
                        return null ;
                    }
                }
                else // null or unexpected range type
                {
                    return null ;
                }
            }
        }
        private class StyleListItem : ListViewItem
        {
            string _name;
            StyleExtractor _extractor;
            public StyleListItem(string name, StyleExtractor extractor)
            {
                _name = name;
                _extractor = extractor;
                SubItems.Add("");
                SubItems.Add("");
            }

            public void RefreshStyle(IHTMLCurrentStyle style)
            {
                object val = _extractor(style);
                SubItems[0].Text = _name;
                SubItems[1].Text = val != null ? val.ToString() : "";
            }
        }

#endif
    }

}
