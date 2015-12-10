// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;
using OpenLiveWriter.InternalWriterPlugin.Controls;

namespace OpenLiveWriter.InternalWriterPlugin
{
    namespace Controls
    {
        internal class MapBitmapButton : BitmapButton
        {
            public MapBitmapButton(string resourceName)
            {
                BackColor = Color.Transparent;
                ButtonStyle = ButtonStyle.Bitmap;
                BitmapEnabled = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "Images.{0}Enabled.png", resourceName));
                BitmapDisabled = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "Images.{0}Enabled.png", resourceName));
                BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "Images.{0}Pressed.png", resourceName));
                BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap(String.Format(CultureInfo.InvariantCulture, "Images.{0}Selected.png", resourceName));
                Width = (int)(BitmapEnabled.Width * scale.X);
                Height = (int)(BitmapEnabled.Height * scale.Y); ;
                AutoSizeHeight = true;
                AutoSizeWidth = true;
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
                scale = new PointF(scale.X * dx, scale.Y * dy);
            }

            PointF scale = new PointF(1f, 1f);
        }
    }

}
