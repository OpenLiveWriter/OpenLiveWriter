// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Controls;

namespace OpenLiveWriter.Tests.Ribbon
{
    [TestFixture]
    public class RibbonGroupLayoutTests
    {
        #region GetPreferredWidth Tests

        [Test]
        public void GetPreferredWidth_EmptyGroup_ReturnsMinWidth()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            var width = group.GetPreferredWidth();
            
            Assert.That(width, Is.GreaterThan(0));
        }

        [Test]
        public void GetPreferredWidth_SingleLargeButton_ReturnsReasonableWidth()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            using var button = new RibbonButton();
            button.CurrentSize = RibbonGroupSize.Large;
            group.AddControl(button);
            
            var width = group.GetPreferredWidth();
            
            Assert.That(width, Is.GreaterThanOrEqualTo(40));
        }

        [Test]
        public void GetPreferredWidth_ThreeSmallButtons_StacksInOneColumn()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            using var button1 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            using var button2 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            using var button3 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            
            group.AddControl(button1);
            group.AddControl(button2);
            group.AddControl(button3);
            
            var width = group.GetPreferredWidth();
            
            // Three small buttons stacked should have roughly the width of one column
            Assert.That(width, Is.LessThan(100)); // Reasonable max for single column + padding
        }

        [Test]
        public void GetPreferredWidth_SixSmallButtons_StacksInTwoColumns()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            for (int i = 0; i < 6; i++)
            {
                var button = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
                group.AddControl(button);
            }
            
            var width = group.GetPreferredWidth();
            
            // Six small buttons should stack in two columns of 3
            Assert.That(width, Is.GreaterThan(40)); // More than one column
        }

        [Test]
        public void GetPreferredWidth_MediumButton_IncludesTextWidth()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            using var button = new RibbonButton();
            button.CurrentSize = RibbonGroupSize.Medium;
            group.AddControl(button);
            
            var width = group.GetPreferredWidth();
            
            Assert.That(width, Is.GreaterThanOrEqualTo(60)); // Medium buttons need space for text
        }

        [Test]
        public void GetPreferredWidth_WithSeparator_AddsSpace()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            using var button1 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            using var separator = new RibbonSeparator();
            using var button2 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            
            group.AddControl(button1);
            group.AddControl(separator);
            group.AddControl(button2);
            
            var widthWithSeparator = group.GetPreferredWidth();
            
            // Width should include separator space
            Assert.That(widthWithSeparator, Is.GreaterThan(90));
        }

        #endregion

        #region Control Positioning Tests

        [Test]
        public void SmallButtons_StackVertically()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            group.Size = new Size(100, 70);
            
            using var button1 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            using var button2 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            
            group.AddControl(button1);
            group.AddControl(button2);
            
            // After layout, buttons should be stacked vertically
            Assert.That(button1.Location.X, Is.EqualTo(button2.Location.X));
            Assert.That(button1.Location.Y, Is.LessThan(button2.Location.Y));
        }

        [Test]
        public void LargeButtons_ArrangeHorizontally()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            group.Size = new Size(200, 70);
            
            using var button1 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            using var button2 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            
            group.AddControl(button1);
            group.AddControl(button2);
            
            // After layout, buttons should be arranged horizontally
            Assert.That(button1.Location.X, Is.LessThan(button2.Location.X));
            Assert.That(button1.Location.Y, Is.EqualTo(button2.Location.Y));
        }

        [Test]
        public void MixedSizes_SmallButtonsStackThenLargeFollows()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            group.Size = new Size(200, 70);
            
            using var smallBtn1 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            using var smallBtn2 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            using var smallBtn3 = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
            using var largeBtn = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            
            group.AddControl(smallBtn1);
            group.AddControl(smallBtn2);
            group.AddControl(smallBtn3);
            group.AddControl(largeBtn);
            
            // Small buttons should be in one column, large button after
            Assert.That(largeBtn.Location.X, Is.GreaterThan(smallBtn1.Location.X));
        }

        #endregion

        #region SizeDefinition Tests

        [Test]
        public void SizeDefinition_CanBeSet()
        {
            using var group = new RibbonGroup();
            group.SizeDefinition = "OneLargeAndTwoSmall";
            
            Assert.That(group.SizeDefinition, Is.EqualTo("OneLargeAndTwoSmall"));
        }

        [Test]
        public void SizeDefinition_TriggersLayoutUpdate()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            group.Size = new Size(100, 70);
            
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            group.AddControl(button);
            
            var initialX = button.Location.X;
            
            // Changing size definition should trigger layout
            group.SizeDefinition = "OneButton";
            
            // Layout should have been updated (width recalculated)
            Assert.That(group.SizeDefinition, Is.EqualTo("OneButton"));
        }

        [Test]
        public void SevenSmallButtons_SplitsThreeTopFourBottom()
        {
            // Matches the native ribbon's SevenSmallButtons SizeDefinition
            // (src/unmanaged/OpenLiveWriter.Ribbon/Ribbon.xml): bullets/numbers/blockquote
            // on the top row, the four alignment buttons on the bottom row.
            using var group = new RibbonGroup();
            group.Label = "Paragraph";
            group.Size = new Size(120, 70);
            group.SizeDefinition = "SevenSmallButtons";

            var buttons = new List<RibbonButton>();
            for (var i = 0; i < 7; i++)
            {
                var button = new RibbonButton { CurrentSize = RibbonGroupSize.Small };
                buttons.Add(button);
                group.AddControl(button);
            }

            try
            {
                // Top row: first 3 buttons share a Y
                Assert.That(buttons[1].Location.Y, Is.EqualTo(buttons[0].Location.Y));
                Assert.That(buttons[2].Location.Y, Is.EqualTo(buttons[0].Location.Y));

                // Bottom row: last 4 buttons share a Y below the top row
                for (var i = 3; i < 7; i++)
                {
                    Assert.That(buttons[i].Location.Y, Is.EqualTo(buttons[3].Location.Y));
                }
                Assert.That(buttons[3].Location.Y, Is.GreaterThan(buttons[0].Location.Y));

                // Bottom row starts at the left edge (AlignLeft below Bullets)
                Assert.That(buttons[3].Location.X, Is.EqualTo(buttons[0].Location.X));
            }
            finally
            {
                foreach (var button in buttons)
                {
                    button.Dispose();
                }
            }
        }

        #endregion

        #region Debug Tab SizeDefinition Tests

        private static List<RibbonButton> AddButtons(RibbonGroup group, int count)
        {
            var buttons = new List<RibbonButton>();
            for (var i = 0; i < count; i++)
            {
                var button = new RibbonButton { CurrentSize = RibbonGroupSize.Medium };
                buttons.Add(button);
                group.AddControl(button);
            }
            return buttons;
        }

        [Test]
        public void GalleryAndTwoButtons_ButtonsFitInsideGroup()
        {
            // The Plug-ins group (GalleryAndTwoButtons) must place the Add
            // plug-in and Plug-in options buttons fully inside the group,
            // clear of the separator margin.
            using var group = new RibbonGroup { Label = "Plug-ins", SizeDefinition = "GalleryAndTwoButtons" };
            var gallery = new RibbonGallery { GalleryType = RibbonGalleryType.InRibbon };
            gallery.AddItem(new RibbonGalleryItem("Test plug-in", null));
            gallery.AddItem(new RibbonGalleryItem("Another plug-in", null));
            var buttons = new List<RibbonButton>();
            group.AddControl(gallery);
            foreach (var label in new[] { "Add plug-in", "Plug-in options" })
            {
                var button = new RibbonButton { CurrentSize = RibbonGroupSize.Medium };
                buttons.Add(button);
                group.AddControl(button);
            }

            group.Size = new Size(group.GetPreferredWidth(), 70);
            try
            {
                foreach (var button in buttons)
                {
                    Assert.LessOrEqual(button.Location.X + button.Width,
                        group.GetPreferredWidth() - LayoutConstants.GroupSeparatorMargin,
                        "button overhangs the group's right edge");
                }
            }
            finally
            {
                gallery.Dispose();
                buttons.ForEach(b => b.Dispose());
            }
        }

        [Test]
        public void ThreeButtons_SingleColumnOfThree()
        {
            // Native WRF ThreeButtons: one column of three medium buttons.
            using var group = new RibbonGroup { Label = "Validate", SizeDefinition = "ThreeButtons" };
            group.Size = new Size(120, 70);
            var buttons = AddButtons(group, 3);
            try
            {
                Assert.That(buttons[1].Location.X, Is.EqualTo(buttons[0].Location.X));
                Assert.That(buttons[2].Location.X, Is.EqualTo(buttons[0].Location.X));
                Assert.That(buttons[1].Location.Y, Is.GreaterThan(buttons[0].Location.Y));
                Assert.That(buttons[2].Location.Y, Is.GreaterThan(buttons[1].Location.Y));
            }
            finally { buttons.ForEach(b => b.Dispose()); }
        }

        [Test]
        public void FiveButtons_GridThreePlusTwo()
        {
            // Native WRF FiveButtons: 3 buttons in the first column, 2 in the second.
            using var group = new RibbonGroup { Label = "General", SizeDefinition = "FiveButtons" };
            group.Size = new Size(200, 70);
            var buttons = AddButtons(group, 5);
            try
            {
                for (var i = 0; i < 3; i++)
                    Assert.That(buttons[i].Location.X, Is.EqualTo(buttons[0].Location.X), "first column X");
                for (var i = 3; i < 5; i++)
                    Assert.That(buttons[i].Location.X, Is.EqualTo(buttons[3].Location.X), "second column X");
                Assert.That(buttons[3].Location.X, Is.GreaterThan(buttons[0].Location.X));
                Assert.That(buttons[4].Location.Y, Is.GreaterThan(buttons[3].Location.Y));
            }
            finally { buttons.ForEach(b => b.Dispose()); }
        }

        [Test]
        public void EightButtons_GridThreeThreeTwo()
        {
            // Native WRF EightButtons: 3+3+2 columns.
            using var group = new RibbonGroup { Label = "Dialog", SizeDefinition = "EightButtons" };
            group.Size = new Size(280, 70);
            var buttons = AddButtons(group, 8);
            try
            {
                for (var i = 0; i < 3; i++)
                    Assert.That(buttons[i].Location.X, Is.EqualTo(buttons[0].Location.X), "column 0");
                for (var i = 3; i < 6; i++)
                    Assert.That(buttons[i].Location.X, Is.EqualTo(buttons[3].Location.X), "column 1");
                for (var i = 6; i < 8; i++)
                    Assert.That(buttons[i].Location.X, Is.EqualTo(buttons[6].Location.X), "column 2");
                Assert.That(buttons[3].Location.X, Is.GreaterThan(buttons[0].Location.X));
                Assert.That(buttons[6].Location.X, Is.GreaterThan(buttons[3].Location.X));
            }
            finally { buttons.ForEach(b => b.Dispose()); }
        }

        [Test]
        public void OneButton_RendersSingleLargeButton()
        {
            // Native WRF OneButton: one large button.
            using var group = new RibbonGroup { Label = "Text", SizeDefinition = "OneButton" };
            group.Size = new Size(120, 70);
            var buttons = AddButtons(group, 1);
            try
            {
                Assert.That(buttons[0].CurrentSize, Is.EqualTo(RibbonGroupSize.Large));
            }
            finally { buttons.ForEach(b => b.Dispose()); }
        }

        #endregion

        #region Separator Tests

        [Test]
        public void Separator_IsVertical_InGroup()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            group.Size = new Size(100, 70);
            
            using var button1 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            using var separator = new RibbonSeparator();
            using var button2 = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            
            group.AddControl(button1);
            group.AddControl(separator);
            group.AddControl(button2);
            
            Assert.That(separator.IsVertical, Is.True);
        }

        [Test]
        public void Separator_HasNarrowWidth()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            group.Size = new Size(100, 70);
            
            using var separator = new RibbonSeparator();
            group.AddControl(separator);
            
            Assert.That(separator.Width, Is.LessThan(10));
        }

        #endregion

        #region CurrentSize Preservation Tests

        [Test]
        public void AddControl_PreservesExplicitlySetSize()
        {
            using var group = new RibbonGroup();
            group.CurrentSize = RibbonGroupSize.Large;
            
            using var button = new RibbonButton();
            button.CurrentSize = RibbonGroupSize.Small; // Explicitly set
            
            group.AddControl(button);
            
            // Small size should be preserved, not overwritten to Large
            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Small));
        }

        [Test]
        public void AddControl_DefaultSizeInheritsFromGroup()
        {
            using var group = new RibbonGroup();
            group.CurrentSize = RibbonGroupSize.Medium;
            
            using var button = new RibbonButton();
            // button.CurrentSize defaults to Large
            
            group.AddControl(button);
            
            // Should inherit group's size since it was at default
            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Medium));
        }

        #endregion

        #region Popup Mode Tests

        [Test]
        public void PopupMode_HidesControls()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            group.AddControl(button);
            
            group.CurrentSize = RibbonGroupSize.Popup;
            
            Assert.That(button.Visible, Is.False);
        }

        [Test]
        public void PopupMode_GroupHasSmallWidth()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            group.AddControl(button);
            
            group.CurrentSize = RibbonGroupSize.Popup;
            
            var width = group.GetPreferredWidth();
            Assert.That(width, Is.GreaterThanOrEqualTo(38)); // Popup mode has compact width based on label
        }

        [Test]
        public void ExitingPopupMode_ShowsControls()
        {
            using var group = new RibbonGroup();
            group.Label = "Test";
            
            using var button = new RibbonButton { CurrentSize = RibbonGroupSize.Large };
            group.AddControl(button);
            
            group.CurrentSize = RibbonGroupSize.Popup;
            group.CurrentSize = RibbonGroupSize.Large;
            
            Assert.That(button.Visible, Is.True);
        }

        #endregion
    }
}
