using CodeSnippet.Config;
using CodeSnippet.Formats;
using CodeSnippet.Helpers;
using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using WLWPluginBase.Win32;

namespace CodeSnippet.Forms
{
	internal class CodeSnippetCompactForm : Form, ICodeSnippetForm
	{
		private IContainer components;

		private ToolStrip ts1;

		private ToolStrip ts2;

		private ToolStripButton btnLineNumbers;

		private ToolStripButton btnAlternateLines;

		private ToolStripButton btnEmbedStyles;

		private ToolStripButton btnUseContainer;

		private ToolStripButton btnCancel;

		private ToolStripButton btnOK;

		private ListBox lbLang;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripButton btnViewFullMode;

		private ToolTip tp1;

		private readonly bool loading = true;

		public string CodeSnippet
		{
			get
			{
				return JustDecompileGenerated_get_CodeSnippet();
			}
			set
			{
				JustDecompileGenerated_set_CodeSnippet(value);
			}
		}

		private string JustDecompileGenerated_CodeSnippet_k__BackingField;

		public string JustDecompileGenerated_get_CodeSnippet()
		{
			return this.JustDecompileGenerated_CodeSnippet_k__BackingField;
		}

		private void JustDecompileGenerated_set_CodeSnippet(string value)
		{
			this.JustDecompileGenerated_CodeSnippet_k__BackingField = value;
		}

		public string CodeSnippetToEdit
		{
			get
			{
				return JustDecompileGenerated_get_CodeSnippetToEdit();
			}
			set
			{
				JustDecompileGenerated_set_CodeSnippetToEdit(value);
			}
		}

		private string JustDecompileGenerated_CodeSnippetToEdit_k__BackingField;

		private string JustDecompileGenerated_get_CodeSnippetToEdit()
		{
			return this.JustDecompileGenerated_CodeSnippetToEdit_k__BackingField;
		}

		public void JustDecompileGenerated_set_CodeSnippetToEdit(string value)
		{
			this.JustDecompileGenerated_CodeSnippetToEdit_k__BackingField = value;
		}

		public CodeSnippetConfig Config
		{
			get;
			set;
		}

		public CodeSnippetCompactForm(CodeSnippetConfig config)
		{
			this.CodeSnippet = string.Empty;
			this.Config = config;
			this.InitializeComponent();
			this.InitializeList();
			this.ConfigToUI();
			this.loading = false;
		}

		private void btnAlternateLines_Click(object sender, EventArgs e)
		{
			this.Config.Style.AlternateLines = this.btnAlternateLines.Checked;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			base.Close();
		}

		private void btnEmbedStyles_Click(object sender, EventArgs e)
		{
			this.Config.Style.EmbedStyles = this.btnEmbedStyles.Checked;
		}

		private void btnLineNumbers_Click(object sender, EventArgs e)
		{
			this.Config.Style.LineNumbers = this.btnLineNumbers.Checked;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			this.CodeSnippet = FormatHelper.Format(SupportedFormat.GetItemKey(this.lbLang.SelectedItem), this.Config.Editor, this.Config.Style, (string.IsNullOrEmpty(this.CodeSnippetToEdit) ? Clipboard.GetText() : this.CodeSnippetToEdit), this.Config.Editor.TrimIndentOnPaste);
			base.DialogResult = System.Windows.Forms.DialogResult.OK;
			base.Close();
		}

		private void btnUseContainer_Click(object sender, EventArgs e)
		{
			this.Config.Style.UseContainer = this.btnUseContainer.Checked;
		}

		private void btnViewFullMode_Click(object sender, EventArgs e)
		{
			this.Config.General.ActiveMode = CodeSnippetViewMode.Full;
			base.DialogResult = System.Windows.Forms.DialogResult.Retry;
			base.Close();
		}

		System.Windows.Forms.DialogResult CodeSnippet.Forms.ICodeSnippetForm.ShowDialog(IWin32Window win32Window)
		{
			return base.ShowDialog(win32Window);
		}

		private void CodeSnippetCompactForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.Config.Layout.CompactModePlacement = Win32WndHelper.GetPlacement(base.Handle);
		}

