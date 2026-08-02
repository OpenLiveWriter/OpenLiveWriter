// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Controls;

namespace OpenLiveWriter.Tests.Ribbon
{
    /// <summary>
    /// Covers the collapsed-group popup shown when a ribbon group no longer fits
    /// the window width (e.g. Paragraph and HTML styles at narrow widths). The
    /// popup must show the group's controls, give the hosted content panel back
    /// to the group when it closes, and work on repeated opens. Regression test
    /// for the popup that hosted invisible controls and never reparented them.
    /// </summary>
    [TestFixture]
    public class RibbonGroupPopupTests
    {
        private static void InvokeShowPopup(RibbonGroup group)
        {
            var method = typeof(RibbonGroup).GetMethod("ShowPopup",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(group, null);
        }

        private static T GetField<T>(RibbonGroup group, string name)
        {
            var field = typeof(RibbonGroup).GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(group);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void CollapsedGroupPopup_ShowsControls_RestoresOnClose_WorksTwice()
        {
            using var form = new Form();
            using var group = new RibbonGroup { Label = "Paragraph" };
            var buttons = Enumerable.Range(0, 3)
                .Select(i => new RibbonButton { Label = "B" + i })
                .ToList();
            foreach (var button in buttons)
                group.AddControl(button);
            form.Controls.Add(group);
            group.Size = new Size(200, 92);
            // Show the form: Control.Visible reports false for every control whose
            // ancestor chain contains an invisible control, regardless of the
            // control's own visibility flag, so a never-shown form would make the
            // assertions meaningless.
            form.Show();

            // Collapse the group: controls hide behind the popup button.
            group.CurrentSize = RibbonGroupSize.Popup;
            Assert.IsTrue(buttons.All(b => !b.Visible), "collapsed group hides its controls");

            // First open: the content panel must be hosted, visible, and showing
            // the group's controls. AutoClose must be off so nested dropdowns
            // (e.g. Picture > From your computer) cannot close the popup and
            // drop their clicks through to the controls behind it.
            InvokeShowPopup(group);
            var dropDown = GetField<ToolStripDropDown>(group, "_popupDropDown");
            var contentPanel = GetField<Panel>(group, "_contentPanel");
            Assert.NotNull(dropDown, "popup dropdown was not created");
            Assert.IsFalse(dropDown.AutoClose,
                "group popup must not auto-close, or nested dropdown clicks fall through");
            Assert.IsTrue(dropDown.Visible, "popup dropdown should be visible");
            Assert.IsTrue(contentPanel.Visible, "content panel must be visible while hosted in the popup");
            Assert.IsTrue(buttons.All(b => b.Visible), "popup must show the group's controls");

            // Close: the panel returns to the group and the controls hide again
            // while the group is still collapsed.
            dropDown.Close();
            Assert.AreSame(group, contentPanel.Parent,
                "content panel must return to the group when the popup closes");
            Assert.IsTrue(buttons.All(b => !b.Visible),
                "still-collapsed group keeps controls hidden after the popup closes");

            // Second open: hosting must work again (the panel was reparented once).
            InvokeShowPopup(group);
            Assert.IsTrue(dropDown.Visible, "popup must open on the second click");
            Assert.IsTrue(contentPanel.Visible, "content panel must be visible on the second open");
            Assert.IsTrue(buttons.All(b => b.Visible), "popup must show controls on the second open");
            dropDown.Close();

            // Re-expand: controls show again inside the group itself.
            group.CurrentSize = RibbonGroupSize.Medium;
            Assert.AreSame(group, contentPanel.Parent,
                "content panel must be back in the group after re-expanding");
            Assert.IsTrue(buttons.All(b => b.Visible),
                "re-expanded group shows its controls");
        }
        [Test]
        [Apartment(ApartmentState.STA)]
        public void DropDownRegistry_InsideDetectionCoversNestedDropDowns()
        {
            using (var dropDown = new ToolStripDropDown())
            {
                // A dropdown with no items never becomes visible.
                dropDown.Items.Add(new ToolStripMenuItem("Item"));
                dropDown.Show(new Point(200, 200));

                var hookType = typeof(RibbonGroup).Assembly.GetType(
                    "OpenLiveWriter.Ribbon.Managed.Controls.DropDownMouseHook");
                var register = hookType.GetMethod("RegisterVisibleDropDown",
                    BindingFlags.NonPublic | BindingFlags.Static);
                var unregister = hookType.GetMethod("UnregisterVisibleDropDown",
                    BindingFlags.NonPublic | BindingFlags.Static);
                var isInside = hookType.GetMethod("IsInsideAnyVisibleDropDown",
                    BindingFlags.NonPublic | BindingFlags.Static);

                register.Invoke(null, new object[] { dropDown });
                var insidePoint = new Point(
                    dropDown.Bounds.X + dropDown.Bounds.Width / 2,
                    dropDown.Bounds.Y + dropDown.Bounds.Height / 2);
                Assert.IsTrue(dropDown.Visible,
                    $"dropdown should be visible; bounds={dropDown.Bounds}");
                Assert.IsTrue((bool)isInside.Invoke(null, new object[] { insidePoint }),
                    "a click inside a registered dropdown must count as inside");
                Assert.IsFalse((bool)isInside.Invoke(null, new object[] { new Point(2000, 2000) }),
                    "a click outside all dropdowns must count as outside");

                unregister.Invoke(null, new object[] { dropDown });
                Assert.IsFalse((bool)isInside.Invoke(null, new object[] { insidePoint }),
                    "unregistered dropdowns no longer count as inside");

                dropDown.Close();
            }
        }
    }
}
