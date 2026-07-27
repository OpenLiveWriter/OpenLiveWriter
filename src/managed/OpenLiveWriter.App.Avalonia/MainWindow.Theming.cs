// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Theming;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Platform;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Theme-based preview for the shell: wires the Blog Account tab's "Use Theme"
    /// (<see cref="CommandId.ViewUseStyles"/>, a per-account persisted toggle) and
    /// "Update Theme" (<see cref="CommandId.UpdateWeblogStyle"/>, a forced re-harvest)
    /// onto the <see cref="ThemeStyleCache"/>. When the toggle is on, the Preview view
    /// layers the blog's homepage stylesheets over the neutral article style; any fetch
    /// failure degrades to the neutral preview with a status-bar message.
    /// </summary>
    public partial class MainWindow
    {
        private ThemeStyleCache _themeCache;

        private void InitializeTheming()
        {
            IThemeHtmlFetcher fetcher = new HttpThemeHtmlFetcher(CreatePublishingHttpClient());

            // Disk cache under the platform app-data dir when available; memory-only
            // otherwise (theme harvesting still works, it just re-fetches per session).
            string cacheDir = null;
            try
            {
                PlatformContext.EnsureInitialized();
                cacheDir = Path.Combine(
                    PlatformContext.Services.GetApplicationDataDirectory(), "Themes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Theming] Disk cache unavailable (memory only): {ex.Message}");
            }

            _themeCache = new ThemeStyleCache(fetcher, cacheDir);

            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel != null)
                editorPanel.PreviewThemeProvider = GetPreviewThemeAsync;
        }

        // Test seams: substitute a temp-store account service / fake-fetcher cache so
        // headless tests never touch the real account store or the network.
        internal BlogAccountService AccountService
        {
            get => _accountService;
            set => _accountService = value;
        }

        internal ThemeStyleCache ThemeCache
        {
            get => _themeCache;
            set => _themeCache = value;
        }

        private async Task<bool> TryHandleThemingCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.ViewUseStyles:
                    await ToggleUseThemeAsync();
                    return true;
                case CommandId.UpdateWeblogStyle:
                    await UpdateThemeAsync();
                    return true;
                default:
                    return false;
            }
        }

        // "Use Theme" (Blog Account tab): per-account toggle, persisted on the account.
        // The ribbon's toggle state mirrors the stored value.
        private async Task ToggleUseThemeAsync()
        {
            BlogAccount account = _accountService?.CurrentAccount;
            if (account == null)
            {
                UpdateStatus("Use Theme: no blog is selected. Add or select a blog account first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(account.HomepageUrl))
            {
                UpdateStatus($"Use Theme: \u201c{account.DisplayLabel}\u201d has no homepage URL to harvest a theme from.");
                return;
            }

            account.UseThemeForPreview = !account.UseThemeForPreview;
            _accountService.SaveAccount(account, password: null); // metadata-only update
            RefreshThemeToggleState();
            UpdateStatus(account.UseThemeForPreview
                ? $"Using the \u201c{account.DisplayLabel}\u201d theme in Preview."
                : "Theme off — Preview uses the neutral style.");
            await RefreshPreviewIfShowingAsync();
        }

        // "Update Theme" (Blog Account tab / Preview tab): force a re-harvest of the
        // current blog's homepage stylesheets. On success the theme is enabled for
        // the account and the Preview view is shown so the result is immediately
        // visible; failures are loud (dialog), not just a status-bar line.
        private async Task UpdateThemeAsync()
        {
            BlogAccount account = _accountService?.CurrentAccount;
            if (account == null)
            {
                UpdateStatus("Update Theme: no blog is selected.");
                await MessageDialog.ShowAsync(this, "Update Theme",
                    "No blog is selected. Add or select a blog account first — the theme is harvested from the blog's homepage.");
                return;
            }
            if (string.IsNullOrWhiteSpace(account.HomepageUrl))
            {
                UpdateStatus($"Update Theme: \u201c{account.DisplayLabel}\u201d has no homepage URL.");
                await MessageDialog.ShowAsync(this, "Update Theme",
                    $"\u201c{account.DisplayLabel}\u201d has no homepage URL to harvest a theme from. Edit the account and add the blog's URL.");
                return;
            }

            UpdateStatus($"Updating theme from {account.HomepageUrl}\u2026");
            BlogThemeStyle theme = await SafeGetThemeAsync(account, forceRefresh: true);
            if (theme == null)
            {
                UpdateStatus($"Update Theme failed: could not fetch {account.HomepageUrl}.");
                await MessageDialog.ShowAsync(this, "Update Theme Failed",
                    $"Could not fetch {account.HomepageUrl}. Check the homepage URL and your connection — Preview keeps the previous style.");
                return;
            }
            if (theme.IsEmpty)
            {
                UpdateStatus($"Update Theme: no stylesheets found on \u201c{account.DisplayLabel}\u201d's homepage.");
                await MessageDialog.ShowAsync(this, "Update Theme",
                    $"No stylesheets were found on \u201c{account.DisplayLabel}\u201d's homepage, so there is no theme to apply.");
                return;
            }

            // Turn the theme on for this account (persisted) so Preview uses it,
            // and bring the Preview view forward to show the result.
            if (!account.UseThemeForPreview)
            {
                account.UseThemeForPreview = true;
                _accountService.SaveAccount(account, password: null); // metadata-only update
            }
            RefreshThemeToggleState();

            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel != null)
                await editorPanel.SetViewAsync("preview");

            UpdateStatus($"Theme updated for \u201c{account.DisplayLabel}\u201d: " +
                $"{theme.StylesheetUrls.Count} stylesheet(s), {theme.InlineStyles.Count} inline style block(s).");
        }

        // EditorPanel's theme provider: returns the current blog's theme only when its
        // "Use Theme" toggle is on; null keeps the neutral preview. A fetch failure
        // surfaces as a status message and a null (neutral) result — never an exception.
        private async Task<BlogThemeStyle> GetPreviewThemeAsync()
        {
            BlogAccount account = _accountService?.CurrentAccount;
            if (account == null || !account.UseThemeForPreview ||
                string.IsNullOrWhiteSpace(account.HomepageUrl))
            {
                return null;
            }

            BlogThemeStyle theme = await SafeGetThemeAsync(account, forceRefresh: false);
            if (theme == null)
            {
                UpdateStatus($"Could not fetch the theme for \u201c{account.DisplayLabel}\u201d — showing the neutral preview.");
                return null;
            }
            if (theme.IsEmpty)
            {
                UpdateStatus($"No theme stylesheets found for \u201c{account.DisplayLabel}\u201d — showing the neutral preview.");
                return null;
            }
            return theme;
        }

        private async Task<BlogThemeStyle> SafeGetThemeAsync(BlogAccount account, bool forceRefresh)
        {
            if (_themeCache == null)
                return null;

            try
            {
                return await _themeCache.GetThemeAsync(account.Id, account.HomepageUrl, forceRefresh);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Theming] Theme fetch failed: {ex.Message}");
                return null;
            }
        }

        // Re-composes the preview when it's the visible surface so a theme toggle or
        // refresh takes effect immediately.
        private Task RefreshPreviewIfShowingAsync()
        {
            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel != null && editorPanel.CurrentView == "preview")
                editorPanel.SetView("preview");
            return Task.CompletedTask;
        }

        // Reflects the current account's persisted "Use Theme" toggle on the ribbon.
        internal void RefreshThemeToggleState()
        {
            _ribbon?.SetToggleState(CommandId.ViewUseStyles,
                _accountService?.CurrentAccount?.UseThemeForPreview == true);
        }
    }
}
