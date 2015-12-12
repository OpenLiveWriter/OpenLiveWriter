// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// Dialog that is displayed when an unhandled exception occurs
    /// </summary>
    public class UnexpectedErrorDialog : BaseForm
    {
        private Panel panel1;
        private Label labelTitle;
        private Label labelMessage;
        private LinkLabel labelClickHere;
        private Button buttonExit;
        private Button buttonContinue;
        private System.Windows.Forms.Label labelDescription;
        private Button buttonSendError;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        public UnexpectedErrorDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            labelTitle.Font = Res.GetFont(FontSize.Large, FontStyle.Bold);
            buttonExit.Text = Res.Get(StringId.UnexpectedErrorExit);
            buttonContinue.Text = Res.Get(StringId.UnexpectedErrorContinue);
            buttonSendError.Text = Res.Get(StringId.UnexpectedErrorSendError);
            labelClickHere.Text = Res.Get(StringId.UnexpectedErrorClickHere);
            labelDescription.Text = Res.Get(StringId.UnexpectedErrorDescription);
            Text = Res.Get(StringId.UnexpectedErrorTitle);

            Icon = ApplicationEnvironment.ProductIcon;
        }

        protected override void OnLoad(EventArgs e)
        {
            Form owner = this.Owner;
            if (owner != null)
                this.TopMost = owner.TopMost;

            base.OnLoad(e);

            DisplayHelper.AutoFitSystemButton(buttonContinue, buttonContinue.Width, int.MaxValue);
            DisplayHelper.AutoFitSystemButton(buttonExit, buttonExit.Width, int.MaxValue);
        }

        /// <summary>
        /// Creates a new dialog
        /// </summary>
        /// <param name="title">The title to display</param>
        /// <param name="text">The text to display</param>
        /// <param name="rootCause">The Exception that is the root cause</param>
        public UnexpectedErrorDialog(string title, string text, Exception rootCause) : this()
        {
            labelTitle.Text = title;
            labelMessage.Text = text;

            m_rootCause = rootCause;
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.labelClickHere = new System.Windows.Forms.LinkLabel();
            this.labelDescription = new System.Windows.Forms.Label();
            this.buttonSendError = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // panel1
            //
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.labelTitle);
            this.panel1.Location = new System.Drawing.Point(-1, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(399, 71);
            this.panel1.TabIndex = 0;
            //
            // labelTitle
            //
            this.labelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelTitle.Location = new System.Drawing.Point(17, 27);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(390, 27);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "Title goes here.";
            //
            // labelMessage
            //
            this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelMessage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMessage.Location = new System.Drawing.Point(41, 82);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(309, 27);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.Text = "Message goes here.";
            //
            // buttonExit
            //
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonExit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExit.Location = new System.Drawing.Point(-35, 211);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(77, 26);
            this.buttonExit.TabIndex = 4;
            this.buttonExit.Text = "Exit";
            this.buttonExit.Visible = false;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            //
            // buttonContinue
            //
            this.buttonContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonContinue.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonContinue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonContinue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonContinue.Location = new System.Drawing.Point(293, 211);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(90, 26);
            this.buttonContinue.TabIndex = 5;
            this.buttonContinue.Text = "&Continue";
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            //
            // labelClickHere
            //
            this.labelClickHere.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelClickHere.AutoSize = true;
            this.labelClickHere.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelClickHere.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelClickHere.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelClickHere.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.labelClickHere.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.labelClickHere.Location = new System.Drawing.Point(41, 159);
            this.labelClickHere.Name = "labelClickHere";
            this.labelClickHere.Size = new System.Drawing.Size(158, 15);
            this.labelClickHere.TabIndex = 7;
            this.labelClickHere.TabStop = true;
            this.labelClickHere.Text = "Click here to see error details";
            this.labelClickHere.Click += new System.EventHandler(this.labelClickHere_Click);
            //
            // labelDescription
            //
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDescription.Location = new System.Drawing.Point(41, 119);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(383, 34);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "We have created an error summary that you can view.";
            //
            // buttonSendError
            //
            this.buttonSendError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendError.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSendError.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSendError.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSendError.Location = new System.Drawing.Point(41, 211);
            this.buttonSendError.Name = "buttonSendError";
            this.buttonSendError.Size = new System.Drawing.Size(90, 26);
            this.buttonSendError.TabIndex = 8;
            this.buttonSendError.Text = "&Send Error";
            this.buttonSendError.Click += new System.EventHandler(this.buttonSendError_Click);
            //
            // UnexpectedErrorDialog
            //
            this.ClientSize = new System.Drawing.Size(390, 249);
            this.ControlBox = false;
            this.Controls.Add(this.buttonSendError);
            this.Controls.Add(this.labelClickHere);
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnexpectedErrorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Unexpected Error";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSendError_Click(object sender, EventArgs e)
        {
            HockeyAppProxy.Current.UploadExceptionAsync(m_rootCause);
            Close();
        }

        private void labelClickHere_Click(object sender, EventArgs e)
        {
            try
            {
                Process p = new Process();
                ProcessStartInfo psi = new ProcessStartInfo(DiagnosticsFilePath);
                psi.UseShellExecute = true;
                p.StartInfo = psi;
                p.Start();
            }
            catch (Exception ex)
            {
                Trace.Fail("Unable to report error.\r\n\r\n" + ex.Message);
            }
        }

        private string DiagnosticsFilePath
        {
            get
            {
                if (m_diagnosticsFilePath == null)
                {
                    string fileName = "ErrorReport.txt";
                    string filePath = Path.Combine(Path.GetTempPath(), fileName);
                    int i = 1;
                    while (File.Exists(filePath))
                    {
                        string newfileName = Path.GetFileNameWithoutExtension(fileName) + i + Path.GetExtension(fileName);
                        filePath = Path.Combine(Path.GetTempPath(), newfileName);
                        i++;
                    }
                    m_diagnosticsFilePath = filePath;

                    using (StreamWriter writer = new StreamWriter(File.Create(m_diagnosticsFilePath)))
                    {
                        writer.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
                        writer.WriteLine();
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Version: {0}", ApplicationEnvironment.ProductVersion));
                        writer.WriteLine();
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "OS Version: {0}", Environment.OSVersion));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Runtime version: {0}", Environment.Version));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Shutdown started: {0}", Environment.HasShutdownStarted));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Program: {0}", ReadExecutable()));
                        writer.WriteLine();
                        WriteMemoryStatus(writer);
                        writer.WriteLine();
                        writer.WriteLine(labelTitle.Text);
                        writer.WriteLine(labelMessage.Text);
                        writer.WriteLine();
                        WriteException(m_rootCause, writer);
                    }
                }
                return m_diagnosticsFilePath;

            }
        }
        private string m_diagnosticsFilePath = null;

        private static void WriteMemoryStatus(TextWriter writer)
        {
            try
            {
                GlobalMemoryStatus memoryStatus = new GlobalMemoryStatus();
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Memory Load: {0}%", memoryStatus.MemoryLoad));
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Total Physical: {0} MB", (memoryStatus.TotalPhysical / 1048576)));
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Available Physical: {0} MB", (memoryStatus.AvailablePhysical / 1048576)));
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Total Page File: {0} MB", (memoryStatus.TotalPageFile / 1048576)));
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Available Page File: {0} MB", (memoryStatus.AvailablePageFile / 1048576)));
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Total Virtual: {0} MB", (memoryStatus.TotalVirtual / 1048576)));
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Available Virtual: {0} MB", (memoryStatus.AvailableVirtual / 1048576)));
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Available Extented Virtual: {0} MB", (memoryStatus.AvailableExtendedVirtual / 1048576)));
            }
            catch (Exception ex)
            {
                Debug.Write("Unexpected exception attempting to get global memory status: " + ex.ToString());
            }
        }

        private static string ReadExecutable()
        {
            string executable = string.Empty;
            try
            {
                executable = Environment.CommandLine;
            }
            catch { }
            return executable;
        }

        private void WriteException(Exception ex, StreamWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine(ex.ToString());

            if (ex.InnerException != null && (m_exceptionDepth++ < MAX_EXCEPTION_DEPTH))
            {
                WriteException(ex.InnerException, writer);
            }
        }
        private int m_exceptionDepth = 0;
        private static readonly int MAX_EXCEPTION_DEPTH = 25;

        private readonly Exception m_rootCause;
    }

}
