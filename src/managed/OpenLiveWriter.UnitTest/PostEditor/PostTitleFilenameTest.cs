// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    /// <summary>
    /// Tests for the FileNameForTitle logic in PostEditorFile,
    /// covering issue #677: filename should update when a title is
    /// added to a previously untitled post.
    /// </summary>
    [TestClass]
    public class PostTitleFilenameTest
    {
        private const string WpostExtension = ".wpost";

        /// <summary>
        /// Invokes the internal FileNameForTitle method via reflection to avoid
        /// needing a fully-initialized PostEditorFile instance (which requires
        /// PostEditorPreferences and other runtime services).
        /// </summary>
        private static string InvokeFileNameForTitle(bool isPage, string postTitle)
        {
            // FileNameForTitle is an instance method, but its logic only depends on
            // its parameters (isPage, postTitle) and static helpers. We create a
            // minimal instance via reflection (bypassing the public constructors
            // which require runtime services).
            var ctor = typeof(PostEditorFile).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(DirectoryInfo) },
                null);

            // If the private constructor cannot be invoked without side-effects
            // (e.g. PostEditorPreferences), fall back to calling FileNameForTitle
            // as a static-like helper via MethodInfo.Invoke on an uninitialized object.
            PostEditorFile instance;
            try
            {
                instance = (PostEditorFile)ctor.Invoke(new object[] { new DirectoryInfo(Path.GetTempPath()) });
            }
            catch
            {
                // FormatterServices.GetUninitializedObject skips all constructors
                instance = (PostEditorFile)System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(typeof(PostEditorFile));
            }

            return instance.FileNameForTitle(isPage, postTitle);
        }

        /// <summary>
        /// Verify that an empty title produces a default untitled filename.
        /// </summary>
        [TestMethod]
        public void EmptyTitle_ProducesDefaultFilename()
        {
            string fileName = InvokeFileNameForTitle(false, String.Empty);

            Assert.IsTrue(fileName.EndsWith(WpostExtension),
                "Filename should have .wpost extension");
            Assert.IsFalse(String.IsNullOrEmpty(Path.GetFileNameWithoutExtension(fileName)),
                "Filename should not be empty before extension");
        }

        /// <summary>
        /// Verify that a null title produces a default untitled filename
        /// (same as empty title) rather than throwing or producing a GUID-based name.
        /// </summary>
        [TestMethod]
        public void NullTitle_ProducesDefaultFilename()
        {
            string fileNameEmpty = InvokeFileNameForTitle(false, String.Empty);
            string fileNameNull = InvokeFileNameForTitle(false, null);

            Assert.AreEqual(fileNameEmpty, fileNameNull,
                "Null title should produce the same filename as empty title");
        }

        /// <summary>
        /// Verify that a real title produces a filename based on that title.
        /// </summary>
        [TestMethod]
        public void RealTitle_ProducesMatchingFilename()
        {
            string fileName = InvokeFileNameForTitle(false, "My Great Blog Post");

            Assert.IsTrue(fileName.EndsWith(WpostExtension),
                "Filename should have .wpost extension");
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            Assert.IsTrue(nameWithoutExtension.Contains("My Great Blog Post"),
                "Filename should contain the post title");
        }

        /// <summary>
        /// Verify that changing from an empty title to a real title produces
        /// a different filename (the core scenario for issue #677).
        /// </summary>
        [TestMethod]
        public void ChangingFromEmptyToTitled_ProducesDifferentFilename()
        {
            string untitledFileName = InvokeFileNameForTitle(false, String.Empty);
            string titledFileName = InvokeFileNameForTitle(false, "Hello World");

            Assert.AreNotEqual(untitledFileName, titledFileName,
                "A titled post should have a different filename than an untitled post");
        }

        /// <summary>
        /// Verify that a whitespace-only title is treated the same as an empty title.
        /// </summary>
        [TestMethod]
        public void WhitespaceOnlyTitle_ProducesDefaultFilename()
        {
            string fileNameEmpty = InvokeFileNameForTitle(false, String.Empty);
            string fileNameWhitespace = InvokeFileNameForTitle(false, "   ");

            Assert.AreEqual(fileNameEmpty, fileNameWhitespace,
                "Whitespace-only title should produce the same filename as empty title");
        }

        /// <summary>
        /// Verify that the page flag changes the default filename
        /// (untitled page vs untitled post).
        /// </summary>
        [TestMethod]
        public void PageFlag_ChangesDefaultFilename()
        {
            string postFileName = InvokeFileNameForTitle(false, String.Empty);
            string pageFileName = InvokeFileNameForTitle(true, String.Empty);

            Assert.AreNotEqual(postFileName, pageFileName,
                "Untitled page and untitled post should have different default filenames");
        }

        /// <summary>
        /// Verify that FileHelper.GetValidFileName returns a usable filename
        /// for a normal blog post title.
        /// </summary>
        [TestMethod]
        public void GetValidFileName_WithNormalTitle_ReturnsUsableFilename()
        {
            string result = FileHelper.GetValidFileName("My First Post");
            Assert.IsFalse(String.IsNullOrEmpty(result));
            Assert.AreEqual("My First Post", result);
        }

        /// <summary>
        /// Verify that FileHelper.GetValidFileName strips invalid characters.
        /// </summary>
        [TestMethod]
        public void GetValidFileName_WithInvalidChars_StripsInvalidChars()
        {
            string result = FileHelper.GetValidFileName("Post: A <Test> Title");
            Assert.IsFalse(String.IsNullOrEmpty(result));
            Assert.IsFalse(result.Contains(":"), "Colon should be removed");
            Assert.IsFalse(result.Contains("<"), "Angle bracket should be removed");
            Assert.IsFalse(result.Contains(">"), "Angle bracket should be removed");
        }
    }
}
