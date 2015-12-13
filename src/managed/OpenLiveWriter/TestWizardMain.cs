// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.Api.BlogClient;
using OpenLiveWriter.PostEditor.Configuration.Wizard ;

namespace OpenLiveWriter
{
    /// <summary>
    /// Summary description for TestWizardMain.
    /// </summary>
    public class TestWizardMain
    {
        private class TestForm : Form
        {

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);


            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick (e);

                try
                {
                    BlogEditingTemplateDetector detector = new BlogEditingTemplateDetector(this);
                    detector.SetContext( "http://localhost/test/editingTemplate.htm", @"C:\Program Files\Apache Group\Apache\htdocs\test\blogtemplates" );
                    detector.DetectTemplate(SilentProgressHost.Instance) ;

                    Trace.WriteLine(detector.BlogTemplateFile);
                }
                catch(Exception ex)
                {
                    UnexpectedErrorMessage.Show( ex ) ;
                }
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // initialize application environment
                ApplicationEnvironment.Initialize();

                //RsdServiceDescription rsdService = RsdServiceDetector.DetectFromRsdUrl("http://localhost/test/foo.rsd", 10000);
                //Trace.WriteLine(rsdService.EditingTemplateLink);

                Application.Run(new TestForm());

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
