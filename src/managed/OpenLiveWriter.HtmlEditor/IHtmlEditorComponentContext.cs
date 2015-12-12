// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.HtmlEditor
{
    public delegate IHtmlEditorComponentContext IHtmlEditorComponentContextDelegate();

    public interface IHtmlEditorComponentContext : IUndoUnitFactory
    {
        /// <summary>
        /// interface to the main frame window that hosts this component
        /// </summary>
        IMainFrameWindow MainFrameWindow
        {
            get;
        }

        IMshtmlDocumentEvents DocumentEvents { get; }

        /// <summary>
        /// Markup services for low level document examination/manipulation
        /// </summary>
        MshtmlMarkupServices MarkupServices { get; }

        /// <summary>
        /// Damage services for low level document damage notification
        /// </summary>
        IHTMLEditorDamageServices DamageServices { get; }

        event EventHandler SelectionChanged;

        bool IsEditFieldSelected { get; }
        IHTMLElement SelectedEditField { get; }

        /// <summary>
        /// Allow components to perform fixups to edited html
        /// prior to publishing or saving. These edits are
        /// automatically reverted after publishing or saving
        /// so that the editing experience is not affected
        /// </summary>
        event TemporaryFixupHandler PerformTemporaryFixupsToEditedHtml;

        void EmptySelection();
        void BeginSelectionChange();
        IHtmlEditorSelection Selection { get; set; }
        void EndSelectionChange();

        event EventHandler BeforeInitialInsertion;
        event EventHandler AfterInitialInsertion;

        void FireSelectionChanged();

        DragDropEffects DoDragDrop(IDataObject dataObject, DragDropEffects effects);

        Point PointToScreen(Point clientPoint);

        Point PointToClient(Point screenPoint);

        IHTMLElement ElementFromClientPoint(Point clientPoint);

        bool PointIsOverDocumentArea(Point clientPoint);

        /// <summary>
        /// Enable overriding of the default cursor
        /// </summary>
        bool OverrideCursor { set; }

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command-d</param>
        void ExecuteCommand(uint cmdID);

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command id</param>
        /// <param name="input">input parameter</param>
        void ExecuteCommand(uint cmdID, object input);

        /// <summary>
        /// Execute an MSHTML command
        /// </summary>
        /// <param name="cmdID">command id</param>
        /// <param name="input">input parameter</param>
        /// <param name="output">out parameter</param>
        void ExecuteCommand(uint cmdID, object input, ref object output);

        /// <summary>
        /// Clear the current selection
        /// </summary>
        void Clear();

        /// <summary>
        /// Provide the ability to filter editor events
        /// </summary>
        event HtmlEditDesignerEventHandler PreHandleEvent;

        event MshtmlEditor.EditDesignerEventHandler PostEventNotify;

        /// <summary>
        /// Provide the ability to override accelerator processing
        /// </summary>
        event HtmlEditDesignerEventHandler TranslateAccelerator;

        event EventHandler HtmlInserted;

        /// <summary>
        /// Provide the ability to override command key processing.
        /// </summary>
        event KeyEventHandler CommandKey;

        /// <summary>
        /// Provide the ability to process key down processing.
        /// </summary>
        event HtmlEventHandler KeyDown;

        /// <summary>
        /// Provide the ability to process key down processing.
        /// </summary>
        event HtmlEventHandler KeyUp;

        /// <summary>
        /// Provide the ability to process clipboard Copy
        /// </summary>
        event HtmlEditorSelectionOperationEventHandler HandleCopy;

        /// <summary>
        /// Provide the ability to process clipboard Cut
        /// </summary>
        ///
        event HtmlEditorSelectionOperationEventHandler HandleCut;

        /// <summary>
        /// Provide the ability to process clipboard Clear
        /// </summary>
        event HtmlEditorSelectionOperationEventHandler HandleClear;

        /// <summary>
        /// A unique identifier for this editor instance.
        /// </summary>
        string EditorId { get; }

        /// <summary>
        /// Returns true if the editor is in edit mode.
        /// </summary>
        bool EditMode { get; }

        /// <summary>
        /// Insert html into the editor
        /// </summary>
        /// <param name="html"></param>
        void InsertHtml(string html, bool moveSelectionRight);

        /// <summary>
        /// Insert content into the editor
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="html"></param>
        void InsertHtml(MarkupPointer start, MarkupPointer end, string html);

        /// <summary>
        /// Create an undo unit for the control
        /// </summary>
        /// <returns></returns>
        IUndoUnit CreateInvisibleUndoUnit();

        /// <summary>
        /// Cookie bag which can be used for components to communicate
        /// with eachother on a per editor-instance basis
        /// </summary>
        IDictionary Cookies { get; }

        /// <summary>
        /// Displays the editor context meny at the specific location.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        bool ShowContextMenu(Point screenPoint);

        bool IsRTLTemplate { get; }

        void ForceDirty();

        CommandManager CommandManager { get; }
    }

    public delegate void TemporaryFixupHandler(TemporaryFixupArgs args);

    public class TemporaryFixupArgs
    {
        public TemporaryFixupArgs(string html)
        {
            _html = html;
        }

        private string _html;
        public string Html
        {
            get
            {
                return _html;
            }
            set
            {
                _html = value;
            }
        }
    }

    public interface IUndoUnitFactory
    {
        /// <summary>
        /// Create an undo unit for the control
        /// </summary>
        /// <returns></returns>
        IUndoUnit CreateUndoUnit();
    }

    public interface IUndoUnit : IDisposable
    {
        void Commit();
    }

    public class HtmlEditorSelectionOperationEventArgs : EventArgs
    {
        public HtmlEditorSelectionOperationEventArgs(IHtmlEditorSelection selection)
        {
            _selection = selection;
        }

        public IHtmlEditorSelection Selection
        {
            get { return _selection; }
        }
        private IHtmlEditorSelection _selection;

        public bool Handled
        {
            get { return _handled; }
            set { _handled = value; }
        }
        private bool _handled = false;
    }

    public delegate void HtmlEditorSelectionOperationEventHandler(HtmlEditorSelectionOperationEventArgs ea);

}

