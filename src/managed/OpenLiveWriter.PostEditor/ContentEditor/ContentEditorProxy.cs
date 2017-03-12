//// Copyright (c) .NET Foundation. All rights reserved.
///Licensed under the MIT license. See LICENSE file in the project root for details.
//
//#define DEBUG_PHOTOMAIL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Mail;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.Mshtml.Mshtml_Interop;
using OpenLiveWriter.PostEditor.Autoreplace;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using IDropTarget = OpenLiveWriter.Interop.Com.IDropTarget;

namespace OpenLiveWriter.PostEditor
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("FC51FC7A-9BB1-4472-86E4-34911D298922")]
    [ComVisible(true)]
    public class ContentEditorFactory : IContentEditorFactory
    {
        #region IContentEditorFactory Members

        private RedirectionLogger _logger;

        public void Initialize(string registrySettingsPath, IContentEditorLogger logger, IContentTarget contentTarget, ISettingsProvider settingsProvider)
        {
            try
            {
                GlobalEditorOptions.Init(contentTarget, settingsProvider);
                HtmlEditorControl.AllowCachedEditor();

                Assembly assembly = Assembly.GetExecutingAssembly();
                ApplicationEnvironment.Initialize(assembly, Path.GetDirectoryName(assembly.Location), registrySettingsPath, contentTarget.ProductName);
                ContentSourceManager.Initialize(false);

                Trace.Listeners.Clear();
                if (logger != null)
                {
                    _logger = new RedirectionLogger(logger);

                    Trace.Listeners.Add(_logger);
                }

#if DEBUG
                Trace.Listeners.Add(new DefaultTraceListener());
#endif
            }
            catch (Exception e)
            {
                Trace.Fail("Failed to initialize Shared Canvas: " + e);
                Trace.Flush();
                throw;
            }
        }

        public void Shutdown()
        {
            HtmlEditorControl.DisposeCachedEditor();
            TempFileManager.Instance.Dispose();
            Trace.Listeners.Clear();
            if (_logger != null)
                _logger.Dispose();
        }

        public IContentEditor CreateEditor(IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, string wysiwygHtml, int dlControlFlags)
        {
            return new ContentEditorProxy(this, contentEditorSite, internetSecurityManager, wysiwygHtml, null, dlControlFlags);
        }

        public IContentEditor CreateEditorFromDraft(IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, string wysiwygHtml, string pathToDraftFile, int dlControlFlags)
        {
            return new ContentEditorProxy(this, contentEditorSite, internetSecurityManager, wysiwygHtml, null, pathToDraftFile, dlControlFlags);
        }

        public IContentEditor CreateEditorFromHtmlDocument(IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, IHTMLDocument2 htmlDocument, HtmlInsertOptions options, int dlControlFlags)
        {
            return new ContentEditorProxy(this, contentEditorSite, internetSecurityManager, htmlDocument, options, dlControlFlags, null, null);
        }

        public IContentEditor CreateEditorFromMoniker(IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, IMoniker moniker, uint codepage, HtmlInsertOptions options, string color, int dlControlFlags, string wpost)
        {
            codepage = EmailShim.GetCodepage(codepage);
            string name;
            string html = HTMLDocumentHelper.MonikerToString(moniker, codepage, out name);

            if (CultureHelper.IsRtlCodepage(codepage))
            {
                EmailContentTarget target =
                    GlobalEditorOptions.ContentTarget as EmailContentTarget;
                if (target != null)
                {
                    target.EnableRtlMode();
                }
            }

            if (string.IsNullOrEmpty(html))
                html = "<html><body></body></html>";

            html = EmailShim.GetContentHtml(name, html);

            // Create a IHtmlDocument2 from the html which will then be loaded into the editor
            IHTMLDocument2 htmlDocument;
            htmlDocument = HTMLDocumentHelper.StringToHTMLDoc(html, name);

            return new ContentEditorProxy(this, contentEditorSite, internetSecurityManager, htmlDocument, options, dlControlFlags, color, wpost);
        }

        #endregion
        
        #region IContentEditorFactory Members

        public void DoPreloadWork()
        {
            ContentEditorProxy.ApplyInstalledCulture();
            SimpleHtmlParser.Create();
            BlogClientHelper.FormatUrl("", "", "", "");
            ContentEditor contentEditor = new ContentEditor(null, new Panel(), null, new BlogPostHtmlEditorControl.BlogPostHtmlEditorSecurityManager(), new ContentEditorProxy.ContentEditorTemplateStrategy(), MshtmlOptions.DEFAULT_DLCTL);
            contentEditor.Dispose();
        }

        #endregion
    }

    public class RedirectionLogger : TraceListener
    {
        public enum ContentEditorLoggingLevel
        {
            Log_Error = 0,
            Log_Terse = 1,
            Log_Verbose = 2,
            Log_Blab = 4,
            Log_Always = 6
        };

        private IContentEditorLogger _logger;
        public RedirectionLogger(IContentEditorLogger logger)
        {
            _logger = logger;
        }

        public override void Write(string message)
        {
            WriteLine(message);
        }

        public override void WriteLine(string message)
        {
            WriteLine(message, null);
        }

        public override void WriteLine(string message, string category)
        {
            try
            {
                if (category == ErrText.FailText)
                {
                    _logger.WriteLine(message, (int)ContentEditorLoggingLevel.Log_Error);
                }
                else
                {
                    _logger.WriteLine(message, (int)ContentEditorLoggingLevel.Log_Blab);
                }
            }
            catch (Exception)
            {
                // TODO: Explore our options here.
                // IContentEditorLogger should not be throwing exceptions, but in the case that it does we do not want
                // to make another Debug or Trace call because it could cause an infinite loop/stack overflow.
            }
        }

        public override void Write(string message, string category)
        {
            WriteLine(message, category);
        }
    }

    public class ContentEditorProxy : IContentEditor
    {
        private ContentEditor contentEditor;
        private MainFrameWindowAdapter mainFrame;
        private IBlogPostEditingContext context;
        private Panel panel;
        private ContentEditorAccountAdapter accountAdapter;
        private IContentEditorSite _contentEditorSite;

        public ContentEditorProxy(ContentEditorFactory factory, IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, string wysiwygHTML, string previewHTML, int dlControlFlags)
        {
            ContentEditorProxyCore(factory, contentEditorSite, internetSecurityManager, wysiwygHTML, previewHTML, new BlogPostEditingContext(ContentEditorAccountAdapter.AccountId, new BlogPost()), new ContentEditorTemplateStrategy(), dlControlFlags, null);
        }

        public ContentEditorProxy(ContentEditorFactory factory, IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, string wysiwygHTML, string previewHTML, string pathToFile, int dlControlFlags)
        {
            ContentEditorProxyCore(factory, contentEditorSite, internetSecurityManager, wysiwygHTML, previewHTML, PostEditorFile.GetExisting(new FileInfo(pathToFile)).Load(false), new ContentEditorTemplateStrategy(), dlControlFlags, null);
        }

        public ContentEditorProxy(ContentEditorFactory factory, IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, IHTMLDocument2 htmlDocument, HtmlInsertOptions options, int dlControlFlags, string color, string wpost)
        {
            string content = htmlDocument.body.innerHTML;
            htmlDocument.body.innerHTML = "{post-body}";
            string wysiwygHTML = HTMLDocumentHelper.HTMLDocToString(htmlDocument);
            BlogPost documentToBeLoaded = null;
            IBlogPostEditingContext editingContext = null;

            if (string.IsNullOrEmpty(wpost) || !File.Exists(wpost))
            {
                documentToBeLoaded = new BlogPost();
                editingContext = new BlogPostEditingContext(ContentEditorAccountAdapter.AccountId,
                                                                               documentToBeLoaded);
            }
            else
            {
                PostEditorFile wpostxFile = PostEditorFile.GetExisting(new FileInfo(wpost));
                editingContext = wpostxFile.Load(false);
                editingContext.BlogPost.Contents = "";
            }

            if (!string.IsNullOrEmpty(content))
                delayedInsertOperations.Enqueue(new DelayedInsert(content, options));

            ContentEditorProxyCore(factory, contentEditorSite, internetSecurityManager, wysiwygHTML, null, editingContext, new ContentEditorTemplateStrategy(), dlControlFlags, color);

        }

        private class DelayedInsert
        {
            public readonly string Content;
            public readonly HtmlInsertOptions Options;

            public DelayedInsert(string content, HtmlInsertOptions options)
            {
                Content = content;
                Options = options;
            }
        }

        private Queue<DelayedInsert> delayedInsertOperations = new Queue<DelayedInsert>();

        /// <summary>
        /// Initializes the IContentEditor.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="contentEditorSite"></param>
        /// <param name="internetSecurityManager"></param>
        /// <param name="wysiwygHTML"></param>
        /// <param name="previewHTML"></param>
        /// <param name="newEditingContext"></param>
        /// <param name="templateStrategy"></param>
        /// <param name="dlControlFlags">
        /// For Mail, these flags should always include DLCTL_DLIMAGES | DLCTL_VIDEOS | DLCTL_BGSOUNDS so that local
        /// images, videos and sounds are loaded. To block external content, it should also include
        /// DLCTL_PRAGMA_NO_CACHE | DLCTL_FORCEOFFLINE | DLCTL_NO_CLIENTPULL so that external images are not loaded
        /// and are displayed as a red X instead.
        /// </param>
        /// <param name="color"></param>
        private void ContentEditorProxyCore(ContentEditorFactory factory, IContentEditorSite contentEditorSite, IInternetSecurityManager internetSecurityManager, string wysiwygHTML, string previewHTML, IBlogPostEditingContext newEditingContext, BlogPostHtmlEditorControl.TemplateStrategy templateStrategy, int dlControlFlags, string color)
        {
            try
            {
                Debug.Assert(contentEditorSite is IUIFramework, "IContentEditorSite must also implement IUIFramework");
                Debug.Assert(contentEditorSite is IDropTarget, "IContentEditorSite must also implement IDropTarget");

                ApplyInstalledCulture();

                this.factory = factory;

                _wysiwygHTML = wysiwygHTML;
                _previewHTML = previewHTML;
                _contentEditorSite = contentEditorSite;

                IntPtr p = _contentEditorSite.GetWindowHandle();
                WINDOWINFO info = new WINDOWINFO();
                User32.GetWindowInfo(p, ref info);
                panel = new Panel();
                panel.Top = 0;
                panel.Left = 0;
                panel.Width = Math.Max(info.rcWindow.Width, 200);
                panel.Height = Math.Max(info.rcWindow.Height, 200);
                panel.CreateControl();
                User32.SetParent(panel.Handle, p);

                accountAdapter = new ContentEditorAccountAdapter();
                mainFrame = new MainFrameWindowAdapter(p, panel, _contentEditorSite, accountAdapter.Id);
                context = newEditingContext;
                contentEditor = new ContentEditor(mainFrame, panel, mainFrame, internetSecurityManager, templateStrategy, dlControlFlags);

                // Prevents asserts
                contentEditor.DisableSpelling();

                contentEditor.OnEditorAccountChanged(accountAdapter);
                contentEditor.DocumentComplete += new EventHandler(blogPostHtmlEditor_DocumentComplete);
                contentEditor.GotFocus += new EventHandler(contentEditor_GotFocus);
                contentEditor.LostFocus += new EventHandler(contentEditor_LostFocus);
                contentEditor.Initialize(context, accountAdapter, wysiwygHTML, previewHTML, false);

                if (!string.IsNullOrEmpty(color))
                {
                    contentEditor.IndentColor = color;
                }
            }
            catch (Exception ex)
            {
                // Something went wrong, make sure we don't reuse a cached editor
                HtmlEditorControl.DisposeCachedEditor();
                Trace.Fail(ex.ToString());
                Trace.Flush();
                throw;
            }
        }

        private bool _inGotFocusHandler = false;
        void contentEditor_GotFocus(object sender, EventArgs e)
        {
            _inGotFocusHandler = true;
            _contentEditorSite.OnGotFocus();
            _inGotFocusHandler = false;
        }

        void contentEditor_LostFocus(object sender, EventArgs e)
        {
            _contentEditorSite.OnLostFocus();
        }

        public static void ApplyInstalledCulture()
        {
            CultureHelper.ApplyUICulture(GlobalEditorOptions.GetSetting<string>(ContentEditorSetting.Language));
        }

        private bool _documentComplete = false;

        void blogPostHtmlEditor_DocumentComplete(object sender, EventArgs e)
        {
            _documentComplete = true;

            if (_editMode != null && _editMode.HasValue)
            {
                EditingMode mode = _editMode.Value;
                _editMode = null;
                ChangeView(mode);
                return;
            }

            while (delayedInsertOperations.Count > 0)
            {
                DelayedInsert insert = delayedInsertOperations.Dequeue();
                InsertHtml(insert.Content, insert.Options);
            }

            _contentEditorSite.OnDocumentComplete();
        }

        #region IContentEditor Members

        public void Save(string fileName, bool preserveDirty)
        {
            contentEditor.SaveChanges(context.BlogPost, BlogPostSaveOptions.DefaultOptions);
            context.LocalFile.SaveContentEditorFile(context, fileName, false);
        }

        public string Publish(IPublishOperation imageConverter)
        {
            return contentEditor.Publish(imageConverter);
        }

        public IHTMLDocument2 GetPublishDocument()
        {
            string body = contentEditor.Publish(null);

            // Before we drop the body into the template, we wrap the whole thing in the user's default font
            // This will help cover for any blocks of text that while editing had their font set by body style.  We
            // cannot send the body style in emails because it will get stripped by some email providers.
            body = contentEditor.CurrentDefaultFont.ApplyFontToBody(body);

            // We also need to wrap the email in a default direction because we support LTR/RTL per paragraph but
            // we inherit the default direction from the body.
            // NOTE: Now that we set the direction of the body (a few lines below) this may not be needed.  It is
            // currently kept to avoid possible regressions with external mail providers
            string dir = contentEditor.IsRTLTemplate ? "rtl" : "ltr";
            body = string.Format(CultureInfo.InvariantCulture, "<div dir=\"{0}\">{1}</div>", dir, body);

            // This forms the whole html document by combining the theme and the body and then turning it into an IHTMLDocument2
            // This is needed for WLM so they can reuse packaging code.
            // We wrap the html document with a class that improves the representation of smart content for an email's plain text MIME part.
            // In order to minimize the potential for regressions we only wrap in the case of a photomail.
            IHTMLDocument2 publishDocument = HTMLDocumentHelper.StringToHTMLDoc(_wysiwygHTML.Replace("{post-body}", body), "");

            // WinLive 262662: although many features work by wrapping the email in a direction div, the
            // email as a whole is determined by the direction defined in the body
            publishDocument.body.setAttribute("dir", dir, 1);

            return publishDocument;
        }

        public void SetSize(int width, int height)
        {
            panel.Size = new Size(width, height);
        }

        private ContentEditorFactory factory;
        private string _wysiwygHTML;
        private string _previewHTML;
        public void SetTheme(string wysiwygHTML)
        {
            // We need to track the wysiwygHTML and previewHTML so that we can use it in the Load() function
            // where we have to call Initialize again which requires the theme to be passed in
            _wysiwygHTML = wysiwygHTML;
            _previewHTML = null;
            contentEditor.SetTheme(_wysiwygHTML, null, false);
        }

        public void SetSpellingOptions(string bcp47Code, uint sobitOptions, bool useAutoCorrect)
        {
            if (CultureHelper.IsRtlCulture(bcp47Code))
            {
                EmailContentTarget target =
                    GlobalEditorOptions.ContentTarget as EmailContentTarget;
                if (target != null)
                {
                    target.EnableRtlMode();
                }
            }
        }

        public void DisableSpelling()
        {
            contentEditor.DisableSpelling();
        }

        public void AutoreplaceEmoticons(bool enabled)
        {
            AutoreplaceSettings.EnableEmoticonsReplacement = enabled;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            contentEditor.DocumentComplete -= new EventHandler(blogPostHtmlEditor_DocumentComplete);
            contentEditor.GotFocus -= new EventHandler(contentEditor_GotFocus);
            contentEditor.LostFocus -= new EventHandler(contentEditor_LostFocus);
            contentEditor.Dispose();

            panel.Dispose();
            accountAdapter.Dispose();
            mainFrame.Dispose();

            Marshal.ReleaseComObject(_contentEditorSite);
            _contentEditorSite = null;
            accountAdapter = null;
            contentEditor = null;
            panel = null;
            context = null;
        }

        private EditingMode? _editMode;
        public void ChangeView(EditingMode editingMode)
        {
            if (!_documentComplete)
            {
                _editMode = editingMode;
                return;
            }

            try
            {
                switch (editingMode)
                {
                    case EditingMode.Wysiwyg:
                        contentEditor.ChangeToWysiwygMode();
                        return;
                    case EditingMode.Source:
                        contentEditor.ChangeToCodeMode();
                        return;
                    case EditingMode.Preview:
                        contentEditor.ChangeToPreviewMode();
                        return;
                    case EditingMode.PlainText:
                        contentEditor.ChangeToPlainTextMode();
                        return;

                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                throw;
            }

            Debug.Fail("Unknown value for editingView: " + editingMode.ToString() + "\r\nAccepted values Wysiwyg, Source, Preview, PlainText");

        }

        public void SetFocus()
        {
            if (!_inGotFocusHandler && !contentEditor.DocumentHasFocus())
                contentEditor.FocusBody();
        }

        public void InsertHtml(string html, HtmlInsertOptions options)
        {
            contentEditor.InsertHtml(html, (HtmlInsertionOptions)options | HtmlInsertionOptions.ExternalContent);
        }

        public void ChangeSelection(SelectionPosition position)
        {
            contentEditor.ChangeSelection(position);
        }

        #endregion

        #region IUICommandHandler Members

        public int Execute(uint commandId, CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            return contentEditor.CommandManager.Execute(commandId, verb, key, currentValue, commandExecutionProperties);
        }

        public int UpdateProperty(uint commandId, ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            return contentEditor.CommandManager.UpdateProperty(commandId, ref key, currentValue, out newValue);
        }

        #endregion

        #region Implementation of IUICommandHandlerOverride

        public int OverrideProperty(uint commandId, ref PropertyKey key, PropVariantRef overrideValue)
        {
            return contentEditor.CommandManager.OverrideProperty(commandId, ref key, overrideValue);
        }

        public int CancelOverride(uint commandId, ref PropertyKey key)
        {
            return contentEditor.CommandManager.CancelOverride(commandId, ref key);
        }

        #endregion

        internal class ContentEditorTemplateStrategy : BlogPostHtmlEditorControl.TemplateStrategy
        {

            public override string OnBodyInserted(string bodyContents)
            {
                return bodyContents;
            }

            public override string OnTitleInserted(string title)
            {
                return null;
            }

            public override void OnDocumentComplete(IHTMLDocument2 doc)
            {
                doc.body.style.overflow = "auto";
                doc.body.id = BODY_FRAGMENT_ID;
                ((IHTMLElement3)doc.body).contentEditable = "true";
                doc.body.style.width = "100%";
                if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.RTLDirectionDefault))
                    doc.body.setAttribute("dir", "rtl", 0);
                else
                    doc.body.setAttribute("dir", "ltr", 0);

                IHTMLElement2 body = (IHTMLElement2)doc.body;
                body.runtimeStyle.padding = "0px 0px 0px 0px";
                body.runtimeStyle.borderWidth = "0px";
            }

            public override IHTMLElement PostBodyElement(IHTMLDocument2 doc)
            {
                return doc.body;
            }

            public override IHTMLElement TitleElement(IHTMLDocument2 doc)
            {
                return null;
            }
        }

        #region IContentEditor Members

        public bool GetDirtyState()
        {
            return contentEditor.IsDirty;
        }

        public void SetDirtyState(bool newState)
        {
            contentEditor.IsDirty = newState;
        }

        public void SetDefaultFont(string fontSetting)
        {
            contentEditor.SetDefaultFont(fontSetting);
        }

        #endregion

    }

}
