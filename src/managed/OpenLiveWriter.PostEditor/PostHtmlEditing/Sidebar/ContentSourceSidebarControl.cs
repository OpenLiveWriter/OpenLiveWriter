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
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{

    internal class ContentSourceSidebarControl : SidebarControl, ISmartContentEditorSite, ICommandManagerHost, ISmartContentEditorCache
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private ISidebarContext _sidebarContext;
        private Hashtable _contentSourceControls;
        private IHTMLElement _selectedElement;
        private SmartContentEditor _currentEditor;
        private IContentSourceSidebarContext _contentSourceContext;
        private SmartContentSource _contentSource;
        private EditableSmartContent _editableSmartContent;
        private String _selectedSmartContentId;
        private object _currentSelection;
        string _contentSourceId;
        string _contentItemId;

        public ContentSourceSidebarControl(ISidebarContext sidebarContext, IContentSourceSidebarContext sourceContext)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            _contentSourceControls = new Hashtable();
            _sidebarContext = sidebarContext;
            _contentSourceContext = sourceContext;
            _contentSourceContext.ContentResized += new ContentResizedEventHandler(_contentSourceContext_ContentResized);
        }

        public SmartContentEditor CurrentEditor
        {
            get
            {
                return _currentEditor;
            }
        }

        public override void UpdateView(object htmlSelection, bool force)
        {
            if (htmlSelection == null) //true when the a non-smartcontent element is selected
            {
                //reset the selected smart content (fixes bug 492456)
                _selectedElement = null;
                _selectedSmartContentId = null;
                UnloadCurrentEditor();
                return;
            }

            Debug.Assert(htmlSelection is SmartContentSelection || (htmlSelection is IHtmlEditorSelection && InlineEditField.IsEditField(htmlSelection)));

            IHTMLElement selectedElement = null;
            SmartContentSelection smartContentSelection = htmlSelection as SmartContentSelection;
            if (smartContentSelection != null && smartContentSelection.ContentState == SmartContentState.Enabled)
            {
                selectedElement = smartContentSelection.HTMLElement;
            }
            else if (htmlSelection is IHtmlEditorSelection)
            {
                selectedElement = ContentSourceManager.GetContainingSmartContent(
                    ((IHtmlEditorSelection)(htmlSelection)).SelectedMarkupRange.ParentElement());
            }

            _currentSelection = htmlSelection;
            if (selectedElement != null)
            {
                //if the selected element id is still the same, then the sidebar is currently
                //in synch with the smart content.
                //Note: the element id will change each time an edit is made to the smart content
                if (!force && _selectedElement != null && _selectedSmartContentId != null && selectedElement.id == _selectedSmartContentId)
                    return;
                else
                {
                    _selectedElement = selectedElement;
                    _selectedSmartContentId = selectedElement.id;

                    if (_currentEditor != null)
                    {
                        UnloadCurrentEditor();
                    }
                }

                ContentSourceManager.ParseContainingElementId(_selectedElement.id, out _contentSourceId, out _contentItemId);

                SmartContentEditor editor = (SmartContentEditor)_contentSourceControls[_contentSourceId];
                ContentSourceInfo contentSource = _contentSourceContext.FindContentSource(_contentSourceId);
                if (contentSource != null && contentSource.Instance is SmartContentSource)
                {
                    _contentSource = (SmartContentSource)contentSource.Instance;

                    if (_editableSmartContent != null)
                        _editableSmartContent.Dispose();
                    _editableSmartContent = new EditableSmartContent(_contentSourceContext, _contentSource, _selectedElement);

                    if (editor == null)
                    {
                        editor = _contentSource.CreateEditor(this);

                        if (editor is IActiveSmartContentEditor)
                            ((IActiveSmartContentEditor)editor).ForceContentEdited += new EventHandler(ContentSourceSidebarControl_ForceContentEdited);

                        //apply the current scale the new control
                        if (editor != null)
                        {
                            editor.Scale(new SizeF(scale.Width, scale.Height));
                        }
                        _contentSourceControls[_contentSourceId] = editor;
                    }

                    if (editor != null)
                    {
                        editor.ContentEdited += new EventHandler(_editor_ContentEdited);
                        editor.SelectedContent = _editableSmartContent;

                        EnableableSmartContentEditor enableableSmartContentEditor = editor as EnableableSmartContentEditor;
                        if (enableableSmartContentEditor != null)
                            enableableSmartContentEditor.ContentEnabled = true;

                        if (editor != _currentEditor)
                        {
                            if (enableableSmartContentEditor != null)
                                Controls.Clear();
                            else
                            {
                                //load the new editor
                                editor.Dock = DockStyle.Fill;
                                editor.Width = Width - DockPadding.Left - DockPadding.Right;
                                this.Controls.Add(editor);
                            }

                            // set the title caption
                            // Text = String.Format( CultureInfo.CurrentCulture, Res.Get(StringId.PluginSidebarTitle), contentSource.InsertableContentSourceSidebarText ) ;
                        }

                    }
                }
                _currentEditor = editor;
            }
            else
                _currentEditor = null;
        }

        /// <summary>
        /// Unloads the current editor.
        /// </summary>
        private void UnloadCurrentEditor()
        {
            if (_currentEditor != null)
            {
                //unload the current editor.
                _currentEditor.ContentEdited -= new EventHandler(_editor_ContentEdited);

                EnableableSmartContentEditor contentContext = _currentEditor as EnableableSmartContentEditor;
                if (contentContext != null)
                    contentContext.UnloadEditor();

                _currentEditor = null;
                //TODO: does this clear cause the current editor to be disposed?  We don't want it to be.
                Controls.Clear();
                _sidebarContext.UpdateStatusBar(String.Empty);
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new SizeF(scale.Width * dx, scale.Height * dy);
        }
        private SizeF scale = new SizeF(1f, 1f);

        private void _editor_ContentEdited(object source, EventArgs e)
        {
            using (IUndoUnit undo = _sidebarContext.CreateUndoUnit())
            {
                _editableSmartContent.SaveEditedSmartContent();
                undo.Commit();
            }
        }

        void ContentSourceSidebarControl_ForceContentEdited(object sender, EventArgs e)
        {
            _editor_ContentEdited(this, e);
            // A plugin requested that we update its HTML right away, but it currently isn't
            // selected, so we need select it before we call ContentEdited.
            _contentSourceContext.SelectSmartContent(sender as string);
        }

        public event ContentResizedEventHandler ContentResized;

        void ISmartContentEditorSite.UpdateStatusBar(string statusText) { _sidebarContext.UpdateStatusBar(statusText); }

        void ISmartContentEditorSite.UpdateStatusBar(Image image, string statusText) { _sidebarContext.UpdateStatusBar(image, statusText); }

        /// <summary>
        /// This call allows the smart content sidebar to tell the editor to remove the currently selected
        /// smart content, as if the user had pressed the delete key.  This for example is used by the cancel button
        /// on the video sidebar to allow the user to cancel the publish of the video.
        /// </summary>
        internal void RemoveSelectedContent()
        {
            _contentSourceContext.DeleteSmartContent(_contentItemId);

        }

        public IContentSourceSidebarContext ContentSourceSidebarContext
        {
            get
            {
                return _contentSourceContext;
            }
        }

        private void _contentSourceContext_ContentResized(Size newSize, bool completed)
        {
            //refresh the sidebar (this synchronizes the smart content state with the resized html)
            if (completed)
                UpdateView(_currentSelection, false);
            if (ContentResized != null)
                ContentResized(newSize, completed);
        }

        internal void NotifyContentResized()
        {

        }

        public CommandManager CommandManager
        {
            get
            {
                return _sidebarContext.CommandManager;
            }
        }

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
            //
            // ContentSourceSidebarControl
            //
            this.Name = "ContentSourceSidebarControl";
            this.Size = new System.Drawing.Size(200, 288);

        }
        #endregion

        private SmartContentEditor CreateSmartContentEditor(string contentSourceId)
        {
            Debug.Assert(!_contentSourceControls.Contains(contentSourceId));

            SmartContentEditor smartContentEditor = null;
            ContentSourceInfo contentSource = _contentSourceContext.FindContentSource(contentSourceId);
            if (contentSource != null && contentSource.Instance is SmartContentSource)
            {
                _contentSource = (SmartContentSource)contentSource.Instance;
                smartContentEditor = _contentSource.CreateEditor(this);
                _contentSourceControls[contentSourceId] = smartContentEditor;

                if (smartContentEditor is IActiveSmartContentEditor)
                    ((IActiveSmartContentEditor)smartContentEditor).ForceContentEdited += new EventHandler(ContentSourceSidebarControl_ForceContentEdited);

                //apply the current scale the new control
                if (smartContentEditor != null)
                {
                    smartContentEditor.Scale(new SizeF(scale.Width, scale.Height));
                }
            }
            else
            {
                Trace.Fail("Incorrectly calling GetSmartContentEditor for a source that is not a SmartContentSource.");
            }
            return smartContentEditor;
        }

        public SmartContentEditor GetSmartContentEditor(string contentSourceId)
        {
            SmartContentEditor smartContentEditor = _contentSourceControls[contentSourceId] as SmartContentEditor;

            if (smartContentEditor == null)
                smartContentEditor = CreateSmartContentEditor(contentSourceId);

            return smartContentEditor;
        }
    }

    internal class ContentSourceSidebar : ISidebar, ISmartContentEditorCache
    {
        IContentSourceSidebarContext _context;
        internal ContentSourceSidebar(IContentSourceSidebarContext context)
        {
            _context = context;
        }
        public bool AppliesToSelection(object selection)
        {
            if (selection is SmartContentSelection || (selection is IHtmlEditorSelection && InlineEditField.IsEditField(selection)))
            {
                SmartContentSelection smartContentSelection = selection as SmartContentSelection;
                if (smartContentSelection != null && smartContentSelection.ContentState == SmartContentState.Preserve)
                    return false;
                return true;
            }
            else
                return false;
        }

        private ISmartContentEditorCache _smartContentEditorCache;
        public SidebarControl CreateSidebarControl(ISidebarContext sidebarContext)
        {
            ContentSourceSidebarControl sidebarControl = new ContentSourceSidebarControl(sidebarContext, _context);
            _smartContentEditorCache = sidebarControl;
            return sidebarControl;
        }

        public SmartContentEditor GetSmartContentEditor(string contentSourceId)
        {
            Debug.Assert(_smartContentEditorCache != null, "Must CreateSidebarControl before calling GetSmartContentEditor.");
            return _smartContentEditorCache.GetSmartContentEditor(contentSourceId);
        }
    }

    internal class BrokenContentSourceSidebar : ISidebar
    {
        IContentSourceSidebarContext _context;
        internal BrokenContentSourceSidebar(IContentSourceSidebarContext context)
        {
            _context = context;
        }
        public bool AppliesToSelection(object selection)
        {
            if (selection is SmartContentSelection)
            {
                SmartContentSelection smartContentSelection = (SmartContentSelection)selection;
                return smartContentSelection.ContentState == SmartContentState.Broken;
            }
            else
                return false;
        }

        public SidebarControl CreateSidebarControl(ISidebarContext sidebarContext)
        {
            return new MessageSidebarControl(Res.Get(StringId.PluginSidebarNotEditable));
        }
    }

    internal class DisabledContentSourceSidebar : ISidebar
    {
        IContentSourceSidebarContext _context;
        internal DisabledContentSourceSidebar(IContentSourceSidebarContext context)
        {
            _context = context;
        }
        public bool AppliesToSelection(object selection)
        {
            if (selection is SmartContentSelection)
            {
                SmartContentSelection smartContentSelection = (SmartContentSelection)selection;
                return smartContentSelection.ContentState == SmartContentState.Disabled;
            }
            else
                return false;
        }

        public SidebarControl CreateSidebarControl(ISidebarContext sidebarContext)
        {
            return new MessageSidebarControl(Res.Get(StringId.PluginSidebarDisabled));
        }
    }

    public interface IContentSourceSidebarContext : IPublishingContext
    {
        event ContentResizedEventHandler ContentResized;
        ContentSourceInfo FindContentSource(string contentSourceId);
        ISmartContent FindSmartContent(string contentId);
        ISmartContent CloneSmartContent(string contentId, string newContentId);
        void RemoveSmartContent(string contentId);
        void DeleteSmartContent(string contentId);
        IExtensionData FindExtentsionData(string contentId);
        void SelectSmartContent(string contentId);
        void OnSmartContentEdited(string contentId);
    }
}
