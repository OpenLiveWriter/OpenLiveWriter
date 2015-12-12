// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal interface IUndoRedoExecutingChecker
    {
        bool UndoRedoExecuting();
    }

    internal class SmartContentElementBehavior : ResizableElementBehavior, IUndoRedoExecutingChecker
    {

        public const string CLICK_HANDLER = "wlClickHandler";

        private IBlogPostSidebarContext _sidebarContext;
        private IHtmlEditorComponentContext _editorContext;
        private IContentSourceSidebarContext _contentSourceContext;
        private SmartContentSource _contentSource;
        private SmartContentResizedListener _resizedListener;
        private bool _realtimeResizing = false;
        private IUndoUnit _resizeUndo;
        private bool _disposed;

        public SmartContentElementBehavior(IHtmlEditorComponentContext editorContext, IBlogPostSidebarContext sidebarContext, IContentSourceSidebarContext contentSourceContext, SmartContentResizedListener resizedListener)
            : base(editorContext)
        {
            _editorContext = editorContext;
            _sidebarContext = sidebarContext;
            _contentSourceContext = contentSourceContext;
            _resizedListener = resizedListener;
        }

        protected override bool QueryElementSelected()
        {
            if (EditorContext.Selection.SelectedMarkupRange.Positioned &&
                IsChildEditFieldSelected(HTMLElement, EditorContext.Selection.SelectedMarkupRange))
                return true;
            return base.QueryElementSelected();
        }

        public static bool IsChildEditFieldSelected(IHTMLElement parent, MarkupRange selection)
        {
            Debug.Assert(parent != null, "Parent element should not be null.");
            return GetSelectedChildEditField(parent, selection) != null;
        }

        public static IHTMLElement GetSelectedChildEditField(IHTMLElement parent, MarkupRange selection)
        {
            if (selection == null || !selection.Positioned)
            {
                Trace.Fail("Selection is invalid!");
                return null;
            }

            IHTMLElement element = selection.ParentElement();
            if (element == null || !HTMLElementHelper.IsChildOrSameElement(parent, element))
                return null;

            do
            {
                if (InlineEditField.IsEditField(element))
                    return element;

                element = element.parentElement;
            } while (element != null && element.sourceIndex != parent.sourceIndex);

            return null;
        }

        private void PersistEditFieldValues()
        {
            SmartContent content = SmartContent;

            if (content == null)
                return;

            foreach (IHTMLElement el in EditFields)
            {
                InlineEditField field = new InlineEditField(el, content, EditorContext, HTMLElement, this);
                field.PersistFieldValueToContent(true);
            }
        }

        private IEnumerable _editFields;
        private IEnumerable EditFields
        {
            get
            {
                if (_editFields == null)
                {
                    _editFields = GetChildren(HTMLElement, InlineEditField.IsEditField);
                }
                return _editFields;
            }
        }

        private delegate bool Filter(IHTMLElement el);
        private static IEnumerable GetChildren(IHTMLElement el, Filter filter)
        {
            foreach (IHTMLElement child in (IHTMLElementCollection)el.children)
            {
                if (filter(child))
                    yield return child;

                foreach (IHTMLElement descendant in GetChildren(child, filter))
                    yield return descendant;
            }
        }

        protected override void OnKeyDown(HtmlEventArgs e)
        {
            // Most arrow gestures should not be overridden if an edit
            // field is focused
            switch (e.htmlEvt.keyCode)
            {
                case (int)Keys.Left:
                case (int)Keys.Right:
                case (int)Keys.Up:
                case (int)Keys.Down:
                    if (IsChildEditFieldSelected(HTMLElement, EditorContext.Selection.SelectedMarkupRange))
                    {
                        bool altKey = (Control.ModifierKeys & Keys.Alt) > 0;
                        if (!altKey)
                            return;
                    }
                    break;
            }

            base.OnKeyDown(e);
        }

        private InlineEditField GetSelectedInlineEditField
        {
            get
            {
                IHTMLElement selectedEditField = EditorContext.SelectedEditField;
                return selectedEditField != null ?
                    new InlineEditField(selectedEditField, SmartContent, EditorContext, HTMLElement, this) :
                    null;
            }
        }

        private bool IsClickHandler(IHTMLElement element)
        {
            if (element == null)
                return false;

            string clickCommand = element.getAttribute(CLICK_HANDLER, 0) as string;
            return !String.IsNullOrEmpty(clickCommand);
        }

        protected override void UpdateCursor(bool selected, int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (ShouldProcessEvents(inEvtDispId, pIEventObj) && IsClickHandler(pIEventObj.srcElement))
            {
                EditorContext.OverrideCursor = true;
                Cursor.Current = Cursors.Hand;
            }
            else
                UpdateCursor(selected);
        }

        protected override int HandlePreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (ShouldProcessEvents(inEvtDispId, pIEventObj))
            {
                IHTMLElement el = pIEventObj.srcElement;

                if (el != null && InlineEditField.IsWithinEditField(el))
                    return HRESULT.S_FALSE;

                if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONBEFOREDEACTIVATE)
                    return HRESULT.S_FALSE;

                // Allow selection of smart content via mouse click when child edit field is selected.
                if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN)
                {
                    leftMouseDown = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
                    if (Selected &&
                        IsChildEditFieldSelected(HTMLElement, EditorContext.Selection.SelectedMarkupRange))
                    {
                        PersistEditFieldValues();
                        return HandlePreHandleEventLeftMouseButtonDown(inEvtDispId, pIEventObj);
                    }
                }
                else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP && leftMouseDown)
                {
                    // Is there a click handler to execute?
                    string clickCommand = pIEventObj.srcElement.getAttribute(CLICK_HANDLER, 0) as string;
                    if (!String.IsNullOrEmpty(clickCommand))
                    {
                        try
                        {
                            // Only allow click handlers on our built in content sources
                            string contentSourceId = ContentSourceId;
                            foreach (var v in ContentSourceManager.BuiltInContentSources)
                            {
                                if (v.Id == contentSourceId)
                                {
                                    int result = base.HandlePreHandleEvent(inEvtDispId, pIEventObj);
                                    CommandId commandId = (CommandId)Enum.Parse(typeof(CommandId), clickCommand, true);
                                    EditorContext.CommandManager.Get(commandId).PerformExecuteWithArgs(new ExecuteEventHandlerArgs());
                                    return result;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is ArgumentException || ex is OverflowException || ex is ArgumentNullException)
                            {
                                Debug.Fail("Failed to parse clickHandler: " + ex);
                            }
                            else
                                throw;
                        }
                    }
                }

                // Allow the selected edit field to receive keyboard events when the mouse is within
                // the smart content but outside of the edit field.
                if ((inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONKEYPRESS) &&
                    IsChildEditFieldSelected(HTMLElement, EditorContext.Selection.SelectedMarkupRange))
                {
                    return HRESULT.S_FALSE;
                }

            }
            else if (Selected && IsChildEditFieldSelected(HTMLElement, EditorContext.Selection.SelectedMarkupRange))
            {
                if (pIEventObj.cancelBubble)
                    return HRESULT.S_FALSE;
            }

            return base.HandlePreHandleEvent(inEvtDispId, pIEventObj);
        }

        protected override void OnElementAttached()
        {
            SmartContent content = SmartContent;
            if (content == null)
                return;

            ContentSourceInfo contentSourceInfo = _contentSourceContext.FindContentSource(ContentSourceId);
            _contentSource = contentSourceInfo.Instance as SmartContentSource;

            if (_contentSource != null && _contentSource.ResizeCapabilities != ResizeCapabilities.None)
            {
                Resizable = true;
                PreserveAspectRatio = ResizeCapabilities.PreserveAspectRatio == (_contentSource.ResizeCapabilities & ResizeCapabilities.PreserveAspectRatio);
                _realtimeResizing = ResizeCapabilities.LiveResize == (_contentSource.ResizeCapabilities & ResizeCapabilities.LiveResize);
            }
            else
                Resizable = false;

            EditorContext.CommandKey += new KeyEventHandler(EditorContext_CommandKey);
            EditorContext.DocumentEvents.DoubleClick += new HtmlEventHandler(EditorContext_DoubleClick);
            EditorContext.SelectionChanged += new EventHandler(EditorContext_SelectionChanged);
            EditorContext.PostEventNotify += new MshtmlEditor.EditDesignerEventHandler(EditorContext_PostEventNotify);
            EditorContext.CommandManager.BeforeExecute += new CommandManagerExecuteEventHandler(CommandManager_BeforeExecute);
            EditorContext.CommandManager.AfterExecute += new CommandManagerExecuteEventHandler(CommandManager_AfterExecute);
            EditorContext.HtmlInserted += new EventHandler(EditorContext_HtmlInserted);

            base.OnElementAttached();

            foreach (IHTMLElement el in EditFields)
            {
                InlineEditField field = new InlineEditField(el, content, EditorContext, HTMLElement, this);

                if (!field.ContentEditable && EditorContext.EditMode)
                {
                    field.ContentEditable = true;
                }

                field.SetDefaultText();
            }
        }

        private bool _undoRedoExecuting;
        private bool _findCommandExecuting = false;
        void CommandManager_BeforeExecute(object sender, CommandManagerExecuteEventArgs e)
        {
            if (Attached)
            {
                switch (e.CommandId)
                {
                    case CommandId.FindButton:
                        _findCommandExecuting = true;
                        break;
                    case CommandId.CheckSpelling:
                        _checkSpellingCommandExecuting = true;
                        break;
                    case CommandId.Undo:
                    case CommandId.Redo:
                        _undoRedoExecuting = true;
                        break;
                }
            }
        }

        private void EditorContext_HtmlInserted(object sender, EventArgs e)
        {
            if (Attached)
            {
                PersistAllEditFields();
            }
        }

        void CommandManager_AfterExecute(object sender, CommandManagerExecuteEventArgs e)
        {
            if (Attached)
            {
                switch (e.CommandId)
                {
                    case CommandId.FindButton:
                        _findCommandExecuting = false;
                        break;
                    case CommandId.CheckSpelling:
                        _checkSpellingCommandExecuting = false;
                        break;
                    case CommandId.Undo:
                    case CommandId.Redo:
                        _undoRedoExecuting = false;
                        break;
                }

                if (Selected)
                    PersistSelectedEditField();
            }
        }

        public bool UndoRedoExecuting()
        {
            return _undoRedoExecuting;
        }

        void EditorContext_PostEventNotify(object sender, MshtmlEditor.EditDesignerEventArgs args)
        {
            if (Attached)
            {
                switch (args.EventDispId)
                {
                    case 0:
                        // WinLive 233200: JA-JP: photoalbum is created with subject instead of album name when album name specified in email body contains Japanese only
                        // We don't get key press events with IME input for languages such as Japanese that employ IME composition
                        // (e.g. multiple keystrokes to generate a single character).
                        if (String.Compare("COMPOSITION", args.EventObj.type, StringComparison.OrdinalIgnoreCase) == 0)
                            PersistSelectedEditField();
                        break;
                    case DISPID_HTMLELEMENTEVENTS2.ONKEYPRESS:
                        PersistSelectedEditField();
                        break;
                    default:
                        break;
                }
            }
        }

        private void PersistSelectedEditField()
        {
            if (HTMLElement == null)
                return;

            IHTMLElement editFieldElement = GetSelectedChildEditField(HTMLElement, EditorContext.Selection.SelectedMarkupRange);
            if (editFieldElement != null)
            {
                PersistSelectedEditField(editFieldElement);
            }
        }

        private void PersistSelectedEditField(IHTMLElement editFieldElement)
        {
            SmartContent content = SmartContent;

            InlineEditField field = new InlineEditField(editFieldElement, content, EditorContext, HTMLElement, this);
            field.PersistFieldValueToContent(true);
        }

        private void PersistAllEditFields()
        {
            SmartContent content = SmartContent;
            foreach (IHTMLElement el in EditFields)
            {
                InlineEditField field = new InlineEditField(el, content, EditorContext, HTMLElement, this);
                field.SetDefaultText();
                field.PersistFieldValueToContent(false);
            }
        }

        void EditorContext_SelectionChanged(object sender, EventArgs e)
        {
            if (Attached)
            {
                MarkupRange range = EditorContext.Selection.SelectedMarkupRange;
                IHTMLElement element = GetSelectedChildEditField(HTMLElement, range);
                if (element != null)
                {
                    if (_findCommandExecuting)
                        return;

                    InlineEditField field = new InlineEditField(element, SmartContent, EditorContext, HTMLElement, this);
                    field.ClearDefaultText();
                    field.PersistFieldValueToContent(true);
                }
                else
                {
                    if (_checkSpellingCommandExecuting)
                        return;

                    PersistAllEditFields();
                }
            }
        }

        private bool ShouldNavigateOutOfEditField(IHTMLElement selectedEditField, Keys keyCode)
        {
            Debug.Assert(keyCode == Keys.Up || keyCode == Keys.Down);

            if (selectedEditField == null)
                return false;

            IDisplayServicesRaw displayServices = (IDisplayServicesRaw)HTMLElement.document;
            MarkupRange editFieldRange = EditorContext.MarkupServices.CreateMarkupRange(selectedEditField, false);
            // If we're navigating up, then position a display pointer on the top line.
            // If we're going down, then position a display pointer on the bottom line.
            IDisplayPointerRaw displayPointer;
            displayServices.CreateDisplayPointer(out displayPointer);
            DisplayServices.TraceMoveToMarkupPointer(displayPointer, keyCode == Keys.Up ? editFieldRange.Start : editFieldRange.End);

            // Now determine if the display pointer is in the top/bottom line rect.
            return IsCaretWithin(GetLineRect(selectedEditField, displayPointer));
        }

        private void EditorContext_CommandKey(object sender, KeyEventArgs e)
        {
            if (Selected)
            {
                switch (e.KeyCode)
                {
                    case Keys.Back:
                        {
                            if (GetSelectedChildEditField(HTMLElement, EditorContext.Selection.SelectedMarkupRange) == null)
                            {
                                EditorContext.Clear();
                                e.Handled = true;

                            }
                        }
                        break;
                    case Keys.Up:
                    case Keys.Down:
                        {
                            IHTMLElement selectedChildEditField = GetSelectedChildEditField(HTMLElement, EditorContext.Selection.SelectedMarkupRange);
                            if (ShouldNavigateOutOfEditField(selectedChildEditField, e.KeyCode))
                            {
                                LastChanceKeyboardHook.OnBeforeKeyHandled(this, e);
                                SelectNextRegion(e.KeyCode == Keys.Up);
                                e.Handled = true;
                            }
                        }
                        break;
                    case Keys.Enter:
                        {
                            LastChanceKeyboardHook.OnBeforeKeyHandled(this, e);
                            SelectNextRegion(false);
                            e.Handled = true;
                            break;
                        }
                    case Keys.Escape:
                        // Move selection to just before smart content
                        Deselect();
                        e.Handled = true;
                        break;
                    case Keys.Tab:
                        LastChanceKeyboardHook.OnBeforeKeyHandled(this, e);
                        SelectNextRegion(e.Shift);
                        e.Handled = true;
                        break;
                }

                // If the command associated with the shortcut is disabled, eat the command key
                // This prevents commands like paste from getting through to the standard handler
                // when they are disabled.
                Command command = EditorContext.CommandManager.FindCommandWithShortcut(e.KeyData);
                if (command != null && !command.Enabled)
                {
                    LastChanceKeyboardHook.OnBeforeKeyHandled(this, e);
                    e.Handled = true;
                }
            }
        }

        private void SelectNextRegion(bool backward)
        {
            MarkupRange smartContentRange = EditorContext.MarkupServices.CreateMarkupRange(HTMLElement, false);
            IHTMLElement[] editFields = smartContentRange.GetElements(ElementFilters.CreateClassFilter(InlineEditField.EDIT_FIELD), true);

            IHTMLElement element = GetSelectedChildEditField(HTMLElement, EditorContext.Selection.SelectedMarkupRange);
            if (element == null)
            {
                if (editFields.Length > 0)
                    SelectElement(backward ? editFields[editFields.Length - 1] : editFields[0]);
                else
                    Select();

                return;
            }

            // One of the edit fields was selected
            for (int i = 0; i < editFields.Length; i++)
            {
                IHTMLElement editField = editFields[i];

                if (element.sourceIndex == editField.sourceIndex)
                {
                    if (i == 0 && backward ||
                        i == editFields.Length - 1 && !backward)
                        Select();
                    else
                        SelectElement(backward ? editFields[i - 1] : editFields[i + 1]);

                    return;
                }
            }

            Debug.Fail("How did we get here?");
        }

        protected override void Select()
        {
            PersistEditFieldValues();
            base.Select();
        }

        protected override void Deselect()
        {
            PersistEditFieldValues();
            base.Deselect();
        }

        private void SelectElement(IHTMLElement element)
        {
            PersistEditFieldValues();

            InlineEditField field = new InlineEditField(element, SmartContent, EditorContext, HTMLElement, this);
            field.ClearDefaultText();

            MarkupRange range = EditorContext.MarkupServices.CreateMarkupRange(element, false);
            range.ToTextRange().select();
        }

        private SmartContent SmartContent
        {
            get
            {
                string contentSourceId;
                string contentId;
                ContentSourceManager.ParseContainingElementId(HTMLElement.id, out contentSourceId, out contentId);
                return (SmartContent)_contentSourceContext.FindSmartContent(contentId);
            }
        }

        private string ContentSourceId
        {
            get
            {
                string contentSourceId;
                string contentId;
                ContentSourceManager.ParseContainingElementId(HTMLElement.id, out contentSourceId, out contentId);
                return contentSourceId;
            }
        }

        private void EditorContext_DoubleClick(object sender, HtmlEventArgs e)
        {
            if (Selected)
                EditorContext.CommandManager.Execute(CommandId.ActivateContextualTab);
        }

        protected override void OnResizeStart(Size size, bool preserveAspectRatio)
        {
            _resizeUndo = EditorContext.CreateUndoUnit();
            base.OnResizeStart(size, preserveAspectRatio);

            // save references
            _initialParentSize = size;
            _preserveAspectRatio = preserveAspectRatio;

            // initialize smart content
            string contentSourceId;
            string contentId;
            ContentSourceManager.ParseContainingElementId(HTMLElement.id, out contentSourceId, out contentId);

            // clone the smart content for resizing so that settings changes made during the resize
            //operation are undoable
            String newContentId = Guid.NewGuid().ToString();
            SmartContent content = (SmartContent)_contentSourceContext.CloneSmartContent(contentId, newContentId);

            if (content == null)
            {
                Trace.WriteLine("Could not clone smart content for resize.");
                return;
            }

            HTMLElement.id = ContentSourceManager.MakeContainingElementId(contentSourceId, newContentId);

            // call sizer
            ResizeOptions resizeOptions = new ResizeOptions();
            _contentSource.OnResizeStart(content, resizeOptions);

            // determine the target size
            IHTMLElement targetSizeElement = GetResizeTargetElement(resizeOptions.ResizeableElementId);
            _initialTargetSize = new Size(targetSizeElement.offsetWidth, targetSizeElement.offsetHeight);
            // Account for areas of the smart content that are not being scaled when preserving aspect ratio.
            // For example, in YouTube plugin, label text below the image.
            UpdateResizerAspectRatioOffset(_initialParentSize - _initialTargetSize);

            // determine the aspect ratio
            if (resizeOptions.AspectRatio > 0)
                _aspectRatio = resizeOptions.AspectRatio;
            else
                _aspectRatio = ((double)_initialTargetSize.Width) / _initialTargetSize.Height;
        }

        private Size _initialParentSize;
        private Size _initialTargetSize;
        private bool _preserveAspectRatio;
        private double _aspectRatio;
        private bool _checkSpellingCommandExecuting = false;

        protected override void OnResizing(Size currentSize)
        {
            base.OnResizing(currentSize);

            if (_realtimeResizing)
            {
                currentSize = UpdateHtmlForResize(currentSize, false);
                CompactElementSize(currentSize);
            }
        }

        protected override void OnResizeEnd(Size newSize)
        {
            base.OnResizeEnd(newSize);

            if (newSize != _initialParentSize)
            {
                // Only do this if the size actually changed.
                newSize = UpdateHtmlForResize(newSize, true);
                CompactElementSize(newSize);

                PersistAllEditFields();

                //commit the undo unit
                _resizeUndo.Commit();
            }

            _resizeUndo.Dispose();
            _resizeUndo = null;

            //notify listeners that the resize is complete. This allows the sidebar to synchronize
            //with the updated smart content state.
            if (_resizedListener != null)
                _resizedListener(newSize, true);
        }

        private void CompactElementSize(Size newSize)
        {
            //compact the HTML element's width as much as possible, don't set the height so
            //the height just flows based on a fixed width
            //Note: this fixed-width compaction algorithm is fairly arbitrary and may need to
            //      change if we find out users are trying to create plugins that need more flexibility
            //      in to compact around resized content.
            //HTMLElement.style.width = newSize.Width + "px";
        }

        private Size UpdateHtmlForResize(Size newSize, bool isComplete)
        {
            Size targetElementSize = newSize;
            if (_contentSource.ResizeCapabilities != ResizeCapabilities.None)
            {
                SmartContent content = SmartContent;

                //base the new size on target element preferred by the source
                //(apply the difference in size to the last known size of the target element)
                targetElementSize = new Size(
                    Math.Max(1, _initialTargetSize.Width + (newSize.Width - _initialParentSize.Width)),
                    Math.Max(1, _initialTargetSize.Height + (newSize.Height - _initialParentSize.Height))
                    );

                if (_preserveAspectRatio)
                {
                    targetElementSize = Utility.GetScaledMaxSize(_aspectRatio, targetElementSize.Width, targetElementSize.Height);
                }

                //let the resizer know about the new size, and regenerate the content
                if (isComplete)
                    _contentSource.OnResizeComplete(content, targetElementSize);
                else
                    _contentSource.OnResizing(content, targetElementSize);

                //insert the new content into the DOM (TODO: need undo support for this)
                SmartContentInsertionHelper.InsertEditorHtmlIntoElement(_contentSourceContext, _contentSource, content, HTMLElement);

                Invalidate();

                if (_resizedListener != null)
                    _resizedListener(newSize, isComplete);

                EditorContext.DamageServices.AddDamage(EditorContext.MarkupServices.CreateMarkupRange(HTMLElement, false));
            }
            return targetElementSize;
        }

        private IHTMLElement GetResizeTargetElement(string id)
        {
            if (_contentSource.ResizeCapabilities != ResizeCapabilities.None)
            {
                if (id != null)
                {
                    IHTMLElement[] elements = ElementRange.GetElements(ElementFilters.CreateIdFilter(id), true);
                    if (elements.Length > 0)
                    {
                        return elements[0];
                    }
                }
            }
            return HTMLElement;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    Debug.Assert(EditorContext != null);
                    EditorContext.CommandKey -= new KeyEventHandler(EditorContext_CommandKey);
                    EditorContext.DocumentEvents.DoubleClick -= new HtmlEventHandler(EditorContext_DoubleClick);
                    EditorContext.SelectionChanged -= new EventHandler(EditorContext_SelectionChanged);
                    EditorContext.CommandManager.AfterExecute -= new CommandManagerExecuteEventHandler(CommandManager_AfterExecute);
                    EditorContext.CommandManager.BeforeExecute -= new CommandManagerExecuteEventHandler(CommandManager_BeforeExecute);
                    EditorContext.PostEventNotify -= new MshtmlEditor.EditDesignerEventHandler(EditorContext_PostEventNotify);
                    EditorContext.HtmlInserted -= new EventHandler(EditorContext_HtmlInserted);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
    }

    internal delegate void SmartContentResizedListener(Size newSize, bool completed);
}

