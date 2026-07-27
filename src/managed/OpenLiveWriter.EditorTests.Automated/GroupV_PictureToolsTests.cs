// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group V — Picture Tools (P1-2). Covers the pure C# side of picture editing:
    /// the <c>getState()</c> image payload parsing, the applyImageAttrs JSON
    /// builder, aspect-ratio math and presets, the properties-dialog link
    /// mapping, and the ribbon wiring (width/height spinners, size-preset
    /// dropdown flyout). JS-side behavior is covered by the [Explicit] WebView
    /// fixture below.
    /// </summary>
    [TestFixture]
    [Category("GroupV")]
    public class GroupV_PictureToolsTests
    {
        // ---- getState() image payload parsing ----

        [Test]
        public void ParseState_ReadsImagePayload()
        {
            string json = "{\"blockTag\":\"p\",\"selectedElementType\":\"image\",\"image\":{" +
                "\"src\":\"https://example.com/pic.png\",\"naturalWidth\":1600,\"naturalHeight\":900," +
                "\"width\":320,\"height\":180,\"alt\":\"A cat\",\"title\":\"Cat\"," +
                "\"alignment\":\"left\",\"margin\":8,\"rotation\":90," +
                "\"borderWidth\":2,\"borderColor\":\"rgb(255, 0, 0)\",\"link\":\"https://example.com\"}}";

            FormatState state = WebViewEditor.ParseFormatStateJson(json);
            ImageFormatState img = state.Image;

            Assert.That(img, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(img.Src, Is.EqualTo("https://example.com/pic.png"));
                Assert.That(img.NaturalWidth, Is.EqualTo(1600));
                Assert.That(img.NaturalHeight, Is.EqualTo(900));
                Assert.That(img.Width, Is.EqualTo(320));
                Assert.That(img.Height, Is.EqualTo(180));
                Assert.That(img.Alt, Is.EqualTo("A cat"));
                Assert.That(img.Title, Is.EqualTo("Cat"));
                Assert.That(img.Alignment, Is.EqualTo("left"));
                Assert.That(img.MarginPx, Is.EqualTo(8));
                Assert.That(img.RotationDeg, Is.EqualTo(90));
                Assert.That(img.BorderWidthPx, Is.EqualTo(2));
                Assert.That(img.BorderColor, Is.EqualTo("#FF0000"), "rgb() colors normalize to hex");
                Assert.That(img.LinkHref, Is.EqualTo("https://example.com"));
                Assert.That(img.HasRemoteSource, Is.True);
                Assert.That(ContextualTabResolver.Resolve(state),
                    Is.EqualTo(RibbonContextualTabGroup.ImageTools));
            });
        }

        [Test]
        public void ParseState_NoImageSelected_ImageIsNull()
        {
            FormatState state = WebViewEditor.ParseFormatStateJson("{\"blockTag\":\"p\",\"image\":null}");
            Assert.That(state.Image, Is.Null);

            FormatState plain = WebViewEditor.ParseFormatStateJson("{\"blockTag\":\"p\"}");
            Assert.That(plain.Image, Is.Null);
        }

        [Test]
        public void ParseState_NormalizesImageFields()
        {
            string json = "{\"image\":{\"alignment\":\"RIGHT\",\"borderWidth\":null," +
                "\"borderColor\":null,\"margin\":null,\"link\":\"\"}}";
            ImageFormatState img = WebViewEditor.ParseFormatStateJson(json).Image;

            Assert.Multiple(() =>
            {
                Assert.That(img.Alignment, Is.EqualTo("right"));
                Assert.That(img.BorderWidthPx, Is.Null);
                Assert.That(img.BorderColor, Is.Null);
                Assert.That(img.MarginPx, Is.Null);
                Assert.That(img.LinkHref, Is.Null, "empty link reports as not-linked");
            });
        }

        [Test]
        public void ParseState_JunkAlignment_FallsBackToInline()
        {
            string json = "{\"image\":{\"alignment\":\"sideways\"}}";
            Assert.That(WebViewEditor.ParseFormatStateJson(json).Image.Alignment, Is.EqualTo("inline"));
        }

        [Test]
        public void HasRemoteSource_DataUri_IsFalse()
        {
            string json = "{\"image\":{\"src\":\"data:image/png;base64,AAAA\"}}";
            Assert.That(WebViewEditor.ParseFormatStateJson(json).Image.HasRemoteSource, Is.False);
        }

        // ---- applyImageAttrs payload builder ----

        [Test]
        public void BuildAttrsJson_EmitsOnlySetMembers()
        {
            string json = ImageEditBuilder.BuildAttrsJson(new ImageAttributes { Width = 320, Alt = "" });
            using var doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.GetProperty("width").GetInt32(), Is.EqualTo(320));
                Assert.That(root.GetProperty("alt").GetString(), Is.EqualTo(""),
                    "an empty alt must still be emitted so it clears the attribute");
                Assert.That(root.TryGetProperty("height", out _), Is.False);
                Assert.That(root.TryGetProperty("title", out _), Is.False);
                Assert.That(root.TryGetProperty("alignment", out _), Is.False);
                Assert.That(root.TryGetProperty("margin", out _), Is.False);
                Assert.That(root.TryGetProperty("rotation", out _), Is.False);
                Assert.That(root.TryGetProperty("borderWidth", out _), Is.False);
            });
        }

        [Test]
        public void BuildAttrsJson_ClearSize_EmitsNullWidthAndHeight()
        {
            string json = ImageEditBuilder.BuildAttrsJson(
                new ImageAttributes { Width = 320, ClearSize = true });
            using var doc = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(doc.RootElement.GetProperty("width").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(doc.RootElement.GetProperty("height").ValueKind, Is.EqualTo(JsonValueKind.Null));
            });
        }

        [Test]
        public void BuildAttrsJson_Border_EmitsWidthAndColor()
        {
            string json = ImageEditBuilder.BuildAttrsJson(
                new ImageAttributes { BorderWidthPx = 2, BorderColor = "#FF0000" });
            using var doc = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(doc.RootElement.GetProperty("borderWidth").GetInt32(), Is.EqualTo(2));
                Assert.That(doc.RootElement.GetProperty("borderColor").GetString(), Is.EqualTo("#FF0000"));
            });
        }

        [Test]
        public void BuildAttrsJson_Null_IsEmptyObject()
        {
            Assert.That(ImageEditBuilder.BuildAttrsJson(null), Is.EqualTo("{}"));
        }

        // ---- Aspect-ratio math and presets ----

        [TestCase(1600, 900, 320, 180)]
        [TestCase(1600, 900, 640, 360)]
        [TestCase(100, 100, 33, 33)]
        [TestCase(3, 2, 1000, 667)] // rounds to nearest pixel
        public void HeightForWidth_PreservesAspect(int naturalW, int naturalH, int width, int expected)
        {
            Assert.That(ImageEditBuilder.HeightForWidth(naturalW, naturalH, width), Is.EqualTo(expected));
        }

        [TestCase(0, 900, 320)]
        [TestCase(1600, 0, 320)]
        [TestCase(1600, 900, 0)]
        public void HeightForWidth_UnknownDims_ReturnsNull(int naturalW, int naturalH, int width)
        {
            Assert.That(ImageEditBuilder.HeightForWidth(naturalW, naturalH, width), Is.Null);
        }

        [Test]
        public void WidthForHeight_PreservesAspect()
        {
            Assert.That(ImageEditBuilder.WidthForHeight(1600, 900, 180), Is.EqualTo(320));
            Assert.That(ImageEditBuilder.WidthForHeight(0, 900, 180), Is.Null);
        }

        [Test]
        public void Presets_AreSensibleProgression()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ImageEditBuilder.SmallWidth, Is.LessThan(ImageEditBuilder.MediumWidth));
                Assert.That(ImageEditBuilder.MediumWidth, Is.LessThan(ImageEditBuilder.LargeWidth));
                Assert.That(ImageEditBuilder.SmallWidth, Is.GreaterThanOrEqualTo(100));
                Assert.That(ImageEditBuilder.LargeWidth, Is.LessThanOrEqualTo(800));
            });
        }

        [TestCase("left", "left")]
        [TestCase(" RIGHT ", "right")]
        [TestCase("center", "center")]
        [TestCase("", "inline")]
        [TestCase(null, "inline")]
        [TestCase("block", "inline")]
        public void NormalizeAlignment_MapsTokens(string input, string expected)
        {
            Assert.That(ImageEditBuilder.NormalizeAlignment(input), Is.EqualTo(expected));
        }

        // ---- Picture properties dialog link mapping ----

        [Test]
        public void InitialLinkChoice_MapsStateToChoice()
        {
            Assert.That(ImagePropertiesDialog.InitialLinkChoice(null), Is.EqualTo(ImageLinkChoice.None));
            Assert.That(ImagePropertiesDialog.InitialLinkChoice(new ImageFormatState()),
                Is.EqualTo(ImageLinkChoice.None));
            Assert.That(ImagePropertiesDialog.InitialLinkChoice(new ImageFormatState
            {
                Src = "https://example.com/p.png", LinkHref = "https://example.com/p.png"
            }), Is.EqualTo(ImageLinkChoice.Source));
            Assert.That(ImagePropertiesDialog.InitialLinkChoice(new ImageFormatState
            {
                Src = "https://example.com/p.png", LinkHref = "https://other.example.com"
            }), Is.EqualTo(ImageLinkChoice.Url));
        }

        [Test]
        public void ResolveLinkUrl_MapsChoiceToHref()
        {
            var remote = new ImageFormatState { Src = "https://example.com/p.png" };
            var embedded = new ImageFormatState { Src = "data:image/png;base64,AAAA" };

            Assert.Multiple(() =>
            {
                Assert.That(ImagePropertiesDialog.ResolveLinkUrl(
                    new ImagePropertiesDialogResult { LinkChoice = ImageLinkChoice.None }, remote), Is.Null);
                Assert.That(ImagePropertiesDialog.ResolveLinkUrl(
                    new ImagePropertiesDialogResult { LinkChoice = ImageLinkChoice.Source }, remote),
                    Is.EqualTo("https://example.com/p.png"));
                Assert.That(ImagePropertiesDialog.ResolveLinkUrl(
                    new ImagePropertiesDialogResult { LinkChoice = ImageLinkChoice.Source }, embedded),
                    Is.Null, "embedded (data-URI) pictures have no source URL to link to");
                Assert.That(ImagePropertiesDialog.ResolveLinkUrl(
                    new ImagePropertiesDialogResult
                    {
                        LinkChoice = ImageLinkChoice.Url, LinkUrl = "  https://example.com/x  "
                    }, embedded), Is.EqualTo("https://example.com/x"));
                Assert.That(ImagePropertiesDialog.ResolveLinkUrl(
                    new ImagePropertiesDialogResult { LinkChoice = ImageLinkChoice.Url, LinkUrl = " " },
                    embedded), Is.Null);
            });
        }

        // ---- Picture properties dialog size mapping ----

        [Test]
        public void BuildSizeAttributes_BothBlank_ResetsToNatural()
        {
            ImageAttributes attrs = ImageEditBuilder.BuildSizeAttributes(null, null, 1600, 900);
            Assert.Multiple(() =>
            {
                Assert.That(attrs.ClearSize, Is.True);
                Assert.That(attrs.Width, Is.Null);
                Assert.That(attrs.Height, Is.Null);
            });
        }

        [Test]
        public void BuildSizeAttributes_OneBlank_ComputesFromNaturalAspect()
        {
            ImageAttributes byWidth = ImageEditBuilder.BuildSizeAttributes(320, null, 1600, 900);
            Assert.Multiple(() =>
            {
                Assert.That(byWidth.Width, Is.EqualTo(320));
                Assert.That(byWidth.Height, Is.EqualTo(180));
            });

            ImageAttributes byHeight = ImageEditBuilder.BuildSizeAttributes(null, 180, 1600, 900);
            Assert.Multiple(() =>
            {
                Assert.That(byHeight.Width, Is.EqualTo(320));
                Assert.That(byHeight.Height, Is.EqualTo(180));
            });
        }

        [Test]
        public void BuildSizeAttributes_OneBlank_UnknownNatural_LeavesOtherUnset()
        {
            ImageAttributes attrs = ImageEditBuilder.BuildSizeAttributes(320, null, 0, 0);
            Assert.Multiple(() =>
            {
                Assert.That(attrs.Width, Is.EqualTo(320));
                Assert.That(attrs.Height, Is.Null, "no natural dims — browser scales proportionally");
                Assert.That(attrs.ClearSize, Is.False);
            });
        }

        [Test]
        public void BuildSizeAttributes_BothSet_PassesThrough()
        {
            ImageAttributes attrs = ImageEditBuilder.BuildSizeAttributes(400, 200, 1600, 900);
            Assert.Multiple(() =>
            {
                Assert.That(attrs.Width, Is.EqualTo(400));
                Assert.That(attrs.Height, Is.EqualTo(200));
                Assert.That(attrs.ClearSize, Is.False);
            });
        }

        [AvaloniaTest]
        public void ImagePropertiesDialog_PrefillsSizeAndLocksAspect()
        {
            var dialog = new ImagePropertiesDialog(new ImageFormatState
            {
                NaturalWidth = 1600,
                NaturalHeight = 900,
                Width = 640,
                Height = 360
            });

            var width = dialog.GetLogicalDescendants().OfType<NumericUpDown>()
                .FirstOrDefault(n => n.Name == "ImageWidthSpinner");
            var height = dialog.GetLogicalDescendants().OfType<NumericUpDown>()
                .FirstOrDefault(n => n.Name == "ImageHeightSpinner");
            var lockCheck = dialog.GetLogicalDescendants().OfType<CheckBox>()
                .FirstOrDefault(c => c.Name == "ImageLockAspectCheck");

            Assert.Multiple(() =>
            {
                Assert.That(width, Is.Not.Null, "dialog renders a width field");
                Assert.That(height, Is.Not.Null, "dialog renders a height field");
                Assert.That(lockCheck, Is.Not.Null, "dialog renders the aspect-lock checkbox");
            });
            Assert.Multiple(() =>
            {
                Assert.That(width.Value, Is.EqualTo(640m), "width prefills the display width");
                Assert.That(height.Value, Is.EqualTo(360m), "height prefills the display height");
                Assert.That(lockCheck.IsChecked, Is.True, "aspect lock defaults on");
            });

            // Editing one dimension recomputes the other from the natural dims.
            width.Value = 320m;
            Assert.That(height.Value, Is.EqualTo(180m));
            height.Value = 90m;
            Assert.That(width.Value, Is.EqualTo(160m));

            // Blanking a dimension blanks the other (both blank = natural size).
            width.Value = null;
            Assert.That(height.Value, Is.Null);

            // With the lock off, editing one dimension leaves the other alone.
            width.Value = 320m;
            height.Value = 180m;
            lockCheck.IsChecked = false;
            width.Value = 500m;
            Assert.That(height.Value, Is.EqualTo(180m));
        }

        [AvaloniaTest]
        public void ImagePropertiesDialog_UnknownDisplaySize_PrefillsBlank()
        {
            var dialog = new ImagePropertiesDialog(new ImageFormatState
            {
                NaturalWidth = 1600,
                NaturalHeight = 900
            });

            var width = dialog.GetLogicalDescendants().OfType<NumericUpDown>()
                .FirstOrDefault(n => n.Name == "ImageWidthSpinner");
            var height = dialog.GetLogicalDescendants().OfType<NumericUpDown>()
                .FirstOrDefault(n => n.Name == "ImageHeightSpinner");

            Assert.Multiple(() =>
            {
                Assert.That(width.Value, Is.Null, "no display width prefills blank (natural)");
                Assert.That(height.Value, Is.Null);
            });
        }

        // ---- Rotate/effects route through the shell (pixel baking), not the editor ----

        [AvaloniaTest]
        public async Task HandleCommand_Rotate_NoLongerEditorHandled()
        {
            // Baked rotation needs the shell (image bytes, proxy-aware fetch), so
            // WebViewEditor.HandleCommandAsync no longer claims the rotate commands;
            // MainWindow.TryHandlePictureCommandAsync handles them instead.
            WebViewEditor.UseLayoutPlaceholder = true;
            try
            {
                var editor = new WebViewEditor();
                Assert.That(await editor.HandleCommandAsync(CommandId.ImageRotateCW), Is.False);
                Assert.That(await editor.HandleCommandAsync(CommandId.ImageRotateCCW), Is.False);
            }
            finally
            {
                WebViewEditor.UseLayoutPlaceholder = false;
            }
        }

        // ---- Ribbon wiring (headless UI) ----

        [AvaloniaTest]
        public void PictureTab_WidthHeightSpinners_RaiseAndReflectValues()
        {
            var ribbon = new AvaloniaRibbonControl();
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);

            var events = new List<RibbonSpinnerValueEventArgs>();
            ribbon.SpinnerValueChanged += (s, e) => events.Add(e);

            // Identify the width spinner by reflecting a sentinel value into it;
            // programmatic reflection must not raise the change event.
            ribbon.SetSpinnerValue(CommandId.FormatImageAdjustWidth, 321m);
            Assert.That(events, Is.Empty);

            NumericUpDown widthSpinner = ribbon.GetLogicalDescendants()
                .OfType<NumericUpDown>()
                .FirstOrDefault(n => n.Value == 321m);
            Assert.That(widthSpinner, Is.Not.Null,
                "the Picture Tools tab should render a width spinner that reflects state");

            widthSpinner.Value = 400m;
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].CommandId, Is.EqualTo(CommandId.FormatImageAdjustWidth));
            Assert.That(events[0].Value, Is.EqualTo(400m));
        }

        [AvaloniaTest]
        public void PictureTab_CustomSizeDropdown_HasLivePresetsAndDeadDefaults()
        {
            var ribbon = new AvaloniaRibbonControl { CommandFilter = HandledCommands.IsHandled };
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);

            RibbonButtonControl button = ribbon.GetLogicalDescendants()
                .OfType<RibbonButtonControl>()
                .FirstOrDefault(b => b.CommandId == CommandId.CustomSizeGallery);
            Assert.That(button, Is.Not.Null);
            Assert.That(button.IsEnabled, Is.True, "the size dropdown parent stays enabled");
            Assert.That(button.Flyout, Is.Not.Null, "MenuItems now produce a flyout");

            var menuItems = ((MenuFlyout)button.Flyout).Items.OfType<MenuItem>().ToList();
            Assert.That(menuItems.Count, Is.EqualTo(5),
                "Small/Medium/Large/Original presets plus Set defaults");
            Assert.That(menuItems.Take(4), Has.All.Property(nameof(MenuItem.IsEnabled)).True);
            Assert.That(menuItems[4].IsEnabled, Is.False,
                "SetCustomSizeDefaults has no handler and stays disabled");

            var fired = new List<CommandId>();
            ribbon.CommandExecuted += (s, id) => fired.Add(id);
            menuItems[1].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.That(fired, Is.EqualTo(new[] { CommandId.CustomSizeMedium }));
        }

        [AvaloniaTest]
        public void PictureTab_LinkToDropdown_DispatchesItems()
        {
            var ribbon = new AvaloniaRibbonControl { CommandFilter = HandledCommands.IsHandled };
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);

            RibbonButtonControl button = ribbon.GetLogicalDescendants()
                .OfType<RibbonButtonControl>()
                .FirstOrDefault(b => b.CommandId == CommandId.FormatImageSelectLink);
            Assert.That(button?.Flyout, Is.Not.Null);

            var menuItems = ((MenuFlyout)button.Flyout).Items.OfType<MenuItem>().ToList();
            Assert.That(menuItems.Count, Is.EqualTo(3), "Source picture / Web address / No link");

            var fired = new List<CommandId>();
            ribbon.CommandExecuted += (s, id) => fired.Add(id);
            menuItems[2].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.That(fired, Is.EqualTo(new[] { CommandId.ImageLinkToNone }));
        }
    }

    /// <summary>
    /// Group V (WebView) — live JS-side coverage for the image bridge: selection
    /// awareness in <c>getState()</c>, applyImageAttrs, the baked-replacement
    /// bridge (<c>replaceSelectedImageSrc</c>), and setImageLink.
    /// Requires a live WKWebView backend.
    /// </summary>
    [TestFixture]
    [Category("GroupV")]
    [Category(WebViewCategories.WebView)]
    [Explicit("Requires a live WKWebView backend — run on a real macOS session")]
    public class GroupV_PictureToolsWebViewTests
    {
        // 1x1 transparent PNG (natural 1x1 keeps aspect math predictable).
        private const string TinyPng =
            "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk" +
            "+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

        private static string SelectFirstImageScript =>
            "(function(){var img=document.querySelector('img');var r=document.createRange();" +
            "r.selectNode(img);var s=window.getSelection();s.removeAllRanges();s.addRange(r);" +
            "OLWBridge.saveSelection();return OLWBridge.getState();})()";

        [Test]
        public async Task GetState_ReportsImagePayload_WhenImageSelected()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync($"<p>x</p><img src=\"{TinyPng}\" alt=\"dot\" /><p>y</p>");

            string state = await harness.Editor.WebView.InvokeScript(SelectFirstImageScript);
            Assert.That(state, Does.Contain("\"selectedElementType\":\"image\""));
            Assert.That(state, Does.Contain("\"alt\":\"dot\""));
            Assert.That(state, Does.Contain("\"naturalWidth\":1"));

            // Moving the caret into a paragraph deselects the image.
            await harness.SetContentAsync("<p>plain</p>");
            string plain = await harness.GetStateAsync();
            Assert.That(plain, Does.Not.Contain("\"selectedElementType\":\"image\""));
        }

        [Test]
        public async Task ApplyImageAttrs_SetsSizeBorderAndAlt()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync($"<img src=\"{TinyPng}\" />");
            await harness.Editor.WebView.InvokeScript(SelectFirstImageScript);

            await harness.Editor.ApplyImageAttrsAsync(new ImageAttributes
            {
                Width = 40,
                Height = 40,
                Alt = "a dot",
                BorderWidthPx = 2,
                BorderColor = "#FF0000",
                Alignment = "right",
                MarginPx = 8
            });

            string json = await harness.Editor.WebView.InvokeScript("OLWBridge.getSelectedImage()");
            using var doc = JsonDocument.Parse(json);
            JsonElement img = doc.RootElement;
            Assert.Multiple(() =>
            {
                Assert.That(img.GetProperty("width").GetInt32(), Is.EqualTo(40));
                Assert.That(img.GetProperty("alt").GetString(), Is.EqualTo("a dot"));
                Assert.That(img.GetProperty("borderWidth").GetInt32(), Is.EqualTo(2));
                Assert.That(img.GetProperty("alignment").GetString(), Is.EqualTo("right"));
                Assert.That(img.GetProperty("margin").GetInt32(), Is.EqualTo(8));
            });
        }

        [Test]
        public async Task ReplaceSelectedImageSrc_SwapsSizeAndClearsRotation()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync($"<img src=\"{TinyPng}\" />");
            await harness.Editor.WebView.InvokeScript(SelectFirstImageScript);

            // Explicit 40x20 display size + a legacy CSS rotation, then a baked
            // 90-degree rotation replacement: size swaps and the transform clears.
            await harness.Editor.ApplyImageAttrsAsync(new ImageAttributes
            {
                Width = 40, Height = 20, RotationDeg = 90
            });
            await harness.Editor.ReplaceSelectedImageSrcAsync(
                TinyPng, OpenLiveWriter.App.Avalonia.ImageEditing.BakedImageSizeMode.Swap);

            string json = await harness.Editor.WebView.InvokeScript("OLWBridge.getSelectedImage()");
            using (var doc = JsonDocument.Parse(json))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(doc.RootElement.GetProperty("width").GetInt32(), Is.EqualTo(20),
                        "explicit display size swaps after a baked 90-degree rotation");
                    Assert.That(doc.RootElement.GetProperty("height").GetInt32(), Is.EqualTo(40));
                    Assert.That(doc.RootElement.GetProperty("rotation").GetInt32(), Is.EqualTo(0),
                        "CSS rotation is cleared once the rotation is baked into the pixels");
                });
            }

            // 'set' mode applies explicit dimensions (crop/resize target size).
            await harness.Editor.ReplaceSelectedImageSrcAsync(
                TinyPng, OpenLiveWriter.App.Avalonia.ImageEditing.BakedImageSizeMode.Set, 10, 5);
            json = await harness.Editor.WebView.InvokeScript("OLWBridge.getSelectedImage()");
            using (var doc = JsonDocument.Parse(json))
            {
                Assert.That(doc.RootElement.GetProperty("width").GetInt32(), Is.EqualTo(10));
                Assert.That(doc.RootElement.GetProperty("height").GetInt32(), Is.EqualTo(5));
            }
        }

        [Test]
        public async Task SetImageLink_WrapsAndUnwraps()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync($"<img src=\"{TinyPng}\" />");
            await harness.Editor.WebView.InvokeScript(SelectFirstImageScript);

            await harness.Editor.SetImageLinkAsync("https://example.com");
            string json = await harness.Editor.WebView.InvokeScript("OLWBridge.getSelectedImage()");
            using (var doc = JsonDocument.Parse(json))
                Assert.That(doc.RootElement.GetProperty("link").GetString(),
                    Is.EqualTo("https://example.com"));

            await harness.Editor.SetImageLinkAsync(null);
            json = await harness.Editor.WebView.InvokeScript("OLWBridge.getSelectedImage()");
            using (var doc = JsonDocument.Parse(json))
                Assert.That(doc.RootElement.GetProperty("link").GetString(), Is.EqualTo(""));
        }

        [Test]
        public async Task ResizeHandles_AppearOnSelection_StayOutOfContent_AndDragResizes()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync($"<p>x</p><img src=\"{TinyPng}\" /><p>y</p>");

            // No handles while no image is selected.
            string hidden = await harness.Editor.WebView.InvokeScript(
                "(function(){var l=document.querySelector('[data-olw-ui=resize-handles]');" +
                "return l ? l.style.display : 'absent';})()");
            Assert.That(hidden, Does.Not.Contain("block"));

            // Selecting the image as a unit shows the 4 corner handles.
            await harness.Editor.WebView.InvokeScript(SelectFirstImageScript);
            string shown = await harness.Editor.WebView.InvokeScript(
                "(function(){var l=document.querySelector('[data-olw-ui=resize-handles]');" +
                "return l ? l.style.display + '|' + l.childNodes.length + '|' + (l.parentElement === document.documentElement) : 'absent';})()");
            Assert.Multiple(() =>
            {
                Assert.That(shown, Does.Contain("block"));
                Assert.That(shown, Does.Contain("|4|"), "one handle per corner");
                Assert.That(shown, Does.Contain("true"), "the layer lives outside <body>");
            });

            // The handles never leak into the document payload.
            string content = await harness.GetContentAsync();
            Assert.That(content, Does.Not.Contain("data-olw-ui"));

            // Corner-drag: 40x20 display size, drag se corner to 80px wide —
            // aspect (2:1) is preserved and width/height persist as attributes
            // plus matching inline styles.
            await harness.Editor.ApplyImageAttrsAsync(new ImageAttributes { Width = 40, Height = 20 });
            string dragged = await harness.Editor.WebView.InvokeScript(
                "(function(){" +
                "var img=document.querySelector('img');" +
                "var rect=img.getBoundingClientRect();" +
                "var h=document.querySelector('[data-olw-corner=se]');" +
                "function ev(t,x,y){return new PointerEvent(t,{clientX:x,clientY:y,bubbles:true,pointerId:1});}" +
                "h.dispatchEvent(ev('pointerdown',rect.right,rect.bottom));" +
                "h.dispatchEvent(ev('pointermove',rect.left+80,rect.top+40));" +
                "h.dispatchEvent(ev('pointerup',rect.left+80,rect.top+40));" +
                "return img.getAttribute('width')+'x'+img.getAttribute('height')+'|'+" +
                "img.style.width+'|'+img.style.height;})()");
            Assert.Multiple(() =>
            {
                Assert.That(dragged, Does.Contain("80x40"), "aspect ratio preserved from display dims");
                Assert.That(dragged, Does.Contain("80px"));
                Assert.That(dragged, Does.Contain("40px"));
            });

            // Deselecting hides the handles again.
            await harness.Editor.WebView.InvokeScript(
                "(function(){var s=window.getSelection();s.removeAllRanges();" +
                "var r=document.createRange();var p=document.querySelector('p');" +
                "r.setStart(p.firstChild,0);r.collapse(true);s.addRange(r);" +
                "OLWBridge.updateResizeHandles();return true;})()");
            string after = await harness.Editor.WebView.InvokeScript(
                "(function(){var l=document.querySelector('[data-olw-ui=resize-handles]');" +
                "return l ? l.style.display : 'absent';})()");
            Assert.That(after, Does.Contain("none"));
        }
    }
}
