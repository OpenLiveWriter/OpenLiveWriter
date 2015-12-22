// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for CopyrightDecorator.
    /// </summary>
    public class WatermarkDecorator : IImageDecorator
    {
        public WatermarkDecorator()
        {

        }

        public readonly static string Id = "Watermark";

        public void Decorate(ImageDecoratorContext context)
        {
            WatermarkDecoratorSettings settings = new WatermarkDecoratorSettings(context.Settings);
            if (string.IsNullOrEmpty(settings.Text))
                return;

            Bitmap bitmap = new Bitmap(context.Image.Width, context.Image.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(context.Image, 0, 0, context.Image.Width, context.Image.Height);

                Font font = new Font(settings.FontFamily, settings.FontSize * (96f / 72f), GraphicsUnit.Pixel);
                SizeF sizef = g.MeasureString(settings.Text, font);

                int x = 0;
                int y = 0;
                switch (settings.Position)
                {
                    case (WatermarkPosition.TopLeft):
                        break;
                    case (WatermarkPosition.TopRight):
                        x = Convert.ToInt32(Math.Max(bitmap.Width - sizef.Width - 2, 0));
                        break;
                    case (WatermarkPosition.Centered):
                        x = Convert.ToInt32(Math.Max((bitmap.Width / 2) - (sizef.Width / 2), 0));
                        y = Convert.ToInt32(Math.Max((bitmap.Height / 2) - (sizef.Height / 2), 0));
                        break;
                    case (WatermarkPosition.BottomLeft):
                        y = Convert.ToInt32(Math.Max(bitmap.Height - sizef.Height, 0));
                        break;
                    case (WatermarkPosition.BottomRight):
                        x = Convert.ToInt32(Math.Max(bitmap.Width - sizef.Width - 2, 0));
                        y = Convert.ToInt32(Math.Max(bitmap.Height - sizef.Height, 0));
                        break;
                }

                g.DrawString(settings.Text, font,
                    new SolidBrush(Color.FromArgb(153, 0, 0, 0)), x, y);

                g.DrawString(settings.Text, font,
                    new SolidBrush(Color.FromArgb(153, 255, 255, 255)), x - 1, y - 1);
            }

            context.Image = bitmap;
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new WatermarkEditor();
        }

        internal class WatermarkDecoratorSettings
        {
            private static readonly string TEXT = "Text";
            private static readonly string POSITION = "Position";
            private static readonly string FONTFAMILY = "FontFamily";
            private static readonly string FONTSIZE = "FontSize";

            private readonly IProperties Settings;
            public WatermarkDecoratorSettings(IProperties settings)
            {
                Settings = settings;
            }

            public string Text
            {
                get { return Settings.GetString(TEXT, ""); }
                set { Settings.SetString(TEXT, value); }
            }

            public WatermarkPosition Position
            {
                get
                {
                    return (WatermarkPosition)Settings.GetInt(POSITION, 3);
                }
                set
                {
                    Settings.SetInt(POSITION, (int)value);
                }
            }

            public string FontFamily
            {
                get
                {
                    return Settings.GetString(FONTFAMILY, Res.Get(StringId.WatermarkDefaultFont));
                }
                set
                {
                    Settings.SetString(FONTFAMILY, value);
                }
            }

            public int FontSize
            {
                get
                {
                    return Settings.GetInt(FONTSIZE, 12);
                }
                set
                {
                    Settings.SetInt(FONTSIZE, value);
                }
            }
        }

        internal enum WatermarkPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Centered
        }
    }
}
