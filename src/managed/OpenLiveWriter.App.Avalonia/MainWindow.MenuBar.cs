// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Reflection;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// macOS menu-bar integration: builds a <see cref="NativeMenu"/> from
    /// <see cref="ShellMenuBuilder"/> descriptors and routes every item through the
    /// same <see cref="ExecuteCommandAsync"/> chain the ribbon uses. Also owns the
    /// menu-only commands (About / Close Window / Edit-Source-Preview switching).
    /// </summary>
    public partial class MainWindow
    {
        private void InitializeMenuBar()
        {
            var menu = new NativeMenu();
            foreach (ShellMenu shellMenu in ShellMenuBuilder.Build())
            {
                var subMenu = new NativeMenu();
                foreach (ShellMenuItem item in shellMenu.Items)
                {
                    if (item.IsSeparator)
                    {
                        subMenu.Add(new NativeMenuItemSeparator());
                        continue;
                    }

                    var nativeItem = new NativeMenuItem(item.Label);
                    if (!string.IsNullOrEmpty(item.Gesture))
                        nativeItem.Gesture = KeyGesture.Parse(item.Gesture);

                    CommandId commandId = item.CommandId;
                    nativeItem.Click += async (s, e) => await ExecuteCommandAsync(commandId);
                    subMenu.Add(nativeItem);
                }
                menu.Add(new NativeMenuItem(shellMenu.Label) { Menu = subMenu });
            }

            NativeMenu.SetMenu(this, menu);
        }

        // Menu-only commands that have no ribbon entry point.
        private async Task<bool> TryHandleMenuCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.About:
                    await ShowAboutAsync();
                    return true;
                case CommandId.Close:
                    Close();
                    return true;
                case CommandId.ViewNormal:
                    SetEditorView("edit");
                    return true;
                case CommandId.ViewSource:
                    SetEditorView("source");
                    return true;
                case CommandId.ViewPreview:
                    SetEditorView("preview");
                    return true;
                default:
                    return false;
            }
        }

        private void SetEditorView(string view) =>
            this.FindControl<EditorPanel>("EditorPanel")?.SetView(view);

        private async Task ShowAboutAsync()
        {
            string version = typeof(App).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
                ?? typeof(App).Assembly.GetName().Version?.ToString()
                ?? "dev";

            await MessageDialog.ShowAsync(this, "About Open Live Writer",
                $"Open Live Writer for macOS\nVersion {version}\n\n" +
                "Open-source blog authoring (MetaWeblog), ported to macOS with Avalonia.");
        }
    }
}
