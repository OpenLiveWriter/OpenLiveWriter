// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    public class SystemButtonHelper
    {
        private class ButtonSettings
        {
            public int BUTTON_FACE_LEFT_INSET = 2;
            public int BUTTON_FACE_TOP_INSET = 2;
            public int BUTTON_FACE_BOTTOM_INSET = 2;
            public int BUTTON_FACE_RIGHT_INSET = 2;
            public int BUTTON_FACE_DROP_DOWN_RIGHT_INSET = 1;
            public int BUTTON_FACE_DROP_DOWN_LEFT_INSET = 1;
            public int DROP_DOWN_BUTTON_WIDTH = 15;
        }
        private static readonly ButtonSettings DefaultButtonSettings = new ButtonSettings();

        public const int SMALL_BUTTON_IMAGE_SIZE = 16;
        public const int LARGE_BUTTON_IMAGE_SIZE = 38;
        public const int LARGE_BUTTON_TOTAL_SIZE = 42;

        /// <summary>
        /// Draw the system button face.
        /// </summary>
        /// <param name="graphics">Graphics context in which the system button face is to be drawn.</param>
        public static void DrawSystemButtonFace(BidiGraphics graphics, bool DropDownContextMenuUserInterface, bool contextMenuShowing, Rectangle clientRectangle, bool isLargeButton)
        {
            DrawSystemButtonFace(graphics, DropDownContextMenuUserInterface, contextMenuShowing, clientRectangle, isLargeButton, DefaultButtonSettings);
        }
        private static void DrawSystemButtonFace(BidiGraphics graphics, bool DropDownContextMenuUserInterface, bool contextMenuShowing, Rectangle VirtualClientRectangle, bool isLargeButton, ButtonSettings settings)
        {
            // calculate bitmaps
            Bitmap hoverButtonFace = isLargeButton ? buttonFaceBitmapLarge : buttonFaceBitmap;
            Bitmap pressedButtonFace = isLargeButton ? buttonFacePushedBitmapLarge : buttonFacePushedBitmap;

            // draw button face
            DrawSystemButtonFace(graphics,
                                 DropDownContextMenuUserInterface,
                                 VirtualClientRectangle,
                                 hoverButtonFace,
                                 contextMenuShowing ? pressedButtonFace : hoverButtonFace,
                                 isLargeButton,
                                 settings);
        }

        /// <summary>
        /// Draw system button face in the pushed state.
        /// </summary>
        /// <param name="graphics">Graphics context in which the system button face is to be drawn in the pushed state.</param>
        public static void DrawSystemButtonFacePushed(BidiGraphics graphics, bool DropDownContextMenuUserInterface, Rectangle clientRectangle, bool isLargeButton)
        {
            DrawSystemButtonFacePushed(graphics, DropDownContextMenuUserInterface, clientRectangle, isLargeButton, DefaultButtonSettings);
        }

        private static void DrawSystemButtonFacePushed(BidiGraphics graphics, bool DropDownContextMenuUserInterface, Rectangle clientRectangle, bool isLargeButton, ButtonSettings settings)
        {
            Bitmap pressedButtonFace = isLargeButton ? buttonFacePushedBitmapLarge : buttonFacePushedBitmap;
            DrawSystemButtonFace(graphics, DropDownContextMenuUserInterface, clientRectangle, pressedButtonFace, pressedButtonFace, isLargeButton, settings);
        }

        private static void DrawSystemButtonFace(BidiGraphics graphics, bool DropDownContextMenuUserInterface, Rectangle clientRectangle, Image buttonBitmap, Image contextMenuButtonBitmap, bool isLargeButton, ButtonSettings settings)
        {
            // get rectangles
            Rectangle leftRectangle = isLargeButton ? buttonFaceLeftRectangleLarge : buttonFaceLeftRectangle;
            Rectangle centerRectangle = isLargeButton ? buttonFaceCenterRectangleLarge : buttonFaceCenterRectangle;
            Rectangle rightRectangle = isLargeButton ? buttonFaceRightRectangleLarge : buttonFaceRightRectangle;

            // determine height
            int height = isLargeButton ? LARGE_BUTTON_TOTAL_SIZE : clientRectangle.Height;

            //	Compute the button face rectangle.
            Rectangle buttonRectangle = new Rectangle(clientRectangle.X,
                clientRectangle.Y,
                clientRectangle.Width,
                height);

            if (DropDownContextMenuUserInterface)
            {
                //	Compute the drop-down rectangle.
                Rectangle dropDownRectangle = new Rectangle(clientRectangle.Right - settings.DROP_DOWN_BUTTON_WIDTH,
                    clientRectangle.Y,
                    settings.DROP_DOWN_BUTTON_WIDTH,
                    height);
                GraphicsHelper.DrawLeftCenterRightImageBorder(graphics,
                    dropDownRectangle,
                    contextMenuButtonBitmap,
                    leftRectangle,
                    centerRectangle,
                    rightRectangle);

                buttonRectangle.Width -= dropDownRectangle.Width - 1;
            }

            //	Draw the border.
            GraphicsHelper.DrawLeftCenterRightImageBorder(graphics,
                buttonRectangle,
                buttonBitmap,
                leftRectangle,
                centerRectangle,
                rightRectangle);
        }

        /// <summary>
        /// Button face.
        /// </summary>
        private static Bitmap buttonFaceBitmap { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonHover.png"); } }

        /// <summary>
        /// Pushed button face.
        /// </summary>
        private static Bitmap buttonFacePushedBitmap { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonPressed.png"); } }

        /// <summary>
        ///	Left button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceLeftRectangle = new Rectangle(0, 0, 3, 24);

        /// <summary>
        ///	Center button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceCenterRectangle = new Rectangle(3, 0, 1, 24);

        /// <summary>
        ///	Right button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceRightRectangle = new Rectangle(4, 0, 3, 24);

        /// <summary>
        /// Button face.
        /// </summary>
        private static Bitmap buttonFaceBitmapLarge { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonHoverLarge.png"); } }

        /// <summary>
        /// Pushed button face.
        /// </summary>
        private static Bitmap buttonFacePushedBitmapLarge { get { return ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonPressedLarge.png"); } }

        /// <summary>
        ///	Left button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceLeftRectangleLarge = new Rectangle(0, 0, 4, 42);

        /// <summary>
        ///	Center button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceCenterRectangleLarge = new Rectangle(4, 0, 1, 42);

        /// <summary>
        ///	Right button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceRightRectangleLarge = new Rectangle(5, 0, 4, 42);
    }
}
