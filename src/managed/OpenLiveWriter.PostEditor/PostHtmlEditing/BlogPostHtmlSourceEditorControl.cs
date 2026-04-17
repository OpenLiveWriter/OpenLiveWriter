// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.SpellChecker;
using OpenLiveWriter.WebView2Shim;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class BlogPostHtmlSourceEditorControl : UserControl, IBlogPostHtmlEditor
    {
        private Panel panelSourceEditor;
        private TextBox textBoxTitle;
        private HtmlSourceEditorControl sourceControl;
        private WebView2SourceEditorControl webView2SourceEditor;
        private readonly bool _useCodeMirror;
        private readonly IBlogPostSpellCheckingContext spellingContext;
        private IBlogPostImageEditingContext editingContext;

        public BlogPostHtmlSourceEditorControl(IBlogPostSpellCheckingContext spellingContext, CommandManager commandManager, IBlogPostImageEditingContext editingContext)
        {
            this.spellingContext = spellingContext;
            this.editingContext = editingContext;
            InitializeComponent();

            // Always create sourceControl for its functionality
            sourceControl = new HtmlSourceEditorControl(spellingContext.SpellingChecker, commandManager, editingContext);
            sourceControl.EditorControl.TextChanged += new EventHandler(EditorControl_TextChanged);
            sourceControl.EditorControl.GotFocus += new EventHandler(EditorControl_GotFocus);

            // Use CodeMirror 5 by default, fallback to TextBox with OLW_USE_TEXTBOX_SOURCE=1
            _useCodeMirror = Environment.GetEnvironmentVariable("OLW_USE_TEXTBOX_SOURCE") != "1";
            Debug.WriteLine($"[OLW-DEBUG] Source editor: {(_useCodeMirror ? "CodeMirror 5 (WebView2)" : "TextBox")}");

            if (_useCodeMirror)
            {
                webView2SourceEditor = new WebView2SourceEditorControl();
                webView2SourceEditor.Dock = DockStyle.Fill;
                webView2SourceEditor.TextChanged += new EventHandler(EditorControl_TextChanged);
                webView2SourceEditor.GotFocus += new EventHandler(EditorControl_GotFocus);
                panelSourceEditor.Controls.Add(webView2SourceEditor);
            }
            else
            {
                BorderControl borderControl = new BorderControl();
                borderControl.SuppressBottomBorder = true;
                borderControl.Control = sourceControl.EditorControl;
                borderControl.Dock = DockStyle.Fill;
                panelSourceEditor.Controls.Add(borderControl);
            }

            ColorizedResources.Instance.RegisterControlForBackColorUpdates(this);

            textBoxTitle.AccessibleName = Res.Get(StringId.PostEditorTitleRegion);
            sourceControl.EditorControl.AccessibleName = Res.Get(StringId.PostEditorBodyRegion);
        }

        public SmartContentEditor CurrentEditor
        {
            get
            {
                return null;
            }
        }

        void EditorControl_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (sourceControl != null)
                    sourceControl.Dispose();
                if (webView2SourceEditor != null)
                    webView2SourceEditor.Dispose();
            }
            base.Dispose(disposing);
        }

        public void ChangeSelection(SelectionPosition position)
        {
            sourceControl.ChangeSelection(position);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
                BackColor = ColorizedResources.Instance.WorkspaceBackgroundColor;
            base.OnVisibleChanged(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //set the default focus for the editor
            TimerHelper.CallbackOnDelay(new InvokeInUIThreadDelegate(sourceControl.Focus), 50);
        }

        private void InitializeComponent()
        {
            this.textBoxTitle = new TextBox();
            this.panelSourceEditor = new Panel();
            this.SuspendLayout();
            //
            // textBoxTitle
            //
            this.textBoxTitle.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.textBoxTitle.Location = new Point(2, 0);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new Size(326, 20);
            this.textBoxTitle.TabIndex = 0;
            this.textBoxTitle.Text = "post title";
            this.textBoxTitle.TextChanged += new EventHandler(this.textBoxTitle_TextChanged);
            this.textBoxTitle.GotFocus += new EventHandler(this.textBoxTitle_TitleGotFocus);
            //
            // panelSourceEditor
            //
            this.panelSourceEditor.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.panelSourceEditor.Location = new Point(2, 24);
            this.panelSourceEditor.Name = "panelSourceEditor";
            this.panelSourceEditor.Size = new Size(326, 272);
            this.panelSourceEditor.TabIndex = 1;

            //
            // BlogPostHtmlSourceEditorControl
            //
            this.BackColor = SystemColors.Control;
            this.Controls.Add(this.panelSourceEditor);
            this.Controls.Add(this.textBoxTitle);
            this.Name = "BlogPostHtmlSourceEditorControl";
            this.Size = new Size(332, 296);
            this.ResumeLayout(false);

        }

        #region IBlogPostHtmlEditor
        public event EventHandler TitleChanged;

        protected void OnTitleChanged()
        {
            titleIsDirty = true;

            if (TitleChanged != null)
                TitleChanged(this, EventArgs.Empty);
        }

        public event EventHandler EditableRegionFocusChanged;

        public void UpdateEditingContext()
        {
            sourceControl.SpellingChecker.StopChecking();
            sourceControl.SpellingChecker.StartChecking();
        }

        private void textBoxTitle_TitleGotFocus(object sender, EventArgs e)
        {
            OnEditableRegionFocusChanged(new EditableRegionFocusChangedEventArgs(false));
            sourceControl.InBody = false;
            //sourceControl.CommandSource.CanInsertLink = false;
        }

        private void EditorControl_GotFocus(object sender, EventArgs e)
        {
            OnEditableRegionFocusChanged(new EditableRegionFocusChangedEventArgs(true));
        }

        protected virtual void OnEditableRegionFocusChanged(EventArgs e)
        {
            if (EditableRegionFocusChanged != null)
                EditableRegionFocusChanged(this, e);
        }

        public void LoadHtmlFragment(string title, string postBodyHtml, string baseUrl, BlogEditingTemplate editingTemplate)
        {
            // Debug.WriteLine($"[OLW-DEBUG] SourceEditor.LoadHtmlFragment - title: '{title}', ContainsTitle: {editingTemplate.ContainsTitle}");
            const int SPACE_BETWEEN_TITLE_AND_BODY = 4;
            if (editingTemplate.ContainsTitle)
            {
                // Make sure the title textbox is showing if there is a title.
                textBoxTitle.Visible = true;
                panelSourceEditor.Top = textBoxTitle.Bottom + SPACE_BETWEEN_TITLE_AND_BODY;
                panelSourceEditor.Height = Height - panelSourceEditor.Top;
                textBoxTitle.Text = title;
                // Debug.WriteLine($"[OLW-DEBUG] SourceEditor - title textbox set to: '{title}'");
            }
            else
            {
                // We need to hide the title textbox if there is no title.
                textBoxTitle.Visible = false;
                panelSourceEditor.Top = textBoxTitle.Top;
                panelSourceEditor.Height = Height - panelSourceEditor.Top;
                // Debug.WriteLine($"[OLW-DEBUG] SourceEditor - title textbox HIDDEN (ContainsTitle=false)");
            }

            //make the post body HTML look pretty
            postBodyHtml = ApplyPostBodyFormatting(postBodyHtml);

            // Always load into sourceControl for its functionality
            sourceControl.LoadHtmlFragment(postBodyHtml);
            
            // Debug.WriteLine($"[OLW-DEBUG] SourceEditor.LoadHtmlFragment - _useCodeMirror={_useCodeMirror}, webView2SourceEditor null={webView2SourceEditor == null}");
            if (_useCodeMirror)
            {
                // Also load into CodeMirror for display
                // Debug.WriteLine($"[OLW-DEBUG] SourceEditor: Calling SetContent with {postBodyHtml?.Length ?? 0} chars of body HTML");
                webView2SourceEditor.SetContent(postBodyHtml);
                webView2SourceEditor.Focus();
            }
            else
            {
                sourceControl.Focus();
            }
        }

        public string GetEditedTitleHtml()
        {
            return textBoxTitle.Text;
        }

        public string GetEditedHtml(bool preferWellFormed)
        {
            bool formatHTML = PostEditorPreferences.Instance.FormatHTML;
            if (_useCodeMirror)
            {
                return webView2SourceEditor.GetContent();
            }
            return sourceControl.GetEditedHtml(formatHTML);
        }

        public string GetEditedHtmlFast()
        {
            if (_useCodeMirror)
            {
                return webView2SourceEditor.GetContent();
            }
            return sourceControl.GetEditedHtmlFast();
        }

        IFocusableControl IBlogPostHtmlEditor.FocusControl
        {
            get { return new FocusableControl(this); }
        }

        void IBlogPostHtmlEditor.Focus()
        {
            if (_useCodeMirror)
            {
                webView2SourceEditor.Focus();
            }
            else
            {
                sourceControl.Focus();
            }
        }

        public void FocusTitle()
        {
            textBoxTitle.Focus();
        }

        public void FocusBody()
        {
            if (_useCodeMirror)
            {
                webView2SourceEditor.Focus();
            }
            else
            {
                sourceControl.Focus();
            }
        }

        public bool DocumentHasFocus()
        {
            if (_useCodeMirror)
            {
                return webView2SourceEditor.Focused || webView2SourceEditor.ContainsFocus;
            }
            return sourceControl.HasFocus;
        }

        public Control EditorControl
        {
            get { return this; }
        }

        public void LoadHtmlFile(string filePath)
        {
            sourceControl.LoadHtmlFile(filePath);
        }

        public string SelectedText
        {
            get
            {
                if (ActiveControl == this.textBoxTitle)
                    return textBoxTitle.Text;
                else
                    return sourceControl.SelectedText;
            }
        }

        public string SelectedHtml
        {
            get
            {
                if (ActiveControl == this.textBoxTitle)
                    return textBoxTitle.Text;
                else
                    return sourceControl.SelectedHtml;
            }
        }

        public void EmptySelection()
        {
            if (ActiveControl == this.textBoxTitle)
                textBoxTitle.SelectionLength = 0;
            else
                sourceControl.EmptySelection();
        }

        public void InsertHtml(string content, bool moveSelectionRight)
        {
            sourceControl.InsertHtml(content, moveSelectionRight);
        }

        public void InsertHtml(string content, HtmlInsertionOptions options)
        {
            sourceControl.InsertHtml(content, options);
        }

        public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            sourceControl.InsertLink(url, linkText, linkTitle, rel, newWindow);
        }

        public void InsertHorizontalLine(bool plainText)
        {
            sourceControl.InsertHtml(plainText ? "<br />" + BlogPost.PlainTextHorizontalLine : BlogPost.HorizontalLine, true);
        }

        public void InsertClearBreak()
        {
            sourceControl.InsertHtml(BlogPost.ClearBreak, true);
        }

        public void InsertExtendedEntryBreak()
        {
            if (sourceControl.GetRawText().IndexOf(BlogPost.ExtendedEntryBreak) == -1)
            {
                sourceControl.InsertHtml(BlogPost.ExtendedEntryBreak, true);
            }
        }

        public bool IsDirty
        {
            get { return sourceControl.IsDirty || titleIsDirty; }
            set
            {
                sourceControl.IsDirty = value;
                titleIsDirty = value;
            }
        }
        bool titleIsDirty;
        public event EventHandler IsDirtyEvent
        {
            add
            {
                sourceControl.IsDirtyEvent += value;
            }
            remove
            {
                sourceControl.IsDirtyEvent -= value;
            }
        }

        public bool SuspendAutoSave
        {
            get { return sourceControl.SuspendAutoSave; }
        }

        public IHtmlEditorCommandSource CommandSource
        {
            get { return sourceControl.CommandSource; }
        }

        public bool FullyEditableRegionActive
        {
            get { return sourceControl.FullyEditableRegionActive; }
            set { sourceControl.FullyEditableRegionActive = value; }
        }

        #endregion
        #region Event handlers
        private void textBoxTitle_TextChanged(object sender, EventArgs e)
        {
            OnTitleChanged();
        }
        #endregion

        #region Private Helpers
        private static string ApplyPostBodyFormatting(string postBodyHtml)
        {
            //If the extended entry break exists, put some line breaks around it so that it
            //sits on its own line.
            if (postBodyHtml.IndexOf(BlogPost.ExtendedEntryBreak) != -1)
            {
                string moreExtendedEntryBreakWS = String.Format(CultureInfo.InvariantCulture, "\r\n{0}\r\n", BlogPost.ExtendedEntryBreak);
                if (postBodyHtml.IndexOf(moreExtendedEntryBreakWS) == -1)
                    postBodyHtml = postBodyHtml.Replace(BlogPost.ExtendedEntryBreak, moreExtendedEntryBreakWS);
            }
            return postBodyHtml;
        }
        #endregion

        public bool SelectAndDelete(string startMaker, string endMarker)
        {
            return sourceControl.SelectAndDelete(startMaker, endMarker);
        }

    }

    /// <summary>
    /// Summary description for BlogPostHtmlSourceEditorControl.
    /// </summary>
    public class HtmlSourceEditorControl : HtmlEditor.HtmlSourceEditorControl
    {
        private ReplaceAbsoluteFilePathsOperation _replaceOperation = new ReplaceAbsoluteFilePathsOperation();
        private IBlogPostImageEditingContext editingContext;

        public HtmlSourceEditorControl(ISpellingChecker spellingChecker, CommandManager commandManager, IBlogPostImageEditingContext editingContext)
            : base(spellingChecker, commandManager)
        {
            this.editingContext = editingContext;
        }

        internal string GetRawText()
        {
            return EditorControl.Text;
        }

        public void LoadHtmlFragment(string postBodyHtml)
        {
            // save current selection state so we can restore it
            int editorPosition = SourceEditor.SelectionStart;

            using (TextReader htmlReader = new StringReader(CleanupHtml(postBodyHtml, SourceEditor.Width - 5)))
            {
                String htmlText = htmlReader.ReadToEnd();

                //shorten all absolute file URLs into variable names so that they don't clutter the source code.
                //the replace operation can be used later to undo the conversions.
                _replaceOperation.Mode = ReplaceAbsoluteFilePathsOperation.REPLACE_MODE.ABS2VAR;
                htmlText = _replaceOperation.Execute(htmlText);

                SourceEditor.Text = htmlText;
            }

            SourceEditor.SelectionStart = editorPosition;

            OnCommandStateChanged();
        }

        public void Focus()
        {
            EditorControl.Focus();
        }

        public override string GetEditedHtml(bool preferWellFormed)
        {
            //get the raw HTML out of the source control's text box
            bool formatHTML = PostEditorPreferences.Instance.FormatHTML;
            String htmlText = base.GetEditedHtml(formatHTML);

            //undo any URL variable-name conversions that were previously applied by the replace operation.
            _replaceOperation.Mode = ReplaceAbsoluteFilePathsOperation.REPLACE_MODE.VAR2ABS;
            htmlText = _replaceOperation.Execute(htmlText);

            return htmlText;
        }

        /// <summary>
        /// If the specified markers exist in the editor text,
        /// delete the first instance of each and position the
        /// selection at the markers.
        /// </summary>
        public bool SelectAndDelete(string startMarker, string endMarker)
        {
            string html = SourceEditor.Text;

            int startIndex, startLen;
            FindMarker(html, startMarker, out startIndex, out startLen);

            int endIndex, endLen;
            FindMarker(html, endMarker, out endIndex, out endLen);

            if (startIndex > endIndex)
            {
                Trace.Fail("Selection startIndex is before endIndex--this should never happen");
                int temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
                temp = startLen;
                startLen = endLen;
                endLen = temp;
            }

            if (endIndex >= 0)
            {
                html = html.Substring(0, endIndex) + html.Substring(endIndex + endLen);
            }

            if (startIndex >= 0)
            {
                html = html.Substring(0, startIndex) + html.Substring(startIndex + startLen);
                if (endIndex >= 0)
                    endIndex -= startLen;
            }

            SourceEditor.Text = html;

            if (startIndex >= 0)
            {
                SourceEditor.Select(startIndex, endIndex - startIndex);
                SourceEditor.ScrollToCaret();
                return true;
            }
            return false;
        }

        private static void FindMarker(string html, string marker, out int index, out int length)
        {
            Match m = Regex.Match(html, Regex.Escape("<!--") + @"\s*" + Regex.Escape(marker) + @"\s*" + Regex.Escape("-->"));
            if (m.Success)
            {
                index = m.Index;
                length = m.Length;
            }
            else
            {
                index = -1;
                length = 0;
            }
        }

        public void ChangeSelection(SelectionPosition position)
        {
            SourceEditor.Select(
                position == SelectionPosition.BodyStart ? 0
                : position == SelectionPosition.BodyEnd ? SourceEditor.TextLength
                : 0,
                0);
        }

        protected override bool ShowAllLinkOptions
        {
            get
            {
                return GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShowAllLinkOptions);
            }
        }
    }

    /// <summary>
    /// Converts URLs to/from absolute file and shortened variable name formats.
    /// </summary>
    public class ReplaceAbsoluteFilePathsOperation : ReplaceOperation
    {
        public enum REPLACE_MODE { ABS2VAR, VAR2ABS };
        private readonly Hashtable var2abs;
        private readonly Hashtable abs2var;
        public REPLACE_MODE Mode;

        public ReplaceAbsoluteFilePathsOperation()
        {
            var2abs = new Hashtable();
            abs2var = new Hashtable();
        }

        protected override string Replace(Element el)
        {
            if (el is BeginTag)
            {
                BeginTag beginTag = (BeginTag)el;
                if (beginTag.NameEquals("a"))
                {
                    Attr href = beginTag.GetAttribute("href");
                    if (href != null && href.Value != null)
                    {
                        href.Value = ConvertUrl(href.Value);
                        return beginTag.ToString();
                    }
                }
                else if (beginTag.NameEquals("img"))
                {
                    Attr src = beginTag.GetAttribute("src");
                    if (src != null && src.Value != null)
                    {
                        src.Value = ConvertUrl(src.Value);
                        return beginTag.ToString();
                    }
                }
            }
            return base.Replace(el);
        }

        /// <summary>
        /// Converts a URL's representation between absolute and variable formats based on the Mode.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ConvertUrl(String url)
        {
            if (Mode == REPLACE_MODE.ABS2VAR)
            {
                if (UrlHelper.IsFileUrl(url))
                {
                    //then this URL is an absolute file URL, so convert it to a shortened variable name.
                    string varName = abs2var[url] as string;
                    if (varName == null)
                    {
                        varName = createUniqueVarNameForFileUrl(url);
                    }
                    return varName;
                }
            }
            else if (Mode == REPLACE_MODE.VAR2ABS)
            {
                if (url.StartsWith("$"))
                {
                    //this URL is a variable that was previously converted from ab ABS url, so convert it back
                    string newUrl = var2abs[url] as string;
                    if (newUrl != null)
                        return newUrl;
                }
            }
            return url;
        }

        /// <summary>
        /// Generates a unique variable name that can be used to convert this URL in a shortened format.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string createUniqueVarNameForFileUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return url;

            string localPath = uri.LocalPath;
            string fileName = Path.GetFileNameWithoutExtension(localPath);
            string ext = Path.GetExtension(localPath);
            string varName = String.Format(CultureInfo.InvariantCulture, "${0}{1}", fileName, ext);
            for (int i = 0; var2abs.ContainsKey(varName); i++)
            {
                varName = String.Format(CultureInfo.InvariantCulture, "${0}-{1}{2}", fileName, i, ext);
            }

            var2abs[varName] = url;
            abs2var[url] = varName;
            return varName;
        }
    }
}
