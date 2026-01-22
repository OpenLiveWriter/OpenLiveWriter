// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Controls;

namespace OpenLiveWriter.Tests.Ribbon
{
    [TestFixture]
    public class RibbonGalleryTests
    {
        #region Gallery Type Tests

        [Test]
        public void GalleryType_DefaultsToDropDown()
        {
            using var gallery = new RibbonGallery();
            Assert.That(gallery.GalleryType, Is.EqualTo(RibbonGalleryType.DropDown));
        }

        [Test]
        public void GalleryType_CanSetToInRibbon()
        {
            using var gallery = new RibbonGallery();
            gallery.GalleryType = RibbonGalleryType.InRibbon;
            Assert.That(gallery.GalleryType, Is.EqualTo(RibbonGalleryType.InRibbon));
        }

        #endregion

        #region Item Management Tests

        [Test]
        public void AddItem_IncreasesItemCount()
        {
            using var gallery = new RibbonGallery();
            var initialCount = gallery.Items.Count;
            
            gallery.AddItem(new RibbonGalleryItem("Test", null));
            
            Assert.That(gallery.Items.Count, Is.EqualTo(initialCount + 1));
        }

        [Test]
        public void ClearItems_RemovesAllItems()
        {
            using var gallery = new RibbonGallery();
            gallery.AddItem(new RibbonGalleryItem("Test1", null));
            gallery.AddItem(new RibbonGalleryItem("Test2", null));
            
            gallery.ClearItems();
            
            Assert.That(gallery.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void SelectedIndex_DefaultsToNegativeOne()
        {
            using var gallery = new RibbonGallery();
            Assert.That(gallery.SelectedIndex, Is.EqualTo(-1));
        }

        [Test]
        public void SelectedIndex_CanBeSet()
        {
            using var gallery = new RibbonGallery();
            gallery.AddItem(new RibbonGalleryItem("Test1", null));
            gallery.AddItem(new RibbonGalleryItem("Test2", null));
            
            gallery.SelectedIndex = 1;
            
            Assert.That(gallery.SelectedIndex, Is.EqualTo(1));
        }

        [Test]
        public void SelectedItem_ReturnsCorrectItem()
        {
            using var gallery = new RibbonGallery();
            var item1 = new RibbonGalleryItem("Test1", null);
            var item2 = new RibbonGalleryItem("Test2", null);
            gallery.AddItem(item1);
            gallery.AddItem(item2);
            
            gallery.SelectedIndex = 1;
            
            Assert.That(gallery.SelectedItem, Is.EqualTo(item2));
        }

        [Test]
        public void SelectedItem_ReturnsNullWhenNoSelection()
        {
            using var gallery = new RibbonGallery();
            gallery.AddItem(new RibbonGalleryItem("Test", null));
            
            Assert.That(gallery.SelectedItem, Is.Null);
        }

        #endregion

        #region Gallery Item Tests

        [Test]
        public void RibbonGalleryItem_StoresLabel()
        {
            var item = new RibbonGalleryItem("TestLabel", null);
            Assert.That(item.Label, Is.EqualTo("TestLabel"));
        }

        [Test]
        public void RibbonGalleryItem_StoresImage()
        {
            using var image = new Bitmap(16, 16);
            var item = new RibbonGalleryItem("Test", image);
            Assert.That(item.Image, Is.EqualTo(image));
        }

        [Test]
        public void RibbonGalleryItem_CanStoreTag()
        {
            var item = new RibbonGalleryItem("Test", null);
            item.Tag = "CustomData";
            Assert.That(item.Tag, Is.EqualTo("CustomData"));
        }

        #endregion

        #region Text Position Tests

        [Test]
        public void TextPosition_DefaultsToBottom()
        {
            using var gallery = new RibbonGallery();
            Assert.That(gallery.TextPosition, Is.EqualTo(RibbonTextPosition.Bottom));
        }

        [Test]
        public void TextPosition_CanSetToRight()
        {
            using var gallery = new RibbonGallery();
            gallery.TextPosition = RibbonTextPosition.Right;
            Assert.That(gallery.TextPosition, Is.EqualTo(RibbonTextPosition.Right));
        }

        [Test]
        public void TextPosition_CanSetToHide()
        {
            using var gallery = new RibbonGallery();
            gallery.TextPosition = RibbonTextPosition.Hide;
            Assert.That(gallery.TextPosition, Is.EqualTo(RibbonTextPosition.Hide));
        }

        #endregion

        #region Dimension Tests

        [Test]
        public void ItemWidth_HasReasonableDefault()
        {
            using var gallery = new RibbonGallery();
            Assert.That(gallery.ItemWidth, Is.GreaterThan(0));
        }

        [Test]
        public void ItemHeight_HasReasonableDefault()
        {
            using var gallery = new RibbonGallery();
            Assert.That(gallery.ItemHeight, Is.GreaterThan(0));
        }

        [Test]
        public void ItemWidth_CanBeSet()
        {
            using var gallery = new RibbonGallery();
            gallery.ItemWidth = 64;
            Assert.That(gallery.ItemWidth, Is.EqualTo(64));
        }

        [Test]
        public void ItemHeight_CanBeSet()
        {
            using var gallery = new RibbonGallery();
            gallery.ItemHeight = 36;
            Assert.That(gallery.ItemHeight, Is.EqualTo(36));
        }

        [Test]
        public void MaxColumns_HasReasonableDefault()
        {
            using var gallery = new RibbonGallery();
            Assert.That(gallery.MaxColumns, Is.GreaterThan(0));
        }

        [Test]
        public void MaxRows_HasReasonableDefault()
        {
            using var gallery = new RibbonGallery();
            Assert.That(gallery.MaxRows, Is.GreaterThan(0));
        }

        #endregion

        #region SemanticHtmlGallery Tests

        [Test]
        public void SemanticHtmlGallery_CommandId_CanBeSet()
        {
            using var gallery = new RibbonGallery();
            gallery.CommandId = CommandId.SemanticHtmlGallery;
            Assert.That(gallery.CommandId, Is.EqualTo(CommandId.SemanticHtmlGallery));
        }

        [Test]
        public void InRibbonGallery_HasCorrectSizing()
        {
            using var gallery = new RibbonGallery();
            gallery.GalleryType = RibbonGalleryType.InRibbon;
            gallery.ItemWidth = 64;
            gallery.ItemHeight = 36;
            gallery.MaxColumns = 7;
            gallery.MaxRows = 3;
            
            // Gallery should be able to display items
            Assert.That(gallery.ItemWidth * gallery.MaxColumns, Is.GreaterThan(100));
        }

        #endregion

        #region Scroll Tests

        [Test]
        public void InRibbonGallery_CanScroll_WhenManyItems()
        {
            using var gallery = new RibbonGallery();
            gallery.GalleryType = RibbonGalleryType.InRibbon;
            gallery.MaxRows = 1;
            gallery.MaxColumns = 3;
            
            // Add more items than can be displayed
            for (int i = 0; i < 10; i++)
            {
                gallery.AddItem(new RibbonGalleryItem($"Item {i}", null));
            }
            
            Assert.That(gallery.Items.Count, Is.EqualTo(10));
        }

        #endregion
    }
}
