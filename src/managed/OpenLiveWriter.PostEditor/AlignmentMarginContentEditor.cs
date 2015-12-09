// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor
{
    public abstract class AlignmentMarginContentEditor : EnableableSmartContentEditor
    {
        private MarginCommand _marginCommand;
        private AlignmentCommand _alignmentCommand;

        /// <summary>
        /// Must be called by inheriting classes to avoid null references!
        /// </summary>
        /// <param name="commandManager"></param>
        protected void InitializeAlignmentMarginCommands(CommandManager commandManager)
        {
            _marginCommand = (MarginCommand)commandManager.Get(CommandId.MarginsGroup);
            _alignmentCommand = (AlignmentCommand)commandManager.Get(CommandId.AlignmentGallery);
        }

        protected override void OnSelectedContentChanged()
        {
            base.OnSelectedContentChanged();

            ClearAlignmentMargin();
        }

        public override void UnloadEditor()
        {
            _marginCommand.MarginChanged -= OnMarginChanged;
            _alignmentCommand.AlignmentChanged -= OnAlignmentChanged;
            base.UnloadEditor();
        }

        public Padding ContentMargin { get { return _marginCommand.Value; } }
        public Alignment ContentAlignment { get { return _alignmentCommand.SelectedItem; } }

        protected void ClearAlignmentMargin()
        {
            _marginCommand.SetMargin(null);
            _alignmentCommand.SetAlignment(null);
        }

        protected void SetAlignmentMargin(Padding margin, Alignment alignment)
        {
            _marginCommand.SetMargin(margin);
            _alignmentCommand.SetAlignment(alignment);
        }

        /// <summary>
        /// Implemented by inheriting classes to be notified of margin changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMarginChanged(object sender, EventArgs e)
        {
            // Changing the right or left margins on center aligned smart content resets the alignment.
            if ((_marginCommand.Right > 0 || _marginCommand.Left > 0) && this._alignmentCommand.SelectedItem == Alignment.Center)
                _alignmentCommand.SelectedItem = Alignment.None;
        }

        /// <summary>
        /// Implemented by inheriting classes to be notified of alignment changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnAlignmentChanged(object sender, EventArgs e)
        {
            // Changing the alignment to centered clears out the left and right margins
            if (_alignmentCommand.SelectedItem == Alignment.Center)
                _marginCommand.Value = new Padding(0, _marginCommand.Top, 0, _marginCommand.Bottom);
        }

        /// <summary>
        /// This is a good place for inheriting classes to update command enable state.
        /// </summary>
        public override bool ContentEnabled
        {
            set
            {
                base.ContentEnabled = value;
                _marginCommand.Enabled = value;
                _alignmentCommand.Enabled = value;
                if (value)
                {
                    _marginCommand.MarginChanged += OnMarginChanged;
                    _alignmentCommand.AlignmentChanged += OnAlignmentChanged;
                }
            }
        }
    }

    public class EnableableSmartContentEditor : SmartContentEditor
    {
        private bool _contentEnabled = false;

        public virtual bool ContentEnabled
        {
            get { return _contentEnabled; }
            set { _contentEnabled = value; }
        }

        public virtual void UnloadEditor()
        {
            ContentEnabled = false;
        }
    }

}
