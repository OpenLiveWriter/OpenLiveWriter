// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    /// <summary>
    /// Tests for the CleanupHtml iteration limit in EditableRegionElementBehavior.
    /// Addresses issue #775: OutOfMemoryException in GetEditedHtml caused by
    /// unbounded cleanup iterations on pathological HTML.
    /// </summary>
    [TestClass]
    public class CleanupHtmlIterationLimitTest
    {
        // The class is internal, so we access it via reflection through a public type in the same assembly.
        private static readonly Type BehaviorType = typeof(OpenLiveWriter.PostEditor.PostEditorPreferences)
            .Assembly
            .GetType("OpenLiveWriter.PostEditor.PostHtmlEditing.EditableRegionElementBehavior");

        [TestMethod]
        public void MaxCleanupIterations_IsPositiveAndReasonable()
        {
            Assert.IsNotNull(BehaviorType, "EditableRegionElementBehavior type should be resolvable via reflection");

            var field = BehaviorType.GetField("MaxCleanupIterations",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.IsNotNull(field, "MaxCleanupIterations field should exist on EditableRegionElementBehavior");
            var value = (int)field.GetValue(null);
            Assert.IsTrue(value > 0, "MaxCleanupIterations should be positive");
            Assert.IsTrue(value <= 100, "MaxCleanupIterations should be reasonably bounded to prevent OOM");
        }

        [TestMethod]
        public void CleanupHtml_WithCleanInput_PreservesContent()
        {
            // Simple HTML that requires no meaningless tag removal should complete
            // in a single iteration and preserve all content.
            string input = "<p>Hello <strong>world</strong></p>";
            string result = InvokeCleanupHtml(input, false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Hello"), "Cleaned HTML should preserve text content");
            Assert.IsTrue(result.Contains("<p>"), "Cleaned HTML should preserve paragraph tags");
            Assert.IsTrue(result.Contains("<strong>"), "Cleaned HTML should preserve strong tags");
        }

        [TestMethod]
        public void CleanupHtml_RemovesMeaninglessEmptyParagraphs()
        {
            // Empty <p></p> tags are considered meaningless and should be removed.
            string input = "<p></p><p>Content</p>";
            string result = InvokeCleanupHtml(input, false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Content"), "Should preserve content paragraphs");
            // The empty <p></p> should have been removed
            Assert.IsFalse(result.Equals(input), "Empty paragraph tags should be cleaned up");
        }

        [TestMethod]
        public void CleanupHtml_LowercasesTagNames()
        {
            string input = "<P>Hello</P>";
            string result = InvokeCleanupHtml(input, false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("<p>"), "Tag names should be lowercased");
            Assert.IsTrue(result.Contains("</p>"), "End tag names should be lowercased");
        }

        [TestMethod]
        public void CleanupHtml_StripsContentEditableAttribute()
        {
            string input = "<div contenteditable=\"true\">Text</div>";
            string result = InvokeCleanupHtml(input, false);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Contains("contenteditable"),
                "contenteditable attribute should be stripped during cleanup");
        }

        [TestMethod]
        public void CleanupHtml_WithManyEmptyParagraphs_RespectsIterationLimit()
        {
            // Create HTML with many empty paragraphs that each require a
            // separate cleanup pass. This verifies the iteration limit prevents
            // unbounded looping on pathological input.
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 50; i++)
            {
                sb.Append("<p></p>");
            }
            sb.Append("<p>Final content</p>");

            string input = sb.ToString();
            string result = InvokeCleanupHtml(input, false);

            Assert.IsNotNull(result, "CleanupHtml should return a result even with many empty paragraphs");
            Assert.IsTrue(result.Contains("Final content"), "Should preserve non-empty content");
        }

        /// <summary>
        /// Invokes the private CleanupHtml method via reflection for testing.
        /// Creates an uninitialized instance using FormatterServices to avoid
        /// constructor dependencies. CleanupHtml does not access any instance
        /// fields, so this is safe.
        /// </summary>
        private static string InvokeCleanupHtml(string html, bool xml)
        {
            Assert.IsNotNull(BehaviorType, "EditableRegionElementBehavior type should be resolvable via reflection");

            var method = BehaviorType.GetMethod("CleanupHtml",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "CleanupHtml method should exist on EditableRegionElementBehavior");

            // Create an uninitialized instance to call the private method.
            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(BehaviorType);

            return (string)method.Invoke(instance, new object[] { html, xml });
        }
    }
}
