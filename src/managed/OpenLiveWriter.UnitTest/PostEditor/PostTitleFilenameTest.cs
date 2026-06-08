// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    /// <summary>
    /// Tests for the FileNameForTitle logic in PostEditorFile,
    /// covering issue #677: filename should update when a title is
    /// added to a previously untitled post.
    /// </summary>
    [TestFixture]
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
        [Test]
        public void EmptyTitle_ProducesDefaultFilename()
        {
            string fileName = InvokeFileNameForTitle(false, String.Empty);

            ClassicAssert.IsTrue(fileName.EndsWith(WpostExtension),
                "Filename should have .wpost extension");
            ClassicAssert.IsFalse(String.IsNullOrEmpty(Path.GetFileNameWithoutExtension(fileName)),
                "Filename should not be empty before extension");
        }

        /// <summary>
        /// Verify that a null title produces a default untitled filename
        /// (same as empty title) rather than throwing or producing a GUID-based name.
        /// </summary>
        [Test]
        public void NullTitle_ProducesDefaultFilename()
        {
            string fileNameEmpty = InvokeFileNameForTitle(false, String.Empty);
            string fileNameNull = InvokeFileNameForTitle(false, null);

            ClassicAssert.AreEqual(fileNameEmpty, fileNameNull,
                "Null title should produce the same filename as empty title");
        }

        /// <summary>
        /// Verify that a real title produces a filename based on that title.
        /// </summary>
        [Test]
        public void RealTitle_ProducesMatchingFilename()
        {
            string fileName = InvokeFileNameForTitle(false, "My Great Blog Post");

            ClassicAssert.IsTrue(fileName.EndsWith(WpostExtension),
                "Filename should have .wpost extension");
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            ClassicAssert.IsTrue(nameWithoutExtension.Contains("My Great Blog Post"),
                "Filename should contain the post title");
        }

        /// <summary>
        /// Verify that changing from an empty title to a real title produces
        /// a different filename (the core scenario for issue #677).
        /// </summary>
        [Test]
        public void ChangingFromEmptyToTitled_ProducesDifferentFilename()
        {
            string untitledFileName = InvokeFileNameForTitle(false, String.Empty);
            string titledFileName = InvokeFileNameForTitle(false, "Hello World");

            ClassicAssert.AreNotEqual(untitledFileName, titledFileName,
                "A titled post should have a different filename than an untitled post");
        }

        /// <summary>
        /// Verify that a whitespace-only title is treated the same as an empty title.
        /// </summary>
        [Test]
        public void WhitespaceOnlyTitle_ProducesDefaultFilename()
        {
            string fileNameEmpty = InvokeFileNameForTitle(false, String.Empty);
            string fileNameWhitespace = InvokeFileNameForTitle(false, "   ");

            ClassicAssert.AreEqual(fileNameEmpty, fileNameWhitespace,
                "Whitespace-only title should produce the same filename as empty title");
        }

        /// <summary>
        /// Verify that the page flag changes the default filename
        /// (untitled page vs untitled post).
        /// </summary>
        [Test]
        public void PageFlag_ChangesDefaultFilename()
        {
            string postFileName = InvokeFileNameForTitle(false, String.Empty);
            string pageFileName = InvokeFileNameForTitle(true, String.Empty);

            ClassicAssert.AreNotEqual(postFileName, pageFileName,
                "Untitled page and untitled post should have different default filenames");
        }

        /// <summary>
        /// Verify that FileHelper.GetValidFileName returns a usable filename
        /// for a normal blog post title.
        /// </summary>
        [Test]
        public void GetValidFileName_WithNormalTitle_ReturnsUsableFilename()
        {
            string result = FileHelper.GetValidFileName("My First Post");
            ClassicAssert.IsFalse(String.IsNullOrEmpty(result));
            ClassicAssert.AreEqual("My First Post", result);
        }

        /// <summary>
        /// Verify that FileHelper.GetValidFileName strips invalid characters.
        /// </summary>
        [Test]
        public void GetValidFileName_WithInvalidChars_StripsInvalidChars()
        {
            string result = FileHelper.GetValidFileName("Post: A <Test> Title");
            ClassicAssert.IsFalse(String.IsNullOrEmpty(result));
            ClassicAssert.IsFalse(result.Contains(":"), "Colon should be removed");
            ClassicAssert.IsFalse(result.Contains("<"), "Angle bracket should be removed");
            ClassicAssert.IsFalse(result.Contains(">"), "Angle bracket should be removed");
        }
    }
}



