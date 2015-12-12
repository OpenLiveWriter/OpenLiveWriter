// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    [Obsolete]
    public interface IWorkspaceBorderManager
    {
        WorkspaceBorder WorkspaceBorders { get; set; }
    }

    [Obsolete]
    internal class SatelliteApplicationFormWorkspace : UserControl, IWorkspaceBorderManager
    {
        private Control _mainControl = null;
        private Panel panelLeft;
        private Panel panelTop;
        private Panel panelRight;
        private Panel panelBottom;
        private Panel panelMain;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private int _workspaceInset;

        public SatelliteApplicationFormWorkspace()
            : this(4) // for designer
        {
        }

        public SatelliteApplicationFormWorkspace(int workspaceInset)
        {
            // record workspace inset
            _workspaceInset = workspaceInset;

            SuspendLayout();

            // create main panel
            panelMain = new Panel();
            panelMain.Dock = DockStyle.Fill;
            Controls.Add(panelMain);

            // create border panels
            panelLeft = new Panel();
            panelLeft.Width = _workspaceInset;
            panelLeft.Dock = DockStyle.Left;
            Controls.Add(panelLeft);

            panelRight = new Panel();
            panelRight.Width = _workspaceInset;
            panelRight.Dock = DockStyle.Right;
            Controls.Add(panelRight);

            panelTop = new Panel();
            panelTop.Height = _workspaceInset;
            panelTop.Dock = DockStyle.Top;
            Controls.Add(panelTop);

            panelBottom = new Panel();
            panelBottom.Height = _workspaceInset; // was -2
            panelBottom.Dock = DockStyle.Bottom;
            Controls.Add(panelBottom);

            ResumeLayout();
        }

        public void SetMainControl(Control mainControl)
        {
            // add main control to main panel
            _mainControl = mainControl;
            mainControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(_mainControl);

            // sync to application style
            UpdateAppearance();
        }

        public WorkspaceBorder WorkspaceBorders
        {
            get
            {
                return (panelLeft.Visible ? WorkspaceBorder.Left : WorkspaceBorder.None) |
                        (panelRight.Visible ? WorkspaceBorder.Right : WorkspaceBorder.None) |
                        (panelTop.Visible ? WorkspaceBorder.Top : WorkspaceBorder.None) |
                        (panelBottom.Visible ? WorkspaceBorder.Bottom : WorkspaceBorder.None);
            }
            set
            {
                panelLeft.Visible = (value & WorkspaceBorder.Left) > 0;
                panelRight.Visible = (value & WorkspaceBorder.Right) > 0;
                panelTop.Visible = (value & WorkspaceBorder.Top) > 0;
                panelBottom.Visible = (value & WorkspaceBorder.Bottom) > 0;
            }
        }

        public void UpdateAppearance()
        {
            Color backgroundColor = ApplicationManager.ApplicationStyle.SecondaryWorkspaceBottomColor;

            // set back-color of panels to correct application style
            panelLeft.BackColor = panelTop.BackColor = panelRight.BackColor = panelBottom.BackColor = backgroundColor;

            // set back color of the main control to the correct application style
            _mainControl.BackColor = backgroundColor;

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

    }

    [Flags]
    public enum WorkspaceBorder
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    };

}
