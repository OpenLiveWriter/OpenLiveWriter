// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Base class for owner-draw menu items.
    /// </summary>
    public class OwnerDrawMenuItem : MenuItem
    {
        /// <summary>
        /// The menu type.
        /// </summary>
        private MenuType menuType;

        private Font menuFont;

        /// <summary>
        /// The menu text.
        /// </summary>
        protected readonly string text;

        /// <summary>
        /// Top-level menu item text padding.
        /// </summary>
        private const int TOP_LEVEL_TEXT_PADDING = 2;

        /// <summary>
        /// Standard menu item minimum width.
        /// </summary>
        private const int STANDARD_MINIMUM_WIDTH = 150;

        /// <summary>
        /// Standard menu item maximum text width.
        /// </summary>
        private const int STANDARD_MAXIMUM_TEXT_WIDTH = 400;

        /// <summary>
        /// Standard menu item bitmap area width.
        /// </summary>
        private const int STANDARD_BITMAP_AREA_WIDTH = 20;

        /// <summary>
        /// Standard menu item text padding.
        /// </summary>
        private const int STANDARD_TEXT_PADDING = 4;

        /// <summary>
        /// Standard menu item separator padding.
        /// </summary>
        private const int STANDARD_SEPARATOR_PADDING = 0;

        /// <summary>
        /// Standard menu item right edge padding.
        /// </summary>
        private const int STANDARD_RIGHT_EDGE_PAD = 12;

        /// <summary>
        /// Main menu item text string format when hot keys are displayed.
        /// </summary>
        private static readonly TextFormatFlags mainMenuItemTextHotKeyStringFormat;

        /// <summary>
        /// Main menu item text string format when hot keys are not displayed.
        /// </summary>
        private static readonly TextFormatFlags mainMenuItemTextNoHotKeyStringFormat;

        /// <summary>
        /// Menu item text string format when hot keys are displayed.
        /// </summary>
        private static readonly TextFormatFlags menuItemTextHotKeyStringFormat;

        /// <summary>
        /// Menu item text string format when hot keys are not displayed.
        /// </summary>
        private static readonly TextFormatFlags menuItemTextNoHotKeyStringFormat;

        /// <summary>
        /// Shortcut text string format.
        /// </summary>
        private static readonly TextFormatFlags shortcutStringFormat;

        /// <summary>
        /// Offscreen bitmap.  Cached between calls to improve efficiency.
        /// </summary>
        private Bitmap offscreenBitmap;

        /// <summary>
        /// Occurs before the menu item is shown.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs before the menu item is shown.")
        ]
        public event EventHandler BeforeShow;

        /// <summary>
        ///	Initializes the static member variables of the OwnerDrawMenuItem class.
        /// </summary>
        static OwnerDrawMenuItem()
        {
            //	Initialize the main menu item hot key text string format.
            mainMenuItemTextHotKeyStringFormat = new TextFormatFlags();
            mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.HorizontalCenter;
            mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.WordBreak;
            mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.EndEllipsis;
            mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.TextBoxControl;

            //	Initialize the main menu item no hot key text string format.
            mainMenuItemTextNoHotKeyStringFormat = new TextFormatFlags();
            mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.HorizontalCenter;
            mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.WordBreak;
            mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.HidePrefix;
            mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.TextBoxControl;
            mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.EndEllipsis;

            //	Initialize the menu item hot key text string format.
            menuItemTextNoHotKeyStringFormat = new TextFormatFlags();
            menuItemTextNoHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            menuItemTextNoHotKeyStringFormat |= TextFormatFlags.HidePrefix;
            menuItemTextNoHotKeyStringFormat |= TextFormatFlags.WordBreak;
            menuItemTextNoHotKeyStringFormat |= TextFormatFlags.TextBoxControl;
            menuItemTextNoHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            menuItemTextNoHotKeyStringFormat |= TextFormatFlags.EndEllipsis;

            //	Initialize the menu item no hot key text string format.
            menuItemTextHotKeyStringFormat = new TextFormatFlags();
            menuItemTextHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            menuItemTextHotKeyStringFormat |= TextFormatFlags.WordBreak;
            menuItemTextHotKeyStringFormat |= TextFormatFlags.TextBoxControl;
            menuItemTextHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            menuItemTextHotKeyStringFormat |= TextFormatFlags.EndEllipsis;

            //	Initialize the shortcut string format.
            shortcutStringFormat = new TextFormatFlags();
            shortcutStringFormat |= TextFormatFlags.VerticalCenter;
            shortcutStringFormat |= TextFormatFlags.Right;
            shortcutStringFormat |= TextFormatFlags.WordBreak;
            shortcutStringFormat |= TextFormatFlags.TextBoxControl;
            shortcutStringFormat |= TextFormatFlags.ExpandTabs;
            shortcutStringFormat |= TextFormatFlags.EndEllipsis;
        }

        /// <summary>
        ///	Initializes a new instance of the OwnerDrawMenuItem class.
        /// </summary>
        public OwnerDrawMenuItem(MenuType menuType, string text)
        {
            //	Set the menu type value.
            this.menuType = menuType;
            this.text = text;
            Text = text;

            menuFont = Res.DefaultFont;

            //	We dont owner draw menu items when in high contrast because it disabled some accessibility features
            //if(!SystemInformation.HighContrast && !BidiHelper.IsRightToLeft)
            //	OwnerDraw = true;
        }

        /// <summary>
        /// Gets the menu type.
        /// </summary>
        public MenuType MenuType
        {
            get
            {
                return menuType;
            }
        }

        /// <summary>
        /// Raises the BeforeShow event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal virtual void InvokeBeforeShow(EventArgs e)
        {
            OnBeforeShow(e);
        }

        /// <summary>
        ///	Menu bitmap method.  Returns the bitmap to draw for the owner draw menu item.  Override
        ///	this method in a derived class to return the appropriate bitmap to draw based on the
        ///	draw item state of the MenuItem.
        /// </summary>
        /// <param name="drawItemState">The draw item state.</param>
        /// <returns>Bitmap to draw.</returns>
        protected virtual Bitmap MenuBitmap(DrawItemState drawItemState)
        {
            return null;
        }

        /// <summary>
        ///	Menu text method.  Returns the text to draw for the owner draw menu item.  Override
        ///	this method in a derived class to return the appropriate text to draw based on the
        ///	draw item state of the MenuItem.
        /// </summary>
        /// <returns>Text to draw.</returns>
        protected virtual bool IncludeShortcutArea()
        {
            return false;
        }

        /// <summary>
        ///	Menu text method.  Returns the text to draw for the owner draw menu item.  Override
        ///	this method in a derived class to return the appropriate text to draw based on the
        ///	draw item state of the MenuItem.
        /// </summary>
        /// <returns>Text to draw.</returns>
        protected virtual string MenuText()
        {
            return text;
        }

        /// <summary>
        /// Raises the BeforeShow event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnBeforeShow(EventArgs e)
        {
            if (BeforeShow != null)
                BeforeShow(this, e);
        }

        /// <summary>
        /// Raises the DrawItem event.
        /// </summary>
        /// <param name="e">A DrawItemEventArgs that contains the event data.</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            //	Call the base class's method.
            base.OnDrawItem(e);

            //	Do the menu hack.
            MenuHack(e);

            //	Draw the menu item.
            if (Parent is MainMenu)
            {
                DrawMainMenuItem(e);
                //	Draw the offscreen bitmap.
                //BidiGraphics g = new BidiGraphics(e.Graphics, new Size(GetMainMenu().GetForm().Width, e.Bounds.Height));
                //g.DrawImage(false, offscreenBitmap, e.Bounds.Location);

            }
            else
            {
                //	If we have an offscreen bitmap, and it's the wrong size, dispose of it.
                //  Add try/catch due to lots of Watson-reported crashes here
                //    on offscreenBitmap.Size ("object is in use elsewhere" yadda yadda)
                try
                {
                    if (offscreenBitmap != null && offscreenBitmap.Size != e.Bounds.Size)
                    {
                        offscreenBitmap.Dispose();
                        offscreenBitmap = null;
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail(ex.ToString());
                    offscreenBitmap = null;
                }

                //	Allocate the offscreen bitmap, if we don't have one cached.
                if (offscreenBitmap == null)
                    offscreenBitmap = new Bitmap(e.Bounds.Width, e.Bounds.Height);

                DrawMenuItem(e.State, e.Bounds);
                //	Draw the offscreen bitmap.
                BidiGraphics g = new BidiGraphics(e.Graphics, e.Bounds);
                //in the main menu drop downs, the highlight rectangle is off by one
                Point location = e.Bounds.Location;
                if (e.Graphics.VisibleClipBounds.X == -1.0)
                    location.X += 1;

                g.DrawImage(false, offscreenBitmap, location);
            }
        }

        /// <summary>
        /// Raises the DrawItem event.
        /// </summary>
        /// <param name="e">A MeasureItemEventArgs that contains the event data.</param>
        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            //	Call the base class's method.
            base.OnMeasureItem(e);
            //	Measure the menu item.
            if (Parent is MainMenu)
            {
                //	Measure main menu item.
                Size textSize = MeasureMainMenuItemText(e.Graphics, MenuText());

                //	Set the width and height.
                e.ItemWidth = textSize.Width + (TOP_LEVEL_TEXT_PADDING * 2);
                e.ItemHeight = SystemInformation.MenuHeight;
            }
            else if (MenuType == MenuType.Context)
            {
                //	Measure context menu item.
                Size textSize = MeasureMenuItemText(e.Graphics, MenuText());

                //	Set the width.
                e.ItemWidth = STANDARD_BITMAP_AREA_WIDTH +
                                STANDARD_TEXT_PADDING +
                                textSize.Width +
                                STANDARD_RIGHT_EDGE_PAD;

                //	Set the height.
                if (MenuText() == "-")
                    e.ItemHeight = 5;
                else
                    e.ItemHeight = SystemInformation.MenuHeight;
            }
            else    //	Standard menu item.
            {
                //	Measure the menu text.
                Size textSize = MeasureMenuItemText(e.Graphics, MenuText());

                //	Determine the size of the shortcut.  If this item does not show a shortcut,
                //	measure a defauly shortcut so it aligns with other menu entries.
                Size shortcutSize;
                if (ShowShortcut && Shortcut != Shortcut.None)
                    shortcutSize = MeasureShortcutMenuItemText(e.Graphics, FormatShortcutString(Shortcut));
                else
                    shortcutSize = MeasureShortcutMenuItemText(e.Graphics, FormatShortcutString(Shortcut.CtrlIns));

                //	Set the width.
                e.ItemWidth = STANDARD_BITMAP_AREA_WIDTH +
                                STANDARD_TEXT_PADDING +
                                textSize.Width +
                                STANDARD_TEXT_PADDING +
                                shortcutSize.Width +
                                STANDARD_RIGHT_EDGE_PAD;
                if (e.ItemWidth < STANDARD_MINIMUM_WIDTH)
                    e.ItemWidth = STANDARD_MINIMUM_WIDTH;

                //	Set the height.
                if (MenuText() == "-")
                    e.ItemHeight = 5;
                else
                    e.ItemHeight = SystemInformation.MenuHeight;
            }
        }

        /// <summary>
        /// Raises the OnPopup event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnPopup(EventArgs e)
        {
            //	Enumerate the menu items and raise the BeforeShow event on each.
            foreach (MenuItem menuItem in MenuItems)
                if (menuItem is OwnerDrawMenuItem)
                    ((OwnerDrawMenuItem)menuItem).OnBeforeShow(EventArgs.Empty);

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPopup(e);
        }

        /// <summary>
        /// Menu hack.
        /// </summary>
        /// <param name="e">A DrawItemEventArgs that contains the event data.</param>
        private void MenuHack(DrawItemEventArgs e)
        {
            //	We start off with a bit of a hack.  The explanation is long.  There is a paint bug
            //	in the .NET MenuItem that manifests itself when:
            //		1) The OwnerDraw property is true.
            //		2) The OnDrawImage draws an image over the entire Bounds of the menu item as
            //		   specified in DrawItemEventArgs (in our case, this is true because we're
            //		   painting using double buffering, but it would be true for any other sort
            //		   of image).
            //		3) The OnDrawItem is being called to redraw a menu item as a result of a
            //		   cascading menu being removed.
            //	When these conditions are true, .NET inexplicably leaves a turd on the screen.
            //	It's really amazing.  Anyway, the ONLY fix is to clean up the turd or not draw an
            //	image.  We needed double buffer
            if (!(Parent is MainMenu))
            {
                using (SolidBrush solidBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control)))
                {
                    //	The turd rectangle.  Roughly 10 pixels at the right edge of the bounds of the
                    //	menu item.
                    Rectangle turdRectangle = new Rectangle(e.Bounds.X + (e.Bounds.Width - 10),
                                                            e.Bounds.Y,
                                                            10,
                                                            e.Bounds.Height);

                    //	Fill in the turd rectangle with the System.Drawing.KnownColor.Menu color, and
                    //	this masks the bug.
                    e.Graphics.FillRectangle(SystemBrushes.Menu, turdRectangle);
                }
            }
        }

        /// <summary>
        /// Draws a top-level menu item.
        /// </summary>
        /// <param name="drawItemState">A DrawItemEventArgs that contains the event data.</param>
        private void DrawMainMenuItem(DrawItemEventArgs ea)
        {
            // record state
            DrawItemState drawItemState = ea.State;

            //	Create graphics context on the offscreen bitmap and set the bounds for painting.
            //Graphics graphics = Graphics.FromImage(offscreenBitmap);
            Graphics graphics = ea.Graphics;

            BidiGraphics g = new BidiGraphics(graphics, new Size(GetMainMenu().GetForm().Width, 0));

            //Rectangle bounds = new Rectangle(0, 0, offscreenBitmap.Width, offscreenBitmap.Height);
            Rectangle bounds = ea.Bounds;

            // get reference to colorized resources
            ColorizedResources cres = ColorizedResources.Instance;

            //	Fill the menu item with the correct color
            Form containingForm = GetMainMenu().GetForm();
            if ((containingForm is IMainMenuBackgroundPainter))
            {
                (containingForm as IMainMenuBackgroundPainter).PaintBackground(g.Graphics, g.TranslateRectangle(ea.Bounds));
            }
            else
            {
                g.FillRectangle(SystemBrushes.Control, bounds);
            }

            //	Draw the hotlight or selection rectangle.
            if ((drawItemState & DrawItemState.HotLight) != 0 || (drawItemState & DrawItemState.Selected) != 0)
            {
                //	Cheat some for the first top-level menu item.  Provide a bit of "air" at the
                //	left of the "HotLight" rectangle so it doesn't run into the frame.
                int xOffset = (Index == 0) ? 1 : 0;

                //	Calculate the hotlight rectangle.
                Rectangle hotlightRectangle = new Rectangle(bounds.X + xOffset,
                                                                bounds.Y + 1,
                                                                bounds.Width - xOffset - 1,
                                                                bounds.Height - 1);
                DrawHotlight(g, cres.MainMenuHighlightColor, hotlightRectangle, !cres.CustomMainMenuPainting);
            }

            //	Calculate the text area rectangle.  This area excludes an area at the right
            //	edge of the menu item where the system draws the cascade indicator.  It would
            //	have been better if MenuItem let us draw the indicator (we did say "OwnerDraw"
            //	after all), but this is just how it works.
            Rectangle textAreaRectangle = new Rectangle(bounds.X,
                                                            bounds.Y + 1,
                                                            bounds.Width,
                                                            bounds.Height);

            //	Determine which StringFormat to use when drawing the menu item text.
            TextFormatFlags textFormat;
            if ((drawItemState & DrawItemState.NoAccelerator) != 0)
                textFormat = mainMenuItemTextNoHotKeyStringFormat;
            else
                textFormat = mainMenuItemTextHotKeyStringFormat;

            //DisplayHelper.FixupGdiPlusLineCentering(graphics, menuFont, MenuText(), ref stringFormat, ref textAreaRectangle);

            //	Draw the shortcut and the menu text.
            TextRenderer.DrawText(g.Graphics, MenuText(), menuFont, textAreaRectangle, cres.MainMenuTextColor, textFormat);

            //	We're finished with the double buffered painting.  Dispose of the graphics context
            //	and draw the offscreen image.  Cache the offscreen bitmap, though.
            graphics.Dispose();
        }

        private static void DrawHotlight(BidiGraphics graphics, Color highlightColor, Rectangle hotlightRectangle, bool drawBackground)
        {
            //	Fill the menu item with the system-defined menu color so the highlight color
            //	looks right.
            if (drawBackground)
                graphics.FillRectangle(SystemBrushes.Menu, hotlightRectangle);

            //	Draw the selection indicator using a 25% opaque version of the highlight color.
            using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(64, highlightColor)))
                graphics.FillRectangle(solidBrush, hotlightRectangle);

            //	Draw a rectangle around the selection indicator to frame it in better using a
            //	50% opaque version of the highlight color (this combines with the selection
            //	indicator to be 75% opaque).
            using (Pen pen = new Pen(Color.FromArgb(128, highlightColor)))
                graphics.DrawRectangle(
                    pen, new Rectangle(
                                        hotlightRectangle.X,
                                        hotlightRectangle.Y,
                                        hotlightRectangle.Width - 1,
                                        hotlightRectangle.Height - 1));
        }

        /// <summary>
        /// Draws a menu item.
        /// </summary>
        /// <param name="drawItemState">A DrawItemEventArgs that contains the event data.</param>
        /// <param name="rect">A DrawItemEventArgs that contains the client rectangle.</param>
        private void DrawMenuItem(DrawItemState drawItemState, Rectangle rect)
        {
            //	Create graphics context on the offscreen bitmap and set the bounds for painting.
            Graphics graphics = Graphics.FromImage(offscreenBitmap);
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            BidiGraphics g = new BidiGraphics(graphics, new Rectangle(Point.Empty, offscreenBitmap.Size));
            try
            {
                Rectangle bounds = new Rectangle(0, 0, offscreenBitmap.Width, offscreenBitmap.Height);

                //	Fill the menu item with the system-defined menu color.
                g.FillRectangle(SystemBrushes.Menu, bounds);

                //	Fill the bitmap area with the system-defind control color.
                Rectangle bitmapAreaRectangle = new Rectangle(bounds.X,
                                                                bounds.Y,
                                                                STANDARD_BITMAP_AREA_WIDTH,
                                                                bounds.Height);

                //	Fill the background.
                /*
                using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.MenuBitmapAreaColor))
                    graphics.FillRectangle(solidBrush, bitmapAreaRectangle);
                */
                Color backgroundColor = SystemColors.Menu;
                //	If the item is selected, draw the selection rectangle.
                if ((drawItemState & DrawItemState.Selected) != 0)
                {
                    DrawHotlight(g, SystemColors.Highlight, bounds, true);
                    backgroundColor = offscreenBitmap.GetPixel(2, 2);
                }

                //	Obtain the bitmap to draw.  If there is one, draw it centered in the bitmap area.
                Bitmap bitmap = MenuBitmap(drawItemState);
                if (bitmap != null)
                    g.DrawImage(false, bitmap, new Point(
                                                bounds.X + Utility.CenterMinZero(bitmap.Width, bitmapAreaRectangle.Width),
                                                bounds.Y + Utility.CenterMinZero(bitmap.Height, bitmapAreaRectangle.Height)));

                //	Obtain the menu text.  If it's not "-", then this is a menu item.  Otherwise, it's
                //	a separator menu item.
                if (MenuText() != "-")
                {
                    //	Calculate the text area rectangle.  This area excludes an area at the right
                    //	edge of the menu item where the system draws the cascade indicator.  It would
                    //	have been better if MenuItem let us draw the indicator (we did say "OwnerDraw"
                    //	afterall), but this is just how it works.
                    Rectangle textAreaRectangle = new Rectangle(bounds.X + STANDARD_BITMAP_AREA_WIDTH + STANDARD_TEXT_PADDING,
                                                                bounds.Y,
                                                                bounds.Width - (STANDARD_BITMAP_AREA_WIDTH + STANDARD_TEXT_PADDING + STANDARD_RIGHT_EDGE_PAD),
                                                                bounds.Height);

                    //	Select the brush to draw the menu text with.
                    Color color;
                    if ((drawItemState & DrawItemState.Disabled) == 0)
                        color = SystemColors.MenuText;
                    else
                        color = SystemColors.GrayText;

                    //	Determine the size of the shortcut, if it is being shown.
                    if (!(MenuType == MenuType.Context))
                    {
                        string shortcut;
                        Size shortcutSize;
                        if (ShowShortcut && Shortcut != Shortcut.None)
                        {
                            shortcut = FormatShortcutString(Shortcut);
                            shortcutSize = MeasureShortcutMenuItemText(graphics, shortcut);
                        }
                        else
                        {
                            shortcut = null;
                            shortcutSize = MeasureShortcutMenuItemText(graphics, FormatShortcutString(Shortcut.CtrlIns));
                        }

                        //	Draw the shortcut.
                        if (shortcut != null)
                        {
                            Rectangle shortcutTextRect = textAreaRectangle;
                            TextFormatFlags textFormatTemp = shortcutStringFormat;
                            //DisplayHelper.FixupGdiPlusLineCentering(graphics, menuFont, shortcut, ref stringFormatTemp, ref shortcutTextRect);
                            //	Draw the shortcut text.
                            g.DrawText(shortcut,
                                        menuFont,
                                        shortcutTextRect,
                                        color,
                                        backgroundColor,
                                        textFormatTemp);
                        }

                        //	Reduce the width of the text area rectangle to account for the shortcut and
                        //	the padding before it.
                        textAreaRectangle.Width -= shortcutSize.Width + STANDARD_TEXT_PADDING;
                    }

                    //	Determine which StringFormat to use when drawing the menu item text.
                    TextFormatFlags textFormat;
                    if ((drawItemState & DrawItemState.NoAccelerator) != 0)
                        textFormat = menuItemTextNoHotKeyStringFormat;
                    else
                        textFormat = menuItemTextHotKeyStringFormat;

                    //DisplayHelper.FixupGdiPlusLineCentering(graphics, menuFont, MenuText(), ref stringFormat, ref textAreaRectangle);
                    //	Draw the text.
                    g.DrawText(MenuText(),
                                menuFont,
                                textAreaRectangle,
                                color,
                                backgroundColor,
                                textFormat);
                }
                else
                {
                    //	Calculate the separator line rectangle.  This area excludes an area at the right
                    //	edge of the menu item where the system draws the cascade indicator.  It would
                    //	have been better if MenuItem let us draw the indicator (we did say "OwnerDraw"
                    //	after all), but this is just how it works.
                    Rectangle separatorLineRectangle = new Rectangle(bounds.X + STANDARD_SEPARATOR_PADDING,
                                                                        bounds.Y + Utility.CenterMinZero(1, bounds.Height),
                                                                        bounds.Width - STANDARD_SEPARATOR_PADDING,
                                                                        1);

                    //	Fill the separator line rectangle.
                    g.FillRectangle(SystemBrushes.ControlDark, separatorLineRectangle);
                }
            }
            finally
            {
                //	We're finished with the double buffered painting.  Dispose of the graphics context
                //	and draw the offscreen image.  Cache the offscreen bitmap, though.
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Measures the size of main menu item text.
        /// </summary>
        /// <param name="graphics">Graphics context in which to perform the measurement.</param>
        /// <param name="text">Text to measure.</param>
        /// <returns>Size of the text in pixels.</returns>
        private Size MeasureMainMenuItemText(Graphics graphics, string text)
        {
            //	Measure the text.
            Size size = Size.Empty;
            if (text != null && text.Length != 0)
                size = TextRenderer.MeasureText(graphics, text, menuFont, new Size(STANDARD_MAXIMUM_TEXT_WIDTH, menuFont.Height),
                                                mainMenuItemTextHotKeyStringFormat);

            return size;
        }

        /// <summary>
        /// Measures the size of menu item text.
        /// </summary>
        /// <param name="graphics">Graphics context in which to perform the measurement.</param>
        /// <param name="text">Text to measure.</param>
        /// <returns>Size of the text in pixels.</returns>
        private Size MeasureMenuItemText(Graphics graphics, string text)
        {
            //	Measure the text.
            Size size = Size.Empty;
            if (text != null && text.Length != 0)
                size = TextRenderer.MeasureText(graphics, text, menuFont, new Size(STANDARD_MAXIMUM_TEXT_WIDTH, menuFont.Height),
                                menuItemTextHotKeyStringFormat);
            return size;
        }

        /// <summary>
        /// Measures the shortcut size of shortcut text.
        /// </summary>
        /// <param name="graphics">Graphics context in which to perform the measurement.</param>
        /// <param name="text">Text to measure.</param>
        /// <returns>Size of the shortcut text in pixels.</returns>
        private Size MeasureShortcutMenuItemText(Graphics graphics, string shortcut)
        {
            //	Measure the shortcut.
            Size size = Size.Empty;
            if (shortcut != null && shortcut.Length != 0)
                size = TextRenderer.MeasureText(graphics, shortcut, menuFont, new Size(1000, menuFont.Height),
                shortcutStringFormat);
            return size;
        }

        /// <summary>
        /// Format a shortcut as a string.
        /// </summary>
        /// <param name="shortcut">Shortcut to format.</param>
        /// <returns>String format of shortcut.</returns>
        private string FormatShortcutString(Shortcut shortcut)
        {
            return KeyboardHelper.FormatShortcutString(shortcut);
        }

    }

    internal interface IMainMenuBackgroundPainter
    {
        void PaintBackground(Graphics g, Rectangle menuItemBounds);
    }
}
