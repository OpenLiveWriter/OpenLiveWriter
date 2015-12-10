// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.PostEditor.Configuration;
using OpenLiveWriter.Api.BlogClient ;

namespace OpenLiveWriter
{
    /// <summary>
    /// Summary description for TestAutoDetection.
    /// </summary>
    public class TestAutoDetection : System.Windows.Forms.Form
    {

        [STAThread]
        public static void Main(string[] args )
        {
            ApplicationEnvironment.Initialize();
            Application.Run( new TestAutoDetection() ) ;
        }

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxResults;
        private System.Windows.Forms.Button buttonAutoDetectWeblog;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUsername;
        private OpenLiveWriter.PostEditor.Configuration.WeblogHomepageUrlControl weblogHomepageUrlControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public TestAutoDetection()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            textBoxPassword.PasswordChar = (char) 0x25cf;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            textBoxUsername.Text = settings.GetString("Username", String.Empty) ;
            textBoxPassword.Text = settings.GetString("Password", String.Empty );
            weblogHomepageUrlControl.HomepageUrl = settings.GetString("WeblogUrl", String.Empty);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing (e);

            settings.SetString( "Username", textBoxUsername.Text );
            settings.SetString( "Password", textBoxPassword.Text );
            settings.SetString( "WeblogUrl", weblogHomepageUrlControl.HomepageUrl);
        }

        private SettingsPersisterHelper settings = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("AutoDetectionTest");

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.weblogHomepageUrlControl = new OpenLiveWriter.PostEditor.Configuration.WeblogHomepageUrlControl();
            this.buttonAutoDetectWeblog = new System.Windows.Forms.Button();
            this.textBoxResults = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(16, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(288, 40);
            this.label2.TabIndex = 18;
            this.label2.Text = "Please enter the URL of your Weblog\'s home page and the username and password tha" +
                "t you use to log in to your Weblog.";
            //
            // label3
            //
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Location = new System.Drawing.Point(168, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "&Password:";
            //
            // textBoxPassword
            //
            this.textBoxPassword.Location = new System.Drawing.Point(168, 128);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(136, 20);
            this.textBoxPassword.TabIndex = 17;
            this.textBoxPassword.Text = "";
            //
            // textBoxUsername
            //
            this.textBoxUsername.Location = new System.Drawing.Point(16, 128);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(137, 20);
            this.textBoxUsername.TabIndex = 15;
            this.textBoxUsername.Text = "";
            //
            // label4
            //
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label4.Location = new System.Drawing.Point(16, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "&Username:";
            //
            // weblogHomepageUrlControl
            //
            this.weblogHomepageUrlControl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.weblogHomepageUrlControl.HomepageUrl = "";
            this.weblogHomepageUrlControl.Location = new System.Drawing.Point(16, 64);
            this.weblogHomepageUrlControl.Name = "weblogHomepageUrlControl";
            this.weblogHomepageUrlControl.Size = new System.Drawing.Size(288, 37);
            this.weblogHomepageUrlControl.TabIndex = 13;
            //
            // buttonAutoDetectWeblog
            //
            this.buttonAutoDetectWeblog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAutoDetectWeblog.Location = new System.Drawing.Point(88, 168);
            this.buttonAutoDetectWeblog.Name = "buttonAutoDetectWeblog";
            this.buttonAutoDetectWeblog.Size = new System.Drawing.Size(144, 23);
            this.buttonAutoDetectWeblog.TabIndex = 19;
            this.buttonAutoDetectWeblog.Text = "Auto Detect Weblog...";
            this.buttonAutoDetectWeblog.Click += new System.EventHandler(this.buttonAutoDetectWeblog_Click);
            //
            // textBoxResults
            //
            this.textBoxResults.Location = new System.Drawing.Point(16, 208);
            this.textBoxResults.Multiline = true;
            this.textBoxResults.Name = "textBoxResults";
            this.textBoxResults.Size = new System.Drawing.Size(288, 168);
            this.textBoxResults.TabIndex = 20;
            this.textBoxResults.Text = "";
            //
            // TestAutoDetection
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(320, 398);
            this.Controls.Add(this.textBoxResults);
            this.Controls.Add(this.buttonAutoDetectWeblog);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.weblogHomepageUrlControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TestAutoDetection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TestAutoDetection";
            this.ResumeLayout(false);

        }
        #endregion

        private void buttonAutoDetectWeblog_Click(object sender, System.EventArgs e)
        {
            try
            {
                textBoxResults.Text = String.Empty ;

                BlogAccountDetector accountDetector = new BlogAccountDetector(this, this.weblogHomepageUrlControl.HomepageUrl, this.textBoxUsername.Text, this.textBoxPassword.Text);

                // setup the progress dialog and kick off the transfer
                using (ProgressDialog progress = new ProgressDialog())
                {
                    // configure progress source
                    progress.ProgressProvider = accountDetector;

                    // set progress title
                    progress.Title = Text;
                    progress.ProgressText = Text ;

                    // start the publisher (this is a non-blocking call)
                    accountDetector.Start();

                    // show the progress dialog
                    progress.ShowDialog(this);
                }

                // show error
                if ( accountDetector.ErrorOccurred )
                {
                    accountDetector.ShowLastError(this);
                }
                else if (!accountDetector.WasCancelled)// ran to completion
                {
                    StringBuilder resultsBuilder = new StringBuilder();
                    resultsBuilder.AppendFormat( "Service: {0}\r\nClientApi: {1}\r\nPost URL: {2}\r\nBlogID: {3}\r\n\r\n",
                        accountDetector.ServiceName,
                        accountDetector.ClientType,
                        accountDetector.PostApiUrl,
                        accountDetector.BlogId ) ;

                    foreach ( BlogInfo blog in accountDetector.UsersBlogs )
                        resultsBuilder.AppendFormat( "{0} ({1})\r\n", blog.HomepageUrl, blog.Id ) ;

                    textBoxResults.Text = resultsBuilder.ToString() ;
                }
            }
            catch(Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }
    }
}
