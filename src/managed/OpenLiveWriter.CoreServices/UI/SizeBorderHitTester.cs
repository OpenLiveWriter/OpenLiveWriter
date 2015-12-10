// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices.UI
{
    public struct SizeBorderHitTester
    {
        private const int BORDER_SIZE = 6;
        private const int CORNER_SIZE = 20;

        private Size cachedSize;
        private Size gripperSize;
        private Size sysMenuSize;
        private RectAndInt[] cachedValues;

        public SizeBorderHitTester(Size gripperSize, Size sysMenuSize)
        {
            this.gripperSize = gripperSize;
            this.sysMenuSize = sysMenuSize;
            this.cachedSize = Size.Empty;
            this.cachedValues = null;
        }

        public int Test(Size formSize, Point test)
        {
            int width = formSize.Width;
            int height = formSize.Height;

            // gripper
            if (new Rectangle(
                width - gripperSize.Width,
                height - gripperSize.Height,
                gripperSize.Width,
                gripperSize.Height).Contains(test))
            {
                return HT.BOTTOMRIGHT;
            }

            // short-circuit if it's not at a border
            if (new Rectangle(BORDER_SIZE, BORDER_SIZE, width - BORDER_SIZE * 2, height - BORDER_SIZE * 2).Contains(test))
                return -1;

            if (cachedSize != formSize)
            {
                cachedSize = formSize;
                if (cachedValues == null)
                    cachedValues = new RectAndInt[12];

                // top border
                cachedValues[0] = new RectAndInt(
                    CORNER_SIZE,
                    0,
                    width - CORNER_SIZE * 2,
                    BORDER_SIZE,
                    HT.TOP);

                // bottom border
                cachedValues[1] = new RectAndInt(
                    CORNER_SIZE,
                    height - BORDER_SIZE,
                    width - CORNER_SIZE * 2,
                    BORDER_SIZE,
                    HT.BOTTOM);

                // left border
                cachedValues[2] = new RectAndInt(
                    0,
                    CORNER_SIZE,
                    BORDER_SIZE,
                    height - CORNER_SIZE * 2,
                    HT.LEFT
                    );

                // right border
                cachedValues[3] = new RectAndInt(
                    width - BORDER_SIZE,
                    CORNER_SIZE,
                    BORDER_SIZE,
                    height - CORNER_SIZE * 2,
                    HT.RIGHT);

                // top left corner
                cachedValues[4] = new RectAndInt(
                    0,
                    0,
                    CORNER_SIZE,
                    BORDER_SIZE,
                    HT.TOPLEFT
                    );
                cachedValues[5] = new RectAndInt(
                    0,
                    0,
                    BORDER_SIZE,
                    CORNER_SIZE,
                    HT.TOPLEFT
                    );

                // top right corner
                cachedValues[6] = new RectAndInt(
                    width - CORNER_SIZE,
                    0,
                    CORNER_SIZE,
                    BORDER_SIZE,
                    HT.TOPRIGHT
                    );
                cachedValues[7] = new RectAndInt(
                    width - BORDER_SIZE,
                    0,
                    BORDER_SIZE,
                    CORNER_SIZE,
                    HT.TOPRIGHT
                    );

                // bottom left corner
                cachedValues[8] = new RectAndInt(
                    0,
                    height - CORNER_SIZE,
                    BORDER_SIZE,
                    CORNER_SIZE,
                    HT.BOTTOMLEFT
                    );
                cachedValues[9] = new RectAndInt(
                    0,
                    height - BORDER_SIZE,
                    CORNER_SIZE,
                    BORDER_SIZE,
                    HT.BOTTOMLEFT
                    );

                // bottom right corner
                cachedValues[10] = new RectAndInt(
                    width - CORNER_SIZE,
                    height - BORDER_SIZE,
                    CORNER_SIZE,
                    BORDER_SIZE,
                    HT.BOTTOMRIGHT
                    );
                cachedValues[11] = new RectAndInt(
                    width - BORDER_SIZE,
                    height - CORNER_SIZE,
                    BORDER_SIZE,
                    CORNER_SIZE,
                    HT.BOTTOMRIGHT
                    );
            }

            foreach (RectAndInt rectAndInt in cachedValues)
            {
                if (rectAndInt.rect.Contains(test))
                    return rectAndInt.intValue;
            }

            return -1;
        }
    }

    struct RectAndInt
    {
        public RectAndInt(int x, int y, int width, int height, int intValue)
        {
            this.rect = new Rectangle(x, y, width, height);
            this.intValue = intValue;
        }

        public Rectangle rect;
        public int intValue;
    }
}
