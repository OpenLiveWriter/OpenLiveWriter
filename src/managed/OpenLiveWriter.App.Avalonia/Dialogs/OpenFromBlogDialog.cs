// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Open from Blog dialog: lists recent posts (or pages) fetched from the current
    /// blog so one can be opened for local editing. Mirrors the Windows OpenPostForm
    /// behavior — a Posts/Pages toggle and a how-many selector (10/25/50) — scoped to
    /// the single current account (the blog the shell is pointed at).
    ///
    /// The fetch itself is injected as a delegate so the dialog stays transport- and
    /// test-friendly: production passes the live <see cref="IBlogClient"/> calls, tests
    /// pass an in-memory fake. Loading shows progress inline; a fetch failure (offline,
    /// bad credentials) degrades to an inline error with a Retry button — never a crash.
    /// Double-click or Open returns the selected <see cref="ServerPost"/>; Cancel null.
    /// </summary>
    public class OpenFromBlogDialog : Window
    {
        /// <summary>Fetches entries for the picker: pages-vs-posts plus the max count.</summary>
        public delegate Task<IReadOnlyList<ServerPost>> FetchPosts(bool pages, int count);

        private static readonly int[] CountChoices = { 10, 25, 50 };

        private readonly FetchPosts _fetch;
        private readonly ListBox _list;
        private readonly TextBlock _status;
        private readonly Button _openButton;
        private readonly Button _retryButton;
        private readonly RadioButton _postsRadio;
        private readonly RadioButton _pagesRadio;
        private readonly ComboBox _countCombo;
        private bool _loading;
        private bool _suppressReload;

        /// <summary>The post the user chose, or null if cancelled.</summary>
        public ServerPost SelectedPost { get; private set; }

        public OpenFromBlogDialog(FetchPosts fetch, bool supportsPages = true)
        {
            _fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));

            Title = "Open from Blog";
            Width = 560;
            Height = 420;
            MinWidth = 420;
            MinHeight = 300;
            CanResize = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _postsRadio = new RadioButton { Content = "Posts", IsChecked = true, GroupName = "ofb-kind" };
            _pagesRadio = new RadioButton { Content = "Pages", IsEnabled = supportsPages, GroupName = "ofb-kind" };
            _countCombo = new ComboBox { MinWidth = 64 };
            foreach (int n in CountChoices)
                _countCombo.Items.Add(n.ToString(CultureInfo.InvariantCulture));
            _countCombo.SelectedIndex = 1; // default 25

            var optionsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Margin = new global::Avalonia.Thickness(0, 0, 0, 8)
            };
            optionsRow.Children.Add(_postsRadio);
            optionsRow.Children.Add(_pagesRadio);
            optionsRow.Children.Add(new TextBlock
            {
                Text = "Show:",
                VerticalAlignment = VerticalAlignment.Center
            });
            optionsRow.Children.Add(_countCombo);

            _status = new TextBlock
            {
                Text = string.Empty,
                FontSize = 11,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                Foreground = new global::Avalonia.Media.SolidColorBrush(
                    global::Avalonia.Media.Color.FromRgb(0x66, 0x66, 0x66)),
                Margin = new global::Avalonia.Thickness(0, 0, 0, 8)
            };

            _list = new ListBox { Margin = new global::Avalonia.Thickness(0, 0, 0, 12) };

            _openButton = new Button { Content = "Open", IsDefault = true, MinWidth = 80, IsEnabled = false };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };
            _retryButton = new Button { Content = "Retry", MinWidth = 80, IsVisible = false };

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };
            buttonRow.Children.Add(_retryButton);
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_openButton);

            var layout = new DockPanel { Margin = new global::Avalonia.Thickness(16) };
            DockPanel.SetDock(optionsRow, Dock.Top);
            DockPanel.SetDock(_status, Dock.Top);
            DockPanel.SetDock(buttonRow, Dock.Bottom);
            layout.Children.Add(optionsRow);
            layout.Children.Add(_status);
            layout.Children.Add(buttonRow);
            layout.Children.Add(_list);

            Content = layout;

            _postsRadio.IsCheckedChanged += (s, e) => OnOptionsChanged();
            _pagesRadio.IsCheckedChanged += (s, e) => OnOptionsChanged();
            _countCombo.SelectionChanged += (s, e) => OnOptionsChanged();
            _list.SelectionChanged += (s, e) => _openButton.IsEnabled = !_loading && _list.SelectedItem != null;
            _list.DoubleTapped += (s, e) => { if (_list.SelectedItem != null) Confirm(); };
            _openButton.Click += (s, e) => Confirm();
            _retryButton.Click += async (s, e) => await RefreshAsync();
            cancelButton.Click += (s, e) => Close(null);

            Opened += async (s, e) => await RefreshAsync();
        }

        /// <summary>True while a fetch is in flight (test/diagnostic seam).</summary>
        public bool IsLoading => _loading;

        /// <summary>The entries currently listed (test seam).</summary>
        public IReadOnlyList<ServerPost> ListedPosts
        {
            get
            {
                var posts = new List<ServerPost>();
                foreach (object item in _list.Items)
                {
                    if ((item as ListBoxItem)?.Tag is ServerPost post)
                        posts.Add(post);
                }
                return posts;
            }
        }

        /// <summary>The current status line (progress / error / empty-state text).</summary>
        public string StatusText => _status.Text;

        private bool ShowPages => _pagesRadio.IsChecked == true;

        private int SelectedCount =>
            _countCombo.SelectedIndex >= 0 && _countCombo.SelectedIndex < CountChoices.Length
                ? CountChoices[_countCombo.SelectedIndex]
                : CountChoices[1];

        private void OnOptionsChanged()
        {
            // Ignore the radio-pair transient (unchecking the old button) and programmatic
            // initialization; only reload on a settled user change once loading finished.
            if (_loading || _suppressReload)
                return;
            _ = RefreshAsync();
        }

        /// <summary>
        /// Switches the Posts/Pages + count selection and reloads, awaiting completion.
        /// Internal test seam: headless tests use this instead of poking the radios,
        /// whose change handlers reload fire-and-forget.
        /// </summary>
        internal async Task RefreshForOptionsAsync(bool pages, int countIndex)
        {
            _suppressReload = true;
            try
            {
                _postsRadio.IsChecked = !pages;
                _pagesRadio.IsChecked = pages;
                _countCombo.SelectedIndex = countIndex;
            }
            finally
            {
                _suppressReload = false;
            }
            await RefreshAsync();
        }

        /// <summary>
        /// (Re)loads the list for the current Posts/Pages + count selection. Internal so
        /// headless tests can drive a load without showing the window. Never throws:
        /// fetch failures land in the status line with a Retry button.
        /// </summary>
        internal async Task RefreshAsync()
        {
            if (_loading)
                return;
            _loading = true;
            _openButton.IsEnabled = false;
            _retryButton.IsVisible = false;
            _list.Items.Clear();
            _status.Text = ShowPages ? "Loading pages\u2026" : "Loading recent posts\u2026";

            try
            {
                IReadOnlyList<ServerPost> posts =
                    await _fetch(ShowPages, SelectedCount).ConfigureAwait(true) ?? Array.Empty<ServerPost>();

                foreach (ServerPost post in posts)
                {
                    _list.Items.Add(new ListBoxItem
                    {
                        Content = FormatEntry(post),
                        Tag = post
                    });
                }

                _status.Text = _list.Items.Count == 0
                    ? (ShowPages ? "No pages found on this blog." : "No recent posts found on this blog.")
                    : string.Empty;
            }
            catch (Exception ex)
            {
                _status.Text = $"Couldn't load from the blog: {ex.Message}";
                _retryButton.IsVisible = true;
            }
            finally
            {
                _loading = false;
                _openButton.IsEnabled = _list.SelectedItem != null;
            }
        }

        private void Confirm()
        {
            SelectedPost = (_list.SelectedItem as ListBoxItem)?.Tag as ServerPost;
            Close(SelectedPost);
        }

        private static string FormatEntry(ServerPost post)
        {
            string title = string.IsNullOrWhiteSpace(post.Title) ? "(untitled)" : post.Title;
            string when = post.DateCreatedUtc.HasValue
                ? post.DateCreatedUtc.Value.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)
                : "no date";
            string status = string.IsNullOrEmpty(post.Status) ? string.Empty : $"   [{post.Status}]";
            return $"{title}   \u2014   {when}{status}";
        }

        /// <summary>
        /// Shows the picker modally for <paramref name="client"/>/<paramref name="blogId"/>
        /// and returns the chosen post, or null if cancelled / no owner window. The fetch
        /// delegate routes posts vs pages to metaWeblog.getRecentPosts / wp.getPages.
        /// </summary>
        public static async Task<ServerPost> ShowAsync(
            Window owner, IBlogClient client, string blogId, bool supportsPages = true)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var dialog = new OpenFromBlogDialog(
                (pages, count) => pages
                    ? client.GetPagesAsync(blogId)
                    : client.GetRecentPostsAsync(blogId, count),
                supportsPages);

            if (owner == null) return null;
            return await dialog.ShowDialog<ServerPost>(owner);
        }
    }
}
