// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// The TabPageControl control provides a convenient means of getting a control and an optional
    /// TabPageCommandBarLightweightControl to appear as a page on a TabLightweightControl.
    /// </summary>
    public class TabPageControl : UserControl
    {
        #region Private Member Variables

        /// <summary>
        /// Required designer cruft.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// The tab bitmap.
        /// </summary>
        private Bitmap tabBitmap;

        /// <summary>
        /// A value which indicates whether the tab is drag-and-drop selectable.
        /// </summary>
        private bool dragDropSelectable = true;

        /// <summary>
        /// The tab text.
        /// </summary>
        private string tabText;

        /// <summary>
        /// The tab ToolTip text.
        /// </summary>
        private string tabToolTipText;

        /// <summary>
        /// Keeps track of whether the tab is selected or not
        /// </summary>
        private bool _tabSelected = false;
        #endregion Private Member Variables

        #region Public Events

        /// <summary>
        /// Occurs when the TabPageControl is selected.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the TabPageControl is selected.")
        ]
        public event EventHandler Selected;

        /// <summary>
        /// Occurs when the TabPageControl is selected.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the TabPageControl is unselected.")
        ]
        public event EventHandler Unselected;

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the TabPage class.
        /// </summary>
        public TabPageControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            Visible = false;
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

        #endregion Class Initialization & Termination

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            //
            // TabPageControl
            //
            this.Name = "TabPageControl";
            this.Size = new System.Drawing.Size(268, 298);

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the tab bitmap.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the tab bitmap.")
        ]
        public Bitmap TabBitmap
        {
            get
            {
                return tabBitmap;
            }
            set
            {
                tabBitmap = value;
            }
        }

        /// <summary>
        /// Gets or sets the tab text.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the tab text.")
        ]
        public string TabText
        {
            get
            {
                return tabText;
            }
            set
            {
                tabText = value;
                AccessibleName = value;
            }
        }

        /// <summary>
        /// Gets or sets the tab ToolTip text.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the tab ToolTip text.")
        ]
        public string TabToolTipText
        {
            get
            {
                return tabToolTipText;
            }
            set
            {
                tabToolTipText = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which indicates whether the tab is drag-and-drop selectable.
        /// </summary>
        [
            Category("Behavior"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the tab is drag-and-drop selectable.")
        ]
        public bool DragDropSelectable
        {
            get
            {
                return dragDropSelectable;
            }
            set
            {
                dragDropSelectable = value;
            }
        }

        public virtual ApplicationStyle ApplicationStyle
        {
            get
            {
                return ApplicationManager.ApplicationStyle;
            }
        }

        public bool IsSelected
        {
            get
            {
                return _tabSelected;
            }
        }
        #endregion Public Properties

        #region Protected Events

        /// <summary>
        /// Raises the Selected event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelected(EventArgs e)
        {
            _tabSelected = true;
            if (Selected != null)
                Selected(this, e);
        }

        /// <summary>
        /// Raises the Unselected event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnUnselected(EventArgs e)
        {
            _tabSelected = false;
            if (Unselected != null)
                Unselected(this, e);
        }

        #endregion Protected Events

        #region Internal Methods

        /// <summary>
        /// Helper method to raise the Selected event.
        /// </summary>
        internal void RaiseSelected()
        {
            OnSelected(EventArgs.Empty);
        }

        /// <summary>
        /// Helper method to raise the Unselected event.
        /// </summary>
        internal void RaiseUnselected()
        {
            OnUnselected(EventArgs.Empty);
        }

        #endregion Internal Methods

        #region Accessibility
        private TabPageAccessibility _accessibleObject;
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            if (_accessibleObject == null)
                _accessibleObject = new TabPageAccessibility(this);
            return _accessibleObject;
        }

        class TabPageAccessibility : ControlAccessibleObject
        {
            TabPageControl _tabpage;
            public TabPageAccessibility(TabPageControl ownerControl) : base(ownerControl)
            {
                _tabpage = ownerControl;
            }

            public override Rectangle Bounds
            {
                get
                {
                    if (_tabpage.Visible)
                        return base.Bounds;
                    else
                    {
                        //return empty rect when not visible to prevent bugs with MSAA info
                        //returning info on the wrong controls when mousing over tab controls
                        return Rectangle.Empty;
                    }
                }
            }

        }
        #endregion
    }
}