		private void CodeSnippetCompactForm_Load(object sender, EventArgs e)
		{
			if (this.Config.Reposition)
			{
				Win32WndHelper.SetPlacement(base.Handle, this.Config.Layout.CompactModePlacement);
			}
		}

		private void ConfigToUI()
		{
			this.lbLang.SelectedItem = SupportedFormat.GetItem(this.Config.General.ActiveLanguage);
			this.btnEmbedStyles.Checked = this.Config.Style.EmbedStyles;
			this.btnUseContainer.Checked = this.Config.Style.UseContainer;
			this.btnLineNumbers.Checked = this.Config.Style.LineNumbers;
			this.btnAlternateLines.Checked = this.Config.Style.AlternateLines;
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
			this.ts1 = new ToolStrip();
			this.btnLineNumbers = new ToolStripButton();
			this.btnAlternateLines = new ToolStripButton();
			this.btnEmbedStyles = new ToolStripButton();
			this.btnUseContainer = new ToolStripButton();
			this.toolStripSeparator1 = new ToolStripSeparator();
			this.btnViewFullMode = new ToolStripButton();
			this.ts2 = new ToolStrip();
			this.btnCancel = new ToolStripButton();
			this.btnOK = new ToolStripButton();
			this.lbLang = new ListBox();
			this.tp1 = new ToolTip(this.components);
			this.ts1.SuspendLayout();
			this.ts2.SuspendLayout();
			base.SuspendLayout();
			this.ts1.GripStyle = ToolStripGripStyle.Hidden;
			ToolStripItemCollection items = this.ts1.Items;
			ToolStripItem[] toolStripItemArray = new ToolStripItem[] { this.btnLineNumbers, this.btnAlternateLines, this.btnEmbedStyles, this.btnUseContainer, this.toolStripSeparator1, this.btnViewFullMode };
			items.AddRange(toolStripItemArray);
			this.ts1.Location = new Point(0, 0);
			this.ts1.Name = "ts1";
			this.ts1.Size = new System.Drawing.Size(134, 25);
			this.ts1.TabIndex = 0;
			this.ts1.Text = "ts1";
			this.btnLineNumbers.CheckOnClick = true;
			this.btnLineNumbers.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnLineNumbers.Image = Resources.text_list_numbers;
			this.btnLineNumbers.ImageTransparentColor = Color.Magenta;
			this.btnLineNumbers.Name = "btnLineNumbers";
			this.btnLineNumbers.Size = new System.Drawing.Size(23, 22);
			this.btnLineNumbers.Text = "Line Numbers";
			this.btnLineNumbers.ToolTipText = "Include line numbers";
			this.btnLineNumbers.Click += new EventHandler(this.btnLineNumbers_Click);
			this.btnAlternateLines.CheckOnClick = true;
			this.btnAlternateLines.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnAlternateLines.Image = Resources.alternate_line_bkg;
			this.btnAlternateLines.ImageTransparentColor = Color.Magenta;
			this.btnAlternateLines.Name = "btnAlternateLines";
			this.btnAlternateLines.Size = new System.Drawing.Size(23, 22);
			this.btnAlternateLines.Text = "Alternate Lines";
			this.btnAlternateLines.ToolTipText = "Alternate line background color";
			this.btnAlternateLines.Click += new EventHandler(this.btnAlternateLines_Click);
			this.btnEmbedStyles.CheckOnClick = true;
			this.btnEmbedStyles.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnEmbedStyles.Image = Resources.style_add;
			this.btnEmbedStyles.ImageTransparentColor = Color.Magenta;
			this.btnEmbedStyles.Name = "btnEmbedStyles";
			this.btnEmbedStyles.Size = new System.Drawing.Size(23, 22);
			this.btnEmbedStyles.Text = "Embed Styles";
			this.btnEmbedStyles.ToolTipText = "Embed styles used to format the code";
			this.btnEmbedStyles.Click += new EventHandler(this.btnEmbedStyles_Click);
			this.btnUseContainer.CheckOnClick = true;
			this.btnUseContainer.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnUseContainer.Image = Resources.layers;
			this.btnUseContainer.ImageTransparentColor = Color.Magenta;
			this.btnUseContainer.Name = "btnUseContainer";
			this.btnUseContainer.Size = new System.Drawing.Size(23, 22);
			this.btnUseContainer.Text = "Use Container";
			this.btnUseContainer.ToolTipText = "Place code in a container";
			this.btnUseContainer.Click += new EventHandler(this.btnUseContainer_Click);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			this.btnViewFullMode.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnViewFullMode.Image = Resources.full_mode;
			this.btnViewFullMode.ImageTransparentColor = Color.Magenta;
			this.btnViewFullMode.Name = "btnViewFullMode";
			this.btnViewFullMode.Size = new System.Drawing.Size(23, 22);
			this.btnViewFullMode.Text = "Switch to Full Mode";
			this.btnViewFullMode.Click += new EventHandler(this.btnViewFullMode_Click);
			this.ts2.Dock = DockStyle.Bottom;
			this.ts2.GripStyle = ToolStripGripStyle.Hidden;
			ToolStripItemCollection toolStripItemCollections = this.ts2.Items;
			ToolStripItem[] toolStripItemArray1 = new ToolStripItem[] { this.btnCancel, this.btnOK };
			toolStripItemCollections.AddRange(toolStripItemArray1);
			this.ts2.Location = new Point(0, 120);
			this.ts2.Name = "ts2";
			this.ts2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.ts2.Size = new System.Drawing.Size(134, 25);
			this.ts2.TabIndex = 2;
			this.ts2.Text = "ts2";
			this.ts2.PreviewKeyDown += new PreviewKeyDownEventHandler(this.lbLang_PreviewKeyDown);
			this.btnCancel.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnCancel.Image = Resources.cancel;
			this.btnCancel.ImageTransparentColor = Color.Magenta;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(23, 22);
			this.btnCancel.Text = "Don't Insert";
			this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
			this.btnOK.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnOK.Image = Resources.accept;
			this.btnOK.ImageTransparentColor = Color.Magenta;
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(23, 22);
			this.btnOK.Text = "Insert";
			this.btnOK.Click += new EventHandler(this.btnOK_Click);
			this.lbLang.Dock = DockStyle.Fill;
			this.lbLang.Location = new Point(0, 25);
			this.lbLang.Name = "lbLang";
			this.lbLang.Size = new System.Drawing.Size(134, 95);
			this.lbLang.TabIndex = 1;
			this.tp1.SetToolTip(this.lbLang, "Language");
			this.lbLang.PreviewKeyDown += new PreviewKeyDownEventHandler(this.lbLang_PreviewKeyDown);
			this.lbLang.DoubleClick += new EventHandler(this.btnOK_Click);
			this.lbLang.SelectedIndexChanged += new EventHandler(this.lbLang_SelectedIndexChanged);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(134, 145);
			base.Controls.Add(this.lbLang);
			base.Controls.Add(this.ts2);
			base.Controls.Add(this.ts1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "CodeSnippetCompactForm";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Code Snippet";
			base.FormClosing += new FormClosingEventHandler(this.CodeSnippetCompactForm_FormClosing);
			base.Load += new EventHandler(this.CodeSnippetCompactForm_Load);
			this.ts1.ResumeLayout(false);
			this.ts1.PerformLayout();
			this.ts2.ResumeLayout(false);
			this.ts2.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void InitializeList()
		{
			this.lbLang.DataSource = SupportedFormat.Items;
		}

		private void lbLang_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			Keys keyCode = e.KeyCode;
			if (keyCode == Keys.Return)
			{
				this.btnOK_Click(null, null);
				return;
			}
			if (keyCode == Keys.Escape)
			{
				this.btnCancel_Click(null, null);
				return;
			}
			if (keyCode != Keys.F12)
			{
				return;
			}
			this.btnViewFullMode_Click(null, null);
		}

		private void lbLang_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!this.loading)
			{
				this.Config.General.ActiveLanguage = SupportedFormat.GetItemKey(this.lbLang.SelectedItem);
			}
		}
	}
}