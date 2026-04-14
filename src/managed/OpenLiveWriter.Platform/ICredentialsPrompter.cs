// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Result of a credentials prompt operation.
    /// </summary>
    public enum CredentialsPromptResult { Cancel, Abort, SaveUsername, SaveUsernameAndPassword }

    /// <summary>
    /// Platform-agnostic interface for prompting the user for credentials.
    /// Implementations handle the UI (WinForms on Windows, native dialogs on Mac, etc.).
    /// </summary>
    public interface ICredentialsPrompter
    {
        CredentialsPromptResult PromptForCredentials(
            IBlogClientUIContext uiContext,
            ref string username,
            ref string password,
            ICredentialsDomainInfo domain);
    }

    /// <summary>
    /// Platform-agnostic interface describing a credentials domain (blog service).
    /// </summary>
    public interface ICredentialsDomainInfo
    {
        string Name { get; }
        string Description { get; }
        byte[] Icon { get; }
        byte[] Image { get; }
        bool AllowsSavePassword { get; }
    }
}
