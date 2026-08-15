// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Markdown;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="AccountDialog"/> when the user saves. Carries the
    /// edited account metadata plus the (separately-stored) password. On an edit where
    /// the password field is left blank, <see cref="Password"/> is null so the existing
    /// stored secret is preserved.
    /// </summary>
    public class AccountDialogResult
    {
        public BlogAccount Account { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Add / Configure blog account dialog: blog URL, username, password, and the
    /// MetaWeblog API endpoint. A "Detect" button auto-fills the endpoint (and blog id)
    /// from the blog homepage via RSD discovery; manual override is retained. A
    /// "Test Connection" button verifies the endpoint + credentials live (async,
    /// cancellable, inline result) so bad settings surface before the first publish.
    /// Mirrors the modal <c>ShowAsync(owner)</c> pattern used by the other shell dialogs.
    /// </summary>
    public class AccountDialog : Window
    {
        private readonly TextBox _displayNameBox;
        private readonly TextBox _homepageBox;
        private readonly TextBox _endpointBox;
        private readonly TextBox _blogIdBox;
        private readonly TextBox _usernameBox;
        private readonly TextBox _passwordBox;
        private readonly ComboBox _editingFormatCombo;
        private readonly ComboBox _publishFormatCombo;
        private readonly Button _saveButton;
        private readonly Button _detectButton;
        private readonly Button _testButton;
        private readonly TextBlock _detectStatus;
        private readonly TextBlock _providerLabel;
        private readonly IRsdHttpFetcher _fetcher;
        private readonly IBlogConnectionVerifier _verifier;
        private readonly IDraftStore _draftStore;
        private readonly IMarkdownService _markdown;
        private readonly BlogAccount _existing;
        private readonly ContentFormat _originalEditingFormat;
        private readonly string _existingId;
        private readonly bool _isEdit;
        private string _providerType;
        private CancellationTokenSource _testCts;

        public AccountDialogResult Result { get; private set; }

        public AccountDialog(BlogAccount existing = null, IRsdHttpFetcher fetcher = null,
            IBlogConnectionVerifier verifier = null, IDraftStore draftStore = null,
            IMarkdownService markdown = null)
        {
            _fetcher = fetcher ?? new HttpRsdFetcher();
            _verifier = verifier ?? new MetaWeblogConnectionVerifier();
            _draftStore = draftStore;
            _markdown = markdown;
            _existing = existing;
            _isEdit = existing != null;
            _existingId = existing?.Id ?? string.Empty;
            _originalEditingFormat = existing?.EditingFormat ?? ContentFormat.Html;
            _providerType = existing?.ProviderType ?? BlogAccount.DefaultProviderType;

            Title = _isEdit ? "Blog Account Settings" : "Add a Blog Account";
            Width = 480;
            MinWidth = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _displayNameBox = new TextBox { Text = existing?.DisplayName ?? string.Empty };
            _homepageBox = new TextBox
            {
                PlaceholderText = "https://",
                Text = existing?.HomepageUrl ?? string.Empty
            };
            _endpointBox = new TextBox
            {
                PlaceholderText = "https://example.com/xmlrpc.php",
                Text = existing?.ApiEndpointUrl ?? string.Empty
            };
            _blogIdBox = new TextBox { Text = existing?.BlogId ?? string.Empty };
            _usernameBox = new TextBox { Text = existing?.Username ?? string.Empty };
            _passwordBox = new TextBox
            {
                PasswordChar = '\u2022',
                Text = string.Empty
            };

            _editingFormatCombo = CreateFormatCombo(existing?.EditingFormat ?? ContentFormat.Html);
            _publishFormatCombo = CreateFormatCombo(existing?.PublishFormat ?? ContentFormat.Html);
            _editingFormatCombo.SelectionChanged += (s, e) => UpdatePublishFormatState();
            UpdatePublishFormatState();

            _saveButton = new Button
            {
                Content = "Save",
                IsDefault = true,
                MinWidth = 80,
                IsEnabled = false
            };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            _detectButton = new Button { Content = "Detect", MinWidth = 80 };
            _detectStatus = new TextBlock
            {
                Text = string.Empty,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                Foreground = new global::Avalonia.Media.SolidColorBrush(
                    global::Avalonia.Media.Color.FromRgb(0x66, 0x66, 0x66))
            };
            _detectButton.Click += async (s, e) => await DetectAsync();

            _testButton = new Button { Content = "Test Connection", MinWidth = 110, IsEnabled = false };
            _testButton.Click += async (s, e) => await TestConnectionAsync();

            _saveButton.Click += async (s, e) => await SaveAsync();
            cancelButton.Click += (s, e) => Close(null);

            void Revalidate(object s, EventArgs e)
            {
                _saveButton.IsEnabled = CanSave();
                _testButton.IsEnabled = CanTestConnection(
                    _endpointBox.Text, _usernameBox.Text, _passwordBox.Text);
            }
            _endpointBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) Revalidate(s, e); };
            _usernameBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) Revalidate(s, e); };
            _passwordBox.PropertyChanged += (s, e) => { if (e.Property == TextBox.TextProperty) Revalidate(s, e); };
            _saveButton.IsEnabled = CanSave();
            _testButton.IsEnabled = CanTestConnection(
                _endpointBox.Text, _usernameBox.Text, _passwordBox.Text);

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*")
            };

            AddRow(grid, 0, "Name:", _displayNameBox);
            AddRow(grid, 1, "Blog URL:", _homepageBox);
            AddRow(grid, 2, "API endpoint:", _endpointBox);
            AddRow(grid, 3, "Blog ID:", _blogIdBox);
            AddRow(grid, 4, "Username:", _usernameBox);
            AddRow(grid, 5, "Password:", _passwordBox);

            // Provider is set by RSD detection (WordPress when the engine says so);
            // read-only here — the transport picks the matching client from it.
            _providerLabel = new TextBlock
            {
                Text = _providerType,
                VerticalAlignment = VerticalAlignment.Center
            };
            AddRow(grid, 6, "Provider:", _providerLabel);
            AddRow(grid, 7, "Content format:", _editingFormatCombo);
            AddRow(grid, 8, "Publish as:", _publishFormatCombo);

            // Detect/Test row: pull the endpoint/blog id from the Blog URL via RSD
            // discovery, or verify the entered endpoint + credentials live.
            var detectRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };
            detectRow.Children.Add(_detectButton);
            detectRow.Children.Add(_testButton);
            detectRow.Children.Add(_detectStatus);
            Grid.SetRow(detectRow, 9);
            Grid.SetColumn(detectRow, 0);
            Grid.SetColumnSpan(detectRow, 2);
            grid.Children.Add(detectRow);

            var hint = new TextBlock
            {
                Text = "Enter your blog's MetaWeblog XML-RPC endpoint (e.g. .../xmlrpc.php), "
                     + "or click Detect to discover it from the Blog URL.",
                FontSize = 11,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                Foreground = new global::Avalonia.Media.SolidColorBrush(
                    global::Avalonia.Media.Color.FromRgb(0x66, 0x66, 0x66)),
                Margin = new global::Avalonia.Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(hint, 10);
            Grid.SetColumn(hint, 0);
            Grid.SetColumnSpan(hint, 2);
            grid.Children.Add(hint);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_saveButton);
            Grid.SetRow(buttonRow, 11);
            Grid.SetColumn(buttonRow, 0);
            Grid.SetColumnSpan(buttonRow, 2);
            grid.Children.Add(buttonRow);

            Content = grid;
        }

        // Runs RSD auto-detection off the Blog URL and fills in the endpoint (and blog id,
        // when the RSD provides one). The network fetch runs on a background thread via the
        // injected fetcher; the endpoint field stays editable so the user can override.
        private async Task DetectAsync()
        {
            string homepage = _homepageBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(homepage))
            {
                _detectStatus.Text = "Enter your Blog URL first.";
                return;
            }

            _detectButton.IsEnabled = false;
            _detectStatus.Text = "Detecting\u2026";
            SetStatusColor(0x66, 0x66, 0x66);
            try
            {
                RsdDetectionResult result = await Task.Run(
                    () => RsdServiceDetector.Detect(homepage, _fetcher));

                if (result.Found)
                {
                    _endpointBox.Text = result.EndpointUrl;
                    if (!string.IsNullOrEmpty(result.BlogId) && string.IsNullOrWhiteSpace(_blogIdBox.Text))
                        _blogIdBox.Text = result.BlogId;
                    _providerType = string.IsNullOrEmpty(result.ProviderType)
                        ? BlogAccount.DefaultProviderType
                        : result.ProviderType;
                    _providerLabel.Text = _providerType;
                    _detectStatus.Text = string.IsNullOrEmpty(result.EngineName)
                        ? "Found the API endpoint."
                        : $"Detected {result.EngineName}.";
                    _saveButton.IsEnabled = CanSave();
                }
                else
                {
                    _detectStatus.Text = "Couldn't detect the endpoint. Enter it manually.";
                }
            }
            catch (Exception ex)
            {
                _detectStatus.Text = $"Detection failed: {ex.Message}";
            }
            finally
            {
                _detectButton.IsEnabled = true;
            }
        }

        // Verifies the entered endpoint + credentials with a lightweight live call
        // (blogger.getUsersBlogs via the injected verifier) and reports the outcome
        // inline — never a modal. A new click cancels any in-flight attempt; the
        // pending attempt is also cancelled when the dialog closes. Network errors
        // are caught and shown, never thrown.
        private async Task TestConnectionAsync()
        {
            string endpoint = _endpointBox.Text?.Trim();
            string username = _usernameBox.Text?.Trim();
            string password = _passwordBox.Text ?? string.Empty;

            if (!CanTestConnection(endpoint, username, password))
                return;

            _testCts?.Cancel();
            _testCts?.Dispose();
            var cts = new CancellationTokenSource();
            _testCts = cts;

            _testButton.IsEnabled = false;
            _detectStatus.Text = "Testing connection\u2026";
            SetStatusColor(0x66, 0x66, 0x66);
            try
            {
                await _verifier.VerifyAsync(endpoint, username, password, cts.Token);
                _detectStatus.Text = "Connection succeeded.";
                SetStatusColor(0x2E, 0x7D, 0x32);
            }
            catch (OperationCanceledException)
            {
                _detectStatus.Text = "Connection test cancelled.";
                SetStatusColor(0x66, 0x66, 0x66);
            }
            catch (Exception ex)
            {
                _detectStatus.Text = $"Connection failed: {ex.Message}";
                SetStatusColor(0xC6, 0x28, 0x28);
            }
            finally
            {
                if (ReferenceEquals(_testCts, cts))
                {
                    _testCts = null;
                    _testButton.IsEnabled = CanTestConnection(
                        _endpointBox.Text, _usernameBox.Text, _passwordBox.Text);
                }
                cts.Dispose();
            }
        }

        private void SetStatusColor(byte r, byte g, byte b)
        {
            _detectStatus.Foreground = new global::Avalonia.Media.SolidColorBrush(
                global::Avalonia.Media.Color.FromRgb(r, g, b));
        }

        private async Task SaveAsync()
        {
            BlogAccount account = BuildAccountFromFields();
            ContentFormat newEditingFormat = account.EditingFormat;

            if (_isEdit
                && _originalEditingFormat == ContentFormat.Html
                && newEditingFormat == ContentFormat.Markdown
                && _draftStore != null
                && _markdown != null
                && DraftConversion.HasDraftsForBlog(_draftStore, account.BlogId))
            {
                YesNoCancelResult choice = await ConfirmDialog.ShowYesNoCancelAsync(
                    this,
                    "Convert Drafts to Markdown",
                    "This blog has saved local drafts in HTML format. "
                    + "Convert them to Markdown now?\n\n"
                    + "Yes converts existing drafts. No keeps them as HTML until opened. "
                    + "Cancel returns to the account settings without saving.");

                if (choice == YesNoCancelResult.Cancel)
                    return;

                if (choice == YesNoCancelResult.Yes)
                    DraftConversion.ConvertBlogDraftsToMarkdown(_draftStore, account.BlogId, _markdown);
            }

            Result = new AccountDialogResult
            {
                Account = account,
                Password = string.IsNullOrEmpty(_passwordBox.Text)
                    ? (_isEdit ? null : string.Empty)
                    : _passwordBox.Text
            };
            Close(Result);
        }

        private BlogAccount BuildAccountFromFields()
        {
            return new BlogAccount
            {
                Id = _existingId,
                DisplayName = _displayNameBox.Text?.Trim() ?? string.Empty,
                HomepageUrl = _homepageBox.Text?.Trim() ?? string.Empty,
                ApiEndpointUrl = _endpointBox.Text?.Trim() ?? string.Empty,
                BlogId = _blogIdBox.Text?.Trim() ?? string.Empty,
                Username = _usernameBox.Text?.Trim() ?? string.Empty,
                ProviderType = _providerType ?? BlogAccount.DefaultProviderType,
                SupportsPages = _existing?.SupportsPages ?? true,
                SupportsCategories = _existing?.SupportsCategories ?? true,
                SupportsExtendedEntries = _existing?.SupportsExtendedEntries ?? true,
                UseThemeForPreview = _existing?.UseThemeForPreview ?? false,
                EditingFormat = GetFormatFromCombo(_editingFormatCombo),
                PublishFormat = GetFormatFromCombo(_publishFormatCombo)
            };
        }

        private void UpdatePublishFormatState()
        {
            if (GetFormatFromCombo(_editingFormatCombo) == ContentFormat.Html)
            {
                _publishFormatCombo.SelectedIndex = 0;
                _publishFormatCombo.IsEnabled = false;
            }
            else
            {
                _publishFormatCombo.IsEnabled = true;
            }
        }

        private static ComboBox CreateFormatCombo(ContentFormat selected)
        {
            var combo = new ComboBox { MinWidth = 160 };
            combo.Items.Add("HTML");
            combo.Items.Add("Markdown");
            combo.SelectedIndex = selected == ContentFormat.Markdown ? 1 : 0;
            return combo;
        }

        private static ContentFormat GetFormatFromCombo(ComboBox combo)
        {
            return combo?.SelectedIndex == 1 ? ContentFormat.Markdown : ContentFormat.Html;
        }

        /// <summary>
        /// Test Connection is enabled only when all three inputs are present — a blank
        /// endpoint, username, or password could never succeed (on edit, a blank
        /// password means "keep the existing one", which the test cannot use).
        /// </summary>
        internal static bool CanTestConnection(string endpoint, string username, string password)
        {
            return !string.IsNullOrWhiteSpace(endpoint)
                && !string.IsNullOrWhiteSpace(username)
                && !string.IsNullOrEmpty(password);
        }

        protected override void OnClosed(EventArgs e)
        {
            _testCts?.Cancel();
            base.OnClosed(e);
        }

        private bool CanSave() => CanSave(
            _endpointBox.Text, _usernameBox.Text, _passwordBox.Text, _isEdit);

        /// <summary>
        /// Save is enabled when a non-trivial endpoint URL and username are present, and
        /// (for a new account) a password has been entered. On edit, a blank password is
        /// allowed and means "keep the existing one".
        /// </summary>
        internal static bool CanSave(string endpoint, string username, string password, bool isEdit)
        {
            if (string.IsNullOrWhiteSpace(endpoint)) return false;
            string trimmed = endpoint.Trim();
            if (trimmed.Equals("https://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("http://", StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (!isEdit && string.IsNullOrEmpty(password)) return false;
            return true;
        }

        private static void AddRow(Grid grid, int row, string label, Control field)
        {
            var text = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new global::Avalonia.Thickness(0, 4, 8, 4),
                MinWidth = 90
            };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            field.Margin = new global::Avalonia.Thickness(0, 4, 0, 4);
            Grid.SetRow(field, row);
            Grid.SetColumn(field, 1);
            grid.Children.Add(field);
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the saved
        /// account + password, or null if cancelled / headless (null owner).
        /// </summary>
        public static async Task<AccountDialogResult> ShowAsync(
            Window owner,
            BlogAccount existing = null,
            IRsdHttpFetcher fetcher = null,
            IBlogConnectionVerifier verifier = null,
            IDraftStore draftStore = null,
            IMarkdownService markdown = null)
        {
            var dialog = new AccountDialog(
                existing,
                fetcher,
                verifier,
                draftStore ?? DraftStoreFactory.CreateDefault(),
                markdown ?? new MarkdownService());
            if (owner != null)
                return await dialog.ShowDialog<AccountDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}
