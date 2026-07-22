// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.App.Avalonia.Spelling;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;
using OpenLiveWriter.Publishing.Drafts;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Deep visual inventory for the mac UI parity review: one ribbon-band PNG per main
    /// tab (Insert / Blog Account / Preview / Debug — Home is covered by the base
    /// harness) and one PNG per shell dialog under <c>Dialogs/</c>, all with small
    /// realistic sample data. Same Explicit + Category("UiReview") gating as
    /// <see cref="GroupP_UiReviewCaptureTests"/> so the default test run stays fast.
    /// </summary>
    [TestFixture]
    [Category("UiReview")]
    [Category("GroupP")]
    [Explicit("Writes PNG artifacts; run via scripts/ui-review.sh or --filter Category=UiReview")]
    public class GroupP_UiReviewDeepCaptureTests
    {
        private const string TabSizeTag = "1280x800";

        [SetUp]
        public void SetUp()
        {
            WebViewEditor.UseLayoutPlaceholder = true;
        }

        [TearDown]
        public void TearDown()
        {
            WebViewEditor.UseLayoutPlaceholder = false;
        }

        [AvaloniaTest]
        public void Capture_RibbonTabs_EachMainTab_WritesBandPngs()
        {
            string outDir = UiReviewHarness.ResolveOutputDirectory();
            var window = UiReviewHarness.CreateLaidOutWindow(1280, 800);
            var written = new List<string>();
            var skipped = new List<string>();
            try
            {
                // The Debug tab is no longer a default mode (developer chrome gated
                // behind OLW_DEBUG_RIBBON) — opt into it explicitly for this capture.
                var ribbon = UiReviewHarness.FindRibbon(window);
                ribbon.ActiveModes |= RibbonApplicationMode.Debug;
                ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());
                UiReviewHarness.PumpLayout(window);
                UiReviewHarness.PumpLayout(window);

                foreach (var (label, fileName) in new[]
                {
                    ("Insert", $"tab-insert-{TabSizeTag}.png"),
                    ("Blog Account", $"tab-blogaccount-{TabSizeTag}.png"),
                    ("Debug", $"tab-debug-{TabSizeTag}.png"),
                })
                {
                    string path = UiReviewHarness.CaptureRibbonTabBand(window, label, fileName, outDir);
                    if (path != null) written.Add(path);
                    else skipped.Add(label);
                }

                // The Preview tab is only visible in preview mode; enable that mode and
                // rebuild the ribbon so it becomes selectable for the capture.
                ribbon.ActiveModes |= RibbonApplicationMode.Preview;
                ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());
                UiReviewHarness.PumpLayout(window);
                UiReviewHarness.PumpLayout(window);

                string preview = UiReviewHarness.CaptureRibbonTabBand(
                    window, "Preview", $"tab-preview-{TabSizeTag}.png", outDir);
                if (preview != null) written.Add(preview);
                else skipped.Add("Preview");

                // Picture Tools contextual tab: simulate the image-selected state by
                // activating the ImageTools group, then capture its Format tab band
                // with the size spinners reflecting a selected 640x360 picture.
                ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);
                ribbon.SetSpinnerValue(CommandId.FormatImageAdjustWidth, 640m);
                ribbon.SetSpinnerValue(CommandId.FormatImageAdjustHeight, 360m);
                UiReviewHarness.PumpLayout(window);
                UiReviewHarness.PumpLayout(window);

                string pictureTools = UiReviewHarness.CaptureRibbonTabBand(
                    window, "Format", $"tab-picturetools-{TabSizeTag}.png", outDir);
                if (pictureTools != null) written.Add(pictureTools);
                else skipped.Add("Picture Tools (Format)");

                ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.None);
            }
            finally
            {
                window.Close();
            }

            UiReviewHarness.WriteIndex(outDir);
            foreach (string f in written)
                TestContext.WriteLine($"wrote {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");
            foreach (string s in skipped)
                TestContext.WriteLine($"skipped tab: {s}");

            Assert.That(skipped, Is.Empty, "Tabs that could not be selected/captured: " + string.Join(", ", skipped));
            foreach (string f in written)
                Assert.That(new FileInfo(f).Length, Is.GreaterThan(5000), $"{f} looks blank");
        }

        [AvaloniaTest]
        public void Capture_Dialogs_AllShellDialogs_WritePngs()
        {
            string outDir = UiReviewHarness.ResolveOutputDirectory();

            var captures = new List<(string FileName, Func<Window> Factory)>
            {
                ("dialog-account.png", () => new AccountDialog(
                    SampleAccount("acct-1", "My WordPress Blog", "doug", "https://doug.wordpress.com"))),
                ("dialog-accountmanager.png", () => new AccountManagerDialog(BuildSampleAccountService())),
                ("dialog-category.png", () => new CategoryDialog(
                    new List<BlogPostCategory>
                    {
                        new BlogPostCategory("1", "macOS"),
                        new BlogPostCategory("2", "Avalonia"),
                        new BlogPostCategory("3", "Open Live Writer"),
                        new BlogPostCategory("4", "Release Notes"),
                    },
                    new[] { "macOS", "Open Live Writer" })),
                ("dialog-confirm.png", CreateConfirmDialog),
                ("dialog-draftpicker.png", () => new DraftPickerDialog(new List<DraftInfo>
                {
                    new DraftInfo("d-3", "Milestone 4 status update", DateTime.UtcNow.AddHours(-2)),
                    new DraftInfo("d-2", "Avalonia ribbon notes", DateTime.UtcNow.AddDays(-1)),
                    new DraftInfo("d-1", "", DateTime.UtcNow.AddDays(-3)),
                })),
                ("dialog-emoticon.png", () => new EmoticonDialog()),
                ("dialog-findreplace.png", () => new FindReplaceDialog(
                    _ => Task.CompletedTask, _ => Task.CompletedTask)),
                ("dialog-imageproperties.png", () => new ImagePropertiesDialog(new ImageFormatState
                {
                    Src = "https://openlivewriter.org/wp-content/uploads/screenshot.png",
                    NaturalWidth = 1600,
                    NaturalHeight = 900,
                    Width = 640,
                    Height = 360,
                    Alt = "Open Live Writer editor screenshot",
                    Title = "Editor screenshot",
                    Alignment = "left",
                    MarginPx = 8,
                    BorderWidthPx = 1,
                    BorderColor = "#999999"
                })),
                ("dialog-link.png", () => new LinkDialog("Open Live Writer")),
                ("dialog-map.png", () => new MapDialog()),
                ("dialog-message.png", () => new MessageDialog(
                    "Publish", "Your post \u201cMilestone 4 status update\u201d was published successfully.")),
                ("dialog-openfromblog.png", () => new OpenFromBlogDialog(
                    (pages, count) => Task.FromResult<IReadOnlyList<ServerPost>>(
                        new List<ServerPost>
                        {
                            new ServerPost
                            {
                                PostId = "412",
                                Title = "Milestone 4 status update",
                                Description = "<p>WebView WYSIWYG landed.</p>",
                                Status = "publish",
                                DateCreatedUtc = DateTime.UtcNow.AddHours(-5)
                            },
                            new ServerPost
                            {
                                PostId = "408",
                                Title = "Avalonia ribbon notes",
                                Description = "<p>Ribbon layout on macOS.</p>",
                                Status = "publish",
                                DateCreatedUtc = DateTime.UtcNow.AddDays(-2)
                            },
                            new ServerPost
                            {
                                PostId = "399",
                                Title = "",
                                Description = "<p>Scratch notes.</p>",
                                Status = "draft",
                                DateCreatedUtc = DateTime.UtcNow.AddDays(-4)
                            },
                        }),
                    supportsPages: true)),
                ("dialog-postproperties.png", () => new PostPropertiesDialog(
                    DateTime.UtcNow.AddDays(3))),
                ("dialog-selectblog.png", () => new SelectBlogDialog(
                    BuildSampleAccountService().ListAccounts(), "acct-1")),
                ("dialog-spelling.png", () => new SpellingDialog(
                    "<p>Open Live Writter is a blog authoring tool.</p>" +
                    "<p>This build runs on macOS with Avalonia.</p>",
                    BuildSampleSpellEngine())),
                ("dialog-table.png", () => new TableDialog()),
                ("dialog-tag.png", () => new TagDialog(new[] { "macOS", "Avalonia", "Open Live Writer" })),
                ("dialog-video.png", () => new VideoDialog()),
                ("dialog-webimage.png", () => new WebImageDialog(
                    "https://openlivewriter.org/wp-content/uploads/screenshot.png")),
                ("dialog-wordcount.png", () => new WordCountDialog(new WordCounter(
                    "<p>Open Live Writer is a blog authoring tool.</p>" +
                    "<p>This build runs on macOS with Avalonia.</p>"))),
            };

            var written = new List<string>();
            var failures = new List<string>();
            foreach (var (fileName, factory) in captures)
            {
                try
                {
                    string path = UiReviewHarness.CaptureDialog(factory(), fileName, outDir);
                    if (path != null) written.Add(path);
                    else failures.Add(fileName);
                }
                catch (Exception ex)
                {
                    failures.Add($"{fileName} ({ex.GetType().Name}: {ex.Message})");
                }
            }

            try
            {
                written.AddRange(CapturePreferencesTabs(outDir));
            }
            catch (Exception ex)
            {
                failures.Add($"dialog-preferences.png ({ex.GetType().Name}: {ex.Message})");
            }

            UiReviewHarness.WriteIndex(outDir);
            foreach (string f in written)
                TestContext.WriteLine($"wrote {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");

            Assert.That(failures, Is.Empty, "Dialogs that could not be captured: " + string.Join("; ", failures));
            foreach (string f in written)
                Assert.That(new FileInfo(f).Length, Is.GreaterThan(1000), $"{f} looks blank");
        }

        /// <summary>
        /// Captures the Preferences dialog once per tab (General as dialog-preferences.png,
        /// the rest as dialog-preferences-&lt;tab&gt;.png). The dialog's constructor is
        /// private (production exposes only the modal ShowAsync), so reflection builds it.
        /// </summary>
        private static List<string> CapturePreferencesTabs(string outDir)
        {
            var ctor = typeof(PreferencesDialog).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(AppPreferences), typeof(BlogAccountService), typeof(Func<AppPreferences, Task>) },
                modifiers: null);
            Assert.That(ctor, Is.Not.Null, "PreferencesDialog constructor not found");

            var dialog = (Window)ctor.Invoke(new object[]
            {
                AppPreferences.CreateDefault(),
                BuildSampleAccountService(),
                (Func<AppPreferences, Task>)(_ => Task.CompletedTask)
            });

            var written = new List<string>();
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            try
            {
                dialog.Show();
                UiReviewHarness.PumpLayout(dialog);
                UiReviewHarness.PumpLayout(dialog);

                var tabs = dialog.GetLogicalDescendants().OfType<TabControl>().FirstOrDefault();
                if (tabs == null || tabs.Items.Count == 0)
                {
                    string only = Path.Combine(outDir, "dialog-preferences.png");
                    if (UiReviewHarness.SaveWindowScreenshot(dialog, only))
                        written.Add(only);
                    return written;
                }

                for (int i = 0; i < tabs.Items.Count; i++)
                {
                    tabs.SelectedIndex = i;
                    UiReviewHarness.PumpLayout(dialog);
                    UiReviewHarness.PumpLayout(dialog);

                    string header = (tabs.Items[i] as TabItem)?.Header as string ?? $"tab{i}";
                    string slug = new string(header.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
                    string fileName = i == 0 ? "dialog-preferences.png" : $"dialog-preferences-{slug}.png";
                    string path = Path.Combine(outDir, fileName);
                    if (UiReviewHarness.SaveWindowScreenshot(dialog, path))
                        written.Add(path);
                }
            }
            finally
            {
                dialog.Close();
            }
            return written;
        }

        /// <summary>
        /// Builds the three-way Save/Discard/Cancel confirm prompt. The constructor is
        /// private (production exposes only the modal factory methods), so reflection
        /// builds it with the same button set the unsaved-changes path uses.
        /// </summary>
        private static Window CreateConfirmDialog()
        {
            var buttons = new[]
            {
                ("Save", ConfirmResult.Save, true, false),
                ("Don\u2019t Save", ConfirmResult.Discard, false, false),
                ("Cancel", ConfirmResult.Cancel, false, true)
            };

            var ctor = typeof(ConfirmDialog).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string), typeof(string), buttons.GetType() },
                modifiers: null);
            Assert.That(ctor, Is.Not.Null, "ConfirmDialog constructor not found");

            return (Window)ctor.Invoke(new object[]
            {
                "Unsaved Changes",
                "You have unsaved changes to \u201cMilestone 4 status update\u201d. Do you want to save them?",
                buttons
            });
        }

        private static BlogAccount SampleAccount(string id, string name, string username, string homepage) =>
            new BlogAccount
            {
                Id = id,
                DisplayName = name,
                Username = username,
                HomepageUrl = homepage,
                ApiEndpointUrl = homepage.TrimEnd('/') + "/xmlrpc.php",
                BlogId = "1"
            };

        /// <summary>
        /// Small in-memory dictionary for the sample Spelling dialog: every word in the
        /// sample post except the deliberate misspelling ("Writter"), with suggestions.
        /// </summary>
        private static ISpellCheckEngine BuildSampleSpellEngine()
        {
            var engine = new InMemorySpellCheckEngine(new[]
            {
                "Open", "Live", "Writer", "is", "a", "blog", "authoring", "tool",
                "This", "build", "runs", "on", "macOS", "with", "Avalonia"
            });
            engine.Suggestions["Writter"] = new[] { "Writer", "Written", "Writes" };
            return engine;
        }

        private static BlogAccountService BuildSampleAccountService()        {
            var service = new BlogAccountService(new InMemoryAccountStore(), new InMemoryCredentialStore());
            service.SaveAccount(SampleAccount("acct-1", "My WordPress Blog", "doug", "https://doug.wordpress.com"), "sample-password");
            service.SaveAccount(SampleAccount("acct-2", "Test MetaWeblog", "tester", "https://test.example.com"), "sample-password");
            service.SetCurrentAccount("acct-1");
            return service;
        }

        /// <summary>Minimal in-memory <see cref="IAccountStore"/> for sample-data dialogs.</summary>
        private sealed class InMemoryAccountStore : IAccountStore
        {
            private readonly Dictionary<string, BlogAccount> _accounts = new Dictionary<string, BlogAccount>();

            public string CurrentAccountId { get; set; }

            public BlogAccount Save(BlogAccount account)
            {
                if (string.IsNullOrEmpty(account.Id))
                    account.Id = Guid.NewGuid().ToString("N");
                _accounts[account.Id] = account;
                return account;
            }

            public BlogAccount Load(string id) =>
                id != null && _accounts.TryGetValue(id, out BlogAccount account) ? account : null;

            public IReadOnlyList<BlogAccount> List() =>
                _accounts.Values.OrderBy(a => a.DisplayLabel, StringComparer.Ordinal).ToList();

            public void Delete(string id)
            {
                if (id != null)
                    _accounts.Remove(id);
            }

            public bool Exists(string id) => id != null && _accounts.ContainsKey(id);
        }
    }
}
