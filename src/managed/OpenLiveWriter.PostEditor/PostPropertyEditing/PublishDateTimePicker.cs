// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    internal class PublishDateTimePicker : DateTimePicker
    {
        private ToolTip _toolTip = new ToolTip();
        private bool _isSetting;

        public PublishDateTimePicker()
        {
            _toolTip.SetToolTip(this, Res.Get(StringId.PublishDateTooltip));
            _isSetting = false;
            // Add our internal value changed handler
            ValueChanged += new EventHandler(Internal_ValueChanged);
        }

        private void Internal_ValueChanged(object sender, EventArgs e)
        {
            // Values are changing, trigger our 2nd event handler only if
            // we are not setting through SetDateTimeAndChecked
            if (_isSetting == false && ValueChanged2 != null)
            {
                ValueChanged2(sender, e);
            }
        }

        public void SetDateTimeAndChecked(bool isChecked, DateTime dateTimeValue)
        {
            // Flag this, we don't want to trigger our 2nd event handler in this case
            _isSetting = true;
            try
            {
                Checked = isChecked;
                Value = dateTimeValue;
            }
            finally
            {
                _isSetting = false;
            }
        }

        public event EventHandler ValueChanged2;
    }
}
