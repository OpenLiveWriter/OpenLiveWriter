// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public class ExifMetadata
    {
        class ExifTags
        {
            //Useful reference for more tags:
            //http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
            internal const int ImageDescription = 0x010e;
            internal const int Orientation = 0x112;
        }

        private Image _image;
        private ExifMetadata(Image image)
        {
            _image = image;
        }

        public string ImageDescription
        {
            get { return GetStringProperty(ExifTags.ImageDescription, null); }
        }

        public ExifOrientation Orientation
        {
            get
            {
                ExifOrientation o = ExifOrientation.Unknown;
                PropertyItem pi = GetProperty(ExifTags.Orientation);
                if (pi != null)
                    o = (ExifOrientation)pi.Value[0];
                return o;
            }
            set
            {
                if (value == ExifOrientation.Unknown)
                {
                    _image.RemovePropertyItem(ExifTags.Orientation);
                }
                PropertyItem orientation = GetProperty(ExifTags.Orientation);
                orientation.Value[0] = (byte)value;
                _image.SetPropertyItem(orientation);
            }
        }

        public static ExifMetadata FromImage(Image image)
        {
            return new ExifMetadata(image);
        }

        private PropertyItem GetProperty(int exifTag)
        {
            try
            {
                return _image.GetPropertyItem(exifTag);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetStringProperty(int exifTag, string defaultValue)
        {
            PropertyItem pi = GetProperty(exifTag);
            if (pi != null)
                return Encoding.UTF8.GetString(pi.Value);
            else
                return defaultValue;
        }

        private void DumpAllExifProperties()
        {
            foreach (PropertyItem pi in _image.PropertyItems)
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "0x{0}: {1}: {2}", pi.Id.ToString("X", CultureInfo.InvariantCulture), pi.Type, pi.Type == 2 ? Encoding.UTF8.GetString(pi.Value) : ""));
            }
        }
    }

    public enum ExifOrientation
    {
        Normal = 1,
        Rotate90CW = 6,
        Rotate270CW = 8,
        Unknown = -1
    }
}
