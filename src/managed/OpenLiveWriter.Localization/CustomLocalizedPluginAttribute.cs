// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Localization
{
    /// <summary>
    /// Indicates that a plugin's localized strings should be
    /// retrieved from the OpenLiveWriter.Localization
    /// assembly, not from the plugin assembly itself. This
    /// allows us to localize built-in plugins (like Maps and
    /// Tags) without building separate satellite assemblies
    /// for them.
    ///
    /// The strings should be named with e.g. the following format:
    ///
    /// Plugin_[Name]_WriterPlugin_Description
    /// Plugin_[Name]_InsertableContentSource_MenuText
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomLocalizedPluginAttribute : Attribute
    {
        private readonly string _name;

        public CustomLocalizedPluginAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
