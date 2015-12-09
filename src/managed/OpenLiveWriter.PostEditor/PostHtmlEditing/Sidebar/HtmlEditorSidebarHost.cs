// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    public class HtmlEditorSidebarHost : Panel, ISidebarContext, ISmartContentEditorCache
    {
        private class SidebarEntry
        {
            public SidebarEntry(ISidebar sidebar)
            {
                Sidebar = sidebar;
            }

            public ISidebar Sidebar;
            public SidebarControl SidebarControl;
        }

        private Container components = new Container();

        private IHtmlEditorComponentContext _editorContext;

        // map sidebar types to sidebar instances
        private ArrayList _sidebars = new ArrayList();

        private Panel _mainPanel;
        // private HtmlEditorSidebarTitle _title ;
        // private HtmlEditorSidebarStatus _status ;

        private SidebarControl _defaultSidebarControl;
        private ControlUITheme _uiTheme;

        public HtmlEditorSidebarHost(IHtmlEditorComponentContext editorContext)
        {
            // save reference to context
            _editorContext = editorContext;
            _editorContext.SelectionChanged += new EventHandler(_editorContext_SelectionChanged);
            _editorContext.DocumentEvents.Click += new OpenLiveWriter.Mshtml.HtmlEventHandler(DocumentEvents_Click);

            base.BackColor = UIPaint.Instance.FrameGradientLight;

            // initialize
            InitializeDockPadding();
            InitializeControls();
            //AdjustLayoutForLargeFonts() ;

            // default visibility to previous value
            Visible = false;

            //create the UI theme
            _uiTheme = new SidebarUITheme(this);
            AccessibleName = Res.Get(StringId.SidebarPanel);

            // Create an automation ID
            Name = "SidebarPanel";
        }

        public CommandManager CommandManager
        {
            get
            {
                return _editorContext.CommandManager;
            }
        }

        /// <summary>
        /// Sidebar that will show when no other sidebar needs to show for the
        /// current editor selection
        /// </summary>
        /// <param name="defaultSidebar"></param>
        public void RegisterDefaultSidebar(ISidebar defaultSidebar)
        {
            _defaultSidebarControl = CreateAndInitializeSidebar(defaultSidebar);
        }

        public void RegisterSidebar(ISidebar sidebar)
        {
            // add type without active instance (demand create for startup perf)
            _sidebars.Add(new SidebarEntry(sidebar));
        }

        public SidebarControl GetSidebarControlOfType(Type type)
        {
            foreach (SidebarEntry sidebarEntry in _sidebars)
            {
                SidebarControl sidebarControl = sidebarEntry.SidebarControl;
                if (sidebarControl != null && type.IsInstanceOfType(sidebarControl))
                    return sidebarControl;
            }

            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int x = ClientRectangle.Left; // Bidi
            using (Pen p = new Pen(ColorizedResources.Instance.BorderDarkColor))
                e.Graphics.DrawLine(p, x, 0, x, ClientRectangle.Bottom);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            UpdateSidebarState();
        }

        private void _editorContext_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSidebarState();
        }

        private void DocumentEvents_Click(object o, OpenLiveWriter.Mshtml.HtmlEventArgs e)
        {
            //fix bug 306300 - if the editor is clicked in the body outside of the editor's main
            //editable regions, we may not receive a selection change event (even though the selection
            //will be changed to none), so we also update the sidebar state on click.
            if (!e.WasCancelled)
                UpdateSidebarState();
        }

        internal void UpdateSidebarState()
        {
            UpdateSidebarState(false);
        }

        private void UpdateVisibility(bool sidebarFocused)
        {
            if (_activeSidebarControl != null && _activeSidebarControl.Controls.Count > 0)
            {
                _activeSidebarControl.Visible = true;
                if (sidebarFocused) //restore the focus (fixes bug 433623)
                    _activeSidebarControl.Focus();
                Visible = true;
            }
            else
            {
                Visible = false;
            }
        }

        private void UpdateSidebarState(bool force)
        {
            // @SharedCanvas - revisit this logic, it was only done to help with the transition time where the ribbon was no finished.
            //if (Visible)
            {
                // update active control
                UpdateActiveSidebarControl();

                // update the view
                if (_activeSidebarControl != null)
                {
                    _activeSidebarControl.UpdateView(_editorContext.Selection, force);
                    //_title.UpdateTitle(_activeSidebarControl.Text) ;
                }

                UpdateVisibility(false);
            }
        }

        internal void ForceUpdateSidebarState()
        {
            UpdateSidebarState(true);
        }

        private void UpdateActiveSidebarControl()
        {
            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.EnableSidebar))
                ActiveSidebarControl = GetSidebarForCurrentSelection(_editorContext.Selection);
        }

        internal SidebarControl ActiveSidebarControl
        {
            get
            {
                return _activeSidebarControl;
            }
            set
            {
                if (_activeSidebarControl != value)
                {
                    SuspendLayout();

                    // reset the status and title bars
                    //_title.UpdateTitle(String.Empty) ;
                    //_status.UpdateStatus(null, String.Empty);

                    bool sidebarFocused = false; //used to restore focus when switching sidebars

                    // first hide the currently active sidebar
                    if (_activeSidebarControl != null)
                    {
                        sidebarFocused = _activeSidebarControl.ContainsFocus;
                        _activeSidebarControl.Visible = false;
                        _activeSidebarControl.MinimumSizeChanged -= new EventHandler(_activeSidebarControl_MinimumSizeChanged);
                        _activeSidebarControl.UpdateView(null, false);
                    }

                    // set the value
                    _activeSidebarControl = value;

                    // if we have a sidebar then manage its appearance
                    if (_activeSidebarControl != null)
                    {
                        //_title.UpdateTitle(_activeSidebarControl.Text) ;
                        //_status.Visible = _activeSidebarControl.HasStatusBar ;
                        _mainPanel.AutoScrollMinSize = _activeSidebarControl.MinimumSize;
                        _activeSidebarControl.MinimumSizeChanged += new EventHandler(_activeSidebarControl_MinimumSizeChanged);

                        UpdateVisibility(sidebarFocused);
                    }
                    else
                    {
                        Visible = false;
                        _mainPanel.AutoScrollMinSize = Size.Empty;
                    }
                    ResumeLayout();
                }
            }
        }

        void _activeSidebarControl_MinimumSizeChanged(object sender, EventArgs e)
        {
            _mainPanel.AutoScrollMinSize = _activeSidebarControl.MinimumSize;
            _mainPanel.AutoScrollPosition = new Point(0, 0);
        }
        private SidebarControl _activeSidebarControl;

        private SidebarControl GetSidebarForCurrentSelection(object selection)
        {
            IHtmlEditorSelection htmlSelection = selection as IHtmlEditorSelection;
            if (selection == null || (!htmlSelection.IsValid && !InlineEditField.IsEditField(selection)))
                return _defaultSidebarControl;

            foreach (SidebarEntry sidebarEntry in _sidebars)
            {
                ISidebar sidebar = sidebarEntry.Sidebar;
                if (sidebar.AppliesToSelection(selection))
                {
                    SidebarControl sidebarControl = sidebarEntry.SidebarControl;
                    if (sidebarControl == null)
                    {
                        // demand-create sidebar
                        sidebarEntry.SidebarControl = CreateAndInitializeSidebar(sidebar);
                        sidebarEntry.SidebarControl.Visible = sidebarEntry.SidebarControl.Controls.Count > 0;
                    }
                    return sidebarEntry.SidebarControl;
                }
            }

            // got this far so no active sidebar for current selection
            return _defaultSidebarControl;
        }

        private SidebarControl CreateAndInitializeSidebar(ISidebar sidebar)
        {
            SidebarControl sidebarControl = sidebar.CreateSidebarControl(this);
            sidebarControl.Dock = DockStyle.Fill;
            sidebarControl.Visible = false;

            //synchronize the scale of the sidebar with this control's scale
            sidebarControl.Scale(new SizeF(scale.Width, scale.Height));

            _mainPanel.Controls.Add(sidebarControl);
            return sidebarControl;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_editorContext != null)
                {
                    _editorContext.SelectionChanged -= new EventHandler(_editorContext_SelectionChanged);
                    _editorContext.DocumentEvents.Click -= new OpenLiveWriter.Mshtml.HtmlEventHandler(DocumentEvents_Click);
                }
                if (_activeSidebarControl != null)
                {
                    _activeSidebarControl.MinimumSizeChanged -= new EventHandler(_activeSidebarControl_MinimumSizeChanged);
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeControls()
        {
            SuspendLayout();

            // NOTE: 200 is the size of the Word sidebar
            Size = new Size(200, 400);

            //_title = new HtmlEditorSidebarTitle();
            //_title.Dock = DockStyle.Top ;
            //_title.TabIndex = 0 ;
            //_title.TabStop = false ;
            //_title.HideTitleBarClicked +=new EventHandler(_title_HideTitleBarClicked);

            _mainPanel = new Panel();
            _mainPanel.Dock = DockStyle.Fill;
            _mainPanel.TabIndex = 1;

            //_status = new HtmlEditorSidebarStatus();
            //_status.Dock = DockStyle.Bottom ;
            //_status.TabIndex = 2 ;
            //_status.TabStop = false ;

            Controls.Add(_mainPanel);
            //Controls.Add(_title);
            //Controls.Add(_status);

            ResumeLayout(false);
        }

        private void _title_HideTitleBarClicked(object sender, EventArgs e)
        {
            Visible = false;
        }

        private const int TOP_INSET = 0;
        private const int LEFT_INSET = 1;
        private const int RIGHT_INSET = 0;
        private const int BOTTOM_INSET = 0;

        private void InitializeDockPadding()
        {
            DockPadding.Top = TOP_INSET;
            DockPadding.Left = LEFT_INSET;
            DockPadding.Right = RIGHT_INSET;
            DockPadding.Bottom = BOTTOM_INSET;
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
        private SizeF scale = new SizeF(1f, 1f); //the currently applied scale

        private class SidebarUITheme : ControlUITheme
        {
            public SidebarUITheme(HtmlEditorSidebarHost sidebarControl) : base(sidebarControl, true)
            {
            }

            protected override void ApplyTheme(bool highContrast)
            {
                if (highContrast)
                    Control.BackColor = SystemColors.Control;
                else
                    Control.BackColor = UIPaint.Instance.FrameGradientLight;
            }
        }

        #region ISidebarContext Members

        public void UpdateStatusBar(string statusText)
        {
            //_status.UpdateStatus(null, statusText);

        }

        public void UpdateStatusBar(Image image, string statusText)
        {
            //_status.UpdateStatus(image, statusText);
        }

        public IWin32Window Owner
        {
            get { return this; }
        }

        public IUndoUnit CreateUndoUnit()
        {
            return _editorContext.CreateUndoUnit();
        }

        #endregion

        private ISmartContentEditorCache _smartContentEditorCache;
        public SmartContentEditor GetSmartContentEditor(string contentSourceId)
        {
            if (_smartContentEditorCache == null)
            {
                // We need to create the smart content editor cache, if we have not already done so.
                ContentSourceSidebarControl contentSourceSidebarControl = GetSidebarControlOfType(typeof(ContentSourceSidebarControl)) as ContentSourceSidebarControl;
                if (contentSourceSidebarControl == null)
                {
                    foreach (SidebarEntry entry in _sidebars)
                    {
                        if (entry.Sidebar is ContentSourceSidebar)
                        {
                            contentSourceSidebarControl = CreateAndInitializeSidebar(entry.Sidebar) as ContentSourceSidebarControl;
                            Debug.Assert(contentSourceSidebarControl != null);

                            entry.SidebarControl = contentSourceSidebarControl;
                            entry.SidebarControl.Visible = entry.SidebarControl.Controls.Count > 0;
                            break;
                        }
                    }
                }
                _smartContentEditorCache = contentSourceSidebarControl;
            }
            Debug.Assert(_smartContentEditorCache != null);
            return _smartContentEditorCache.GetSmartContentEditor(contentSourceId);
        }
    }
}
