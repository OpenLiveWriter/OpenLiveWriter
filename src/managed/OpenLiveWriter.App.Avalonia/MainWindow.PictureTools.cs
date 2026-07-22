// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Picture Tools (contextual tab) behavior for the shell: size presets and
    /// width/height spinners with aspect-ratio lock, rotate is handled directly by
    /// <see cref="WebViewEditor.HandleCommandAsync"/>, the Picture properties
    /// dialog (alt text, Link To, alignment, margins, border), and the border
    /// toggle. All operations apply to the image currently selected in the editor,
    /// whose last-reported state is cached in <see cref="_lastImageState"/>.
    /// </summary>
    public partial class MainWindow
    {
        // Last image payload reported by getState() (null when no image is selected).
        private ImageFormatState _lastImageState;

        // Picture Tools aspect-ratio lock toggle (on by default, like Windows).
        private bool _aspectLocked = true;

        private async Task<bool> TryHandlePictureCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.FormatImageAltText:
                case CommandId.FormatImageLinkOptions:
                case CommandId.ImageLinkToUrl:
                    await ShowImagePropertiesAsync();
                    return true;
                case CommandId.ImageLinkToSource:
                    await LinkImageToSourceAsync();
                    return true;
                case CommandId.ImageLinkToNone:
                    await SetImageLinkAsync(null, "Removed picture link.");
                    return true;
                case CommandId.CustomSizeSmall:
                    await ApplyImagePresetAsync(ImageEditBuilder.SmallWidth);
                    return true;
                case CommandId.CustomSizeMedium:
                    await ApplyImagePresetAsync(ImageEditBuilder.MediumWidth);
                    return true;
                case CommandId.CustomSizeLarge:
                    await ApplyImagePresetAsync(ImageEditBuilder.LargeWidth);
                    return true;
                case CommandId.CustomSizeOriginal:
                    await ApplyImageAttrsSafeAsync(
                        new ImageAttributes { ClearSize = true }, "Original size.");
                    return true;
                case CommandId.FormatImageLockAspectRatio:
                    _aspectLocked = !_aspectLocked;
                    _ribbon.SetToggleState(CommandId.FormatImageLockAspectRatio, _aspectLocked);
                    UpdateStatus(_aspectLocked ? "Aspect ratio locked." : "Aspect ratio unlocked.");
                    return true;
                case CommandId.ImageBorderGallery:
                    await ToggleImageBorderAsync();
                    return true;
                default:
                    return false;
            }
        }

        // Routes Picture Tools spinner changes (width/height) to the selected image.
        private async Task OnImageSpinnerValueChangedAsync(RibbonSpinnerValueEventArgs args)
        {
            var editor = GetEditor();
            var img = _lastImageState;
            if (editor == null || img == null || args.Value == null || args.Value < 1)
                return;

            int value = (int)args.Value;
            switch (args.CommandId)
            {
                case CommandId.FormatImageAdjustWidth:
                    int? height = _aspectLocked
                        ? ImageEditBuilder.HeightForWidth(img.NaturalWidth, img.NaturalHeight, value)
                        : null;
                    await editor.ApplyImageAttrsAsync(new ImageAttributes { Width = value, Height = height });
                    if (height.HasValue)
                        _ribbon.SetSpinnerValue(CommandId.FormatImageAdjustHeight, height.Value);
                    break;
                case CommandId.FormatImageAdjustHeight:
                    int? width = _aspectLocked
                        ? ImageEditBuilder.WidthForHeight(img.NaturalWidth, img.NaturalHeight, value)
                        : null;
                    await editor.ApplyImageAttrsAsync(new ImageAttributes { Width = width, Height = value });
                    if (width.HasValue)
                        _ribbon.SetSpinnerValue(CommandId.FormatImageAdjustWidth, width.Value);
                    break;
            }
        }

        // Small / Medium / Large presets: fixed width, height from the aspect ratio.
        private async Task ApplyImagePresetAsync(int width)
        {
            var img = _lastImageState;
            if (img == null)
            {
                UpdateStatus("Select a picture first.");
                return;
            }

            int? height = ImageEditBuilder.HeightForWidth(img.NaturalWidth, img.NaturalHeight, width);
            await ApplyImageAttrsSafeAsync(
                new ImageAttributes { Width = width, Height = height }, $"Size: {width}px wide.");
        }

        private async Task ApplyImageAttrsSafeAsync(ImageAttributes attrs, string statusMessage)
        {
            var editor = GetEditor();
            if (editor == null || _lastImageState == null)
            {
                UpdateStatus("Select a picture first.");
                return;
            }

            await editor.ApplyImageAttrsAsync(attrs);
            UpdateStatus(statusMessage);
        }

        // Border toggle: apply the last-used (or default) solid border, or remove it.
        private async Task ToggleImageBorderAsync()
        {
            var img = _lastImageState;
            if (img == null)
            {
                UpdateStatus("Select a picture first.");
                return;
            }

            if (img.BorderWidthPx.HasValue)
            {
                await ApplyImageAttrsSafeAsync(new ImageAttributes { BorderWidthPx = 0 },
                    "Removed picture border.");
            }
            else
            {
                await ApplyImageAttrsSafeAsync(new ImageAttributes
                {
                    BorderWidthPx = 1,
                    BorderColor = img.BorderColor ?? "#999999"
                }, "Applied picture border.");
            }
        }

        // Link To → Source picture: wrap the image in an anchor to its own URL.
        // Only meaningful for web pictures — embedded (data-URI) pictures have no
        // source URL (on Windows this links to the original file).
        private async Task LinkImageToSourceAsync()
        {
            var img = _lastImageState;
            if (img == null)
            {
                UpdateStatus("Select a picture first.");
                return;
            }

            if (!img.HasRemoteSource)
            {
                UpdateStatus("Embedded pictures have no source URL to link to.");
                return;
            }

            await SetImageLinkAsync(img.Src, "Linked picture to its source.");
        }

        private async Task SetImageLinkAsync(string url, string statusMessage)
        {
            var editor = GetEditor();
            if (editor == null || _lastImageState == null)
            {
                UpdateStatus("Select a picture first.");
                return;
            }

            await editor.SetImageLinkAsync(url);
            UpdateStatus(statusMessage);
        }

        // The Picture properties dialog: alt text/title, Link To, alignment,
        // margin, border — prefilled from the selected image's current state.
        private async Task ShowImagePropertiesAsync()
        {
            var editor = GetEditor();
            var img = _lastImageState;
            if (editor == null || img == null)
            {
                UpdateStatus("Select a picture first.");
                return;
            }

            ImagePropertiesDialogResult result = await ImagePropertiesDialog.ShowAsync(this, img);
            if (result == null)
                return;

            await editor.ApplyImageAttrsAsync(new ImageAttributes
            {
                Alt = result.AltText,
                Title = result.Title,
                Alignment = result.Alignment,
                MarginPx = result.MarginPx,
                BorderWidthPx = result.BorderWidthPx,
                BorderColor = result.BorderColor
            });

            string linkUrl = ImagePropertiesDialog.ResolveLinkUrl(result, img);
            if (result.LinkChoice == ImageLinkChoice.None)
            {
                if (img.LinkHref != null)
                    await editor.SetImageLinkAsync(null);
            }
            else if (linkUrl != null)
            {
                if (!string.Equals(linkUrl, img.LinkHref, System.StringComparison.Ordinal))
                    await editor.SetImageLinkAsync(linkUrl);
            }
            else
            {
                UpdateStatus("Embedded pictures have no source URL to link to.");
                return;
            }

            UpdateStatus("Updated picture properties.");
        }
    }
}
