// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Configuration;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Represents a tab in the ribbon.
    /// </summary>
    public class RibbonTab : UserControl
    {
        private const int GROUP_SPACING = 4; // Match native ribbon inter-group spacing
        private const int GROUP_LABEL_HEIGHT = 18;
        private const int CONTENT_PADDING = 2;

        private RibbonCommandManager _commandManager;
        private CommandId _commandId;
        private string _label;
        private string _keytip;
        private RibbonApplicationMode _visibleModes = RibbonApplicationMode.All;
        private RibbonContextualTabGroup _contextualGroup = RibbonContextualTabGroup.None;

        private readonly List<RibbonGroup> _groups = new List<RibbonGroup>();
        private readonly Panel _contentPanel;

        /// <summary>
        /// Gets or sets the command ID for this tab.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandId CommandId
        {
            get => _commandId;
            set => _commandId = value;
        }

        /// <summary>
        /// Gets or sets the display label.
        /// </summary>
        public string Label
        {
            get => _label;
            set => _label = value;
        }

        /// <summary>
        /// Gets or sets the keytip for keyboard navigation.
        /// </summary>
        public string Keytip
        {
            get => _keytip;
            set => _keytip = value;
        }

        /// <summary>
        /// Gets or sets the application modes where this tab is visible.
        /// </summary>
        public RibbonApplicationMode VisibleModes
        {
            get => _visibleModes;
            set => _visibleModes = value;
        }

        /// <summary>
        /// Gets or sets the contextual tab group this tab belongs to.
        /// </summary>
        public RibbonContextualTabGroup ContextualGroup
        {
            get => _contextualGroup;
            set => _contextualGroup = value;
        }

        /// <summary>
        /// Gets or sets the command manager.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCommandManager CommandManager
        {
            get => _commandManager;
            set
            {
                _commandManager = value;
                foreach (var group in _groups)
                {
                    group.CommandManager = value;
                }
            }
        }

        /// <summary>
        /// Updates visibility of groups based on the current application mode.
        /// </summary>
        /// <param name="currentMode">The current application mode.</param>
        public void UpdateGroupVisibility(RibbonApplicationMode currentMode)
        {
            bool anyChanged = false;
            foreach (var group in _groups)
            {
                var shouldBeVisible = (group.VisibleModes & currentMode) != 0;
                if (group.Visible != shouldBeVisible)
                {
                    group.Visible = shouldBeVisible;
                    anyChanged = true;
                }
            }

            // Re-layout if any group visibility changed
            if (anyChanged)
            {
                UpdateScaling();
            }
        }

        /// <summary>
        /// Gets or sets the scaling policy for this tab.
        /// When set, groups scale down in the specific order defined by the policy
        /// rather than using the generic algorithm.
        /// </summary>
        public ScalingPolicy ScalingPolicy { get; set; }

        /// <summary>
        /// Gets the header bounds (for hit testing in RibbonPanel).
        /// </summary>
        internal Rectangle HeaderBounds { get; set; }

        /// <summary>
        /// Gets the groups in this tab.
        /// </summary>
        public IReadOnlyList<RibbonGroup> Groups => _groups.AsReadOnly();

        public RibbonTab()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = RibbonColors.Current.TabBackgroundSelected;

            // Use a TransparentPanel for proper transparent background support
            _contentPanel = new TransparentPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                BackColor = Color.Transparent
            };

            Controls.Add(_contentPanel);
        }

        /// <summary>
        /// Adds a group to this tab.
        /// </summary>
        public void AddGroup(RibbonGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));

            group.CommandManager = _commandManager;
            _groups.Add(group);
            _contentPanel.Controls.Add(group);
        }

        /// <summary>
        /// Removes a group from this tab.
        /// </summary>
        public void RemoveGroup(RibbonGroup group)
        {
            if (group == null) return;

            if (_groups.Remove(group))
            {
                _contentPanel.Controls.Remove(group);
            }
        }

        /// <summary>
        /// Clears all groups from this tab.
        /// </summary>
        public void ClearGroups()
        {
            foreach (var group in _groups)
            {
                _contentPanel.Controls.Remove(group);
                group.Dispose();
            }
            _groups.Clear();
        }

        /// <summary>
        /// Updates group sizes based on scaling policy and available width.
        /// </summary>
        public void UpdateScaling()
        {
            var availableWidth = Width - CONTENT_PADDING * 2;

            // Calculate total width needed at current sizes
            int CalculateTotalWidth()
            {
                var total = 0;
                foreach (var group in _groups)
                {
                    if (!group.Visible) continue;
                    total += group.GetPreferredWidth() + GROUP_SPACING;
                }
                return total;
            }

// Use policy-based scaling if a ScalingPolicy with steps is defined
            if (ScalingPolicy != null && ScalingPolicy.ScaleSteps.Count > 0)
            {
                UpdateScalingWithPolicy(availableWidth, CalculateTotalWidth);
            }
            else
            {
                UpdateScalingGeneric(availableWidth, CalculateTotalWidth);
            }

            // Layout groups manually with correct height
            LayoutGroups();
        }

        /// <summary>
        /// Policy-based scaling: applies scale steps in the exact order defined by the
        /// native ribbon's ScalingPolicy XML until all groups fit the available width.
        /// </summary>
        private void UpdateScalingWithPolicy(int availableWidth, Func<int> calculateTotalWidth)
        {
            // Build a lookup from CommandId to group(s)
            var groupsByCommandId = new Dictionary<CommandId, List<RibbonGroup>>();
            foreach (var group in _groups)
            {
                if (!groupsByCommandId.ContainsKey(group.CommandId))
                    groupsByCommandId[group.CommandId] = new List<RibbonGroup>();
                groupsByCommandId[group.CommandId].Add(group);
            }

            // Step 1: Reset all groups to their ideal sizes
            foreach (var group in _groups)
            {
                if (!group.Visible) continue;

                if (ScalingPolicy.IdealSizes.TryGetValue(group.CommandId, out var idealSize))
                    group.CurrentSize = idealSize;
                else
                    group.CurrentSize = RibbonGroupSize.Large;
            }

            // Step 2: Check if groups fit at ideal sizes
            var totalWidth = calculateTotalWidth();
            if (totalWidth <= availableWidth)
                return;

            // Step 3: Apply scale steps one at a time until groups fit
            foreach (var step in ScalingPolicy.ScaleSteps)
            {
                if (groupsByCommandId.TryGetValue(step.GroupId, out var groups))
                {
                    foreach (var group in groups)
                    {
                        if (!group.Visible) continue;
                        group.CurrentSize = step.Size;
                    }
                }

                totalWidth = calculateTotalWidth();
                if (totalWidth <= availableWidth)
                    return;
            }
        }

        /// <summary>
        /// Generic scaling: scales down groups by size (largest first), then to Popup.
        /// Used as fallback when no ScalingPolicy is defined.
        /// </summary>
        private void UpdateScalingGeneric(int availableWidth, Func<int> calculateTotalWidth)
        {
            var totalWidth = calculateTotalWidth();

            if (totalWidth <= availableWidth)
                return;

            // Phase 1: Scale down groups from Large -> Medium -> Small
            bool anyGroupScaled;
            do
            {
                anyGroupScaled = false;
                
                var groupsToScale = new List<RibbonGroup>();
                foreach (var group in _groups)
                {
                    if (!group.Visible) continue;
                    if (group.CurrentSize > RibbonGroupSize.Small)
                        groupsToScale.Add(group);
                }
                
                groupsToScale.Sort((a, b) => b.CurrentSize.CompareTo(a.CurrentSize));
                
                foreach (var group in groupsToScale)
                {
                    if (group.CurrentSize > RibbonGroupSize.Small)
                    {
                        group.CurrentSize = (RibbonGroupSize)((int)group.CurrentSize + 1);
                        anyGroupScaled = true;
                        
                        totalWidth = calculateTotalWidth();
                        if (totalWidth <= availableWidth)
                            return;
                    }
                }
            } while (anyGroupScaled && totalWidth > availableWidth);

            // Phase 2: Scale to Popup, widest first
            while (totalWidth > availableWidth)
            {
                var groupsAtSmall = new List<RibbonGroup>();
                foreach (var group in _groups)
                {
                    if (!group.Visible) continue;
                    if (group.CurrentSize == RibbonGroupSize.Small)
                        groupsAtSmall.Add(group);
                }

                if (groupsAtSmall.Count == 0)
                    break;

                groupsAtSmall.Sort((a, b) => b.GetPreferredWidth().CompareTo(a.GetPreferredWidth()));
                groupsAtSmall[0].CurrentSize = RibbonGroupSize.Popup;
                
                totalWidth = calculateTotalWidth();
                if (totalWidth <= availableWidth)
                    break;
            }
        }

        private void LayoutGroups()
        {
            var availableWidth = Width - CONTENT_PADDING * 2;
            var groupHeight = _contentPanel.Height - 2; // Full height minus border

            // First pass: calculate preferred widths and find expandable groups
            var groupWidths = new Dictionary<RibbonGroup, int>();
            var totalPreferred = 0;
            var expandableGroups = new List<RibbonGroup>();

            foreach (var group in _groups)
            {
                if (!group.Visible) continue;

                var preferred = group.GetPreferredWidth();
                groupWidths[group] = preferred;
                totalPreferred += preferred + GROUP_SPACING;

                if (group.CanExpand)
                    expandableGroups.Add(group);
            }

            // Second pass: distribute surplus to expandable (gallery) groups only.
            // Groups stay at preferred width (left-aligned), gallery expands for more columns.
            // Cap gallery expansion so it doesn't dominate the ribbon at very wide widths.
            var remainingSpace = availableWidth - totalPreferred;
            if (remainingSpace > 0 && expandableGroups.Count > 0)
            {
                foreach (var group in expandableGroups)
                {
                    var preferred = groupWidths[group];
                    var maxWidth = group.GetMaxPreferredWidth();
                    // Only expand up to the gallery's max (MaxColumns), and never take
                    // more than half the total surplus to keep the layout balanced.
                    var maxSurplus = remainingSpace / 2;
                    var extraWidth = Math.Min(maxSurplus, maxWidth - preferred);
                    if (extraWidth > 0)
                    {
                        groupWidths[group] = preferred + extraWidth;
                    }
                }
            }

            // Final pass: position groups with calculated widths
            var x = CONTENT_PADDING;
            foreach (var group in _groups)
            {
                if (!group.Visible) continue;

                var groupWidth = groupWidths[group];
                group.Location = new Point(x, 0);
                group.Size = new Size(groupWidth, groupHeight);

                x += groupWidth + GROUP_SPACING;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScaling();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && IsHandleCreated)
            {
                BeginInvoke(new Action(() => UpdateScaling()));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw bottom border
            using (var pen = new Pen(RibbonColors.Current.RibbonBorder))
            {
                e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            }
        }
    }
}
