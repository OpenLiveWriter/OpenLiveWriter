// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Marshalling;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.Mshtml.Mshtml_Interop;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.Tables;
using OpenLiveWriter.PostEditor.Tagging;
using OpenLiveWriter.SpellChecker;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.PostEditor.Emoticons;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{

    internal class BlogPostHtmlEditorControl : HtmlEditorControl, IBlogPostHtmlEditor, IHtmlEditorComponentContext
    {
        public BlogPostHtmlEditorControl(IMainFrameWindow mainFrameWindow, IStatusBar statusBar, MshtmlOptions options, IBlogPostImageEditingContext imageEditingContext, IBlogPostSidebarContext sidebarContext, IContentSourceSidebarContext sourceContext, SmartContentResizedListener resizedListener, IBlogPostSpellCheckingContext spellingContext, IImageReferenceFixer referenceFixer, IInternetSecurityManager internetSecurityManager, CommandManager commandManager, TemplateStrategy strategy, IEditingMode editingModeContext)
            : base(mainFrameWindow, statusBar, options, spellingContext.SpellingChecker, internetSecurityManager, commandManager)
        {
            _strategy = strategy;
            _imageEditingContext = imageEditingContext;
            _sidebarContext = sidebarContext;
            _sourceContext = sourceContext;
            _resizedListener = resizedListener;
            _spellingContext = spellingContext;

            _spellingManager = new SpellingManager(CommandManager);
            _keyBoardHandler = new PostEditorKeyboardHandler(this, imageEditingContext, editingModeContext);
            _referenceFixer = referenceFixer;

            InitializeTableEditingManager();

            InitializeElementBehaviors();

            SelectionChanged += BlogPostHtmlEditorControl_SelectionChanged;
            KeyPress += new HtmlEventHandler(BlogPostHtmlEditorControl_KeyPress);
        }

        public override bool CanDrop(IHTMLElement scope, DataObjectMeister dataObjectMeister)
        {
            return (!InlineEditField.IsWithinEditField(scope) || InlineEditField.EditFieldAcceptsData()) && scope.className != ContentSourceManager.SMART_CONTENT;
        }

        public override bool ShouldMoveDropLocationRight(MarkupPointer dropLocation)
        {
            MarkupContext mc = new MarkupContext();
            dropLocation.Right(false, mc);
            if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope && ElementFilters.IsBlockElement(mc.Element) && !ContentSourceManager.IsSmartContent(mc.Element))
            {
                dropLocation.Left(false, mc);
                if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope && ElementFilters.IsBlockElement(mc.Element))
                    return true;
            }
            return false;
        }

        protected override bool StopTryMoveIntoNextTable(IHTMLElement e)
        {
            return ContentSourceManager.IsSmartContent(e);
        }

        void BlogPostHtmlEditorControl_KeyPress(object o, HtmlEventArgs e)
        {
            // When the user hits Enter we need to invalidate the font group in the ribbon
            // because we could have broken out of a header tags
            if (e.htmlEvt.keyCode == VK.RETURN)
            {
                ((IHtmlEditorComponentContext)this).MainFrameWindow.BeginInvoke(new ThreadStart(() => CommandManager.Invalidate(CommandId.FontGroup)), null);
            }
        }

        public SmartContentEditor CurrentEditor
        {
            get
            {
                return _sidebarContext.CurrentEditor;
            }
        }

        public override void Dispose()
        {
            EditingContextChanged -= _spellingManager.UpdateSpellingContext;
            _spellingContext.SpellingOptionsChanged -= spellingSettings_SpellingSettingsChanged;

            if (_spellingManager != null)
                _spellingManager.Dispose();

            if (_keyBoardHandler != null)
            {
                _keyBoardHandler.Dispose();
                _keyBoardHandler = null;
            }

            if (_elementBehaviorManager != null)
                _elementBehaviorManager.Dispose();

            _referenceFixer = null;

            base.Dispose();
        }

        readonly IBlogPostImageEditingContext _imageEditingContext;
        readonly IBlogPostSidebarContext _sidebarContext;
        readonly IContentSourceSidebarContext _sourceContext;
        readonly SmartContentResizedListener _resizedListener;
        readonly IBlogPostSpellCheckingContext _spellingContext;
        private PostEditorKeyboardHandler _keyBoardHandler;
        private IImageReferenceFixer _referenceFixer;
        private TemplateStrategy _strategy;

        // initialize document editing options
        protected override void InitializeDocumentEditingOptions()
        {
            // call base
            base.InitializeDocumentEditingOptions();

            // register context menu handlers
            RegisterContextMenuHandler(new ShowContextMenuHandler(ShowContextMenu));

            //initialize the spell checker (required for form-based and realtime spellchecking)
            InitializeSpellChecking();
        }

        /// <summary>
        /// Custom security manager for the blog post editor.
        /// </summary>
        public class BlogPostHtmlEditorSecurityManager : InternetSecurityManager
        {
            public override int ProcessUrlAction(string pwszUrl, int dwAction, out byte pPolicy, int cbPolicy, IntPtr pContext, int cbContext, int dwFlags, int dwReserved)
            {
                //Allow binary behaviors even if the Internet security systems have them disabled.
                //This prevents Internet Security settings from breaking the editor's editing behaviors.
                if (dwAction == URLACTION.BEHAVIOR_RUN)
                {
                    pPolicy = URLPOLICY.ALLOW;
                    return HRESULT.S_OK;
                }
                return base.ProcessUrlAction(pwszUrl, dwAction, out pPolicy, cbPolicy, pContext, cbContext, dwFlags, dwReserved);
            }
        }

        protected override DamageCommitStrategy CreateDamageCommitStrategy()
        {
            return new SpellCheckingDamageCommitStrategy(this);
        }

        internal void SelectText(MarkupRange range)
        {
            base.SelectAll();
        }

        internal string EditingTargetName
        {
            set
            {
                if (_titleBehavior != null)
                    _titleBehavior.EditingTargetName = value;
            }
        }

        public event EventHandler TitleChanged;
        private void titleBehavior_TitleChanged(object sender, EventArgs e)
        {
            OnTitleChanged(e);
        }

        protected virtual void OnTitleChanged(EventArgs e)
        {
            if (TitleChanged != null)
                TitleChanged(this, e);
        }

        public event EventHandler EditingContextChanged;

        public void UpdateEditingContext()
        {
            if (EditingContextChanged != null)
                EditingContextChanged(_spellingContext, null);
        }

        public event EventHandler EditableRegionFocusChanged;

        private void regionBehavior_EditableRegionFocusChanged(object sender, EventArgs e)
        {
            ControlHelper.HideFocus(MshtmlEditor);
            OnEditableRegionFocusChanged(e);
        }

        protected virtual void OnEditableRegionFocusChanged(EventArgs e)
        {
            if (EditableRegionFocusChanged != null)
                EditableRegionFocusChanged(this, e);
        }

        protected override void OnEditorCreated()
        {
            base.OnEditorCreated();

            // set the document to not be editable by default (we will only allow
            // editing of the html section)
            HTMLDocument.designMode = "Off";
        }

        protected override void OnCommandStateChanged()
        {

            base.OnCommandStateChanged();

            if (HtmlEditorSettings.AggressivelyInvalidate)
                MshtmlEditor.Invalidate(true);
        }

        protected override void ResetForDocumentComplete()
        {
            base.ResetForDocumentComplete();
            StopRealTimeSpellChecking();
        }

        protected override void OnDocumentComplete(EventArgs e)
        {
            if (HTMLDocument.url == "about:blank") //the editing template has not been loaded yet.
                return;

            //verify the editor document was not loaded in the local security zone since this is either
            //much more restrictive, or much more loose (depending on whether Local Machine Lockdown mode is enabled).
            Debug.Assert(GetSecurityZone() != InternetSecurityZone.LocalMachine, "Warning!, document was loaded in the LocalMachine security zone");

            _strategy.OnDocumentComplete(HTMLDocument);

            IHTMLElement postTitleElement = null;

            if (_templateContainsTitle)
                postTitleElement = PostTitleElement;

            IHTMLElement postBodyElement = PostBodyElement;
            if (postBodyElement == null || (_templateContainsTitle && postTitleElement == null))
            {
                base.OnDocumentComplete(e);
                Trace.Fail("Document load failed: could not locate body elements");
                return;
            }

            //ensure everyone is notified that the editable region focus was changed (fixes bug 303603)
            OnEditableRegionFocusChanged(new EditableRegionFocusChangedEventArgs(false));

            if (Editable)
            {
                AttachEditingStyles(HTMLDocument);

                //put some margins around the body style so that text won't be painted right
                //against the left side and so the region borders won't get cut off
                IHTMLElement2 body = (IHTMLElement2)HTMLDocument.body;
                try
                {
                    if (body.runtimeStyle.margin == null)
                        body.runtimeStyle.margin = "10px 10px 10px 10px";
                }
                catch (NullReferenceException nre)
                {
                    //avoids bug 418173, which randomly occurs down in MSHTML
                    //if the margin isn't set, its no big deal, but exception
                    //here is a big deal, so we eat it
                    Debug.Fail("Error setting margin", nre.ToString());
                }

            }
            else
            {
                // WinLive 211551: Scrollbars do not appear in Web Preview when configured against a SharePoint 2010
                // blog because the CSS style overflow="hidden" is set on the body.
                RemoveScrollOverflowStyles(HTMLDocument.body);
            }

            base.OnDocumentComplete(e);

            if (DocumentIsReady)
            {
                MarkupRange postBodyRange = MarkupServices.CreateMarkupRange(PostBodyElement, false);
                InflateEmptyParagraphs(postBodyRange);
                FixUpStickyBrs(postBodyRange);

                if (HTMLDocumentHelper.IsQuirksMode(HTMLDocument))
                {
                    ForceTablesToInheritFontColor(postBodyRange);
                }

                // Since we move around the HTML where the intial cursor is positioned, we need
                // to make sure that it is still visible.
                if (HTMLCaret != null)
                    HTMLCaret.Show(1);
            }

            //fire the title changed event
            OnTitleChanged(e);

            // focus the fragment element
            // Work around for38082...this line is stealing focus from Mail. Commenting out for now.
            //FocusElement((IHTMLElement2)PostBodyElement, false);

            //focusing the body will cause the top of the scroll to move to the top of the
            //post body element, so force the scroll back to the top of the document.
            ScrollToTop();

            _spellingManager.ClearIgnoreOnce();
            refreshSpellCheckingSettings();

            // Work around for 38082...conditionalize this for writer so that it doesn't steal focus for mail's case.
            if (Editable && GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ResetFocusToFixIME))
            {
                // Fix bug 449174: IME doesn't work in the body of the post until focus leaves and comes back.
                ((IHtmlEditorComponentContext)this).MainFrameWindow.BeginInvoke(new ThreadStart(FixIMEBug), null);
            }

            // We may want to show the link navigation tooltip for images.
            if (LinkNavigator != null)
                LinkNavigator.SuppressForImages = !GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShowLinkTooltipForImages);
        }

        protected override bool SelectByTagId(IHTMLElement toSelect)
        {
            _ELEMENT_TAG_ID tagId = MarkupServices.GetElementTagId(toSelect);
            switch (tagId)
            {
                case _ELEMENT_TAG_ID.TAGID_DIV:
                    return SmartContentSelection.SelectIfSmartContentElement(this, toSelect) != null;
                default:
                    return base.SelectByTagId(toSelect);
            }
        }

        protected IHTMLCaret HTMLCaret
        {
            get
            {
                IHTMLCaret caret;
                ((IDisplayServices)HTMLDocument).GetCaret(out caret);
                Debug.Assert(caret != null, "HTML Document should have caret");
                return caret;
            }
        }

        protected override IHTMLElement[] EditableElements
        {
            get
            {
                if (_templateContainsTitle)
                {
                    return new IHTMLElement[]
                                       {
                                           PostBodyElement,
                                           PostTitleElement
                                       };
                }
                else
                {
                    return new IHTMLElement[]
                                       {
                                           PostBodyElement
                                       };

                }
            }
        }

        private void FixIMEBug()
        {
            // Check if document is ready before trying to refresh focus
            if (DocumentIsReady)
            {
                FocusBody();
                ScrollToTop();
            }
        }

        protected override int OnPreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            //rope-off handling mouse events on the scrollbar so behaviors and editor handlers
            //don't accidentally intercept them (fixes bugs 327222, 324688)
            switch (inEvtDispId)
            {
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN:
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP:
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE:
                case DISPID_HTMLELEMENTEVENTS2.ONCLICK:
                    IHTMLElement srcElement = pIEventObj.srcElement;
                    bool isScrollBarClick = srcElement != null && srcElement.tagName == "HTML";
                    if (isScrollBarClick)
                    {
                        //cancel the event so no one else can override the click
                        pIEventObj.cancelBubble = true;

                        if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN)
                        {
                            //The scrollbars will not respond to a drag operation unless the selection
                            //is emptied, so we empty the selection so that the scrollbar will
                            //immediately respond to the drag (fixes bug 327222)
                            HTMLDocument.selection.empty();
                        }
                    }
                    if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE)
                        break;

                    // Block some navigations in preview mode
                    if (!Editable &&
                        !pIEventObj.cancelBubble &&
                        srcElement != null)
                    {
                        IHTMLAnchorElement anchorElement = HTMLElementHelper.GetContainingAnchorElement(srcElement);

                        if (anchorElement == null)
                            break;

                        // Push an anchor that was clicked in an iframe off to a new window
                        if (MshtmlEditor.HTMLDocument.url != ((IHTMLDocument2)srcElement.document).url &&
                            (string.IsNullOrEmpty(anchorElement.target) || anchorElement.target == "_top" || anchorElement.target == "_parent"))
                        {
                            anchorElement.target = "_blank";
                        }

                        string url = ((IHTMLElement)anchorElement).getAttribute("href", 2) as string;
                        // Ignore clicks on anchor tags that don't have a valid URL in their href
                        if (string.IsNullOrEmpty(url) || !UrlHelper.IsKnownScheme(url))
                        {
                            pIEventObj.cancelBubble = true;
                            return HRESULT.S_OK;
                        }

                    }
                    break;

            }
            return base.OnPreHandleEvent(inEvtDispId, pIEventObj);
        }

        /// <summary>
        /// Returns the context associated with the first DIV encountered when moving from the selection start in the specified direction.
        /// Returns null if a non-DIV block element is encountered before a DIV when moving in that direction.
        /// </summary>
        /// <param name="left"></param>
        /// <returns></returns>
        private MarkupContext GetAdjacentDiv(bool left)
        {
            MarkupContext context = null;
            bool stillLooking = true;
            MarkupPointer pointer = Selection.SelectedMarkupRange.Start.Clone();
            while (stillLooking)
            {
                context = left ? pointer.Left(true) : pointer.Right(true);
                if (context.Element == null)
                    break;

                switch (context.Context)
                {
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text:
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None:
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope:
                        stillLooking = false;
                        break;
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope:
                        break;
                    case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope:
                        if (MarkupServices.GetElementTagId(context.Element) == _ELEMENT_TAG_ID.TAGID_DIV)
                        {
                            stillLooking = false;
                        }
                        else if (ElementFilters.IsBlockElement(context.Element))
                        {
                            return null;
                        }
                        break;
                }
            }

            return context;
        }
        protected override void HandleKeyboardNavigationKey(HtmlEventArgs e)
        {
            MarkupRange selection = Selection.SelectedMarkupRange;
            if (!selection.IsEmpty())
            {
                // WinLive 196413: MSHTML seems to have a weird issue where if you have two hyperlinked images that
                // break onto two separate lines and you select the bottom image and hit the left arrow key, then
                // you'll end up between the anchor and the image (e.g. <a><img /></a><a>[caret]<img /></a>).
                // However, if you select the top image and hit the right arrow key, then you'll end up in the right
                // place -- in between the two anchors (e.g. <a><img /></a>[caret]<a><img></a>.
                if (e.htmlEvt.keyCode == (int)Keys.Left)
                {
                    // If an image is currently selected.
                    IHTMLElement imgElement = (IHTMLElement)Selection.SelectedImage;
                    if (imgElement == null)
                        return;

                    // And it's parent is an anchor.
                    IHTMLElement imgParentElement = imgElement.parentElement;
                    if (!(imgParentElement is IHTMLAnchorElement))
                        return;

                    // And there is no other HTML besides the anchor and the image.
                    IHTMLElementCollection anchorChildren = (IHTMLElementCollection)imgParentElement.children;
                    if (anchorChildren.length > 1 || !String.IsNullOrEmpty(imgParentElement.innerText))
                        return;

                    // Move the caret outside the hyperlink.
                    selection.Start.MoveAdjacentToElement(imgParentElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                    selection.Collapse(true);
                    selection.ToTextRange().select();
                    e.Cancel();
                }

                return;
            }
        }

        protected override void HandleTabKey(HtmlEventArgs e)
        {
            // don't handle ctrl or alt key combos
            if (e.htmlEvt.ctrlKey || e.htmlEvt.altKey)
                return;

            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.TabAsIndent))
            {
                if (!e.htmlEvt.shiftKey)
                    _keyBoardHandler.MaybeHandleInsert('\t', () => InsertHtml("&nbsp;&nbsp;&nbsp; ", true));
            }
            else
            {
                IHtmlEditorCommandSource commandSource = this;
                if (e.htmlEvt.shiftKey)
                {
                    if (commandSource.CanOutdent)
                        commandSource.ApplyOutdent();
                }
                else
                {
                    if (commandSource.CanIndent)
                        commandSource.ApplyIndent();
                }
            }

            e.Cancel();
        }

        protected override void HandleBackspaceKey(HtmlEventArgs e)
        {
            // This is a fix for an issue where hitting the backspace key deletes just an anchor tag around an image,
            // but does not delete the image as well. The repro for this issue is to insert an image in Writer (which
            // by default adds an anchor around the image), click on the empty space to the right of the image (or hit
            // End) and then hit backspace.
            if (SelectedMarkupRange != null && SelectedMarkupRange.IsEmpty())
            {
                MarkupContext context = SelectedMarkupRange.Start.Left(false);

                // If we're backspacing over an anchor element.
                if (context.Element == null ||
                    context.Context != _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope ||
                    !(context.Element is IHTMLAnchorElement))
                {
                    return;
                }

                // And the anchor contains just a single child element.
                IHTMLElementCollection anchorChildren = (IHTMLElementCollection)context.Element.children;
                if (anchorChildren.length > 1 || !String.IsNullOrEmpty(context.Element.innerText))
                {
                    return;
                }

                // And that single child element is an image.
                IHTMLElementCollection childImgs = (IHTMLElementCollection)anchorChildren.tags("img");
                if (childImgs != null && childImgs.length == 1)
                {
                    using (IUndoUnit undoUnit = CreateUndoUnit())
                    {
                        HTMLElementHelper.RemoveElement(context.Element);
                        e.Cancel();

                        undoUnit.Commit();
                    }
                }

                return;
            }
        }

        internal bool HasTags
        {
            get
            {
                try
                {
                    MarkupRange markupRange = MarkupServices.CreateMarkupRange(HTMLDocument.body);
                    return ContentSourceManager.ContainsSmartContentFromSource(TagContentSource.ID, markupRange);
                }
                catch (Exception ex)
                {
                    // we return true on exception here because we don't want to halt publishing
                    // due to an unexpected error
                    Trace.Fail("Unexpected exception attemptign to discover tags in document: " + ex.ToString());
                    return true;
                }
            }
        }

        private void ScrollToTop()
        {
            HTMLDocument.parentWindow.scrollTo(0, 0);
        }

        /// <summary>
        /// Attaches runtime defined styles that can be used to tweak the editing
        /// behavior of the editing template.
        /// </summary>
        /// <param name="document"></param>
        private void AttachEditingStyles(IHTMLDocument document)
        {
            IHTMLDocument3 doc3 = (IHTMLDocument3)document;
            IHTMLElementCollection c = doc3.getElementsByTagName("HEAD");
            IHTMLElement head = (IHTMLElement)c.item(0, 0);

            MarkupPointer insertionPoint = MarkupServices.CreateMarkupPointer(head, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);

            int major;
            int minor;
            BrowserHelper.GetInstalledVersion(out major, out minor);

            string postTitleStyles = "margin: 0px 0px 10px 0px; padding: 0px; border: 0px;"; //add some space btwn the title and body

            //set a minimum height for the body element so that it takes up a larger space when its empty.
            string postBodyStyles = String.Format(CultureInfo.InvariantCulture, "margin: 0px; padding: 0px; border: 0px; {0}", _postBodyInlineStyle);
            string defaultCssStyles = ".postTitle {" + postTitleStyles + "} .postBody {" + postBodyStyles + "}";

            string style = String.Format(CultureInfo.InvariantCulture, "<style>{0}</style>", defaultCssStyles);
            MarkupServices.InsertHtml(style, insertionPoint);

            RemoveProblematicEditingStyles();

            //bug 297804: on IE7 beta3, updating the head styles doesn't update the display of the rendered document,
            //so we need to invalidate the postBody layout.
            InvalidatePostBodyElement();
        }

        /// <summary>
        /// Removes editing styles that are known to cause buggy editing.
        /// </summary>
        private void RemoveProblematicEditingStyles()
        {
            IHTMLElement postTitleElement = PostTitleElement;
            IHTMLElement postBodyElement = PostBodyElement;

            //remove scroll bars (fixes bug 298805)
            RemoveScrollOverflowStyles(postBodyElement);
            RemoveScrollOverflowStyles(postTitleElement);

            //remove 0px line height styles
            RemoveEmptyLineHeightStyles(postTitleElement);
            RemoveEmptyLineHeightStyles(postBodyElement);

            //remove negative margins if they exist
            FixupNegativeMarginsAndPadding(postTitleElement);
            FixupNegativeMarginsAndPadding(postBodyElement);
        }

        private void FixupNegativeMarginsAndPadding(IHTMLElement element)
        {
            if (element == null)
                return;

            IHTMLElement currentElement = element;
            while (currentElement != null)
            {
                IHTMLElement2 e2 = (IHTMLElement2)currentElement;

                int marginTop = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginTop, currentElement, null, true);
                int marginBottom = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginBottom, currentElement, null, true);
                int marginLeft = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginLeft, currentElement, null, false);
                int marginRight = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginRight, currentElement, null, false);

                if (marginTop < 0)
                    e2.runtimeStyle.marginTop = "0px";

                if (marginLeft < 0)
                    e2.runtimeStyle.marginLeft = "0px";

                if (marginBottom < 0)
                    e2.runtimeStyle.marginBottom = "0px";

                if (marginRight < 0)
                    e2.runtimeStyle.marginRight = "0px";

                int paddingTop = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingTop, currentElement, null, true);
                int paddingBottom = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingBottom, currentElement, null, true);
                int paddingLeft = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingLeft, currentElement, null, false);
                int paddingRight = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingRight, currentElement, null, false);

                if (paddingTop < 0)
                    e2.runtimeStyle.paddingTop = "0px";

                if (paddingLeft < 0)
                    e2.runtimeStyle.paddingLeft = "0px";

                if (paddingBottom < 0)
                    e2.runtimeStyle.paddingBottom = "0px";

                if (paddingRight < 0)
                    e2.runtimeStyle.paddingRight = "0px";

                currentElement = currentElement.parentElement;
            }
        }

        /// <summary>
        /// Removes styles that can cause scroll bars to appear in the editable regions on IE7
        /// </summary>
        private void RemoveScrollOverflowStyles(IHTMLElement e)
        {
            IHTMLElement currElement = e;
            while (currElement != null)
            {
                IHTMLElement2 e2 = currElement as IHTMLElement2;
                if (e2 is IHTMLBodyElement)
                {
                    // Don't let the body have a hidden overflow, because it hides the canvas's vertical scroll bar.
                    if (e2.currentStyle.overflow == "hidden")
                        e2.runtimeStyle.overflow = "auto";
                }
                else
                {
                    e2.runtimeStyle.overflow = "visible";
                }

                currElement = currElement.parentElement;
                if (currElement.parentElement == null)
                {
                    //don't clear the overflow style for the top most element.
                    return;
                }
            }
        }

        /// <summary>
        /// Remove zero height line styles, so editable text is not invisible.
        /// </summary>
        /// <param name="e"></param>
        private void RemoveEmptyLineHeightStyles(IHTMLElement e)
        {
            if (e == null)
                return;

            IHTMLElement2 e2 = e as IHTMLElement2;
            string lineHeight = e2.currentStyle.lineHeight as string;
            if (lineHeight != null)
            {
                if (lineHeight == "0px" || lineHeight == "0em")
                {
                    e2.runtimeStyle.lineHeight = "1em";
                }
            }
        }

        private void InvalidatePostBodyElement()
        {
            //We invalidate the document layout by adjusting some of the editor runtimeStyles.
            IHTMLElement2 postBody = PostBodyElement as IHTMLElement2;
            string margin = postBody.runtimeStyle.margin;
            postBody.runtimeStyle.margin = "0px";
            postBody.runtimeStyle.margin = margin;
        }

        protected override bool Editable
        {
            get { return _editable; }
        }
        bool _editable = true;

        public void SetEditable(bool editMode)
        {
            _editable = editMode;
        }

        public override bool FullyEditableRegionActive
        {
            get
            {
                return base.FullyEditableRegionActive;
            }
            set
            {
                base.FullyEditableRegionActive = value;
                MarshalImagesSupported = value;
                MarshalHtmlSupported = value;
                MarshalFilesSupported = value;
                MarshalTextSupported = true;
                MarshalUrlSupported = value;
            }
        }

        protected override bool AssignBehaviorPolicy(string behavior, out byte pPolicy)
        {
            if (BehaviorManager == null)
            {
                return base.AssignBehaviorPolicy(behavior, out pPolicy);
            }

            if (BehaviorManager.ContainsBehavior(behavior))
            {
                pPolicy = URLPOLICY.ALLOW;
                return true;
            }

            return base.AssignBehaviorPolicy(behavior, out pPolicy);
        }

        #region Behavior Management

        internal ElementBehaviorManager BehaviorManager
        {
            get { return _elementBehaviorManager; }
        }
        private ElementBehaviorManager _elementBehaviorManager;

        private void InitializeElementBehaviors()
        {
            _elementBehaviorManager = new ElementBehaviorManager();

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                "." + Emoticon.CLASS_NAME,
                new Guid("2D089C28-4102-4aea-93D9-0126FE478032"),
                new ElementBehaviorCreator(CreateDisabledImageElementBehavior)));

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                "img", new Guid("FA320E44-8E9F-431a-8022-8152E58A2A95"),
                new ElementBehaviorCreator(CreateImageEditingElementBehavior)));

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                "." + ContentSourceManager.EDITABLE_SMART_CONTENT,
                new Guid("BD5C8C69-C389-42df-8262-4B775174F40F"),
                new ElementBehaviorCreator(CreateContentSourceElementBehavior)));

            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.BrokenSmartContent))
            {
                _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                                                             "." + ContentSourceManager.SMART_CONTENT + ", ." +
                                                             HtmlPreserver.PRESERVE_CLASS,
                                                             new Guid("8EBB1172-E4EB-4a54-9CB5-C32DD626065C"),
                                                             new ElementBehaviorCreator(
                                                                 CreateDisabledContentSourceElementBehavior)));
            }

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                "." + ContentSourceManager.EDITABLE_SMART_CONTENT + " > ." + InlineEditField.EDIT_FIELD,
                new Guid("39E96718-EAAF-4223-812B-A44F47F52A88"),
                new ElementBehaviorCreator(CreateContentSourceEditFieldBehavior)));

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                "blockquote",
                new Guid("00164784-50F5-4aba-9662-D4633F1AFE07"),
                new ElementBehaviorCreator(CreateBlockquoteEditingElementBehavior)));

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                "#extendedEntryBreak",
                new Guid("BBDC4E96-F705-4679-B64B-858CBBD6CF50"),
                new ElementBehaviorCreator(CreateExtendedEntrySplitterElementBehavior)));

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                ".postBody table",
                new Guid("{378004A4-88DE-491b-BAEB-DB205267DB12}"),
                new ElementBehaviorCreator(_tableEditingManager.CreateTableEditingElementBehavior)));

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                ".postBody td",
                new Guid("{62A0CB45-EB5C-445d-8FC2-9B923F8B83F4}"),
                new ElementBehaviorCreator(_tableEditingManager.CreateTableCellEditingElementBehavior)));

            _elementBehaviorManager.RegisterBehavior(new ElementBehaviorDefinition(
                ".postBody th",
                new Guid("{CC4500E4-ECEE-4cca-AF33-D3D862E9D324}"),
                new ElementBehaviorCreator(_tableEditingManager.CreateTableCellEditingElementBehavior)));
        }

        private MshtmlElementBehavior CreateDisabledImageElementBehavior()
        {
            return new DisabledImageElementBehavior(this);
        }

        private MshtmlElementBehavior CreateImageEditingElementBehavior()
        {
            return new ImageEditingElementBehavior(this);
        }

        private MshtmlElementBehavior CreateContentSourceElementBehavior()
        {
            return new SmartContentElementBehavior(this, _sidebarContext, _sourceContext, _resizedListener);
        }

        private MshtmlElementBehavior CreateDisabledContentSourceElementBehavior()
        {
            return new DisabledSmartContentElementBehavior(this, _sourceContext);
        }

        private MshtmlElementBehavior CreateContentSourceEditFieldBehavior()
        {
            return new TitledRegionElementBehavior(this, null, null);
        }

        private MshtmlElementBehavior CreateBlockquoteEditingElementBehavior()
        {
            return new BlockquoteEditingElementBehavior(this);
        }

        private MshtmlElementBehavior CreateExtendedEntrySplitterElementBehavior()
        {
            return new ExtendedEntrySplitterElementBehavior(this);
        }

        protected override void OnFindBehavior(string bstrBehavior, string bstrBehaviorUrl, IElementBehaviorSite pSite, out IElementBehaviorRaw ppBehavior)
        {
            ppBehavior = BehaviorManager.CreateBehavior(bstrBehavior);
        }

        private void RemoveParentAnchorFromTitle(IHTMLElement titleElement, MshtmlMarkupServices markupServices)
        {
            try
            {
                // Look for a parent <A> anchor tag
                MarkupPointer startPointer = markupServices.CreateMarkupPointer(titleElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                IHTMLElement parentAnchor = startPointer.GetParentElement(ElementFilters.ANCHOR_ELEMENTS);

                if (parentAnchor != null)
                {
                    // To make sure the title is readable, we will replace the anchor tag with a new span tag
                    // that has all of the anchor tag properties we believe need to be propogated.  This will
                    // keep the final render as close to the original as possible.

                    // Get the next parent block element to compare against the anchor tag
                    IHTMLElement parentBlock = startPointer.GetParentElement(ElementFilters.BLOCK_OR_TABLE_CELL_ELEMENTS);

                    // If no block element was found we will compare against the body
                    if (parentBlock == null)
                        parentBlock = startPointer.GetParentElement(ElementFilters.BODY_ELEMENT);

                    Debug.Assert(parentBlock != null, "parentBlock was unexpectedly null!");

                    IHTMLElement newAnchor = markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_SPAN, string.Empty);
                    IHTMLElement2 newAnchor2 = (IHTMLElement2)newAnchor;
                    IHTMLElement2 parentAnchor2 = (IHTMLElement2)parentAnchor;
                    IHTMLElement2 parentBlock2 = (IHTMLElement2)parentBlock;

                    // Only insert the size if it was redefined from the anchor's parent.  That way
                    // relative sizes don't layer on each other.
                    if (!(parentAnchor2.currentStyle.fontSize.Equals(parentBlock2.currentStyle.fontSize)))
                        newAnchor2.runtimeStyle.fontSize = parentAnchor2.currentStyle.fontSize;

                    // Copy all other attributes
                    CopyAnchorAttributes(parentAnchor2, newAnchor2);

                    markupServices.ReplaceElement(parentAnchor, newAnchor);
                }
            }
            catch (Exception ex)
            {
                // Any failure is ignorable, we will just continue with the parent anchor tag
                Debug.WriteLine("Failed to remove parent anchor tag from title, " + ex);
            }
        }

        private void CopyAnchorAttributes(IHTMLElement2 source, IHTMLElement2 destination)
        {
            destination.runtimeStyle.backgroundAttachment = source.currentStyle.backgroundAttachment;
            destination.runtimeStyle.backgroundColor = source.currentStyle.backgroundColor;
            destination.runtimeStyle.backgroundImage = source.currentStyle.backgroundImage;
            destination.runtimeStyle.backgroundPositionX = source.currentStyle.backgroundPositionX;
            destination.runtimeStyle.backgroundPositionY = source.currentStyle.backgroundPositionY;
            destination.runtimeStyle.backgroundRepeat = source.currentStyle.backgroundRepeat;
            destination.runtimeStyle.borderBottomColor = source.currentStyle.borderBottomColor;
            destination.runtimeStyle.borderBottomStyle = source.currentStyle.borderBottomStyle;
            destination.runtimeStyle.borderBottomWidth = source.currentStyle.borderBottomWidth;
            destination.runtimeStyle.borderColor = source.currentStyle.borderColor;
            destination.runtimeStyle.borderLeftColor = source.currentStyle.borderLeftColor;
            destination.runtimeStyle.borderLeftStyle = source.currentStyle.borderLeftStyle;
            destination.runtimeStyle.borderLeftWidth = source.currentStyle.borderLeftWidth;
            destination.runtimeStyle.borderRightColor = source.currentStyle.borderRightColor;
            destination.runtimeStyle.borderRightStyle = source.currentStyle.borderRightStyle;
            destination.runtimeStyle.borderRightWidth = source.currentStyle.borderRightWidth;
            destination.runtimeStyle.borderStyle = source.currentStyle.borderStyle;
            destination.runtimeStyle.borderTopColor = source.currentStyle.borderTopColor;
            destination.runtimeStyle.borderTopStyle = source.currentStyle.borderTopStyle;
            destination.runtimeStyle.borderTopWidth = source.currentStyle.borderTopWidth;
            destination.runtimeStyle.borderWidth = source.currentStyle.borderWidth;
            destination.runtimeStyle.clear = source.currentStyle.clear;
            destination.runtimeStyle.color = source.currentStyle.color;
            destination.runtimeStyle.display = source.currentStyle.display;
            destination.runtimeStyle.fontFamily = source.currentStyle.fontFamily;
            destination.runtimeStyle.fontStyle = source.currentStyle.fontStyle;
            destination.runtimeStyle.fontVariant = source.currentStyle.fontVariant;
            destination.runtimeStyle.fontWeight = source.currentStyle.fontWeight.ToString();
            destination.runtimeStyle.height = source.currentStyle.height;
            destination.runtimeStyle.left = source.currentStyle.left;
            destination.runtimeStyle.letterSpacing = source.currentStyle.letterSpacing;
            destination.runtimeStyle.lineHeight = source.currentStyle.lineHeight;
            destination.runtimeStyle.listStyleImage = source.currentStyle.listStyleImage;
            destination.runtimeStyle.listStylePosition = source.currentStyle.listStylePosition;
            destination.runtimeStyle.listStyleType = source.currentStyle.listStyleType;
            destination.runtimeStyle.margin = source.currentStyle.margin;
            destination.runtimeStyle.marginBottom = source.currentStyle.marginBottom;
            destination.runtimeStyle.marginLeft = source.currentStyle.marginLeft;
            destination.runtimeStyle.marginRight = source.currentStyle.marginRight;
            destination.runtimeStyle.marginTop = source.currentStyle.marginTop;
            ((IHTMLStyle5)destination.runtimeStyle).maxHeight = ((IHTMLCurrentStyle4)(source.currentStyle)).maxHeight;
            ((IHTMLStyle5)destination.runtimeStyle).maxWidth = ((IHTMLCurrentStyle4)(source.currentStyle)).maxWidth;
            ((IHTMLStyle4)destination.runtimeStyle).minHeight = ((IHTMLCurrentStyle3)(source.currentStyle)).minHeight;
            ((IHTMLStyle5)destination.runtimeStyle).minWidth = ((IHTMLCurrentStyle4)(source.currentStyle)).minWidth;
            // IHTMLCurrentStyle5 and IHTMLStyle 6 are only available on IE8 or higher
            if ((source.currentStyle as IHTMLCurrentStyle5) != null && (destination.runtimeStyle as IHTMLStyle6) != null)
            {
                ((IHTMLStyle6)destination.runtimeStyle).outlineColor = ((IHTMLCurrentStyle5)(source.currentStyle)).outlineColor;
                ((IHTMLStyle6)destination.runtimeStyle).outlineStyle = ((IHTMLCurrentStyle5)(source.currentStyle)).outlineStyle;
                // Only available on IE8 or higher.
                // Attempting to query the outline-width directly may throw a COMException with HRESULT E_FAIL
                // If so we just leave it alone, rather than forcing the default value
                try
                {
                    ((IHTMLStyle6)destination.runtimeStyle).outlineWidth = ((IHTMLCurrentStyle5)(source.currentStyle)).outlineWidth;
                }
                catch (COMException e)
                {
                    // Known issue, just ignore the exception.
                    if (e.ErrorCode != HRESULT.E_FAILED)
                    {
                        throw;
                    }
                }
            }
            destination.runtimeStyle.overflow = source.currentStyle.overflow;
            destination.runtimeStyle.padding = source.currentStyle.padding;
            destination.runtimeStyle.paddingBottom = source.currentStyle.paddingBottom;
            destination.runtimeStyle.paddingLeft = source.currentStyle.paddingLeft;
            destination.runtimeStyle.paddingRight = source.currentStyle.paddingRight;
            destination.runtimeStyle.paddingTop = source.currentStyle.paddingTop;
            destination.runtimeStyle.pageBreakAfter = source.currentStyle.pageBreakAfter;
            destination.runtimeStyle.pageBreakBefore = source.currentStyle.pageBreakBefore;
            ((IHTMLStyle2)destination.runtimeStyle).right = source.currentStyle.right;
            destination.runtimeStyle.styleFloat = source.currentStyle.styleFloat;
            destination.runtimeStyle.textAlign = source.currentStyle.textAlign;
            destination.runtimeStyle.textDecoration = source.currentStyle.textDecoration;
            destination.runtimeStyle.textIndent = source.currentStyle.textIndent;
            destination.runtimeStyle.textTransform = source.currentStyle.textTransform;
            destination.runtimeStyle.top = source.currentStyle.top;
            ((IHTMLStyle2)destination.runtimeStyle).unicodeBidi = source.currentStyle.unicodeBidi;
            destination.runtimeStyle.verticalAlign = source.currentStyle.verticalAlign;
            destination.runtimeStyle.visibility = source.currentStyle.visibility;
            destination.runtimeStyle.whiteSpace = ((IHTMLCurrentStyle3)(source.currentStyle)).whiteSpace;
            destination.runtimeStyle.width = source.currentStyle.width;
            destination.runtimeStyle.wordSpacing = ((IHTMLCurrentStyle3)(source.currentStyle)).wordSpacing;
            destination.runtimeStyle.zIndex = source.currentStyle.zIndex;
        }

        /// <summary>
        /// Attach to behaviors manually
        /// </summary>
        /// <param name="context"></param>
        protected override void AttachBehaviors(IHtmlEditorComponentContext context)
        {
            if (Editable)
            {
                base.AttachBehaviors(context);

                // re-attach to behaviors
                IHTMLElement titleElement = PostTitleElement;
                IHTMLElement bodyElement = PostBodyElement;

                if (titleElement != null)
                {
                    // WinLive 247899, 240926: If we have a title element, check if it has a parent anchor <A> element
                    // Remove parent anchor, otherwise
                    //  a. we cannot perform mouse based text selection inside title text
                    //  b. clicking in the middle of title text will place the cursor at the beginning, causing
                    //     inconsistent selection states in other parts (like damage services)
                    RemoveParentAnchorFromTitle(titleElement, context.MarkupServices);

                    _titleBehavior = new PostTitleEditingElementBehavior(context, null, bodyElement);
                    TitleBehavior.AttachToElement(titleElement);
                    TitleBehavior.TitleChanged += new EventHandler(titleBehavior_TitleChanged);
                    TitleBehavior.EditableRegionFocusChanged += new EventHandler(regionBehavior_EditableRegionFocusChanged);
                }

                if (bodyElement != null)
                {
                    _bodyBehavior = new PostBodyEditingElementBehavior(this, context, titleElement, null);
                    BodyBehavior.EditableRegionFocusChanged += new EventHandler(regionBehavior_EditableRegionFocusChanged);
                    BodyBehavior.AttachToElement(bodyElement);
                }

                SnapRectEvent += new SnapRectEventHandler(htmlEditor_SnapRectEvent);
                MouseUp += new HtmlEventHandler(htmlEditor_MouseUp);
            }
            else
            {
                _titleBehavior = null;
                _bodyBehavior = null;
            }
        }

        /// <summary>
        /// Detach from manually attached behaviors
        /// </summary>
        protected override void DetachBehaviors()
        {
            try
            {
                SnapRectEvent -= new SnapRectEventHandler(htmlEditor_SnapRectEvent);
                MouseUp -= new HtmlEventHandler(htmlEditor_MouseUp);

                base.DetachBehaviors();

                if (_titleBehavior != null)
                {
                    _titleBehavior.TitleChanged -= new EventHandler(titleBehavior_TitleChanged);
                    _titleBehavior.EditableRegionFocusChanged -= new EventHandler(regionBehavior_EditableRegionFocusChanged);
                    _titleBehavior.DetachFromElement();
                    _titleBehavior = null;
                }
                if (_bodyBehavior != null)
                {
                    _bodyBehavior.EditableRegionFocusChanged -= new EventHandler(regionBehavior_EditableRegionFocusChanged);
                    _bodyBehavior.DetachFromElement();
                    _bodyBehavior = null;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception detaching behavior -- investigate this immediately!!!!: " + ex.ToString());
            }
        }

        #endregion

        public override void LoadHtmlFile(string filePath)
        {
            throw new NotSupportedException("LoadHtmlFile is not supported by BlogPostHtmlEditorControl (use WriterHtmlEditorControl)");
        }

        protected override string GetEditedHtmlCore(bool preferWellFormed)
        {
            if (_bodyBehavior != null && BodyBehavior.ElementBehaviorAttached)
            {
                string editedHtml = BodyBehavior.GetEditedHtml(preferWellFormed, true);
                editedHtml = HTMLTrimmer.Trim(editedHtml, true);
                return preserver.RestorePreserved(editedHtml);
            }
            else
            {
                //This avoids a bug that causes HTML to be nulled out when switching between editors too quickly
                //(or when there is no bodyBehavior applied)
                return _lastSetBodyHtml;
            }
        }

        public override string GetEditedHtmlFast()
        {
            if (_bodyBehavior != null && BodyBehavior.ElementBehaviorAttached)
            {
                return BodyBehavior.GetEditedHtml(false, false);
            }
            else
            {
                return _lastSetBodyHtml;
            }
        }

        public string GetEditedTitleHtml()
        {
            if (_titleBehavior != null && TitleBehavior.ElementBehaviorAttached)
                return TitleBehavior.GetEditedHtml(false, true);
            else
            {
                //This avoids a bug that causes HTML to be nulled out when switching between editors too quickly
                //(or when there is no titleBehavior applied)
                return _lastSetTitle;
            }
        }

        public IFocusableControl FocusControl
        {
            get
            {
                return new FocusableControl(MshtmlEditor);
            }
        }

        public void Focus()
        {
            MshtmlEditor.Focus();
        }

        public void FocusTitle()
        {
            // focus the fragment element
            if (_templateContainsTitle)
                FocusElement((IHTMLElement2)PostTitleElement, false);
            else
            {
                Debug.Fail("FocusTitle called but no title in template.");
                FocusBody();
            }
        }

        public void FocusBody()
        {
            // focus the fragment element
            FocusElement((IHTMLElement2)PostBodyElement, false);
        }

        public override bool CleanHtmlOnPaste
        {
            get
            {
                if (CleanHtmlOnPasteOverride.HasValue)
                {
                    return CleanHtmlOnPasteOverride.Value;
                }

                return GlobalEditorOptions.SupportsFeature(ContentEditorFeature.CleanHtmlOnPaste);
            }
        }

        internal IHTMLDocument2 GetHtmlDocument()
        {
            return this.HTMLDocument;
        }

        private void FocusElement(IHTMLElement2 element, bool selectAll)
        {
            if (element != null)
            {
                try
                {
                    element.focus();
                }
                catch (Exception e)
                {
                    //This can cause an exception under some weblog styles when some behaviors are attached.  Bug 407544.
                    Trace.Fail("Unexpected exception while focusing element", e.ToString());
                }
                if (selectAll)
                    SelectAll();
            }
        }

        public bool DocumentHasFocus()
        {
            return ((IHTMLDocument4)HTMLDocument).hasFocus();
        }

        private bool _templateContainsTitle;
        public void LoadHtmlFragment(string title, string blogPostBody, string baseUrl, BlogEditingTemplate editingTemplate)
        {
            _spellingManager.ClearIgnoreOnce();
            _templateContainsTitle = editingTemplate.ContainsTitle;

            //if any manually attached behaviors are attached, remove them.
            //Note: this is necessary to prevent errors from occuring when switching between wysiwyg and code view
            //to quickly.
            DetachBehaviors();

            // Deterministically dispose any automatically created behaviors.
            _elementBehaviorManager.DisposeCreatedBehaviors();

            if (TidyWhitespace && Editable)
            {
                blogPostBody = HTMLTrimmer.Trim(blogPostBody, true);
                if (blogPostBody != String.Empty)
                    blogPostBody += CONTENT_BODY_PADDING;
            }

            // SInce this action is reverted in the deattach of the behvaiors, we only do this in edit mode,
            // otherwie there is no deattach to balance it out.  See that AttachBehaviors() doesnt attach in edit mode
            if (Editable)
            {
                //attach the extended entry HTML behavior
                blogPostBody = PostBodyEditingElementBehavior.ApplyExtendedEntryBehavior(blogPostBody);
            }

            //Hack: cache the title and body HTML to avoid returning a bogus value
            //GetEdited[Html|TitleHtml] is called before the document behaviors are attached.
            _lastSetTitle = title;
            _lastSetBodyHtml = blogPostBody;

            preserver.Reset();
            blogPostBody = preserver.ScanAndPreserve(blogPostBody);

            // update html content with standard header and footer
            // Hack: put some padding at the bottom of the div so that the bottom line of text does not get
            // cutoff if it extends below the baseline (p's and g's sometimes cause issues)

            //put an ID'd span around the title so that editing behaviors can be attachd to it.
            //Note: use a span instead of a DIV as since DIVs can't be nested inside inline elements, so if
            //the title text was surrounded by inline tags (like font, or bold, etc) then the DIV would
            //break the rendering of the surrounding inline styles.
            string titleHtml = _strategy.OnTitleInserted(title);

            if (blogPostBody == String.Empty && TidyWhitespace)
                blogPostBody = "<p>&nbsp;</p>";

            blogPostBody = _strategy.OnBodyInserted(blogPostBody);

            string postHtml = editingTemplate.ApplyTemplateToPostHtml(title, titleHtml, blogPostBody);

            //add the base tag for the HTML so relative resource references in the post will be resolved.
            postHtml = postHtml.Replace("</HEAD>", String.Format(CultureInfo.InvariantCulture, "<BASE href=\"{0}\"></HEAD>", baseUrl));

            // save the content to a temp file for loading
            string documentPath = TempFileManager.Instance.CreateTempFile("index.htm");
            using (StreamWriter streamWriter = new StreamWriter(documentPath, false, Encoding.UTF8))
                streamWriter.Write(postHtml);

            // load the document
            base.LoadHtmlFile(documentPath);
        }

        private readonly HtmlPreserver preserver = new HtmlPreserver();

        private void BlogPostHtmlEditorControl_SelectionChanged(object sender, EventArgs e)
        {
            SetSelectedEditField();
            _tableEditingManager.ManageCommands();

            IHtmlEditorComponentContext editorComponentContext = this;
            if (editorComponentContext.Selection.SelectedImage != null || editorComponentContext.Selection is SmartContentSelection)
                _imageEditingContext.ActivateDecoratorsManager();
            else
                _imageEditingContext.DeactivateDecoratorsManager();
        }

        protected override bool TryMoveToValidSelection(MarkupRange range)
        {
            IHTMLElement element;

            element = range.ParentElement(ElementFilters.CreateTagIdFilter("DIV"));
            while (element != null)
            {
                if (ContentSourceManager.IsSmartContent(element))
                {
                    SmartContentSelection.SelectIfSmartContentElement(this, element);
                    return true;
                }

                element = element.parentElement;
            }

            return false;
        }

        protected override bool IsValidContiguousSelection()
        {
            try
            {
                //The selection is only valid if it is contained within an Editable element.
                //This avoids bugs related to executing DOM commands on selections that are
                //not contained within the template's editable regions (such as deleting the entire document)
                MarkupRange selection = SelectedMarkupRange;
                if (!IsValidContentInsertionPoint(selection))
                {
                    // Ultimately, we should make this a Debug.Assert so that we can immediately determine that an unexpected invalid region has been detected.
                    Debug.WriteIf(_logInvalidEditRegions, "Invalid selection detected (" + selection.Start.PositionTextDetail + " " + selection.End.PositionTextDetail + ") with stack trace: " + new StackTrace());
                    return false;
                }
                else
                    return true;
            }
            catch (COMException ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }

        }

        /// <summary>
        /// When inserting we want to check to make sure the editor is in 'edit' mode
        /// and that what is being inserted is in one of the editable regions of the canvas.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected override bool IsValidContentInsertionPoint(MarkupRange target)
        {
            if (Editable)
            {
                // Check to make sure the target is in the body of the post
                MarkupRange bounds = MarkupServices.CreateMarkupRange();
                IHTMLElement[] editableElements = EditableElements;
                for (int i = 0; i < editableElements.Length; i++)
                {
                    bounds.MoveToElement(editableElements[i], false);
                    if (bounds.InRange(target))
                        return IsValidEditRegion(target);
                }
            }
            return false;
        }

        protected override bool ShouldEmptySelection()
        {
            // WinLive 196005: We do not want to empty the selection if the user is selecting SmartContent.
            if (ContentSourceManager.IsSmartContent(SelectedMarkupRange.Start.CurrentScope) ||
                ContentSourceManager.IsSmartContent(SelectedMarkupRange.End.CurrentScope))
            {
                return false;
            }

            return true;
        }

        protected MarkupRange[] IntersectWithEditableElements(MarkupRange range)
        {
            List<MarkupRange> targets = new List<MarkupRange>();

            if (Editable && range.Positioned)
            {
                MarkupRange bounds = MarkupServices.CreateMarkupRange();
                foreach (IHTMLElement editableElement in EditableElements)
                {
                    bounds.MoveToElement(editableElement, false);
                    if (bounds.InRange(range))
                        targets.Add(range);
                    else if (bounds.Intersects(range))
                        targets.Add(bounds.Intersect(range));
                }
            }

            return targets.ToArray();
        }

        /// <summary>
        /// When we check to see if location is editable by a user we want check everything we check in
        /// IsValidContentInsertionPoint() as well as that they are in a contentEditable=true region, otherwise
        /// mshtml will prevent them from typing.  This function should be used when validating mouse clicks
        /// and keyboard navigation.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected bool IsValidEditRegion(MarkupRange target)
        {
            // Check to make sure what it is being inserted into is contentEditable=true
            if (MarkupHelpers.GetEditableRange(target.Start.CurrentScope, MarkupServices) != null && MarkupHelpers.GetEditableRange(target.End.CurrentScope, MarkupServices) != null)
                return true;

            return false;
        }

        public IHTMLElement PostTitleElement
        {
            get
            {
                Debug.Assert(DocumentIsReady,
                             "The document isn't fully loaded, but you are accessing parts of the document. If this is a cached editor, you will be accessing the last mail message.");
                return _strategy.TitleElement(HTMLDocument);
            }
        }

        public override IHTMLElement PostBodyElement
        {
            get
            {
                Debug.Assert(DocumentIsReady,
                             "The document isn't fully loaded, but you are accessing parts of the document. If this is a cached editor, you will be accessing the last mail message. HTMLDocument.readyState: " + HTMLDocument.readyState);
                return _strategy.PostBodyElement(HTMLDocument);
            }
        }

        /// <summary>
        /// Make sure that the display pointer doesn't end up inside SmartContent,
        /// where the user can't edit anything anyway. If necessary, move the pointer
        /// to the left or right of the content depending on its position.
        /// </summary>
        /// <param name="displayPointer"></param>
        /// <param name="fromClientPoint"></param>
        protected override void AdjustDisplayPointerPosition(ref IDisplayPointerRaw displayPointer, Point fromClientPoint)
        {
            MarkupPointer tester = MarkupServices.CreateMarkupPointer();
            displayPointer.PositionMarkupPointer(tester.PointerRaw);
            IHTMLElement el = GetSurroundingSmartContent(tester);
            if (el == null)
                return;

            int center = HTMLElementHelper.GetLeftRelativeToClient(el) + el.offsetWidth / 2;
            tester.MoveAdjacentToElement(el,
                (fromClientPoint.X < center)
                ? _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin
                : _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);

            DisplayServices.TraceMoveToMarkupPointer(displayPointer, tester);
        }

        private static IHTMLElement GetSurroundingSmartContent(MarkupPointer p)
        {
            IHTMLElement el = p.CurrentScope;
            while (true)
            {
                switch ((((IHTMLElement3)el).contentEditable ?? "inherit").ToLowerInvariant())
                {
                    case "inherit":
                        el = el.parentElement;
                        if (el == null)
                            return null;
                        break;
                    case "false":
                        return el;
                    case "true":
                        return null;
                }
            }
        }

        public void InsertExtendedEntryBreak()
        {
            _bodyBehavior.InsertExtendedEntryBreak();
        }

        public void ChangeSelection(SelectionPosition position)
        {
            MarkupPointer location;
            switch (position)
            {
                case SelectionPosition.BodyStart:
                    location = MarkupServices.CreateMarkupPointer(PostBodyElement,
                                                                  _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    break;
                case SelectionPosition.BodyEnd:
                    location = MarkupServices.CreateMarkupPointer(PostBodyElement,
                                                                  _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                    break;
                default:
                    throw new ArgumentException("Unknown position value");
            }

            IHTMLCaret caret;
            ((IDisplayServices)HTMLDocument).GetCaret(out caret);
            IDisplayPointer displayPointer;
            ((IDisplayServices)HTMLDocument).CreateDisplayPointer(out displayPointer);
            displayPointer.MoveToMarkupPointer((IMarkupPointer)location.PointerRaw, null);
            caret.MoveCaretToPointerEx(displayPointer, 1, 1, _CARET_DIRECTION.CARET_DIRECTION_INDETERMINATE);
        }

        protected override bool ShowAllHyperlinkOptions
        {
            get { return GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShowAllLinkOptions); }
        }

        protected override bool UrlIsTemporaryLocalFilePath(string url)
        {
            if (UrlHelper.IsFileUrl(url))
            {
                ISupportingFile supportingFile = _imageEditingContext.SupportingFileService.GetFileByUri(new Uri(url));
                return supportingFile != null;
            }
            return false;
        }

        protected override void InsertImageLink(string url, string title, bool newWindow, string rel)
        {
            if (UpdateImageLink != null)
                UpdateImageLink(url, title, newWindow, rel);

        }

        public void InsertHorizontalLine(bool plainText)
        {
            using (IUndoUnit undo = CreateUndoUnit())
            {
                using (DamageServices.CreateDamageTracker(SelectedMarkupRange.Clone(), true))
                {
                    InsertHtml(plainText ? "<br />" + BlogPost.PlainTextHorizontalLine : BlogPost.HorizontalLine, true);
                }
                undo.Commit();
            }
        }

        public void InsertClearBreak()
        {
            using (IUndoUnit undo = CreateUndoUnit())
            {
                using (DamageServices.CreateDamageTracker(SelectedMarkupRange.Clone(), true))
                {
                    InsertHtml("<br clear=\"all\" />", true);
                }
                undo.Commit();
            }
        }

        private bool IsWithinTitleField(MarkupPointer ptr)
        {
            // If not null, we are within the "title" element.
            return (this.PostTitleElement != null &&
                ptr.GetParentElement(delegate (IHTMLElement e)
                {
                    return e.id == this.PostTitleElement.id;
                }
            ) != null);
        }

        private bool IsWithinTextOnlyField(MarkupPointer ptr)
        {
            if (IsEditFieldSelected)
                return true;

            if (InlineEditField.IsWithinEditField(ptr.CurrentScope))
                return true;

            return IsWithinTitleField(ptr);
        }

        public override void EmptySelection()
        {
            using (new RestoreBool(ref _logInvalidEditRegions, false, b => { _logInvalidEditRegions = b; }))
                base.EmptySelection();
        }

        protected override void InsertHtml(MarkupPointer start, MarkupPointer end, string html, string sourceUrl, bool allowBlockBreakout)
        {
            html = preserver.ScanAndPreserve(html);

            try
            {
                using (IUndoUnit undoUnit = CreateUndoUnit())
                {
                    if (sourceUrl != null)
                    {
                        // clean up smart content fragments being inserted into the document.
                        // this will clone existing smart content, and disable smart content from other editors
                        html = SmartContentInsertionHelper.PrepareSmartContentHtmlForEditorInsertion(html, _sourceContext);
                    }

                    if (IsWithinTextOnlyField(start))
                    {
                        html = HtmlServices.HtmlEncode(HTMLDocumentHelper.HTMLToPlainText(html).Replace(Environment.NewLine, " "));
                    }

                    if (IsWithinTitleField(start))
                    {
                        // Do we need to clean up the title before inserting?
                        _titleBehavior.CleanBeforeInsert();
                    }

                    base.InsertHtml(start, end, html, sourceUrl, allowBlockBreakout);

                    OnHtmlInserted(EventArgs.Empty);
                    undoUnit.Commit();
                }
            }
            catch (COMException e)
            {
                //The editor threw a COM exception while trying to insert the HTML, so
                //just no-op the insertion. Not ideal, but at least the user won't get scarey
                //error dialogs.
                Trace.Fail("Error inserting HTML", e.ToString());
            }
        }

        protected override bool ShouldAllowNewLineInsert(string html)
        {
            SimpleHtmlParser p = new SimpleHtmlParser(html);
            for (Element el; null != (el = p.Next());)
            {
                BeginTag tag = el as BeginTag;
                if (tag != null && tag.NameEquals("img"))
                {
                    // Don't allow new lines after emoticons.
                    string classNames = tag.GetAttributeValue("class");
                    if (!String.IsNullOrEmpty(classNames) && classNames.Contains(Emoticon.CLASS_NAME))
                        return false;
                }
            }

            return base.ShouldAllowNewLineInsert(html);
        }

        protected override void OnInsertHtml(MarkupRange newContentRange, HtmlInsertionOptions options)
        {
            base.OnInsertHtml(newContentRange, options);

            //the inserted element will be the element directly to the right of the range.
            MarkupContext markupContext = newContentRange.Start.Right(false);
            IHTMLElement insertedElement = markupContext.Element;

            MshtmlEditor.BeginInvoke(new ThreadStart(RefreshEmbeds), null);

            //automatically select the element if it is a smart content element.
            SmartContentSelection.SelectIfSmartContentElement(this, insertedElement);

            if (HtmlInsertionOptions.SuppressSpellCheck == (options & HtmlInsertionOptions.SuppressSpellCheck))
            {
                MshtmlEditor.BeginInvoke(new ThreadStart(() =>
                                                         {
                                                             _spellingManager.DamagedRange(newContentRange, false);
                                                             _spellingManager.IgnoreOnce(newContentRange);
                                                         }), null);
            }
        }

        /// <summary>
        /// Toggle embed visibility to make sure they are visible.
        /// </summary>
        private void RefreshEmbeds()
        {
            using (IUndoUnit undo = CreateInvisibleUndoUnit())
            {
                foreach (IHTMLElement ele in HTMLDocument.embeds)
                {
                    IHTMLEmbedElement dispEmbed = ele as IHTMLEmbedElement;
                    if (dispEmbed != null && dispEmbed.hidden == "false")
                    {
                        dispEmbed.hidden = "true";
                        dispEmbed.hidden = "false";
                    }
                }
                undo.Commit();
            }
        }

        public override bool CanPaste
        {
            get
            {
                bool canPaste = base.CanPaste;

                if (IsEditFieldSelected)
                    canPaste &= InlineEditField.EditFieldAcceptsCurrentClipboardData;

                return canPaste;
            }
        }

        public override bool IsEditFieldSelected
        {
            get
            {
                return SelectedEditField != null;
            }
        }

        private IHTMLElement _selectedEditField;
        public override IHTMLElement SelectedEditField
        {
            get
            {
                return _selectedEditField;
            }
        }

        private void SetSelectedEditFieldInternal(IHTMLElement element)
        {
            if (_selectedEditField != element)
            {
                _selectedEditField = element;
                FireSelectedEditFieldChanged();
            }
        }

        public event EventHandler SelectedEditFieldChanged;
        private void FireSelectedEditFieldChanged()
        {
            if (SelectedEditFieldChanged != null)
                SelectedEditFieldChanged(this, EventArgs.Empty);
        }

        private void SetSelectedEditField()
        {
            MarkupRange selection = SelectedMarkupRange;

            if (selection == null || !selection.Positioned)
            {
                SetSelectedEditFieldInternal(null);
                return;
            }

            IHTMLElement element = selection.ParentElement();
            while (element != null)
            {
                if (InlineEditField.IsEditField(element))
                {
                    SetSelectedEditFieldInternal(element);
                    return;
                }

                element = element.parentElement;
            }

            SetSelectedEditFieldInternal(null);
        }

        protected override MarkupRange PrimaryEditableBounds
        {
            get
            {
                IHTMLElement el = PostBodyElement;
                if (el == null)
                    return null;
                return MarkupServices.CreateMarkupRange(el, false);
            }
        }

        #region Context Menu Handling

        private bool ShowContextMenu(IHTMLElement element, Point screenPoint)
        {
            // show the context menu (if the caret is placed, use that as the context since the
            // editor has placed the caret into a specific DOM location based on the click, otherwise
            // use the element under the point)
            MarkupRange selectedMarkupRange = SelectedMarkupRange;
            if (selectedMarkupRange != null && selectedMarkupRange.IsEmpty())
            {
                //resolve problems with display-vs-markup position ambiguity by moving past
                //invisible markupon the left. This makes the element context consistent
                //with the element the caret exists within when typing text (avoids bugs like 403028)
                MoveSelectionToPoint(screenPoint);
            }

            using (CommandContextMenuDefinition contextMenuDefinition = ContextMenuForElement(element, screenPoint))
            {
                Command command = CommandContextMenu.ShowModal(CommandManager, MshtmlEditor, screenPoint, contextMenuDefinition);
                if (command != null)
                    command.PerformExecute();

            }
            return true;
        }

        public override bool IsSelectionMisspelled()
        {
            return _spellingManager.FindMisspelling(SelectedMarkupRange.Start) != null || _spellingManager.IsInIgnoredWord(SelectedMarkupRange.Start);
        }

        private CommandContextMenuDefinition ContextMenuForElement(IHTMLElement element, Point screenPoint)
        {
            bool isEditField = InlineEditField.IsWithinEditField(element);

            if ((ContentSourceManager.IsSmartContent(element) && !isEditField) || EmoticonsManager.GetEmoticon(element) != null)
            {
                SmartContentContextMenuDefinition imageContextMenu = new SmartContentContextMenuDefinition();
                return imageContextMenu;
            }
            else if (element is IHTMLImgElement)
            {
                ImageContextMenuDefinition imageContextMenu = new ImageContextMenuDefinition();
                /*MenuDefinitionEntryCommand imageDecoratorMenuEntry = new MenuDefinitionEntryCommand(components);
                imageDecoratorMenuEntry.CommandIdentifier = this._imageDataContext.DecoratorsManager.GetImageDecorator(BrightnessDecorator.Id).Command.Identifier;
                imageContextMenu.Entries.Add(imageDecoratorMenuEntry);*/
                return imageContextMenu;
            }
            else
            {
                MisspelledWordInfo wordInfo = _spellingManager.FindMisspelling(ScreenPointToMarkupPointer(screenPoint));
                CommandContextMenuDefinition minimalTextEditingCommands = new TextContextMenuDefinition(true);
                bool anchorCommandsSupported = FullyEditableRegionActive &&
                                               HTMLElementHelper.GetContainingAnchorElement(element) != null;

                // Ignore once once work in an edit field because we replace the HTML of smart content to much
                _spellingManager.IsIgnoreOnceEnabled = !isEditField;

                if (anchorCommandsSupported)
                {
                    if (null != wordInfo)
                    {
                        return MergeContextMenuDefinitions(_spellingManager.CreateSpellCheckingContextMenu(wordInfo),
                            MergeContextMenuDefinitions(new AnchorContextMenuDefinition(GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShowAllLinkOptions)), minimalTextEditingCommands));
                    }
                    else
                    {
                        return MergeContextMenuDefinitions(new AnchorContextMenuDefinition(GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShowAllLinkOptions)), minimalTextEditingCommands);
                    }
                }
                else
                {
                    if (null != wordInfo)
                    {
                        return MergeContextMenuDefinitions(_spellingManager.CreateSpellCheckingContextMenu(wordInfo), minimalTextEditingCommands);
                    }
                    else
                    {
                        if (_tableEditingManager.ShowTableContextMenuForElement(element))
                        {
                            return MergeContextMenuDefinitions(
                                _tableEditingManager.CreateTableContextMenuDefinition(), new TextContextMenuDefinition(true));
                        }
                        else
                        {
                            return new TextContextMenuDefinition();
                        }
                    }
                }
            }

        }

        private CommandContextMenuDefinition MergeContextMenuDefinitions(CommandContextMenuDefinition menu1, CommandContextMenuDefinition menu2)
        {
            // menu to return
            CommandContextMenuDefinition mergedMenu = new CommandContextMenuDefinition();

            // first menu (ensure a trailing separator)
            mergedMenu.Entries.AddRange(menu1.Entries);
            mergedMenu.Entries[mergedMenu.Entries.Count - 1].SeparatorAfter = true;

            // second menu
            mergedMenu.Entries.AddRange(menu2.Entries);

            // return the merged menu
            return mergedMenu;
        }

        #endregion

        #region Live Image Resizing

        internal event SelectedImageResizedHandler SelectedImageResized;

        internal event UpdateImageLinkHandler UpdateImageLink;

        private bool snapRectPreserveConstraints;
        private Size snapRectInitialImageSize;
        private IHTMLElement snapRectImageElement;
        private void htmlEditor_MouseUp(object o, HtmlEventArgs e)
        {
            //if there is an image that has been resized, then regenerate the image to optimize its appearance
            //for the new size.
            if (snapRectImageElement != null)
            {
                IHTMLImgElement imgElement = (IHTMLImgElement)snapRectImageElement;
                Size newSize = new Size(imgElement.width, imgElement.height);
                using (IUndoUnit undo = CreateInvisibleUndoUnit())
                {
                    using (new WaitCursor())
                    {
                        if (SelectedImageResized != null)
                            SelectedImageResized(newSize, snapRectInitialImageSize, snapRectPreserveConstraints, imgElement);
                    }
                    undo.Commit();
                }
            }
            snapRectInitialImageSize = Size.Empty;
            snapRectImageElement = null;
            snapRectPreserveConstraints = false;
        }

        private void htmlEditor_SnapRectEvent(IHTMLElement pIElement, ref RECT prcNEW, _ELEMENT_CORNER elementCorner)
        {
            IHTMLImgElement imgElement = pIElement as IHTMLImgElement;
            if (imgElement != null)
            {
                // see if we are scaling a new image
                if (snapRectImageElement == null || snapRectImageElement.sourceIndex != snapRectImageElement.sourceIndex)
                {
                    // save the image element so that mouseUp handler can regenerate the resized image.
                    snapRectImageElement = pIElement;

                    //save the initial image size so that we can scale the image based on the original size.
                    //Note: this is important to minimize rounding errors that may occurs while the image is snapping
                    //the old logic scaled based on the imgElement height and width attributes, so each snap iteration would
                    //case the image to distort more and more.
                    snapRectInitialImageSize = new Size(imgElement.width, imgElement.height);
                }

                // only preserve constraints if we are sizing a true corner
                if (IsTrueCorner(elementCorner))
                {
                    snapRectPreserveConstraints = true;
                    // get the new width and height
                    int width = prcNEW.right - prcNEW.left;
                    int height = prcNEW.bottom - prcNEW.top;

                    // scale the picture (using its current proportions!) based on the size of the snap rectangle.
                    Size constrainedSize = ImageUtils.GetScaledImageSize(width, height, snapRectInitialImageSize);
                    width = constrainedSize.Width;
                    height = constrainedSize.Height;

                    // adjust the location and size of the image
                    switch (elementCorner)
                    {
                        case _ELEMENT_CORNER.ELEMENT_CORNER_BOTTOMLEFT:
                            prcNEW.left = prcNEW.right - width;
                            prcNEW.bottom = prcNEW.top + height;
                            break;
                        case _ELEMENT_CORNER.ELEMENT_CORNER_BOTTOMRIGHT:
                            prcNEW.right = prcNEW.left + width;
                            prcNEW.bottom = prcNEW.top + height;
                            break;
                        case _ELEMENT_CORNER.ELEMENT_CORNER_TOPLEFT:
                            prcNEW.left = prcNEW.right - width;
                            prcNEW.top = prcNEW.bottom - height;
                            break;
                        case _ELEMENT_CORNER.ELEMENT_CORNER_TOPRIGHT:
                            prcNEW.right = prcNEW.left + width;
                            prcNEW.top = prcNEW.bottom - height;
                            break;
                        default:
                            Trace.Fail("Unexpected element corner: " + elementCorner.ToString());
                            break;
                    }
                }

                //Hack: unset the width and height attributes so that the MSHTMLEditor doesn't set the height/width
                //using the style attribute when it handles the snapRect.  Once its set using style, it can never be unset...
                pIElement.removeAttribute("width", 0);
                pIElement.removeAttribute("height", 0);

            }

            //suppress resizing of the the extended entry splitter
            if (pIElement.id == PostBodyEditingElementBehavior.EXTENDED_ENTRY_ID)
            {
                IHTMLElement2 e2 = (IHTMLElement2)pIElement;
                IHTMLRect rect = e2.getBoundingClientRect();
                prcNEW.top = rect.top;
                prcNEW.bottom = rect.bottom;
                prcNEW.left = rect.left;
                prcNEW.right = rect.right;
            }
        }

        private bool IsTrueCorner(_ELEMENT_CORNER elementCorner)
        {
            return elementCorner == _ELEMENT_CORNER.ELEMENT_CORNER_BOTTOMLEFT ||
                elementCorner == _ELEMENT_CORNER.ELEMENT_CORNER_BOTTOMRIGHT ||
                elementCorner == _ELEMENT_CORNER.ELEMENT_CORNER_TOPLEFT ||
                elementCorner == _ELEMENT_CORNER.ELEMENT_CORNER_TOPRIGHT;
        }

        #endregion

        #region Table Editing

        private void InitializeTableEditingManager()
        {
            _tableEditingManager = new TableEditingManager(this as IHtmlEditorComponentContext);
        }
        private TableEditingManager _tableEditingManager;

        protected override bool ContentIsDeletableForInsert(MarkupPointer start, MarkupPointer end)
        {
            // if a table element is contained within the selection then it is not deleteable for insert
            TableSelection tableSelection = new TableSelection(MarkupServices.CreateMarkupRange(start, end));
            bool contentIsDeletableForInsert = !tableSelection.HasContiguousSelection;

            // allow either our logic or the base class to veto deletion
            return contentIsDeletableForInsert && base.ContentIsDeletableForInsert(start, end);
        }

        #endregion

        #region SpellChecking

        /// <summary>
        /// Initialized the spell checking runtime for the editor.
        /// </summary>
        private void InitializeSpellChecking()
        {
            IHTMLEditorDamageServices damageServices = ((IHtmlEditorComponentContext)this).DamageServices;
            if (_spellingManager == null)
                return;

            _spellingManager.Initialize(SpellingChecker, MshtmlEditor.MshtmlControl, HTMLDocument,
                new ReplaceWord(InsertHtml), IgnoreRangeForSpellChecking, new DamageFunction(damageServices.AddDamage));

            EditingContextChanged += new EventHandler(_spellingManager.UpdateSpellingContext);
            _spellingContext.SpellingOptionsChanged += new EventHandler(spellingSettings_SpellingSettingsChanged);
            _spellingManager.InitializeSession(_spellingContext);

        }

        protected override void OnSpellCheckWordIgnored(MarkupRange range)
        {
            _spellingManager.IgnoreOnce(range);
        }

        public override bool IgnoreRangeForSpellChecking(MarkupRange range)
        {
            // Special case for inline edit fields, since they only appear in SmartContent and therefore would be
            // ignored otherwise.
            if (InlineEditField.IsWithinEditField(range.Start.CurrentScope))
            {
                return InlineEditField.IsDefaultTextShowing(range.Start.CurrentScope);
            }

            if (range.Start.GetParentElement(IgnoreElementForSpellChecking) != null)
                return true;

            return _spellingManager.IsWordIgnored(range);
        }

        private bool IgnoreElementForSpellChecking(IHTMLElement element)
        {
            if (element == null)
                return false;

            if (ContentSourceManager.IsSmartContentClass(element.className))
                return true;

            if (_titleBehavior != null && _titleBehavior.DefaultedText && PostTitleElement != null && element.id == PostTitleElement.id)
            {
                return true;
            }

            // WinLive 237279: We get an error when attempting to highlight misspelled words in <select> elements or <option> elements.
            if (element is IHTMLSelectElement || element is IHTMLOptionElement)
                return true;

            return false;
        }

        private SpellingManager _spellingManager;

        private void spellingSettings_SpellingSettingsChanged(object sender, EventArgs e)
        {
            if (MshtmlEditor.InvokeRequired)
                MshtmlEditor.BeginInvoke(new ThreadStart(refreshSpellCheckingSettings));
            else
                refreshSpellCheckingSettings();
        }

        private void refreshSpellCheckingSettings()
        {

            if (!_damageHandlerInstalled)
            {
                DamageOccured += HandleSpellingDamage;
                _damageHandlerInstalled = true;
            }

            Command spellCheckingCommand = CommandManager.Get(CommandId.CheckSpelling);

            if (spellCheckingCommand != null)
            {
                bool canSpellCheck = _spellingContext.CanSpellCheck;
                if (spellCheckingCommand.On != canSpellCheck)
                {
                    CommandManager.BeginUpdate();
                    try
                    {
                        spellCheckingCommand.On = canSpellCheck;
                        spellCheckingCommand.Enabled = canSpellCheck;
                    }
                    finally
                    {
                        CommandManager.EndUpdate(true);
                    }
                }
            }

            if (_spellingContext.CanSpellCheck && SpellingSettings.RealTimeSpellChecking && Editable)
            {
                //(re)initialize spelling squiggles for the new document
                MshtmlEditor.BeginInvoke(new ThreadStart(() => StartRealTimeSpellChecking(true)));
            }
            else
            {
                StopRealTimeSpellChecking();
            }

            _keyBoardHandler.SetAutoCorrectFile(_spellingContext.AutoCorrectLexiconFilePath);
        }

        private bool _realTimeSpellCheckingStarted = false;
        private bool _damageHandlerInstalled = false;
        /// <summary>
        /// Turns on realtime spellchecking.
        /// </summary>
        /// <param name="forceSpellingScan"></param>
        private void StartRealTimeSpellChecking(bool forceSpellingScan)
        {
            if (!_realTimeSpellCheckingStarted)
            {
                _realTimeSpellCheckingStarted = true;
            }

            if (forceSpellingScan)
                _spellingManager.StartSession();
        }

        /// <summary>
        /// Turns off realtime-spellchecking
        /// </summary>
        private void StopRealTimeSpellChecking()
        {
            if (_realTimeSpellCheckingStarted)
            {
                _realTimeSpellCheckingStarted = false;
                _spellingManager.StopSession(false);
            }
        }

        public void SuspendSpellChecking()
        {
            StopRealTimeSpellChecking();
        }

        public void ResumeSpellChecking()
        {
            if (_spellingContext.CanSpellCheck)
            {
                StartRealTimeSpellChecking(true);
            }
        }

        protected void HandleSpellingDamage(object source, DamageEvent evt)
        {
            if (_spellingContext.CanSpellCheck)
            {
                foreach (MarkupRange range in evt.DamageRegions)
                {
                    if (!range.Positioned)
                        continue;

                    foreach (MarkupRange targetRange in IntersectWithEditableElements(range))
                        _spellingManager.DamagedRange(targetRange, _realTimeSpellCheckingStarted);
                }
            }
        }

        private MarkupPointer ScreenPointToMarkupPointer(Point screenPoint)
        {
            MarkupPointer markupPointer = MarkupServices.CreateMarkupPointer();
            IDisplayPointerRaw displayPointer;
            MshtmlEditor.MshtmlControl.DisplayServices.CreateDisplayPointer(out displayPointer);
            Point clientPoint = EditorControl.PointToClient(screenPoint);

            MoveDisplayPointerToClientPoint(displayPointer, clientPoint);

            displayPointer.PositionMarkupPointer(markupPointer.PointerRaw);
            return markupPointer;
        }

        /// <summary>
        /// Implements a specialized damage commit strategy for the inline spellchecking.
        /// Spellchecking generally utilizes the word-based commit strategy, but when editing
        /// a word that is already marked as mispelled, then the realtime commit strategy needs
        /// to be used.
        /// </summary>
        class SpellCheckingDamageCommitStrategy : DamageCommitStrategy
        {
            private RealtimeDamageCommitStrategy _realtimeCommitStrategy;
            private WordBasedDamageCommitStrategy _wordBasedCommitStrategy;
            private DamageCommitStrategy _currentCommitStrategy;
            private BlogPostHtmlEditorControl _editor;

            public SpellCheckingDamageCommitStrategy(BlogPostHtmlEditorControl editor)
            {
                _wordBasedCommitStrategy = new WordBasedDamageCommitStrategy(editor);
                _realtimeCommitStrategy = new RealtimeDamageCommitStrategy();
                _realtimeCommitStrategy.CommitDamage += new EventHandler(_currentCommitStrategy_CommitDamage);
                _wordBasedCommitStrategy.CommitDamage += new EventHandler(_currentCommitStrategy_CommitDamage);
                CurrentCommitStrategy = _wordBasedCommitStrategy;
                _editor = editor;
            }

            private DamageCommitStrategy CurrentCommitStrategy
            {
                get { return _currentCommitStrategy; }
                set
                {
                    if (_currentCommitStrategy != value)
                    {
                        _currentCommitStrategy = value;
                    }
                }
            }

            public override void OnKeyDown(HtmlEventArgs e)
            {
                CurrentCommitStrategy.OnKeyDown(e);
            }

            public override void OnKeyPress(HtmlEventArgs e)
            {
                CurrentCommitStrategy.OnKeyPress(e);
                char ch = (char)e.htmlEvt.keyCode;
                if (CurrentCommitStrategy == _realtimeCommitStrategy)
                {
                    if (!Char.IsLetterOrDigit(ch) || !ShouldUseRealtimeDamageCommitStrategy())
                    {
                        //the caret position moved onto a new word, so revoke realtime spelling strategy
                        CurrentCommitStrategy = _wordBasedCommitStrategy;
                    }
                }
            }

            private bool ShouldUseRealtimeDamageCommitStrategy()
            {
                return _editor.IsSelectionMisspelled();
            }

            public override void OnSelectionChange()
            {
                CurrentCommitStrategy.OnSelectionChange();
                UpdateCommitStrategyForCurrentWord();
            }

            private void UpdateCommitStrategyForCurrentWord()
            {
                //if the current word is misspelled, switch commit to realtime.
                if (ShouldUseRealtimeDamageCommitStrategy())
                {
                    CurrentCommitStrategy = _realtimeCommitStrategy;
                }
                else
                {
                    CurrentCommitStrategy = _wordBasedCommitStrategy;
                }
            }

            public override void OnDelete()
            {
                CurrentCommitStrategy.OnDelete();
            }

            public override void OnDamaged()
            {
                CurrentCommitStrategy.OnDamaged();
            }

            private void _currentCommitStrategy_CommitDamage(object sender, EventArgs e)
            {
                OnCommitDamage(e);
            }
        }

        #endregion

        protected override bool CanIndent
        {
            get
            {
                if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.TabAsIndent))
                    return base.CanIndent;

                //indent/outdent is implemented using blockquote for blogs, and we only allow one level
                IHtmlEditorCommandSource commandSource = this;
                return commandSource.CanApplyFormatting(null) && !commandSource.SelectionBlockquoted;
            }
        }

        private PostTitleEditingElementBehavior TitleBehavior
        {
            get
            {
                Debug.Assert(_templateContainsTitle, "TitleBehavior should not be called if template does not contain a title.");
                Debug.Assert(_titleBehavior != null, "Illegal State! behaviors are not attached");
                return _titleBehavior;
            }
        }

        private PostBodyEditingElementBehavior BodyBehavior
        {
            get
            {
                Debug.Assert(_bodyBehavior != null, "Illegal State! behaviors are not attached");
                return _bodyBehavior;
            }
        }

        protected override void ApplyIndent()
        {
            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.TabAsIndent))
            {
                base.ApplyIndent();
                return;
            }

            IHtmlEditorCommandSource commandSource = this;
            commandSource.ApplyBlockquote();
        }

        public override bool IsRTLTemplate
        {
            get
            {
                return _imageEditingContext.EditorOptions.IsRTLTemplate;
            }
        }

        private string _postBodyInlineStyle;
        public string PostBodyInlineStyle
        {
            get { return _postBodyInlineStyle; }
            set { _postBodyInlineStyle = value; }
        }

        public override string FixImageReferences(string html, string sourceUrl)
        {
            if (_referenceFixer == null || String.IsNullOrEmpty(sourceUrl))
                return html;

            return _referenceFixer.FixImageReferences(html, sourceUrl);
        }

        private PostTitleEditingElementBehavior _titleBehavior;
        private PostBodyEditingElementBehavior _bodyBehavior;
        private string _lastSetTitle = "";
        private string _lastSetBodyHtml = "";

        public abstract class TemplateStrategy
        {
            protected readonly string BODY_FRAGMENT_ID = Guid.NewGuid().ToString();
            protected readonly string TITLE_FRAGMENT_ID = Guid.NewGuid().ToString();
            public abstract string OnBodyInserted(string bodyContents);
            public abstract string OnTitleInserted(string title);
            public abstract void OnDocumentComplete(IHTMLDocument2 doc);
            public abstract IHTMLElement PostBodyElement(IHTMLDocument2 doc);
            public abstract IHTMLElement TitleElement(IHTMLDocument2 doc);
        }

    }

    internal delegate void SelectedImageResizedHandler(Size newSize, Size originalSize, bool preserveRatio, IHTMLImgElement image);

    internal delegate void UpdateImageLinkHandler(string newLink, string title, bool newWindow, string rel);

}
