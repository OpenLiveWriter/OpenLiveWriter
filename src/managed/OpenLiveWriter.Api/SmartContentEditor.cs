// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// <para>Sidebar editor for SmartContent</para>
    /// <para>There is a single instance of a given SmartContentEditor created for each Open Live Writer
    /// post editor window. The implementation of SmartContentEditor objects must therefore be
    /// stateless and assume that they will be the editor for multiple distince SmartContent objects.</para>
    /// </summary>
    public class SmartContentEditor : UserControl
    {
        private bool _layedOut = false;
        /// <summary>
        /// Initialize a new SmartContentEditor instance.
        /// </summary>
        public SmartContentEditor()
        {
            InitializeComponent();
            Font = Res.DefaultFont;
        }

        /// <summary>
        /// Get or set the currently selected SmartContent object. The editor should adapt
        /// its state to the current selection when this property changes (notification
        /// of the change is provided via the SelectedContentChanged event).
        /// </summary>
        public virtual ISmartContent SelectedContent
        {
            get
            {
                return _selectedContent;
            }
            set
            {
                _selectedContent = value;
                OnSelectedContentChanged();
            }
        }
        private ISmartContent _selectedContent;

        /// <summary>
        /// Notification that the currently selected SmartContent object
        /// has changed. The editor should adapt its state to the current
        /// selection when this event is fired.
        /// </summary>
        public event EventHandler SelectedContentChanged;

        /// <summary>
        /// Notification that the currently selected SmartContent object
        /// has changed. The editor should adapt its state to the current
        /// selection when this event is fired.
        /// </summary>
        protected virtual void OnSelectedContentChanged()
        {
            if (SelectedContentChanged != null)
                SelectedContentChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fired by the SmartContentEditor whenever it makes a change
        /// to the underlying properties of the SmartContent.
        /// </summary>
        public event EventHandler ContentEdited;

        /// <summary>
        /// Event fired by the SmartContentEditor whenever it makes a change
        /// to the underlying properties of the SmartContent.
        /// </summary>
        protected virtual void OnContentEdited()
        {
            if (ContentEdited != null)
                ContentEdited(this, EventArgs.Empty);
        }

        private void InitializeComponent()
        {
            //
            // SmartContentEditor
            //
            this.Name = "SmartContentEditor";
            this.Size = new System.Drawing.Size(200, 500);

        }

        /// <summary>
        /// Raises the CreateControl method and reverses
        /// the location of any child controls if running
        /// in a right-to-left culture.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!_layedOut)
            {
                _layedOut = true;
                BidiHelper.RtlLayoutFixup(this);
            }

        }
    }

}
