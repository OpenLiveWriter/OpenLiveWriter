using CodeSnippet.Config;
using CodeSnippet.Formats;
using CodeSnippet.Formats.Base;
using CodeSnippet.Helpers;
using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using WLWPluginBase.Win32;
using WLWPluginBase.Windows.Forms;

namespace CodeSnippet.Forms
{
	internal class CodeSnippetForm : Form, ICodeSnippetForm
	{
		private IContainer components;

		private SplitContainer sc1;

		private Label lblCode;

		private Button btnCancel;

		private Button btnOK;

		private ToolStripComboBox cbLang;

		private ToolStripButton btnEmbedStyles;

		private ToolStripButton btnLineNumbers;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripButton btnAlternateLines;

		private ToolStripButton btnUseContainer;

		private WebBrowser wbPreview;

		private ToolStrip ts1;

		private ToolStrip ts2;

		private ToolStripButton btnPaste;

		private ToolStripSeparator toolStripSeparator2;

		private ToolStripButton btnDecIndent;

		private ToolStripButton btnIncIndent;

		private ToolStripButton btnCut;

		private ToolStripButton btnCopy;

		private ToolStripButton btnViewHtml;

		private ToolStripSeparator toolStripSeparator3;

		private ToolStripSeparator toolStripSeparator4;

		private ToolStripButton btnWordWrap;

		private RichTextBoxEx rtbCodeSnippet;

		private ToolStripButton btnUndo;

		private ToolStripButton btnRedo;

		private ToolStripSeparator toolStripSeparator5;

		private ToolStripButton btnClipboardClear;

		private MenuStrip ms1;

		private ToolStripMenuItem miFile;

		private ToolStripSeparator toolStripSeparator;

		private ToolStripMenuItem miFileClose;

		private ToolStripMenuItem miEdit;

		private ToolStripMenuItem miEditUndo;

		private ToolStripMenuItem miEditRedo;

		private ToolStripSeparator toolStripSeparator8;

		private ToolStripMenuItem miEditCut;

		private ToolStripMenuItem miEditCopy;

		private ToolStripMenuItem miEditPaste;

		private ToolStripSeparator toolStripSeparator9;

		private ToolStripMenuItem miEditSelectAll;

		private ToolStripMenuItem miHelp;

		private ToolStripMenuItem miHelpVisitMyBlog;

		private ToolStripSeparator toolStripSeparator6;

		private ToolStripMenuItem miEditClearClipboard;

		private ToolStripSeparator toolStripSeparator7;

		private ToolStripMenuItem miView;

		private ToolStripMenuItem miViewHtml;

		private ToolStripSeparator toolStripSeparator11;

		private ToolStripMenuItem miViewCodeSnippet;

		private ToolStripMenuItem miViewFormattedCodeSnippet;

		private ToolStripMenuItem miViewBoth;

		private ToolStripMenuItem miEditLineIndent;

		private ToolStripMenuItem miEditLineIndentIncrease;

		private ToolStripMenuItem miEditLineIndentDecrease;

		private ToolStripSeparator toolStripSeparator10;

		private ToolStripMenuItem miEditTrimOnPaste;

		private ToolStripMenuItem miViewWrap;

		private ToolStripSeparator toolStripSeparator12;

		private Button btnMyBlog;

		private ToolTip tp1;

		private ToolStripMenuItem miFileRunSilent;

		private ToolStripMenuItem miFormat;

		private ToolStripMenuItem miFormatLineNumbers;

		private ToolStripMenuItem miFormatAlternateLines;

		private ToolStripMenuItem miFormatEmbedStyles;

		private ToolStripMenuItem miFormatUseContainer;

		private ToolStripSeparator toolStripSeparator13;

		private ToolStripMenuItem miCopyStylesToClipboard;

		private ToolStripMenuItem miHelpReadme;

		private ToolStripSeparator toolStripSeparator14;

		private ToolStripSeparator toolStripSeparator15;

		private ToolStripMenuItem miViewCompactMode;

		private Label lblFormattedCode;

		private ToolStripMenuItem miTools;

		private ToolStripMenuItem miToolsOptions;

		private ToolStripSeparator toolStripSeparator16;

		private ToolStripMenuItem miHelpAbout;

		private bool isInitialized;

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
			set
			{
				this.rtbCodeSnippet.Text = value;
			}
		}

		public CodeSnippetConfig Config
		{
			get;
			set;
		}

		private static string ReadmeFile
		{
			get
			{
				if (Assembly.GetExecutingAssembly().Location == null)
				{
					return string.Empty;
				}
				return Assembly.GetExecutingAssembly().Location.Replace(".dll", ".rtf");
			}
		}

		public CodeSnippetForm(CodeSnippetConfig config)
		{
			this.CodeSnippet = string.Empty;
			this.Config = config;
			this.InitializeComponent();
			this.InitializeList();
			this.ConfigToUI();
		}

		private void btnAlternateLineBkg_Click(object sender, EventArgs e)
		{
			this.miFormatAlternateLines.Checked = this.btnAlternateLines.Checked;
			this.FormatCode(true);
		}

		private void btnEmbedStyles_Click(object sender, EventArgs e)
		{
			this.miFormatEmbedStyles.Checked = this.btnEmbedStyles.Checked;
		}

		private void btnLineNumbers_Click(object sender, EventArgs e)
		{
			this.miFormatLineNumbers.Checked = this.btnLineNumbers.Checked;
			this.FormatCode(true);
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			this.FormatCode(false);
		}

		private void btnUseContainer_Click(object sender, EventArgs e)
		{
			this.miFormatUseContainer.Checked = this.btnUseContainer.Checked;
			this.FormatCode(true);
		}

		private void btnViewHtml_Click(object sender, EventArgs e)
		{
			Win32IEHelper.ViewSource(this.wbPreview.Handle);
		}

		private void btnWordWrap_Click(object sender, EventArgs e)
		{
			EditorConfig editor = this.Config.Editor;
			RichTextBoxEx richTextBoxEx = this.rtbCodeSnippet;
			ToolStripMenuItem toolStripMenuItem = this.miViewWrap;
			bool @checked = this.btnWordWrap.Checked;
			bool flag = @checked;
			toolStripMenuItem.Checked = @checked;
			bool flag1 = flag;
			bool flag2 = flag1;
			richTextBoxEx.WordWrap = flag1;
			editor.WordWrap = flag2;
		}

		 System.Windows.Forms.DialogResult CodeSnippet.Forms.ICodeSnippetForm.ShowDialog(IWin32Window win32Window)
		{
			return base.ShowDialog(win32Window);
		}

		private void CodeSnippetForm_Activated(object sender, EventArgs e)
		{
			this.UpdateButtonStates();
			this.rtbCodeSnippet.Focus();
		}

