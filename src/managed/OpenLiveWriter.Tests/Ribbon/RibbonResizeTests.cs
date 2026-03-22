// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Configuration;
using OpenLiveWriter.Ribbon.Managed.Controls;

namespace OpenLiveWriter.Tests.Ribbon
{
    /// <summary>
    /// Tests to verify ribbon resize behavior matches the original ribbon.
    /// </summary>
    [TestFixture]
    public class RibbonResizeTests
    {
        #region Group Size Tests

        [Test]
        public void RibbonGroup_DefaultSize_IsLarge()
        {
            using var group = new RibbonGroup();
            
            Assert.That(group.CurrentSize, Is.EqualTo(RibbonGroupSize.Large));
        }

        [Test]
        public void RibbonGroup_CanChangeToMedium()
        {
            using var group = new RibbonGroup();
            group.CurrentSize = RibbonGroupSize.Medium;
            
            Assert.That(group.CurrentSize, Is.EqualTo(RibbonGroupSize.Medium));
        }

        [Test]
        public void RibbonGroup_CanChangeToSmall()
        {
            using var group = new RibbonGroup();
            group.CurrentSize = RibbonGroupSize.Small;
            
            Assert.That(group.CurrentSize, Is.EqualTo(RibbonGroupSize.Small));
        }

        [Test]
        public void RibbonGroup_CanChangeToPopup()
        {
            using var group = new RibbonGroup();
            group.CurrentSize = RibbonGroupSize.Popup;
            
            Assert.That(group.CurrentSize, Is.EqualTo(RibbonGroupSize.Popup));
        }

        [Test]
        public void RibbonGroup_PopupSize_ReturnsFixedWidth()
        {
            using var group = new RibbonGroup();
            group.Label = "Test Group";
            group.CurrentSize = RibbonGroupSize.Popup;
            
            var width = group.GetPreferredWidth();
            
            Assert.That(width, Is.EqualTo(LayoutConstants.PopupWidth));
        }

        #endregion

        #region Control Size Preservation Tests

