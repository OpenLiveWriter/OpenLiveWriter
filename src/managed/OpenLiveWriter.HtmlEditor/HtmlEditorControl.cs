// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using mshtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.HtmlEditor.Marshalling;
using OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers;
using OpenLiveWriter.HtmlEditor.Undo;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.Mshtml.Mshtml_Interop;
//using OpenLiveWriter.SpellChecker;
using IDataObject = System.Windows.Forms.IDataObject;

namespace OpenLiveWriter.HtmlEditor
{
    public abstract class HtmlEditorControl : IHtmlEditor, IHtmlEditorCommandSource, IHtmlEditorComponentContext, IHtmlMarshallingTarget, IHTMLEditHostRaw, IElementBehaviorFactoryRaw, IServiceProviderRaw, IWordBasedEditor, IDisposable
    {
        #region Construction/Disposal
        public HtmlEditorControl(IMainFrameWindow mainFrameWindow, IStatusBar statusBar, MshtmlOptions options, IInternetSecurityManager internetSecurityManager, CommandManager commandManager)
        {
            _commandManager = commandManager;

            // save reference to main frame window
            _mainFrameWindow = mainFrameWindow;
            _statusBar = statusBar;

            //ToDo: OLW Spell Checker
            //_spellingChecker = spellingChecker;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //initialize the data format handlers for this control
            DataFormatHandlerFactory = new HtmlEditorMarshallingHandler(this);
            MarshalImagesSupported = true;
            MarshalFilesSupported = true;
            MarshalUrlSupported = true;
            MarshalHtmlSupported = true;
            MarshalTextSupported = true;

            // The version host service provider tells MSHTML what feature versions are available (e.g. VML 1.0).
            VersionHostServiceProvider = new VersionHostServiceProvider(new DisableVmlVersionHost());

            // initialize the html editor
            if (_editorCache == null)
            {
                _internetSecurityManager = new InternetSecurityManagerShim(internetSecurityManager);
                // If mainFrameWindow == null, then we are pre-caching mshtml and don't want it to steal focus
                _mshtmlEditor = new MshtmlEditor(this, options, (mainFrameWindow == null));
            }
            else
            {
                _mshtmlEditor = _editorCache.Editor;
                _internetSecurityManager = _editorCache.SecurityManager;
                _internetSecurityManager.SecurityManager = internetSecurityManager;

                _editorCache = null;
                _mshtmlEditor.Active = true;
                _mshtmlEditor.MshtmlControl.ProtectFocus = false;
                _mshtmlEditor.ClearContextMenuHandlers();
                _mshtmlEditor.SetServiceProvider(this);
                _mshtmlEditor.UpdateOptions(options, true);
            }

            _mshtmlOptions = options;
            this.DefaultBlockElement = _mshtmlOptions.UseDivForCarriageReturn
                                           ? (DefaultBlockElement)new DivDefaultBlockElement()
                                           : new ParagraphDefaultBlockElement();

            PostEditorEvent += new MshtmlEditor.EditDesignerEventHandler(HtmlEditorControl_PostEditorEvent);
            HandleClear += new HtmlEditorSelectionOperationEventHandler(TryMoveIntoNextTable);

            // Hook the editor into the stream of the security manager so we
            // can allow our own objects (smart content, image resizing) to load in the editor
            _internetSecurityManager.HandleProcessUrlAction = HandleProcessUrlAction;

            //  Automation uses this to find the editor to automate it
            _mshtmlEditor.Name = "BorderControl";

            // subscribe to key events
            _mshtmlEditor.DocumentComplete += new EventHandler(_mshtmlEditor_DocumentComplete);
            _mshtmlEditor.DocumentEvents.GotFocus += htmlEditor_GotFocus;
            _mshtmlEditor.DocumentEvents.LostFocus += htmlEditor_LostFocus;
            _mshtmlEditor.DocumentEvents.KeyDown += new HtmlEventHandler(DocumentEvents_KeyDown);
            _mshtmlEditor.DocumentEvents.KeyUp += new HtmlEventHandler(DocumentEvents_KeyUp);
            _mshtmlEditor.DocumentEvents.KeyPress += new HtmlEventHandler(DocumentEvents_KeyPress);
            _mshtmlEditor.DocumentEvents.MouseDown += new HtmlEventHandler(DocumentEvents_MouseDown);
            _mshtmlEditor.DocumentEvents.MouseUp += new HtmlEventHandler(DocumentEvents_MouseUp);
            _mshtmlEditor.DocumentEvents.SelectionChanged += new EventHandler(DocumentEvents_SelectionChanged);
            _mshtmlEditor.DisplayChanged += new EventHandler(_mshtmlEditor_DisplayChanged);
            _mshtmlEditor.DocumentEvents.Click += new HtmlEventHandler(DocumentEvents_Click);
            _mshtmlEditor.CommandKey += new KeyEventHandler(_mshtmlEditor_CommandKey);
            _mshtmlEditor.DropTargetHandler = new MshtmlEditor.DropTargetUIHandler(_mshtmlEditor_GetDropTarget);
            _mshtmlEditor.BeforeShowContextMenu += new EventHandler(_mshtmlEditor_BeforeShowContextMenu);
            _mshtmlEditor.MshtmlControl.DLControlFlagsChanged += new EventHandler(_mshtmlControl_DLControlFlagsChanged);
            _mshtmlEditor.TranslateAccelerator += new HtmlEditDesignerEventHandler(_mshtmlEditor_TranslateAccelerator);

            InitDamageServices();

            // notify subclasses that the editor has been created
            OnEditorCreated();
        }

