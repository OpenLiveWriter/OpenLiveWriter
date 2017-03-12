// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.Extensibility.ImageServices;

namespace OpenLiveWriter.Extensibility.ImageServices
{
    /// <summary>
    /// Base editor control for editing file upload settings for an individual image file.
    /// </summary>
    public class ImageFileUploadSettingsEditor : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageFileUploadSettingsEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // set the background color
            BackColor = ApplicationManager.ApplicationStyle.ActiveTabBottomColor ;
            ApplicationStyleManager.ApplicationStyleChanged += new EventHandler(ApplicationManager_ApplicationStyleChanged);
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
                ApplicationStyleManager.ApplicationStyleChanged -= new EventHandler(ApplicationManager_ApplicationStyleChanged);
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
            components = new System.ComponentModel.Container();
        }
        #endregion

        /// <summary>
        /// Hook that allows subclasses to refresh their cached appearance settings.
        /// </summary>
        protected virtual void LoadAppearanceSettings()
        {
            BackColor = ApplicationManager.ApplicationStyle.ActiveTabBottomColor ;
        }

        private void ApplicationManager_ApplicationStyleChanged(object sender, EventArgs e)
        {
            LoadAppearanceSettings();
            PerformLayout();
            Invalidate();
        }

        protected enum ControlState { Uninitialized, Loading, Loaded };
        protected ControlState EditorState
        {
            get{ return _loadedState; }
        }
        private ControlState _loadedState = ControlState.Uninitialized;

        public void LoadEditor(IImageUploadSettingsEditorContext context)
        {
            _loadedState = ControlState.Loading;
            _context = context;
            LoadEditor();
            _loadedState = ControlState.Loaded;
        }

        /// <summary>
        /// Hook to allow subclasses to initialize the editor after the editor context has been set.
        /// </summary>
        protected virtual void LoadEditor()
        {

        }

        internal protected IImageUploadSettingsEditorContext EditorContext
        {
            get
            {
                return _context;
            }
        }
        private IImageUploadSettingsEditorContext _context;
    }
}
