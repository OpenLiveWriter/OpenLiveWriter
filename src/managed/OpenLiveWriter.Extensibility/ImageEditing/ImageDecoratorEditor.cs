// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    /// <summary>
    /// The base class for building an editor for an image decorator.
    /// </summary>
    public class ImageDecoratorEditor : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageDecoratorEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
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
            components = new System.ComponentModel.Container();
        }
        #endregion

        protected enum ControlState { Uninitialized, Loading, Loaded };
        protected ControlState EditorState
        {
            get { return _loadedState; }
        }
        private ControlState _loadedState = ControlState.Uninitialized;
        private IImageTargetEditor _imageTargetEditor;

        public void LoadEditor(ImageDecoratorEditorContext context, object state, IImageTargetEditor imageTargetEditor)
        {
            _loadedState = ControlState.Loading;
            _context = context;
            _state = state;
            _imageTargetEditor = imageTargetEditor;
            LoadEditor();
            _loadedState = ControlState.Loaded;
            OnEditorLoaded();
        }

        protected virtual void OnEditorLoaded()
        {

        }
        /// <summary>
        /// Signal the editor to save its settings and to apply the image decoration.
        /// </summary>
        /// <param name="forced">If true, the settings will be applied even if the editor is not fully loaded.</param>
        protected void SaveSettingsAndApplyDecorator(bool forced)
        {
            if (forced || EditorState == ControlState.Loaded)
            {
                using (new WaitCursor())
                {
                    using (IImageDecoratorUndoUnit undo = EditorContext.CreateUndoUnit())
                    {
                        OnSaveSettings();
                        EditorContext.ApplyDecorator();
                        undo.Commit();
                        _imageTargetEditor.ImageEditFinished();
                    }
                }
            }
        }

        /// <summary>
        /// Signal the editor to save its settings and to apply the image decoration.
        /// Note: if the editor is not in Loaded state, this call is a no-op.
        /// </summary>
        protected void SaveSettingsAndApplyDecorator()
        {
            SaveSettingsAndApplyDecorator(false);
        }

        /// <summary>
        /// Signal the editor to save its settings.
        /// </summary>
        /// <param name="forced">If true, the settings will be applied even if the editor is not fully loaded.</param>
        protected void SaveSettings(bool forced)
        {
            if (forced || EditorState == ControlState.Loaded)
            {
                using (IImageDecoratorUndoUnit undo = EditorContext.CreateUndoUnit())
                {
                    OnSaveSettings();
                    undo.Commit();
                    _imageTargetEditor.ImageEditFinished();
                }
            }
        }

        /// <summary>
        /// Signal the editor to save its settings.
        /// Note: if the editor is not in Loaded state, this call is a no-op.
        /// </summary>
        protected void SaveSettings()
        {
            SaveSettings(false);
        }

        /// <summary>
        /// Should be implemented by subclasses to save their settings.
        /// </summary>
        protected virtual void OnSaveSettings()
        {

        }

        protected virtual void LoadEditor()
        {

        }

        protected IProperties Settings
        {
            get
            {
                return _context.Settings;
            }
        }

        public virtual FormBorderStyle FormBorderStyle
        {
            get { return FormBorderStyle.FixedDialog; }
        }

        internal protected ImageDecoratorEditorContext EditorContext
        {
            get
            {
                return _context;
            }
        }
        private ImageDecoratorEditorContext _context;

        internal protected object State
        {
            get { return _state; }
        }
        private object _state;

        public virtual Size GetPreferredSize()
        {
            return new Size(ScaleX(100), ScaleX(100));
        }

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
        }

        private PointF scale = new PointF(1f, 1f);

        protected int ScaleX(int x)
        {
            return (int)(x * scale.X);
        }

        protected int ScaleY(int y)
        {
            return (int)(y * scale.Y);
        }
        #endregion
    }
}
