// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls
{

    /// <summary>
    /// Summary description for ProgressDialog.
    /// </summary>
    public class ProgressDialog : BaseForm
    {
        public ProgressDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.cancelButton.Text = Res.Get(StringId.CancelButton);

            // This will flip the direction of progress bar if we are RTL
            progressBar.RightToLeftLayout = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (m_progressProviderResult != DialogResult.None)
            {
                //the progress operation has already completed, so don't show the dialog.
                this.DialogResult = m_progressProviderResult;
                Close();
            }
            else
            {
                base.OnLoad(e);
                DisplayHelper.AutoFitSystemButton(cancelButton, cancelButton.Width, int.MaxValue);
            }
        }

        /// <summary>
        /// The title displayed for this dialog
        /// </summary>
        public string Title
        {
            get
            {
                return Text;
            }
            set
            {
                AccessibleName = Text = value;
            }
        }

        /// <summary>
        /// Makes the capture button visible or invisible
        /// </summary>
        public bool CancelButtonVisible
        {
            get
            {
                return this.cancelButton.Visible;
            }
            set
            {
                this.cancelButton.Visible = value;
            }
        }

        /// <summary>
        /// The text displayed on the cancel button
        /// </summary>
        public StringId CancelButtonText
        {
            set
            {
                cancelButton.Text = Res.Get(value);
            }
        }

        /// <summary>
        /// The text displayed in the progress bar.
        /// </summary>
        public string ProgressText
        {
            get
            {
                return this.progressLabel.Text;
            }
            set
            {
                this.progressLabel.Text = value;
                progressBar.AccessibleName = ControlHelper.ToAccessibleName(value);
            }
        }

        /// <summary>
        /// The AsyncOperation that this dialog is bound to.
        /// Progress reported by this AsynOperation will be reflected in the progress bar.
        /// </summary>
        public IProgressProvider ProgressProvider
        {
            get
            {
                return m_progressProvider;
            }
            set
            {
                m_progressProvider = value;
                ConfigureCategoryUI();
                HookEvents();
            }
        }

        public bool IgnoreProgressMessages = false;

        /// <summary>
        /// Invoked when the operation has made progress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progressUpdatedEvt"></param>
        private void OnProgressUpdated(object sender, ProgressUpdatedEventArgs progressUpdatedEvt)
        {
            if (!IgnoreProgressMessages)
                this.progressLabel.Text = progressUpdatedEvt.ProgressMessage;
            this.progressBar.Maximum = progressUpdatedEvt.Total;
            this.progressBar.Value = progressUpdatedEvt.Completed;
        }

        /// <summary>
        /// Handles user clicking the cancel button
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            progressLabel.Text = Res.Get(StringId.Canceling);
            ProgressProvider.Cancel();
        }

        /// <summary>
        /// Cleans up when the AsycOperation has been cancelled.
        /// </summary>
        private void asyncOp_Cancelled(object sender, EventArgs e)
        {
            // reset progress
            progressBar.Value = 0;
            progressLabel.Text = string.Empty;

            UnHookEvents();
            this.DialogResult = DialogResult.Cancel;
            m_progressProviderResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Cleans up after the AsynOperation has completed its work.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void asyncOp_Completed(object sender, EventArgs e)
        {
            UnHookEvents();
            this.DialogResult = DialogResult.OK;
            m_progressProviderResult = DialogResult.OK;
        }

        /// <summary>
        /// The operation has failed
        /// </summary>
        private void asyncOp_Failed(object sender, ThreadExceptionEventArgs e)
        {
            UnHookEvents();
            this.DialogResult = DialogResult.Abort;
            m_progressProviderResult = DialogResult.Abort;
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

        private void ConfigureCategoryUI()
        {
            // if this progress provider has categories then allow this UI to show
            // on the form -- otherwise hide it and shrink the form as appropriate
            m_progressCategoryProvider = m_progressProvider as IProgressCategoryProvider;
            if (m_progressCategoryProvider != null && m_progressCategoryProvider.ShowCategories)
            {
                // Signup for the category changed event
                m_progressCategoryProvider.ProgressCategoryChanged += new EventHandler(m_progressCategoryProvider_ProgressCategoryChanged);
            }
            else // hide the category ui
            {
                panelCategory.Visible = false;
                ClientSize = new Size(ClientSize.Width, ClientSize.Height - panelCategory.Height);
            }
        }

        /// <summary>
        /// Synch the category ui w/ the category provider
        /// </summary>
        private void m_progressCategoryProvider_ProgressCategoryChanged(object sender, EventArgs e)
        {
            pictureBoxCategory.Image = m_progressCategoryProvider.CurrentCategory.Icon;
            labelCategory.Text = m_progressCategoryProvider.CurrentCategory.Name;
        }

        private void HookEvents()
        {
            //Hook up this dialog's event handler's to the AsyncOperation events
            m_progressProvider.ProgressUpdated += new ProgressUpdatedEventHandler(OnProgressUpdated);
            m_progressProvider.Cancelled += new EventHandler(asyncOp_Cancelled);
            m_progressProvider.Completed += new EventHandler(asyncOp_Completed);
            m_progressProvider.Failed += new ThreadExceptionEventHandler(asyncOp_Failed);
        }

        private void UnHookEvents()
        {
            //Hook up this dialog's event handler's to the AsyncOperation events
            m_progressProvider.ProgressUpdated -= new ProgressUpdatedEventHandler(OnProgressUpdated);
            m_progressProvider.Cancelled -= new EventHandler(asyncOp_Cancelled);
            m_progressProvider.Completed -= new EventHandler(asyncOp_Completed);
            m_progressProvider.Failed -= new ThreadExceptionEventHandler(asyncOp_Failed);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cancelButton = new System.Windows.Forms.Button();
            this.progressBar = new ProgressBar();
            this.progressLabel = new OpenLiveWriter.Controls.LabelControl();
            this.panelCategory = new System.Windows.Forms.Panel();
            this.labelCategory = new System.Windows.Forms.Label();
            this.pictureBoxCategory = new System.Windows.Forms.PictureBox();
            this.panelCategory.SuspendLayout();
            this.SuspendLayout();
            //
            // cancelButton
            //
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(237, 83);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar.Location = new System.Drawing.Point(10, 56);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(304, 19);
            this.progressBar.TabIndex = 0;
            //
            // progressLabel
            //
            this.progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressLabel.Location = new System.Drawing.Point(10, 38);
            this.progressLabel.MultiLine = false;
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(292, 16);
            this.progressLabel.TabIndex = 2;
            //
            // panelCategory
            //
            this.panelCategory.Controls.Add(this.labelCategory);
            this.panelCategory.Controls.Add(this.pictureBoxCategory);
            this.panelCategory.Location = new System.Drawing.Point(10, 13);
            this.panelCategory.Name = "panelCategory";
            this.panelCategory.Size = new System.Drawing.Size(300, 24);
            this.panelCategory.TabIndex = 3;
            //
            // labelCategory
            //
            this.labelCategory.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCategory.Location = new System.Drawing.Point(22, 1);
            this.labelCategory.Name = "labelCategory";
            this.labelCategory.Size = new System.Drawing.Size(278, 16);
            this.labelCategory.TabIndex = 1;
            //
            // pictureBoxCategory
            //
            this.pictureBoxCategory.Location = new System.Drawing.Point(1, 0);
            this.pictureBoxCategory.Name = "pictureBoxCategory";
            this.pictureBoxCategory.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxCategory.TabIndex = 0;
            this.pictureBoxCategory.TabStop = false;
            //
            // ProgressDialog
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(324, 113);
            this.ControlBox = false;
            this.Controls.Add(this.panelCategory);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Progress Title";
            this.panelCategory.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The progress provider for this progress dialog
        /// </summary>
        private IProgressProvider m_progressProvider;
        private IProgressCategoryProvider m_progressCategoryProvider;
        DialogResult m_progressProviderResult = DialogResult.None;

        private Button cancelButton;
        private ProgressBar progressBar;
        private LabelControl progressLabel;
        private Panel panelCategory;
        private PictureBox pictureBoxCategory;
        private Label labelCategory;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

    }
}
