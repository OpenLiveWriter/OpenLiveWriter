// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// A single entry in a shell menu: a label, the command it routes (through the
    /// same handler chain as ribbon buttons), and an optional keyboard gesture in
    /// Avalonia <c>KeyGesture</c> text form (e.g. "Cmd+S"). Separators carry no
    /// command. Deliberately free of Avalonia types so headless tests can assert
    /// the menu structure without a UI thread.
    /// </summary>
    public sealed class ShellMenuItem
    {
        private ShellMenuItem(string label, CommandId commandId, string gesture, bool isSeparator)
        {
            Label = label;
            CommandId = commandId;
            Gesture = gesture;
            IsSeparator = isSeparator;
        }

        public string Label { get; }
        public CommandId CommandId { get; }
        public string Gesture { get; }
        public bool IsSeparator { get; }

        public static ShellMenuItem Command(string label, CommandId commandId, string gesture = null) =>
            new ShellMenuItem(label, commandId, gesture, isSeparator: false);

        public static ShellMenuItem Separator() =>
            new ShellMenuItem(null, CommandId.None, null, isSeparator: true);
    }

    /// <summary>A top-level menu (File / Edit / View / Help) with its items.</summary>
    public sealed class ShellMenu
    {
        public ShellMenu(string label, IReadOnlyList<ShellMenuItem> items)
        {
            Label = label;
            Items = items;
        }

        public string Label { get; }
        public IReadOnlyList<ShellMenuItem> Items { get; }
    }

    /// <summary>
    /// Builds the macOS menu-bar structure (rendered as a <c>NativeMenu</c> by
    /// MainWindow). Menus only reference commands the shell actually handles —
    /// menu clicks route through <c>MainWindow.ExecuteCommandAsync</c>, the same
    /// chain as ribbon buttons, so there is no duplicated command logic.
    /// </summary>
    public static class ShellMenuBuilder
    {
        public static IReadOnlyList<ShellMenu> Build()
        {
            var file = new ShellMenu("File", new[]
            {
                ShellMenuItem.Command("New Post", CommandId.NewPost, "Cmd+N"),
                ShellMenuItem.Command("New Page", CommandId.NewPage, "Cmd+Shift+N"),
                ShellMenuItem.Command("Open Draft\u2026", CommandId.OpenDrafts, "Cmd+O"),
                ShellMenuItem.Command("Save Draft", CommandId.SavePost, "Cmd+S"),
                ShellMenuItem.Command("Delete Draft", CommandId.DeleteDraft),
                ShellMenuItem.Separator(),
                ShellMenuItem.Command("Set Categories\u2026", CommandId.ShowCategoryPopup),
                ShellMenuItem.Separator(),
                ShellMenuItem.Command("Preferences\u2026", CommandId.Options, "Cmd+,"),
                ShellMenuItem.Command("About Open Live Writer", CommandId.About),
                ShellMenuItem.Command("Close Window", CommandId.Close, "Cmd+W"),
            });

            var edit = new ShellMenu("Edit", new[]
            {
                ShellMenuItem.Command("Undo", CommandId.Undo, "Cmd+Z"),
                ShellMenuItem.Command("Redo", CommandId.Redo, "Cmd+Shift+Z"),
                ShellMenuItem.Separator(),
                ShellMenuItem.Command("Cut", CommandId.Cut, "Cmd+X"),
                ShellMenuItem.Command("Copy", CommandId.CopyCommand, "Cmd+C"),
                ShellMenuItem.Command("Paste", CommandId.Paste, "Cmd+V"),
                ShellMenuItem.Command("Paste Special", CommandId.PasteSpecial, "Cmd+Shift+V"),
                ShellMenuItem.Command("Select All", CommandId.SelectAll, "Cmd+A"),
                ShellMenuItem.Separator(),
                ShellMenuItem.Command("Find\u2026", CommandId.FindButton, "Cmd+F"),
            });

            var view = new ShellMenu("View", new[]
            {
                ShellMenuItem.Command("Edit", CommandId.ViewNormal),
                ShellMenuItem.Command("Source", CommandId.ViewSource),
                ShellMenuItem.Command("Preview", CommandId.ViewPreview),
            });

            var help = new ShellMenu("Help", new[]
            {
                ShellMenuItem.Command("About Open Live Writer", CommandId.About),
            });

            return new[] { file, edit, view, help };
        }
    }
}
