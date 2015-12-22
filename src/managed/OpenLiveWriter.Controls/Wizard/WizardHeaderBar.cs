// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls.Wizard
{
    /// <summary>
    /// Summary description for WizardHeaderBar.
    /// </summary>
    public class WizardHeaderBar : UserControl
    {
        private GradientPanel titleBarPanel;
        private LabelControl labelDescriptionText;
        private LabelControl labelTitleText;
        private PictureBox pictureBoxTitleBar;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private HeaderBarUITheme _uiTheme;
        public WizardHeaderBar()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.labelTitleText.Font = Res.GetFont(FontSize.Heading, FontStyle.Regular);

            _uiTheme = new HeaderBarUITheme(this);
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WizardHeaderBar));
            this.titleBarPanel = new OpenLiveWriter.Controls.Wizard.GradientPanel();
            this.labelDescriptionText = new OpenLiveWriter.Controls.LabelControl();
            this.labelTitleText = new OpenLiveWriter.Controls.LabelControl();
            this.pictureBoxTitleBar = new System.Windows.Forms.PictureBox();
            this.titleBarPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // titleBarPanel
            //
            this.titleBarPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.titleBarPanel.Controls.Add(this.labelDescriptionText);
            this.titleBarPanel.Controls.Add(this.labelTitleText);
            this.titleBarPanel.Controls.Add(this.pictureBoxTitleBar);
            this.titleBarPanel.Location = new System.Drawing.Point(0, 0);
            this.titleBarPanel.Name = "titleBarPanel";
            this.titleBarPanel.Size = new System.Drawing.Size(392, 64);
            this.titleBarPanel.TabIndex = 1;
            //
            // labelDescriptionText
            //
            this.labelDescriptionText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescriptionText.BackColor = System.Drawing.Color.Transparent;
            this.labelDescriptionText.ForeColor = System.Drawing.Color.RoyalBlue;
            this.labelDescriptionText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDescriptionText.Location = new System.Drawing.Point(72, 29);
            this.labelDescriptionText.MultiLine = false;
            this.labelDescriptionText.Name = "labelDescriptionText";
            this.labelDescriptionText.Size = new System.Drawing.Size(312, 31);
            this.labelDescriptionText.TabIndex = 13;
            this.labelDescriptionText.Text = "A brief description of what this wizard does. djhdkjsdfhsd dfsdfsdf sdfsdf sdfsdf" +
                "";
            //
            // labelTitleText
            //
            this.labelTitleText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitleText.BackColor = System.Drawing.Color.Transparent;
            this.labelTitleText.Font = Res.GetFont(FontSize.Heading, FontStyle.Regular);
            this.labelTitleText.ForeColor = System.Drawing.Color.Navy;
            this.labelTitleText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelTitleText.Location = new System.Drawing.Point(72, 4);
            this.labelTitleText.MultiLine = false;
            this.labelTitleText.Name = "labelTitleText";
            this.labelTitleText.Size = new System.Drawing.Size(312, 23);
            this.labelTitleText.TabIndex = 12;
            this.labelTitleText.Text = "Wizard Title";
            //
            // pictureBoxTitleBar
            //
            this.pictureBoxTitleBar.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxTitleBar.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTitleBar.Image")));
            this.pictureBoxTitleBar.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBoxTitleBar.Location = new System.Drawing.Point(0, 8);
            this.pictureBoxTitleBar.Name = "pictureBoxTitleBar";
            this.pictureBoxTitleBar.Size = new System.Drawing.Size(72, 40);
            this.pictureBoxTitleBar.TabIndex = 11;
            this.pictureBoxTitleBar.TabStop = false;
            //
            // WizardHeaderBar
            //
            this.Controls.Add(this.titleBarPanel);
            this.Name = "WizardHeaderBar";
            this.Size = new System.Drawing.Size(392, 64);
            this.titleBarPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Gets or sets the title bar image for the wizard.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the image to use in the wizard title bar.")
        ]
        public Image TitleBarImage
        {
            get { return pictureBoxTitleBar.Image; }
            set { pictureBoxTitleBar.Image = value; }
        }

        /// <summary>
        /// Gets or sets the title of the wizard.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue("Wizard Title"),
                Description("Specifies the title of the wizard.")
        ]
        public string TitleText
        {
            get { return labelTitleText.Text; }
            set { labelTitleText.Text = value; }
        }

        /// <summary>
        /// Gets or sets the title of the wizard.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue("A description of what this wizard does."),
                Description("Specifies the description of the wizard.")
        ]
        public string DescriptionText
        {
            get { return labelDescriptionText.Text; }
            set { labelDescriptionText.Text = value; }
        }

        private class HeaderBarUITheme : ControlUITheme
        {
            WizardHeaderBar _headerBar;
            public HeaderBarUITheme(WizardHeaderBar headerBar) : base(headerBar, false)
            {
                _headerBar = headerBar;
                ApplyTheme();
            }

            protected override void ApplyTheme(bool highContrast)
            {
                //tweak the colors to something more fine tuned
                _headerBar.labelTitleText.ForeColor = !highContrast ? ColorHelper.StringToColor("#004E98") : SystemColors.WindowText;
                _headerBar.labelDescriptionText.ForeColor = !highContrast ? ColorHelper.StringToColor("#316AC5") : SystemColors.WindowText;
            }
        }
    }

    class GradientPanel : Panel
    {
        public GradientPanel() : base()
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle gradientRect = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height + 40);
            using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientRect, Color.White, BackColor, LinearGradientMode.Vertical))
                //Rectangle gradientRect = ClientRectangle;
                //using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientRect, Color.White, BackColor, LinearGradientMode.Horizontal))
                e.Graphics.FillRectangle(linearGradientBrush, new Rectangle(0, 0, this.Width, this.Height));

            base.OnPaint(e);
        }
    }
}
