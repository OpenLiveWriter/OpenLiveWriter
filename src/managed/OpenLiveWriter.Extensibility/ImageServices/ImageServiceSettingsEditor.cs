// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.Extensibility.ImageServices
{
    /// <summary>
    /// Summary description for ImageServiceImageSettingsEditor.
    /// </summary>
    public class ImageServiceSettingsEditor: UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageServiceSettingsEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
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

        protected enum ControlState { Uninitialized, Loading, Loaded };
        protected ControlState EditorState
        {
            get{ return _loadedState; }
        }
        private ControlState _loadedState = ControlState.Uninitialized;

        public void LoadEditor(IProperties imageServiceSettings)
        {
            _loadedState = ControlState.Loading;
            _imageServiceSettings = imageServiceSettings;
            LoadEditor();
            _loadedState = ControlState.Loaded;
        }

        /// <summary>
        /// Hook to allow subclasses to initialize the editor after the editor context has been set.
        /// </summary>
        protected virtual void LoadEditor()
        {

        }

        internal protected IProperties ImageServiceSettings
        {
            get
            {
                return _imageServiceSettings;
            }
        }
        private IProperties _imageServiceSettings;
    }
}
