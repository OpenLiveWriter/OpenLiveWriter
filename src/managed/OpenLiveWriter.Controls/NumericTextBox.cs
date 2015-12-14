// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Controls
{
    public class NumericTextBox : TextBox
    {
        const int ES_NUMBER = 0x2000;

        protected override CreateParams CreateParams
        {
            [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.Style |= ES_NUMBER;
                return createParams;
            }
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM.PASTE)
            {
                try
                {
                    IDataObject dataObject = Clipboard.GetDataObject();
                    if (dataObject != null)
                    {
                        DataObjectMeister meister = new DataObjectMeister(Clipboard.GetDataObject());

                        // if there is no text data then suppress paste
                        if (meister.TextData == null)
                            return;

                        // if the text-data can't be parsed into an integer then suppress paste
                        try
                        {
                            int.Parse(meister.TextData.Text, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception filtering WM_PASTE: " + ex.ToString());
                    return;
                }
            }

            // default processing
            base.WndProc(ref m);
        }

    }
}
