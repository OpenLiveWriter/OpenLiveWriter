// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com.StructuredStorage;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{

    public class PostEditorException : DisplayableException
    {
        protected PostEditorException(StringId titleFormat, StringId textFormat, params object[] textFormatArgs)
            : base(titleFormat, textFormat, textFormatArgs)
        {
        }
    }

    public class PostEditorStorageException : PostEditorException
    {
        private PostEditorStorageException(StorageNoDiskSpaceException ex)
            : base(StringId.PostEditorStorageExceptionTitle,
            StringId.PostEditorDiskSpaceExceptionMessage,
            ex.NativeErrorCode,
            ex.Message)
        {
        }

        private PostEditorStorageException(StorageException ex)
            : base(StringId.PostEditorStorageExceptionTitle,
            StringId.PostEditorStorageExceptionMessage,
            ex.NativeErrorCode,
            ex.Message)
        {
        }

        private PostEditorStorageException(IOException ex)
            : base(StringId.PostEditorStorageExceptionTitle2,
            StringId.PostEditorStorageExceptionMessage2,
            ex.GetType().Name,
            ex.Message)
        {
        }

        private PostEditorStorageException(Exception ex)
            : base(StringId.PostEditorStorageExceptionTitle,
            StringId.PostEditorStorageExceptionMessage,
            ex.GetType().Name,
            ex.Message)
        {
        }

        public static PostEditorStorageException Create(Exception ex)
        {
            if (ex is StorageNoDiskSpaceException)
                return new PostEditorStorageException((StorageNoDiskSpaceException)ex);

            if (ex is IOException)
                return new PostEditorStorageException((IOException)ex);

            if (ex is StorageException)
                return new PostEditorStorageException((StorageException)ex);

            return new PostEditorStorageException(ex);
        }
    }

}
