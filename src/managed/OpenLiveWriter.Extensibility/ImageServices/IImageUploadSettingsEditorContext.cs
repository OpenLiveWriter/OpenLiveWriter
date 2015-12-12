// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Api;

namespace OpenLiveWriter.Extensibility.ImageServices
{
    /// <summary>
    /// Summary description for IImageUploadSettingsEditorContext.
    /// </summary>
    public interface IImageUploadSettingsEditorContext
    {
        IProperties ImageUploadSettings { get; }
    }
}
