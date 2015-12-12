// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor
{
    public sealed class WindowCascadeHelper
    {

        private static Point referenceLocation;
        /// <summary>
        /// Cannot be instantiated or subclassed
        /// </summary>
        private WindowCascadeHelper()
        {
        }

        public static void SetNextOpenedLocation(Point location)
        {
            referenceLocation = location;
        }

        public static Point GetNewPostLocation(Size formSize, int offset)
        {
            //case 1: opened through new post, so there is a window to cascade against
            if (!referenceLocation.IsEmpty)
            {
                Point openerLocation = referenceLocation;
                //make sure that we will cascade against a visible window
                Screen targetScreen = FindTitlebarVisibleScreen(openerLocation, formSize, offset);
                if (null != targetScreen)
                {
                    //cascade our location, and check that it doesn't go off screen
                    Point newLocation = new Point(openerLocation.X + offset, openerLocation.Y + offset);
                    if (targetScreen.WorkingArea.Contains(new Rectangle(newLocation, formSize)))
                    {
                        return newLocation;
                    }
                    //roll window over
                    return FixUpLocation(targetScreen, newLocation, formSize);
                }
            }
            //else, we are going to try the setting from last closed post
            Point lastClosedLocation = PostEditorSettings.PostEditorWindowLocation;
            if (null != FindTitlebarVisibleScreen(lastClosedLocation, formSize, offset))
            {
                return lastClosedLocation;
            }
            //nothing works, return empty point
            return Point.Empty;
        }

        private static Point FixUpLocation(Screen ourScreen, Point location, Size recSize)
        {
            Point topRight = new Point(location.X + recSize.Width, location.Y);
            Point lowerLeft = new Point(location.X, location.Y + recSize.Height);
            int newLeft = location.X;
            int newTop = location.Y;
            if (!ourScreen.WorkingArea.Contains(topRight))
            {
                newLeft = ourScreen.WorkingArea.X;
            }
            if (!ourScreen.WorkingArea.Contains(lowerLeft))
            {
                newTop = ourScreen.WorkingArea.Y;
            }
            return new Point(newLeft, newTop);
        }

        private static Screen FindVisibleScreen(Point location, Size recSize)
        {
            Screen[] allScreens = Screen.AllScreens;
            Rectangle rect = new Rectangle(location, recSize);
            foreach (Screen screen in allScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return screen;
                }
            }
            return null;
        }
        //verifies that if the form is at this location at least the toolbar will be visible
        //helps in cases where screens have been disabled, resolution changed, etc.
        private static Screen FindTitlebarVisibleScreen(Point location, Size formSize, int offset)
        {
            Size topbarSize = new Size(formSize.Width, offset);
            return FindVisibleScreen(location, topbarSize);
        }
    }
}
