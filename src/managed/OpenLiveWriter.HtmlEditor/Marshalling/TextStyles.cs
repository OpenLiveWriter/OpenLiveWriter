// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class TextStyles : IEnumerable<TextStyle>
    {
        /// <summary>
        /// This is what MSHTML uses internally.
        /// </summary>
        private const int LF_FACESIZE = 32;

        public TextStyles(MarkupPointer markupPointer)
        {
            if (markupPointer == null)
            {
                throw new ArgumentNullException("markupPointer");
            }

            if (!markupPointer.Positioned)
            {
                throw new ArgumentException("markupPointer must be positioned.");
            }

            IHTMLDocument2 document = markupPointer.Container.Document;
            IDisplayServicesRaw displayServices = (IDisplayServicesRaw)document;
            IHTMLComputedStyle computedStyle;
            displayServices.GetComputedStyle(markupPointer.PointerRaw, out computedStyle);

            MarkupPointer = markupPointer;
            ComputedStyle = computedStyle;
        }

        public MarkupPointer MarkupPointer
        {
            get;
            private set;
        }

        public IHTMLComputedStyle ComputedStyle
        {
            get;
            private set;
        }

        public string FontFamily
        {
            get
            {
                StringBuilder fontNameBuffer = new StringBuilder(LF_FACESIZE);
                ComputedStyle.fontName(fontNameBuffer);
                return fontNameBuffer.ToString();
            }
        }

        public float FontSize
        {
            get
            {
                float pixels = DisplayHelper.TwipsToPixelsY(ComputedStyle.fontSize);
                return HTMLElementHelper.PixelsToPointSize(pixels, true);
            }
        }

        public Color FontColor
        {
            get
            {
                return Color.FromArgb(ComputedStyle.textColor);
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return Color.FromArgb(ComputedStyle.backgroundColor);
            }
        }

        public bool Bold
        {
            get
            {
                return ComputedStyle.bold;
            }
        }

        public bool Italic
        {
            get
            {
                return ComputedStyle.italic;
            }
        }

        public bool Underline
        {
            get
            {
                return ComputedStyle.underline;
            }
        }

        public bool StrikeThrough
        {
            get
            {
                return ComputedStyle.strikeOut;
            }
        }

        public bool Overline
        {
            get
            {
                return ComputedStyle.overline;
            }
        }

        public bool SmallCaps
        {
            get
            {
                IHTMLElement currentScope = MarkupPointer.CurrentScope;
                IHTMLCurrentStyle currentStyle = ((IHTMLElement2)currentScope).currentStyle;
                return String.Compare(currentStyle.fontVariant, "small-caps", StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

        public bool SubScript
        {
            get
            {
                return ComputedStyle.subScript;
            }
        }

        public bool SuperScript
        {
            get
            {
                return ComputedStyle.superScript;
            }
        }

        #region IEnumerable<TextStyle> Members

        public IEnumerator<TextStyle> GetEnumerator()
        {
            yield return new FontFamilyTextStyle(FontFamily);
            yield return new FontSizeTextStyle(FontSize);
            yield return new FontColorTextStyle(FontColor);
            yield return new BackgroundColorTextStyle(BackgroundColor);
            yield return new BoldTextStyle(Bold);
            yield return new ItalicTextStyle(Italic);
            yield return new UnderlineTextStyle(Underline);
            yield return new StrikeThroughTextStyle(StrikeThrough);
            yield return new OverlineTextStyle(Overline);
            yield return new SubScriptTextStyle(SubScript);
            yield return new SuperScriptTextStyle(SuperScript);
            yield return new SmallCapsTextStyle(SmallCaps);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
