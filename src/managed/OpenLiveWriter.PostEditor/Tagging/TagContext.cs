// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.Tagging
{
    public class TagContext
    {
        public TagContext(ISmartContent smartContent, IProperties pluginSettings, string currentBlogId)
        {
            _smartContent = smartContent;
            _pluginSettings = pluginSettings;
            _currentBlogId = currentBlogId;
        }
        private ISmartContent _smartContent;
        private IProperties _pluginSettings;
        private string _currentBlogId;

        private TagProvider[] Providers
        {
            get
            {
                return Manager.TagProviders;
            }
        }

        public TagProvider CurrentProvider
        {
            get
            {
                string defaultProviderName = DefaultProviderSettings.GetString(_currentBlogId, "63DA9645-F182-4da7-88BC-B430ECA1AD75");
                string providerId = _smartContent.Properties.GetString("currentProvider", defaultProviderName);

                foreach (TagProvider provider in Providers)
                {
                    if (providerId == provider.Id)
                        return provider;
                }
                return Providers[0];
            }
            set
            {
                if (value != null)
                {
                    _smartContent.Properties.SetString("currentProvider", value.Id);
                    DefaultProviderSettings.SetString(_currentBlogId, value.Id);
                }
            }
        }

        public string[] Tags
        {
            get
            {
                if (TagProperties == null)
                    return new string[0];
                return TagProperties.Names;
            }
            set
            {
                ClearProperties();
                foreach (string tag in value)
                    TagProperties.SetString(tag, "");
            }
        }

        public string[] PreviouslyUsedTags
        {
            get
            {
                return Decode(_pluginSettings.GetString("previousTags", ""));
            }
        }

        public void AddTagsToHistory(string[] tags)
        {
            string[] tagList = tags;
            if (PreviouslyUsedTags.Length > 0)
            {
                tagList = (string[])Union(new string[][] { PreviouslyUsedTags, tagList });
            }
            _pluginSettings.SetString("previousTags", Encode(tagList));

        }

        public IProperties PluginSettings
        {
            get
            {
                return _pluginSettings;
            }
        }

        public static Array Union(Array[] arrays)
        {
            Type underlyingType = arrays.GetType().GetElementType().GetElementType();
            if (underlyingType == null)
            {
                Trace.Fail("Underlying type is null");
            }
            if (arrays.Length == 0)
                return Array.CreateInstance(underlyingType, 0);

            if (arrays.Length == 1)
                return arrays[0];

            // using Hashtable as a HashSet
            Hashtable table = new Hashtable(arrays[0].Length * arrays.Length + 1);
            foreach (Array array in arrays)
            {
                foreach (object o in array)
                {
                    table[o] = true;
                }
            }

            Array union = new object[table.Count];

            int pos = 0;
            foreach (object o in table.Keys)
            {
                union.SetValue(o, pos++);
            }
            return Narrow(union, underlyingType);
        }

        private TagProviderManager Manager
        {
            get
            {
                if (_manager == null)
                    _manager = new TagProviderManager(_pluginSettings);
                return _manager;
            }
        }
        private TagProviderManager _manager;

        public static Array Narrow(Array array, Type type)
        {
            Array newArray = Array.CreateInstance(type, array.LongLength);
            Array.Copy(array, newArray, array.LongLength);
            return newArray;
        }

        private string Encode(string[] items)
        {
            StringBuilder builder = new StringBuilder();
            string sep = "";
            foreach (string item in items)
            {
                builder.Append(sep + item);
                sep = SEP.ToString();
            }
            return builder.ToString();
        }
        private const char SEP = ',';

        private string[] Decode(string encodedItems)
        {
            if (encodedItems.Length < 1)
                return new string[0];
            return encodedItems.Split(SEP);
        }

        private IProperties TagProperties
        {
            get
            {
                return _smartContent.Properties.GetSubProperties("tags");
            }
        }

        private void ClearProperties()
        {
            _smartContent.Properties.RemoveSubProperties("tags");
        }

        private IProperties DefaultProviderSettings
        {
            get
            {
                return _pluginSettings.GetSubProperties("Defaults");
            }
        }
    }
}