        void HtmlEditorControl_PostEditorEvent(object sender, MshtmlEditor.EditDesignerEventArgs args)
        {
            if (Editable && args.EventDispId == DISPID_HTMLELEMENTEVENTS2.ONKEYPRESS && SelectedMarkupRange.Positioned)
            {
                switch (args.EventObj.keyCode)
                {
                    case (int)Keys.Enter:
                        // WinLive 245925: Because we inline some CSS into the font tag (specifically, the font-size
                        // property), we need to make sure that when the user hits enter that the font-size persists
                        // onto the next line. MSHTML does not handle this for us.
                        MarkupRange currentSelection = SelectedMarkupRange.Clone();
                        currentSelection.Start.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Left);
                        currentSelection.End.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);

                        try
                        {
                            if (_fontSizeBeforeEnter != null)
                            {
                                using (IUndoUnit undoUnit = CreateInvisibleUndoUnit())
                                {
                                    // Apply the previous block's font size to this new block.
                                    var fontSizeTextStyle = new FontSizeTextStyle(_fontSizeBeforeEnter.Value);
                                    fontSizeTextStyle.Apply(MarkupServices, currentSelection, _mshtmlEditor.Commands);

                                    // Force MSHTML to re-select inside the font tag we just created.
                                    IHTMLElement fontElement = currentSelection.Start.SeekElementRight(_fontTagWithFontSizeFilter, currentSelection.End);
                                    if (fontElement != null)
                                    {
                                        MarkupServices.CreateMarkupRange(fontElement, false).ToTextRange().select();
                                    }
                                    else
                                    {
                                        Debug.Fail("Didn't find font tag to reselect!");
                                        OnSelectionChanged(EventArgs.Empty, _selection, false);
                                    }

                                    undoUnit.Commit();
                                }
                            }
                            else
                            {
                                OnSelectionChanged(EventArgs.Empty, _selection, false);
                            }
                        }
                        finally
                        {
                            _fontSizeBeforeEnter = null;
                        }
                        break;
                    case (int)Keys.Back:
                        // Bug 101165: MSHTML maintains internal state about font tag. After forcibly clearing the
                        // font backcolor, MSHTML will often insert an empty font tag on the next keystroke. This
                        // font tag can then mutate into <font size="+0">, which causes the font to be rendered
                        // incorrectly.
                        GetMshtmlCommand(IDM.BACKCOLOR).Execute(null);
                        _backColorWasReset = true;
                        break;
                    default:
                        if (_backColorWasReset)
                        {
                            using (IUndoUnit undoUnit = CreateInvisibleUndoUnit())
                            {
                                // Remove any empty font tags.
                                SelectedMarkupRange.RemoveElementsByTagId(_ELEMENT_TAG_ID.TAGID_FONT, true);
                                undoUnit.Commit();
                            }
                            _backColorWasReset = false;
                        }
                        break;
                }
            }
        }

        private bool _backColorWasReset;

        private bool HandleProcessUrlAction(string pwszUrl, int dwAction, out byte pPolicy, int cbPolicy, IntPtr pContext, int cbContext, int dwFlags, int dwReserved)
        {
            // We only give sub classes a chance at to set loading policy
            // when it is a binary behavior in question, we don't allow
            // them to approve other objects like activex and java
            if (dwAction == URLACTION.BEHAVIOR_RUN)
            {
                if (AssignBehaviorPolicy(Marshal.PtrToStringUni(pContext) /* html identifier of the behavior */, out pPolicy))
                    return true;
            }

            pPolicy = new byte();
            return false;
        }

        /// <summary>
        /// Gives the sub classes a chance to allow binary behaviors a chance
        /// to set the security policy that will be returned to mshtml
        ///
        /// This will be needed by BlogPostHtmlEditor so it can set an allow
        /// policy for the bevhaiors that the canvas defines
        /// </summary>
        /// <param name="behavior"></param>
        /// <param name="pPolicy"></param>
        /// <returns></returns>
        protected virtual bool AssignBehaviorPolicy(string behavior, out byte pPolicy)
        {
            pPolicy = new byte();
            return false;
        }

        public bool TrackKeyboardLanguageChanges
        {
            get { return _mshtmlEditor.TrackKeyboardLanguageChanges; }
            set { _mshtmlEditor.TrackKeyboardLanguageChanges = value; }
        }

        public event EventHandler KeyboardLanguageChanged
        {
            add { _mshtmlEditor.KeyboardLanguageChanged += value; }
            remove { _mshtmlEditor.KeyboardLanguageChanged -= value; }
        }

        private RtlAcceleratorTranslator rtlAcceleratorTranslator = new RtlAcceleratorTranslator();

        int _mshtmlEditor_TranslateAccelerator(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            KeyEventArgs e = rtlAcceleratorTranslator.ProcessEvent(inEvtDispId, pIEventObj);
            if (e != null)
            {
                if (!e.SuppressKeyPress)
                {
                    OnCommandKey(e);
                }

                return HRESULT.S_OK;
            }

            return HRESULT.S_FALSE;
        }

        /// <summary>
        /// Notification that the editor has been created (do one-time configuration and event subscription here)
        /// </summary>
        protected virtual void OnEditorCreated() { }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public virtual void Dispose()
        {
            _internetSecurityManager.ReleaseInnerSecurityManager();

            PostEditorEvent -= new MshtmlEditor.EditDesignerEventHandler(HtmlEditorControl_PostEditorEvent);
            HandleClear -= new HtmlEditorSelectionOperationEventHandler(TryMoveIntoNextTable);

            if (components != null)
            {
                components.Dispose();
            }

            // detach behaviors
            DetachBehaviors();

            // dispose drag and drop manager
            if (mshtmlEditorDragAndDropTarget != null)
            {
                mshtmlEditorDragAndDropTarget.Dispose();
                mshtmlEditorDragAndDropTarget = null;
            }

            if (_dataFormatHandlerFactory != null)
            {
                _dataFormatHandlerFactory.Dispose();
                _dataFormatHandlerFactory = null;
            }

            if (_damageServices != null)
                _damageServices.Dispose();

            // dispose link navigator
            if (linkNavigator != null)
                linkNavigator.Dispose();

            if (_mshtmlEditor != null)
            {
                _mshtmlEditor.DocumentComplete -= new EventHandler(_mshtmlEditor_DocumentComplete);
                _mshtmlEditor.DocumentEvents.GotFocus -= htmlEditor_GotFocus;
                _mshtmlEditor.DocumentEvents.LostFocus -= htmlEditor_LostFocus;
                _mshtmlEditor.DocumentEvents.KeyDown -= new HtmlEventHandler(DocumentEvents_KeyDown);
                _mshtmlEditor.DocumentEvents.KeyUp -= new HtmlEventHandler(DocumentEvents_KeyUp);
                _mshtmlEditor.DocumentEvents.KeyPress -= new HtmlEventHandler(DocumentEvents_KeyPress);
                _mshtmlEditor.DocumentEvents.MouseDown -= new HtmlEventHandler(DocumentEvents_MouseDown);
                _mshtmlEditor.DocumentEvents.MouseUp -= new HtmlEventHandler(DocumentEvents_MouseUp);
                _mshtmlEditor.DocumentEvents.SelectionChanged -= new EventHandler(DocumentEvents_SelectionChanged);
                _mshtmlEditor.DisplayChanged -= new EventHandler(_mshtmlEditor_DisplayChanged);
                _mshtmlEditor.DocumentEvents.Click -= new HtmlEventHandler(DocumentEvents_Click);
                _mshtmlEditor.CommandKey -= new KeyEventHandler(_mshtmlEditor_CommandKey);
                _mshtmlEditor.BeforeShowContextMenu -= new EventHandler(_mshtmlEditor_BeforeShowContextMenu);
                _mshtmlEditor.DropTargetHandler = null;
                _mshtmlEditor.PreHandleEvent -= new HtmlEditDesignerEventHandler(OnPreHandleEvent);
                _mshtmlEditor.MshtmlControl.DLControlFlagsChanged -= new EventHandler(_mshtmlControl_DLControlFlagsChanged);
                _mshtmlEditor.TranslateAccelerator -= new HtmlEditDesignerEventHandler(_mshtmlEditor_TranslateAccelerator);

                if (ShouldCacheEditor())
                {
                    _mshtmlEditor.Active = false;
                    _editorCache = new CachedEditorAndSecurityManager { Editor = _mshtmlEditor, SecurityManager = _internetSecurityManager };
                }
                else
                {
                    _mshtmlEditor.Dispose();
                    _mshtmlEditor = null;
                }
            }

        }

        private static bool ShouldCacheEditor()
        {
            return _allowCachedEditor && _editorCache == null;
        }

        public static void DisposeCachedEditor()
        {
            if (_editorCache != null)
            {
                MshtmlEditor tempEditor = _editorCache.Editor;
                InternetSecurityManagerShim tempSecMan = _editorCache.SecurityManager;
                _editorCache = null;
                tempEditor.Dispose();
                tempSecMan.ReleaseInnerSecurityManager();
            }
        }

        private static bool _allowCachedEditor = false;
        public static void AllowCachedEditor()
        {
            _allowCachedEditor = true;
        }

        private void InitDamageServices()
        {
            _damageServices = new HtmlEditorControlDamageServices(this, MshtmlEditor, CreateDamageCommitStrategy());
            //_damageServices.DamageOccured += new DamageListener(new DamageTracer().HandleDamageOccured);
        }

        protected virtual DamageCommitStrategy CreateDamageCommitStrategy()
        {
            return new WordBasedDamageCommitStrategy(this);
        }

        #endregion

        #region Public Interface

        public event MshtmlEditor.EditDesignerEventHandler PostEditorEvent
        {
            add { _mshtmlEditor.PostEditorEvent += value; }
            remove { _mshtmlEditor.PostEditorEvent -= value; }
        }

        /// <summary>
        /// Register a context menu handler
        /// </summary>
        /// <param name="contextMenuHandler">handler to register</param>
        public void RegisterContextMenuHandler(ShowContextMenuHandler contextMenuHandler)
        {
            _mshtmlEditor.RegisterContextMenuHandler(contextMenuHandler);
        }

        public IDataFormatHandlerFactory DataFormatHandlerFactory
        {
            get
            {
                return _dataFormatHandlerFactory;
            }
            set
            {
                _dataFormatHandlerFactory = value;

                //dispose the existing drag and drop target
                if (mshtmlEditorDragAndDropTarget != null)
                    mshtmlEditorDragAndDropTarget.Dispose();

                //initialize the drag drop targetting for this control
                mshtmlEditorDragAndDropTarget = new MshtmlEditorDragAndDropTarget(this, _dataFormatHandlerFactory);
            }
        }
        private IDataFormatHandlerFactory _dataFormatHandlerFactory;

        /// <summary>
        /// Save the file that was previously loaded from
        /// </summary>
        public virtual void SaveFile()
        {
            _mshtmlEditor.SaveToFile();
        }

        public void PageSetup()
        {
            GetMshtmlCommand(IDM.PAGESETUP).Execute();
        }

        public void Print()
        {
            GetMshtmlCommand(IDM.PRINT).Execute();
        }

        public void PrintPreview()
        {
            GetMshtmlCommand(IDM.PRINTPREVIEW).Execute();
        }

        public bool CanPrint
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the Html generation service.
        /// </summary>
        public IHtmlGenerationService HtmlGenerationService
        {
            get
            {
                if (htmlGenerationService == null)
                {
                    htmlGenerationService = new BasicHtmlGenerationService(this.DefaultBlockElement);
                }
                return htmlGenerationService;
            }
            set
            {
                htmlGenerationService = value;
            }
        }
        private IHtmlGenerationService htmlGenerationService;

        public virtual string FixImageReferences(string html, string sourceUrl)
        {
            return html;
        }

        public event EventHandler HtmlInserted;
        protected virtual void OnHtmlInserted(EventArgs e)
        {
            if (HtmlInserted != null)
                HtmlInserted(this, e);
        }

        /// <summary>
        /// Occurs for command key processing.
        /// </summary>
        [
        Category("Action"),
        Description("")
        ]
        public event KeyEventHandler CommandKey;

        /// <summary>
        /// Raises the CommandKey event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        protected virtual void OnCommandKey(KeyEventArgs e)
        {
            // @SharedCanvas - if shortcut keys dont work, look here 2nd
            e.Handled = CommandManager.ProcessCmdKeyShortcut(e.KeyData);
            if (!e.Handled)
            {
                //if the command was not handled, but there was a disabled command configured we
                //need to eat it so that the underlying editor doesn't try to handle this command
                //using its default implementation (such as for Ctrl+Z triggering undo!).
                Command command = CommandManager.FindCommandWithShortcut(e.KeyData);
                if (command != null && command.On)
                {
                    e.Handled = true;
                }
                else
                {
                    if (CommandKey != null)
                        CommandKey(this, e);
                }
            }

            //Hack: the Enter key affects the DOM in a way that visually changes the selection, but doesn't
            //affect the DOM selection pointer location, so no selection change event is fired.  This hack always
            //forces the selection changed event after the enter key is pressed.  This is especially necessary when
            //the editor incorrectly allows the caret to be visually positioned at the end of a block element even
            //though the markupPointer is actually positioned AfterEnd of the block element (this typically happens
            //after a backspace command).  When the enter key is pressed from this caret position, the DOM will
            //automatically reposition the caret inside the block element, but without firing a selection changed
            //event.  This is especially evil when there is a behavior attached to the block element, because
            //it will not know that the selection caret has been placed inside of the behavior's element, so
            //it will not update is selected state.
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    FireSelectionChanged();
                    break;
                case Keys.Back:
                    if (ApplicationPerformance.IsEnabled)
                    {
                        ApplicationPerformance.StartEvent("Backspace");
                    }
                    break;
                // WinLive 252760 - It is possible to create a selection on the screen that persists even when you
                // create a new selection, navigate around, etc.  This phantom selection is not properly reflected
                // in the various variable regarding selection range, but is corrected when the complete mshmtl
                // control is forced to refresh due to an invalidation. (Note: this bug was only listed for mail,
                // but could also be reproed in writer using a slightly different situation)
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    if (e.Shift == false)
                    {
                        _mshtmlEditor.MshtmlControl.Invalidate(true);
                    }
                    break;
                default:
                    break;
            }
        }

        public event HtmlEventHandler KeyDown;
        protected virtual void OnKeyDown(HtmlEventArgs evt)
        {
            if (KeyDown != null)
            {
                foreach (HtmlEventHandler handler in KeyDown.GetInvocationList())
                {
                    handler(this, evt);

                    if (evt.WasCancelled)
                        break;
                }
            }
        }

        public event HtmlEventHandler KeyUp;
        protected virtual void OnKeyUp(HtmlEventArgs evt)
        {
            if (KeyUp != null)
            {
                foreach (HtmlEventHandler handler in KeyUp.GetInvocationList())
                {
                    handler(this, evt);

                    if (evt.WasCancelled)
                        break;
                }
            }
        }

        public event HtmlEventHandler KeyPress;
        protected virtual void OnKeyPress(HtmlEventArgs evt)
        {
            if (KeyPress != null)
            {
                foreach (HtmlEventHandler handler in KeyPress.GetInvocationList())
                {
                    handler(this, evt);

                    if (evt.WasCancelled)
                        break;
                }
            }
        }

        /// <summary>
        /// Provide the ability to process clipboard Copy
        /// </summary>
        public event HtmlEditorSelectionOperationEventHandler HandleCopy;
        protected virtual void OnCopy(HtmlEditorSelectionOperationEventArgs ea)
        {
            if (HandleCopy != null)
            {
                foreach (HtmlEditorSelectionOperationEventHandler handler in HandleCopy.GetInvocationList())
                {
                    handler(ea);

                    if (ea.Handled)
                        break;
                }
            }
        }

        /// <summary>
        /// Provide the ability to process clipboard Cut
        /// </summary>
        ///
        public event HtmlEditorSelectionOperationEventHandler HandleCut;
        protected virtual void OnCut(HtmlEditorSelectionOperationEventArgs ea)
        {
            if (HandleCut != null)
            {
                foreach (HtmlEditorSelectionOperationEventHandler handler in HandleCut.GetInvocationList())
                {
                    handler(ea);

                    if (ea.Handled)
                        break;
                }
            }
        }

        /// <summary>
        /// Provide the ability to process clipboard Clear
        /// </summary>
        public event HtmlEditorSelectionOperationEventHandler HandleClear;
        protected virtual void OnClear(HtmlEditorSelectionOperationEventArgs ea)
        {
            if (HandleClear != null)
            {
                foreach (HtmlEditorSelectionOperationEventHandler handler in HandleClear.GetInvocationList())
                {
                    handler(ea);

                    if (ea.Handled)
                        break;
                }
            }
        }

        protected virtual bool StopTryMoveIntoNextTable(IHTMLElement e)
        {
            return false;
        }

        /// <summary>
        /// WinLive 225587: If the user hits delete when the cursor is just before a table, we want to move the
        /// content at the cursor into the first table cell. This behavior aligns us with Word 2010.
        /// </summary>
        private void TryMoveIntoNextTable(HtmlEditorSelectionOperationEventArgs ea)
        {
            if (!SelectedMarkupRange.IsEmpty() || IsEditFieldSelected)
            {
                return;
            }

            IHTMLElement nextTable = null;
            bool enteredABlockElement = false;
            int numBlockElementsBetweenSelectionAndNextTable = 0;
            IHTMLElement brElement = null;

            MarkupRange postBodyRange = MarkupServices.CreateMarkupRange(PostBodyElement, false);
            postBodyRange.Start.MoveToPointer(SelectedMarkupRange.Start);
            postBodyRange.WalkRange(
                delegate (MarkupRange currentRange, MarkupContext context, string text)
                {
                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text ||
                        context.Element == null)
                    {
                        // There is text between the cursor position and the next table. Stop processing and quit.
                        return false;
                    }

                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope &&
                        ElementFilters.IsVisibleEmptyElement(context.Element))
                    {
                        if (context.Element is IHTMLBRElement && brElement == null)
                        {
                            // Allow up to one <br> element to be between the cursor position and the next table as
                            // long as its in our parent block element, since it won't cause a line break, e.g.:
                            //  <div>Hello[cursor]<br></div><table>...</table>
                            //  <table><tr><td>Hello[cursor]<br><table>...</table></td></tr></table>
                            brElement = context.Element;
                            return true;
                        }

                        // There is content between the cursor position and the next table. Stop processing and quit.
                        return false;
                    }

                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                    {
                        if (context.Element is IHTMLTable)
                        {
                            if (StopTryMoveIntoNextTable(context.Element))
                                return false;

                            // We found the next table so we can stop walking the range.
                            nextTable = context.Element;
                            return false;
                        }

                        if (ElementFilters.IsBlockElement(context.Element) || ElementFilters.IsListElement(context.Element))
                        {
                            // Set a flag so that we know that since starting at the cursor position, we've walked into
                            // a block element, e.g.:
                            //  <div>Hello[cursor]<div>[currentPosition]</div></div>
                            //  <ul><li>Hello[cursor]</li><li>[currentPosition]</li></ul>
                            enteredABlockElement = true;
                        }
                    }

                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                    {
                        if (ElementFilters.IsBlockElement(context.Element) || ElementFilters.IsListElement(context.Element))
                        {
                            if (enteredABlockElement)
                            {
                                // We entered and exited a block element without finding a table, e.g.:
                                //  <div>Hello[cursor]<div></div>[currentPosition]</div>
                                //  <ul><li>Hello[cursor]</li><li></li>[currentPosition]</ul>
                                // Stop processing and quit.
                                return false;
                            }
                            else
                            {
                                // We count up the number of block elements we've exited so we know what parent to
                                // remove later, e.g.:
                                //  <div><div><div>Hello[cursor]</div></div>[currentPosition]</div><table>...</table>
                                numBlockElementsBetweenSelectionAndNextTable++;
                            }
                        }
                        else if (ElementFilters.IsTableCellElement(context.Element))
                        {
                            // We exited a table cell element without finding a table. Stop processing and quit.
                            //  <table><tr><td>Hello[cursor]</td>[currPosition]</tr></table>
                            return false;
                        }
                    }

                    // Keep walking the range.
                    return true;
                }, false);

            if (nextTable == null)
            {
                return;
            }

            // First, let's figure out what element the current selection is in.
            IHTMLElement parentBlockElement = SelectedMarkupRange.ParentElement(
                e => ElementFilters.IsListItemElement(e) || ElementFilters.IsTableCellElement(e) ||
                    ((ElementFilters.IsBlockElement(e) || ElementFilters.IsListElement(e)) && --numBlockElementsBetweenSelectionAndNextTable <= 0));

            // This represents the range of markup that we should move into the table.
            MarkupRange currentBlockRange = MarkupServices.CreateMarkupRange(parentBlockElement, false);

            using (IUndoUnit undoUnit = CreateUndoUnit())
            {
                if (brElement != null)
                {
                    if (!currentBlockRange.InRange(brElement))
                    {
                        // There's a <br> causing a line break between the cursor and the next table, so we won't try to move anything.
                        return;
                    }
                    else
                    {
                        // The <br> would cause a line break if we moved it into the <table>, e.g.:
                        //  <div>Hello<br></div><table><tr><td></td></tr></table> - if we moved "Hello<br>" into the
                        //  <td>, the <br> would now cause a line break (although it did not when wrapped in a <div>).
                        HTMLElementHelper.RemoveElement(brElement);
                    }
                }

                // In the case of <div>Some text...|<table>...</table>...</div> - we need to be careful not to remove the <div>.
                if (currentBlockRange.InRange(nextTable))
                {
                    currentBlockRange.Start.MoveAdjacentToElement(parentBlockElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    currentBlockRange.End.MoveAdjacentToElement(nextTable, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);

                    if (ElementFilters.IsTableCellElement(parentBlockElement))
                    {
                        // We use <br>s for line breaks in tables.
                        IHTMLElement[] brElements = currentBlockRange.GetElements(e => e is IHTMLBRElement, true);
                        if (brElements.Length > 0)
                        {
                            // We'll only move the last line, e.g.:
                            //  <table><tr><td>Hello<br>World<table>...<table></td></tr></table> - only move "World" into the nested table.
                            currentBlockRange.Start.MoveAdjacentToElement(brElements[brElements.Length - 1], _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                        }
                    }

                    parentBlockElement = null;
                }
                else
                {
                    // Fix up the range to only contain inline elements and text, e.g.:
                    //  <div><div><div><em>Hello</em></div></div></div><table>...</table> - we'll only move "<em>Hello</em>" into the table.
                    currentBlockRange.SelectInner();
                    MarkupPointerMoveHelper.MoveUnitBounded(currentBlockRange.Start, MarkupPointerMoveHelper.MoveDirection.LEFT,
                        MarkupPointerAdjacency.BeforeExitBlock, parentBlockElement);
                    MarkupPointerMoveHelper.MoveUnitBounded(currentBlockRange.End, MarkupPointerMoveHelper.MoveDirection.RIGHT,
                        MarkupPointerAdjacency.BeforeExitBlock, parentBlockElement);
                }

                // We're ready to move content into the <table> we found, now find the first <td> inside of it.
                MarkupPointer p = MarkupServices.CreateMarkupPointer(nextTable, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                IHTMLElement firstTd = p.SeekElementRight(e => e is IHTMLTableCell,
                    MarkupServices.CreateMarkupPointer(nextTable, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd));
                if (firstTd != null)
                {
                    if (MarkupServices.CreateMarkupRange(firstTd, false).IsEmptyOfContent())
                    {
                        // The table behavior leaves an empty space in the table, so make sure we remove it before inserting.
                        firstTd.innerHTML = String.Empty;
                    }

                    // Move the content at the cursor into the <td>.
                    MarkupPointer afterBeginFirstTd = MarkupServices.CreateMarkupPointer(firstTd, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    afterBeginFirstTd.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
                    MarkupServices.Move(currentBlockRange.Start, currentBlockRange.End, afterBeginFirstTd);

                    // Remove the original container around the cursor if necessary, e.g.:
                    //  <p>Hello[cursor]</p><table><tr><td></td></tr></table> should become =>
                    //  <table><tr><td>Hello[cursor]</td></tr></table>
                    if (parentBlockElement != null)
                    {
                        if (ElementFilters.IsListItemElement(parentBlockElement))
                        {
                            // If this is the only list item in the list, then we want to remove the list too, e.g.:
                            //  <ul><li>Hello[cursor]</li></ul><table><tr><td></td></tr></table> should become =>
                            //  <table><tr><td>Hello[cursor]</td></tr></table>
                            IHTMLElement listElement = parentBlockElement.parentElement;
                            if (listElement != null && ElementFilters.IsListElement(listElement) &&
                                ((IHTMLElementCollection)listElement.children).length == 1)
                            {
                                HTMLElementHelper.RemoveElement(listElement);
                            }
                        }

                        HTMLElementHelper.RemoveElement(parentBlockElement);
                    }

                    // Move the cursor inside the table.
                    MarkupServices.CreateMarkupRange(afterBeginFirstTd, afterBeginFirstTd).ToTextRange().select();

                    // Make sure the content gets spellchecked since we just moved it.
                    _damageServices.AddDamage(MarkupServices.CreateMarkupRange(firstTd, false));

                    ea.Handled = true;
                    undoUnit.Commit();
                }
            }
        }

        public event SnapRectEventHandler SnapRectEvent;

        public event HtmlEventHandler MouseDown;
        protected virtual void OnMouseDown(HtmlEventArgs evt)
        {
            if (MouseDown != null)
            {
                foreach (HtmlEventHandler handler in MouseDown.GetInvocationList())
                {
                    handler(this, evt);

                    if (evt.WasCancelled)
                        break;
                }
            }
        }

        public event HtmlEventHandler MouseUp;
        protected virtual void OnMouseUp(HtmlEventArgs evt)
        {
            if (MouseUp != null)
            {
                foreach (HtmlEventHandler handler in MouseUp.GetInvocationList())
                {
                    handler(this, evt);

                    if (evt.WasCancelled)
                        break;
                }
            }
        }

        /// <summary>
        /// Help requested via the F1 key
        /// </summary>
        public event EventHandler HelpRequest
        {
            add
            {
                _mshtmlEditor.HelpRequest += value;
            }
            remove
            {
                _mshtmlEditor.HelpRequest -= value;
            }
        }

        public event DamageListener DamageOccured
        {
            add { _damageServices.DamageOccured += value; }
            remove { _damageServices.DamageOccured -= value; }
        }

        /// <summary>
        /// Hook allowing editor subclasses to override (and veto handlers for) the pre-handle event
        /// </summary>
        /// <param name="inEvtDispId"></param>
        /// <param name="pIEventObj"></param>
        /// <returns></returns>
        protected virtual int OnPreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            switch (inEvtDispId)
            {
                case DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE:
                    //fix evil bug 435455 - removing invalid selections using the Selection.empty()
                    //call during onSelectionChanged is known to cause the editor to crash during
                    //mouseUp if a dragdrop operation had been started by the editor.  To prevent
                    //the editor from hitting this condition, we hook the mouseMove event and eat
                    //it if the editor's selection state is invalid. This prevents the editor from
                    //intiating a dragdrop while in an invalid selection state.
                    if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                    {
                        if (HasContiguousSelection && !IsValidContiguousSelection())
                        {
                            return HRESULT.S_OK;
                        }
                    }
                    break;
                case DISPID_HTMLELEMENTEVENTS2.ONKEYDOWN:
                    // WinLive 245925: Because we inline some CSS into the font tag (specifically, the font-size
                    // property), we need to make sure that when the user hits enter that the font-size persists
                    // onto the next line. MSHTML does not handle this for us.
                    if (Editable && pIEventObj.keyCode == (int)Keys.Enter && SelectedMarkupRange.IsEmptyOfText(false))
                    {
                        bool restOfBlockElementIsEmpty = true;
                        bool foundFontSizeElement = false;

                        MarkupContext context;
                        MarkupPointer p;
                        for (p = SelectedMarkupRange.Start.Clone(), context = new MarkupContext(); !ElementFilters.IsBlockElement(p.CurrentScope); p.Right(true, context))
                        {
                            if (context.Element != null)
                            {
                                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope &&
                                    _fontTagWithFontSizeFilter(context.Element))
                                {
                                    // We found the end tag of a font element that wraps our current selection.
                                    foundFontSizeElement = true;
                                }
                                else if (ElementFilters.IsVisibleEmptyElement(context.Element))
                                {
                                    // If there is any content to the right of the cursor in the current block
                                    // element, then it (and any font tags) will go down to the new line
                                    // automatically, so we don't have to worry about anything in this case.
                                    restOfBlockElementIsEmpty = false;
                                    break;
                                }
                            }
                            else if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text)
                            {
                                // If there is any content to the right of the cursor in the current block
                                // element, then it (and any font tags) will go down to the new line
                                // automatically, so we don't have to worry about anything in this case.
                                restOfBlockElementIsEmpty = false;
                                break;
                            }
                        }

                        if (foundFontSizeElement && restOfBlockElementIsEmpty && !ElementFilters.IsHeaderElement(p.CurrentScope))
                        {
                            // Save this for later. It will be processed after the Enter key has been processed.
                            _fontSizeBeforeEnter = GetFontSizeAt(SelectedMarkupRange.Start);
                        }
                    }
                    break;
            }

            return HRESULT.S_FALSE;
        }

        private float? _fontSizeBeforeEnter;

        private IHTMLElementFilter _fontTagWithFontSizeFilter = ElementFilters.CreateCompoundElementFilter(
            ElementFilters.CreateTagIdFilter("font"),
            e => !String.IsNullOrEmpty(((IHTMLElement2)e).currentStyle.fontSize as string)
        );

        public virtual bool IsRTLTemplate
        {
            get
            {
                return false;
            }
        }

        public void ForceDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// Returns the InternetSecurityZone assign to the currently loaded document
        /// </summary>
        /// <returns></returns>
        protected InternetSecurityZone? GetSecurityZone()
        {
            object output = null;
            GetMshtmlCommand(IDM.GETFRAMEZONE).Execute(null, ref output);

            if (output == DBNull.Value)
                return null;

            UInt32 zoneInt = (UInt32)output;
            InternetSecurityZone zone = (InternetSecurityZone)zoneInt;
            return zone;
        }
        protected enum InternetSecurityZone { LocalMachine = 0, LocalIntranet = 1, TrustedSites = 2, Internet = 3, RestrictedSites = 4 };

        #endregion

        # region Command Initialization

        private IMshtmlCommand GetMshtmlCommand(uint key)
        {
            return _mshtmlEditor.Commands[key] as IMshtmlCommand;
        }

        private void DocumentEvents_Click(object o, HtmlEventArgs e)
        {
            if (!e.htmlEvt.cancelBubble)
            {
                MoveSelectionToPoint(new Point(e.htmlEvt.screenX, e.htmlEvt.screenY));
            }
        }

        /// <summary>
        /// Gets the bounding box of the element using all the childern of the control.  This is useful
        /// for getting the bounding box that repersents the body because we artifically make the box larger
        /// to give the user a visual effect of a 'page' as one would see in Word.  However, the dead space below the last child
        /// can cause problems during click detection so this function will trim that off.
        /// </summary>
        protected RECT GetBodyBoundingBox(IHTMLElement element)
        {
            RECT elementRect = new RECT();
            elementRect.top = Int32.MaxValue;
            elementRect.left = Int32.MaxValue;

            bool foundChild = false;

            // WinLive 226885: If DOM is not well formed, then using element.children may not give us all the children
            // of this element. Instead, we walk the range to ensure we account for all elements.
            MarkupRange elementRange = MarkupServices.CreateMarkupRange(element, false);
            elementRange.WalkRange(
                delegate (MarkupRange currentRange, MarkupContext context, string text)
                {
                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                    {
                        foundChild = true;
                        IHTMLRect childRect = ((IHTMLElement2)context.Element).getBoundingClientRect();
                        elementRect.top = Math.Min(childRect.top, elementRect.top);
                        elementRect.bottom = Math.Max(childRect.bottom, elementRect.bottom);
                        elementRect.left = Math.Min(childRect.left, elementRect.left);
                        elementRect.right = Math.Max(childRect.right, elementRect.right);
                    }

                    return true;

                }, true);

            if (!foundChild)
            {
                // Default to the full size of the element.
                IHTMLRect mshtmlRect = ((IHTMLElement2)element).getBoundingClientRect();
                elementRect.top = mshtmlRect.top;
                elementRect.bottom = mshtmlRect.bottom;
                elementRect.left = mshtmlRect.left;
                elementRect.right = mshtmlRect.right;
            }

            return elementRect;
        }

        protected void MoveSelectionToPoint(Point screenPoint)
        {
            Point point = MshtmlEditor.PointToClient(screenPoint);

            MarkupRange selection = MarkupServices.CreateMarkupRange();
            if (Selection.SelectedMarkupRange.IsEmpty() && // If the user is selecting text dont try to mess with the clicks
                !IsEditFieldSelected &&    // If it is an edit field, don't mess with repositioning the caret
                (!HasContiguousSelection   //(don't re-position the caret if a selection is visible...that clears the selection)
                || !IsValidContiguousSelection()  // but don't allow the caret to be inside a non-editable region
                || GetBodyBoundingBox(PostBodyElement).bottom < point.Y))  // and clicks past the bottom of the body cause the caret to go to the beginning of the body, we want it at the end
            {
                //adjust the cursor to the closest valid caret location.
                //Note: this call will account for the documents editable regions so that the caret
                //cannot be placed outside of those regions.
                IHTMLCaretRaw caret = MoveCaretToClientPoint(point);

                // Fix bug 579672: Unable to enter text to the editor after switching font size
                caret.MoveMarkupPointerToCaret(selection.Start.PointerRaw);
                caret.MoveMarkupPointerToCaret(selection.End.PointerRaw);
            }
            else
            {
                selection = Selection.SelectedMarkupRange.Clone();
            }

            bool moved = MarkupPointerMoveHelper.DriveSelectionToLogicalPosition(selection, PostBodyElement);

            // If we have found a good place to move, move the selection there, unless
            // it is an edit field, then we will let the edit field control its own selection
            if ((moved || !HasContiguousSelection) && !IsEditFieldSelected)
                selection.ToTextRange().select();
        }

        public Size PostBodySize
        {
            get
            {
                IHTMLElement bodyElement = PostBodyElement;
                return new Size(bodyElement.offsetWidth, bodyElement.offsetHeight);
            }
        }

        public virtual IHTMLElement PostBodyElement
        {
            get { return _mshtmlEditor.HTMLDocument.body; }
        }

        #endregion

        public bool DocumentIsReady
        {
            get
            {
                return HTMLDocument.readyState == "complete" || HTMLDocument.readyState == "interactive";
            }
        }

        #region Document Event Handling
        private void _mshtmlEditor_DocumentComplete(object sender, EventArgs e)
        {
            if (!_initialDocumentLoaded)
            {
                // initialize editing options
                InitializeDocumentEditingOptions();

                // initialize our support for drag and drop
                mshtmlEditorDragAndDropTarget.Initialize(EditorControl);

#if DEBUG_STYLES
                // StyleDebugger.ShowDebugger(_mainFrameWindow, MshtmlEditor);
#endif
                // one-time init
                _initialDocumentLoaded = true;
            }

            ResetForDocumentComplete();

            //reset the damage services
            if (_damageServices != null)
                _damageServices.Reset();

            //reset the selection so that we don't keep a stale selection object around.
            //Note: This fixes a bug that caused exceptions when switching editing templates while
            //a smart content item was selected.
            ResetSelection();

            // notify subclasses that they should initialize/reset behaviors
            DetachBehaviors();
            AttachBehaviors(this);

            OnDocumentComplete(e);

            // Remove null-attributed font tags, e.g. <p><font>blah</font></p> --> <p>blah</p>
            if (DocumentIsReady && PostBodyElement != null)
            {
                MarkupRange bodyRange = MarkupServices.CreateMarkupRange(PostBodyElement);
                bodyRange.RemoveElementsByTagId(_ELEMENT_TAG_ID.TAGID_FONT, true);
            }

            // And lastly, reset the document's undo state.
            ClearUndoStack();
        }

        protected virtual void ResetForDocumentComplete()
        {
            htmlCaret = null;
            caretPositionDisplayPointer = null;
        }

        private bool _initialDocumentLoaded = false;

        protected virtual void InitializeDocumentEditingOptions()
        {

        }

        void DocumentEvents_KeyPress(object o, HtmlEventArgs e)
        {
            OnKeyPress(e);

            if (e.WasCancelled)
                return;
        }

        private void DocumentEvents_KeyDown(object o, HtmlEventArgs e)
        {
            OnKeyDown(e);

            // return if someone else cancelled processing
            if (e.WasCancelled)
                return;

            // global processing for special keys
            switch (e.htmlEvt.keyCode)
            {
                // tab key
                case (int)Keys.Tab:
                    HandleTabKey(e);
                    break;
                case (int)Keys.Left:
                case (int)Keys.Right:
                case (int)Keys.Up:
                case (int)Keys.Down:
                    HandleKeyboardNavigationKey(e);
                    break;
                case (int)Keys.Back:
                    HandleBackspaceKey(e);
                    break;
            }
        }

        protected abstract void HandleTabKey(HtmlEventArgs e);
        protected abstract void HandleKeyboardNavigationKey(HtmlEventArgs e);
        protected abstract void HandleBackspaceKey(HtmlEventArgs e);

        private void DocumentEvents_KeyUp(object o, HtmlEventArgs e)
        {
            OnKeyUp(e);

            if (e.WasCancelled)
                return;

            // global processing for KeyUp (none for the time being)
        }

        private void DocumentEvents_MouseDown(object o, HtmlEventArgs e)
        {
            OnMouseDown(e);

            if (e.WasCancelled)
                return;

            // global processing for MouseDown (none for the time being)
        }

        private void DocumentEvents_MouseUp(object o, HtmlEventArgs e)
        {
            OnMouseUp(e);

            if (e.WasCancelled)
                return;

            // global processing for Mouse Up (none for the time being )
        }

#if SELECTION_DEBUG
        private SelectionDebugDialog SelectionDebugDialog;
