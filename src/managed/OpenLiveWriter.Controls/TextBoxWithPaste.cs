// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Controls
{

    public class TextBoxWithPaste : TextBox
    {
        public event OnPasteEventHandler OnPaste;

        #region Protected Method Overrides

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows Message to process.</param>
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            //	process the message first--don't want to notify about paste before it happens!
            try
            {
                base.WndProc(ref m);

                //	Dispatch the message.
                if (m.Msg == (int)WM.PASTE)
                {
                    OnPasteTriggered(new PasteEventArgs());
                }
            }
            catch (Exception e)
            {
                Trace.Fail("TextBoxWithPaste WndProc Exception", e.ToString());
            }
        }

        #endregion Protected Method Overrides

        protected void OnPasteTriggered(PasteEventArgs e)
        {
            if (OnPaste != null)
                OnPaste(this, e);
        }

        public delegate void OnPasteEventHandler(object sender, PasteEventArgs eventArgs);

        public class PasteEventArgs : EventArgs
        {

        }
    }
}
