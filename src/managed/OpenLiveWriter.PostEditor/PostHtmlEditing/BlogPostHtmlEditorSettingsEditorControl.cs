// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices.Progress;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageServiceSettingsEditorControl.
    /// </summary>
    public class BlogPostHtmlEditorSettingsEditorControl : UserControl
    {
        private Button buttonLoadTemplate;
        private Label label2;
        private Label label1;
        private TextBox textBoxTemplate;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public BlogPostHtmlEditorSettingsEditorControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonLoadTemplate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTemplate = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // buttonLoadTemplate
            //
            this.buttonLoadTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoadTemplate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonLoadTemplate.Location = new System.Drawing.Point(232, 196);
            this.buttonLoadTemplate.Name = "buttonLoadTemplate";
            this.buttonLoadTemplate.Size = new System.Drawing.Size(112, 23);
            this.buttonLoadTemplate.TabIndex = 19;
            this.buttonLoadTemplate.Text = "Download template";
            this.buttonLoadTemplate.Click += new System.EventHandler(this.buttonLoadTemplate_Click);
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 16);
            this.label2.TabIndex = 20;
            this.label2.Text = "Template:";
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(0, 160);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(344, 48);
            this.label1.TabIndex = 23;
            this.label1.Text = "Download an editing template based on your weblog.  This will post a temporary it" +
                "em to your blog that can be used to detect the editing styles for items posted t" +
                "o your blog.";
            //
            // textBoxTemplate
            //
            this.textBoxTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTemplate.Location = new System.Drawing.Point(0, 16);
            this.textBoxTemplate.Multiline = true;
            this.textBoxTemplate.Name = "textBoxTemplate";
            this.textBoxTemplate.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxTemplate.Size = new System.Drawing.Size(344, 136);
            this.textBoxTemplate.TabIndex = 24;
            this.textBoxTemplate.Text = "";
            this.textBoxTemplate.TextChanged += new System.EventHandler(this.textBoxTemplate_TextChanged);
            //
            // BlogPostHtmlEditorSettingsEditorControl
            //
            this.Controls.Add(this.textBoxTemplate);
            this.Controls.Add(this.buttonLoadTemplate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "BlogPostHtmlEditorSettingsEditorControl";
            this.Size = new System.Drawing.Size(344, 252);
            this.ResumeLayout(false);

        }
        #endregion

        public event EventHandler SettingsChanged;
        public void OnSettingsChanged(EventArgs evt)
        {
            if(SettingsChanged != null)
                SettingsChanged(this, evt);
        }

        internal void LoadSettings(BlogSettings settings)
        {
            //blog template
            _id = settings.Id;
            using(PostHtmlEditingSettings blogTemplate = new PostHtmlEditingSettings(settings.Id))
            {
                _templateFile = blogTemplate.EditorTemplateHtmlFile;
                textBoxTemplate.Text = blogTemplate.EditorTemplateHtml;
            }
        }

        public void ApplySettings(BlogSettings settings)
        {
            //blog template
            using(PostHtmlEditingSettings editingSettings = new PostHtmlEditingSettings(settings.Id))
            {
                editingSettings.EditorTemplateHtmlFile = _templateFile;
                editingSettings.EditorTemplateHtml = textBoxTemplate.Text;
            }
        }

        private string _id;
        private string _templateFile;
        private void buttonLoadTemplate_Click(object sender, EventArgs e)
        {
            BlogSettings settings = BlogSettings.ForBlogId(_id);

            try
            {
                _templateFile = BlogTemplateDetector.DetectTemplate(this, settings ) ;

                using(TextReader reader = new StreamReader(_templateFile, Encoding.UTF8))
                    textBoxTemplate.Text = reader.ReadToEnd();
            }
            catch(OperationCancelledException)
            {
                //user cancelled call.
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message ) ;
            }

        }

        private void textBoxTemplate_TextChanged(object sender, EventArgs e)
        {
            OnSettingsChanged(EventArgs.Empty) ;
        }

        private void cbAllowMaximizedEditing_CheckedChanged(object sender, EventArgs e)
        {
            OnSettingsChanged(EventArgs.Empty) ;
        }
    }
}
