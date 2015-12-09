// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using Microsoft.Win32;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using System.Security;
using System.IO;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    using PostHtmlEditing;

    public class OpenLiveWriterSettingsProvider : ISettingsProvider
    {
        public ManagedPropVariant GetSetting(ContentEditorSetting setting)
        {
            switch (setting)
            {
                case ContentEditorSetting.MshtmlOptionKeyPath:
                    return ManagedPropVariant.FromPropVariant(GetMshtmlOptionKeyPath());

                case ContentEditorSetting.ImageDefaultSize:
                    return ManagedPropVariant.FromPropVariant(GetImageDefaultSize());

                case ContentEditorSetting.Language:
                    return ManagedPropVariant.FromPropVariant(GetLanguage());

                default:
                    Debug.Fail("Unexpected ContentEditorSetting!");
                    PropVariant propVariant = new PropVariant();
                    propVariant.SetError();
                    return ManagedPropVariant.FromPropVariant(propVariant);
            }
        }

        private PropVariant GetMshtmlOptionKeyPath()
        {
            // Writer just uses the default MSHTML options, so we don't provide an option key path.
            return new PropVariant(string.Empty);
        }

        private PropVariant GetImageDefaultSize()
        {
            // Writer defaults to inserting images as Small.
            return new PropVariant(ImageSizeName.Small.ToString());
        }

        private PropVariant GetLanguage()
        {
            // TODO:OLW This used to get the language.
            return new PropVariant("en");
        }
    }
}