#endif
        private void DocumentEvents_SelectionChanged(object sender, EventArgs e)
        {
            using (ApplicationPerformance.LogEvent("SelectionChanged"))
            {
                BeginSelectionChangedCaching();

                try
                {

                    if (_mshtmlEditor.MshtmlControl.DocumentIsComplete)
                    {
#if SELECTION_DEBUG
                        if (SelectionDebugDialog == null)
                        {
                            SelectionDebugDialog = new SelectionDebugDialog();
                            SelectionDebugDialog.Show();
                        }

                        SelectionDebugDialog.Add(SelectedMarkupRange);
#endif
                        // update selection type
                        if (_defaultSelection == null)
                        {
                            _defaultSelection = new HtmlEditorSelection(MshtmlEditor, HTMLDocument);
                        }

                        ((IHtmlEditorComponentContext)this).Selection = _defaultSelection;

                        if (_suspendSelectionValidationDepth == 0)
                        {
                            //If the selection is contiguous, let the editor decide if its a valid selection
                            //and clear it if it is not.

                            //Resetting the selection may cause another selection change event, which can incorrectly return
                            //a non-empty selection, so to avoid recursive stack overflows, we suspend validation for subsequent
                            //selection changes. (bug 405391)
                            _suspendSelectionValidationDepth++;

                            try
                            {
                                if (!IsValidContiguousSelection())
                                {
                                    if (HasContiguousSelection)
                                    {
                                        if (ShouldEmptySelection())
                                        {
                                            EmptySelection();
                                        }
                                        else
                                        {
                                            // WinLive 196005: In some circumstances we do not want to empty an invalid selection.
                                            if (_selectionIsInvalid == false)
                                            {
                                                _selectionIsInvalid = true;
                                                OnCommandStateChanged();
                                            }
                                        }

                                        return;
                                    }
                                    else
                                    {
                                        if (TryMoveToValidSelection(((IHtmlEditorComponentContext)this).Selection.SelectedMarkupRange))
                                        {
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    if (_selectionIsInvalid == true)
                                    {
                                        _selectionIsInvalid = false;
                                        OnCommandStateChanged();
                                    }
                                }
                            }
                            finally
                            {
                                //unsuspend selection validation now that any subsequent selection changes have passed.
                                _suspendSelectionValidationDepth--;
                            }
                        }
                        else
                        {
                            // Are we still seeing the problems that made this _suspendSelectionValidationDepth stuff necessary?
                            // Rather than masking product instability, we should figure out the root cause.
                            // I don't feel good about just removing this immediately, however.
                            // Who knows what code has now been written that depends on this code being in place.
                            // Let's try to determine if we're still hitting the stack overflow scenario.
                            // In time, if we have not seen this assert, then we may want to remove the _suspendSelectionValidationDepth code.
                            Debug.WriteLine("Avoided a recursive stack overflow.  Figure out why it was happening in the first place!");
                        }
                    }
                }
                finally
                {
                    EndSelectionChangedCaching();
                    if (ApplicationPerformance.ContainsEvent("Backspace"))
                        ApplicationPerformance.EndEvent("Backspace");
                }
            }

        }

        protected bool _logInvalidEditRegions = true;

        /// <summary>
        /// Overridable hook that allows subclasses to customize the validity of contiguous selections.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsValidContiguousSelection()
        {
            return true;
        }

        protected virtual bool TryMoveToValidSelection(MarkupRange range)
        {
            return false;
        }

        /// <summary>
        /// Overridable hook that allows subclasses to customize the validity of insertion points.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsValidContentInsertionPoint(MarkupRange target)
        {
            return true;
        }

        /// <summary>
        /// Overridable hook that allows subclasses to customize if the current selection should be emptied because
        /// it is invalid.
        /// </summary>
        protected virtual bool ShouldEmptySelection()
        {
            return true;
        }

        private bool _selectionIsInvalid;
        public bool SelectionIsInvalid
        {
            get
            {
                return _selectionIsInvalid;
            }
        }

        private void ResetSelection()
        {
            try
            {
                // Keep it within the postBody div!
                _mshtmlEditor.DocumentEvents.SelectionChanged -= new EventHandler(DocumentEvents_SelectionChanged);
                IHTMLElement postBody = PostBodyElement;
                if (postBody != null)
                {
                    MarkupRange range = MarkupServices.CreateMarkupRange(postBody, false);
                    range.Collapse(true);
                    range.ToTextRange().select();
                }
            }
            finally
            {
                _mshtmlEditor.DocumentEvents.SelectionChanged += new EventHandler(DocumentEvents_SelectionChanged);
            }

            _defaultSelection = new HtmlEditorSelection(_mshtmlEditor, HTMLDocument);
            ((IHtmlEditorComponentContext)this).Selection = _defaultSelection;
        }

        /// <summary>
        /// This function will make sure the selection is somewhere that the
        /// IDM_FONTNAME command will find a font using using one of 2 methods
        ///
        /// 1) <div>|</div> becomes <div><font ...>|</font></div>
        /// 2) <div>|<font></font></div> becomes <div><font>|</font></div>
        ///
        /// This function is needed for Mail to provide high probability that
        /// no matter where the user selects a font tag will be there to express
        /// the font in the ribbon.Mail must use font tags to be to make sure it interops correctly
        /// </summary>
        /// <returns>Returns true if the function moves the selection</returns>
        public bool MoveSelectionToFont()
        {
            if (CurrentDefaultFont != null)
            {
                MarkupRange range = SelectedMarkupRange;
                if (range.IsEmpty())
                {
                    IHTMLElement ele = range.Start.CurrentScope;
                    if (ele.tagName == "DIV")
                    {
                        // If the div we are inside if empty, add the default font
                        if (string.IsNullOrEmpty(ele.innerHTML))
                            ele.innerHTML = CurrentDefaultFont.ApplyFont("");
                    }

                    IHTMLElement finalElement = FindFontElement(true);

                    if (finalElement == null)
                    {
                        finalElement = FindFontElement(false);
                    }

                    if (finalElement != null && string.IsNullOrEmpty(finalElement.innerHTML))
                    {
                        MarkupServices.CreateMarkupRange(finalElement, false).ToTextRange().select();
                        return true;
                    }
                }
            }

            return false;
        }

        private IHTMLElement FindFontElement(bool right)
        {
            IHTMLElement returnElement = null;
            MarkupPointer markupPointer = SelectedMarkupRange.Start.Clone();
            MarkupContext mc = null;

            if (right)
                mc = markupPointer.Right(true);
            else
                mc = markupPointer.Left(true);

            // Starting from <div>|... walk to the right we are in the middle of any formatting
            // tags we might have inserted.  For example we want to go from <div>|<font><u><em><strong></...
            // to <div><font><u><em><strong>|</...
            while (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
            {
                IHTMLElement tempElement = mc.Element;

                // If we found something other then the tags
                // used for defineing the font we have walked to far
                if (tempElement.tagName != "FONT" &&
                    tempElement.tagName != "EM" &&
                    tempElement.tagName != "STRONG" &&
                    tempElement.tagName != "U" &&
                    tempElement.tagName != "DIV")
                    break;

                returnElement = tempElement;
                if (right)
                    mc = markupPointer.Right(true);
                else
                    mc = markupPointer.Left(true);
            }

            return returnElement;
        }

        protected void OnSelectionChanged(EventArgs e, IHtmlEditorSelection newSelection, bool moveSelectionToFont)
        {
            _selection = newSelection;

            // fire our own selection changed event
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);

            // fire our command-state changed event
            OnCommandStateChanged();
        }

        private void _mshtmlEditor_BeforeShowContextMenu(object sender, EventArgs e)
        {
            //FireDefaultSelectionChanged();
        }

        private void _mshtmlEditor_DisplayChanged(object sender, EventArgs e)
        {
            OnAggressiveCommandStateChanged();
        }

        public int _mshtmlEditor_GetDropTarget(OpenLiveWriter.Interop.Com.IDropTarget pDropTarget, out OpenLiveWriter.Interop.Com.IDropTarget ppDropTarget)
        {
            try
            {
                return mshtmlEditorDragAndDropTarget.GetDropTarget(pDropTarget, out ppDropTarget);
            }
            catch (Exception e)
            {
                UnexpectedErrorMessage.Show(e);
                ppDropTarget = null;
                return HRESULT.E_NOTIMPL;
            }
        }

        private void _mshtmlEditor_CommandKey(object sender, KeyEventArgs e)
        {
            OnCommandKey(e);
        }


        #endregion

        #region Spell Checking Helpers

        //ToDo: OLW Spell Checker
        /// <summary>
        /// Get the spelling-checker (demand-create and cache/re-use)
        /// </summary>
        //protected ISpellingChecker SpellingChecker
        //{
        //    get
        //    {
        //        return _spellingChecker;
        //    }
        //}
        //private ISpellingChecker _spellingChecker;

        #endregion

        #region Protected Utility Methods
        /// <summary>
        /// The document's editable elements.
        /// </summary>
        protected virtual IHTMLElement[] EditableElements
        {
            get { return new IHTMLElement[] { HTMLDocument.body }; }
        }

        protected IHTMLDocument2 HTMLDocument
        {
            get
            {
                return _mshtmlEditor.HTMLDocument;
            }
        }

        protected MshtmlEditor MshtmlEditor
        {
            get
            {
                return _mshtmlEditor;
            }
        }

        protected MshtmlMarkupServices MarkupServices
        {
            get
            {
                return _mshtmlEditor.MshtmlControl.MarkupServices;
            }
        }

        protected IMainFrameWindow FrameWindow
        {
            get
            {
                return _mainFrameWindow;
            }
        }

        bool IHtmlEditorComponentContext.EditMode
        {
            get { return Editable; }
        }

        protected virtual bool Editable
        {
            get
            {
                return _mshtmlEditor != null && _mshtmlEditor.MshtmlControl.EditMode;
            }
        }

        protected EditorLinkNavigator LinkNavigator
        {
            get { return linkNavigator; }
        }

        public IUndoUnit CreateUndoUnit()
        {
            return new UndoUnit(this);
        }

        /// <summary>
        /// Creates an undo unit that is able to restore the selection state of the editor
        /// during undo/redo operations.
        /// </summary>
        /// <returns></returns>
        public IUndoUnit CreateSelectionUndoUnit()
        {
            return new SelectionUndoUnit(this, SelectedMarkupRange);
        }

        public IUndoUnit CreateSelectionUndoUnit(MarkupRange selection)
        {
            return new SelectionUndoUnit(this, selection);
        }

        public IUndoUnit CreateInvisibleUndoUnit()
        {
            return new InvisibleUndoUnit(this);
        }

        public IOleUndoManager UndoManager
        {
            get
            {
                return _mshtmlEditor.MshtmlControl.OleUndoManager;
            }
        }

        protected MarkupRange CleanUpRange()
        {
            MarkupRange mRange = null;
            IHTMLElement[] anchors = SelectedMarkupRange.GetElements(ElementFilters.ANCHOR_ELEMENTS, false);
            foreach (IHTMLElement childAnchor in anchors)
                MarkupServices.RemoveElement(childAnchor);

            //remove the current <a> tag
            IHTMLElement anchor = GetCurrentEditableAnchorElement();
            if (anchor != null)
            {
                mRange = MarkupServices.CreateMarkupRange(anchor);
                MarkupServices.RemoveElement(anchor);
            }
            return mRange;
        }

        private bool ContainsMultipleAnchors()
        {
            //used to check for multiple anchors within the selection
            if (_mshtmlEditor.MshtmlControl.DocumentIsComplete) //avoids case where this gets called before startup completes
            {
                MarkupRange markupRange = SelectedMarkupRange;
                if (markupRange != null)
                {
                    if (HasContiguousSelection)
                    {
                        //try to locate the anchor within the selection.
                        MarkupRange range = markupRange.Clone();
                        IHTMLElement[] anchors = range.GetElements(ElementFilters.ANCHOR_ELEMENTS, false);
                        if (anchors.Length > 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the currently selected anchor element.
        /// </summary>
        /// <returns></returns>
        private IHTMLElement GetCurrentEditableAnchorElement()
        {
            IHTMLElement anchor = null;
            if (_mshtmlEditor.MshtmlControl.DocumentIsComplete) //avoids case where this gets called before startup completes
            {
                MarkupRange markupRange = SelectedMarkupRange;
                if (markupRange != null)
                {
                    //correct range as necessary
                    MarkupRange editableBoundary = PrimaryEditableBounds;
                    MarkupRange range = markupRange.Clone();
                    MarkupPointerMoveHelper.MoveUnitBounded(range.Start,
                        MarkupPointerMoveHelper.MoveDirection.LEFT,
                        MarkupPointerAdjacency.BeforeVisible | MarkupPointerAdjacency.BeforeEnterScope,
                        editableBoundary.Start);

                    // determine if this is an edit of an existing link
                    if (HasContiguousSelection)
                    {
                        //try to locate the anchor within the selection.
                        IHTMLElement[] anchors = range.GetElements(ElementFilters.ANCHOR_ELEMENTS, false);
                        if (anchors.Length > 0)
                        {
                            anchor = anchors[0];
                        }
                    }

                    if (anchor == null)
                    {
                        //walk up the parent node incase the selection is entirely within an anchor
                        IHTMLElement parent = range.Start.CurrentScope;
                        while (parent != null && !(parent is IHTMLAnchorElement))
                        {
                            parent = parent.parentElement;
                        }
                        anchor = parent;

                        //verify the anchor is within the editable boundary
                        if (anchor != null)
                        {
                            range.MoveToElement(anchor, true);
                            if (!editableBoundary.InRange(range))
                                anchor = null;
                        }
                    }
                }
            }
            return anchor;
        }

        /// <summary>
        /// Get current selection pointers
        /// </summary>
        /// <returns>selection pointers</returns>
        public MarkupRange SelectedMarkupRange
        {
            get
            {
                return Selection != null ? Selection.SelectedMarkupRange : null;
            }
        }

        #endregion

        #region Undo Management (UndoUnit and InvisibleUndoUnit)

        protected void ClearUndoStack()
        {
            UndoManager.DiscardFrom(null);
        }

        private class SelectionUndoUnit : IUndoUnit
        {
            HtmlEditorControl _editor;
            MarkupRange _selection;
            MshtmlMarkupServices _markupServices;
            private IOleUndoUnit undoUnit;
            private IOleUndoUnit redoUnit;
            private bool committed = false;
            public SelectionUndoUnit(HtmlEditorControl editor, MarkupRange selection)
            {
                _editor = editor;
                _selection = selection;
                _markupServices = _editor.MshtmlEditor.MshtmlControl.MarkupServices;

                undoUnit = new SelectionOleUndoUnit(_markupServices, _selection);
                _editor.UndoManager.Add(undoUnit);
            }

            public void Commit()
            {
                redoUnit = new SelectionOleRedoUnit(_markupServices, _selection);
                _editor.UndoManager.Add(redoUnit);
                committed = true;
            }

            public void Dispose()
            {
                if (!committed)
                    _editor.UndoManager.UndoTo(undoUnit);
            }
        }

        /// <summary>
        /// Class which wraps creating and relating an undo unit
        /// </summary>
        private class UndoUnit : IUndoUnit
        {
            /// <summary>
            /// Create/initialize
            /// </summary>
            /// <param name="editor"></param>
            public UndoUnit(HtmlEditorControl editor)
                : this(editor, Guid.NewGuid().ToString())
            {

            }

            public UndoUnit(HtmlEditorControl editor, string title)
            {
                // save reference to editor
                _editor = editor;

                // begin undo unit
                _editor.MarkupServices.BeginUndoUnit(title);

                undoDepth++;
            }

            /// <summary>
            /// Call this method to indicate that the undo unit should be
            /// committed. Users of UndoUnit must call this when their edit is
            /// completed -- if the object is disposed and this method has not
            /// been called then all of the changes will be rolled back.
            /// </summary>
            public void Commit()
            {
                committed = true;
            }

            /// <summary>
            /// Release
            /// </summary>
            public void Dispose()
            {
                try
                {
                    undoDepth--;

                    if (!committed)
                        _editor.uncommittedCount++;
                    else if (undoDepth == 0 && _editor.uncommittedCount != 0)
                        Trace.Fail("Uncommitted child undo units detected while committing undo");

                    if (_editor.uncommittedCount > 0 && undoDepth == 0)
                    {
                        //Hack: if no changes are made during an undo, the undo operation improperly
                        //reverts the previous change.  To avoid this empty undo case, we perform
                        //an edit on the doc, then the perform the undo.
                        string bodyId = _editor.HTMLDocument.body.id;
                        _editor.HTMLDocument.body.id = "body" + Environment.TickCount;
                        _editor.HTMLDocument.body.id = bodyId;
                    }

                    // end the undo unit
                    _editor.MarkupServices.EndUndoUnit();

                    // if the edit was not completed then roll all of the changes back
                    if (_editor.uncommittedCount > 0 && undoDepth == 0)
                    {
                        try
                        {
                            _editor.GetMshtmlCommand(IDM.UNDO).Execute();
                        }
                        catch (Exception e)
                        {
                            //MHTML occasionally throws a "catastropic" error while undoing an operation
                            //that went wrong.  Log the error instead of scaring the crap out of the
                            //user when this occurs.
                            Trace.Fail("Undo error occurred: " + e.Message, e.StackTrace);
                        }
                    }

                    // command state changed
                    try
                    {
                        // WinLive 160235: Sometimes we have an unpositioned selection at this point
                        if (!_editor.SelectedMarkupRange.Positioned)
                            _editor.ResetSelection();

                        _editor.OnCommandStateChanged();
                    }
                    catch { }
                }
                catch (Exception e)
                {
                    Trace.Fail("exception during undo: " + e.Message, e.StackTrace);
                }
                finally
                {
                    if (undoDepth == 0)
                    {
                        //reset the uncommitted count
                        _editor.uncommittedCount = 0;
                    }
                }
            }

            /// <summary>
            /// Editor we are managing an undo unit for
            /// </summary>
            private HtmlEditorControl _editor;

            /// <summary>
            /// Was the edit completed
            /// </summary>
            private bool committed = false;
        }

        private class InvisibleUndoUnit : UndoUnit
        {
            public static bool UnitIsInvisible(string title)
            {
                return title.StartsWith(INVISIBLE_UNDO_UNIT_PREFIX);
            }

            public InvisibleUndoUnit(HtmlEditorControl editorControl)
                : base(editorControl, INVISIBLE_UNDO_UNIT_PREFIX + Guid.NewGuid().ToString())
            {
            }

            private const string INVISIBLE_UNDO_UNIT_PREFIX = "InvisibleUndoUnit";
        }

        private IOleUndoUnit GetTargetUndoUnit()
        {
            // get a list of all of the undo units and copy them into an array
            IEnumOleUndoUnits undoUnits = null;
            UndoManager.EnumUndoable(out undoUnits);
            ArrayList activeUndoUnits = CopyUndoUnitsToArray(undoUnits);

            // reverse the list so the most recent shows up first
            activeUndoUnits.Reverse();

            // scan the list starting with the most recent, attempting to identify
            // the first "visible" undo unit
            foreach (IOleUndoUnit undoUnit in activeUndoUnits)
            {
                // get the description
                string description = GetUndoUnitDescription(undoUnit);

                // see if it is a visible undo item
                if (!InvisibleUndoUnit.UnitIsInvisible(description))
                    return undoUnit;
            }

            // if we got this far then there is no visible undo unit
            return null;
        }

        private IOleUndoUnit GetTargetRedoUnit()
        {
            // get a list of all of the undo units and copy them into an array
            IEnumOleUndoUnits redoUnits = null;
            UndoManager.EnumRedoable(out redoUnits);
            ArrayList activeRedoUnits = CopyUndoUnitsToArray(redoUnits);

            // reverse the list for scanning
            activeRedoUnits.Reverse();

            // screen empty list case
            if (activeRedoUnits.Count > 0)
            {
                // the first unit in the list is our logical target
                IOleUndoUnit targetRedoUnit = activeRedoUnits[0] as IOleUndoUnit;

                // we assume the first item in the list is visible b/c invisible undos
                // should always be chained with a visible one. check this assumptoin
                Trace.Assert(!InvisibleUndoUnit.UnitIsInvisible(GetUndoUnitDescription(targetRedoUnit)));

                // scan the list for 'invisible' undos that are logically part of
                // of the target redo unit. keep scanning until we find another
                // 'visible' undo
                for (int i = 1; i < activeRedoUnits.Count; i++)
                {
                    // if the unit is invisible then make it the target
                    IOleUndoUnit undoUnit = activeRedoUnits[i] as IOleUndoUnit;
                    if (InvisibleUndoUnit.UnitIsInvisible(GetUndoUnitDescription(undoUnit)))
                    {
                        targetRedoUnit = undoUnit;
                    }
                    else
                    {
                        // otherwise as soon as we hit a visible unit we have gone too far
                        break;
                    }
                }

                return targetRedoUnit;
            }
            else // no redo units available
            {
                return null;
            }
        }

        private ArrayList CopyUndoUnitsToArray(IEnumOleUndoUnits undoUnits)
        {
            // get a list of all of the undo units
            ArrayList undoUnitArray = new ArrayList();
            IOleUndoUnit current;
            int result;
            while ((result = undoUnits.Next(1, out current, IntPtr.Zero)) == HRESULT.S_OK)
                undoUnitArray.Add(current);

            // return the list in an array
            return undoUnitArray;
        }

        private string GetUndoUnitDescription(IOleUndoUnit undoUnit)
        {
            // get the description
            String description;
            undoUnit.GetDescription(out description);
            return description;
        }

        #endregion

        #region BlockCommand Execution

        protected delegate void CommandExecutor();

        /// <summary>
        /// Executes an MSHTML block command with protective hacks necessary for IE7.
        /// </summary>
        /// <param name="command"></param>
        protected void ExecuteBlockCommand(CommandExecutor command)
        {
            if (!Editable)
                return;

            try
            {
                using (IUndoUnit undo = CreateUndoUnit())
                {
                    using (new BlockCommandExecutionContext(Selection.SelectedMarkupRange.Start.CurrentBlockScope()))
                        command();
                    undo.Commit();
                }
            }
            catch (Exception e)
            {
                Trace.Fail("An unexpected error occurred while executing the block command", e.ToString());
            }
        }

        /// <summary>
        /// Inserts a temporary element into the DOM while a block command is executing to prevent
        /// IE7 from aggressively molesting the DIVs outside of the main editable region. (bug 292762)
        /// </summary>
        class BlockCommandExecutionContext : IDisposable
        {
            private IHTMLElement _rootElement;
            private IHTMLElement _tempBlockElement;
            private IHTMLDOMNode _tempTextNode;
            public BlockCommandExecutionContext(IHTMLElement rootElement)
            {
                //note: we need to insert a block element with a text node into the DOM and guarantee
                //and guarantee neither node gets orphaned as part of the command execution.
                _rootElement = GetEditableParent(rootElement);
                IHTMLDocument2 document = rootElement.document as IHTMLDocument2;
                _tempBlockElement = document.createElement("p");
                _tempBlockElement.innerText = "tempHTML";

                (_rootElement as IHTMLDOMNode).appendChild(_tempBlockElement as IHTMLDOMNode);
                _tempTextNode = (_tempBlockElement as IHTMLDOMNode).firstChild;
            }

            private IHTMLElement GetEditableParent(IHTMLElement e)
            {
                while (e.parentElement != null && (e.parentElement as IHTMLElement3).isContentEditable)
                {
                    e = e.parentElement;
                }
                return e;
            }
            public void Dispose()
            {
                try
                {
                    if (_tempBlockElement.parentElement != null)
                        (_tempBlockElement as IHTMLDOMNode).removeNode(true);
                    else
                    {
                        if (_tempTextNode != null && _tempTextNode.parentNode != null)
                            (_tempTextNode).removeNode(true);
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail("Failed to remove temp nodes", e.ToString());
                }
            }
        }

        #endregion

        #region Private Data

        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected Container components = null;

        private MshtmlEditorDragAndDropTarget mshtmlEditorDragAndDropTarget;
        private MshtmlEditor _mshtmlEditor;
        private MshtmlOptions _mshtmlOptions;
        private IMainFrameWindow _mainFrameWindow;
        private IStatusBar _statusBar;
        private String _originalText;

        [ThreadStatic]
        private static CachedEditorAndSecurityManager _editorCache;
        private class CachedEditorAndSecurityManager
        {
            public MshtmlEditor Editor { get; set; }
            public InternetSecurityManagerShim SecurityManager { get; set; }
        }

        private IHtmlEditorSelection _selection;
        private IHtmlEditorSelection _defaultSelection;
        private int _suspendSelectionValidationDepth;
        private HtmlEditorControlDamageServices _damageServices;

        /// <summary>
        /// Object used to do navigation to links embedded in the document
        /// </summary>
        private EditorLinkNavigator linkNavigator;

        #endregion

        #region Public Data
        public const string CONTENT_BODY_PADDING = "<P></P>";
        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        #region Caret Management
        /// <summary>
        /// Move the HTML caret to the specified point (specified in screen coordinates)
        /// </summary>
        /// <param name="screenPoint">point (in screen coordinates)</param>
        /// <returns>the current location of the html caret</returns>
        public IHTMLCaretRaw MoveCaretToScreenPoint(Point screenPoint)
        {
            // initialize the point to move to (translate to client coordinates)
            return MoveCaretToClientPoint(EditorControl.PointToClient(screenPoint), false);
        }

        /// <summary>
        /// Move the caret to a client-point
        /// </summary>
        /// <param name="clientPoint"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        ///
        public IHTMLCaretRaw MoveCaretToClientPoint(Point clientPoint)
        {
            return MoveCaretToClientPoint(clientPoint, true);
        }

        public IHTMLCaretRaw MoveCaretToClientPoint(Point clientPoint, bool selectLocation)
        {
            //position the caret display pointer at the nearest valid location.
            MoveCaretPointersToNearestValidLocation(clientPoint, selectLocation);

            // move the html caret to the display position
            SynchronizeCaretWithDisplayPointer(CaretPositionDisplayPointer, false);

            // return the caret
            return HtmlCaret;
        }

        /// <summary>
        /// Returns the point of the nearest valid caret position within the document's editable range.
        /// </summary>
        /// <param name="fromClientPoint"></param>
        private void MoveCaretPointersToNearestValidLocation(Point fromClientPoint, bool selectLocation)
        {
            IHTMLElement targetElement = GetNearestEditableElement(fromClientPoint);
            IDisplayPointerRaw displayPointer = CaretPositionDisplayPointer;
            MarkupPointer markupPointer = CaretPositionMarkupPointer;
            Point newPoint = new Point();
            RECT elementRect = new RECT();

            // The point that we are trying to move the caret to is directly in the PostBodyElement
            // When we find the size of the element, we use GetBodyBoundingBox which
            // will determine the size of the element using the childern.  If the user clicks past the last
            // child element of the post body, calcuating the size this will will result in the code path
            // that places at the last line of the body.  This is correct.  If we used the normal getBoundingClientRect
            // and the user clicked inside of the body but below the last line, the caret would move to the top
            // left corner of the post body which is not what users would expect.
            if (targetElement == PostBodyElement)
            {
                elementRect = GetBodyBoundingBox(PostBodyElement);
            }
            else
            {
                IHTMLRect mshtmlRect = ((IHTMLElement2)targetElement).getBoundingClientRect();
                elementRect.top = mshtmlRect.top;
                elementRect.bottom = mshtmlRect.bottom;
                elementRect.left = mshtmlRect.left;
                elementRect.right = mshtmlRect.right;
            }

            if (fromClientPoint.Y > elementRect.bottom)
            {
                //move caret to bottom-right corner of element
                newPoint.Y = elementRect.bottom;
                markupPointer.MoveAdjacentToElement(targetElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                DisplayServices.TraceMoveToMarkupPointer(displayPointer, markupPointer);
            }
            else if (fromClientPoint.Y < elementRect.top)
            {
                //move caret to top-left corner of element
                newPoint.Y = elementRect.top;
                markupPointer.MoveAdjacentToElement(targetElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                DisplayServices.TraceMoveToMarkupPointer(displayPointer, markupPointer);
            }
            else
            {
                //the Y position is good, so the line is a valid location, now adjust the x position on the line
                Point lineMidPoint = new Point();
                lineMidPoint.Y = fromClientPoint.Y;
                lineMidPoint.X = (int)((elementRect.left + elementRect.right) / 2f);
                MoveDisplayPointerToClientPoint(displayPointer, lineMidPoint);
                if (fromClientPoint.X >= elementRect.right)
                {
                    //move caret to the end of the line
                    displayPointer.MoveUnit(_DISPLAY_MOVEUNIT.DISPLAY_MOVEUNIT_CurrentLineEnd, -1);
                }
                else if (fromClientPoint.X <= elementRect.left)
                {
                    //move caret to the start of the line
                    displayPointer.MoveUnit(_DISPLAY_MOVEUNIT.DISPLAY_MOVEUNIT_CurrentLineStart, -1);
                }
                else
                {
                    //the point is inside the editable region, so just place the caret at the point.
                    MoveDisplayPointerToClientPoint(displayPointer, fromClientPoint);
                }

                AdjustDisplayPointerPosition(ref displayPointer, fromClientPoint);

                displayPointer.PositionMarkupPointer(markupPointer.PointerRaw);
            }

            // We might not want to select the element if this is an edit field
            // that will select its whole inner contents on intial focus inside of the element
            if (selectLocation)
            {
                //focus the target element in case focus is currently in another element.
                // TODO: This call to focus is causing the scroll position to jump when selecting the first line of text.
                //(targetElement as IHTMLElement2).focus();
                (targetElement as IHTMLElement3).setActive();
            }
        }

        protected virtual void AdjustDisplayPointerPosition(ref IDisplayPointerRaw displayPointer, Point fromClientPoint)
        {
        }

        private IHTMLElement GetNearestEditableElement(Point fromClientPoint)
        {
            IHTMLElement[] editableElements = EditableElements;
            IHTMLElement nearestElement = null;
            int nearestElementDistance = Int32.MaxValue;

            foreach (IHTMLElement element in editableElements)
            {
                IHTMLElement2 element2 = (IHTMLElement2)element;
                IHTMLRect rect = element2.getBoundingClientRect();
                if (fromClientPoint.Y >= rect.top && fromClientPoint.Y <= rect.bottom)
                    return element;
                else
                {
                    int distance;
                    if (fromClientPoint.Y < rect.top)
                        distance = rect.top - fromClientPoint.Y;
                    else
                        distance = fromClientPoint.Y - rect.bottom;
                    if (distance < nearestElementDistance)
                    {
                        nearestElementDistance = distance;
                        nearestElement = element;
                    }
                }
            }
            return nearestElement;
        }

        /// <summary>
        /// Move the caret to the location of the specified pointer.
        /// </summary>
        /// <param name="displayPointer"></param>
        /// <param name="select">if true, the selection will be explicitly driven to the new caret position.
        /// This ensures that all selection-driver components are displayed based on the new caret position (commands, etc)</param>
        public void SynchronizeCaretWithDisplayPointer(IDisplayPointerRaw displayPointer, bool select)
        {
            HtmlCaret.MoveCaretToPointerEx(
                displayPointer,
                // Display pointer to move caret to
                true,		// Make caret visible
                true,		// Auto-scroll to caret
                _CARET_DIRECTION.CARET_DIRECTION_SAME
                // Preserve direction of caret
                );

            if (select)
            {
                //update the selection (collapse it to ensure that it doesn't span further than the caret)
                MarkupRange selection = SelectedMarkupRange;
                selection.Collapse(true);
                UpdateSelection(selection);

                //Avoid bug with caret placement: Updating the selection occasionally shifts the caret into another
                //position.  This re-placement ensures that the caret is always placed correctly (even if the
                //selection known to MSHTML is slightly off)
                HtmlCaret.MoveCaretToPointerEx(
                    displayPointer,
                    // Display pointer to move caret to
                    true,		// Make caret visible
                    true,		// Auto-scroll to caret
                    _CARET_DIRECTION.CARET_DIRECTION_SAME
                    // Preserve direction of caret
                    );
            }
        }

        /// <summary>
        /// Demand retrieve (and cache) the html caret
        /// </summary>
        private IHTMLCaretRaw HtmlCaret
        {
            get
            {
                // get caret if necessary
                if (htmlCaret == null)
                {
                    MshtmlEditor.MshtmlControl.DisplayServices.GetCaret(out htmlCaret);
                }

                // return the caret
                return htmlCaret;
            }
        }
        private IHTMLCaretRaw htmlCaret;

        /// <summary>
        /// Demand create (and cache) the display pointer used to position the caret
        /// </summary>
        private IDisplayPointerRaw CaretPositionDisplayPointer
        {
            get
            {
                // create display pointer if necessary
                if (caretPositionDisplayPointer == null)
                    MshtmlEditor.MshtmlControl.DisplayServices.CreateDisplayPointer(out caretPositionDisplayPointer);

                // return the display pointer
                return caretPositionDisplayPointer;
            }
        }
        private IDisplayPointerRaw caretPositionDisplayPointer;

        /// <summary>
        /// Demand create (and cache) the markup pointer used to position the caret
        /// </summary>
        private MarkupPointer CaretPositionMarkupPointer
        {
            get
            {
                // create markup pointer if necessary
                if (caretPositionMarkupPointer == null)
                    caretPositionMarkupPointer = MarkupServices.CreateMarkupPointer();

                // return the markup pointer
                return caretPositionMarkupPointer;
            }
        }
        private MarkupPointer caretPositionMarkupPointer;

        /// <summary>
        /// Positions the specified displayPointer at the specified clientPoint.
        /// </summary>
        /// <param name="displayPointer"></param>
        /// <param name="clientPoint"></param>
        protected void MoveDisplayPointerToClientPoint(IDisplayPointerRaw displayPointer, Point clientPoint)
        {
            POINT point = new POINT();
            point.x = clientPoint.X;
            point.y = clientPoint.Y;

            // move the caret position display pointer to the point
            uint htRes;
            displayPointer.MoveToPoint(
                point,		// point to move to
                _COORD_SYSTEM.COORD_SYSTEM_GLOBAL,
                // global (client area) coordinates
                null,		// element coordinates are relative to (not used for GLOBAL)
                0,			// HT_OPTIONS (don't hit test beyond EOL)
                out htRes);	// HT_RESULTS (1 indicates hit test is over a glyph)
        }
        #endregion

        #region MSHTML Utility Operations
        /// <summary>
        /// Synchronizes the selection with a TextRange and notifies selection listeners that the selection has changed.
        /// </summary>
        private void UpdateSelection(MarkupRange selection)
        {
            if (selection != null)
                selection.ToTextRange().select();

            //fire the selection changed event since the IHTMLTxtRange object doesn't
            (MshtmlEditor.MshtmlControl.DocumentEvents as HTMLDocumentEvents2).onselectionchange(null);
        }

        public void UpdateSelection(IHTMLControlRange selection)
        {
            if (selection != null)
                selection.select();

            //fire the selection changed event since the IHTMLControlRange object doesn't
            (MshtmlEditor.MshtmlControl.DocumentEvents as HTMLDocumentEvents2).onselectionchange(null);
        }

        protected virtual void InsertImageLink(string url, string title, bool newWindow, string rel)
        {
            throw new NotImplementedException();
        }

        public void InsertHtml(MarkupPointer start, MarkupPointer end, string html)
        {
            InsertHtml(start, end, html, null, true);
        }

        protected virtual void InsertHtml(MarkupPointer start, MarkupPointer end, string html, string sourceUrl, bool allowBlockBreakout)
        {
            MarkupRange range = MarkupServices.CreateMarkupRange(start, end);
            if (!IsValidContentInsertionPoint(range))
            {
                DisplayMessage.Show(MessageId.InvalidInsertionPoint);
                return;
            }

            Trace.Assert(start.Positioned && end.Positioned, string.Format(CultureInfo.InvariantCulture, "Invalid pointer being used for insert. start:({0}),end:({1})", start.Positioned, end.Positioned));

            // begin undo unit
            IUndoUnit undoUnit = CreateUndoUnit();

            start.PushCling(true);
            end.PushCling(true);

            using (undoUnit)
            {
                // Any changes to the way we remove the content in the destination may need to be changed in
                // KeepSourceFormatting.PasteSourceOverDestination as well!
                MarkupPointerMoveHelper.PerformImageBreakout(start);
                MarkupPointerMoveHelper.PerformImageBreakout(end);

                //if the start and endpoints are not equal, then we need to paste over the selection
                //so delete the selected region (which will collapse the pointers to now be equal.
                if (!start.IsEqualTo(end))
                {
                    //delete the selected content
                    if (ContentIsDeletableForInsert(start, end))
                    {
                        // CT: There is currently a case where we leave empty blocks behind
                        // see bug 628054. This happens when the start and end markup pointers don't completely
                        // contain the selected blocks, like: <p>|line1</p><p>line2|</p>. In this case, calling
                        // deleteNoContentNoClient will leave you with <p>|</p><p>|</p>. The next line collapses
                        // the end pointer back to the start point since that is where selection started.
                        DeleteContentNoCling(start, end);
                        end.MoveToPointer(start);
                    }
                }

                if (!string.IsNullOrEmpty(html))
                {
                    //Note: we use MarkupServices to insert the content so that IE doesn't try to fix up URLs.
                    //Element.insertAdjacentHTML() is a no-no because it rewrites relaive URLs to include
                    //the fullpath from the local filesytem.

                    //MarkupServices.ParseString() doesn't attempt to fix up URLs, so its safe to use.
                    //We will now stage the new content into a MarkupContainer, and then move it into
                    //the working document.
                    MarkupPointer sc1 = MarkupServices.CreateMarkupPointer();
                    MarkupPointer sc2 = MarkupServices.CreateMarkupPointer();

                    // Do the work ahead of time to get an <p></P> ready to be inserted
                    // doing this work after the insert this html was called with sometimes
                    // causes mshtml to not paint things until they are moused over(embeds)
                    // BUG: 624122, 622715
                    MarkupPointer mpStart = MarkupServices.CreateMarkupPointer();
                    MarkupPointer mpEnd = MarkupServices.CreateMarkupPointer();
                    // Make a temp document and load our ending html into it.
                    MarkupServices.ParseString(CONTENT_BODY_PADDING, mpStart, mpEnd);

                    //Create a temporary document from the html and set the start/end pointers to the
                    //start and end of the document.

                    MarkupServices.ParseString(html, sc1, sc2);

                    IHTMLDocument2 doc = sc1.GetDocument();
                    MarkupRange stagingRange = MarkupServices.CreateMarkupRange(sc1, sc2);
                    stagingRange.MoveToElement(doc.body, false);

                    // We only need to insert the ending new line if there was a div or image added
                    bool allowNewLineInsert = ShouldAllowNewLineInsert(html);

                    Trace.Assert(stagingRange.Positioned && stagingRange.Start.Positioned && stagingRange.End.Positioned && sc1.Positioned && sc2.Positioned, String.Format(CultureInfo.InvariantCulture, "Staging document is not ready. stagingRange:({0}),stagingRange.Start:({1}),stagingRange.End:({2}),sc1:({3}),sc2:({4})", stagingRange.Positioned, stagingRange.Start.Positioned, stagingRange.End.Positioned, sc1.Positioned, sc2.Positioned));

                    try
                    {
                        // Any changes to the way we remove the content in the destination may need to be changed in
                        // KeepSourceFormatting.PasteSourceOverDestination as well!
                        bool emptyBlockRemoved;
                        if (stagingRange.ContainsElements(ElementFilters.IsBlockOrTableElement))
                        {
                            // if the destination is an empty block element then just overwrite it
                            emptyBlockRemoved = OverwriteDestinationBlockIfEmpty(start, PrimaryEditableBounds);
                            if (!emptyBlockRemoved && allowBlockBreakout)
                            {
                                // otherwise split the destination block or move outside of the block
                                MarkupHelpers.SplitBlockForInsertionOrBreakout(MarkupServices, PrimaryEditableBounds, start);
                            }
                            end.MoveToPointer(start);
                        }
                    }
                    catch (COMException ex)
                    {
                        Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "RemoveBlockOrTableElement Failed ({0},{1},{2},{4}): {3}", stagingRange.Start.Positioned, stagingRange.End.Positioned, end.Positioned, ex, start.Positioned));
                        throw;
                    }

                    InflateEmptyParagraphs(stagingRange);
                    FixUpStickyBrs(stagingRange);

                    if (HTMLDocumentHelper.IsQuirksMode(HTMLDocument))
                    {
                        ForceTablesToInheritFontColor(stagingRange);
                    }

                    Trace.Assert(stagingRange.Positioned && stagingRange.Start.Positioned && stagingRange.End.Positioned && sc1.Positioned && sc2.Positioned, String.Format(CultureInfo.InvariantCulture, "Staging document corrupt after RemoveBlockOrTableElement. stagingRange:({0}),stagingRange.Start:({1}),stagingRange.End:({2}),sc1:({3}),sc2:({4})", stagingRange.Positioned, stagingRange.Start.Positioned, stagingRange.End.Positioned, sc1.Positioned, sc2.Positioned));

                    IDisposable damageTracker = null;
                    try
                    {
                        damageTracker = CreateDamageTracking(end, true);
                    }
                    catch (COMException ex)
                    {
                        Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "CreateDamageTracking Failed ({0}): {1}", end.Positioned, ex));
                        throw;
                    }

                    Trace.Assert(stagingRange.Positioned && stagingRange.Start.Positioned && stagingRange.End.Positioned && sc1.Positioned && sc2.Positioned, String.Format(CultureInfo.InvariantCulture, "Staging document corrupt after CreateDamageTracking. stagingRange:({0}),stagingRange.Start:({1}),stagingRange.End:({2}),sc1:({3}),sc2:({4})", stagingRange.Positioned, stagingRange.Start.Positioned, stagingRange.End.Positioned, sc1.Positioned, sc2.Positioned));

                    using (damageTracker)
                    {

                        // CT: Because we don't set gravity, these pointers can end up in indeterminant positions.
                        // For example, when inserting HTML over a selection inside of a block, the start
                        // pointer can end up on the right side of the end pointer. Pushing gravity onto
                        // the pointers before we call this should provide consistent markup pointer behavior.
                        try
                        {
                            start.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Left);
                            end.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);

                            Trace.Assert(stagingRange.Positioned && stagingRange.Start.Positioned && stagingRange.End.Positioned && sc1.Positioned && sc2.Positioned, String.Format(CultureInfo.InvariantCulture, "Staging document corrupt after applying gravity. stagingRange:({0}),stagingRange.Start:({1}),stagingRange.End:({2}),sc1:({3}),sc2:({4})", stagingRange.Positioned, stagingRange.Start.Positioned, stagingRange.End.Positioned, sc1.Positioned, sc2.Positioned));

                            try
                            {
                                MarkupServices.Move(stagingRange.Start, stagingRange.End, end);
                            }
                            catch (COMException ex)
                            {
                                Trace.WriteLine(
                                    String.Format(CultureInfo.InvariantCulture, "MarkupServices.Move Failed ({0},{1},{2}): {3}",
                                                  stagingRange.Start.Positioned, stagingRange.End.Positioned,
                                                  end.Positioned, ex));
                                throw;
                            }
                        }
                        finally
                        {
                            end.PopGravity();
                            start.PopGravity();
                        }

                        if (allowNewLineInsert && TidyWhitespace)
                        {
                            try
                            {
                                EnsureNewLineAtDocEnd(mpStart, mpEnd);
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("Failed to insert new line at end of document: " + ex);
                            }

                        }

                    }
                }
                // note that we have completed our edit
                undoUnit.Commit();

                start.PopCling();
                end.PopCling();

            }

        }

        protected void InflateEmptyParagraphs(MarkupRange range)
        {
            IHTMLElementFilter paragraphFilter = ElementFilters.CreateCompoundElementFilter(
                    ElementFilters.CreateTagIdFilter("p"),
                    ElementFilters.CreateTagIdFilter("div")
                );

            foreach (IHTMLElement paragraphElement in range.GetElements(paragraphFilter, true))
            {
                // We want only the "empty" paragraphs (e.g. <p>&nbsp;</p> or <p><br /></p>).
                bool paragraphIsEmpty = false;

                string innerHtml = paragraphElement.innerHTML ?? string.Empty;
                if (innerHtml.Equals("&nbsp;", StringComparison.OrdinalIgnoreCase))
                {
                    // Covers the case of <p>&nbsp;</p>.
                    paragraphIsEmpty = true;
                }
                else if (String.IsNullOrEmpty(paragraphElement.innerText))
                {
                    IHTMLElementCollection children = (IHTMLElementCollection)paragraphElement.children;
                    if (children != null && children.length == 1)
                    {
                        IHTMLElementCollection childBrs = (IHTMLElementCollection)children.tags("br");
                        if (childBrs != null && childBrs.length == 1)
                        {
                            // Covers the case of <p><br></p>.
                            paragraphIsEmpty = true;
                        }
                    }
                }

                if (paragraphIsEmpty)
                {
                    // Force MSHTML to render it even though its empty.
                    IHTMLElement3 paragraphElement3 = (IHTMLElement3)paragraphElement;
                    paragraphElement3.inflateBlock = true;

                    // MSHTML will add the &nbsp; back in anyway, but in a special way so that it does not appear
                    // selectable to the user. It seems to be their internal way of doing inflateBlock.
                    paragraphElement.innerHTML = "";
                }
            }
        }

        /// <summary>
        /// WinLive 107762: In IE8, the cursor can get stuck if a user continues to tap an arrow key in a single
        /// direction. To work around the issue, we wrap the &lt;br&gt;s in a single &lt;div&gt;.
        /// </summary>
        protected void FixUpStickyBrs(MarkupRange range)
        {
            IHTMLElementFilter brFilter = ElementFilters.CreateTagIdFilter(MarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_BR));

            foreach (IHTMLElement br in range.GetElements(brFilter, true))
            {
                MarkupRange brRange = MarkupServices.CreateMarkupRange(br, true);

                // We want to be moving from left to right to get the correct _MARKUP_CONTEXT_TYPE.
                brRange.Start.Left(true);
                MarkupContext beforeBr = brRange.Start.Right(true);
                MarkupContext afterBr = brRange.End.Right(false);

                if (beforeBr.Element is IHTMLDivElement && afterBr.Element is IHTMLDivElement)
                {
                    // There are three cases that IE8 screws up:
                    //   1. <div><br><div>...</div></div>
                    //   2. <div>...</div><br><div>...</div>
                    //   3. <div><div>...</div><br></div>
                    // The solution is to make sure the <br> is directly wrapped in a single <div>:
                    //   ...<div><br></div>...
                    if (!(beforeBr.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope &&
                        afterBr.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope))
                    {
                        HtmlStyleHelper.WrapRangeInElement(MarkupServices, brRange, _ELEMENT_TAG_ID.TAGID_DIV);
                    }
                }
            }
        }

        /// <summary>
        /// WinLive 209110: When IE renders tables in quirks mode, the table inherits its font color from the body
        /// element. We want the table to inherit its font color normally, so we force it to.
        /// </summary>
        protected void ForceTablesToInheritFontColor(MarkupRange range)
        {
            IHTMLElementFilter tableFilter = ElementFilters.CreateTagIdFilter(MarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TABLE));

            foreach (IHTMLElement table in range.GetElements(tableFilter, true))
            {
                if (table.style.color == null)
                {
                    IHTMLElement2 tableParent = (IHTMLElement2)table.parentElement;
                    if (tableParent != null)
                    {
                        // Setting the color to "inherit" doesn't work. We have to manually specify a color.
                        table.style.color = tableParent.currentStyle.color;
                    }
                }
            }
        }

        /// <summary>
        /// Decides if, after the given HTML is inserted, we should insert a new line.
        /// </summary>
        protected virtual bool ShouldAllowNewLineInsert(string html)
        {
            SimpleHtmlParser p = new SimpleHtmlParser(html);
            for (Element el; null != (el = p.Next());)
            {
                if (el is BeginTag && (((BeginTag)el).NameEquals("div") || ((BeginTag)el).NameEquals("img")))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TidyWhitespace { get; set; }

        private IDisposable CreateDamageTracking(MarkupPointer end, bool p)
        {
            return _damageServices.CreateDamageTracker(end, end, true);
        }

        private void EnsureNewLineAtDocEnd(MarkupPointer mpStart, MarkupPointer mpEnd)
        {
            // Add a <p></p> at the end of the document if there isn't one.
            MarkupPointer endBody = MarkupServices.CreateMarkupPointer();
            endBody.MoveAdjacentToElement(PostBodyElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);

            // If we have to insert the <p> this is where we will do it, so save it for later
            MarkupPointer insertLocation = endBody.Clone();

            // Move left one more time to find out if it is p with a space in it.
            endBody.Left(true);

            // Check to see if it a p with a space.  We use innerHtml because if the html looked like
            // <p><img ...>&nbsp;</p> innerText only reports " " though this isnt really just a blank line.
            // We dont use HtmlUtils.HtmlToText because it will replace img tags as a space.
            if (endBody.CurrentScope.tagName != "P" || endBody.CurrentScope.innerHTML != "&nbsp;")
            {
                // Move it into the real document
                IHTMLDocument2 docInsert = mpStart.GetDocument();
                MarkupRange stagingRangeInsert = MarkupServices.CreateMarkupRange(mpStart, mpEnd);
                stagingRangeInsert.MoveToElement(docInsert.body, false);
                MarkupServices.Move(stagingRangeInsert.Start, stagingRangeInsert.End, insertLocation);
            }
        }

        protected virtual bool ContentIsDeletableForInsert(MarkupPointer start, MarkupPointer end)
        {
            return true;
        }

        private bool OverwriteDestinationBlockIfEmpty(MarkupPointer destinationStart, MarkupRange bounds)
        {
            bool blockOverwritten = false;
            IHTMLElement parentBlockElement = destinationStart.GetParentElement(ElementFilters.BLOCK_ELEMENTS);
            if (parentBlockElement != null)
            {
                MarkupRange parentBlockRange = MarkupServices.CreateMarkupRange(parentBlockElement, false);
                if (bounds.InRange(parentBlockRange, false))
                {
                    if (PrimaryEditableBounds == null || PrimaryEditableBounds.InRange(parentBlockRange))
                    {
                        if (parentBlockRange.IsEmptyOfContent())
                        {
                            parentBlockRange.MoveToElement(parentBlockElement, true);
                            DeleteContentNoCling(parentBlockRange.Start, parentBlockRange.End);
                            // Check to see if the delete we just did caused the insertion point to be
                            // to become a no longer positioned, and if it did we move it to the start
                            // of what we deleted
                            if (!destinationStart.Positioned)
                            {
                                //Debug.WriteLine("Invalid pointer after delete, moving the target pointer to the start of the deleted markup.");
                                destinationStart.MoveToPointer(parentBlockRange.Start);
                            }

                            blockOverwritten = true;
                        }
                    }
                }
            }
            return blockOverwritten;
        }

        void IHtmlMarshallingTarget.InsertHtml(MarkupPointer start, MarkupPointer end, string html, string sourceUrl)
        {
            InsertHtml(start, end, html, sourceUrl, true);
        }

        /// <summary>
        /// When splitting HTML blocks on insertion, this gives subclasses a chance
        /// to specify regions within which those splits can occur--any insertion
        /// outside the bounds will not be subject to a split.
        /// </summary>
        protected virtual MarkupRange PrimaryEditableBounds
        {
            get { return null; }
        }

        public void InsertPlainText(MarkupPointer start, MarkupPointer end, string text)
        {
            IUndoUnit undo = CreateUndoUnit();
            using (undo)
            {
                if (!start.IsEqualTo(end))
                {
                    DeleteContentNoCling(start, end);
                }

                if (start.CurrentScope.tagName == "PRE")
                {
                    //if pasting into a preblock, then we want to preserve the whitespace in
                    //the text, and we don't want to convert line breaks into <BR>.  MSHTML
                    //is really nasty about tossing out whitespace, but will keep its hands
                    //off the text as long as it has <pre> tags around it

                    //hack MSHTML's whitespace mangling by the text wrapping in a <pre> tag
                    //for insertion, and then yank it.
                    text = "<pre>" + HttpUtility.HtmlEncode(text) + "</pre>";
                    MarkupRange range = MarkupServices.CreateMarkupRange();
                    MarkupContainer c = MarkupServices.ParseString(text, range.Start, range.End);
                    range.MoveToElement((IHTMLElement)(c.Document.body as IHTMLDOMNode).firstChild, false);
                    MarkupServices.Move(range.Start, range.End, start);
                }
                else
                {
                    //if this insn't wrapped in a <pre> tag, then use a textRange to insert the
                    //text so that it will be padded with <BR> and &nbsp.
                    MarkupRange range = MarkupServices.CreateMarkupRange(start, end);
                    IHTMLTxtRange txtRange = range.ToTextRange();
                    txtRange.text = text;
                }

                undo.Commit();
            }
        }

        /// <summary>
        /// Utility for deleting a range of content while suppressing pointer cling.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void DeleteContentNoCling(MarkupPointer start, MarkupPointer end)
        {
            start.PushCling(false);
            end.PushCling(false);

            try
            {
                DeleteContent(start, end);
            }
            finally
            {
                start.PopCling();
                end.PopCling();
            }
        }

        /// <summary>
        /// Delete the content between 2 pointers from the document.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void DeleteContent(MarkupPointer start, MarkupPointer end)
        {
            MarkupRange range = MarkupServices.CreateMarkupRange(start, end);
            DeleteContent(range);
        }

        /// <summary>
        /// Delete the content between a markup range from the document.
        /// </summary>
        /// <param name="selection"></param>
        private void DeleteContent(MarkupRange selection)
        {
            //there is nothing being deleted, so just return
            //Note: this avoids overhead associated with needlessly notifying that the content changed
            if (selection.Start.IsEqualTo(selection.End))
            {
                return;
            }

            IUndoUnit undo = CreateUndoUnit();
            using (undo)
            {
                try
                {
                    selection.RemoveContent();
                }
                catch (Exception e)
                {
                    Trace.Fail("error while deleting selection: " + e.Message, e.StackTrace);
                    throw e;
                }

                undo.Commit();
            }
        }

        /// <summary>
        /// Get the element at the specified screen point (returns null if there is no element)
        /// </summary>
        /// <param name="screenPoint">screen point to check</param>
        /// <returns>element at point (or null if none)</returns>
        public IHTMLElement ElementAtPoint(Point screenPoint)
        {
            Point clientPoint = EditorControl.PointToClient(screenPoint);
            if (PointIsOverDocumentArea(clientPoint.X, clientPoint.Y))
                return HTMLDocument.elementFromPoint(clientPoint.X, clientPoint.Y);
            else
                return null;
        }

        /// <summary>
        /// Check to see if the specified point is directly over the document (client rectangle
        /// excluding scrollbars if they are visible)
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns>true if it is in the client area, otherwise false</returns>
        private bool PointIsOverDocumentArea(int x, int y)
        {
            // get references to body element
            IHTMLElement body = HTMLDocument.body;
            IHTMLElement2 body2 = (IHTMLElement2)body;

            // see if the point is over one of the scrollbars
            bool pointOverVerticalScrollBar = x >= (EditorControl.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth);
            bool pointOverHorizontalScrollBar = ((body.offsetWidth - body2.scrollWidth) < SystemInformation.VerticalScrollBarWidth) && (y >= EditorControl.ClientRectangle.Height - SystemInformation.HorizontalScrollBarHeight);

            // return true if it is not over one of the two scroll-bars
            return (!(pointOverVerticalScrollBar || pointOverHorizontalScrollBar));
        }
        #endregion

        #region IHtmlMarshallingTarget Members

        IWin32Window IHtmlMarshallingTarget.FrameWindow
        {
            get { return _mainFrameWindow; }
        }

        public void Invoke(ThreadStart func)
        {
            _mainFrameWindow.BeginInvoke(new InvokeInUIThreadDelegate(func), null);
        }

        IHTMLDocument2 IHtmlMarshallingTarget.HtmlDocument
        {
            get
            {
                return HTMLDocument;
            }
        }

        bool IHtmlMarshallingTarget.IsEditable
        {
            get
            {
                return Editable;
            }
        }

        MshtmlMarkupServices IHtmlMarshallingTarget.MarkupServices
        {
            get
            {
                return MarkupServices;
            }
        }

        public bool MarshalImagesSupported
        {
            get { return marshalImagesSupported; }
            set { marshalImagesSupported = value; }
        }
        private bool marshalImagesSupported;

        public bool MarshalFilesSupported
        {
            get { return marshalFilesSupported; }
            set { marshalFilesSupported = value; }
        }
        private bool marshalFilesSupported;

        public bool MarshalHtmlSupported
        {
            get { return marshalHtmlSupported; }
            set { marshalHtmlSupported = value; }
        }
        private bool marshalHtmlSupported;

        public bool MarshalTextSupported
        {
            get { return marshalTextSupported; }
            set { marshalTextSupported = value; }
        }
        private bool marshalTextSupported;

        public bool MarshalUrlSupported
        {
            get { return marshalUrlSupported; }
            set { marshalUrlSupported = value; }
        }
        private bool marshalUrlSupported;

        #endregion

        #region IHtmlEditor Members

        public Control EditorControl
        {
            get
            {

                if (!_mshtmlInit)
                {
                    _mshtmlEditor.RightToLeft = RightToLeft.No;
                    _mshtmlInit = true;
                }
                return _mshtmlEditor;
                //if ( _editorControl == null )
                //{
                //    _editorControl = new BorderControl();
                //    _editorControl.Control = _mshtmlEditor ;
                //    _editorControl.SuppressBottomBorder = true;
                //    _editorControl.RightToLeft = RightToLeft.No;
                //}
                //return _editorControl;
            }
        }

        private bool _mshtmlInit = false;
        //private BorderControl _editorControl ;

        public IHtmlEditorCommandSource CommandSource
        {
            get { return this; }
        }

        private bool _editableRegionActive;

        public virtual bool FullyEditableRegionActive
        {
            get { return _editableRegionActive; }
            set { _editableRegionActive = value; }
        }

        /// <summary>
        /// Load the contents of the control from a file
        /// </summary>
        /// <param name="filePath"></param>
        public virtual void LoadHtmlFile(string filePath)
        {
            // load the content
            _mshtmlEditor.LoadFromFile(filePath);
        }

        public virtual void LoadHtmlString(string html)
        {
            _mshtmlEditor.LoadFromString(html);
        }

        public string GetEditedHtml(bool preferWellFormed)
        {
            Debug.Assert(undoDepth == 0, "This call should not be surrounded by an undo as is.  The temporary fixup will cause all changes in the current undo stack to rollback!");

            // tell editor to ignore next notify for purposes of dirty tracking
            // (the next notify will be the undo of the temporary fixups and
            // this shouldn't count)
            string html = GetEditedHtmlCore(preferWellFormed);

            TemporaryFixupArgs args = new TemporaryFixupArgs(html);
            if (PerformTemporaryFixupsToEditedHtml != null)
                PerformTemporaryFixupsToEditedHtml(args);

            return args.Html;
        }

        public virtual string GetEditedHtmlFast()
        {
            return GetEditedHtmlCore(false);
        }

        protected abstract string GetEditedHtmlCore(bool preferWellFormed);

        /// <summary>
        /// Is the document currently dirty?
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return _mshtmlEditor.IsDirty;
            }
            set
            {
                _mshtmlEditor.IsDirty = value;
            }
        }
        public event EventHandler IsDirtyEvent
        {
            add
            {
                _mshtmlEditor.IsDirtyEvent += value;
            }
            remove
            {
                _mshtmlEditor.IsDirtyEvent -= value;
            }

        }

        public bool SuspendAutoSave
        {
            get
            {
                return undoDepth > 0;
            }
        }

        public string SelectedText
        {
            get
            {
                if (_mshtmlEditor.HasContiguousSelection)
                {
                    return SelectedMarkupRange.Text;

                }
                else
                {
                    return null;
                }
            }
        }

        public string SelectedHtml
        {
            get
            {
                if (_mshtmlEditor.HasContiguousSelection)
                {
                    return SelectedMarkupRange.HtmlText;
                }
                else
                {
                    return null;
                }
            }
        }

        public virtual void EmptySelection()
        {
            if (HTMLDocument.readyState == "complete" && HTMLDocument.selection != null)
                HTMLDocument.selection.empty();
        }

        public void InsertHtml(string html, bool moveSelectionRight)
        {
            InsertHtml(html, moveSelectionRight ? HtmlInsertionOptions.MoveCursorAfter : HtmlInsertionOptions.Default);
        }

        /// <summary>
        /// Returns true if the selection was made, otherwise false
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        protected virtual bool SelectByTagId(IHTMLElement toSelect)
        {
            _ELEMENT_TAG_ID tagId = MarkupServices.GetElementTagId(toSelect);
            switch (tagId)
            {
                case _ELEMENT_TAG_ID.TAGID_IMG:
                    SelectImage(toSelect);
                    return true;
                default:
                    Debug.Fail("Unexpected tag id.");
                    return false;
            }
        }

        public void InsertHtml(string html, HtmlInsertionOptions options)
        {
            //Use gravity/cling to make the selection pointers to stay with the outer edges
            //of the content so that they are surrounding the new HTML after the insertion
            //is completed.
            MarkupRange range;

            switch (options & (HtmlInsertionOptions.InsertAtBeginning | HtmlInsertionOptions.InsertAtEnd))
            {
                case HtmlInsertionOptions.InsertAtBeginning:
                    range = MarkupServices.CreateMarkupRange(PostBodyElement, false);
                    range.Collapse(true);
                    break;
                case HtmlInsertionOptions.InsertAtEnd:
                    range = MarkupServices.CreateMarkupRange(PostBodyElement, false);
                    range.Collapse(false);
                    break;
                case HtmlInsertionOptions.Default:
                    range = SelectedMarkupRange;
                    break;
                case HtmlInsertionOptions.InsertAtBeginning | HtmlInsertionOptions.InsertAtEnd:
                    throw new ArgumentException("Invalid flag combination: InsertAtBeginning|InsertAtEnd");
                default:
                    throw new ArgumentException("Invalid flag combination");
            }

            Trace.Assert(range != null, "SelectedMarkupRange should not be null");
            Trace.Assert(range.Start.Positioned && range.End.Positioned, "InsertHtml with bad markup pointers");

            range.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            range.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            range.Start.Cling = false;
            range.End.Cling = false;

            bool allowBlockBreakout = (options & HtmlInsertionOptions.AllowBlockBreakout) == HtmlInsertionOptions.AllowBlockBreakout;
            InsertHtml(range.Start, range.End, html, null, allowBlockBreakout);

            // Now drive the selection to the left or right of the newly inserted
            bool selectHandled = false;
            MarkupRange selectionRange = range.Clone();
            if (HtmlInsertionOptions.SelectFirstControl == (options & HtmlInsertionOptions.SelectFirstControl))
            {
                IHTMLElement[] controlElements = range.GetElements(ElementFilters.CreateControlElementFilter(), true);
                for (int i = 0; !selectHandled && i < controlElements.Length; i++)
                {
                    selectHandled = SelectByTagId(controlElements[i]);
                }
            }

            if (!selectHandled)
            {
                selectionRange.Collapse(HtmlInsertionOptions.MoveCursorAfter != (options & HtmlInsertionOptions.MoveCursorAfter));

                IHTMLTxtRange textRange = selectionRange.ToTextRange();
                if (textRange != null)
                    textRange.select();
            }

            // Allow subclasses to do postprocessing on the inserted HTML
            OnInsertHtml(range, options);
        }

        protected virtual void OnInsertHtml(MarkupRange newContentRange, HtmlInsertionOptions options)
        {
            if (HtmlInsertionOptions.ClearUndoStack == (options & HtmlInsertionOptions.ClearUndoStack))
            {
                ClearUndoStack();
            }
        }

        public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            MarkupRange mRange = CleanUpRange();
            InsertLink(url, linkText, linkTitle, rel, newWindow, mRange);
        }

        public void InsertLink(string url, string linkText, string title, string rel, bool newWindow, MarkupRange range)
        {
            string html = HtmlGenerationService.GenerateHtmlFromLink(url, linkText, title, rel, newWindow);
            if (range == null)
            {
                range = SelectedMarkupRange;
            }
            IHTMLTxtRange txtRange = range.ToTextRange();
            if (txtRange.text != null)
            {
                int length = txtRange.text.TrimEnd(null).Length;
                txtRange.moveEnd("CHARACTER", length - txtRange.text.Length);
                if (txtRange.text != null)
                {
                    length = txtRange.text.Length;
                    int startLength = txtRange.text.TrimStart(null).Length;
                    txtRange.moveStart("CHARACTER", length - startLength);
                }
                range.MoveToTextRange(txtRange);
            }

            //put the cursor at the end of the link, but outside of it
            range.Start.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Left);
            range.End.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Right);

            try
            {
                InsertHtml(range.Start, range.End, html);
                range.Start.MoveToPointer(range.End);
                range.ToTextRange().select();
            }
            finally
            {
                range.Start.PopGravity();
                range.End.PopGravity();
            }

        }

        public void SelectImage(IHTMLElement imageElement)
        {
            // see if the image is already selected
            IHTMLImgElement selectedImage = ((IHtmlEditorComponentContext)this).Selection.SelectedImage;
            if (selectedImage != null)
                if (Marshal.GetIUnknownForObject(imageElement) == Marshal.GetIUnknownForObject(selectedImage))
                    return;

            SelectControlElement(imageElement as IHTMLControlElement);
        }

        public class InitialInsertionNotify : IDisposable
        {
            private HtmlEditorControl _editor;
            public InitialInsertionNotify(HtmlEditorControl editor)
            {
                Debug.Assert(editor != null, "Editor is unexpectedly null!");
                _editor = editor;
                _editor.FireBeforeInitialInsertion();
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                _editor.FireAfterInitialInsertion();
            }

            #endregion
        }

        public event EventHandler BeforeInitialInsertion;
        public event EventHandler AfterInitialInsertion;

        protected void FireBeforeInitialInsertion()
        {
            if (BeforeInitialInsertion != null)
                BeforeInitialInsertion(this, EventArgs.Empty);
        }

        protected void FireAfterInitialInsertion()
        {
            if (AfterInitialInsertion != null)
                AfterInitialInsertion(this, EventArgs.Empty);
        }

        public void SelectControlElement(IHTMLControlElement controlElement)
        {
            using (new InitialInsertionNotify(this))
            {
                // see if the image is already selected
                IHTMLElement selectedControl = ((IHtmlEditorComponentContext)this).Selection.SelectedControl;
                if (selectedControl != null)
                    if (Marshal.GetIUnknownForObject(controlElement) == Marshal.GetIUnknownForObject(selectedControl))
                        return;

                // select the image
                IHTMLTextContainer textContainer = HTMLDocument.body as IHTMLTextContainer;
                IHTMLControlRange controlRange = textContainer.createControlRange() as IHTMLControlRange;
                controlRange.add(controlElement);
                UpdateSelection(controlRange);
            }
        }

        public void Find()
        {
            GetMshtmlCommand(IDM.FIND).Execute();
        }

        private bool _isSpellChecking;
        /// <summary>
        /// Check the spelling of the document, returning true if the user completed the spelling check
        /// </summary>
        /// <returns>false if they cancelled the spelling check or if we're already in the middle of executing a spell check</returns>
        public bool CheckSpelling(string contextDictionaryPath)
        {
            if (!Editable || _isSpellChecking)
                return false;

            try
            {
                _isSpellChecking = true;

                // create an undo unit for the spell-check
                IUndoUnit undoUnit = CreateUndoUnit();

                // save the current selection because it will be lost during spell checking
                MarkupRange previousMarkupRange = null;
                bool previousMarkupRangeCollapsed = true;

                if (SelectedMarkupRange != null)
                {
                    previousMarkupRange = SelectedMarkupRange.Clone();
                    previousMarkupRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
                    previousMarkupRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

                    // if the current selection is collapsed we'll make sure it stays collapsed.
                    // the selection can grow if its inside a misspelled word that gets corrected.
                    previousMarkupRangeCollapsed = previousMarkupRange.IsEmpty();
                }

                // must first force the control to lose focus so that it doesn't "lose"
                // the selection when the dialog opens
                IntPtr hPrevious = User32.SetFocus(IntPtr.Zero);

                bool ignoreOnceSupported = CommandManager.Get(CommandId.IgnoreOnce).On;

                // check spelling
                bool fCompleted = false;

                //ToDo: OLW Spell Checker
                //using (SpellCheckerForm spellCheckerForm = new SpellCheckerForm(SpellingChecker, EditorControl.FindForm(), ignoreOnceSupported))
                //{
                //  center the spell-checking form over the document body
                //spellCheckerForm.StartPosition = FormStartPosition.CenterParent;

                // determine whether we are checking a selection or the whole document
                // get selection
                IHTMLSelectionObject selection = HTMLDocument.selection;
                bool checkSelection = (selection != null) && (selection.type.ToLower(CultureInfo.InvariantCulture) == "text");

                // get the word range to check
                // MshtmlWordRange wordRange = new MshtmlWordRange(HTMLDocument, checkSelection, IgnoreRangeForSpellChecking, new DamageFunction(_damageServices.AddDamage));

                //spellCheckerForm.WordIgnored += (sender, args) => OnSpellCheckWordIgnored(wordRange.CurrentWordRange);

                // check spelling
                using (undoUnit)
                {
                    //spellCheckerForm.CheckSpelling(wordRange, contextDictionaryPath);
                    undoUnit.Commit();
                }

                // reselect what was selected previous to spell-checking
                if (previousMarkupRange != null)
                {
                    if (previousMarkupRangeCollapsed)
                        previousMarkupRange.Collapse(true);

                    previousMarkupRange.ToTextRange().select();
                }

                // return completed status
                fCompleted = true; // spellCheckerForm.Completed;
                //}

                if (fCompleted && (_mainFrameWindow != null)) // && (_mainFrameWindow is IWordRangeProvider))
                {
                    // Spell check the subject, it doesn't support the "ignore once" feature
                    //using (SpellCheckerForm spellCheckerForm = new SpellCheckerForm(SpellingChecker, EditorControl.FindForm(), false))
                    {
                        //  center the spell-checking form over the document body
                        //spellCheckerForm.StartPosition = FormStartPosition.CenterParent;

                        //IWordRangeProvider wordRangeProvider = (IWordRangeProvider)_mainFrameWindow;
                        //IWordRange wordRangeSubject = wordRangeProvider.GetSubjectSpellcheckWordRange();

                        //spellCheckerForm.CheckSpelling(wordRangeSubject, contextDictionaryPath);

                        //wordRangeProvider.CloseSubjectSpellcheckWordRange();
                        fCompleted = true; // spellCheckerForm.Completed;
                    }
                }

                // restore focus to the control that had it before we spell-checked
                User32.SetFocus(hPrevious);

                return fCompleted;

            }
            finally
            {
                _isSpellChecking = false;
            }
        }

        protected abstract void OnSpellCheckWordIgnored(MarkupRange range);

        public virtual bool IgnoreRangeForSpellChecking(MarkupRange range)
        {
            return false;
        }

        public event EventHandler DocumentComplete;
        /// <summary>
        /// Notification to subclasses that the document is complete
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDocumentComplete(EventArgs e)
        {
            if (DocumentComplete != null)
                DocumentComplete(this, e);

            // OnDocumentComplete can be called multiple times with the same MshtmlEditor, so we need to make sure that
            // we don't have multiple hooks into the same event.
            MshtmlEditor.PreHandleEvent -= new HtmlEditDesignerEventHandler(OnPreHandleEvent);
            MshtmlEditor.PreHandleEvent += new HtmlEditDesignerEventHandler(OnPreHandleEvent);

            if (linkNavigator == null) // Don't new one of these up every time- only if we don't have one yet.
                linkNavigator = new EditorLinkNavigator(this, FrameWindow, _statusBar, this.DocumentEvents);
        }

        public event EventHandler GotFocus;
        protected void htmlEditor_GotFocus(object sender, EventArgs e)
        {
            if (GotFocus != null)
                GotFocus(sender, e);
        }

        public event EventHandler LostFocus;
        protected void htmlEditor_LostFocus(object sender, EventArgs e)
        {
            if (LostFocus != null)
                LostFocus(sender, e);
        }

        protected virtual void AttachBehaviors(IHtmlEditorComponentContext context)
        {
        }

        protected virtual void DetachBehaviors()
        {
        }

        #endregion

        #region IHtmlEditorCommandSource Members

        private void BeginSelectionChangedCaching()
        {
            _canApplyFormatting = GetMshtmlCommand(IDM.BOLD).Enabled;
        }

        private void EndSelectionChangedCaching()
        {
            _canApplyFormatting = null;
        }

        private bool? _canApplyFormatting = null;
        bool IHtmlEditorCommandSource.CanApplyFormatting(CommandId? commandId)
        {
            return !SelectionIsInvalid && (_canApplyFormatting ?? GetMshtmlCommand(IDM.BOLD).Enabled);
        }

        string IHtmlEditorCommandSource.SelectionFontFamily
        {
            get
            {
                string fontFamily = GetMshtmlCommand(IDM.FONTNAME).GetValue() as string;
                return fontFamily ?? String.Empty;
            }
        }

        public MshtmlFontWrapper CurrentDefaultFont { get; set; }

        public void UpdateOptions(MshtmlOptions options, bool updateComposeSettings)
        {
            if (options.EditingOptions.Contains(IDM.COMPOSESETTINGS))
            {
                CurrentDefaultFont = new MshtmlFontWrapper(options.EditingOptions[IDM.COMPOSESETTINGS].ToString());
                CurrentDefaultFont.ApplyFontToBody((IHTMLDocument2)PostBodyElement.document);
            }

            MshtmlEditor.UpdateOptions(options, updateComposeSettings);
        }

        void _mshtmlControl_DLControlFlagsChanged(object sender, EventArgs e)
        {
            // Make sure we keep our copy of the dlctl flags up to date.
            _mshtmlOptions.DLCTLOptions = MshtmlEditor.MshtmlControl.AmbientDLControl;
        }

        void IHtmlEditorCommandSource.ApplyFontFamily(string fontFamily)
        {
            GetMshtmlCommand(IDM.FONTNAME).Execute(fontFamily);
        }

        public void DisableDefaultFont()
        {
            GetMshtmlCommand(IDM.HTMLEDITMODE).Execute(false);
        }

        public void EnableDefaultFont()
        {
            GetMshtmlCommand(IDM.HTMLEDITMODE).Execute(true);
        }

        /// <summary>
        /// Returns the point font size, e.g. 14pt.
        /// Returns 0 if the selection's point font size cannot be determined.
        /// </summary>
        float IHtmlEditorCommandSource.SelectionFontSize
        {
            get
            {
                float fontSize = 0;

                try
                {
                    if (_initialDocumentLoaded)
                    {
                        // Check to see if mshtml can give us the font size before we try to find it on our own
                        object idmFontSize = GetMshtmlCommand(IDM.FONTSIZE).GetValue();
                        if (idmFontSize is IConvertible && !(idmFontSize is DBNull))
                            return HTMLElementHelper.HtmlFontSizeToPointFontSize(Convert.ToInt32(idmFontSize, CultureInfo.InvariantCulture));

                        MarkupRange selection = Selection.SelectedMarkupRange;
                        if (!selection.Positioned)
                        {
                            return 0;
                        }

                        // WinLive 195207: New behavior for returning the selection font size (aligns with Word 2010).
                        // If the selection contains no text, the control displays the font size of the start of the
                        // selection.
                        string selectionText = selection.Text ?? string.Empty;
                        if (String.IsNullOrEmpty(selectionText.Trim()))
                        {
                            return GetFontSizeAt(selection.Start);
                        }

                        // Otherwise, the selection contains text and we'll need to walk through it.
                        selection.WalkRange(
                            delegate (MarkupRange currentRange, MarkupContext context, string text)
                            {
                                text = text ?? string.Empty;
                                if (String.IsNullOrEmpty(text.Trim()))
                                {
                                    // Continue walking the range.
                                    return true;
                                }

                                IHTMLElement currentElement = currentRange.End.CurrentScope;
                                if (currentElement == null)
                                {
                                    Debug.Fail("currentElement was unexpectedly null!");

                                    // Continue walking the range.
                                    return true;
                                }

                                float currentPositionFontSize = GetFontSizeAt(currentRange.End);

                                if (fontSize == 0)
                                {
                                    // This is the first text we've found.
                                    fontSize = currentPositionFontSize;
                                }
                                else if (fontSize != currentPositionFontSize)
                                {
                                    // If the selection includes text of different sizes, the control is blank.
                                    fontSize = 0;
                                    return false;
                                }

                                // Continue walking the range.
                                return true;

                            }, false);
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail("Exception thrown when determining selection font size: " + ex);
                    return 0;
                }

                return fontSize;
            }
        }

        private float GetFontSizeAt(MarkupPointer location)
        {
            IDisplayServicesRaw displayServices = (IDisplayServicesRaw)HTMLDocument;
            IHTMLComputedStyle computedStyle;
            displayServices.GetComputedStyle(location.PointerRaw, out computedStyle);
            float pixels = DisplayHelper.TwipsToPixelsY(computedStyle.fontSize);
            float points = HTMLElementHelper.PixelsToPointSize(pixels, true);
            return points;
        }

        void IHtmlEditorCommandSource.ApplyFontSize(float fontSize)
        {
            using (IUndoUnit undoUnit = CreateUndoUnit())
            {
                // We inline the CSS font-size property on font tags when doing a paste with keep formatting, but MSHTML
                // doesn't check for the presence of this CSS property and therefore may not apply the new font size
                // correctly. To fix this we need manually to remove the inline font-sizes. We are not attempting to
                // handle all edge cases with this code path, only cases in which the content was pasted with keep
                // formatting. See the KeepSourceFormatting class for implementation details.
                MarkupRange selection = SelectedMarkupRange.Clone();

                // Make sure we expand the selection to include any <font> tags that may be wrapping us.
                MarkupPointerMoveHelper.MoveUnitBounded(selection.Start, MarkupPointerMoveHelper.MoveDirection.LEFT,
                                                        MarkupPointerAdjacency.BeforeVisible, PostBodyElement);
                MarkupPointerMoveHelper.MoveUnitBounded(selection.End, MarkupPointerMoveHelper.MoveDirection.RIGHT,
                                                        MarkupPointerAdjacency.BeforeVisible, PostBodyElement);

                selection.WalkRange(
                    delegate (MarkupRange currentRange, MarkupContext context, string text)
                    {
                        IHTMLElement currentElement = context.Element;
                        if (currentElement != null && context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope)
                        {
                            if (MarkupServices.GetElementTagId(currentElement) == _ELEMENT_TAG_ID.TAGID_FONT &&
                               !String.IsNullOrEmpty(currentElement.style.fontSize as string))
                            {
                                currentElement.style.fontSize = string.Empty;
                            }
                        }

                        return true;

                    }, true);

                GetMshtmlCommand(IDM.FONTSIZE).Execute(HTMLElementHelper.PointFontSizeToHtmlFontSize(fontSize));

                undoUnit.Commit();
            }
        }

        /// <summary>
        /// Returns color as ARGB
        /// </summary>
        int IHtmlEditorCommandSource.SelectionForeColor
        {
            get
            {
                object color;
                try
                {
                    // This is BGR
                    color = GetMshtmlCommand(IDM.FORECOLOR).GetValue();
                }
                catch (Exception ex)
                {
                    // WinLive 105991: Carry on inspite of OLECMDERR_E_DISABLED (0x80040101)
                    Trace.WriteLine("Exception thrown when getting selection fore color: " + ex);
                    return 0;
                }

                if (color == DBNull.Value)
                    return 0;

                int value = SystemColors.WindowText.ToArgb();
                try
                {
                    if (color is int)
                        value = ColorHelper.BGRToColor((int)color).ToArgb();
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to convert fore color. Exception: " + e);
                }
                return value;
            }
        }

        int IHtmlEditorCommandSource.SelectionBackColor
        {
            get
            {
                object color = GetMshtmlCommand(IDM.BACKCOLOR).GetValue();

                if (color == DBNull.Value)
                    return 0;

                // @RIBBON TODO: Figure out why this sometimes fails to convert to an int.
                try
                {
                    return ColorHelper.BGRToColor((int)color).ToArgb();
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to convert back color. Exception: " + e);
                    return SystemColors.Window.ToArgb();
                }
            }
        }

        void IHtmlEditorCommandSource.ApplyFontForeColor(int color)
        {
            GetMshtmlCommand(IDM.FORECOLOR).Execute(ColorHelper.ColorToString(Color.FromArgb(color)));
        }

        void IHtmlEditorCommandSource.ApplyFontBackColor(int? color)
        {
            if (_initialDocumentLoaded)
            {
                MarkupRange selectedMarkupRange = SelectedMarkupRange;
                if (IsValidContentInsertionPoint(selectedMarkupRange))
                {
                    using (IUndoUnit undo = CreateUndoUnit())
                    {
                        MarkupRange selection = SelectedMarkupRange;
                        if (selection.IsEmpty())
                            return;

                        using (DamageServices.CreateDamageTracker(selection, true))
                        {
                            if (color == null)
                            {
                                // WinLive 111477: Sometimes IDM.BACKCOLOR just doesn't do the job, so we
                                // use our own method here.
                                //GetMshtmlCommand(IDM.BACKCOLOR).Execute(null);
                                HtmlStyleHelper.ClearBackgroundColor(MarkupServices, selection);
                            }
                            else
                            {
                                GetMshtmlCommand(IDM.BACKCOLOR).Execute(ColorHelper.ColorToString(Color.FromArgb(color.Value)));
                            }
                        }

                        if (selection.IsEmpty())
                        {
                            DisplayServices.TraceMoveToMarkupPointer(CaretPositionDisplayPointer, selection.Start);
                            SynchronizeCaretWithDisplayPointer(CaretPositionDisplayPointer, true);
                        }
                        else
                        {
                            // We want to collapse the selection and move it outside of the highlighted region.
                            // Just trying to move the selection outside of the <font> tag doesn't work,
                            // because MSHTML maintains internal state that we are within a font tag and
                            // if you continue to type, then the new text highlighted also.
                            // This is a bit hacky, but it does what we want.
                            selection.Collapse(false);
                            selection.ToTextRange().select();
                            GetMshtmlCommand(IDM.BACKCOLOR).Execute(null);

                            //reselect the selected text.
                            selection.ToTextRange().select();
                        }

                        undo.Commit();
                    }
                }
            }
        }

        public void ViewSource()
        {
            GetMshtmlCommand(IDM.VIEWSOURCE).Execute();
        }

        void IHtmlEditorCommandSource.ClearFormatting()
        {
            if (_initialDocumentLoaded)
            {
                MarkupRange selectedMarkupRange = SelectedMarkupRange;
                if (IsValidContentInsertionPoint(selectedMarkupRange))
                {
                    using (IUndoUnit undo = CreateUndoUnit())
                    {
                        MarkupRange selection = SelectedMarkupRange;
                        using (DamageServices.CreateDamageTracker(selection, true))
                        {
                            try
                            {
                                // Note: that if the selection is non-empty, the mshtml command REMOVEFORMAT applies to word level.
                                GetMshtmlCommand(IDM.REMOVEFORMAT).Execute();

                                // REMOVEFORMAT does not remove :h1, h2, h3, h4, h5, h6, ul, ol, li.
                                // We have to remove these ourselves.
                                // First, we change h1, h2, h3, h4, h5, h6, and li to p
                                // Then we remove ul and ol.

                                // Any list items should now be paragraph elements
                                _ELEMENT_TAG_ID[] toBeParagraph = new _ELEMENT_TAG_ID[] { _ELEMENT_TAG_ID.TAGID_LI,
                                                                                      _ELEMENT_TAG_ID.TAGID_H1,
                                                                                      _ELEMENT_TAG_ID.TAGID_H2,
                                                                                      _ELEMENT_TAG_ID.TAGID_H3,
                                                                                      _ELEMENT_TAG_ID.TAGID_H4,
                                                                                      _ELEMENT_TAG_ID.TAGID_H5,
                                                                                      _ELEMENT_TAG_ID.TAGID_H6,
                                                                                    };
                                if (!IsEditFieldSelected)
                                {
                                    HtmlStyleHelper.ChangeElementTagIds(MarkupServices, selection, toBeParagraph, _ELEMENT_TAG_ID.TAGID_P);

                                    MarkupRange parentBlockRange = MarkupServices.CreateMarkupRange(selection.ParentBlockElement());
                                    foreach (_ELEMENT_TAG_ID tagId in new[] {_ELEMENT_TAG_ID.TAGID_UL,
                                                                     _ELEMENT_TAG_ID.TAGID_OL,
                                                                     _ELEMENT_TAG_ID.TAGID_BLOCKQUOTE})
                                    {
                                        parentBlockRange.RemoveElementsByTagId(tagId, false);
                                    }

                                    // We'll remove the alignment by removing the align attribute at block level
                                    HtmlStyleHelper.RemoveAttributes(MarkupServices, parentBlockRange, new[] { "align" });
                                }

                                //reselect the selected text.
                                if (selection.IsEmpty())
                                {
                                    DisplayServices.TraceMoveToMarkupPointer(CaretPositionDisplayPointer, selection.Start);
                                    SynchronizeCaretWithDisplayPointer(CaretPositionDisplayPointer, true);
                                }
                                else
                                    selection.ToTextRange().select();

                            }
                            catch (Exception ex)
                            {
                                Trace.Fail("Exception thrown during ClearFormatting: " + ex);
                            }
                        }

                        undo.Commit();
                    }
                }
            }
        }

        private bool IsSelection(_ELEMENT_TAG_ID tagId)
        {
            if (_initialDocumentLoaded)
            {
                return SelectedMarkupRange.IsTagId(tagId, true);
            }
            return false;
        }

        bool IHtmlEditorCommandSource.SelectionSuperscript
        {
            get
            {
                // IDM.SUPERSCRIPT is not supported, so we'll have to roll our own.
                //return GetMshtmlCommand(IDM.SUPERSCRIPT).Latched;
                return IsSelection(_ELEMENT_TAG_ID.TAGID_SUP);
            }
        }

        void IHtmlEditorCommandSource.ApplySuperscript()
        {
            // IDM.SUPERSCRIPT is not supported, so we'll have to roll our own.
            // GetMshtmlCommand(IDM.SUPERSCRIPT).Execute();
            ApplyInlineTag(_ELEMENT_TAG_ID.TAGID_SUP, null, true);
        }

        bool IHtmlEditorCommandSource.SelectionSubscript
        {
            get
            {
                // IDM.SUBSCRIPT is not supported, so we'll have to roll our own.
                //return GetMshtmlCommand(IDM.SUBSCRIPT).Latched;
                return IsSelection(_ELEMENT_TAG_ID.TAGID_SUB);
            }
        }

        void IHtmlEditorCommandSource.ApplySubscript()
        {
            // IDM.SUBSCRIPT is not supported, so we'll have to roll our own.
            //GetMshtmlCommand(IDM.SUBSCRIPT).Execute();
            ApplyInlineTag(_ELEMENT_TAG_ID.TAGID_SUB, null, true);
        }

        private void ApplyInlineTag(_ELEMENT_TAG_ID tagId, string attributes, bool toggle)
        {
            if (_initialDocumentLoaded)
            {
                MarkupRange selectedMarkupRange = SelectedMarkupRange;
                if (IsValidContentInsertionPoint(selectedMarkupRange))
                {
                    using (IUndoUnit undo = CreateUndoUnit())
                    {
                        MarkupRange selection = SelectedMarkupRange;
                        MarkupRange newSelection;
                        using (DamageServices.CreateDamageTracker(selection, true))
                        {
                            newSelection = HtmlStyleHelper.ApplyInlineTag(MarkupServices, tagId, attributes, selection, toggle);

                            if (!newSelection.Positioned)
                            {
                                Debug.Fail("Shouldn't have applied inline tag because there was nothing to apply it to.");
                                return;
                            }
                        }

                        if (newSelection.IsEmpty())
                        {
                            DisplayServices.TraceMoveToMarkupPointer(CaretPositionDisplayPointer, newSelection.Start);
                            SynchronizeCaretWithDisplayPointer(CaretPositionDisplayPointer, true);
                        }
                        else
                        {
                            //reselect the selected text.
                            newSelection.ToTextRange().select();
                        }

                        undo.Commit();
                    }
                }
            }
        }

        string IHtmlEditorCommandSource.SelectionStyleName
        {
            get
            {
                if (_initialDocumentLoaded)
                {
                    try
                    {
                        MarkupRange selection = SelectedMarkupRange;
                        if (selection != null)
                        {
                            IHTMLElement element = SelectedMarkupRange.ParentBlockElement();
                            if (element != null)
                                return element.tagName.ToLower(CultureInfo.InvariantCulture);
                        }
                    }
                    catch (Exception)
                    {
                        //avoids MSHTML error reported while getting stylename (bug 287515)
                        //Debug.Fail("Error getting stylename", e.ToString());
                    }
                }
                return null;
            }
        }

        void IHtmlEditorCommandSource.ApplyHtmlFormattingStyle(IHtmlFormattingStyle style)
        {
            if (_initialDocumentLoaded)
            {
                MarkupRange selectedMarkupRange = SelectedMarkupRange;
                IHTMLElement maxEditableElement = null;
                MarkupRange maxEditableRange = MarkupServices.CreateMarkupRange();
                IHTMLElement[] editableElements = EditableElements;
                for (int i = editableElements.Length - 1; i >= 0 && maxEditableElement == null; i--)
                {
                    maxEditableRange.MoveToElement(editableElements[i], false);
                    if (maxEditableRange.InRange(selectedMarkupRange.Start) || maxEditableRange.InRange(selectedMarkupRange.End))
                        maxEditableElement = editableElements[i];
                }
                if (maxEditableElement != null)
                {
                    using (IUndoUnit undo = CreateUndoUnit())
                    {
                        MarkupRange selection = SelectedMarkupRange;
                        using (DamageServices.CreateDamageTracker(selection, true))
                        {
                            HtmlBlockFormatHelper.ApplyBlockStyle(this, style.ElementTagId, selection, maxEditableRange, selection.Clone());
                        }

                        //reselect the selected text.
                        selection.ToTextRange().select();
                        undo.Commit();
                    }
                }
            }
        }

        bool IHtmlEditorCommandSource.SelectionBold
        {
            get
            {
                return GetMshtmlCommand(IDM.BOLD).Latched;
            }
        }

        void IHtmlEditorCommandSource.ApplyBold()
        {
            using (IUndoUnit undoUnit = CreateUndoUnit())
            {
                BoldApplier boldApplier = new BoldApplier(MarkupServices, SelectedMarkupRange, GetMshtmlCommand(IDM.BOLD));
                boldApplier.Execute();

                undoUnit.Commit();
            }
        }

        public void RemoveFormat()
        {
            MarkupRange range = MarkupServices.CreateMarkupRange(PostBodyElement, true);
            IHTMLTxtRange txtRange = range.ToTextRange();
            txtRange.execCommand("RemoveFormat", false, null);
        }

        bool IHtmlEditorCommandSource.SelectionItalic
        {
            get
            {
                return GetMshtmlCommand(IDM.ITALIC).Latched;
            }
        }

        void IHtmlEditorCommandSource.ApplyItalic()
        {
            GetMshtmlCommand(IDM.ITALIC).Execute();
        }

        bool IHtmlEditorCommandSource.SelectionUnderlined
        {
            get
            {
                return GetMshtmlCommand(IDM.UNDERLINE).Latched;
            }
        }

        void IHtmlEditorCommandSource.ApplyUnderline()
        {
            GetMshtmlCommand(IDM.UNDERLINE).Execute();
        }

        bool IHtmlEditorCommandSource.SelectionStrikethrough
        {
            get
            {
                return GetMshtmlCommand(IDM.STRIKETHROUGH).Latched;
            }
        }

        void IHtmlEditorCommandSource.ApplyStrikethrough()
        {
            GetMshtmlCommand(IDM.STRIKETHROUGH).Execute();
        }

        bool IHtmlEditorCommandSource.SelectionIsLTR
        {
            get { return SelectionHasDir(ElementFilters.IsLTRElement); }
        }

        void IHtmlEditorCommandSource.InsertLTRTextBlock()
        {
            ApplyDirectionChange("ltr", ElementFilters.IsLTRElement);
        }

        bool IHtmlEditorCommandSource.SelectionIsRTL
        {
            get { return SelectionHasDir(ElementFilters.IsRTLElement); }
        }

        void IHtmlEditorCommandSource.InsertRTLTextBlock()
        {
            ApplyDirectionChange("rtl", ElementFilters.IsRTLElement);
        }

        private delegate bool ElementFilter(IHTMLElement x);

        private bool SelectionHasDir(ElementFilter filter)
        {
            if (Selection != null)
            {
                try
                {
                    MarkupRange bounds = FindBoundaries(Selection.SelectedMarkupRange);

                    // All block elements can have the language direction attribute applied to them.
                    List<IHTMLElement> blocks = GetBlockElements(bounds);

                    List<MarkupRange> unwrappedContentRanges = GetUnwrappedContentRanges(blocks, bounds);
                    if (unwrappedContentRanges.Count > 0 || blocks.Count == 0)
                    {
                        // Unwrapped content ranges inherit their language direction from their parent block element.
                        blocks.Add(bounds.ParentBlockElement());
                    }

                    // Return true if all the selected content has the same direction.
                    return blocks.TrueForAll(e => filter(e));
                }
                catch (Exception e)
                {
                    //Eat error that seems to commonly occur when the selection is in an invalid state
                    //(like when it is stale from a previously loaded document!). If this error message
                    //is displayed, we need to figure out why the selection object is invalid.
                    Debug.Fail("Selection object is in an invalid state. Figure out why!", e.ToString());
                }
            }

            return false;
        }

        public DefaultBlockElement DefaultBlockElement
        {
            get;
            private set;
        }

        private void ApplyDirectionChange(string direction, ElementFilter filter)
        {
            using (IUndoUnit undo = CreateUndoUnit())
            {
                MarkupRange bounds = FindBoundaries(Selection.SelectedMarkupRange);

                List<IHTMLElement> blocks = GetBlockElements(bounds);

                List<MarkupRange> unwrappedContentRanges = GetUnwrappedContentRanges(blocks, bounds);
                foreach (MarkupRange contentRange in unwrappedContentRanges)
                {
                    blocks.Add(HtmlStyleHelper.WrapRangeInElement(MarkupServices, contentRange, this.DefaultBlockElement.TagId));
                }

                if (blocks.Count == 0)
                {
                    // If there are no block elements at all, create one.
                    IHTMLElement newElement = HtmlStyleHelper.WrapRangeInElement(MarkupServices, bounds, this.DefaultBlockElement.TagId);
                    blocks.Add(newElement);

                    // Make sure the selection is inside of it.
                    MarkupRange selection = Selection.SelectedMarkupRange.Clone();
                    selection.MoveToElement(newElement, false);
                    selection.ToTextRange().select();
                }

                ApplyDirectionChangeToBlocks(blocks, direction, filter);

                undo.Commit();
            }
        }

        private void ApplyDirectionChangeToBlocks(List<IHTMLElement> elements, string direction, ElementFilter filter)
        {
            using (IUndoUnit undo = CreateUndoUnit())
            {
                foreach (IHTMLElement element in elements)
                {
                    element.removeAttribute("dir", 0);
                    if (!filter(element))
                    {
                        element.setAttribute("dir", direction, 0);

                        // If the element is empty, MSHTML won't render the cursor on the correct side of the canvas
                        // until the the users starts typing. We can force the empty element to render correctly by
                        // setting the inflateBlock property.
                        if (String.IsNullOrEmpty(element.innerHTML))
                        {
                            ((IHTMLElement3)element).inflateBlock = true;
                        }
                    }
                }
                undo.Commit();
            }
        }

        /// <summary>
        /// Returns a list of MarkupRanges, where each MarkupRange surrounds HTML content that is not wrapped in an
        /// in-scope block element.
        /// </summary>
        private List<MarkupRange> GetUnwrappedContentRanges(List<IHTMLElement> elements, MarkupRange range)
        {
            List<MarkupRange> unwrappedContentRanges = new List<MarkupRange>();

            MarkupPointer rangeStart = range.Start.Clone();
            MarkupPointer rangeEnd = range.End.Clone();
            MarkupPointer elementStart = MarkupServices.CreateMarkupPointer();

            for (int i = 0; !rangeStart.IsRightOfOrEqualTo(rangeEnd) && i < elements.Count; i++)
            {
                elementStart.MoveAdjacentToElement(elements[i], _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                if (rangeStart.IsLeftOf(elementStart))
                {
                    MarkupRange unBlockedRange = MarkupServices.CreateMarkupRange(rangeStart, elementStart);
                    if (!unBlockedRange.IsEmptyOfContent())
                    {
                        unwrappedContentRanges.Add(unBlockedRange.Clone());
                    }
                }

                rangeStart.MoveAdjacentToElement(elements[i], _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            }

            //unblocked text after the block elements
            if (rangeStart.IsLeftOf(rangeEnd))
            {
                MarkupRange unBlockedRange = MarkupServices.CreateMarkupRange(rangeStart, rangeEnd);
                if (!unBlockedRange.IsEmptyOfContent())
                {
                    unwrappedContentRanges.Add(unBlockedRange.Clone());
                }
            }

            return unwrappedContentRanges;
        }

        private MarkupRange FindBoundaries(MarkupRange range)
        {
            MarkupRange newRange = range.Clone();

            // WinLive 194115: Find out if the MarkupPointer is in a table cell (without going outside the PostBodyElement).
            IHTMLElementFilter tableCellFilter = ElementFilters.CreateCompoundElementFilter(ElementFilters.TABLE_CELL_ELEMENT, ElementFilters.CreateElementEqualsFilter(PostBodyElement));

            IHTMLElement startElement = range.Start.GetParentElement(tableCellFilter);
            if (startElement == null || HTMLElementHelper.ElementsAreEqual(startElement, PostBodyElement))
            {
                startElement = range.Start.CurrentBlockScope();
            }

            IHTMLElement endElement = range.End.GetParentElement(tableCellFilter);
            if (endElement == null || HTMLElementHelper.ElementsAreEqual(endElement, PostBodyElement))
            {
                endElement = range.End.CurrentBlockScope();
            }

            IHTMLElement postBodyElement = PostBodyElement;
            if (startElement != null)
            {
                if (startElement != postBodyElement)
                    newRange.Start.MoveAdjacentToElement(startElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                else
                {
                    IHTMLElement previousBlock = newRange.Start.SeekElementLeft(ElementFilters.IsBlockOrTableCellElement);
                    if (previousBlock == postBodyElement)
                        newRange.Start.MoveAdjacentToElement(startElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    else
                        newRange.Start.MoveAdjacentToElement(previousBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                }
            }
            if (endElement != null)
            {
                if (endElement != postBodyElement)
                    newRange.End.MoveAdjacentToElement(endElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                else
                {
                    IHTMLElement nextBlock = newRange.End.SeekElementRight(ElementFilters.IsBlockOrTableCellElement);
                    if (nextBlock == postBodyElement)
                        newRange.End.MoveAdjacentToElement(endElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                    else
                        newRange.End.MoveAdjacentToElement(nextBlock, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
                }
            }
            return newRange;
        }

        private List<IHTMLElement> GetBlockElements(MarkupRange bounds)
        {
            return new List<IHTMLElement>(bounds.GetTopLevelBlocksAndCells(ElementFilters.IsBlockOrTableCellElement, true));
        }

        EditorTextAlignment IHtmlEditorCommandSource.GetSelectionAlignment()
        {
            if (GetMshtmlCommand(IDM.JUSTIFYLEFT).Latched)
                return EditorTextAlignment.Left;
            else if (GetMshtmlCommand(IDM.JUSTIFYCENTER).Latched)
                return EditorTextAlignment.Center;
            else if (GetMshtmlCommand(IDM.JUSTIFYRIGHT).Latched)
                return EditorTextAlignment.Right;
            else if (GetMshtmlCommand(IDM.JUSTIFYFULL).Latched)
                return EditorTextAlignment.Justify;
            else
                return EditorTextAlignment.None;
        }

        void IHtmlEditorCommandSource.ApplyAlignment(EditorTextAlignment alignment)
        {
            switch (alignment)
            {
                case EditorTextAlignment.Left:
                    ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.JUSTIFYLEFT).Execute));
                    break;
                case EditorTextAlignment.Center:
                    ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.JUSTIFYCENTER).Execute));
                    break;
                case EditorTextAlignment.Right:
                    ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.JUSTIFYRIGHT).Execute));
                    break;
                case EditorTextAlignment.Justify:
                    ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.JUSTIFYFULL).Execute));
                    break;
                case EditorTextAlignment.None:
                    ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.JUSTIFYNONE).Execute));
                    break;
            }
        }

        bool IHtmlEditorCommandSource.SelectionBulleted
        {
            get
            {
                return GetMshtmlCommand(IDM.UNORDERLIST).Latched;
            }
        }

        void IHtmlEditorCommandSource.ApplyBullets()
        {
            ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.UNORDERLIST).Execute));
        }

        bool IHtmlEditorCommandSource.SelectionNumbered
        {
            get
            {
                return GetMshtmlCommand(IDM.ORDERLIST).Latched;
            }
        }

        void IHtmlEditorCommandSource.ApplyNumbers()
        {
            ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.ORDERLIST).Execute));
        }

        bool IHtmlEditorCommandSource.SelectionBlockquoted
        {
            get
            {
                try
                {
                    IHTMLElementFilter filter = ElementFilters.CreateElementNameFilter("BLOCKQUOTE");
                    MarkupRange selection = SelectedMarkupRange;
                    if (selection != null && selection.Positioned)
                    {
                        return selection.Start.GetParentElement(filter) != null
                            || selection.End.GetParentElement(filter) != null
                            || selection.ContainsElements(filter);
                    }
                }
                catch (Exception e)
                {
                    //Eat error that seems to commonly occur when the selection is in an invalid state
                    //(like when it is stale from a previously loaded document!). If this error message
                    //is displayed, we need to figure out why the selection object is invalid.
                    Debug.Fail("Selection object is in an invalid state. Figure out why!", e.ToString());
                }
                return false;
            }
        }

        void IHtmlEditorCommandSource.ApplyBlockquote()
        {
            ExecuteBlockCommand(new CommandExecutor(applyBlockquote));
        }

        private void applyBlockquote()
        {
            MarkupRange selection = SelectedMarkupRange;
            selection.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            selection.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            using (IUndoUnit undo = CreateUndoUnit())
            {
                if (((IHtmlEditorCommandSource)this).SelectionBlockquoted)
                {
                    GetMshtmlCommand(IDM.OUTDENT).Execute();
                }
                else
                {
                    GetMshtmlCommand(IDM.INDENT).Execute();
                    IHTMLElement blockquoteElement = selection.Start.GetParentElement(ElementFilters.CreateElementNameFilter("BLOCKQUOTE"));
                    if (blockquoteElement != null)
                    {
                        //MSHTML's blockquoting adds extra dir/style attrs, which is not nice, so we replace
                        //the newly inserted blockquote element with a squeaky clean blockquote element.
                        //Note: that the undo command started throwing exceptions when we tried to remove the dir/style attributes
                        //so replacing the element was the next best approach to sterilizing the blockquote.
                        IHTMLElement element = MarkupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_BLOCKQUOTE, null);
                        MarkupServices.ReplaceElement(blockquoteElement, element);

                        // Special case when on a blank document, if there is an empty p in the blockquote
                        // we should remove it because it makes the cursor look like it is in a funny spot
                        if (element.innerHTML == CONTENT_BODY_PADDING)
                            element.innerHTML = "";
                    }
                }
                undo.Commit();
            }
        }

        bool IHtmlEditorCommandSource.CanIndent
        {
            get { return CanIndent; }
        }

        void IHtmlEditorCommandSource.ApplyIndent()
        {
            ApplyIndent();
        }

        bool IHtmlEditorCommandSource.CanOutdent
        {
            get { return ((IHtmlEditorCommandSource)this).CanApplyFormatting(CommandId.Outdent); }
        }

        protected virtual bool CanIndent
        {
            get { return ((IHtmlEditorCommandSource)this).CanApplyFormatting(CommandId.Indent); }
        }

        protected virtual void ApplyIndent()
        {
            ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.INDENT).Execute));
        }

        void IHtmlEditorCommandSource.ApplyOutdent()
        {
            ExecuteBlockCommand(new CommandExecutor(GetMshtmlCommand(IDM.OUTDENT).Execute));
        }

        bool IHtmlEditorCommandSource.CanInsertLink
        {
            get
            {
                return Editable;
            }
        }

        LinkInfo IHtmlEditorCommandSource.DiscoverCurrentLink()
        {
            String hyperlink = null;
            String text = null;
            String title = null;
            String rel = null;
            bool newWindow = false;

            IHTMLElement anchor = GetCurrentEditableAnchorElement();

            // default values for hyperlink form
            if (anchor != null)
            {
                //note: use getAttribute("href", 2") to get the unresolved version of the path.
                hyperlink = anchor.getAttribute("href", 2) as string;
                //if current hyperlink is to image or is empty, there won't be any anchor text
                if (anchor.innerText != null)
                {
                    //if we selected more than the anchor, expand the link text to include selection
                    MarkupRange selectedRange = SelectedMarkupRange;
                    MarkupRange anchorRange = MarkupServices.CreateMarkupRange(anchor);
                    if (selectedRange.End.IsRightOf(anchorRange.End))
                        anchorRange.End.MoveToPointer(selectedRange.End);
                    if (selectedRange.Start.IsLeftOf(anchorRange.Start))
                        anchorRange.Start.MoveToPointer(selectedRange.Start);

                    text = anchorRange.Text;
                }
                title = anchor.getAttribute("title", 0) as string;
                rel = anchor.getAttribute("rel", 0) as string;
                if ((string)anchor.getAttribute("target", 0) == "_blank")
                {
                    newWindow = true;
                }
            }
            else
            {
                text = SelectedMarkupRange.Text;
            }
            return new LinkInfo(text, hyperlink, title, rel, newWindow);
        }

        void IHtmlEditorCommandSource.InsertLink()
        {
            // allow for inserting links even when there is no text selection (insert a
            // link w/ the user specified title at the current caret location). Wasn't exactly
            // sure how to do this....

            // show our "insert-link" dialog
            using (new WaitCursor())
            {
                //check out the selection to see if it is appropriate use of insert hyperlink
                bool _isImageOnly = false;
                if ((this as IHtmlEditorComponentContext).Selection.SelectedImage != null)
                {
                    _isImageOnly = true;

                }
                else if (!FullyEditableRegionActive || SelectedMarkupRange.GetElements(ElementFilters.VISIBLE_EMPTY_ELEMENTS, true).Length > 0)
                {
                    DisplayMessage.Show(MessageId.NotLinkable);
                    return;
                }
                else if (ContainsMultipleAnchors()) //contains multiple links
                {
                    DisplayMessage.Show(MessageId.MultipleLinks);
                    return;
                }

                //else is text only

                using (HyperlinkForm hyperlinkForm = new HyperlinkForm(this.CommandManager, ShowAllHyperlinkOptions))
                {

                    LinkInfo info = ((IHtmlEditorCommandSource)this).DiscoverCurrentLink();

                    hyperlinkForm.ContainsImage = _isImageOnly;

                    if (info.AnchorText != null && !_isImageOnly)
                    {
                        hyperlinkForm.LinkText = info.AnchorText;
                    }

                    //if not editing a current link, check glossary for auto-populate info
                    if (info.Url == null && !_isImageOnly)
                    {
                        if (info.AnchorText != null)
                        {
                            GlossaryLinkItem item = GlossaryManager.Instance.FindEntry(info.AnchorText.Trim());
                            if (item != null)
                            {
                                if (item.Url != String.Empty) hyperlinkForm.Hyperlink = item.Url;
                                if (item.Title != String.Empty) hyperlinkForm.LinkTitle = item.Title;
                                hyperlinkForm.IsInGlossary = true;
                            }
                        }
                    }
                    else if (info.Url != null)
                    {
                        // don't set the url if it is pointing to a local "backing file" for
                        // the current image thumbnail
                        if (!UrlIsTemporaryLocalFilePath(info.Url))
                            hyperlinkForm.Hyperlink = info.Url;
                        if (info.LinkTitle != null) hyperlinkForm.LinkTitle = info.LinkTitle;
                        if (info.Rel != null) hyperlinkForm.Rel = info.Rel;
                        hyperlinkForm.NewWindow = info.NewWindow;
                        if (!_isImageOnly)
                        {
                            hyperlinkForm.IsInGlossary =
                                GlossaryManager.Instance.FindExactEntry(hyperlinkForm.LinkText, hyperlinkForm.Hyperlink, hyperlinkForm.LinkTitle);
                        }
                    }

                    //tell whether this is edit style
                    hyperlinkForm.EditStyle = info.Url != null;

                    // show the dialog to edit the link
                    DialogResult linkDialogResult = hyperlinkForm.ShowDialog(EditorControl.FindForm());
                    if (linkDialogResult != DialogResult.Cancel)
                    {
                        IUndoUnit undoUnit = CreateUndoUnit();
                        using (undoUnit)
                        {
                            if (_isImageOnly)
                            {
                                if (linkDialogResult == DialogResult.OK)
                                {
                                    InsertImageLink(hyperlinkForm.Hyperlink, hyperlinkForm.LinkTitle, hyperlinkForm.NewWindow, hyperlinkForm.Rel);
                                }
                                else
                                {
                                    InsertImageLink(String.Empty, String.Empty, false, String.Empty);
                                }
                            }
                            else
                            {

                                ExpandAnchorSelection();
                                MarkupRange mRange = CleanUpRange();

                                //add the link if this is add/edit
                                if (linkDialogResult == DialogResult.OK)
                                {
                                    InsertLink(hyperlinkForm.Hyperlink, hyperlinkForm.LinkText, hyperlinkForm.LinkTitle, hyperlinkForm.Rel, hyperlinkForm.NewWindow, mRange);
                                }

                            }
                            // commit the change
                            undoUnit.Commit();
                        }
                    }
                }
            }
        }

        protected abstract bool ShowAllHyperlinkOptions { get; }

        protected virtual bool UrlIsTemporaryLocalFilePath(string url)
        {
            return false;
        }

        bool IHtmlEditorCommandSource.CanRemoveLink
        {
            get
            {
                return GetMshtmlCommand(IDM.UNLINK).Enabled;
            }
        }

        void IHtmlEditorCommandSource.RemoveLink()
        {
            GetMshtmlCommand(IDM.UNLINK).Execute();
        }

        bool IHtmlEditorCommandSource.CanFind
        {
            get
            {
                return true;
            }
        }

        void IHtmlEditorCommandSource.OpenLink()
        {
            string href = String.Empty;
            try
            {
                // get href
                IHTMLElement anchor = GetCurrentEditableAnchorElement();

                //note: this sometimes throws an exception, such as when the link is www.goog&nbsp; le.com. But we do want to fix up paths here
                // so using "0" as flag seems required (using 2 wouldn't throw the exception, but wouldn't work with relative URLs).
                href = anchor.getAttribute("href", 0) as string;

                // launch it using the shell
                if (UrlHelper.IsUrl(href))
                    Process.Start(href);
            }
            catch (Exception e)
            {
                Trace.Fail("Unexpected failure to navigate to link " + href, e.Message);
            }
        }

        void IHtmlEditorCommandSource.AddToGlossary()
        {
            IHTMLElement anchor = GetCurrentEditableAnchorElement();
            if (anchor != null)
                GlossaryManager.Instance.AddEntry(anchor.innerText, (string)anchor.getAttribute("href", 2), (string)anchor.getAttribute("title", 2), (string)anchor.getAttribute("rel", 2), ((string)anchor.getAttribute("target", 2)) == "_blank");
        }

        #endregion

        #region IHtmlEditorComponentContext Members

        IMainFrameWindow IHtmlEditorComponentContext.MainFrameWindow
        {
            get
            {
                return _mainFrameWindow;
            }
        }

        MshtmlMarkupServices IHtmlEditorComponentContext.MarkupServices
        {
            get
            {
                return MarkupServices;
            }
        }

        public IHTMLEditorDamageServices DamageServices
        {
            get { return _damageServices; }
        }

        Point IHtmlEditorComponentContext.PointToClient(Point p)
        {
            return MshtmlEditor.PointToClient(p);
        }

        Point IHtmlEditorComponentContext.PointToScreen(Point p)
        {
            return MshtmlEditor.PointToScreen(p);
        }

        IHTMLElement IHtmlEditorComponentContext.ElementFromClientPoint(Point clientPoint)
        {
            return MshtmlEditor.HTMLDocument.elementFromPoint(clientPoint.X, clientPoint.Y);
        }

        bool IHtmlEditorComponentContext.PointIsOverDocumentArea(Point clientPoint)
        {
            // get references to body element
            IHTMLElement body = HTMLDocument.body;
            IHTMLElement2 body2 = (IHTMLElement2)body;

            // see if the point is over one of the scrollbars
            bool pointOverVerticalScrollBar = clientPoint.X >= (MshtmlEditor.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth);
            bool pointOverHorizontalScrollBar = ((body.offsetWidth - body2.scrollWidth) < SystemInformation.VerticalScrollBarWidth) && (clientPoint.Y >= MshtmlEditor.ClientRectangle.Height - SystemInformation.HorizontalScrollBarHeight);

            // return true if it is not over one of the two scroll-bars
            return (!(pointOverVerticalScrollBar || pointOverHorizontalScrollBar));
        }

        bool IHtmlEditorComponentContext.OverrideCursor
        {
            set
            {
                MshtmlEditor.MshtmlControl.ExecuteCommand(IDM.OVERRIDE_CURSOR, value);
            }
        }

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command-d</param>
        void IHtmlEditorComponentContext.ExecuteCommand(uint cmdID)
        {
            MshtmlEditor.MshtmlControl.ExecuteCommand(cmdID);
        }

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command id</param>
        /// <param name="input">input parameter</param>
        void IHtmlEditorComponentContext.ExecuteCommand(uint cmdID, object input)
        {
            MshtmlEditor.MshtmlControl.ExecuteCommand(cmdID, input);
        }

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command id</param>
        /// <param name="input">input parameter</param>
        /// <param name="output">out parameter</param>
        void IHtmlEditorComponentContext.ExecuteCommand(uint cmdID, object input, ref object output)
        {
            MshtmlEditor.MshtmlControl.ExecuteCommand(cmdID, input, ref output);
        }

        public event EventHandler SelectionChanged;

        public virtual bool IsEditFieldSelected
        {
            get { return false; }
        }

        public virtual IHTMLElement SelectedEditField
        {
            get { return null; }
        }

        public event TemporaryFixupHandler PerformTemporaryFixupsToEditedHtml;

        //used to prevent selection change events while the selection is being updated.
        private int _selectionChanging = 0;
        private readonly string editorId = Guid.NewGuid().ToString();

        void IHtmlEditorComponentContext.BeginSelectionChange()
        {
            _selectionChanging++;
        }

        void IHtmlEditorComponentContext.EndSelectionChange()
        {
            Debug.Assert(_selectionChanging >= 0, "Mismatched selection begin/end change calls.");
            _selectionChanging--;
            FireSelectionChanged();
        }

        IDictionary IHtmlEditorComponentContext.Cookies
        {
            get
            {
                return _cookies;
            }
        }
        private Hashtable _cookies = new Hashtable();

        void IHtmlEditorComponentContext.FireSelectionChanged()
        {
            this.FireSelectionChanged();
        }

        IHtmlEditorSelection IHtmlEditorComponentContext.Selection
        {
            get
            {
                if (_selection != null)
                    return _selection;
                else
                    return _defaultSelection;
            }

            set
            {
                _selection = value;
                FireSelectionChanged();
            }
        }

        DragDropEffects IHtmlEditorComponentContext.DoDragDrop(IDataObject dataObject, DragDropEffects allowedEffects)
        {
            return _mshtmlEditor.DoDragDrop(dataObject, allowedEffects);
        }

        public string EditorId
        {
            get { return editorId; }
        }

        public IMshtmlDocumentEvents DocumentEvents
        {
            get
            {
                return MshtmlEditor.MshtmlControl.DocumentEvents;
            }
        }

        private bool HasEmptySelection
        {
            get
            {
                return Selection != null && Selection.SelectedMarkupRange.IsEmpty();
            }
        }

        private bool HasContiguousSelection
        {
            get
            {
                return Selection != null && Selection.HasContiguousSelection;
            }
        }

        protected IHtmlEditorSelection Selection
        {
            get
            {
                return (this as IHtmlEditorComponentContext).Selection;
            }
        }

        event HtmlEditDesignerEventHandler IHtmlEditorComponentContext.PreHandleEvent
        {
            add
            {
                MshtmlEditor.PreHandleEvent += value;
            }
            remove
            {
                MshtmlEditor.PreHandleEvent -= value;
            }
        }

        event MshtmlEditor.EditDesignerEventHandler IHtmlEditorComponentContext.PostEventNotify
        {
            add
            {
                MshtmlEditor.PostEditorEvent += value;
            }
            remove
            {
                MshtmlEditor.PostEditorEvent -= value;
            }
        }

        event HtmlEditDesignerEventHandler IHtmlEditorComponentContext.TranslateAccelerator
        {
            add
            {
                MshtmlEditor.TranslateAccelerator += value;
            }
            remove
            {
                MshtmlEditor.TranslateAccelerator -= value;
            }
        }

        public bool ShowContextMenu(Point screenPoint)
        {
            return MshtmlEditor.ShowContextMenu(screenPoint);
        }

        #endregion

        #region ISimpleTextEditorCommandSource Members

        public bool HasFocus
        {
            get
            {
                return EditorControl.ContainsFocus;
            }
        }

        public bool CanUndo
        {
            get
            {
                return GetMshtmlCommand(IDM.UNDO).Enabled && GetTargetUndoUnit() != null;
            }
        }

        public void Undo()
        {
            try
            {
                IOleUndoUnit targetUndoUnit = GetTargetUndoUnit();

                if (targetUndoUnit != null)
                {
                    _originalText = _mshtmlEditor.HTMLDocument.body.innerText;

                    UndoManager.UndoTo(targetUndoUnit);

                    DamageChangedRegion();

                    DelayedFireDefaultSelectionChanged();
                }

            }
            catch (Exception e)
            {
                Trace.Fail("Failure executing undo", e.ToString());
            }
        }

        private void DamageChangedRegion()
        {
            // Make local copy of _originalText
            string originalText = _originalText;

            if (String.IsNullOrEmpty(originalText))
                return;

            // Setup pointers
            MarkupPointer startPointer = MarkupServices.CreateMarkupPointer(_mshtmlEditor.HTMLDocument.body, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            MarkupPointer endPointer = MarkupServices.CreateMarkupPointer(_mshtmlEditor.HTMLDocument.body, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            MarkupPointer beginDamagePointer = startPointer.Clone();
            MarkupPointer endDamagePointer = endPointer.Clone();
            int idx;

            MarkupRange bodyRange = MarkupServices.CreateMarkupRange(startPointer, endPointer);
            bodyRange.WalkRange(
                delegate (MarkupRange currentRange, MarkupContext context, string text)
                {
                    if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text &&
                        !String.IsNullOrEmpty(text))
                    {
                        idx = originalText.IndexOf(text);
                        if (idx == 0)
                        {
                            // Drop this portion from the expected string
                            originalText = originalText.Substring(text.Length);

                            // Update the current pointer
                            beginDamagePointer.MoveToPointer(currentRange.End);
                        }
                        else if (idx > 0 && originalText.Substring(0, idx).Replace("\r\n", string.Empty).Length == 0)
                        {
                            // Drop this portion from the expected string
                            originalText = originalText.Substring(text.Length + idx);
                            // Update the current pointer
                            beginDamagePointer.MoveToPointer(currentRange.End);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (currentRange.End.IsRightOfOrEqualTo(bodyRange.End))
                    {
                        beginDamagePointer = null;
                        return false;
                    }

                    return true;

                }, true);
            if (beginDamagePointer != null)
            {
                bodyRange = MarkupServices.CreateMarkupRange(beginDamagePointer, endPointer);
                bodyRange.WalkRangeReverse(
                    delegate (MarkupRange currentRange, MarkupContext context, string text)
                    {
                        if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text &&
                        !String.IsNullOrEmpty(text))
                        {
                            idx = originalText.LastIndexOf(text);
                            // If the text falls at the end of the expected text string
                            if (idx > 0 &&
                                ((originalText.Length - idx) == text.Length ||
                                originalText.Substring(idx + text.Length).Replace("\r\n", string.Empty).Length == 0))
                            {
                                // Drop this portion from the expected string
                                originalText = originalText.Substring(0, idx);
                                // Update the current pointer
                                endDamagePointer.MoveToPointer(currentRange.Start);
                            }
                            else
                            {
                                return false;
                            }
                        }

                        return true;
                    }, true);
                // Add damage
                _damageServices.AddDamage(MarkupServices.CreateMarkupRange(beginDamagePointer, endDamagePointer), true);
            }
        }

        private void DelayedFireDefaultSelectionChanged()
        {
            // force selection changed to fire (normally the execution
            // of the native UNDO command would take care of this)
            // we do this in a delayed manner so that behaviors can be
            // attached/detached before they receive selection changed
            // events (otherwise they get selection changed events first
            // and then detached, causing crashes or other strange behavior)
            MshtmlEditor.BeginInvoke(new InvokeInUIThreadDelegate(FireDefaultSelectionChanged));
        }

        private void FireDefaultSelectionChanged()
        {
            ((IHtmlEditorComponentContext)this).Selection = _defaultSelection;
        }

        private void FireSelectionChanged()
        {
            if (_selectionChanging == 0)
                OnSelectionChanged(EventArgs.Empty, _selection, true);
        }

        public bool CanRedo
        {
            get
            {
                return GetMshtmlCommand(IDM.REDO).Enabled;
            }
        }

        public void Redo()
        {
            try
            {
                IOleUndoUnit targetRedoUnit = GetTargetRedoUnit();

                if (targetRedoUnit != null)
                {
                    _originalText = _mshtmlEditor.HTMLDocument.body.innerText;

                    UndoManager.RedoTo(targetRedoUnit);

                    DamageChangedRegion();

                    DelayedFireDefaultSelectionChanged();
                }
            }
            catch (Exception e)
            {
                Trace.Fail("Failure executing redo", e.ToString());
            }
        }

        public bool CanCut
        {
            get
            {
                return !SelectionIsInvalid && Editable && (GetMshtmlCommand(IDM.CUT).Enabled || HasContiguousSelection) && !HasEmptySelection;
            }
        }

        public void Cut()
        {
            Selection.ExecuteSelectionOperation(new HtmlEditorSelectionOperation(CutSelection));
        }

        private void CutSelection(IHtmlEditorSelection selection)
        {
            using (_damageServices.CreateDeleteDamageTracker(selection.SelectedMarkupRange))
            {
                // allow override by components
                HtmlEditorSelectionOperationEventArgs ea = new HtmlEditorSelectionOperationEventArgs(selection);
                OnCut(ea);

                // if no override then execute
                if (!ea.Handled)
                    GetMshtmlCommand(IDM.CUT).Execute();
            }
        }

        public bool CanCopy
        {
            get
            {
                return !SelectionIsInvalid && Editable && HasContiguousSelection && !HasEmptySelection;
            }
        }

        public void Copy()
        {
            Selection.ExecuteSelectionOperation(new HtmlEditorSelectionOperation(CopySelection));
        }

        private void CopySelection(IHtmlEditorSelection selection)
        {
            // allow override by components
            HtmlEditorSelectionOperationEventArgs ea = new HtmlEditorSelectionOperationEventArgs(selection);
            OnCopy(ea);

            // if no override then execute
            if (!ea.Handled)
                GetMshtmlCommand(IDM.COPY).Execute();
        }

        private static bool ClipboardHasData
        {
            get
            {
                return User32.CountClipboardFormats() != 0;
            }
        }

        public virtual bool CanPaste
        {
            get
            {
                return !SelectionIsInvalid && Editable &&
                    (FullyEditableRegionActive || MarshalHtmlSupported || MarshalImagesSupported || MarshalFilesSupported || MarshalTextSupported || MarshalUrlSupported) && ClipboardHasData;
            }
        }

        bool IHtmlEditorCommandSource.CanPasteSpecial
        {
            get
            {
                return CanPaste;
            }
        }

        bool IHtmlEditorCommandSource.AllowPasteSpecial
        {
            get
            {
                return true;
            }
        }

        public void Paste()
        {
            using (new WaitCursor())
                Selection.ExecuteSelectionOperation(new HtmlEditorSelectionOperation(PasteOverSelection));
        }

        public virtual void PasteKeepFormatting()
        {
            try
            {
                CleanHtmlOnPasteOverride = false;
                Paste();
            }
            finally
            {
                CleanHtmlOnPasteOverride = null;
            }
        }

        void IHtmlEditorCommandSource.PasteSpecial()
        {
            using (new WaitCursor())
            {
                DataObjectMeister dataObject = new DataObjectMeister(Clipboard.GetDataObject());
                string html, baseUrl;
                if (dataObject.HTMLData != null)
                {
                    html = dataObject.HTMLData.HTMLSelection;
                    baseUrl = UrlHelper.GetBaseUrl(dataObject.HTMLData.SourceURL);
                    using (PasteSpecialForm pasteSpecialForm = new PasteSpecialForm())
                    {
                        if (pasteSpecialForm.ShowDialog(_mainFrameWindow) == DialogResult.OK)
                        {
                            PasteSpecialForm.PasteType formatting = pasteSpecialForm.ChosenFormatting;

                            switch (formatting)
                            {
                                //this is just the standard paste
                                case PasteSpecialForm.PasteType.Standard:
                                    using (ApplicationPerformance.LogEvent("StandardHtmlPasteSpecial"))
                                    {
                                        //special case: paste from within writer--standard paste
                                        //keeps formatting, and we want to thin
                                        if (HtmlHandler.IsPasteFromSharedCanvas(dataObject))
                                        {
                                            html = HtmlCleaner.CleanupHtml(html, baseUrl, true, false);
                                            break;
                                        }
                                        Paste();
                                    }
                                    return;
                                //just strip any scripts
                                case PasteSpecialForm.PasteType.KeepFormatting:
                                    using (ApplicationPerformance.LogEvent("KeepFormattingHtmlPasteSpecial"))
                                    {
                                        PasteKeepFormatting();
                                        return;
                                    }
                                //remove all formatting except for line breaks
                                case PasteSpecialForm.PasteType.RemoveFormatting:
                                    using (ApplicationPerformance.LogEvent("RemoveFormattingHtmlPasteSpecial"))
                                        html = HtmlCleaner.CleanupHtml(html, baseUrl, true, true, false);
                                    break;
                            }
                            html = UnsafeHtmlFragmentHelper.SterilizeHtml(html, UnsafeHtmlFragmentHelper.Flag.RemoveDocumentTags);
                            using (IUndoUnit undo = CreateUndoUnit())
                            {
                                InsertHtml(html, true);
                                undo.Commit();
                            }
                        }
                    }

                }
                else if (dataObject.TextData != null)
                {
                    html = dataObject.TextData.Text;
                    using (PasteSpecialFormText pasteSpecialForm = new PasteSpecialFormText())
                    {
                        if (pasteSpecialForm.ShowDialog(_mainFrameWindow) == DialogResult.OK)
                        {
                            PasteSpecialFormText.PasteType formatting = pasteSpecialForm.ChosenFormatting;

                            switch (formatting)
                            {
                                //this is just the standard paste
                                case PasteSpecialFormText.PasteType.Standard:
                                    using (ApplicationPerformance.LogEvent("StandardTextPasteSpecial"))
                                    using (IUndoUnit undo = CreateUndoUnit())
                                    {
                                        string htmlFromText = TextHelper.GetHTMLFromText(html, true, this.DefaultBlockElement);
                                        InsertHtml(htmlFromText, true);
                                        undo.Commit();
                                    }
                                    break;
                                //insert as is
                                case PasteSpecialFormText.PasteType.KeepFormatting:
                                    using (ApplicationPerformance.LogEvent("TextAsHtmlPasteSpecial"))
                                    {
                                        using (IUndoUnit undo = CreateUndoUnit())
                                        {
                                            html = HtmlCleaner.PreserveFormatting(html, String.Empty);
                                            InsertHtml(html, true);
                                            undo.Commit();
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                }
                else
                {
                    //non html or text data on clipboard--file, etc.
                    DisplayMessage.Show(MessageId.PasteSpecialInvalidData, _mainFrameWindow);
                }
            }
        }

        private void PasteOverSelection(IHtmlEditorSelection selection)
        {
            if (!IsValidContentInsertionPoint(selection.SelectedMarkupRange))
                return;

            try
            {
                // get the current contents of the clipboard
                DataObjectMeister clipboardMeister = new DataObjectMeister(Clipboard.GetDataObject());

                // see if one of our data handlers wants to handle the paste
                using (DataFormatHandler dataFormatHandler = DataFormatHandlerFactory.CreateFrom(clipboardMeister, DataFormatHandlerContext.ClipboardPaste))
                {
                    if (dataFormatHandler != null)
                    {
                        using (new WaitCursor())
                        {
                            // get selection
                            MarkupRange selectionRange = selection.SelectedMarkupRange.Clone();
                            selectionRange.Start.Cling = false;
                            selectionRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
                            selectionRange.End.Cling = false;
                            selectionRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

                            // do the paste
                            dataFormatHandler.InsertData(DataAction.Copy, selectionRange.Start, selectionRange.End);

                            MarkupRange newMarkUpRange = MarkupServices.CreateMarkupRange(selectionRange.End.Clone(), selectionRange.End.Clone());
                            MarkupPointerMoveHelper.DriveSelectionToLogicalPosition(newMarkUpRange, PostBodyElement, false);

                            // update selection
                            selectionRange.Start.MoveToPointer(newMarkUpRange.Start);
                            selectionRange.End.MoveToPointer(newMarkUpRange.Start);
                            selectionRange.ToTextRange().select();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //avoid throwing up error dialogs if paste fails.
                Trace.Fail("failed to paste content: " + ex.Message, ex.StackTrace);
            }
        }

        protected bool? CleanHtmlOnPasteOverride
        {
            get;
            set;
        }

        public virtual bool CleanHtmlOnPaste
        {
            get
            {
                if (CleanHtmlOnPasteOverride.HasValue)
                {
                    return CleanHtmlOnPasteOverride.Value;
                }

                return true;
            }
        }

        /// <summary>
        /// Used to track nested undo situations.  In these situations, if any undo
        /// unit is not committed, all undo units on the stack are rolled back.
        /// Note: rolling back nested undos individually is not allowed and will
        /// cause "catastrophic" errors from COM (bug 492362)
        /// </summary>
        internal static int undoDepth;
        internal int uncommittedCount;

        private readonly CommandManager _commandManager;
        public CommandManager CommandManager
        {
            get
            {
                return _commandManager;
            }
        }

        public bool CanClear
        {
            get
            {
                return !SelectionIsInvalid && Editable && (GetMshtmlCommand(IDM.DELETE).Enabled || HasContiguousSelection);
            }
        }

        public void Clear()
        {
            Selection.ExecuteSelectionOperation(new HtmlEditorSelectionOperation(ClearSelection));
        }

        private void ClearSelection(IHtmlEditorSelection selection)
        {
            using (_damageServices.CreateDeleteDamageTracker(selection.SelectedMarkupRange))
            {
                // allow override by components
                HtmlEditorSelectionOperationEventArgs ea = new HtmlEditorSelectionOperationEventArgs(selection);
                OnClear(ea);

                // if no override then execute
                if (!ea.Handled)
                {
                    GetMshtmlCommand(IDM.DELETE).Execute();
                    // Clear out any state maintained by MSHTML regarding the backcolor.
                    if (selection.SelectedMarkupRange.IsTagId(_ELEMENT_TAG_ID.TAGID_FONT, false))
                        GetMshtmlCommand(IDM.BACKCOLOR).Execute(null);
                }

                FireSelectionChanged();
            }
        }

        private void ExpandAnchorSelection()
        {
            IHTMLElement anchor = GetCurrentEditableAnchorElement();
            if (anchor != null)
            {
                //if we selected more than the anchor, expand the link text to include selection
                MarkupRange selectedRange = SelectedMarkupRange;
                MarkupRange anchorRange = MarkupServices.CreateMarkupRange(anchor);
                if (selectedRange.End.IsRightOf(anchorRange.End))
                    anchorRange.End.MoveToPointer(selectedRange.End);
                if (selectedRange.Start.IsLeftOf(anchorRange.Start))
                    anchorRange.Start.MoveToPointer(selectedRange.Start);

                anchorRange.ToTextRange().select();
            }
        }

        public void SelectAll()
        {
            GetMshtmlCommand(IDM.SELECTALL).Execute();
        }

        public void InsertEuroSymbol()
        {
            InsertHtml("&euro;", true);
        }

        bool ISimpleTextEditorCommandSource.ReadOnly
        {
            get { return !Editable; }
        }

        public event EventHandler CommandStateChanged;
        protected virtual void OnCommandStateChanged()
        {
            if (CommandStateChanged != null)
                CommandStateChanged(this, EventArgs.Empty);
        }

        public event EventHandler AggressiveCommandStateChanged;
        protected virtual void OnAggressiveCommandStateChanged()
        {
            if (AggressiveCommandStateChanged != null)
                AggressiveCommandStateChanged(this, EventArgs.Empty);
        }

        #endregion

        #region IHTMLEditHostRaw Members

        int IHTMLEditHostRaw.SnapRect(IHTMLElement pIElement, ref RECT prcNEW, _ELEMENT_CORNER elementCorner)
        {
            try
            {
                // forward SnapRect out via events
                if (SnapRectEvent != null)
                    SnapRectEvent(pIElement, ref prcNEW, elementCorner);

                // anytime the editor calls SnapRect it implies an edit -- force the editor dirty
                IsDirty = true;

                return HRESULT.S_OK;
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception during SnapRect event: " + ex.ToString());
                return HRESULT.E_FAILED;
            }
        }

        #endregion

        #region IElementBehaviorFactoryRaw Members

        // allow subclasses to implement the behavior factory by overriding
        void IElementBehaviorFactoryRaw.FindBehavior(string bstrBehavior, string bstrBehaviorUrl, IElementBehaviorSite pSite, ref IElementBehaviorRaw ppBehavior)
        {
            // Fix bug 519990: Setting ppBehavior to null, even in the failure case,
            // causes Writer to crash when an embedded Google Map is pasted into the
            // editor. If there is no behavior, DON'T TOUCH ppBehavior!

            IElementBehaviorRaw behavior;
            OnFindBehavior(bstrBehavior, bstrBehaviorUrl, pSite, out behavior);
            if (behavior != null)
                ppBehavior = behavior;
            else
                throw new NotImplementedException();
        }

        protected virtual void OnFindBehavior(string bstrBehavior, string bstrBehaviorUrl, IElementBehaviorSite pSite, out IElementBehaviorRaw ppBehavior)
        {
            // default to no behavior
            ppBehavior = null;
        }

        #endregion

        #region IServiceProviderRaw Members

        // make virtual as escpae hatch for derived classes to implement other services
        int IServiceProviderRaw.QueryService(ref Guid guid, ref Guid riid, out IntPtr service)
        {
            return OnQueryService(ref guid, ref riid, out service);
        }

        protected virtual int OnQueryService(ref Guid guid, ref Guid riid, out IntPtr service)
        {
            // Edit Host service
            Guid SID_SHTMLEditHost = new Guid(0x3050f6a0, 0x98b5, 0x11cf, 0xbb, 0x82, 0x00, 0xaa, 0x00, 0xbd, 0xce, 0x0b);
            if (guid == SID_SHTMLEditHost && riid == typeof(IHTMLEditHostRaw).GUID)
            {
                service = Marshal.GetComInterfaceForObject(this, typeof(IHTMLEditHostRaw));
                return HRESULT.S_OK;
            }

            // IInternetSecurityManager
            Guid SID_SInternetSecurityManager = typeof(IInternetSecurityManager).GUID;
            if (guid == SID_SInternetSecurityManager && riid == typeof(IInternetSecurityManager).GUID)
            {
                IInternetSecurityManager securityManager = GetInternetSecurityManager();
                if (securityManager != null)
                {
                    service = Marshal.GetComInterfaceForObject(securityManager, typeof(IInternetSecurityManager));
                    return HRESULT.S_OK;
                }
            }

            // Element behavior factory service
            Guid SID_SHTMLElementBehaviorFactory = typeof(IElementBehaviorFactoryRaw).GUID;
            if (guid == SID_SHTMLElementBehaviorFactory && riid == typeof(IElementBehaviorFactoryRaw).GUID)
            {
                service = Marshal.GetComInterfaceForObject(this, typeof(IElementBehaviorFactoryRaw));
                return HRESULT.S_OK;
            }

            // IVersionHost service.
            if (VersionHostServiceProvider.QueryService(ref guid, ref riid, out service) == HRESULT.S_OK)
            {
                return HRESULT.S_OK;
            }

            //this interface is not supported by the editor
            service = IntPtr.Zero;
            return HRESULT.E_NOINTERFACE;
        }

        #endregion

        #region IServiceProvider Hooks
        private InternetSecurityManagerShim _internetSecurityManager;
        protected virtual IInternetSecurityManager GetInternetSecurityManager()
        {
            return _internetSecurityManager;
        }

        protected virtual VersionHostServiceProvider VersionHostServiceProvider
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Converts a clientPoint to a screenPoint.
        /// </summary>
        /// <param name="clientPoint"></param>
        public Point ClientPointToScreenPoint(Point clientPoint)
        {
            return EditorControl.PointToScreen(clientPoint);
        }

        #region Implementation of IDropFeedback

        public virtual bool CanDrop(IHTMLElement scope, DataObjectMeister meister)
        {
            return false;
        }

        public virtual bool ShouldMoveDropLocationRight(MarkupPointer dropLocation)
        {
            return false;
        }

        #endregion

        public virtual bool IsSelectionMisspelled()
        {
            return false;
        }
    }

    public delegate void SnapRectEventHandler(IHTMLElement pIElement, ref RECT prcNEW, _ELEMENT_CORNER elementCorner);

    public class HtmlEditorControlOptions
    {
        public HtmlEditorControlOptions()
        {
            UseDivForCarriageReturn = true;
        }

        public bool UseDivForCarriageReturn
        {
            get { return _useDivForCarriageReturn; }
            set { _useDivForCarriageReturn = value; }
        }
        private bool _useDivForCarriageReturn;
    }

    internal class InternetSecurityManagerShim : IInternetSecurityManager
    {
        public InternetSecurityManagerShim(IInternetSecurityManager manager)
        {
            SecurityManager = manager;
        }

        public void ReleaseInnerSecurityManager()
        {
            HandleProcessUrlAction = null;

            if (SecurityManager != null && Marshal.IsComObject(SecurityManager))
                Marshal.ReleaseComObject(SecurityManager);

            // Install a dummy handler.
            SecurityManager = new InternetSecurityManager();
        }

        public IInternetSecurityManager SecurityManager { get; set; }

        public int SetSecuritySite(IntPtr pSite)
        {
            return SecurityManager.SetSecuritySite(pSite);
        }

        public int GetSecuritySite(out IntPtr pSite)
        {
            return SecurityManager.GetSecuritySite(out pSite);
        }

        public int MapUrlToZone(string pwszUrl, ref int pdwZone, int dwFlags)
        {
            return SecurityManager.MapUrlToZone(pwszUrl, ref pdwZone, dwFlags);
        }

        public int GetSecurityId(string pwszUrl, out byte pbSecurityId, ref int pcbSecurityId, IntPtr dwReserved)
        {
            return SecurityManager.GetSecurityId(pwszUrl, out pbSecurityId, ref pcbSecurityId, dwReserved);
        }

        public delegate bool HandleProcessUrlActionDelegate(
            string pwszUrl, int dwAction, out byte pPolicy, int cbPolicy, IntPtr pContext, int cbContext, int dwFlags,
            int dwReserved);

        public HandleProcessUrlActionDelegate HandleProcessUrlAction { get; set; }

        public int ProcessUrlAction(string pwszUrl, int dwAction, out byte pPolicy, int cbPolicy, IntPtr pContext, int cbContext, int dwFlags, int dwReserved)
        {
            if (HandleProcessUrlAction != null)
            {
                if (HandleProcessUrlAction(pwszUrl, dwAction, out pPolicy, cbPolicy, pContext, cbContext, dwFlags, dwReserved))
                {
                    return HRESULT.S_OK;
                }
            }
            return SecurityManager.ProcessUrlAction(pwszUrl, dwAction, out pPolicy, cbPolicy, pContext, cbContext, dwFlags, dwReserved);
        }

        public int QueryCustomPolicy(string pwszUrl, ref Guid guidKey, out byte ppPolicy, out int pcbPolicy, byte pContext, int cbContext, int dwReserved)
        {
            return SecurityManager.QueryCustomPolicy(pwszUrl, ref guidKey, out ppPolicy, out pcbPolicy, pContext, cbContext, dwReserved);
        }

        public int SetZoneMapping(int dwZone, string lpszPattern, int dwFlags)
        {
            return SecurityManager.SetZoneMapping(dwZone, lpszPattern, dwFlags);
        }

        public int GetZoneMappings(int dwZone, out IEnumString ppenumString, int dwFlags)
        {
            return SecurityManager.GetZoneMappings(dwZone, out ppenumString, dwFlags);
        }
    }

}
