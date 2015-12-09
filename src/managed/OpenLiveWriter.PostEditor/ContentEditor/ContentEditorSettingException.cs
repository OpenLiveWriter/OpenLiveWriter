// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor
{
    public class ContentEditorSettingException : ApplicationException
    {
        public ContentEditorSettingException(ContentEditorSetting setting)
        {
            Setting = setting;
        }

        public ContentEditorSetting Setting { get; set; }
    }
}
