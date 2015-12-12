// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{
    /// <summary>
    /// Summary description for AdvancedPanel.
    /// </summary>
    public class AdvancedPanel : WeblogSettingsPanel
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxTransportEncoding;
        private System.Windows.Forms.ComboBox comboEncoding;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private Kernel32.CodePageDelegate m_codePageDelegate;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.Label lblScripts;
        private System.Windows.Forms.Label lblEmbeds;
        private System.Windows.Forms.ComboBox comboScripts;
        private System.Windows.Forms.ComboBox comboEmbeds;
        private System.Windows.Forms.GroupBox groupBoxXHTML;
        private System.Windows.Forms.Label labelXHTML;
        private System.Windows.Forms.ComboBox comboXHTML;
        private ArrayList codepages = new ArrayList();

        private SupportsFeature clientSupportsScripts;
        private SupportsFeature clientSupportsEmbeds;

        public AdvancedPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            UpdateStrings();
            // TODO: Add any initialization after the InitializeComponent call

        }

        public AdvancedPanel(TemporaryBlogSettings targetBlogSettings, TemporaryBlogSettings editableBlogSettings)
            : base(targetBlogSettings, editableBlogSettings)
        {
            InitializeComponent();
            UpdateStrings();
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Configuration.Settings.Images.AdvancedPanelBitmap.png");

            //gets all the system encodings
            m_codePageDelegate = new Kernel32.CodePageDelegate(this.CodePageProc);
            if (!Kernel32.EnumSystemCodePages(m_codePageDelegate, Kernel32.CP_SUPPORTED))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            SetValues();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                LayoutHelper.FixupGroupBox(groupBoxTransportEncoding);
                LayoutHelper.FixupGroupBox(groupBoxXHTML);
                LayoutHelper.DistributeVertically((int)DisplayHelper.ScaleY(8), groupBoxTransportEncoding, groupBoxXHTML, groupBoxOptions);
                int delta = LayoutHelper.AutoFitLabels(lblEmbeds, lblScripts);
                comboEmbeds.Left += delta;
                comboEmbeds.Width -= delta;
                comboScripts.Left += delta;
                comboScripts.Width -= delta;
            }
        }

        private void UpdateStrings()
        {
            groupBoxTransportEncoding.Text = Res.Get(StringId.AdvancedTransport);
            label1.Text = Res.Get(StringId.AdvancedText);
            PanelName = Res.Get(StringId.AdvancedName);
            groupBoxXHTML.Text = Res.Get(StringId.AdvancedXHTMLGroupName);
            labelXHTML.Text = Res.Get(StringId.AdvancedXHTMLLabel);
            groupBoxOptions.Text = Res.Get(StringId.AdvancedBlogOverride);
            lblEmbeds.Text = Res.Get(StringId.AdvancedEmbedTags);
            lblScripts.Text = Res.Get(StringId.AdvancedScripts);
        }

        private void SetValues()
        {
            SetEncodingValues();
            SetXHTMLValues();
            SetSupportValues();
        }

        private void SetEncodingValues()
        {
            string defaultEncoding = Res.Get(StringId.AdvancedDefault); //"Default ({0})"
            string blogOverride = (string)TemporaryBlogSettings.OptionOverrides[BlogClientOptions.CHARACTER_SET];
            string userOverride = (string)TemporaryBlogSettings.UserOptionOverrides[BlogClientOptions.CHARACTER_SET];
            string homepageOverride = (string)TemporaryBlogSettings.HomePageOverrides[BlogClientOptions.CHARACTER_SET];

            if (!String.IsNullOrEmpty(blogOverride))
                defaultEncoding = String.Format(CultureInfo.CurrentCulture, defaultEncoding, blogOverride);
            else if (!String.IsNullOrEmpty(homepageOverride))
                defaultEncoding = String.Format(CultureInfo.CurrentCulture, defaultEncoding, homepageOverride);
            else
                defaultEncoding = String.Format(CultureInfo.CurrentCulture, defaultEncoding, "UTF-8");

            comboEncoding.Items.Add(new OptionItem(defaultEncoding, null));
            codepages.Sort(new EncodingComparer());
            foreach (Encoding codeName in codepages)
            {
                if (codeName.IsBrowserDisplay)
                    comboEncoding.Items.Add(new OptionItem(codeName.EncodingName + ": " + codeName.WebName, codeName));
            }

            if (userOverride != null && userOverride != String.Empty)
            {
                Encoding setEncoding = Encoding.GetEncoding(userOverride);
                comboEncoding.SelectedItem = new OptionItem(setEncoding.EncodingName + ": " + setEncoding.WebName, setEncoding);
            }
            else
                comboEncoding.SelectedIndex = 0;
        }

        private void SetSupportValues()
        {
            using (Blog blog = new Blog(TemporaryBlogSettings))
            {
                clientSupportsScripts = blog.ClientOptions.SupportsScripts;
                clientSupportsEmbeds = blog.ClientOptions.SupportsEmbeds;
            }
            //scripts
            comboScripts.Items.Add(new SupportsOptionItem(SupportsFeature.Yes));
            comboScripts.Items.Add(new SupportsOptionItem(SupportsFeature.Unknown));
            comboScripts.Items.Add(new SupportsOptionItem(SupportsFeature.No));
            string userScriptOverride = (string)TemporaryBlogSettings.UserOptionOverrides[BlogClientOptions.SUPPORTS_SCRIPTS];
            if (userScriptOverride != null && userScriptOverride != String.Empty)
            {
                switch (userScriptOverride)
                {
                    case "yes":
                        {
                            comboScripts.SelectedItem = new SupportsOptionItem(SupportsFeature.Yes);
                            break;
                        }
                    case "no":
                        {
                            comboScripts.SelectedItem = new SupportsOptionItem(SupportsFeature.No);
                            break;
                        }
                    default:
                        {
                            comboScripts.SelectedItem = new SupportsOptionItem(SupportsFeature.Unknown);
                            break;
                        }
                }
            }
            else
            {
                comboScripts.SelectedItem = new SupportsOptionItem(clientSupportsScripts);
            }

            //embeds
            comboEmbeds.Items.Add(new SupportsOptionItem(SupportsFeature.Yes));
            comboEmbeds.Items.Add(new SupportsOptionItem(SupportsFeature.Unknown));
            comboEmbeds.Items.Add(new SupportsOptionItem(SupportsFeature.No));
            string userEmbedOverride = (string)TemporaryBlogSettings.UserOptionOverrides[BlogClientOptions.SUPPORTS_EMBEDS];
            if (userEmbedOverride != null && userEmbedOverride != String.Empty)
            {
                switch (userEmbedOverride)
                {
                    case "yes":
                        {
                            comboEmbeds.SelectedItem = new SupportsOptionItem(SupportsFeature.Yes);
                            break;
                        }
                    case "no":
                        {
                            comboEmbeds.SelectedItem = new SupportsOptionItem(SupportsFeature.No);
                            break;
                        }
                    default:
                        {
                            comboEmbeds.SelectedItem = new SupportsOptionItem(SupportsFeature.Unknown);
                            break;
                        }
                }
            }
            else
            {
                comboEmbeds.SelectedItem = new SupportsOptionItem(clientSupportsEmbeds);
            }
        }

        private void SetXHTMLValues()
        {
            // Note that the values for REQUIRES_XHTML can be EITHER bool or string depending
            // on how the pref dialog is invoked!! ToString() and then Parse() takes care of both cases.

            string defaultXHTML = Res.Get(StringId.AdvancedXHTMLDefault);
            bool optionOverride = TemporaryBlogSettings.HomePageOverrides.Contains(BlogClientOptions.REQUIRES_XHTML)
                                ? StringHelper.ToBool(TemporaryBlogSettings.HomePageOverrides[BlogClientOptions.REQUIRES_XHTML].ToString(), false)
                                : false;

            optionOverride = TemporaryBlogSettings.OptionOverrides.Contains(BlogClientOptions.REQUIRES_XHTML)
                                ? StringHelper.ToBool(TemporaryBlogSettings.OptionOverrides[BlogClientOptions.REQUIRES_XHTML].ToString(), optionOverride)
                                : optionOverride;

            int currentOption = 0; // default: unspecified, use OptionOverride
            if (TemporaryBlogSettings.UserOptionOverrides.Contains(BlogClientOptions.REQUIRES_XHTML))
            {
                if (StringHelper.ToBool(TemporaryBlogSettings.UserOptionOverrides[BlogClientOptions.REQUIRES_XHTML].ToString(), false))
                    currentOption = 2; // XHTML desired
                else
                    currentOption = 1; // HTML desired
            }

            defaultXHTML =
                string.Format(CultureInfo.CurrentCulture, defaultXHTML, Res.Get(optionOverride ? StringId.MarkupTypeXHTML : StringId.MarkupTypeHTML));

            comboXHTML.Items.Add(defaultXHTML);
            comboXHTML.Items.Add(Res.Get(StringId.MarkupTypeHTML));
            comboXHTML.Items.Add(Res.Get(StringId.MarkupTypeXHTML));

            comboXHTML.SelectedIndex = currentOption;
        }

        private bool CodePageProc(string codePageName)
        {
            try
            {
                codepages.Add(Encoding.GetEncoding(Int32.Parse(codePageName, CultureInfo.InvariantCulture)));
            }
            catch (NotSupportedException) { }
            catch (ArgumentException) { }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while getting system code pages: " + ex.ToString());
            }
            return true;
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxTransportEncoding = new System.Windows.Forms.GroupBox();
            this.comboEncoding = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.comboEmbeds = new System.Windows.Forms.ComboBox();
            this.comboScripts = new System.Windows.Forms.ComboBox();
            this.lblEmbeds = new System.Windows.Forms.Label();
            this.lblScripts = new System.Windows.Forms.Label();
            this.groupBoxXHTML = new System.Windows.Forms.GroupBox();
            this.comboXHTML = new System.Windows.Forms.ComboBox();
            this.labelXHTML = new System.Windows.Forms.Label();
            this.groupBoxTransportEncoding.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.groupBoxXHTML.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxTransportEncoding
            //
            this.groupBoxTransportEncoding.Controls.Add(this.comboEncoding);
            this.groupBoxTransportEncoding.Controls.Add(this.label1);
            this.groupBoxTransportEncoding.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxTransportEncoding.Location = new System.Drawing.Point(8, 32);
            this.groupBoxTransportEncoding.Name = "groupBoxTransportEncoding";
            this.groupBoxTransportEncoding.Size = new System.Drawing.Size(352, 112);
            this.groupBoxTransportEncoding.TabIndex = 1;
            this.groupBoxTransportEncoding.TabStop = false;
            this.groupBoxTransportEncoding.Text = "Transport Encoding";
            //
            // comboEncoding
            //
            this.comboEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEncoding.Location = new System.Drawing.Point(16, 80);
            this.comboEncoding.Name = "comboEncoding";
            this.comboEncoding.Size = new System.Drawing.Size(320, 21);
            this.comboEncoding.TabIndex = 6;
            this.comboEncoding.SelectedIndexChanged += new System.EventHandler(this.comboEncoding_SelectedIndexChanged);
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(16, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(320, 56);
            this.label1.TabIndex = 5;
            this.label1.Text = "Open Live Writer will use the appropriate default encoding for your posts, usu" +
                "ally UTF-8. You can manually override this suggestion by selecting a different e" +
                "ncoding below.";
            //
            // groupBoxOptions
            //
            this.groupBoxOptions.Controls.Add(this.comboEmbeds);
            this.groupBoxOptions.Controls.Add(this.comboScripts);
            this.groupBoxOptions.Controls.Add(this.lblEmbeds);
            this.groupBoxOptions.Controls.Add(this.lblScripts);
            this.groupBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxOptions.Location = new System.Drawing.Point(8, 264);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(352, 80);
            this.groupBoxOptions.TabIndex = 2;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Blog Override Options";
            //
            // comboEmbeds
            //
            this.comboEmbeds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEmbeds.Location = new System.Drawing.Point(104, 48);
            this.comboEmbeds.Name = "comboEmbeds";
            this.comboEmbeds.Size = new System.Drawing.Size(232, 21);
            this.comboEmbeds.TabIndex = 3;
            this.comboEmbeds.SelectedIndexChanged += new System.EventHandler(this.comboEmbeds_SelectedIndexChanged);
            //
            // comboScripts
            //
            this.comboScripts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboScripts.Location = new System.Drawing.Point(104, 24);
            this.comboScripts.Name = "comboScripts";
            this.comboScripts.Size = new System.Drawing.Size(232, 21);
            this.comboScripts.TabIndex = 1;
            this.comboScripts.SelectedIndexChanged += new System.EventHandler(this.comboScripts_SelectedIndexChanged);
            //
            // lblEmbeds
            //
            this.lblEmbeds.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblEmbeds.Location = new System.Drawing.Point(16, 51);
            this.lblEmbeds.Name = "lblEmbeds";
            this.lblEmbeds.Size = new System.Drawing.Size(80, 23);
            this.lblEmbeds.TabIndex = 2;
            this.lblEmbeds.Text = "Embed Tags:";
            //
            // lblScripts
            //
            this.lblScripts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblScripts.Location = new System.Drawing.Point(16, 27);
            this.lblScripts.Name = "lblScripts";
            this.lblScripts.Size = new System.Drawing.Size(80, 23);
            this.lblScripts.TabIndex = 0;
            this.lblScripts.Text = "Scripts:";
            //
            // groupBoxXHTML
            //
            this.groupBoxXHTML.Controls.Add(this.comboXHTML);
            this.groupBoxXHTML.Controls.Add(this.labelXHTML);
            this.groupBoxXHTML.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxXHTML.Location = new System.Drawing.Point(8, 152);
            this.groupBoxXHTML.Name = "groupBoxXHTML";
            this.groupBoxXHTML.Size = new System.Drawing.Size(352, 104);
            this.groupBoxXHTML.TabIndex = 2;
            this.groupBoxXHTML.TabStop = false;
            this.groupBoxXHTML.Text = "Markup Type";
            //
            // comboXHTML
            //
            this.comboXHTML.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboXHTML.Location = new System.Drawing.Point(16, 72);
            this.comboXHTML.Name = "comboXHTML";
            this.comboXHTML.Size = new System.Drawing.Size(320, 21);
            this.comboXHTML.TabIndex = 1;
            this.comboXHTML.SelectedIndexChanged += new System.EventHandler(this.comboXHTML_SelectedIndexChanged);
            //
            // labelXHTML
            //
            this.labelXHTML.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelXHTML.Location = new System.Drawing.Point(16, 24);
            this.labelXHTML.Name = "labelXHTML";
            this.labelXHTML.Size = new System.Drawing.Size(320, 48);
            this.labelXHTML.TabIndex = 0;
            this.labelXHTML.Text = "If your blog homepage uses XHTML, Writer can generate well-formed markup. If you " +
                "prefer standard HTML, you can manually override this behavior below.";
            //
            // AdvancedPanel
            //
            this.Controls.Add(this.groupBoxXHTML);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.groupBoxTransportEncoding);
            this.Name = "AdvancedPanel";
            this.PanelName = "Advanced";
            this.Controls.SetChildIndex(this.groupBoxTransportEncoding, 0);
            this.Controls.SetChildIndex(this.groupBoxOptions, 0);
            this.Controls.SetChildIndex(this.groupBoxXHTML, 0);
            this.groupBoxTransportEncoding.ResumeLayout(false);
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxXHTML.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void comboEncoding_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            TemporaryBlogSettings.UserOptionOverrides.Remove(BlogClientOptions.CHARACTER_SET);
            if (comboEncoding.SelectedIndex != 0)
            {
                OptionItem selected = (OptionItem)comboEncoding.SelectedItem;
                if (selected != null)
                {
                    TemporaryBlogSettings.UserOptionOverrides.Add(BlogClientOptions.CHARACTER_SET, selected.ItemValue.WebName);
                }
            }
            TemporaryBlogSettingsModified = true;
        }

        private void comboScripts_SelectedIndexChanged(object sender, EventArgs e)
        {
            TemporaryBlogSettings.UserOptionOverrides.Remove(BlogClientOptions.SUPPORTS_SCRIPTS);

            SupportsOptionItem selected = (SupportsOptionItem)comboScripts.SelectedItem;
            if (selected != null)
            {
                TemporaryBlogSettings.UserOptionOverrides.Add(BlogClientOptions.SUPPORTS_SCRIPTS, selected.OptionVal);
            }
            TemporaryBlogSettingsModified = true;
        }

        private void comboEmbeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            TemporaryBlogSettings.UserOptionOverrides.Remove(BlogClientOptions.SUPPORTS_EMBEDS);
            SupportsOptionItem selected = (SupportsOptionItem)comboEmbeds.SelectedItem;
            if (selected != null)
            {
                TemporaryBlogSettings.UserOptionOverrides.Add(BlogClientOptions.SUPPORTS_EMBEDS, selected.OptionVal);
            }
            TemporaryBlogSettingsModified = true;
        }

        private void comboXHTML_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboXHTML.SelectedIndex == 0)
                TemporaryBlogSettings.UserOptionOverrides.Remove(BlogClientOptions.REQUIRES_XHTML);
            else
            {
                bool requireXHTML = comboXHTML.SelectedIndex == 2;
                TemporaryBlogSettings.UserOptionOverrides[BlogClientOptions.REQUIRES_XHTML] = requireXHTML;
            }
            TemporaryBlogSettingsModified = true;
        }

        internal class EncodingComparer : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                Encoding e1;
                Encoding e2;

                if (x is Encoding)
                    e1 = x as Encoding;
                else
                    throw new ArgumentException("Object is not of type Encoding.");

                if (y is Encoding)
                    e2 = y as Encoding;
                else
                    throw new ArgumentException("Object is not of type Encoding.");

                return string.Compare(e1.EncodingName, e2.EncodingName, true, CultureInfo.CurrentCulture);
            }

            #endregion
        }

        internal class OptionItem
        {
            internal string Text;
            internal Encoding ItemValue;
            public OptionItem(string text, Encoding val)
            {
                Text = text;
                ItemValue = val;
            }

            public override bool Equals(object obj)
            {
                if (obj is OptionItem)
                {
                    OptionItem newOne = (OptionItem)obj;
                    if (newOne.ItemValue == null ^ ItemValue == null)
                        return false;
                    if (newOne.ItemValue == null)
                        return true;
                    return newOne.ItemValue.Equals(ItemValue);
                }
                return false;
            }

            public override int GetHashCode()
            {
                if (ItemValue == null)
                    return 0;
                return ItemValue.GetHashCode();
            }

            public override string ToString()
            {
                return Text;
            }
        }

        internal class SupportsOptionItem
        {
            enum OptionType { SCRIPT, EMBED }

            private void SetVals(SupportsFeature val)
            {
                switch (val)
                {
                    case SupportsFeature.No:
                        {
                            Text = Res.Get(StringId.Blog_Option_Not_Supported);
                            OptionVal = "no";
                            break;
                        }
                    case SupportsFeature.Yes:
                        {
                            Text = Res.Get(StringId.Blog_Option_Supported);
                            OptionVal = "yes";
                            break;
                        }
                    case SupportsFeature.Unknown:
                    default:
                        {
                            Text = Res.Get(StringId.Blog_Option_Unknown);
                            OptionVal = "unknown";
                            break;
                        }
                }
            }

            internal string Text;
            internal string OptionVal;
            internal SupportsFeature ItemValue;
            public SupportsOptionItem(SupportsFeature val)
            {
                SetVals(val);
                ItemValue = val;
            }

            public override bool Equals(object obj)
            {
                if (obj is SupportsOptionItem)
                {
                    SupportsOptionItem newOne = (SupportsOptionItem)obj;
                    return newOne.ItemValue.Equals(ItemValue);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return ItemValue.GetHashCode();
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