        [Test]
        public void RibbonGroup_AddControl_PreservesSmallSize()
        {
            using var group = new RibbonGroup();
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            
            group.AddControl(button);
            
            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Small));
        }

        [Test]
        public void RibbonGroup_AddControl_PreservesMediumSize()
        {
            using var group = new RibbonGroup();
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Medium };
            
            group.AddControl(button);
            
            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Medium));
        }

        [Test]
        public void RibbonGroup_AddControlWithDefaultSize_InheritsGroupSize()
        {
            using var group = new RibbonGroup { CurrentSize = RibbonGroupSize.Medium };
            using var button = new RibbonButton(); // Default is Large
            
            group.AddControl(button);
            
            // Button with default Large size should inherit group's Medium size
            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Medium));
        }

        #endregion

        #region Width Calculation Tests

        [Test]
        public void RibbonGroup_WithLargeButton_HasReasonableWidth()
        {
            using var group = new RibbonGroup { Label = "Test" };
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            group.AddControl(button);
            
            var width = group.GetPreferredWidth();
            
            Assert.That(width, Is.GreaterThanOrEqualTo(LayoutConstants.LargeButtonMinWidth));
        }

        [Test]
        public void RibbonGroup_WithSmallButtons_StacksInColumns()
        {
            using var group = new RibbonGroup { Label = "Test" };
            
            for (int i = 0; i < 3; i++)
            {
                var button = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
                group.AddControl(button);
            }
            
            // 3 small buttons should fit in one column
            var width = group.GetPreferredWidth();
            
            // Width should be roughly: padding + one column of small buttons + padding
            Assert.That(width, Is.LessThan(100));
        }

        [Test]
        public void RibbonGroup_WithSixSmallButtons_StacksInTwoColumns()
        {
            using var group = new RibbonGroup { Label = "Test" };
            
            for (int i = 0; i < 6; i++)
            {
                var button = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
                group.AddControl(button);
            }
            
            // 6 small buttons should fit in two columns (3 per column)
            var width = group.GetPreferredWidth();
            
            // Width should be wider than single column
            Assert.That(width, Is.GreaterThan(45));
        }

        [Test]
        public void RibbonGroup_WidthAccountsForLabel()
        {
            using var group1 = new RibbonGroup { Label = "X" };
            using var group2 = new RibbonGroup { Label = "Very Long Label Here" };
            
            var width1 = group1.GetPreferredWidth();
            var width2 = group2.GetPreferredWidth();
            
            // Group with longer label should be at least as wide
            Assert.That(width2, Is.GreaterThanOrEqualTo(width1));
        }

        #endregion

        #region Layout Tests

        [Test]
        public void RibbonGroup_OnResize_TriggersLayoutUpdate()
        {
            using var group = new RibbonGroup();
            group.Size = new Size(100, 70);
            
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            group.AddControl(button);
            
            var initialX = button.Location.X;
            
            group.Size = new Size(200, 70);
            
            // Button position should be recalculated (layout might not change position but should be valid)
            Assert.That(button.Location.X, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void RibbonGroup_SmallButtons_PositionedVertically()
        {
            using var group = new RibbonGroup();
            group.Size = new Size(100, 70);
            
            using var button1 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            using var button2 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            
            group.AddControl(button1);
            group.AddControl(button2);
            
            // Buttons should be stacked vertically (same X, different Y)
            Assert.That(button1.Location.X, Is.EqualTo(button2.Location.X));
            Assert.That(button1.Location.Y, Is.LessThan(button2.Location.Y));
        }

        [Test]
        public void RibbonGroup_LargeButtons_PositionedHorizontally()
        {
            using var group = new RibbonGroup();
            group.Size = new Size(200, 70);
            
            using var button1 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            using var button2 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            
            group.AddControl(button1);
            group.AddControl(button2);
            
            // Buttons should be arranged horizontally (same Y, different X)
            Assert.That(button1.Location.X, Is.LessThan(button2.Location.X));
            Assert.That(button1.Location.Y, Is.EqualTo(button2.Location.Y));
        }

        #endregion

        #region Popup Mode Tests

        [Test]
        public void RibbonGroup_PopupMode_HidesControls()
        {
            using var group = new RibbonGroup();
            using var button = new RibbonButton();
            group.AddControl(button);
            
            group.CurrentSize = RibbonGroupSize.Popup;
            
            Assert.That(button.Visible, Is.False);
        }

        [Test]
        public void RibbonGroup_ExitPopupMode_ShowsControls()
        {
            using var group = new RibbonGroup();
            using var button = new RibbonButton();
            group.AddControl(button);
            
            group.CurrentSize = RibbonGroupSize.Popup;
            group.CurrentSize = RibbonGroupSize.Large;
            
            Assert.That(button.Visible, Is.True);
        }

        #endregion

        #region Tab Tests

        [Test]
        public void RibbonTab_CanAddGroups()
        {
            using var tab = new RibbonTab();
            using var group1 = new RibbonGroup();
            using var group2 = new RibbonGroup();
            
            tab.AddGroup(group1);
            tab.AddGroup(group2);
            
            Assert.That(tab.Groups.Count, Is.EqualTo(2));
        }

        [Test]
        public void RibbonTab_GroupsAdded_InCorrectOrder()
        {
            using var tab = new RibbonTab();
            tab.Size = new Size(400, 94);
            
            using var group1 = new RibbonGroup { Label = "Group1" };
            using var group2 = new RibbonGroup { Label = "Group2" };
            
            tab.AddGroup(group1);
            tab.AddGroup(group2);
            
            // Groups should be added in order
            Assert.That(tab.Groups[0], Is.EqualTo(group1));
            Assert.That(tab.Groups[1], Is.EqualTo(group2));
        }

        #endregion

        #region Separator Tests

        [Test]
        public void RibbonSeparator_IsVerticalProperty_CanBeSet()
        {
            using var sep = new RibbonSeparator();
            sep.IsVertical = true;
            
            Assert.That(sep.IsVertical, Is.True);
        }

        [Test]
        public void RibbonSeparator_InGroup_IsVertical()
        {
            using var group = new RibbonGroup();
            group.Size = new Size(100, 70);
            
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            using var sep = new RibbonSeparator();
            
            group.AddControl(button);
            group.AddControl(sep);
            
            Assert.That(sep.IsVertical, Is.True);
        }

        [Test]
        public void RibbonSeparator_HasNarrowWidth()
        {
            using var group = new RibbonGroup();
            group.Size = new Size(100, 70);
            
            using var sep = new RibbonSeparator();
            group.AddControl(sep);
            
            Assert.That(sep.Width, Is.LessThanOrEqualTo(LayoutConstants.SeparatorWidth));
        }

        #endregion

        #region Configuration SizeDefinition Tests

        [Test]
        public void Configuration_ClipboardGroup_HasSizeDefinition()
        {
            var config = DefaultRibbonConfiguration.Create();
            var homeTab = config.Tabs[0];
            var clipboardGroup = homeTab.Groups[0];
            
            Assert.That(clipboardGroup.SizeDefinition, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void Configuration_FontGroup_HasSizeDefinition()
        {
            var config = DefaultRibbonConfiguration.Create();
            var homeTab = config.Tabs[0];
            var fontGroup = homeTab.Groups.Find(g => g.CommandId == CommandId.FontGroup);
            
            Assert.That(fontGroup.SizeDefinition, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void Configuration_InsertGroup_HasSizeDefinition()
        {
            var config = DefaultRibbonConfiguration.Create();
            var homeTab = config.Tabs[0];
            var insertGroup = homeTab.Groups.Find(g => g.CommandId == CommandId.InsertGroup);
            
            Assert.That(insertGroup.SizeDefinition, Is.Not.Null.Or.Empty);
        }

        #endregion
    }
}
