// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Maps;

namespace OpenLiveWriter
{
    /// <summary>
    /// Summary description for TestMainForm.
    /// </summary>
    public class TestMainForm : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public TestMainForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            MapControl mapControl = new MapControl();
            mapControl.Dock = DockStyle.Fill;
            mapControl.LoadMap("48 Stone Ave., Somerville, MA");
            this.Controls.Add(mapControl);
        }

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
            this.components = new System.ComponentModel.Container();
            this.Size = new System.Drawing.Size(300,300);
            this.Text = "TestMainForm";
        }
        #endregion

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // initialize application environment
                ApplicationEnvironment.Initialize();

                //RsdServiceDescription rsdService = RsdServiceDetector.DetectFromRsdUrl("http://localhost/test/foo.rsd", 10000);
                //Trace.WriteLine(rsdService.EditingTemplateLink);

                Application.Run(new TestMainForm());

                // launch blogging form
                //WeblogConfigurationWizardController.Add(Win32WindowImpl.DesktopWin32Window);
                //WeblogConfigurationWizardController.Edit(Win32WindowImpl.DesktopWin32Window, BlogSettings.DefaultBlogId);
            }
            catch( Exception ex )
            {
                UnexpectedErrorMessage.Show( ex ) ;
            }
        }
    }
}
