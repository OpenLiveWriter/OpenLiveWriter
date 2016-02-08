// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Emoticons
{
    public class Emoticon : IComparable
    {
        public const string CLASS_NAME = "wlEmoticon";
        private Bitmap _bitmapStrip = null;     // bitmap strip containing all emoticons
        private readonly int _index = 0;        // index into the _bitmapStrip
        private Bitmap _bitmapCached = null;

        public Emoticon(List<string> autoReplaceText, string altText, Bitmap bitmapStrip, int index, string id, string fileExtension)
        {
            AutoReplaceText = autoReplaceText;
            AltText = altText;
            _bitmapStrip = bitmapStrip;
            _index = index;
            Id = id;
            FileExtension = fileExtension;
        }

        public List<string> AutoReplaceText { get; private set; }
        public string AltText { get; private set; }
        public Bitmap Bitmap
        {
            get
            {
                // WinLive 276619: Watson: System.InvalidOperationException: Object is currently in use elsewhere.
                // We must protect against the Bitmap being referenced by multiple threads simultaneously.
                if (_bitmapCached == null && _bitmapStrip != null && _index >= 0)
                {
                    // Create a square bitmap out of the strip at location specified by _index
                    // Strip is assumed to be horizontal and so the height is size of an individual square emoticon's size
                    lock (_bitmapStrip)
                    {
                        int emoSize = _bitmapStrip.Height;
                        _bitmapCached = _bitmapStrip.Clone(new Rectangle(_index * emoSize, 0, emoSize, emoSize), _bitmapStrip.PixelFormat);
                    }
                }

                return _bitmapCached;
            }
        }
        public string Id { get; private set; }
        public string FileExtension { get; private set; }

        public string Label
        {
            get
            {
                Debug.Assert(AutoReplaceText.Count > 0, AltText + " emoticon is missing auto-replace text!");

                if (AutoReplaceText.Count == 1)
                    return String.Format(CultureInfo.InvariantCulture, Res.Get(StringId.EmoticonsTooltipOneAutoReplace), AltText, AutoReplaceText[0]);
                else
                    return String.Format(CultureInfo.InvariantCulture, Res.Get(StringId.EmoticonsTooltipTwoAutoReplace), AltText, AutoReplaceText[0], AutoReplaceText[1]);
            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is Emoticon)
            {
                Emoticon otherEmoticon = (Emoticon)obj;
                return Id.CompareTo(otherEmoticon.Id);
            }
            else if (obj == null)
            {
                // By definition, any object compares greater than a null reference.
                return 1;
            }

            throw new ArgumentException("object is not an Emoticon");
        }

        #endregion
    }
}
