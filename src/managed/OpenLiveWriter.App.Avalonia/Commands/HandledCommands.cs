// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia.Commands
{
    /// <summary>
    /// The single source of truth for which <see cref="CommandId"/>s the macOS shell
    /// actually routes to a handler. The ribbon consults this set to disable dead
    /// buttons ("not yet available") and the menu builder is validated against it,
    /// so the UI never advertises a command that falls through to the status bar.
    /// The set must mirror the routing switches in MainWindow.TryHandle*CommandAsync,
    /// <see cref="Editor.WebViewEditor.HandleCommandAsync"/> and EditorPanel's
    /// CommandBridge registrations; the GroupQ tests pin both ends.
    /// </summary>
    public static class HandledCommands
    {
        private static readonly HashSet<CommandId> Handled = BuildHandledSet();

        /// <summary>True when the shell routes <paramref name="commandId"/> to a real handler.</summary>
        public static bool IsHandled(CommandId commandId) => Handled.Contains(commandId);

        /// <summary>All handled commands (for menu/registry validation tests).</summary>
        public static IReadOnlyCollection<CommandId> All => Handled;

        private static HashSet<CommandId> BuildHandledSet()
        {
            var set = new HashSet<CommandId>
            {
                // Options / Preferences (MainWindow.Preferences)
                CommandId.Options,

                // File / document lifecycle (MainWindow.axaml.cs)
                CommandId.NewPost,
                CommandId.NewPage,
                CommandId.SavePost,
                CommandId.OpenDrafts,
                CommandId.OpenPost,
                CommandId.DeleteDraft,

                // Accounts / publishing (MainWindow.Publishing)
                CommandId.AddWeblog,
                CommandId.ConfigureWeblog,
                CommandId.Accounts,
                CommandId.SelectBlog,
                CommandId.ShowCategoryPopup,
                CommandId.PostAndPublish,
                CommandId.PostAsDraft,
                CommandId.PostAsDraftAndEditOnline,

                // Insert tab (MainWindow.Insert)
                CommandId.InsertTable,
                CommandId.InsertTable2,
                CommandId.InsertVideoSplit,
                CommandId.InsertVideoFromWeb,
                CommandId.InsertVideoFromFile,
                CommandId.InsertVideoFromService,
                CommandId.InsertEmoticon,
                CommandId.InsertMap,
                CommandId.InsertTags,
                CommandId.EditTags,
                CommandId.Paste,
                CommandId.PasteSpecial,

                // Spelling (MainWindow.Spelling)
                CommandId.CheckSpelling,
                CommandId.OpenSpellingForm,

                // Plug-ins — informational "not available on macOS" dialog (MainWindow.Plugins)
                CommandId.AddPlugin,
                CommandId.ManagePlugins,

                // Shell utilities (MainWindow.axaml.cs / MainWindow.MenuBar)
                CommandId.WordCount,
                CommandId.FindButton,
                CommandId.FindAndReplace,
                CommandId.About,
                CommandId.Close,
                CommandId.ViewNormal,
                CommandId.ViewSource,
                CommandId.ViewPreview,

                // Editor bridge (WebViewEditor.HandleCommandAsync)
                CommandId.Bold,
                CommandId.Italic,
                CommandId.Underline,
                CommandId.Strikethrough,
                CommandId.Subscript,
                CommandId.Superscript,
                CommandId.ClearFormatting,
                CommandId.Bullets,
                CommandId.Numbers,
                CommandId.Indent,
                CommandId.Outdent,
                CommandId.AlignLeft,
                CommandId.AlignCenter,
                CommandId.AlignRight,
                CommandId.Justify,
                CommandId.Blockquote,
                CommandId.Undo,
                CommandId.Redo,
                CommandId.SelectAll,
                CommandId.Cut,
                CommandId.CopyCommand,
                CommandId.InsertHorizontalLine,
                CommandId.InsertClearBreak,
                CommandId.InsertExtendedEntry,
                CommandId.InsertRowAbove,
                CommandId.InsertRowBelow,
                CommandId.InsertColumnLeft,
                CommandId.InsertColumnRight,
                CommandId.DeleteRow,
                CommandId.DeleteColumn,
                CommandId.DeleteTable,

                // EditorPanel CommandBridge
                CommandId.InsertLink,
                CommandId.InsertPictureFromFile,
                CommandId.InsertImageSplit,
            };

            // Draft MRU slots 0–9 (MainWindow.axaml.cs).
            for (CommandId mru = CommandId.OpenDraftMRU0; mru <= CommandId.OpenDraftMRU9; mru++)
                set.Add(mru);

            return set;
        }
    }
}
