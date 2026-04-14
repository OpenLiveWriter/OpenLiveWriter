// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.Platform.Windows.BlogClient
{
    /// <summary>
    /// Windows/WinForms implementation of ICredentialsPrompter.
    /// Shows the BlogClientLoginDialog to prompt for credentials.
    /// </summary>
    public class WindowsCredentialsPrompter : ICredentialsPrompter
    {
        public CredentialsPromptResult PromptForCredentials(
            IBlogClientUIContext uiContext,
            ref string username,
            ref string password,
            ICredentialsDomainInfo domain)
        {
            CredentialsPromptResult result;
            string capturedUsername = username;
            string capturedPassword = password;
            CredentialsPromptResult capturedResult = CredentialsPromptResult.Abort;

            void ShowPrompt()
            {
                using (BlogClientLoginDialog form = new BlogClientLoginDialog())
                {
                    if (capturedUsername != null)
                        form.UserName = capturedUsername;
                    if (capturedPassword != null)
                        form.Password = capturedPassword;
                    if (domain != null)
                    {
                        form.Domain = domain;
                        form.Text = form.Text + " - " + domain.Name;
                    }

                    IWin32Window owner = new NativeWindowWrapper(uiContext.NativeWindowHandle);
                    DialogResult dialogResult = form.ShowDialog(owner);
                    if (dialogResult == DialogResult.OK)
                    {
                        capturedUsername = form.UserName;
                        capturedPassword = form.Password;
                        capturedResult = form.SavePassword
                                    ? CredentialsPromptResult.SaveUsernameAndPassword
                                    : CredentialsPromptResult.SaveUsername;
                    }
                    else
                    {
                        capturedResult = CredentialsPromptResult.Cancel;
                    }
                }
            }

            if (uiContext.InvokeRequired)
            {
                uiContext.Invoke(new ThreadStart(ShowPrompt), null);
            }
            else
            {
                ShowPrompt();
                // Force a UI loop so that the dialog closes without hanging
                Application.DoEvents();
            }

            result = capturedResult;
            if (result != CredentialsPromptResult.Cancel)
            {
                username = capturedUsername;
                password = capturedPassword;
            }

            return result;
        }
    }

    /// <summary>
    /// Simple IWin32Window wrapper around an IntPtr handle.
    /// </summary>
    internal class NativeWindowWrapper : IWin32Window
    {
        private readonly IntPtr _handle;
        public NativeWindowWrapper(IntPtr handle) { _handle = handle; }
        public IntPtr Handle => _handle;
    }
}
