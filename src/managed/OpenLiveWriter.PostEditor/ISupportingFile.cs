// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;

namespace OpenLiveWriter.PostEditor
{
    public interface ISupportingFile
    {
        string FileId { get; }

        int FileVersion { get; }

        string FileName { get; }

        string FileNameUniqueToken { get; }

        bool Embedded { get; }

        BlogPostSettingsBag Settings { get; }

        ISupportingFile UpdateFile(Stream newContents);

        ISupportingFile UpdateFile(Stream newContents, string newFileName);

        Uri FileUri { get; }

        ISupportingFileUploadInfo GetUploadInfo(string uploadContextId);

        bool IsUploaded(string uploadContextId);

        void MarkUploaded(string uploadContextId, Uri uploadUri);

        void Delete();
    }
}
