// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Autoreplace
{
    public partial class AutoreplaceEditForm : ApplicationDialog
    {
        private readonly AutoreplacePreferences _preferences;

        public AutoreplaceEditForm(AutoreplacePreferences preferences)
        {
            _preferences = preferences;
            InitializeComponent();

            buttonOK.Text = Res.Get(StringId.OKButtonText);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            Text = Res.Get(StringId.AutoreplaceFormTitle);
        }

        public string Phrase
        {
            get
            {
                return textBoxPhrase.Text;
            }
            set
            {
                textBoxPhrase.Text = value;
                _newReplaceValue = false;
            }
        }

        private bool _newReplaceValue = true;

        public string ReplaceValue
        {
            get
            {
                return textBoxReplace.Text;
            }
            set
            {
                textBoxReplace.Text = value;
            }
        }

        private bool ValidateForm()
        {
            if (_newReplaceValue)
            {

                if (String.IsNullOrEmpty(Phrase))
                {
                    DisplayMessage.Show(MessageId.AutoreplacePhraseRequired);
                    textBoxPhrase.Focus();
                    return false;
                }

                if (String.IsNullOrEmpty(ReplaceValue))
                {
                    DisplayMessage.Show(MessageId.AutoreplaceReplacementRequired);
                    textBoxReplace.Focus();
                    return false;
                }

                AutoreplacePhrase phrase = _preferences.GetPhrase(Phrase);
                if (phrase != null && phrase.ReplaceValue != null)
                {
                    DialogResult result = DisplayMessage.Show(MessageId.AutoreplaceAlreadyExists);
                    if (result == DialogResult.No)
                    {
                        textBoxPhrase.Focus();
                        return false;
                    }
                }

            }
            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
                DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
