// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Windows.Forms;

    using JetBrains.Annotations;

    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// <para>Sidebar editor for SmartContent</para>
    /// <para>There is a single instance of a given SmartContentEditor created for each Open Live Writer
    /// post editor window. The implementation of SmartContentEditor objects must therefore be
    /// stateless and assume that they will be the editor for multiple distinct SmartContent objects.</para>
    /// </summary>
    public class SmartContentEditor : UserControl
    {
        /// <summary>
        /// The laid out
        /// </summary>
        private bool controlCreated = false;

        /// <summary>
        /// The selected content
        /// </summary>
        [CanBeNull]
        private ISmartContent selectedContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartContentEditor"/> class.
        /// </summary>
        public SmartContentEditor()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Event fired by the SmartContentEditor whenever it makes a change
        /// to the underlying properties of the SmartContent.
        /// </summary>
        public event EventHandler ContentEdited;

        /// <summary>
        /// Notification that the currently selected SmartContent object
        /// has changed. The editor should adapt its state to the current
        /// selection when this event is fired.
        /// </summary>
        public event EventHandler SelectedContentChanged;

        /// <summary>
        /// Gets or sets the currently selected SmartContent object. The editor should adapt
        /// its state to the current selection when this property changes (notification
        /// of the change is provided via the SelectedContentChanged event).
        /// </summary>
        [CanBeNull]
        public virtual ISmartContent SelectedContent
        {
            get
            {
                return this.selectedContent;
            }

            set
            {
                if (this.selectedContent == value)
                {
                    return;
                }

                this.selectedContent = value;
                this.OnSelectedContentChanged();
            }
        }

        /// <summary>
        /// Event fired by the SmartContentEditor whenever it makes a change
        /// to the underlying properties of the SmartContent.
        /// </summary>
        protected virtual void OnContentEdited()
        {
            this.ContentEdited?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the CreateControl method and reverses
        /// the location of any child controls if running
        /// in a right-to-left culture.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (this.controlCreated)
            {
                return;
            }

            this.controlCreated = true;
            BidiHelper.RtlLayoutFixup(this);
        }

        /// <summary>
        /// Notification that the currently selected SmartContent object
        /// has changed. The editor should adapt its state to the current
        /// selection when this event is fired.
        /// </summary>
        protected virtual void OnSelectedContentChanged()
        {
            this.SelectedContentChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        private void InitializeComponent()
        {
            // SmartContentEditor
            this.Name = nameof(OpenLiveWriter.Api.SmartContentEditor);
            this.Size = new System.Drawing.Size(200, 500);
            this.Font = Res.DefaultFont;
        }
    }
}
