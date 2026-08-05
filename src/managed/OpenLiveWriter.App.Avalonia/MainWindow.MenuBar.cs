// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
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

        // Menu-bar commands routed here: About / Close Window / view switching, plus
        // Print / Print Preview (MainWindow.Print) and Post Properties (publish date).
        // Print and Post Properties also have ribbon entry points (ribbon File menu);
        // both surfaces share this single handler chain.
        private async Task<bool> TryHandleMenuCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.About:
                    await ShowAboutAsync();
                    return true;
                case CommandId.CheckForUpdates:
                    await CheckForUpdatesAsync();
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
                case CommandId.ClosePreview:
                    // Preview tab's Close Preview: on Windows this only switches the
                    // editor back to the Edit view — same here.
                    SetEditorView("edit");
                    return true;
                case CommandId.Print:
                    await PrintCurrentAsync();
                    return true;
                case CommandId.PrintPreview:
                    await PrintPreviewCurrentAsync();
                    return true;
                case CommandId.PostProperties:
                    await ShowPostPropertiesAsync();
                    return true;
                default:
                    return false;
            }
        }

        private void SetEditorView(string view) =>
            this.FindControl<EditorPanel>("EditorPanel")?.SetView(view);

        // F2 / F7 shortcuts. These must NOT be NativeMenu gestures: a bare
        // function-key gesture (no modifiers) silently breaks Avalonia's macOS
        // native menu export — the whole menu bar disappears. Window-level
        // keys reach the same handlers instead (verified 2026-07).
        protected override void OnKeyDown(global::Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled)
                return;

            switch (e.Key)
            {
                case global::Avalonia.Input.Key.F2:
                    _ = ExecuteCommandAsync(CommandId.PostProperties);
                    e.Handled = true;
                    break;
                case global::Avalonia.Input.Key.F7:
                    _ = ExecuteCommandAsync(CommandId.CheckSpelling);
                    e.Handled = true;
                    break;
            }
        }

        // Post Properties (F2): publish date plus slug/excerpt/ping URLs. The values
        // are stored on the draft and sent as dateCreated / wp_slug / mt_excerpt /
        // mt_tb_ping_urls on publish.
        private async Task ShowPostPropertiesAsync()
        {
            if (_draftSession == null)
                return;

            PostPropertiesDialogResult result = await PostPropertiesDialog.ShowAsync(
                this, _draftSession.Current.PublishDateUtc,
                _draftSession.Current.Slug, _draftSession.Current.Excerpt,
                _draftSession.Current.PingUrls);
            if (result == null)
                return;

            _draftSession.Current.PublishDateUtc = result.PublishDateUtc;
            _draftSession.Current.Slug = result.Slug ?? string.Empty;
            _draftSession.Current.Excerpt = result.Excerpt ?? string.Empty;
            _draftSession.Current.PingUrls = result.PingUrls ?? new List<string>();
            _draftSession.Current.IsDirty = true;
            UpdateStatus(result.PublishDateUtc.HasValue
                ? $"Publish date: {result.PublishDateUtc.Value.ToLocalTime():f}"
                : "Publish date cleared — the post publishes immediately.");
        }

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

        // The startup check is silent by design; this one reports, because the
        // user asked.
        private async Task CheckForUpdatesAsync()
        {
            string staged = await AppUpdater.CheckAsync();
            if (staged != null)
            {
                await MessageDialog.ShowAsync(this, "Check for Updates",
                    $"Version {staged} has been downloaded and will be installed the next time Open Live Writer starts.");
                return;
            }

            // CheckAsync returns null for up-to-date, not-installed and failed
            // alike, so distinguish them here for the message.
            await MessageDialog.ShowAsync(this, "Check for Updates",
                AppUpdater.IsUpdatable
                    ? "Open Live Writer is up to date."
                    : "This copy was not installed, so it cannot update itself. "
                      + "Download an installer to receive updates.");
        }
    }
}
