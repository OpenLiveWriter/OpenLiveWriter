// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BrowserControl
{
    /// <summary>
    /// Enumeration of available BrowserCommands.
    /// </summary>
    public enum BrowserCommand
    {
        // standard file menu commands
        NewWindow,
        SaveAs,
        PageSetup,
        Print,
        PrintPreview,
        Properties,

        // standard edit menu commands
        Cut,
        Copy,
        Paste,
        SelectAll,
        Find,

        // standard view menu commands
        GoBack,
        GoForward,
        Stop,
        Refresh,
        GoHome,
        GoSearch,
        ViewSource,
        Languages,

        // standard favorites menu commands
        AddFavorite,
        OrganizeFavorites,

        // standard tools menu commands
        InternetOptions
    }

    /// <summary>
    /// Interface for accessing browser commands
    /// </summary>
    public interface IBrowserCommand
    {
        /// <summary>
        /// Determine whether the current command is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Execute the command
        /// </summary>
        void Execute();

    }
}
