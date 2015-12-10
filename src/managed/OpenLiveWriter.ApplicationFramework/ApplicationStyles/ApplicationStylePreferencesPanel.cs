// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    /// <summary>
    /// Appearance preferences panel.
    /// </summary>
    public class ApplicationStylePreferencesPanel : PreferencesPanel
    {
        #region Static & Constant Declarations

        /// <summary>
        ///	The types of ApplicationStyle objects provided by the system.
        /// </summary>
        private Type[] applicationStyleTypes = new Type[]
        {
            typeof(ApplicationStyleSkyBlue),
        };

        #endregion Static & Constant Declarations

        #region Private Member Variables

        /// <summary>
        /// The AppearancePreferences object.
        /// </summary>
        private ApplicationStylePreferences applicationStylePreferences;

        #endregion Private Member Variables

        #region Windows Form Designer generated code

        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.GroupBox groupBoxTheme;
        private System.Windows.Forms.Label labelFolderNameFont;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBoxApplicationStyles;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.ComponentModel.Container components = null;

        #endregion Windows Form Designer generated code

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the AppearancePreferencesPanel class.
        /// </summary>
        public ApplicationStylePreferencesPanel() : this(new ApplicationStylePreferences(false))
        {
        }

        public ApplicationStylePreferencesPanel(ApplicationStylePreferences preferences)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Set the panel bitmap.
            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("ApplicationStyles.Images.ApplicationStyleSmall.png");

            //	Instantiate the MicroViewPreferences object and initialize the controls.
            applicationStylePreferences = preferences;
            applicationStylePreferences.PreferencesModified += new EventHandler(appearancePreferences_PreferencesModified);

            //	Initialize the application styles listbox.
            InitializeApplicationStyles();
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

        #endregion Class Initialization & Termination

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxTheme = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.labelFolderNameFont = new System.Windows.Forms.Label();
            this.listBoxApplicationStyles = new System.Windows.Forms.ListBox();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.groupBoxTheme.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxTheme
            //
            this.groupBoxTheme.Controls.Add(this.label1);
            this.groupBoxTheme.Controls.Add(this.pictureBoxPreview);
            this.groupBoxTheme.Controls.Add(this.labelFolderNameFont);
            this.groupBoxTheme.Controls.Add(this.listBoxApplicationStyles);
            this.groupBoxTheme.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxTheme.Location = new System.Drawing.Point(8, 32);
            this.groupBoxTheme.Name = "groupBoxTheme";
            this.groupBoxTheme.Size = new System.Drawing.Size(354, 282);
            this.groupBoxTheme.TabIndex = 1;
            this.groupBoxTheme.TabStop = false;
            this.groupBoxTheme.Text = "Color";
            //
            // label1
            //
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(150, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 18);
            this.label1.TabIndex = 2;
            this.label1.Text = "Preview:";
            //
            // pictureBoxPreview
            //
            this.pictureBoxPreview.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBoxPreview.Location = new System.Drawing.Point(150, 36);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(192, 235);
            this.pictureBoxPreview.TabIndex = 2;
            this.pictureBoxPreview.TabStop = false;
            //
            // labelFolderNameFont
            //
            this.labelFolderNameFont.BackColor = System.Drawing.SystemColors.Control;
            this.labelFolderNameFont.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFolderNameFont.Location = new System.Drawing.Point(10, 18);
            this.labelFolderNameFont.Name = "labelFolderNameFont";
            this.labelFolderNameFont.Size = new System.Drawing.Size(132, 18);
            this.labelFolderNameFont.TabIndex = 0;
            this.labelFolderNameFont.Text = "&Color scheme:";
            //
            // listBoxApplicationStyles
            //
            this.listBoxApplicationStyles.DisplayMember = "DisplayName";
            this.listBoxApplicationStyles.IntegralHeight = false;
            this.listBoxApplicationStyles.Location = new System.Drawing.Point(10, 36);
            this.listBoxApplicationStyles.Name = "listBoxApplicationStyles";
            this.listBoxApplicationStyles.Size = new System.Drawing.Size(130, 235);
            this.listBoxApplicationStyles.TabIndex = 1;
            this.listBoxApplicationStyles.SelectedIndexChanged += new System.EventHandler(this.listBoxApplicationStyles_SelectedIndexChanged);
            //
            // fontDialog
            //
            this.fontDialog.AllowScriptChange = false;
            this.fontDialog.AllowVerticalFonts = false;
            this.fontDialog.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.fontDialog.FontMustExist = true;
            this.fontDialog.ScriptsOnly = true;
            this.fontDialog.ShowEffects = false;
            //
            // AppearancePreferencesPanel
            //
            this.Controls.Add(this.groupBoxTheme);
            this.Name = "AppearancePreferencesPanel";
            this.PanelName = "Appearance";
            this.Controls.SetChildIndex(this.groupBoxTheme, 0);
            this.groupBoxTheme.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Saves the PreferencesPanel.
        /// </summary>
        public override void Save()
        {
            if (applicationStylePreferences.IsModified())
                applicationStylePreferences.Save();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Initialize the application styles listbox.
        /// </summary>
        private void InitializeApplicationStyles()
        {
            //	Add each of the available styles.
            foreach (Type type in applicationStyleTypes)
            {
                ApplicationStyle applicationStyle;
                try
                {
                    applicationStyle = Activator.CreateInstance(type) as ApplicationStyle;
                    listBoxApplicationStyles.Items.Add(applicationStyle);
                    if (applicationStyle.GetType() == ApplicationManager.ApplicationStyle.GetType())
                        listBoxApplicationStyles.SelectedItem = applicationStyle;
                }
                catch (Exception e)
                {
                    Debug.Fail("Error loading ApplicationStyle "+type.ToString(), e.StackTrace.ToString());
                }
            }
        }

        #endregion Private Methods

        #region Private Event Handlers

        /// <summary>
        /// appearancePreferences_PreferencesModified event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void appearancePreferences_PreferencesModified(object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
        }

        /// <summary>
        /// listBoxApplicationStyles_SelectedIndexChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void listBoxApplicationStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            //	If the list box is empty, just ignore the event.
            if (listBoxApplicationStyles.Items.Count == 0)
                return;

            //	Update state.
            ApplicationStyle applicationStyle = (ApplicationStyle)listBoxApplicationStyles.SelectedItem;
            pictureBoxPreview.Image = applicationStyle.PreviewImage;
            applicationStylePreferences.ApplicationStyleType = applicationStyle.GetType();
        }

        #endregion Private Event Handlers
    }
}
