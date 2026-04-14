// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Platform;

using IBlogClientUIContext = OpenLiveWriter.Platform.IBlogClientUIContext;

namespace OpenLiveWriter.BlogClient.Clients
{
    public class CredentialsHelper
    {
        public static IDisposable ShowWaitCursor()
        {
            IBlogClientUIContext uiContext = BlogClientUIContext.ContextForCurrentThread;
            if (uiContext != null && !uiContext.InvokeRequired)
            {
                var dialogService = PlatformContext.DialogService;
                return dialogService?.ShowWaitCursor();
            }
            return null;
        }

        public static CredentialsPromptResult PromptForCredentials(ref string username, ref string password, ICredentialsDomain domain)
        {
            if (BlogClientUIContext.SilentModeForCurrentThread)
                return CredentialsPromptResult.Abort;

            IBlogClientUIContext uiContext = BlogClientUIContext.ContextForCurrentThread;
            if (uiContext == null)
                return CredentialsPromptResult.Abort;

            var prompter = PlatformContext.CredentialsPrompter;
            if (prompter == null)
                return CredentialsPromptResult.Abort;

            return prompter.PromptForCredentials(uiContext, ref username, ref password, domain);
        }
    }
}
