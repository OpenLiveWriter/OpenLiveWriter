// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.Interop.Com.StructuredStorage;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestClass]
    public class PostEditorStorageExceptionTests
    {
        [TestMethod]
        public void Create_WithStorageNoDiskSpaceException_ReturnsStorageException()
        {
            // STG_E_MEDIUMFULL = 0x80030070
            var comEx = new COMException("Disk full", unchecked((int)0x80030070));
            var storageEx = new StorageNoDiskSpaceException(comEx);

            var result = PostEditorStorageException.Create(storageEx);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PostEditorStorageException));
        }

        [TestMethod]
        public void Create_WithIOException_ReturnsStorageException()
        {
            var ioEx = new IOException("Disk error");

            var result = PostEditorStorageException.Create(ioEx);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PostEditorStorageException));
        }

        [TestMethod]
        public void Create_WithStorageException_ReturnsStorageException()
        {
            // STG_E_INSUFFICIENTMEMORY = 0x80030008
            var comEx = new COMException("Insufficient memory", unchecked((int)0x80030008));
            var storageEx = new StorageException(comEx);

            var result = PostEditorStorageException.Create(storageEx);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PostEditorStorageException));
        }

        [TestMethod]
        public void Create_WithGenericException_ReturnsStorageException()
        {
            // 0x80070008 = ERROR_NOT_ENOUGH_MEMORY (-2147024888)
            // This is the error code from issue #678
            var ex = new InvalidOperationException("Not enough memory");

            var result = PostEditorStorageException.Create(ex);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PostEditorStorageException));
        }

        [TestMethod]
        public void Create_WithNullMessageException_DoesNotThrow()
        {
            var ex = new Exception();

            var result = PostEditorStorageException.Create(ex);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void StorageDispose_IsIdempotent()
        {
            // Verify that Storage.Dispose can be called multiple times without error.
            // This is important because SupportingFilePersister.Dispose now releases
            // the Storage, and in the Load path the outer using block may also dispose it.
            // We test Storage's IDisposable contract here.
            var tempFile = Path.GetTempFileName();
            try
            {
                File.Delete(tempFile); // Storage.Create needs the file to not exist
                var storage = new Storage(tempFile, StorageMode.Create, true);

                // First dispose should release the COM object
                storage.Dispose();

                // Second dispose should be a no-op (not throw)
                storage.Dispose();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
