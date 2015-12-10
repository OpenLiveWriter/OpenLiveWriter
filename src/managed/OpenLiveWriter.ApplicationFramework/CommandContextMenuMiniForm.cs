// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.ApplicationFramework
{

    /// <summary>
    /// ContextMenuMiniForm
    /// </summary>
    internal class CommandContextMenuMiniForm : BaseForm
    {
        /* NOTE: When being shown in the context of the browser (or any non .NET
         * application) this form will not handle any dialog level keyboard
         * commands (tab, enter, escape, alt-mnenonics, etc.). This is because
         * it is a modeless form that does not have its own thread/message-loop.
         * Because the form was created by our .NET code the main IE frame that
         * has the message loop has no idea it needs to route keyboard events'
         * to us. There are several possible workarounds:
         *
         *    (1) Create and show this form on its own thread with its own
         *        message loop. In this case all calls from the form back
         *        to the main UI thread would need to be marshalled.
         *
         *    (2) Manually process keyboard events in the low-level
         *        ProcessKeyPreview override (see commented out method below)
         *
         *    (3) Change the implementation of the mini-form to be a modal
         *        dialog. The only problem here is we would need to capture
         *        mouse input so that clicks outside of the modal dialog onto
         *        the IE window result in the window being dismissed. We were
         *        not able to get this to work (couldn't capture the mouse)
         *        in experimenting with this implementation.
         *
         * Our judgement was to leave it as-is for now as it is unlikely that
         * keyboard input into a mini-form will be a big deal (the only way
         * to access the mini-form is with a mouse gesture on the toolbar so
         * the user is still in "mouse-mode" when the form pops up.
         *
         */

        public CommandContextMenuMiniForm(IWin32Window parentFrame, Command command)
        {
            // save a reference to the parent frame
            _parentFrame = parentFrame;

            // save a reference to the command and context menu control handler
            _command = command;
            _contextMenuControlHandler = command.CommandBarButtonContextMenuControlHandler;

            // set to top most form (allows us to appear on top of our
            // owner if the owner is also top-most)
            TopMost = true;

            // other window options/configuration
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;

            // Paint performance optimizations
            User32.SetWindowLong(Handle, GWL.STYLE, User32.GetWindowLong(Handle, GWL.STYLE) & ~WS.CLIPCHILDREN);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // create and initialize the context menu control
            Control commandContextMenuControl = _contextMenuControlHandler.CreateControl();
            commandContextMenuControl.TabIndex = 0;
            commandContextMenuControl.BackColor = BACKGROUND_COLOR;
            commandContextMenuControl.Font = ApplicationManager.ApplicationStyle.NormalApplicationFont;
            commandContextMenuControl.Left = HORIZONTAL_INSET;
            commandContextMenuControl.Top = HEADER_INSET + HEADER_HEIGHT + HEADER_INSET + HEADER_INSET;
            Controls.Add(commandContextMenuControl);

            // create action button (don't add it yet)
            _actionButton = new BitmapButton();
            _actionButton.TabIndex = 1;
            _actionButton.Click += new EventHandler(_actionButton_Click);
            _actionButton.BackColor = BACKGROUND_COLOR;
            _actionButton.Font = ApplicationManager.ApplicationStyle.NormalApplicationFont;
            _actionButton.BitmapDisabled = _command.CommandBarButtonBitmapDisabled;
            _actionButton.BitmapEnabled = _command.CommandBarButtonBitmapEnabled;
            _actionButton.BitmapPushed = _command.CommandBarButtonBitmapPushed;
            _actionButton.BitmapSelected = _command.CommandBarButtonBitmapSelected;
            _actionButton.ButtonText = _contextMenuControlHandler.ButtonText;
            _actionButton.ToolTip = _contextMenuControlHandler.ButtonText;
            _actionButton.AutoSizeWidth = true;
            _actionButton.AutoSizeHeight = true;
            _actionButton.Size = new Size(0, 0); // dummy call to force auto-size

            // size the form based on the size of the context menu control and button
            Width = HORIZONTAL_INSET + commandContextMenuControl.Width + HORIZONTAL_INSET;
            Height = commandContextMenuControl.Bottom + (BUTTON_VERTICAL_PAD * 3) + miniFormBevelBitmap.Height + _actionButton.Height;

            // position the action button and add it to the form
            _actionButton.Top = Height - BUTTON_VERTICAL_PAD - _actionButton.Height;
            _actionButton.Left = HORIZONTAL_INSET - 4;
            Controls.Add(_actionButton);
        }

        /// <summary>
        /// Override out Activated event to allow parent form to retains its 'activated'
        /// look (caption bar color, etc.) even when we are active
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            // call base
            base.OnActivated(e);

            // send the parent form a WM_NCACTIVATE message to cause it to to retain it's
            // activated title bar appearance
            User32.SendMessage(_parentFrame.Handle, WM.NCACTIVATE, new UIntPtr(1), IntPtr.Zero);
        }

        /// <summary>
        /// Automatically close when the form is deactivated
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            // set a timer that will result in the closing of the form
            // (we do this because if actually call Close right here it
            // will prevent the mouse event that resulted in the deactivation
            // of the form from actually triggering in the new target
            // winodw -- this allows the mouse event to trigger and the
            // form to go away almost instantly
            Timer closeDelayTimer = new Timer();
            closeDelayTimer.Tick += new EventHandler(closeDelayTimer_Tick);
            closeDelayTimer.Interval = 10;
            closeDelayTimer.Start();
        }

        /// <summary>
        /// Actually close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeDelayTimer_Tick(object sender, EventArgs e)
        {
            // stop and dispose the timer
            Timer closeDelayTimer = (Timer)sender;
            closeDelayTimer.Stop();
            closeDelayTimer.Dispose();

            // cancel the form
            Cancel();
        }

        // handle painting
        protected override void OnPaint(PaintEventArgs e)
        {
            // get refrence to graphics context
            Graphics g = e.Graphics;

            // fill background
            using (SolidBrush backgroundBrush = new SolidBrush(BACKGROUND_COLOR))
                g.FillRectangle(backgroundBrush, ClientRectangle);

            // draw outer border
            Rectangle borderRectangle = ClientRectangle;
            borderRectangle.Width -= 1;
            borderRectangle.Height -= 1;
            using (Pen borderPen = new Pen(ApplicationManager.ApplicationStyle.BorderColor))
                g.DrawRectangle(borderPen, borderRectangle);

            // draw header region background
            using (SolidBrush headerBrush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceTopColor))
                g.FillRectangle(headerBrush, HEADER_INSET, HEADER_INSET, Width - (HEADER_INSET * 2), HEADER_HEIGHT);

            // draw header region text
            using (SolidBrush textBrush = new SolidBrush(ApplicationManager.ApplicationStyle.ToolWindowTitleBarTextColor))
                g.DrawString(
                    _contextMenuControlHandler.CaptionText,
                    ApplicationManager.ApplicationStyle.NormalApplicationFont,
                    textBrush,
                    new PointF(HEADER_INSET + 1, HEADER_INSET + 1));

            // draw bottom bevel line
            g.DrawImage(miniFormBevelBitmap, new Rectangle(
                HORIZONTAL_INSET - 1,
                Height - (2 * BUTTON_VERTICAL_PAD) - _actionButton.Height,
                Width - (HORIZONTAL_INSET * 2),
                miniFormBevelBitmap.Height));
        }

        /// <summary>
        /// Prevent background painting (supports double-buffering)
        /// </summary>
        /// <param name="pevent"></param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }


        /*
        protected override bool ProcessKeyPreview(ref Message m)
        {
            // NOTE: this is the only keyboard "event" which appears
            // to get called when our form is shown in the browser.
            // if we want to support tab, esc, enter, mnemonics, etc.
            // without creating a new thread/message-loop for this
            // form (see comment at the top) then this is where we
            // would do the manual processing

            return base.ProcessKeyPreview (ref m);
        }
        */

        /// <summary>
        /// User clicked the action button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void _actionButton_Click(object sender, EventArgs e)
        {
            Execute();
        }

        /// <summary>
        /// Cancel the mini-form
        /// </summary>
        private void Cancel()
        {
            Close();
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        private void Execute()
        {
            // get the data entered by the user
            object userInput = _contextMenuControlHandler.GetUserInput();

            // close the form
            Close();

            // tell the context menu control to execute using the specified user input
            _contextMenuControlHandler.Execute(userInput);
        }

        /// <summary>
        /// Handle to parent frame window
        /// </summary>
        private IWin32Window _parentFrame;

        /// <summary>
        /// Command we are associated with
        /// </summary>
        private Command _command;

        /// <summary>
        /// Context menu control handler
        /// </summary>
        private ICommandContextMenuControlHandler _contextMenuControlHandler;

        /// <summary>
        /// Button user clicks to take action
        /// </summary>
        private BitmapButton _actionButton;

        /// <summary>
        /// Button face bitmap
        /// </summary>
        private readonly Bitmap miniFormBevelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.MiniFormBevel.png");

        // layout and drawing contants
        private const int HEADER_INSET = 2;
        private const int HEADER_HEIGHT = 17;
        private const int HORIZONTAL_INSET = 10;
        private const int BUTTON_VERTICAL_PAD = 3;
        private static readonly Color BACKGROUND_COLOR = Color.FromArgb(244, 243, 238);

    }
}
