// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Insert-tab behavior for the shell: routes the Insert commands that need a
    /// dialog (Insert Table, Insert Video) onto the editor. Simple bridge-only
    /// inserts (horizontal line, table-tools ops) are handled directly by
    /// <see cref="WebViewEditor.HandleCommandAsync"/>.
    /// </summary>
    public partial class MainWindow
    {
        private async Task<bool> TryHandleInsertCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.InsertTable:
                case CommandId.InsertTable2:
                    await ShowInsertTableAsync();
                    return true;
                default:
                    return false;
            }
        }

        private async Task ShowInsertTableAsync()
        {
            var editor = GetEditor();
            if (editor == null)
                return;

            TableDialogResult result = await TableDialog.ShowAsync(this);
            if (result == null)
                return;

            await editor.InsertTableAsync(result.Rows, result.Columns, result.HeaderRow, result.Width);
            UpdateStatus($"Inserted {result.Rows}\u00D7{result.Columns} table.");
        }
    }
}
