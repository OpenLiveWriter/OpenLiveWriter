// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices.Marketization;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{
    internal class MapViewComboBox : ImageComboBox
    {
        private ComboItem birdsEyeComboItem;
        public MapViewComboBox()
            : base(new Size(18, 17))
        {
        }

        public bool AerialSupported
        {
            get
            {
                string aerialParameter = MarketizationOptions.GetFeatureParameter(MarketizationOptions.Feature.Maps, "aerial");
                if (aerialParameter == null || aerialParameter.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
        }

        public void Initialize()
        {
            Items.Add(new ComboItem(VEMapStyle.Road, Res.Get(StringId.MapRoad), "Images.RoadIcon.png"));

            if (AerialSupported)
            {
                Items.Add(new ComboItem(VEMapStyle.Aerial, Res.Get(StringId.MapAerial), "Images.AerialIcon.png"));
            }

            birdsEyeComboItem = new ComboItem(VEMapStyle.Birdseye, Res.Get(StringId.MapBirdseye), "Images.BEViewIcon.png");
        }

        public void Initialize(VEMapStyle initialValue)
        {
            Initialize();
            MapStyle = initialValue;
        }

        public void ShowBirdsEye()
        {
            if (!Items.Contains(birdsEyeComboItem))
                Items.Add(birdsEyeComboItem);
        }

        public void HideBirdsEye()
        {
            Items.Remove(birdsEyeComboItem);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VEMapStyle MapStyle
        {
            get
            {
                return (SelectedItem as ComboItem).MapStyle;
            }
            set
            {
                if (value == VEMapStyle.Birdseye && !Items.Contains(birdsEyeComboItem))
                    ShowBirdsEye();

                if (value == VEMapStyle.Aerial && !AerialSupported)
                {
                    value = VEMapStyle.Road;
                }

                ComboItem itemToSelect = null;
                foreach (ComboItem item in Items)
                {
                    if (item.MapStyle == value)
                    {
                        itemToSelect = item;
                        break;
                    }
                }
                if (itemToSelect != null)
                    SelectedItem = itemToSelect;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (ComboItem comboItem in Items)
                    comboItem.Dispose();
            }

            base.Dispose(disposing);
        }

        private class ComboItem : ImageComboBox.IComboItem, IDisposable
        {
            public ComboItem(VEMapStyle mapStyle, string caption, string imageResourcePath)
            {
                _mapStyle = mapStyle;
                _caption = caption;

                //get the image (relative to the root assembly)
                _image = new Bitmap(typeof(MapForm), imageResourcePath);
            }

            public void Dispose()
            {
                if (_image != null)
                    _image.Dispose();
            }

            public VEMapStyle MapStyle { get { return _mapStyle; } }
            private VEMapStyle _mapStyle;

            public Image Image { get { return _image; } }
            private Image _image;

            public override string ToString() { return _caption; }
            private string _caption;

            public override bool Equals(object obj) { return (obj as ComboItem).MapStyle == MapStyle; }
            public override int GetHashCode() { return MapStyle.GetHashCode(); }
        }
    }
}
