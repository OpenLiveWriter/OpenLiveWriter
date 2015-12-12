// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;
using System.Web;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tagging
{

    public class TagProvider : IComparable
    {
        private const string DEFAULT_FORMAT = "<a href=\"{0}\" rel=\"tag\">{1}</a>";

        public string GenerateHtmlForTags(string[] tags)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            for (int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];

                string encodedTags;
                if (String.IsNullOrEmpty(EncodingName))
                    encodedTags = HttpUtility.UrlEncode(tag);
                else
                    encodedTags = HttpUtility.UrlEncode(tag, Encoding.GetEncoding(EncodingName));

                string finalHtml = HtmlFormat.Replace(TAG_TOKEN, HttpUtility.HtmlEncode(tag));
                finalHtml = finalHtml.Replace(TAG_ENCODED_TOKEN, HttpUtility.HtmlEncode(encodedTags));

                htmlBuilder.Append(finalHtml);
                if (i < (tags.Length - 1))
                    htmlBuilder.Append(Separator);
            }

            string tagGroup = htmlBuilder.ToString();

            if (Caption.IndexOf(TAG_GROUP_TOKEN) > -1)
            {
                return Caption.Replace(TAG_GROUP_TOKEN, tagGroup);
            }
            else
            {
                return Caption + tagGroup;
            }
        }

        public string Id
        {
            get
            {
                if (_id == null)
                    _id = Guid.NewGuid().ToString();
                return _id;
            }
        }
        private string _id;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string Caption
        {
            get
            {
                if (_caption == null)
                    _caption = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.TagsProviderDefaultCaption), Name, TAG_GROUP_TOKEN).Trim();
                return _caption;
            }
            set
            {
                _caption = value;
            }
        }

        public bool New
        {
            get
            {
                return _new;
            }
        }

        public string HtmlFormat
        {
            get
            {
                if (_htmlFormatString == null)
                {
                    _htmlFormatString = string.Format(CultureInfo.InvariantCulture, DEFAULT_FORMAT, "http://www.example.com/" + TAG_ENCODED_TOKEN, TAG_TOKEN);
                }
                return _htmlFormatString;
            }
            set
            {
                _htmlFormatString = value;
            }
        }

        public string EncodingName
        {
            get
            {
                return _encodingName;
            }
            set
            {
                _encodingName = value;
            }
        }

        private string _encodingName = null;

        public string Separator
        {
            get
            {
                if (_separator == null)
                    _separator = Res.ListSeparator;
                return _separator;
            }
            set
            {
                _separator = value;
            }
        }
        private string _separator;

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return (obj is TagProvider && ((TagProvider)obj).Id == Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            TagProvider otherProvider = obj as TagProvider;
            if (otherProvider == null)
                return 0;

            return (Name.CompareTo(otherProvider.Name));
        }

        public TagProvider(string id)
        {
            _id = id;
        }

        public TagProvider(string id, string name, string caption, string htmlFormat, string separator, string encodingName)
        {
            _id = id;
            _name = name;
            _caption = caption;
            _htmlFormatString = htmlFormat;
            _separator = separator;
            _encodingName = encodingName;
        }

        public TagProvider()
        {
            _new = true;
        }

        private static IProperties GetTagProviderSettings(IProperties pluginSettings)
        {
            return pluginSettings.GetSubProperties("TagProviders");
        }

        public static void Delete(string providerId, IProperties pluginSettings)
        {
            GetTagProviderSettings(pluginSettings).RemoveSubProperties(providerId);
        }

        private string _name;
        private string _caption;
        private bool _new = false;
        private string _htmlFormatString;

        public const string TAG_TOKEN = "{tag}";
        public const string TAG_ENCODED_TOKEN = "{tag-encoded}";
        public const string TAG_GROUP_TOKEN = "{tag-group}";
    }
}
