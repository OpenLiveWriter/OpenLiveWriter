// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Represents a tab entry.
    /// </summary>
    internal class TabEntry
    {
        /// <summary>
        /// The tab lightweight control.
        /// </summary>
        private TabLightweightControl tabLightweightControl;

        /// <summary>
        /// Gets the tab lightweight control.
        /// </summary>
        public TabLightweightControl TabLightweightControl
        {
            get
            {
                return tabLightweightControl;
            }
        }

        /// <summary>
        /// The tab page control.
        /// </summary>
        private TabPageControl tabPageControl;

        /// <summary>
        /// Gets the tab page control.
        /// </summary>
        public TabPageControl TabPageControl
        {
            get
            {
                return tabPageControl;
            }
        }

        /// <summary>
        /// The tab selector lightweight control.
        /// </summary>
        private TabSelectorLightweightControl tabSelectorLightweightControl;

        /// <summary>
        /// Gets the tab control.
        /// </summary>
        public TabSelectorLightweightControl TabSelectorLightweightControl
        {
            get
            {
                return tabSelectorLightweightControl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the tab entry is the first tab entry or not.
        /// </summary>
        public bool IsFirstTabEntry
        {
            get
            {
                return tabLightweightControl.FirstTabEntry == this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the tab entry is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return tabLightweightControl.SelectedTabEntry == this;
            }
        }

        public bool Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
                tabSelectorLightweightControl.Visible = value;
                tabPageControl.Visible = value;
            }
        }
        private bool _hidden;

        public void Selected()
        {
            if (tabPageControl != null)
            {
                if (_hidden)
                    Hidden = false;
                tabPageControl.Visible = true;
                tabPageControl.TabStop = true;
                tabPageControl.RaiseSelected();
                tabPageControl.SelectNextControl(null, true, true, true, false);
            }
        }

        public void Unselected()
        {
            if (tabPageControl != null)
            {
                tabPageControl.Visible = false;
                tabPageControl.TabStop = false;
                tabPageControl.RaiseUnselected();
            }
        }

        /// <summary>
        /// Initializes a new instance of the TabEntry class.
        /// </summary>
        /// <param name="tabControl">The tab page control.</param>
        public TabEntry(TabLightweightControl tabLightweightControl, TabPageControl tabPageControl)
        {
            //	Set the the tab control and the tab lightweight control.
            this.tabPageControl = tabPageControl;
            this.tabPageControl.TabStop = false;
            this.tabLightweightControl = tabLightweightControl;

            //	Instantiate the tab selector lightweight control.
            tabSelectorLightweightControl = new TabSelectorLightweightControl(this);
        }
    }
}
