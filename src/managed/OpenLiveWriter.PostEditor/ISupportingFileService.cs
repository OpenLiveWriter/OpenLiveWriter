// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor
{
    public interface ISupportingFileService
    {
        ISupportingFile CreateSupportingFile(string fileName, Stream s);
        ISupportingFile CreateSupportingFile(string fileName, string fileNameUniqueToken, Stream s);
        ISupportingFile AddLinkedSupportingFileReference(Uri sourceUri);
        ISupportingFile GetFileByUri(Uri uri);
        ISupportingFile GetFileById(string id);
    }

    public interface ISupportingFileUploadInfo
    {
        int UploadedFileVersion { get; }

        Uri UploadUri { get; }

        BlogPostSettingsBag UploadSettings { get; }
    }
}
