// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunnerGui
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml;

    using BlogRunner.Core.Config;

    using JetBrains.Annotations;

    /// <summary>
    /// Class Form1.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class Form1 : Form
    {
        /// <summary>
        /// The setting configuration
        /// </summary>
        private const string SETTING_CONFIG = "config";

        /// <summary>
        /// The setting output
        /// </summary>
        private const string SETTING_OUTPUT = "output";

        /// <summary>
        /// The setting provider
        /// </summary>
        private const string SETTING_PROVIDER = "provider";

        /// <summary>
        /// The setting providers
        /// </summary>
        private const string SETTING_PROVIDERS = "providers";

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the selected providers.
        /// </summary>
        /// <value>The selected providers.</value>
        [NotNull]
        private BlogProviderItem[] SelectedProviders
            => this.listProviders.CheckedItems.Cast<BlogProviderItem>().ToArray();

        /// <summary>
        /// Sets the selected provider ids.
        /// </summary>
        /// <param name="providerIds">The provider ids.</param>
        public void SetSelectedProviderIds([NotNull] string[] providerIds)
        {
            for (var i = 0; i < this.listProviders.Items.Count; i++)
            {
                if (Array.IndexOf(providerIds, ((BlogProviderItem)this.listProviders.Items[i]).Id) >= 0)
                {
                    this.listProviders.SetItemChecked(i, true);
                }
            }
        }

        /// <summary>
        /// Adds quotes around the specified string if it contains a space.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The output.</returns>
        [CanBeNull]
        private static string MaybeQuote([CanBeNull]string str) => str?.Contains(" ") ?? false ? $"\"{str}\"" : str;

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnRun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            var cmdLine = this.textBox1.Text;
            if (cmdLine.Length == 0)
            {
                MessageBox.Show("Nothing to do!");
                return;
            }

            var chunks = cmdLine.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Length == 1)
            {
                Process.Start(chunks[0]);
            }
            else
            {
                Process.Start(chunks[0], chunks[1]);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < this.listProviders.Items.Count; i++)
            {
                this.listProviders.SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectNone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < this.listProviders.Items.Count; i++)
            {
                this.listProviders.SetItemChecked(i, false);
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkVerbose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void chkVerbose_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateCommand();
        }

        /// <summary>
        /// Handles the PathChanged event of the fileBlogProviders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fileBlogProviders_PathChanged(object sender, EventArgs e)
        {
            this.UpdateCommand();
        }

        /// <summary>
        /// Handles the PathChanged event of the fileConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fileConfig_PathChanged(object sender, EventArgs e)
        {
            this.listProviders.Items.Clear();

            var configPath = this.fileConfig.Path;

            this.UpdateCommand();

            try
            {
                if (configPath.Length == 0 || !File.Exists(configPath))
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(configPath);
            var xmlNodeList = xmlDoc.SelectNodes("/config/providers/provider/blog/.."); // Not L10N
            if (xmlNodeList != null)
            {
                foreach (XmlElement providerEl in xmlNodeList)
                {
                    var id = providerEl.SelectSingleNode("id/text()")?.Value; // Not L10N
                    var name = providerEl.SelectSingleNode("name/text()")?.Value; // Not L10N
                    if (id == null || name == null)
                    {
                        continue;
                    }

                    var item = new BlogProviderItem(id, name);
                    this.listProviders.Items.Add(item);
                }
            }

            this.UpdateCommand();
        }

        /// <summary>
        /// Handles the PathChanged event of the fileOutput control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fileOutput_PathChanged(object sender, EventArgs e)
        {
            this.UpdateCommand();
        }

        /// <summary>
        /// Handles the Load event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            this.LoadSettings();
            this.UpdateCommand();
        }

        /// <summary>
        /// Handles the ItemCheck event of the listProviders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemCheckEventArgs"/> instance containing the event data.</param>
        private void listProviders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var checkedItems = new List<BlogProviderItem>(this.SelectedProviders);

            var currentItem = (BlogProviderItem)this.listProviders.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                checkedItems.Add(currentItem);
            }
            else
            {
                checkedItems.Remove(currentItem);
            }

            this.UpdateCommand(checkedItems);
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        private void LoadSettings()
        {
            var options = new BlogRunnerCommandLineOptions();
            options.Parse(Environment.GetCommandLineArgs(), false);
            this.fileBlogProviders.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_PROVIDERS, string.Empty);
            this.fileConfig.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_CONFIG, string.Empty);
            this.fileOutput.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_OUTPUT, string.Empty);
            this.chkVerbose.Checked = options.GetFlagValue(BlogRunnerCommandLineOptions.OPTION_VERBOSE, false);
            this.SetSelectedProviderIds(options.UnnamedArgs);
        }

        /// <summary>
        /// Updates the command.
        /// </summary>
        private void UpdateCommand()
        {
            this.UpdateCommand(this.SelectedProviders);
        }

        /// <summary>
        /// Updates the command.
        /// </summary>
        /// <param name="providers">The providers.</param>
        private void UpdateCommand(IEnumerable<BlogProviderItem> providers)
        {
            var ids = new List<BlogProviderItem>(providers).ConvertAll(item => item.Id);
            if (ids.Count == this.listProviders.Items.Count)
            {
                ids.Clear();
            }

            var args = new List<string>
                           {
                               "BlogRunner.exe",
                               $"/{BlogRunnerCommandLineOptions.OPTION_PROVIDERS}:{this.fileBlogProviders.Path}",
                               $"/{BlogRunnerCommandLineOptions.OPTION_CONFIG}:{this.fileConfig.Path}"
                           };

            if (this.fileOutput.Path.Length > 0)
            {
                args.Add($"/{BlogRunnerCommandLineOptions.OPTION_OUTPUT}:{this.fileOutput.Path}");
            }

            if (this.chkVerbose.Checked)
            {
                args.Add($"/{BlogRunnerCommandLineOptions.OPTION_VERBOSE}");
            }

            args.Add($"/{BlogRunnerCommandLineOptions.OPTION_PAUSE}");

            args.AddRange(ids);
            args = args.ConvertAll(MaybeQuote);
            this.textBox1.Text = string.Join(" ", args.ToArray());
        }

        /// <summary>
        /// Class BlogProviderItem.
        /// </summary>
        private class BlogProviderItem
        {
            /// <summary>
            /// The identifier
            /// </summary>
            [NotNull]
            public readonly string Id;

            /// <summary>
            /// The name
            /// </summary>
            [NotNull]
            public readonly string Name;

            /// <summary>
            /// Initializes a new instance of the <see cref="BlogProviderItem"/> class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="name">The name.</param>
            public BlogProviderItem([NotNull] string id, [NotNull] string name)
            {
                this.Id = id;
                this.Name = name;
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
            public override string ToString() => this.Name;

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            public override int GetHashCode() => this.Id.GetHashCode();

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The object to compare with the current object.</param>
            /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
            public override bool Equals([CanBeNull] object obj)
            {
                var other = obj as BlogProviderItem;
                return other != null && string.Equals(this.Id, other.Id, StringComparison.Ordinal);
            }
        }

    }
}
