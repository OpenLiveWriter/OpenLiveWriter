using CodeSnippet.Config;
using CodeSnippet.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using WLWPluginBase.Win32;

namespace CodeSnippet.Forms
{
	internal class CodeSnippetOptionsForm : Form
	{
		private IContainer components;

		private Button btnOK;

		private Button btnCancel;

		private PropertyGrid pgSectionDetail;

		private ListView lvSections;

		private ImageList ilSections;

		public CodeSnippetConfig Config
		{
			get;
			private set;
		}

		public CodeSnippetOptionsForm(CodeSnippetConfig config)
		{
			this.Config = config;
			this.InitializeComponent();
		}

		private void CodeSnippetOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.Config.Layout.OptionsPlacement = Win32WndHelper.GetPlacement(base.Handle);
		}

		private void CodeSnippetOptionsForm_Load(object sender, EventArgs e)
		{
			Win32WndHelper.SetPlacement(base.Handle, this.Config.Layout.OptionsPlacement);
			foreach (IConfigSection section in this.Config.Sections)
			{
				this.ilSections.Images.Add(section.SectionName, section.Image);
				this.lvSections.Items.Add(section.SectionName, section.SectionName);
			}
			PropertyGridHelper.ResizePropertyGridColumns(this.pgSectionDetail, 30);
			this.lvSections.Items[0].Selected = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.btnOK = new Button();
			this.btnCancel = new Button();
			this.pgSectionDetail = new PropertyGrid();
			this.lvSections = new ListView();
			this.ilSections = new ImageList(this.components);
			base.SuspendLayout();
			this.btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnOK.AutoSize = true;
			this.btnOK.CausesValidation = false;
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new Point(466, 417);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnCancel.AutoSize = true;
			this.btnCancel.CausesValidation = false;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new Point(547, 417);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.pgSectionDetail.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.pgSectionDetail.Location = new Point(109, 12);
			this.pgSectionDetail.Name = "pgSectionDetail";
			this.pgSectionDetail.Size = new System.Drawing.Size(513, 397);
			this.pgSectionDetail.TabIndex = 1;
			this.lvSections.Activation = ItemActivation.OneClick;
			this.lvSections.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			this.lvSections.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.lvSections.GridLines = true;
			this.lvSections.HideSelection = false;
			this.lvSections.LargeImageList = this.ilSections;
			this.lvSections.Location = new Point(13, 13);
			this.lvSections.MultiSelect = false;
			this.lvSections.Name = "lvSections";
			this.lvSections.Scrollable = false;
			this.lvSections.Size = new System.Drawing.Size(90, 396);
			this.lvSections.TabIndex = 0;
			this.lvSections.UseCompatibleStateImageBehavior = false;
			this.lvSections.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(this.lvSections_ItemSelectionChanged);
			this.ilSections.ColorDepth = ColorDepth.Depth32Bit;
			this.ilSections.ImageSize = new System.Drawing.Size(48, 48);
			this.ilSections.TransparentColor = Color.Transparent;
			base.AcceptButton = this.btnOK;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new System.Drawing.Size(634, 452);
			base.Controls.Add(this.lvSections);
			base.Controls.Add(this.pgSectionDetail);
			base.Controls.Add(this.btnOK);
			base.Controls.Add(this.btnCancel);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(640, 480);
			base.Name = "CodeSnippetOptionsForm";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Options";
			base.Load += new EventHandler(this.CodeSnippetOptionsForm_Load);
			base.FormClosing += new FormClosingEventHandler(this.CodeSnippetOptionsForm_FormClosing);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void lvSections_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			this.pgSectionDetail.SelectedObject = this.Config[e.Item.Text];
		}
	}
}