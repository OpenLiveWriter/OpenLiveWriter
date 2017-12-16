using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace CodeSnippet.Forms
{
	internal class CodeSnippetAboutForm : Form
	{
		private IContainer components;

		private TableLayoutPanel lpTable;

		private PictureBox pbLogo;

		private TextBox tbProductName;

		private TextBox tbVersion;

		private TextBox tbCopyright;

		private RichTextBox rtbCompanyName;

		private TextBox tbDescription;

		private Button btnOK;

		public static string AssemblyCompany
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if ((int)customAttributes.Length == 0)
				{
					return string.Empty;
				}
				return ((AssemblyCompanyAttribute)customAttributes[0]).Company;
			}
		}

		public static string AssemblyCopyright
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if ((int)customAttributes.Length == 0)
				{
					return string.Empty;
				}
				return ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
			}
		}

		public static string AssemblyDescription
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if ((int)customAttributes.Length == 0)
				{
					return string.Empty;
				}
				return ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
			}
		}

		public static string AssemblyProduct
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if ((int)customAttributes.Length == 0)
				{
					return string.Empty;
				}
				return ((AssemblyProductAttribute)customAttributes[0]).Product;
			}
		}

		public static string AssemblyTitle
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if ((int)customAttributes.Length > 0)
				{
					AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)customAttributes[0];
					if (!string.IsNullOrEmpty(assemblyTitleAttribute.Title))
					{
						return assemblyTitleAttribute.Title;
					}
				}
				return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public static string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

        public IContainer Components { get => components; set => components = value; }

        public CodeSnippetAboutForm()
		{
			this.InitializeComponent();
			this.InitializeChildren();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeChildren()
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			string formAboutTitle = Resources.FormAbout_Title;
			object[] assemblyTitle = new object[] { CodeSnippetAboutForm.AssemblyTitle };
			this.Text = string.Format(currentUICulture, formAboutTitle, assemblyTitle);
			this.tbProductName.Text = CodeSnippetAboutForm.AssemblyProduct;
			TextBox textBox = this.tbVersion;
			CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
			string formAboutVersion = Resources.FormAbout_Version;
			object[] assemblyVersion = new object[] { CodeSnippetAboutForm.AssemblyVersion };
			textBox.Text = string.Format(cultureInfo, formAboutVersion, assemblyVersion);
			this.tbCopyright.Text = CodeSnippetAboutForm.AssemblyCopyright;
			RichTextBox richTextBox = this.rtbCompanyName;
			CultureInfo currentUICulture1 = CultureInfo.CurrentUICulture;
			string formAboutCompany = Resources.FormAbout_Company;
			object[] assemblyCompany = new object[] { CodeSnippetAboutForm.AssemblyCompany };
			richTextBox.Text = string.Format(currentUICulture1, formAboutCompany, assemblyCompany);
			this.tbDescription.Text = CodeSnippetAboutForm.AssemblyDescription;
		}

		private void InitializeComponent()
		{
			this.lpTable = new TableLayoutPanel();
			this.btnOK = new Button();
			this.tbProductName = new TextBox();
			this.tbVersion = new TextBox();
			this.tbCopyright = new TextBox();
			this.rtbCompanyName = new RichTextBox();
			this.tbDescription = new TextBox();
			this.pbLogo = new PictureBox();
			this.lpTable.SuspendLayout();
			((ISupportInitialize)this.pbLogo).BeginInit();
			base.SuspendLayout();
			this.lpTable.BackColor = Color.Transparent;
			this.lpTable.ColumnCount = 2;
			this.lpTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.19128f));
			this.lpTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 82.80872f));
			this.lpTable.Controls.Add(this.btnOK, 1, 5);
			this.lpTable.Controls.Add(this.tbProductName, 1, 0);
			this.lpTable.Controls.Add(this.tbVersion, 1, 1);
			this.lpTable.Controls.Add(this.tbCopyright, 1, 2);
			this.lpTable.Controls.Add(this.rtbCompanyName, 1, 3);
			this.lpTable.Controls.Add(this.tbDescription, 1, 4);
			this.lpTable.Controls.Add(this.pbLogo, 0, 0);
			this.lpTable.Dock = DockStyle.Fill;
			this.lpTable.Location = new Point(3, 6);
			this.lpTable.Name = "lpTable";
			this.lpTable.RowCount = 6;
			this.lpTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
			this.lpTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
			this.lpTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
			this.lpTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
			this.lpTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.lpTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
			this.lpTable.Size = new System.Drawing.Size(413, 213);
			this.lpTable.TabIndex = 0;
			this.btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnOK.BackColor = SystemColors.Control;
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOK.Location = new Point(335, 187);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 5;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = false;
			this.tbProductName.BackColor = SystemColors.Window;
			this.tbProductName.BorderStyle = BorderStyle.None;
			this.tbProductName.Dock = DockStyle.Fill;
			this.tbProductName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.tbProductName.Location = new Point(70, 0);
			this.tbProductName.Margin = new System.Windows.Forms.Padding(0);
			this.tbProductName.Multiline = true;
			this.tbProductName.Name = "tbProductName";
			this.tbProductName.ReadOnly = true;
			this.tbProductName.Size = new System.Drawing.Size(343, 31);
			this.tbProductName.TabIndex = 0;
			this.tbProductName.TabStop = false;
			this.tbProductName.Text = "Product Name";
			this.tbVersion.BackColor = SystemColors.Window;
			this.tbVersion.BorderStyle = BorderStyle.None;
			this.tbVersion.Dock = DockStyle.Fill;
			this.tbVersion.Location = new Point(70, 31);
			this.tbVersion.Margin = new System.Windows.Forms.Padding(0);
			this.tbVersion.Multiline = true;
			this.tbVersion.Name = "tbVersion";
			this.tbVersion.ReadOnly = true;
			this.tbVersion.Size = new System.Drawing.Size(343, 31);
			this.tbVersion.TabIndex = 1;
			this.tbVersion.TabStop = false;
			this.tbVersion.Text = "Version";
			this.tbCopyright.BackColor = SystemColors.Window;
			this.tbCopyright.BorderStyle = BorderStyle.None;
			this.tbCopyright.Dock = DockStyle.Fill;
			this.tbCopyright.Location = new Point(70, 62);
			this.tbCopyright.Margin = new System.Windows.Forms.Padding(0);
			this.tbCopyright.Multiline = true;
			this.tbCopyright.Name = "tbCopyright";
			this.tbCopyright.ReadOnly = true;
			this.tbCopyright.Size = new System.Drawing.Size(343, 31);
			this.tbCopyright.TabIndex = 2;
			this.tbCopyright.TabStop = false;
			this.tbCopyright.Text = "Copyright";
			this.rtbCompanyName.BackColor = SystemColors.Window;
			this.rtbCompanyName.BorderStyle = BorderStyle.None;
			this.rtbCompanyName.Dock = DockStyle.Fill;
			this.rtbCompanyName.Location = new Point(70, 93);
			this.rtbCompanyName.Margin = new System.Windows.Forms.Padding(0);
			this.rtbCompanyName.Multiline = false;
			this.rtbCompanyName.Name = "rtbCompanyName";
			this.rtbCompanyName.ReadOnly = true;
			this.rtbCompanyName.Size = new System.Drawing.Size(343, 31);
			this.rtbCompanyName.TabIndex = 3;
			this.rtbCompanyName.TabStop = false;
			this.rtbCompanyName.Text = "Company Name";
			this.rtbCompanyName.LinkClicked += new LinkClickedEventHandler(this.rtbCompanyName_LinkClicked);
			this.tbDescription.BackColor = SystemColors.Window;
			this.tbDescription.BorderStyle = BorderStyle.None;
			this.tbDescription.Dock = DockStyle.Fill;
			this.tbDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
			this.tbDescription.Location = new Point(70, 124);
			this.tbDescription.Margin = new System.Windows.Forms.Padding(0);
			this.tbDescription.Multiline = true;
			this.tbDescription.Name = "tbDescription";
			this.tbDescription.ReadOnly = true;
			this.tbDescription.Size = new System.Drawing.Size(343, 53);
			this.tbDescription.TabIndex = 4;
			this.tbDescription.TabStop = false;
			this.tbDescription.Text = "Description";
			this.pbLogo.BackColor = Color.Transparent;
			this.pbLogo.Dock = DockStyle.Top;
			this.pbLogo.Image = Resources.blog_logo;
			this.pbLogo.Location = new Point(3, 3);
			this.pbLogo.Name = "pbLogo";
			this.lpTable.SetRowSpan(this.pbLogo, 4);
			this.pbLogo.Size = new System.Drawing.Size(64, 78);
			this.pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
			this.pbLogo.TabIndex = 12;
			this.pbLogo.TabStop = false;
			base.AcceptButton = this.btnOK;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = SystemColors.Window;
			base.CancelButton = this.btnOK;
			base.ClientSize = new System.Drawing.Size(419, 222);
			base.Controls.Add(this.lpTable);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "CodeSnippetAboutForm";
			base.Padding = new System.Windows.Forms.Padding(3, 6, 3, 3);
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "About Code Snippet";
			this.lpTable.ResumeLayout(false);
			this.lpTable.PerformLayout();
			((ISupportInitialize)this.pbLogo).EndInit();
			base.ResumeLayout(false);
		}

		private void rtbCompanyName_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
			this.btnOK.Focus();
		}
	}
}