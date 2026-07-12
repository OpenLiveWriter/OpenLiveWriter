// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Plug-in ribbon commands. The Windows extensibility stack
    /// (<c>OpenLiveWriter.Extensibility</c>) depends on WinForms/MSHTML and does not
    /// build on macOS yet, so these commands show an informational dialog instead.
    /// </summary>
    public partial class MainWindow
    {
        private async Task<bool> TryHandlePluginCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.AddPlugin:
                case CommandId.ManagePlugins:
                    await ShowPluginsNotAvailableAsync();
                    return true;
                default:
                    return false;
            }
        }

        private Task ShowPluginsNotAvailableAsync() =>
            MessageDialog.ShowAsync(
                this,
                "Plug-ins",
                "Open Live Writer plug-ins are not available on macOS yet.\n\n"
                + "The Windows plug-in model (OpenLiveWriter.Extensibility) depends on "
                + "WinForms and MSHTML and has not been ported to the cross-platform shell. "
                + "Use the built-in Insert and Publishing features for now.");
    }
}