		private void CodeSnippetForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.Config = this.UIToConfig(false);
		}

		private void CodeSnippetForm_Load(object sender, EventArgs e)
		{
			if (this.Config.Reposition)
			{
				Win32WndHelper.SetPlacement(base.Handle, this.Config.Layout.FullModePlacement);
			}
			this.isInitialized = true;
			this.FormatCode(true);
		}

		private void ConfigToUI()
		{
			this.miFileRunSilent.Checked = this.Config.General.RunSilent;
			this.cbLang.SelectedItem = SupportedFormat.GetItem(this.Config.General.ActiveLanguage);
			ToolStripMenuItem toolStripMenuItem = this.miEditTrimOnPaste;
			RichTextBoxEx richTextBoxEx = this.rtbCodeSnippet;
			bool trimIndentOnPaste = this.Config.Editor.TrimIndentOnPaste;
			bool flag = trimIndentOnPaste;
			richTextBoxEx.TrimIndentOnPaste = trimIndentOnPaste;
			toolStripMenuItem.Checked = flag;
			ToolStripMenuItem toolStripMenuItem1 = this.miViewWrap;
			ToolStripButton toolStripButton = this.btnWordWrap;
			RichTextBoxEx richTextBoxEx1 = this.rtbCodeSnippet;
			bool wordWrap = this.Config.Editor.WordWrap;
			bool flag1 = wordWrap;
			richTextBoxEx1.WordWrap = wordWrap;
			bool flag2 = flag1;
			bool flag3 = flag2;
			toolStripButton.Checked = flag2;
			toolStripMenuItem1.Checked = flag3;
			ToolStripMenuItem toolStripMenuItem2 = this.miFormatEmbedStyles;
			ToolStripButton toolStripButton1 = this.btnEmbedStyles;
			bool embedStyles = this.Config.Style.EmbedStyles;
			bool flag4 = embedStyles;
			toolStripButton1.Checked = embedStyles;
			toolStripMenuItem2.Checked = flag4;
			this.rtbCodeSnippet.TabSpaces = this.Config.Editor.TabSpaces;
			ToolStripMenuItem toolStripMenuItem3 = this.miFormatUseContainer;
			ToolStripButton toolStripButton2 = this.btnUseContainer;
			bool useContainer = this.Config.Style.UseContainer;
			bool flag5 = useContainer;
			toolStripButton2.Checked = useContainer;
			toolStripMenuItem3.Checked = flag5;
			ToolStripMenuItem toolStripMenuItem4 = this.miFormatLineNumbers;
			ToolStripButton toolStripButton3 = this.btnLineNumbers;
			bool lineNumbers = this.Config.Style.LineNumbers;
			bool flag6 = lineNumbers;
			toolStripButton3.Checked = lineNumbers;
			toolStripMenuItem4.Checked = flag6;
			ToolStripMenuItem toolStripMenuItem5 = this.miFormatAlternateLines;
			ToolStripButton toolStripButton4 = this.btnAlternateLines;
			bool alternateLines = this.Config.Style.AlternateLines;
			bool flag7 = alternateLines;
			toolStripButton4.Checked = alternateLines;
			toolStripMenuItem5.Checked = flag7;
			this.ShowCodePane(this.Config.General.ActiveView);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void FormatCode(bool isPreview)
		{
			CodeSnippetConfig config = this.UIToConfig(isPreview);
			this.CodeSnippet = FormatHelper.Format(SupportedFormat.GetItemKey(this.cbLang.SelectedItem), config.Editor, config.Style, this.rtbCodeSnippet.Text, this.rtbCodeSnippet.TrimIndentOnPaste);
			if (isPreview)
			{
				this.wbPreview.DocumentText = this.CodeSnippet;
			}
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.sc1 = new SplitContainer();
			this.ts1 = new ToolStrip();
			this.cbLang = new ToolStripComboBox();
			this.toolStripSeparator1 = new ToolStripSeparator();
			this.btnUndo = new ToolStripButton();
			this.btnRedo = new ToolStripButton();
			this.toolStripSeparator5 = new ToolStripSeparator();
			this.btnCut = new ToolStripButton();
			this.btnCopy = new ToolStripButton();
			this.btnPaste = new ToolStripButton();
			this.btnClipboardClear = new ToolStripButton();
			this.toolStripSeparator2 = new ToolStripSeparator();
			this.btnDecIndent = new ToolStripButton();
			this.btnIncIndent = new ToolStripButton();
			this.toolStripSeparator4 = new ToolStripSeparator();
			this.btnWordWrap = new ToolStripButton();
			this.rtbCodeSnippet = new RichTextBoxEx();
			this.lblCode = new Label();
			this.ts2 = new ToolStrip();
			this.btnLineNumbers = new ToolStripButton();
			this.btnAlternateLines = new ToolStripButton();
			this.btnEmbedStyles = new ToolStripButton();
			this.btnUseContainer = new ToolStripButton();
			this.toolStripSeparator3 = new ToolStripSeparator();
			this.btnViewHtml = new ToolStripButton();
			this.wbPreview = new WebBrowser();
			this.lblFormattedCode = new Label();
			this.btnCancel = new Button();
			this.btnOK = new Button();
			this.ms1 = new MenuStrip();
			this.miFile = new ToolStripMenuItem();
			this.miFileRunSilent = new ToolStripMenuItem();
			this.toolStripSeparator = new ToolStripSeparator();
			this.miFileClose = new ToolStripMenuItem();
			this.miEdit = new ToolStripMenuItem();
			this.miEditUndo = new ToolStripMenuItem();
			this.miEditRedo = new ToolStripMenuItem();
			this.toolStripSeparator8 = new ToolStripSeparator();
			this.miEditCut = new ToolStripMenuItem();
			this.miEditCopy = new ToolStripMenuItem();
			this.miEditPaste = new ToolStripMenuItem();
			this.toolStripSeparator9 = new ToolStripSeparator();
			this.miEditSelectAll = new ToolStripMenuItem();
			this.toolStripSeparator6 = new ToolStripSeparator();
			this.miEditClearClipboard = new ToolStripMenuItem();
			this.toolStripSeparator7 = new ToolStripSeparator();
			this.miEditLineIndent = new ToolStripMenuItem();
			this.miEditLineIndentDecrease = new ToolStripMenuItem();
			this.miEditLineIndentIncrease = new ToolStripMenuItem();
			this.toolStripSeparator10 = new ToolStripSeparator();
			this.miEditTrimOnPaste = new ToolStripMenuItem();
			this.miView = new ToolStripMenuItem();
			this.miViewCompactMode = new ToolStripMenuItem();
			this.toolStripSeparator15 = new ToolStripSeparator();
			this.miViewWrap = new ToolStripMenuItem();
			this.toolStripSeparator12 = new ToolStripSeparator();
			this.miViewCodeSnippet = new ToolStripMenuItem();
			this.miViewFormattedCodeSnippet = new ToolStripMenuItem();
			this.miViewBoth = new ToolStripMenuItem();
			this.toolStripSeparator11 = new ToolStripSeparator();
			this.miViewHtml = new ToolStripMenuItem();
			this.miFormat = new ToolStripMenuItem();
			this.miFormatLineNumbers = new ToolStripMenuItem();
			this.miFormatAlternateLines = new ToolStripMenuItem();
			this.miFormatEmbedStyles = new ToolStripMenuItem();
			this.miFormatUseContainer = new ToolStripMenuItem();
			this.toolStripSeparator13 = new ToolStripSeparator();
			this.miCopyStylesToClipboard = new ToolStripMenuItem();
			this.miTools = new ToolStripMenuItem();
			this.miToolsOptions = new ToolStripMenuItem();
			this.miHelp = new ToolStripMenuItem();
			this.miHelpReadme = new ToolStripMenuItem();
			this.toolStripSeparator14 = new ToolStripSeparator();
			this.miHelpVisitMyBlog = new ToolStripMenuItem();
			this.toolStripSeparator16 = new ToolStripSeparator();
			this.miHelpAbout = new ToolStripMenuItem();
			this.btnMyBlog = new Button();
			this.tp1 = new ToolTip(this.components);
			this.sc1.Panel1.SuspendLayout();
			this.sc1.Panel2.SuspendLayout();
			this.sc1.SuspendLayout();
			this.ts1.SuspendLayout();
			this.ts2.SuspendLayout();
			this.ms1.SuspendLayout();
			base.SuspendLayout();
			this.sc1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.sc1.BorderStyle = BorderStyle.Fixed3D;
			this.sc1.Location = new Point(5, 28);
			this.sc1.Name = "sc1";
			this.sc1.Orientation = Orientation.Horizontal;
			this.sc1.Panel1.Controls.Add(this.ts1);
			this.sc1.Panel1.Controls.Add(this.rtbCodeSnippet);
			this.sc1.Panel1.Controls.Add(this.lblCode);
			this.sc1.Panel2.Controls.Add(this.ts2);
			this.sc1.Panel2.Controls.Add(this.wbPreview);
			this.sc1.Panel2.Controls.Add(this.lblFormattedCode);
			this.sc1.Size = new System.Drawing.Size(613, 373);
			this.sc1.SplitterDistance = 147;
			this.sc1.TabIndex = 1;
			this.sc1.TabStop = false;
			this.ts1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.ts1.Dock = DockStyle.None;
			ToolStripItemCollection items = this.ts1.Items;
			ToolStripItem[] toolStripItemArray = new ToolStripItem[] { this.cbLang, this.toolStripSeparator1, this.btnUndo, this.btnRedo, this.toolStripSeparator5, this.btnCut, this.btnCopy, this.btnPaste, this.btnClipboardClear, this.toolStripSeparator2, this.btnDecIndent, this.btnIncIndent, this.toolStripSeparator4, this.btnWordWrap };
			items.AddRange(toolStripItemArray);
			this.ts1.Location = new Point(243, 0);
			this.ts1.Name = "ts1";
			this.ts1.Size = new System.Drawing.Size(366, 25);
			this.ts1.TabIndex = 3;
			this.ts1.Text = "Code Snippet ToolStrip";
			this.cbLang.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cbLang.Name = "cbLang";
			this.cbLang.Size = new System.Drawing.Size(121, 25);
			this.cbLang.ToolTipText = "Language";
			this.cbLang.SelectedIndexChanged += new EventHandler(this.ReformatCode);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			this.btnUndo.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnUndo.Enabled = false;
			this.btnUndo.Image = Resources.undo;
			this.btnUndo.ImageTransparentColor = Color.Magenta;
			this.btnUndo.Name = "btnUndo";
			this.btnUndo.Size = new System.Drawing.Size(23, 22);
			this.btnUndo.Text = "Undo";
			this.btnUndo.Click += new EventHandler(this.miEditUndo_Click);
			this.btnRedo.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnRedo.Enabled = false;
			this.btnRedo.Image = Resources.redo;
			this.btnRedo.ImageTransparentColor = Color.Magenta;
			this.btnRedo.Name = "btnRedo";
			this.btnRedo.Size = new System.Drawing.Size(23, 22);
			this.btnRedo.Text = "Redo";
			this.btnRedo.Click += new EventHandler(this.miEditRedo_Click);
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			this.btnCut.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnCut.Enabled = false;
			this.btnCut.Image = Resources.cut;
			this.btnCut.ImageTransparentColor = Color.Magenta;
			this.btnCut.Name = "btnCut";
			this.btnCut.Size = new System.Drawing.Size(23, 22);
			this.btnCut.Text = "Cut";
			this.btnCut.Click += new EventHandler(this.miEditCut_Click);
			this.btnCopy.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnCopy.Enabled = false;
			this.btnCopy.Image = Resources.copy;
			this.btnCopy.ImageTransparentColor = Color.Magenta;
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(23, 22);
			this.btnCopy.Text = "Copy";
			this.btnCopy.Click += new EventHandler(this.miEditCopy_Click);
			this.btnPaste.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnPaste.Enabled = false;
			this.btnPaste.Image = Resources.paste;
			this.btnPaste.ImageTransparentColor = Color.Magenta;
			this.btnPaste.Name = "btnPaste";
			this.btnPaste.Size = new System.Drawing.Size(23, 22);
			this.btnPaste.Text = "toolStripButton1";
			this.btnPaste.ToolTipText = "Paste";
			this.btnPaste.Click += new EventHandler(this.miEditPaste_Click);
			this.btnClipboardClear.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnClipboardClear.Enabled = false;
			this.btnClipboardClear.Image = Resources.clipboard_clear;
			this.btnClipboardClear.ImageTransparentColor = Color.Magenta;
			this.btnClipboardClear.Name = "btnClipboardClear";
			this.btnClipboardClear.Size = new System.Drawing.Size(23, 22);
			this.btnClipboardClear.Text = "Clear Clipboard";
			this.btnClipboardClear.ToolTipText = "Clear the contents of the clipboard";
			this.btnClipboardClear.Click += new EventHandler(this.miEditClearClipboard_Click);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			this.btnDecIndent.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnDecIndent.Enabled = false;
			this.btnDecIndent.Image = Resources.text_indent_remove;
			this.btnDecIndent.ImageTransparentColor = Color.Magenta;
			this.btnDecIndent.Name = "btnDecIndent";
			this.btnDecIndent.Size = new System.Drawing.Size(23, 22);
			this.btnDecIndent.Text = "Decrease Indent";
			this.btnDecIndent.ToolTipText = "Decrease line indent";
			this.btnDecIndent.Click += new EventHandler(this.miEditLineIndentDecrease_Click);
			this.btnIncIndent.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnIncIndent.Enabled = false;
			this.btnIncIndent.Image = Resources.text_indent;
			this.btnIncIndent.ImageTransparentColor = Color.Magenta;
			this.btnIncIndent.Name = "btnIncIndent";
			this.btnIncIndent.Size = new System.Drawing.Size(23, 22);
			this.btnIncIndent.Text = "Increase Indent";
			this.btnIncIndent.ToolTipText = "Increase line indent";
			this.btnIncIndent.Click += new EventHandler(this.miEditLineIndentIncrease_Click);
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			this.btnWordWrap.CheckOnClick = true;
			this.btnWordWrap.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnWordWrap.Image = Resources.page_word_wrap;
			this.btnWordWrap.ImageTransparentColor = Color.Magenta;
			this.btnWordWrap.Name = "btnWordWrap";
			this.btnWordWrap.Size = new System.Drawing.Size(23, 22);
			this.btnWordWrap.Text = "Word Wrap";
			this.btnWordWrap.ToolTipText = "Wrap text to window";
			this.btnWordWrap.Click += new EventHandler(this.btnWordWrap_Click);
			this.rtbCodeSnippet.AcceptsTab = true;
			this.rtbCodeSnippet.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.rtbCodeSnippet.BorderStyle = BorderStyle.None;
			this.rtbCodeSnippet.CausesValidation = false;
			this.rtbCodeSnippet.DetectUrls = false;
			this.rtbCodeSnippet.Font = new System.Drawing.Font("Courier New", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.rtbCodeSnippet.Location = new Point(1, 28);
			this.rtbCodeSnippet.Name = "rtbCodeSnippet";
			this.rtbCodeSnippet.Size = new System.Drawing.Size(608, 116);
			this.rtbCodeSnippet.TabIndex = 2;
			this.rtbCodeSnippet.TabSpaces = 4;
			this.rtbCodeSnippet.Text = "";
			this.rtbCodeSnippet.TrimIndentOnPaste = true;
			this.rtbCodeSnippet.WordWrap = false;
			this.rtbCodeSnippet.SelectionChanged += new EventHandler(this.rtbCodeSnippet_SelectionChanged);
			this.rtbCodeSnippet.TextChanged += new EventHandler(this.ReformatCode);
			this.lblCode.AutoSize = true;
			this.lblCode.Location = new Point(0, 7);
			this.lblCode.Name = "lblCode";
			this.lblCode.Size = new System.Drawing.Size(118, 13);
			this.lblCode.TabIndex = 0;
			this.lblCode.Text = "Code Snippet to Format";
			this.ts2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.ts2.Dock = DockStyle.None;
			ToolStripItemCollection toolStripItemCollections = this.ts2.Items;
			ToolStripItem[] toolStripItemArray1 = new ToolStripItem[] { this.btnLineNumbers, this.btnAlternateLines, this.btnEmbedStyles, this.btnUseContainer, this.toolStripSeparator3, this.btnViewHtml };
			toolStripItemCollections.AddRange(toolStripItemArray1);
			this.ts2.Location = new Point(163, 0);
			this.ts2.Name = "ts2";
			this.ts2.Size = new System.Drawing.Size(445, 25);
			this.ts2.TabIndex = 2;
			this.ts2.Text = "Formatted Code Snippet ToolStrip";
			this.btnLineNumbers.CheckOnClick = true;
			this.btnLineNumbers.Image = Resources.text_list_numbers;
			this.btnLineNumbers.ImageTransparentColor = Color.Magenta;
			this.btnLineNumbers.Name = "btnLineNumbers";
			this.btnLineNumbers.Size = new System.Drawing.Size(101, 22);
			this.btnLineNumbers.Text = "Line Numbers";
			this.btnLineNumbers.ToolTipText = "Include line numbers";
			this.btnLineNumbers.Click += new EventHandler(this.btnLineNumbers_Click);
			this.btnAlternateLines.CheckOnClick = true;
			this.btnAlternateLines.Image = Resources.alternate_line_bkg;
			this.btnAlternateLines.ImageTransparentColor = Color.Magenta;
			this.btnAlternateLines.Name = "btnAlternateLines";
			this.btnAlternateLines.Size = new System.Drawing.Size(105, 22);
			this.btnAlternateLines.Text = "Alternate Lines";
			this.btnAlternateLines.ToolTipText = "Alternate line background color";
			this.btnAlternateLines.Click += new EventHandler(this.btnAlternateLineBkg_Click);
			this.btnEmbedStyles.CheckOnClick = true;
			this.btnEmbedStyles.Image = Resources.style_add;
			this.btnEmbedStyles.ImageTransparentColor = Color.Magenta;
			this.btnEmbedStyles.Name = "btnEmbedStyles";
			this.btnEmbedStyles.Size = new System.Drawing.Size(97, 22);
			this.btnEmbedStyles.Text = "Embed Styles";
			this.btnEmbedStyles.ToolTipText = "Embed styles used to format the code";
			this.btnEmbedStyles.Click += new EventHandler(this.btnEmbedStyles_Click);
			this.btnUseContainer.CheckOnClick = true;
			this.btnUseContainer.Image = Resources.layers;
			this.btnUseContainer.ImageTransparentColor = Color.Magenta;
			this.btnUseContainer.Name = "btnUseContainer";
			this.btnUseContainer.Size = new System.Drawing.Size(101, 22);
			this.btnUseContainer.Text = "Use Container";
			this.btnUseContainer.ToolTipText = "Place code in a container";
			this.btnUseContainer.Click += new EventHandler(this.btnUseContainer_Click);
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			this.btnViewHtml.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.btnViewHtml.Enabled = false;
			this.btnViewHtml.Image = Resources.page_code;
			this.btnViewHtml.ImageTransparentColor = Color.Magenta;
			this.btnViewHtml.Name = "btnViewHtml";
			this.btnViewHtml.Size = new System.Drawing.Size(23, 22);
			this.btnViewHtml.Text = "View HTML";
			this.btnViewHtml.ToolTipText = "View the HTML markup that will be inserted";
			this.btnViewHtml.Click += new EventHandler(this.btnViewHtml_Click);
			this.wbPreview.AllowWebBrowserDrop = false;
			this.wbPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.wbPreview.CausesValidation = false;
			this.wbPreview.IsWebBrowserContextMenuEnabled = false;
			this.wbPreview.Location = new Point(1, 28);
			this.wbPreview.MinimumSize = new System.Drawing.Size(20, 20);
			this.wbPreview.Name = "wbPreview";
			this.wbPreview.ScriptErrorsSuppressed = true;
			this.wbPreview.Size = new System.Drawing.Size(608, 187);
			this.wbPreview.TabIndex = 1;
			this.wbPreview.TabStop = false;
			this.wbPreview.WebBrowserShortcutsEnabled = false;
			this.wbPreview.PreviewKeyDown += new PreviewKeyDownEventHandler(this.wbPreview_PreviewKeyDown);
			this.wbPreview.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.wbPreview_DocumentCompleted);
			this.lblFormattedCode.AutoSize = true;
			this.lblFormattedCode.Location = new Point(1, 7);
			this.lblFormattedCode.Name = "lblFormattedCode";
			this.lblFormattedCode.Size = new System.Drawing.Size(121, 13);
			this.lblFormattedCode.TabIndex = 0;
			this.lblFormattedCode.Text = "Formatted Code Snippet";
			this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnCancel.AutoSize = true;
			this.btnCancel.CausesValidation = false;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Image = Resources.cancel;
			this.btnCancel.Location = new Point(531, 409);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(87, 23);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Don't Insert";
			this.btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnOK.AutoSize = true;
			this.btnOK.CausesValidation = false;
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Image = Resources.accept;
			this.btnOK.Location = new Point(450, 409);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "Insert";
			this.btnOK.TextImageRelation = TextImageRelation.ImageBeforeText;
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new EventHandler(this.btnOK_Click);
			ToolStripItemCollection items1 = this.ms1.Items;
			ToolStripItem[] toolStripItemArray2 = new ToolStripItem[] { this.miFile, this.miEdit, this.miView, this.miFormat, this.miTools, this.miHelp };
			items1.AddRange(toolStripItemArray2);
			this.ms1.Location = new Point(0, 0);
			this.ms1.Name = "ms1";
			this.ms1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.ms1.Size = new System.Drawing.Size(624, 24);
			this.ms1.TabIndex = 4;
			this.ms1.Text = "Top Menu";
			ToolStripItemCollection dropDownItems = this.miFile.DropDownItems;
			ToolStripItem[] toolStripItemArray3 = new ToolStripItem[] { this.miFileRunSilent, this.toolStripSeparator, this.miFileClose };
			dropDownItems.AddRange(toolStripItemArray3);
			this.miFile.Name = "miFile";
			this.miFile.Size = new System.Drawing.Size(37, 20);
			this.miFile.Text = "&File";
			this.miFileRunSilent.CheckOnClick = true;
			this.miFileRunSilent.Name = "miFileRunSilent";
			this.miFileRunSilent.Size = new System.Drawing.Size(145, 22);
			this.miFileRunSilent.Text = "Run &Silent";
			this.miFileRunSilent.Click += new EventHandler(this.miFileRunSilent_Click);
			this.toolStripSeparator.Name = "toolStripSeparator";
			this.toolStripSeparator.Size = new System.Drawing.Size(142, 6);
			this.miFileClose.Name = "miFileClose";
			this.miFileClose.ShortcutKeys = Keys.LButton | Keys.RButton | Keys.Cancel | Keys.ShiftKey | Keys.ControlKey | Keys.Menu | Keys.Pause | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.D0 | Keys.D1 | Keys.D2 | Keys.D3 | Keys.A | Keys.B | Keys.C | Keys.P | Keys.Q | Keys.R | Keys.S | Keys.NumPad0 | Keys.NumPad1 | Keys.NumPad2 | Keys.NumPad3 | Keys.F1 | Keys.F2 | Keys.F3 | Keys.F4 | Keys.Alt;
			this.miFileClose.Size = new System.Drawing.Size(145, 22);
			this.miFileClose.Text = "&Close";
			this.miFileClose.Click += new EventHandler(this.miFileClose_Click);
			ToolStripItemCollection dropDownItems1 = this.miEdit.DropDownItems;
			ToolStripItem[] toolStripItemArray4 = new ToolStripItem[] { this.miEditUndo, this.miEditRedo, this.toolStripSeparator8, this.miEditCut, this.miEditCopy, this.miEditPaste, this.toolStripSeparator9, this.miEditSelectAll, this.toolStripSeparator6, this.miEditClearClipboard, this.toolStripSeparator7, this.miEditLineIndent };
			dropDownItems1.AddRange(toolStripItemArray4);
			this.miEdit.Name = "miEdit";
			this.miEdit.Size = new System.Drawing.Size(39, 20);
			this.miEdit.Text = "&Edit";
			this.miEditUndo.Image = Resources.undo;
			this.miEditUndo.Name = "miEditUndo";
			this.miEditUndo.ShortcutKeys = Keys.RButton | Keys.Back | Keys.LineFeed | Keys.ShiftKey | Keys.Menu | Keys.FinalMode | Keys.B | Keys.H | Keys.J | Keys.P | Keys.R | Keys.X | Keys.Z | Keys.Control;
			this.miEditUndo.Size = new System.Drawing.Size(239, 22);
			this.miEditUndo.Text = "&Undo";
			this.miEditUndo.Click += new EventHandler(this.miEditUndo_Click);
			this.miEditRedo.Image = Resources.redo;
			this.miEditRedo.Name = "miEditRedo";
			this.miEditRedo.ShortcutKeys = Keys.LButton | Keys.Back | Keys.Tab | Keys.ShiftKey | Keys.ControlKey | Keys.FinalMode | Keys.HanjaMode | Keys.KanjiMode | Keys.A | Keys.H | Keys.I | Keys.P | Keys.Q | Keys.X | Keys.Y | Keys.Control;
			this.miEditRedo.Size = new System.Drawing.Size(239, 22);
			this.miEditRedo.Text = "&Redo";
			this.miEditRedo.Click += new EventHandler(this.miEditRedo_Click);
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(236, 6);
			this.miEditCut.Image = Resources.cut;
			this.miEditCut.ImageTransparentColor = Color.Magenta;
			this.miEditCut.Name = "miEditCut";
			this.miEditCut.ShortcutKeys = Keys.Back | Keys.ShiftKey | Keys.FinalMode | Keys.H | Keys.P | Keys.X | Keys.Control;
			this.miEditCut.Size = new System.Drawing.Size(239, 22);
			this.miEditCut.Text = "Cu&t";
			this.miEditCut.Click += new EventHandler(this.miEditCut_Click);
			this.miEditCopy.Image = Resources.copy;
			this.miEditCopy.ImageTransparentColor = Color.Magenta;
			this.miEditCopy.Name = "miEditCopy";
			this.miEditCopy.ShortcutKeys = Keys.LButton | Keys.RButton | Keys.Cancel | Keys.A | Keys.B | Keys.C | Keys.Control;
			this.miEditCopy.Size = new System.Drawing.Size(239, 22);
			this.miEditCopy.Text = "&Copy";
			this.miEditCopy.Click += new EventHandler(this.miEditCopy_Click);
			this.miEditPaste.Image = Resources.paste;
			this.miEditPaste.ImageTransparentColor = Color.Magenta;
			this.miEditPaste.Name = "miEditPaste";
			this.miEditPaste.ShortcutKeys = Keys.RButton | Keys.MButton | Keys.XButton2 | Keys.ShiftKey | Keys.Menu | Keys.Capital | Keys.CapsLock | Keys.B | Keys.D | Keys.F | Keys.P | Keys.R | Keys.T | Keys.V | Keys.Control;
			this.miEditPaste.Size = new System.Drawing.Size(239, 22);
			this.miEditPaste.Text = "&Paste";
			this.miEditPaste.Click += new EventHandler(this.miEditPaste_Click);
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(236, 6);
			this.miEditSelectAll.Name = "miEditSelectAll";
			this.miEditSelectAll.ShortcutKeys = Keys.LButton | Keys.A | Keys.Control;
			this.miEditSelectAll.Size = new System.Drawing.Size(239, 22);
			this.miEditSelectAll.Text = "Select &All";
			this.miEditSelectAll.Click += new EventHandler(this.miEditSelectAll_Click);
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(236, 6);
			this.miEditClearClipboard.Image = Resources.clipboard_clear;
			this.miEditClearClipboard.Name = "miEditClearClipboard";
			this.miEditClearClipboard.ShortcutKeys = Keys.RButton | Keys.MButton | Keys.XButton2 | Keys.Back | Keys.LineFeed | Keys.Clear | Keys.Space | Keys.Next | Keys.PageDown | Keys.Home | Keys.Up | Keys.Down | Keys.Print | Keys.Snapshot | Keys.PrintScreen | Keys.Delete | Keys.Shift | Keys.Control;
			this.miEditClearClipboard.Size = new System.Drawing.Size(239, 22);
			this.miEditClearClipboard.Text = "Cl&ear Clipboard";
			this.miEditClearClipboard.Click += new EventHandler(this.miEditClearClipboard_Click);
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(236, 6);
			ToolStripItemCollection toolStripItemCollections1 = this.miEditLineIndent.DropDownItems;
			ToolStripItem[] toolStripItemArray5 = new ToolStripItem[] { this.miEditLineIndentDecrease, this.miEditLineIndentIncrease, this.toolStripSeparator10, this.miEditTrimOnPaste };
			toolStripItemCollections1.AddRange(toolStripItemArray5);
			this.miEditLineIndent.Name = "miEditLineIndent";
			this.miEditLineIndent.Size = new System.Drawing.Size(239, 22);
			this.miEditLineIndent.Text = "&Line Indent";
			this.miEditLineIndentDecrease.Image = Resources.text_indent_remove;
			this.miEditLineIndentDecrease.Name = "miEditLineIndentDecrease";
			this.miEditLineIndentDecrease.Size = new System.Drawing.Size(147, 22);
			this.miEditLineIndentDecrease.Text = "&Decrease";
			this.miEditLineIndentDecrease.Click += new EventHandler(this.miEditLineIndentDecrease_Click);
			this.miEditLineIndentIncrease.Image = Resources.text_indent;
			this.miEditLineIndentIncrease.Name = "miEditLineIndentIncrease";
			this.miEditLineIndentIncrease.Size = new System.Drawing.Size(147, 22);
			this.miEditLineIndentIncrease.Text = "&Increase";
			this.miEditLineIndentIncrease.Click += new EventHandler(this.miEditLineIndentIncrease_Click);
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(144, 6);
			this.miEditTrimOnPaste.Checked = true;
			this.miEditTrimOnPaste.CheckOnClick = true;
			this.miEditTrimOnPaste.CheckState = CheckState.Checked;
			this.miEditTrimOnPaste.Name = "miEditTrimOnPaste";
			this.miEditTrimOnPaste.Size = new System.Drawing.Size(147, 22);
			this.miEditTrimOnPaste.Text = "&Trim on Paste";
			this.miEditTrimOnPaste.Click += new EventHandler(this.miEditTrimIndentOnPaste_Click);
			ToolStripItemCollection dropDownItems2 = this.miView.DropDownItems;
			ToolStripItem[] toolStripItemArray6 = new ToolStripItem[] { this.miViewCompactMode, this.toolStripSeparator15, this.miViewWrap, this.toolStripSeparator12, this.miViewCodeSnippet, this.miViewFormattedCodeSnippet, this.miViewBoth, this.toolStripSeparator11, this.miViewHtml };
			dropDownItems2.AddRange(toolStripItemArray6);
			this.miView.Name = "miView";
			this.miView.Size = new System.Drawing.Size(44, 20);
			this.miView.Text = "&View";
			this.miViewCompactMode.Name = "miViewCompactMode";
			this.miViewCompactMode.ShortcutKeys = Keys.F12;
			this.miViewCompactMode.Size = new System.Drawing.Size(269, 22);
			this.miViewCompactMode.Text = "&Compact Mode";
			this.miViewCompactMode.Click += new EventHandler(this.miViewCompactMode_Click);
			this.toolStripSeparator15.Name = "toolStripSeparator15";
			this.toolStripSeparator15.Size = new System.Drawing.Size(266, 6);
			this.miViewWrap.CheckOnClick = true;
			this.miViewWrap.Image = Resources.page_word_wrap;
			this.miViewWrap.Name = "miViewWrap";
			this.miViewWrap.Size = new System.Drawing.Size(269, 22);
			this.miViewWrap.Text = "&Wrap";
			this.miViewWrap.Click += new EventHandler(this.miViewWrap_Click);
			this.toolStripSeparator12.Name = "toolStripSeparator12";
			this.toolStripSeparator12.Size = new System.Drawing.Size(266, 6);
			this.miViewCodeSnippet.CheckOnClick = true;
			this.miViewCodeSnippet.Name = "miViewCodeSnippet";
			this.miViewCodeSnippet.ShortcutKeys = Keys.LButton | Keys.ShiftKey | Keys.ControlKey | Keys.Space | Keys.Prior | Keys.PageUp | Keys.D0 | Keys.D1 | Keys.Control;
			this.miViewCodeSnippet.Size = new System.Drawing.Size(269, 22);
			this.miViewCodeSnippet.Text = "C&ode Snippet";
			this.miViewCodeSnippet.Click += new EventHandler(this.miViewCodeSnippet_Click);
			this.miViewFormattedCodeSnippet.CheckOnClick = true;
			this.miViewFormattedCodeSnippet.Name = "miViewFormattedCodeSnippet";
			this.miViewFormattedCodeSnippet.ShortcutKeys = Keys.RButton | Keys.ShiftKey | Keys.Menu | Keys.Space | Keys.Next | Keys.PageDown | Keys.D0 | Keys.D2 | Keys.Control;
			this.miViewFormattedCodeSnippet.Size = new System.Drawing.Size(269, 22);
			this.miViewFormattedCodeSnippet.Text = "For&matted Code Snippet";
			this.miViewFormattedCodeSnippet.Click += new EventHandler(this.miViewFormattedCodeSnippet_Click);
			this.miViewBoth.Checked = true;
			this.miViewBoth.CheckOnClick = true;
			this.miViewBoth.CheckState = CheckState.Checked;
			this.miViewBoth.Name = "miViewBoth";
			this.miViewBoth.ShortcutKeys = Keys.LButton | Keys.RButton | Keys.Cancel | Keys.ShiftKey | Keys.ControlKey | Keys.Menu | Keys.Pause | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.D0 | Keys.D1 | Keys.D2 | Keys.D3 | Keys.Control;
			this.miViewBoth.Size = new System.Drawing.Size(269, 22);
			this.miViewBoth.Text = "&Both";
			this.miViewBoth.Click += new EventHandler(this.miViewBoth_Click);
			this.toolStripSeparator11.Name = "toolStripSeparator11";
			this.toolStripSeparator11.Size = new System.Drawing.Size(266, 6);
			this.miViewHtml.Name = "miViewHtml";
			this.miViewHtml.ShortcutKeys = Keys.ShiftKey | Keys.P | Keys.Control | Keys.Alt;
			this.miViewHtml.Size = new System.Drawing.Size(269, 22);
			this.miViewHtml.Text = "Formatted Code &HTML...";
			this.miViewHtml.Click += new EventHandler(this.btnViewHtml_Click);
			ToolStripItemCollection toolStripItemCollections2 = this.miFormat.DropDownItems;
			ToolStripItem[] toolStripItemArray7 = new ToolStripItem[] { this.miFormatLineNumbers, this.miFormatAlternateLines, this.miFormatEmbedStyles, this.miFormatUseContainer, this.toolStripSeparator13, this.miCopyStylesToClipboard };
			toolStripItemCollections2.AddRange(toolStripItemArray7);
			this.miFormat.Name = "miFormat";
			this.miFormat.Size = new System.Drawing.Size(57, 20);
			this.miFormat.Text = "F&ormat";
			this.miFormatLineNumbers.CheckOnClick = true;
			this.miFormatLineNumbers.Image = Resources.text_list_numbers;
			this.miFormatLineNumbers.Name = "miFormatLineNumbers";
			this.miFormatLineNumbers.Size = new System.Drawing.Size(204, 22);
			this.miFormatLineNumbers.Text = "&Line Numbers";
			this.miFormatLineNumbers.Click += new EventHandler(this.miFormatLineNumbers_Click);
			this.miFormatAlternateLines.CheckOnClick = true;
			this.miFormatAlternateLines.Image = Resources.alternate_line_bkg;
			this.miFormatAlternateLines.Name = "miFormatAlternateLines";
			this.miFormatAlternateLines.Size = new System.Drawing.Size(204, 22);
			this.miFormatAlternateLines.Text = "&AlternateLines Lines";
			this.miFormatAlternateLines.Click += new EventHandler(this.miFormatAlternateLines_Click);
			this.miFormatEmbedStyles.CheckOnClick = true;
			this.miFormatEmbedStyles.Image = Resources.style_add;
			this.miFormatEmbedStyles.Name = "miFormatEmbedStyles";
			this.miFormatEmbedStyles.Size = new System.Drawing.Size(204, 22);
			this.miFormatEmbedStyles.Text = "&Embed Styles";
			this.miFormatEmbedStyles.Click += new EventHandler(this.miFormatEmbedStyles_Click);
			this.miFormatUseContainer.CheckOnClick = true;
			this.miFormatUseContainer.Image = Resources.layers;
			this.miFormatUseContainer.Name = "miFormatUseContainer";
			this.miFormatUseContainer.Size = new System.Drawing.Size(204, 22);
			this.miFormatUseContainer.Text = "&Use Container";
			this.miFormatUseContainer.Click += new EventHandler(this.miFormatUseContainer_Click);
			this.toolStripSeparator13.Name = "toolStripSeparator13";
			this.toolStripSeparator13.Size = new System.Drawing.Size(201, 6);
			this.miCopyStylesToClipboard.Name = "miCopyStylesToClipboard";
			this.miCopyStylesToClipboard.Size = new System.Drawing.Size(204, 22);
			this.miCopyStylesToClipboard.Text = "&Copy Styles to Clipboard";
			this.miCopyStylesToClipboard.Click += new EventHandler(this.miCopyStylesToClipboard_Click);
			this.miTools.DropDownItems.AddRange(new ToolStripItem[] { this.miToolsOptions });
			this.miTools.Name = "miTools";
			this.miTools.Size = new System.Drawing.Size(48, 20);
			this.miTools.Text = "&Tools";
			this.miToolsOptions.Image = Resources.options;
			this.miToolsOptions.Name = "miToolsOptions";
			this.miToolsOptions.Size = new System.Drawing.Size(152, 22);
			this.miToolsOptions.Text = "&Options...";
			this.miToolsOptions.Click += new EventHandler(this.miToolsOptions_Click);
			ToolStripItemCollection dropDownItems3 = this.miHelp.DropDownItems;
			ToolStripItem[] toolStripItemArray8 = new ToolStripItem[] { this.miHelpReadme, this.toolStripSeparator14, this.miHelpVisitMyBlog, this.toolStripSeparator16, this.miHelpAbout };
			dropDownItems3.AddRange(toolStripItemArray8);
			this.miHelp.Name = "miHelp";
			this.miHelp.Size = new System.Drawing.Size(44, 20);
			this.miHelp.Text = "&Help";
			this.miHelpReadme.Image = Resources.help;
			this.miHelpReadme.Name = "miHelpReadme";
			this.miHelpReadme.ShortcutKeys = Keys.F1;
			this.miHelpReadme.Size = new System.Drawing.Size(181, 22);
			this.miHelpReadme.Text = "&Readme...";
			this.miHelpReadme.Click += new EventHandler(this.miHelpReadme_Click);
			this.toolStripSeparator14.Name = "toolStripSeparator14";
			this.toolStripSeparator14.Size = new System.Drawing.Size(178, 6);
			this.miHelpVisitMyBlog.Image = Resources.blog_logo;
			this.miHelpVisitMyBlog.Name = "miHelpVisitMyBlog";
			this.miHelpVisitMyBlog.Size = new System.Drawing.Size(181, 22);
			this.miHelpVisitMyBlog.Text = "&Visit My Blog...";
			this.miHelpVisitMyBlog.Click += new EventHandler(this.miHelpVisitMyBlog_Click);
			this.toolStripSeparator16.Name = "toolStripSeparator16";
			this.toolStripSeparator16.Size = new System.Drawing.Size(178, 6);
			this.miHelpAbout.Name = "miHelpAbout";
			this.miHelpAbout.Size = new System.Drawing.Size(181, 22);
			this.miHelpAbout.Text = "&About Code Snippet";
			this.miHelpAbout.Click += new EventHandler(this.miHelpAbout_Click);
			this.btnMyBlog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			this.btnMyBlog.AutoSize = true;
			this.btnMyBlog.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnMyBlog.FlatAppearance.BorderSize = 0;
			this.btnMyBlog.FlatStyle = FlatStyle.Flat;
			this.btnMyBlog.Image = Resources.blog_logo;
			this.btnMyBlog.Location = new Point(8, 403);
			this.btnMyBlog.Name = "btnMyBlog";
			this.btnMyBlog.Size = new System.Drawing.Size(28, 38);
			this.btnMyBlog.TabIndex = 5;
			this.tp1.SetToolTip(this.btnMyBlog, "Visit my blog");
			this.btnMyBlog.UseVisualStyleBackColor = true;
			this.btnMyBlog.Click += new EventHandler(this.miHelpVisitMyBlog_Click);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			base.CausesValidation = false;
			base.ClientSize = new System.Drawing.Size(624, 442);
			base.Controls.Add(this.btnMyBlog);
			base.Controls.Add(this.btnOK);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.sc1);
			base.Controls.Add(this.ms1);
			base.MainMenuStrip = this.ms1;
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(640, 480);
			base.Name = "CodeSnippetForm";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Code Snippet";
			base.Load += new EventHandler(this.CodeSnippetForm_Load);
			base.Activated += new EventHandler(this.CodeSnippetForm_Activated);
			base.FormClosing += new FormClosingEventHandler(this.CodeSnippetForm_FormClosing);
			this.sc1.Panel1.ResumeLayout(false);
			this.sc1.Panel1.PerformLayout();
			this.sc1.Panel2.ResumeLayout(false);
			this.sc1.Panel2.PerformLayout();
			this.sc1.ResumeLayout(false);
			this.ts1.ResumeLayout(false);
			this.ts1.PerformLayout();
			this.ts2.ResumeLayout(false);
			this.ts2.PerformLayout();
			this.ms1.ResumeLayout(false);
			this.ms1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void InitializeList()
		{
			if (this.cbLang.ComboBox != null)
			{
				this.cbLang.ComboBox.DataSource = SupportedFormat.Items;
			}
		}

		private void miCopyStylesToClipboard_Click(object sender, EventArgs e)
		{
			MessageBoxOptions messageBoxOption;
			Clipboard.SetText(SourceFormat.CssString);
			string copyStyles = Resources.CopyStyles;
			string text = this.Text;
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				messageBoxOption = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}
			else
			{
				messageBoxOption = (MessageBoxOptions)0;
			}
			MessageBox.Show(this, copyStyles, text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, messageBoxOption);
		}

		private void miEditClearClipboard_Click(object sender, EventArgs e)
		{
			Clipboard.Clear();
			this.UpdateButtonStates();
		}

		private void miEditCopy_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.Copy();
		}

		private void miEditCut_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.Cut();
			this.UpdateButtonStates();
		}

		private void miEditLineIndentDecrease_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.DecIndent();
		}

		private void miEditLineIndentIncrease_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.IncIndent();
		}

		private void miEditPaste_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.Paste();
			this.UpdateButtonStates();
		}

		private void miEditRedo_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.Redo();
		}

		private void miEditSelectAll_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.SelectAll();
		}

		private void miEditTrimIndentOnPaste_Click(object sender, EventArgs e)
		{
			EditorConfig editor = this.Config.Editor;
			RichTextBoxEx richTextBoxEx = this.rtbCodeSnippet;
			bool @checked = this.miEditTrimOnPaste.Checked;
			bool flag = @checked;
			richTextBoxEx.TrimIndentOnPaste = @checked;
			editor.TrimIndentOnPaste = flag;
		}

		private void miEditUndo_Click(object sender, EventArgs e)
		{
			this.rtbCodeSnippet.Undo();
		}

		private void miFileClose_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		private void miFileRunSilent_Click(object sender, EventArgs e)
		{
			MessageBoxOptions messageBoxOption;
			string runningSilent = Resources.RunningSilent;
			string text = this.Text;
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				messageBoxOption = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}
			else
			{
				messageBoxOption = (MessageBoxOptions)0;
			}
			if (System.Windows.Forms.DialogResult.Yes != MessageBox.Show(this, runningSilent, text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, messageBoxOption))
			{
				this.miFileRunSilent.Checked = false;
				return;
			}
			this.Config.General.RunSilent = this.miFileRunSilent.Checked;
			base.Close();
		}

		private void miFormatAlternateLines_Click(object sender, EventArgs e)
		{
			this.btnAlternateLines.Checked = this.miFormatAlternateLines.Checked;
			this.FormatCode(true);
		}

		private void miFormatEmbedStyles_Click(object sender, EventArgs e)
		{
			this.btnEmbedStyles.Checked = this.miFormatEmbedStyles.Checked;
		}

		private void miFormatLineNumbers_Click(object sender, EventArgs e)
		{
			this.btnLineNumbers.Checked = this.miFormatLineNumbers.Checked;
			this.FormatCode(true);
		}

		private void miFormatUseContainer_Click(object sender, EventArgs e)
		{
			this.btnUseContainer.Checked = this.miFormatUseContainer.Checked;
			this.FormatCode(true);
		}

		private void miHelpAbout_Click(object sender, EventArgs e)
		{
			(new CodeSnippetAboutForm()).ShowDialog(this);
		}

		private void miHelpReadme_Click(object sender, EventArgs e)
		{
			MessageBoxOptions messageBoxOption;
			try
			{
				Process.Start(CodeSnippetForm.ReadmeFile);
			}
			catch (Exception exception)
			{
				string message = exception.Message;
				string text = this.Text;
				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
				{
					messageBoxOption = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
				}
				else
				{
					messageBoxOption = (MessageBoxOptions)0;
				}
				MessageBox.Show(this, message, text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, messageBoxOption);
			}
		}

		private void miHelpVisitMyBlog_Click(object sender, EventArgs e)
		{
			Process.Start("http://lvildosola.blogspot.com");
		}

		private void miToolsOptions_Click(object sender, EventArgs e)
		{
			this.Config = this.UIToConfig(false);
			CodeSnippetOptionsForm codeSnippetOptionsForm = new CodeSnippetOptionsForm(this.Config);
			if (System.Windows.Forms.DialogResult.OK == codeSnippetOptionsForm.ShowDialog(this))
			{
				this.Config = codeSnippetOptionsForm.Config;
				this.ConfigToUI();
				return;
			}
			this.Config.Layout.OptionsPlacement = codeSnippetOptionsForm.Config.Layout.OptionsPlacement;
		}

		private void miViewBoth_Click(object sender, EventArgs e)
		{
			this.ShowCodePane(CodeSnippetViewType.Both);
		}

		private void miViewCodeSnippet_Click(object sender, EventArgs e)
		{
			this.ShowCodePane(CodeSnippetViewType.CodeSnippet);
		}

		private void miViewCompactMode_Click(object sender, EventArgs e)
		{
			this.Config.General.ActiveMode = CodeSnippetViewMode.Compact;
			base.DialogResult = System.Windows.Forms.DialogResult.Retry;
			base.Close();
		}

		private void miViewFormattedCodeSnippet_Click(object sender, EventArgs e)
		{
			this.ShowCodePane(CodeSnippetViewType.FormattedCodeSnippet);
		}

		private void miViewWrap_Click(object sender, EventArgs e)
		{
			EditorConfig editor = this.Config.Editor;
			RichTextBoxEx richTextBoxEx = this.rtbCodeSnippet;
			ToolStripButton toolStripButton = this.btnWordWrap;
			bool @checked = this.miViewWrap.Checked;
			bool flag = @checked;
			toolStripButton.Checked = @checked;
			bool flag1 = flag;
			bool flag2 = flag1;
			richTextBoxEx.WordWrap = flag1;
			editor.WordWrap = flag2;
		}

		private void ReformatCode(object sender, EventArgs e)
		{
			if (this.isInitialized)
			{
				this.FormatCode(true);
			}
		}

		private void rtbCodeSnippet_SelectionChanged(object sender, EventArgs e)
		{
			this.UpdateButtonStates();
		}

		private void ShowCodePane(CodeSnippetViewType type)
		{
			this.Config.General.ActiveView = type;
			switch (type)
			{
				case CodeSnippetViewType.CodeSnippet:
				{
					ToolStripMenuItem toolStripMenuItem = this.miViewFormattedCodeSnippet;
					int num = 0;
					bool flag = (bool)(num==1);
					this.miViewBoth.Checked = (bool)(num==1);
					toolStripMenuItem.Checked = flag;
					this.sc1.Panel1Collapsed = false;
					this.sc1.Panel2Collapsed = true;
					return;
				}
				case CodeSnippetViewType.FormattedCodeSnippet:
				{
					this.Config.General.ActiveView = CodeSnippetViewType.FormattedCodeSnippet;
					ToolStripMenuItem toolStripMenuItem1 = this.miViewCodeSnippet;
					int num1 = 0;
					bool flag1 = (bool)(num1==1);
					this.miViewBoth.Checked = (bool)(num1==1);
					toolStripMenuItem1.Checked = flag1;
					this.sc1.Panel1Collapsed = true;
					this.sc1.Panel2Collapsed = false;
					return;
				}
				case CodeSnippetViewType.Both:
				{
					this.Config.General.ActiveView = CodeSnippetViewType.Both;
					ToolStripMenuItem toolStripMenuItem2 = this.miViewCodeSnippet;
					int num2 = 0;
                    bool flag2 = (bool)(num2 == 1);
					this.miViewFormattedCodeSnippet.Checked = (bool)(num2==1);
					toolStripMenuItem2.Checked = flag2;
					this.sc1.Panel1Collapsed = false;
					this.sc1.Panel2Collapsed = false;
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private CodeSnippetConfig UIToConfig(bool isPreview)
		{
			CodeSnippetConfig codeSnippetConfig = new CodeSnippetConfig(this.Config);
			codeSnippetConfig.General.ActiveLanguage = SupportedFormat.GetItemKey(this.cbLang.SelectedItem);
			codeSnippetConfig.Layout.FullModePlacement = Win32WndHelper.GetPlacement(base.Handle);
			codeSnippetConfig.Style.AlternateLines = this.btnAlternateLines.Checked;
			codeSnippetConfig.Style.EmbedStyles = (isPreview ? true : this.btnEmbedStyles.Checked);
			codeSnippetConfig.Style.LineNumbers = this.btnLineNumbers.Checked;
			codeSnippetConfig.Style.UseContainer = this.btnUseContainer.Checked;
			return codeSnippetConfig;
		}

		private void UpdateButtonStates()
		{
			ToolStripMenuItem toolStripMenuItem = this.miEditUndo;
			ToolStripButton toolStripButton = this.btnUndo;
			bool canUndo = this.rtbCodeSnippet.CanUndo;
			bool flag = canUndo;
			toolStripButton.Enabled = canUndo;
			toolStripMenuItem.Enabled = flag;
			ToolStripMenuItem toolStripMenuItem1 = this.miEditRedo;
			ToolStripButton toolStripButton1 = this.btnRedo;
			bool canRedo = this.rtbCodeSnippet.CanRedo;
			bool flag1 = canRedo;
			toolStripButton1.Enabled = canRedo;
			toolStripMenuItem1.Enabled = flag1;
			ToolStripMenuItem toolStripMenuItem2 = this.miEditCut;
			ToolStripButton toolStripButton2 = this.btnCut;
			ToolStripMenuItem toolStripMenuItem3 = this.miEditCopy;
			ToolStripButton toolStripButton3 = this.btnCopy;
			ToolStripMenuItem toolStripMenuItem4 = this.miEditLineIndentDecrease;
			ToolStripButton toolStripButton4 = this.btnDecIndent;
			ToolStripMenuItem toolStripMenuItem5 = this.miEditLineIndentIncrease;
			ToolStripButton toolStripButton5 = this.btnIncIndent;
			bool hasSelectedText = this.rtbCodeSnippet.HasSelectedText;
			bool flag2 = hasSelectedText;
			toolStripButton5.Enabled = hasSelectedText;
			bool flag3 = flag2;
			bool flag4 = flag3;
			toolStripMenuItem5.Enabled = flag3;
			bool flag5 = flag4;
			bool flag6 = flag5;
			toolStripButton4.Enabled = flag5;
			bool flag7 = flag6;
			bool flag8 = flag7;
			toolStripMenuItem4.Enabled = flag7;
			bool flag9 = flag8;
			bool flag10 = flag9;
			toolStripButton3.Enabled = flag9;
			bool flag11 = flag10;
			bool flag12 = flag11;
			toolStripMenuItem3.Enabled = flag11;
			bool flag13 = flag12;
			bool flag14 = flag13;
			toolStripButton2.Enabled = flag13;
			toolStripMenuItem2.Enabled = flag14;
			ToolStripMenuItem toolStripMenuItem6 = this.miEditPaste;
			ToolStripButton toolStripButton6 = this.btnPaste;
			ToolStripMenuItem toolStripMenuItem7 = this.miEditClearClipboard;
			ToolStripButton toolStripButton7 = this.btnClipboardClear;
			bool canPasteTextFromClipboard = this.rtbCodeSnippet.CanPasteTextFromClipboard;
			bool flag15 = canPasteTextFromClipboard;
			toolStripButton7.Enabled = canPasteTextFromClipboard;
			bool flag16 = flag15;
			bool flag17 = flag16;
			toolStripMenuItem7.Enabled = flag16;
			bool flag18 = flag17;
			bool flag19 = flag18;
			toolStripButton6.Enabled = flag18;
			toolStripMenuItem6.Enabled = flag19;
			ToolStripMenuItem toolStripMenuItem8 = this.miEditSelectAll;
			ToolStripButton toolStripButton8 = this.btnViewHtml;
			ToolStripMenuItem toolStripMenuItem9 = this.miViewHtml;
			bool hasText = this.rtbCodeSnippet.HasText;
			bool flag20 = hasText;
			toolStripMenuItem9.Enabled = hasText;
			bool flag21 = flag20;
			bool flag22 = flag21;
			toolStripButton8.Enabled = flag21;
			toolStripMenuItem8.Enabled = flag22;
		}

		private void wbPreview_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if (!string.IsNullOrEmpty(this.rtbCodeSnippet.Text) && string.IsNullOrEmpty(this.wbPreview.DocumentText))
			{
				this.FormatCode(true);
			}
		}

		private void wbPreview_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (!e.Control)
			{
				return;
			}
			Keys keyCode = e.KeyCode;
			switch (keyCode)
			{
				case Keys.D1:
				{
					this.miViewCodeSnippet_Click(sender, e);
					return;
				}
				case Keys.D2:
				{
					this.miViewFormattedCodeSnippet_Click(sender, e);
					return;
				}
				case Keys.D3:
				{
					this.miViewBoth_Click(sender, e);
					return;
				}
				default:
				{
					if (keyCode == Keys.A)
					{
						break;
					}
					else
					{
						if (keyCode != Keys.V)
						{
							return;
						}
						this.miEditPaste_Click(sender, e);
						return;
					}
				}
			}
			this.miEditSelectAll_Click(sender, e);
		}
	}
}