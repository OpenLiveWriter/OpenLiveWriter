// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{
    internal class MapSearchButton : BitmapButton
    {
        public MapSearchButton()
        {
            Bitmap buttonFace = ResourceHelper.LoadAssemblyResourceBitmap("Images.BingSearchButton.gif");
            this.BitmapEnabled = buttonFace;
            this.BitmapSelected = buttonFace;
            this.BitmapPushed = buttonFace;
            this.ButtonStyle = ButtonStyle.Bitmap;
            this.Cursor = Cursors.Hand;
        }
    }

}
