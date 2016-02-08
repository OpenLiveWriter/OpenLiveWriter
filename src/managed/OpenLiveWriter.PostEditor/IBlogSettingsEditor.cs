// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;

namespace OpenLiveWriter.PostEditor
{

    public interface IBlogSettingsEditor : IDisposable
    {
        event EventHandler SettingsChanged;
        void Init(BlogSettings settings);
        Control EditorControl { get; }
        void ApplySettings();
        string Title { get; }
    }
}
