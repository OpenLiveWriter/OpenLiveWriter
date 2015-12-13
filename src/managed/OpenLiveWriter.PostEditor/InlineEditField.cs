// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor
{
    class InlineEditField
    {
        private readonly IHTMLElement _element;
        private readonly ISmartContent _smartContent;
        private readonly IHtmlEditorComponentContext _editorContext;
        private readonly IHTMLElement _smartContentElement;
        private readonly IUndoRedoExecutingChecker _undoRedoCheck;

        public InlineEditField(IHTMLElement element, ISmartContent smartContent, IHtmlEditorComponentContext editorContext, IHTMLElement smartContentElement, IUndoRedoExecutingChecker undoRedoCheck)
        {
            Debug.Assert(element != null, "Making an edit field with a null element.");
            _element = element;
            _smartContent = smartContent;
            _smartContentElement = smartContentElement;
            _editorContext = editorContext;
            _undoRedoCheck = undoRedoCheck;
        }

        public string TextValue
        {
            // The innerText may contain a shortened form of the title
            // This field should only return the full title.
            get
            {
                return _element.innerText;
            }
        }

        public bool ContentEditable
        {
            get
            {
                return ((IHTMLElement3)_element).isContentEditable;
            }
            set
            {
                ((IHTMLElement3)_element).contentEditable = value ? bool.TrueString : bool.FalseString;
            }
        }

        public string PropertyPath
        {
            get
            {
                string propertyPath = _element.getAttribute("wlPropertyPath", 2) as string;
                if (!String.IsNullOrEmpty(propertyPath))
                    _smartContent.Properties.SetString("wlPropertyPath", propertyPath);

                return _smartContent.Properties.GetString("wlPropertyPath", String.Empty);
            }
        }

        public string DefaultText
        {
            get { return _element.getAttribute("defaultText", 2) as string; }
        }

        public bool IsDefaultText
        {
            get { return _element.getAttribute("isDefaultText", 2) as string != null; }
            set
            {
                if (value)
                {
                    _element.setAttribute("isDefaultText", "true", 0);
                }
                else
                {
                    _element.removeAttribute("isDefaultText", 0);
                }
            }
        }

        public string DefaultTextColor
        {
            get { return _element.getAttribute("defaultTextColor", 2) as string; }
        }

        public string TextColor
        {
            get { return _element.getAttribute("textColor", 2) as string; }
        }

        internal void ClearDefaultText()
        {
            Debug.Assert(_undoRedoCheck != null, "Clearing default text on an unmanaged inline edit field");
            if (IsDefaultText && (!_undoRedoCheck.UndoRedoExecuting()))
            {
                using (IUndoUnit undo = _editorContext.CreateInvisibleUndoUnit())
                {
                    _element.innerHTML = "";
                    if (!string.IsNullOrEmpty(TextColor))
                        _element.style.color = TextColor;
                    IsDefaultText = false;
                    undo.Commit();
                }
            }
        }

        /// <summary>
        /// This writes into the currentEditor's SelectedContent properties.
        /// Be sure that only the edit field associated with the *selected* content gets persisted there.
        /// </summary>
        public void PersistFieldValueToContent(bool persistToEditorContent)
        {
            SmartContentEditor currentEditor = ((IBlogPostHtmlEditor)_editorContext).CurrentEditor;

            if (currentEditor == null || currentEditor.SelectedContent == null || IsDefaultText)
                return;

            string propertyPath = PropertyPath;
            IProperties sidebarProperties = currentEditor.SelectedContent.Properties;
            IProperties smartContentProperties = _smartContent.Properties;

            string[] pathElements = (PropertyPath ?? "").Split('\\');
            for (int i = 0; i < pathElements.Length; i++)
            {
                if (string.IsNullOrEmpty(pathElements[i]))
                {
                    Trace.Fail("Failed to sync to malformed property path " + propertyPath);
                    return;
                }

                if (i == pathElements.Length - 1)
                {

                    if (persistToEditorContent)
                    {
                        // Save to smart content in sidebar contextual editor
                        sidebarProperties[pathElements[i]] = TextValue;
                        sidebarProperties.SetString("wlPropertyPath", pathElements[i]);
                    }

                    // Save to smart content in canvas
                    smartContentProperties[pathElements[i]] = TextValue;
                    smartContentProperties.SetString("wlPropertyPath", pathElements[i]);
                    return;
                }
                else
                {
                    sidebarProperties = sidebarProperties.GetSubProperties(pathElements[i]);
                    smartContentProperties = smartContentProperties.GetSubProperties(pathElements[i]);
                }
            }
        }

        public void SetDefaultText()
        {
            Debug.Assert(_undoRedoCheck != null, "Setting default text on an unmanaged inline edit field");
            // WinLive 210281: Don't update the default text unless really necessary. If an undo forces this function
            // to run, then creating a new undo unit will clear the redo stack.
            if (String.IsNullOrEmpty(_element.innerText) && (!_undoRedoCheck.UndoRedoExecuting()))
            {
                using (IUndoUnit undo = _editorContext.CreateInvisibleUndoUnit())
                {
                    _element.innerText = DefaultText;
                    if (!string.IsNullOrEmpty(DefaultTextColor))
                        _element.style.color = DefaultTextColor;
                    IsDefaultText = true;

                    undo.Commit();
                }
            }
        }

        public static readonly string EDIT_FIELD = "wlEditField";
        private static readonly string EditFieldRegExPattern = "\\b" + EDIT_FIELD + "\\b";

        public static bool IsEditField(IHTMLElement el)
        {
            if (el == null)
                return false;

            return Regex.IsMatch(el.className ?? "", EditFieldRegExPattern);
        }

        public static bool IsEditField(object selection)
        {
            IHtmlEditorSelection htmlSelection = selection as IHtmlEditorSelection;
            if (htmlSelection != null)
                return IsEditField(htmlSelection.SelectedMarkupRange.ParentElement());
            else if (selection is IHTMLElement)
            {
                return IsEditField((IHTMLElement)selection);
            }
            return false;
        }

        public static bool IsWithinEditField(IHTMLElement element)
        {
            while (element != null)
            {
                if (IsEditField(element))
                    return true;
                element = element.parentElement;
            }
            return false;
        }

        public static bool IsDefaultTextShowing(IHTMLElement element)
        {
            while (element != null)
            {
                if (IsEditField(element))
                {
                    InlineEditField editField = new InlineEditField(element, null, null, null, null);
                    return editField.IsDefaultText;
                }
                element = element.parentElement;
            }
            return false;
        }

        public static bool EditFieldAcceptsData()
        {

            IDataObject clipboard = Clipboard.GetDataObject();

            string[] formats = clipboard.GetFormats();

            foreach (string format in formats)
            {
                switch (format)
                {
                    case "HTML Format":
                    case "System.String":
                    case "Text":
                    case "UnicodeText":
                        return true;
                }
            }
            return false;
        }

        public static bool EditFieldAcceptsCurrentClipboardData
        {
            get
            {
                return EditFieldAcceptsData();
            }
        }
    }
}
