// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Configuration;
using BlogRunner.Core.Config;

namespace BlogRunnerGui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void fileBlogProviders_PathChanged(object sender, EventArgs e)
        {
            UpdateCommand();
        }

        private void fileConfig_PathChanged(object sender, EventArgs e)
        {
            listProviders.Items.Clear();

            string configPath = fileConfig.Path;

            UpdateCommand();

            try
            {
                if (configPath.Length == 0 || !File.Exists(configPath))
                    return;
            }
            catch
            {
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configPath);
            foreach (XmlElement providerEl in xmlDoc.SelectNodes("/config/providers/provider/blog/.."))
            {
                string id = providerEl.SelectSingleNode("id/text()").Value;
                string name = providerEl.SelectSingleNode("name/text()").Value;
                BlogProviderItem item = new BlogProviderItem(id, name);
                listProviders.Items.Add(item);
            }

            UpdateCommand();
        }

        private void fileOutput_PathChanged(object sender, EventArgs e)
        {
            UpdateCommand();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listProviders.Items.Count; i++)
                listProviders.SetItemChecked(i, true);
        }

        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listProviders.Items.Count; i++)
                listProviders.SetItemChecked(i, false);
        }

        private void listProviders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            List<BlogProviderItem> checkedItems = new List<BlogProviderItem>(SelectedProviders);

            BlogProviderItem currentItem = (BlogProviderItem)listProviders.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
                checkedItems.Add(currentItem);
            else
                checkedItems.Remove(currentItem);

            UpdateCommand(checkedItems);
        }

        private void chkVerbose_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCommand();
        }

        private void UpdateCommand()
        {
            UpdateCommand(SelectedProviders);
        }

        private BlogProviderItem[] SelectedProviders
        {
            get
            {
                List<BlogProviderItem> checkedItems = new List<BlogProviderItem>();
                foreach (BlogProviderItem item in listProviders.CheckedItems)
                    checkedItems.Add(item);
                return checkedItems.ToArray();
            }
        }

        public void SetSelectedProviderIds(string[] providerIds)
        {
            for (int i = 0; i < listProviders.Items.Count; i++)
            {
                if (Array.IndexOf(providerIds, ((BlogProviderItem)listProviders.Items[i]).Id) >= 0)
                    listProviders.SetItemChecked(i, true);
            }
        }

        private void UpdateCommand(IEnumerable<BlogProviderItem> providers)
        {
            List<string> ids = new List<BlogProviderItem>(providers).ConvertAll<string>(delegate (BlogProviderItem item) { return item.Id; });
            if (ids.Count == listProviders.Items.Count)
                ids.Clear();

            List<string> args = new List<string>();
            args.Add("BlogRunner.exe");

            args.Add("/" + BlogRunnerCommandLineOptions.OPTION_PROVIDERS + ":" + fileBlogProviders.Path);
            args.Add("/" + BlogRunnerCommandLineOptions.OPTION_CONFIG + ":" + fileConfig.Path);
            if (fileOutput.Path.Length > 0)
                args.Add("/" + BlogRunnerCommandLineOptions.OPTION_OUTPUT + ":" + fileOutput.Path);

            if (chkVerbose.Checked)
                args.Add("/" + BlogRunnerCommandLineOptions.OPTION_VERBOSE);

            args.Add("/" + BlogRunnerCommandLineOptions.OPTION_PAUSE);

            args.AddRange(ids);
            args = args.ConvertAll<string>(delegate (string str) { return MaybeQuote(str); });
            textBox1.Text = string.Join(" ", args.ToArray());
        }

        private string MaybeQuote(string str)
        {
            if (str.Contains(" "))
                return "\"" + str + "\"";
            return str;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            UpdateCommand();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            string cmdLine = textBox1.Text;
            if (cmdLine.Length == 0)
            {
                MessageBox.Show("Nothing to do!");
                return;
            }

            string[] chunks = cmdLine.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Length == 1)
            {
                Process.Start(chunks[0]);
            }
            else
            {
                Process.Start(chunks[0], chunks[1]);
            }
        }

        private void LoadSettings()
        {
            BlogRunnerCommandLineOptions options = new BlogRunnerCommandLineOptions();
            options.Parse(Environment.GetCommandLineArgs(), false);
            fileBlogProviders.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_PROVIDERS, "");
            fileConfig.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_CONFIG, "");
            fileOutput.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_OUTPUT, "");
            chkVerbose.Checked = options.GetFlagValue(BlogRunnerCommandLineOptions.OPTION_VERBOSE, false);
            SetSelectedProviderIds(options.UnnamedArgs);
        }

        const string SETTING_PROVIDERS = "providers";
        const string SETTING_CONFIG = "config";
        const string SETTING_OUTPUT = "output";
        const string SETTING_PROVIDER = "provider";

        class BlogProviderItem
        {
            public readonly string Id;
            public readonly string Name;

            public BlogProviderItem(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                BlogProviderItem other = obj as BlogProviderItem;
                if (other == null)
                    return false;
                return string.Equals(Id, other.Id, StringComparison.Ordinal);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
