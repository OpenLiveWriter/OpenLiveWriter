// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.ImageInsertion
{
    /// <summary>
    /// Summary description for ImageInsertSource
    /// </summary>
    public interface InsertImageSource
    {
        void Init(int width, int height);
        //tab name
        string TabName { get; }
        Bitmap TabBitmap { get; }
        //controls for central panel
        UserControl ImageSelectionControls { get; }
        //final chosen image file
        string FileName { get; }
        //used when select button is clicked
        bool ValidateSelection();
        //used when enter button is clicked--need to force your own buttons to accept!
        bool HandleEnter(int cmdId);
        bool Selected { get; set; }
        void Repaint(Size newSize);
        //void ResizeImageSelection(int fileIDWidth, int sideLength, Size mainSize, int fileInfoWidth);
        string SourceImageLink { get; }
        void TabSelected();
        event EventHandler OnSelectionMade;
    }
}
