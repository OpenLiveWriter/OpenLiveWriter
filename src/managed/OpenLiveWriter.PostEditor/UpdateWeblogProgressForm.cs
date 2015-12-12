// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor
{
    public class UpdateWeblogProgressForm : BaseForm
    {
        private CheckBox checkBoxViewPost;
        private AnimatedBitmapControl progressAnimatedBitmap;
        private Label labelPublishingTo;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private IWin32Window _parentFrame;
        private IBlogPostPublishingContext _publishingContext;
        private bool _publish;
        private bool _republishOnSuccess;
        private UpdateWeblogAsyncOperation _updateWeblogAsyncOperation;
        private string _defaultProgressMessage;
        private string _cancelReason;

        /// <param name="parentFrame"></param>
        /// <param name="publishingContext"></param>
        /// <param name="isPage"></param>
        /// <param name="destinationName"></param>
        /// <param name="publish">If false, the publishing operation will post as draft</param>
        public UpdateWeblogProgressForm(IWin32Window parentFrame, IBlogPostPublishingContext publishingContext, bool isPage, string destinationName, bool publish)
        {
            InitializeComponent();

            this.checkBoxViewPost.Text = Res.Get(StringId.UpdateWeblogViewPost);

            // reference to parent frame and editing context
            _parentFrame = parentFrame;
            _publishingContext = publishingContext;
            _publish = publish;

            // look and feel (no form border and theme dervied background color)
            FormBorderStyle = FormBorderStyle.None;
            BackColor = ColorizedResources.Instance.FrameGradientLight;

            // bitmaps for animation
            progressAnimatedBitmap.Bitmaps = AnimationBitmaps;

            // initialize controls
            string entityName = isPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post);
            Text = FormatFormCaption(entityName, publish);
            ProgressMessage = _defaultProgressMessage = FormatPublishingToCaption(destinationName, entityName, publish);
            checkBoxViewPost.Visible = publish;
            checkBoxViewPost.Checked = PostEditorSettings.ViewPostAfterPublish;

            // hookup event handlers
            checkBoxViewPost.CheckedChanged += new EventHandler(checkBoxViewPost_CheckedChanged);
        }

        /// <summary>
        /// Makes available the name of the plug-in that caused the publish
        /// operation to be canceled
        /// </summary>
        public string CancelReason { get { return _cancelReason; } }

        public event PublishHandler PrePublish;
        public event PublishingHandler Publishing;
        public event PublishHandler PostPublish;

        public delegate void PublishingHandler(object sender, PublishingEventArgs args);
        public class PublishingEventArgs : EventArgs
        {
            public readonly bool Publish;
            public bool RepublishOnSuccess = false;

            public PublishingEventArgs(bool publish)
            {
                Publish = publish;
            }
        }

        public delegate void PublishHandler(object sender, PublishEventArgs args);
        public class PublishEventArgs : EventArgs
        {
            public readonly bool Publish;
            public bool Cancel = false;
            public string CancelReason = null;

            public PublishEventArgs(bool publish)
            {
                Publish = publish;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Update();

            PublishEventArgs args = new PublishEventArgs(_publish);
            if (PrePublish != null)
                PrePublish(this, args);

            if (args.Cancel)
            {
                _cancelReason = args.CancelReason;
                SetDialogResult(DialogResult.Abort);
                return;
            }

            StartUpdate();
        }

        private void StartUpdate()
        {
            PublishingEventArgs args = new PublishingEventArgs(_publish);
            if (Publishing != null)
                Publishing(this, args);

            _republishOnSuccess = args.RepublishOnSuccess;

            // kickoff weblog update
            // Blogger drafts don't have permalinks, therefore, we must do a full publish twice
            bool doPublish = _publish; // && (!_republishOnSuccess || !_publishingContext.Blog.ClientOptions.SupportsPostAsDraft);
            _updateWeblogAsyncOperation = new UpdateWeblogAsyncOperation(new BlogClientUIContextImpl(this), _publishingContext, doPublish);
            _updateWeblogAsyncOperation.Completed += new EventHandler(_updateWeblogAsyncOperation_Completed);
            _updateWeblogAsyncOperation.Cancelled += new EventHandler(_updateWeblogAsyncOperation_Cancelled);
            _updateWeblogAsyncOperation.Failed += new ThreadExceptionEventHandler(_updateWeblogAsyncOperation_Failed);
            _updateWeblogAsyncOperation.ProgressUpdated += new ProgressUpdatedEventHandler(_updateWeblogAsyncOperation_ProgressUpdated);
            _updateWeblogAsyncOperation.Start();
        }

        public Exception Exception
        {
            get
            {
                return _updateWeblogAsyncOperation.Exception;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;

                // add system standard drop shadow
                const int CS_DROPSHADOW = 0x20000;
                createParams.ClassStyle |= CS_DROPSHADOW;

                return createParams;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // fix up layout
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeight(checkBoxViewPost);
            }

            // position form
            RECT parentRect = new RECT();
            User32.GetWindowRect(_parentFrame.Handle, ref parentRect);
            Rectangle parentBounds = RectangleHelper.Convert(parentRect);
            Location = new Point(parentBounds.Left + ((parentBounds.Width - Width) / 2), parentBounds.Top + (int)(1.5 * Height));
        }

        private string FormatFormCaption(string entityName, bool publish)
        {
            return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.UpdateWeblogPublish1), publish ? entityName : Res.Get(StringId.UpdateWeblogDraft));
        }

        private string FormatPublishingToCaption(string destinationName, string entityName, bool publish)
        {
            return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.UpdateWeblogPublish2), publish ? entityName : Res.Get(StringId.UpdateWeblogDraft), destinationName);
        }

        private void _updateWeblogAsyncOperation_Completed(object sender, EventArgs ea)
        {
            Debug.Assert(!InvokeRequired);

            if (_republishOnSuccess)
            {
                _republishOnSuccess = false;
                Debug.Assert(_publishingContext.BlogPost.Id != null);
                StartUpdate();
            }
            else
            {
                if (PostPublish != null)
                    PostPublish(this, new PublishEventArgs(_publish));
                SetDialogResult(DialogResult.OK);
            }
        }

        private void _updateWeblogAsyncOperation_Cancelled(object sender, EventArgs ea)
        {
            Debug.Assert(!InvokeRequired);
            Debug.Fail("Cancel not supported for UpdateWeblogAsyncOperation!");
            SetDialogResult(DialogResult.OK);
        }

        private void _updateWeblogAsyncOperation_Failed(object sender, ThreadExceptionEventArgs e)
        {
            Debug.Assert(!InvokeRequired);
            SetDialogResult(DialogResult.Cancel);
        }

        private void SetDialogResult(DialogResult result)
        {
            _okToClose = true;
            DialogResult = result;
        }
        private bool _okToClose = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_okToClose)
            {
                progressAnimatedBitmap.Stop();
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Override out Activated event to allow parent form to retains its 'activated'
        /// look (caption bar color, etc.) even when we are active
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            // start the animation if necessary (don't start until we are activated so that the
            // loading of the form is not delayed)
            if (!progressAnimatedBitmap.Running)
                progressAnimatedBitmap.Start();
        }

        private void checkBoxViewPost_CheckedChanged(object sender, EventArgs e)
        {
            PostEditorSettings.ViewPostAfterPublish = checkBoxViewPost.Checked;
        }

        private Bitmap[] AnimationBitmaps
        {
            get
            {
                if (_animationBitmaps == null)
                {
                    ArrayList list = new ArrayList();
                    for (int i = 1; i <= 26; i++)
                    {
                        string resourceName = String.Format(CultureInfo.InvariantCulture, "Images.PublishAnimation.post{0:00}.png", i);
                        list.Add(ResourceHelper.LoadAssemblyResourceBitmap(resourceName));
                    }
                    _animationBitmaps = (Bitmap[])list.ToArray(typeof(Bitmap));
                }
                return _animationBitmaps;
            }
        }
        private Bitmap[] _animationBitmaps;

        private Bitmap bottomBevelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PublishAnimation.BottomBevel.png");

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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxViewPost = new System.Windows.Forms.CheckBox();
            this.progressAnimatedBitmap = new OpenLiveWriter.Controls.AnimatedBitmapControl();
            this.labelPublishingTo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // checkBoxViewPost
            //
            this.checkBoxViewPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxViewPost.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxViewPost.Location = new System.Drawing.Point(19, 136);
            this.checkBoxViewPost.Name = "checkBoxViewPost";
            this.checkBoxViewPost.Size = new System.Drawing.Size(325, 18);
            this.checkBoxViewPost.TabIndex = 1;
            this.checkBoxViewPost.Text = "View in browser after publishing";
            this.checkBoxViewPost.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // progressAnimatedBitmap
            //
            this.progressAnimatedBitmap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.progressAnimatedBitmap.Bitmaps = null;
            this.progressAnimatedBitmap.Interval = 100;
            this.progressAnimatedBitmap.Location = new System.Drawing.Point(19, 25);
            this.progressAnimatedBitmap.Name = "progressAnimatedBitmap";
            this.progressAnimatedBitmap.Running = false;
            this.progressAnimatedBitmap.Size = new System.Drawing.Size(321, 71);
            this.progressAnimatedBitmap.TabIndex = 2;
            this.progressAnimatedBitmap.UseVirtualTransparency = false;
            //
            // labelPublishingTo
            //
            this.labelPublishingTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPublishingTo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPublishingTo.Location = new System.Drawing.Point(19, 105);
            this.labelPublishingTo.Name = "labelPublishingTo";
            this.labelPublishingTo.Size = new System.Drawing.Size(317, 18);
            this.labelPublishingTo.TabIndex = 3;
            this.labelPublishingTo.Text = "Publishing to: My Random Ramblings";
            this.labelPublishingTo.UseMnemonic = false;
            this.labelPublishingTo.Visible = false;
            //
            // UpdateWeblogProgressForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(360, 164);
            this.ControlBox = false;
            this.Controls.Add(this.labelPublishingTo);
            this.Controls.Add(this.progressAnimatedBitmap);
            this.Controls.Add(this.checkBoxViewPost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateWeblogProgressForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Publishing {0} to Weblog";
            this.ResumeLayout(false);

        }
        #endregion

        private void _updateWeblogAsyncOperation_ProgressUpdated(object sender, ProgressUpdatedEventArgs progressUpdatedHandler)
        {
            string msg = progressUpdatedHandler.ProgressMessage;
            if (msg != null)
                ProgressMessage = msg;
        }

        private string _progressMessage;
        private string ProgressMessage
        {
            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, ClientSize, false);

            ColorizedResources colRes = ColorizedResources.Instance;

            // draw the outer border
            using (Pen p = new Pen(colRes.BorderDarkColor, 1))
                g.DrawRectangle(p, new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1));

            // draw the caption
            using (Font f = Res.GetFont(FontSize.Large, FontStyle.Bold))
                g.DrawText(Text, f, new Rectangle(19, 8, ClientSize.Width - 1, ClientSize.Height - 1), SystemColors.WindowText, TextFormatFlags.NoPrefix);

            GdiTextHelper.DrawString(this, labelPublishingTo.Font, _progressMessage, labelPublishingTo.Bounds, false, GdiTextDrawMode.EndEllipsis);
        }

        public void SetProgressMessage(string msg)
        {
            ProgressMessage = msg ?? _defaultProgressMessage;
        }
    }

}

