// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OpenLiveWriter.Localization
{
    public class Res
    {
        [ThreadStatic]
        private static Font defaultFont;
        [ThreadStatic]
        private static Font italicFont;

        private static ResourceManager resMan;
        private static ResourceManager propResMan;
        private static readonly float SIZE_NORMAL;
        private static readonly float SIZE_LARGE;
        private static readonly float SIZE_XLARGE;
        private static readonly float SIZE_XXLARGE;
        private static readonly float SIZE_HEADING;
        private static readonly float SIZE_GIANTHEADING;
        private static readonly float SIZE_POSTSPLITCAPTION;
        private static readonly float SIZE_SMALL;

        static Res()
        {
            resMan = new ResourceManager("OpenLiveWriter.Localization.Strings", typeof(Res).Assembly);
            propResMan = new ResourceManager("OpenLiveWriter.Localization.Properties", typeof(Res).Assembly);
            SIZE_NORMAL = DefaultFont.SizeInPoints;
            SIZE_LARGE = GetFontSize(FontSize.Large, DefaultLargeFontSize);
            SIZE_XLARGE = GetFontSize(FontSize.XLarge, DefaultXLargeFontSize);
            SIZE_XXLARGE = GetFontSize(FontSize.XXLarge, DefaultXXLargeFontSize);
            SIZE_HEADING = GetFontSize(FontSize.Heading, DefaultHeadingFontSize);
            SIZE_GIANTHEADING = GetFontSize(FontSize.GiantHeading, DefaultGiantHeadingFontSize);
            SIZE_POSTSPLITCAPTION = GetFontSize(FontSize.PostSplitCaption, DefaultPostSplitCaptionFontSize);
            SIZE_SMALL = GetFontSize(FontSize.Small, DefaultSmallFontSize);
        }

        public static float DefaultNormalFontSize
        {
            get { return 9f; }
        }

        public static float DefaultLargeFontSize
        {
            get { return 10f; }
        }

        public static float DefaultXLargeFontSize
        {
            get { return 11f; }
        }

        public static float DefaultXXLargeFontSize
        {
            get { return 12f; }
        }

        public static float DefaultHeadingFontSize
        {
            get { return 12f; }
        }

        public static float DefaultGiantHeadingFontSize
        {
            get { return 15.75f; }
        }

        public static float DefaultToolbarFormatButtonFontSize
        {
            get { return 15f; }
        }

        public static float DefaultPostSplitCaptionFontSize
        {
            get { return 7f; }
        }

        public static float DefaultSmallFontSize
        {
            get { return 7f; }
        }

        static float GetFontSize(FontSize fontSize, float defaultSize)
        {
            string fontSizeString = propResMan.GetString("Font.Size." + fontSize);
            float fontSizeFloat;
            if (!String.IsNullOrEmpty(fontSizeString) && float.TryParse(fontSizeString.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out fontSizeFloat))
                return fontSizeFloat;
            else
                return defaultSize;
        }

        public static bool DebugMode = false;

        public static Font ItalicFont
        {
            get
            {
                if (!UseItalics)
                    return DefaultFont;

                if (italicFont == null)
                    italicFont = new Font(defaultFont, FontStyle.Italic);
                return (Font)italicFont.Clone();
            }
        }

        public static string Get(StringId stringId)
        {
            return Get(stringId.ToString());
        }

        public static string Get(string name)
        {
            if (DebugMode)
                return "[" + name + "]";

            return resMan.GetString(name) ?? ("@" + name);
        }

        public static string GetProp(string name)
        {
            return propResMan.GetString(name);
        }

        private static string GetInternal(StringId stringId)
        {
            return resMan.GetString(stringId.ToString());
        }

        public static char PasswordChar
        {
            get
            {
                if (IsPseudoLoc)
                    return (char)0x25CF;

                string passwordChar = Res.GetInternal(StringId.PasswordChar);
                if (passwordChar.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    return (char)int.Parse(passwordChar.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                else
                    return (char)int.Parse(passwordChar, CultureInfo.InvariantCulture);
            }
        }

        private static bool IsPseudoLoc
        {
            get { return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fo" || CultureInfo.CurrentUICulture.Name == "ar-ploc" || CultureInfo.CurrentUICulture.Name == "ja-ploc"; }
        }

        public static char Comma
        {
            get
            {
                return IsPseudoLoc ? ',' : Res.GetInternal(StringId.Comma).Trim()[0];
            }
        }

        public static string ListSeparator
        {
            get
            {
                return IsPseudoLoc ? ", " : Res.GetInternal(StringId.CommaSpace);
            }
        }

        public static string DefaultFontName
        {
            get { return "Segoe UI"; }
        }

        public static Font DefaultFont
        {
            get
            {
                if (defaultFont == null)
                {
                    string fontName = propResMan.GetString("Font") ?? DefaultFontName;

                    float fontSize;
                    if (!float.TryParse(propResMan.GetString("Font.Size.Normal"), NumberStyles.Float, CultureInfo.InvariantCulture, out fontSize))
                        fontSize = DefaultNormalFontSize;

                    defaultFont = new Font(fontName, fontSize, GraphicsUnit.Point);
                }

                return (Font)defaultFont.Clone();
            }
        }

        public static int DefaultSidebarWidth
        {
            get { return 200; }
        }

        public static int SidebarWidth
        {
            get
            {
                int sidebarWidth;
                if (int.TryParse(propResMan.GetString("Sidebar.WidthInPixels"), NumberStyles.Integer, CultureInfo.InvariantCulture, out sidebarWidth))
                    return sidebarWidth;
                else
                    return DefaultSidebarWidth;
            }
        }

        public static int DefaultWizardHeight
        {
            get { return 380; }
        }

        public static int WizardHeight
        {
            get
            {
                int wizardHeight;
                if (int.TryParse(propResMan.GetString("ConfigurationWizard.Height"), NumberStyles.Integer, CultureInfo.InvariantCulture, out wizardHeight))
                    return wizardHeight;
                else
                    return DefaultWizardHeight;
            }
        }

        public static bool DefaultUseItalics
        {
            get { return true; }
        }

        public static bool UseItalics
        {
            get
            {
                bool useItalics;
                if (bool.TryParse(propResMan.GetString("Culture.UseItalics"), out useItalics))
                    return useItalics;
                else
                    return DefaultUseItalics;
            }
        }

        public static Font GetFont(FontSize fontSize, FontStyle fontStyle)
        {
            float size;
            switch (fontSize)
            {
                default:
                case FontSize.Normal:
                    size = SIZE_NORMAL;
                    break;
                case FontSize.Large:
                    size = SIZE_LARGE;
                    break;
                case FontSize.XLarge:
                    size = SIZE_XLARGE;
                    break;
                case FontSize.XXLarge:
                    size = SIZE_XXLARGE;
                    break;
                case FontSize.Heading:
                    size = SIZE_HEADING;
                    break;
                case FontSize.GiantHeading:
                    size = SIZE_GIANTHEADING;
                    break;
                case FontSize.PostSplitCaption:
                    size = SIZE_POSTSPLITCAPTION;
                    break;
                case FontSize.Small:
                    size = SIZE_SMALL;
                    break;
            }
            return new Font(DefaultFont.FontFamily, size, fontStyle, GraphicsUnit.Point);
        }

        /// <summary>
        /// Toolbar font size **IN PIXELS**!
        /// This is the size of the font used to draw B(old),
        /// I(talic), U(nderline), S(trikethrough) buttons.
        /// </summary>
        public static float ToolbarFormatButtonFontSize
        {
            get
            {
                return GetFontSize(FontSize.ToolbarFormatButton, 15);
            }
        }

        /// <summary>
        /// Use this to mark places in the code that need to
        /// be localized. Like a comment, except you can use
        /// Resharper to find instances (important, because
        /// "Find in Files" is broken under VS.NET 2003 on Vista).
        /// </summary>
        [Conditional("DEBUG")]
        public static void LOCME(string comment)
        {
        }

        public static string[] Validate()
        {
            ArrayList errors = new ArrayList();

            ResourceSet resourceSet =
                propResMan.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            string fontFamilyName = propResMan.GetString("Font");

            try
            {
                char pc = PasswordChar;
            }
            catch (Exception)
            {
                errors.Add("Could not parse PasswordChar value: " + GetInternal(StringId.PasswordChar));
            }

            try
            {
                switch (fontFamilyName.ToUpperInvariant())
                {
                    case "GULIM":
                    case "PMINGLIU":
                    case "MS SANS SERIF":
                    case "SEGOE UI":
                    // These fonts are known good, but they can return localized names
                    // under localized Windows versions
                    // except Segoe UI, which is only on Vista (though the suite installer installs it on XP)
                    case "MICROSOFT YAHEI":
                        // this one is OK only for Vista
                        break;

                    default:
                        Font f = new Font(fontFamilyName, 10f);
                        if (0 != string.Compare(f.Name, fontFamilyName, true, CultureInfo.CurrentUICulture))
                        {
                            errors.Add("The font requested could not be loaded. Requested: \"" + fontFamilyName +
                                       "\", loaded: \"" + f.Name + "\"");
                        }
                        break;
                }
            }
            catch
            {
                errors.Add("The font requested could not be loaded. Requested: \"" + fontFamilyName + "\"");
            }

            foreach (string fontSize in new string[] { "Normal", "Large", "XLarge", "XXLarge", "Heading", "GiantHeading", "ToolbarFormatButton", "PostSplitCaption" })
                CheckFont(fontSize, errors);

            foreach (DictionaryEntry entry in resourceSet)
            {
                string key = entry.Key as string;
                string val = entry.Value as string;
                if (key != null && val != null)
                {
                    if (key.EndsWith(".Shortcut", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Enum.Parse(typeof(Shortcut), val, true);
                        }
                        catch
                        {
                            errors.Add("Invalid shortcut value: " + key + "=" + val);
                        }
                    }
                }
            }

            try
            {
                bool b = UseItalics;
            }
            catch (Exception)
            {
                errors.Add("Could not parse UseItalics value: " + propResMan.GetString("Culture.UseItalics"));
            }

            return (string[])errors.ToArray(typeof(string));
        }

        private static void CheckFont(string fontSizeBaseName, ArrayList errors)
        {
            string fontSizeName = "Font.Size." + fontSizeBaseName;
            string fontSize = propResMan.GetString(fontSizeName);
            float f;
            try
            {
                f = float.Parse(fontSize, CultureInfo.InvariantCulture);
            }
            catch
            {
                errors.Add(string.Format(CultureInfo.InvariantCulture, "{0} could not be parsed: {1}", fontSizeName, fontSize));
                return;
            }
            if (f > 25)
                errors.Add(string.Format(CultureInfo.InvariantCulture, "{0} is supiciously large: {1} => {2}", fontSizeName, fontSize, f));
            else if (f < 6)
                errors.Add(string.Format(CultureInfo.InvariantCulture, "{0} is supiciously small: {1} => {2}", fontSizeName, fontSize, f));
        }
    }

    public enum FontSize
    {
        Normal,
        /// <summary>
        /// 9 point
        /// </summary>
        Large,
        /// <summary>
        /// 9.75 point
        /// </summary>
        XLarge,
        /// <summary>
        /// 12 point
        /// </summary>
        XXLarge,
        /// <summary>
        /// 14.25 point
        /// </summary>
        Heading,
        /// <summary>
        /// 15.75 point
        /// </summary>
        GiantHeading,
        /// <summary>
        /// 15 pixels
        /// </summary>
        ToolbarFormatButton,
        /// <summary>
        /// 7 point
        /// </summary>
        PostSplitCaption,
        /// <summary>
        /// 7 point
        /// </summary>
        Small
    }
}
