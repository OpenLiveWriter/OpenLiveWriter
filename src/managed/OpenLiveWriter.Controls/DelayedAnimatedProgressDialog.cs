// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls
{

    public class DelayedAnimatedProgressDialog : ApplicationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private OpenLiveWriter.CoreServices.AsyncOperation _asyncOperation;
        private AnimatedBitmapControl _animatedBitmapControl;

        public DelayedAnimatedProgressDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Text = Res.Get(StringId.ProgressDialogTitle);
            Icon = ApplicationEnvironment.ProductIcon;
        }

        public void ShowDialogWithDelay(IWin32Window owner, OpenLiveWriter.CoreServices.AsyncOperation asyncOperation, int delayMs)
        {
            // JJA: THIS TECHNIQUE DOES NOT WORK AND CAUSES ALL KINDS OF PROBLEMS! WE NEED A DIFFERENT
            // TECHNIQUE THAT DOES NOT STARVE THE UI THREAD. DO NOT UNDER ANY CONDITIONS RESTORE
            // THIS CODE!!!!!!
            /*
            // start out by polling the get post operation for completion for the delay interval
            using ( new WaitCursor() )
            {
                const int SPLICE_MS = 100 ;
                int waitMs = 0 ;
                while( waitMs < delayMs )
                {
                    if ( asyncOperation.IsDone )
                    {
                        return ;
                    }
                    else
                    {
                        Thread.Sleep(SPLICE_MS);
                        waitMs += SPLICE_MS ;
                    }
                }
            }
            */

            // got past the delay interval, need to signup for events and show the dialog
            _asyncOperation = asyncOperation;

            if (_asyncOperation.IsDone)
                return;

            // show the dialog
            ShowDialog(owner);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _asyncOperation.Completed += new EventHandler(_asyncOperation_Completed);
            _asyncOperation.Cancelled += new EventHandler(_asyncOperation_Cancelled);
            _asyncOperation.Failed += new ThreadExceptionEventHandler(_asyncOperation_Failed);

            if (_asyncOperation.IsDone)
                Close();
        }

        protected void SetAnimatatedBitmapControl(AnimatedBitmapControl animatedBitmapControl)
        {
            _animatedBitmapControl = animatedBitmapControl;
        }

        protected void Cancel()
        {
            CancelWithoutClose();
            Close();
        }

        protected void CancelWithoutClose()
        {
            _asyncOperation.Cancel();
        }

        private void _asyncOperation_Failed(object sender, ThreadExceptionEventArgs e)
        {
            Close();
        }

        private void _asyncOperation_Completed(object sender, EventArgs e)
        {
            Close();
        }

        private void _asyncOperation_Cancelled(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            Debug.Assert(_animatedBitmapControl != null);

            if (!_animatedBitmapControl.Running)
                _animatedBitmapControl.Start();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_animatedBitmapControl.IsDisposed && _animatedBitmapControl.Running)
                {
                    _animatedBitmapControl.Stop();
                }

                if (_asyncOperation != null)
                {
                    _asyncOperation.Completed -= new EventHandler(_asyncOperation_Completed);
                    _asyncOperation.Cancelled -= new EventHandler(_asyncOperation_Cancelled);
                    _asyncOperation.Failed -= new ThreadExceptionEventHandler(_asyncOperation_Failed);
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // DelayedAnimatedProgressDialog
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(264, 141);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DelayedAnimatedProgressDialog";
            this.Text = "Progress Dialog";

        }
        #endregion

    }
}
