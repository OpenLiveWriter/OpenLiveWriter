// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.Video;

namespace OpenLiveWriter.Tests.PostEditor
{
    /// <summary>
    /// Insert > Video routes through the sidebar's smart content editor cache.
    /// The deprecated Video source has no editor, and caching that null made the
    /// second insert trip the "already cached" assertion, which takes the app
    /// down. These cover the repeat-insert path for every editor-less source.
    /// </summary>
    [TestFixture]
    public class ContentSourceSidebarControlTests
    {
        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            if (OpenLiveWriter.CoreServices.ApplicationEnvironment.InstallationDirectory == null)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                OpenLiveWriter.CoreServices.ApplicationEnvironment.Initialize(assembly,
                    System.IO.Path.GetDirectoryName(assembly.Location),
                    "Software\\OpenLiveWriter.Tests", "Open Live Writer Tests");
            }
        }

        [Test]
        public void VideoContentSource_HasNoEditor()
        {
            // The premise of the caching bug: this source returns null forever.
            Assert.IsNull(new VideoContentSource().CreateEditor(null));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void GetSmartContentEditor_ForEditorlessSource_SurvivesRepeatedCalls()
        {
            var context = new StubContentSourceSidebarContext(typeof(VideoContentSource));
            using (var control = new ContentSourceSidebarControl(new StubSidebarContext(), context))
            using (var failures = new AssertionRecorder())
            {
                Assert.IsNull(control.GetSmartContentEditor(VideoContentSource.ID));

                // Before the fix, the first call cached null, so this one hit
                // Debug.Assert(!_contentSourceControls.Contains(...)).
                Assert.IsNull(control.GetSmartContentEditor(VideoContentSource.ID));
                Assert.IsNull(control.GetSmartContentEditor(VideoContentSource.ID));

                Assert.IsEmpty(failures.Messages,
                    "Repeated inserts from an editor-less content source must not assert");
            }
        }

        /// <summary>
        /// Captures Debug.Assert/Trace.Fail messages instead of letting the
        /// default listener abort the test host, and restores the listeners on
        /// dispose.
        /// </summary>
        private sealed class AssertionRecorder : TraceListener, IDisposable
        {
            private readonly TraceListener[] _saved;

            public AssertionRecorder()
            {
                _saved = new TraceListener[Trace.Listeners.Count];
                Trace.Listeners.CopyTo(_saved, 0);
                Trace.Listeners.Clear();
                Trace.Listeners.Add(this);
            }

            public List<string> Messages { get; } = new List<string>();

            public override void Fail(string message) => Messages.Add(message);

            public override void Fail(string message, string detailMessage) =>
                Messages.Add(message + " " + detailMessage);

            public override void Write(string message) { }

            public override void WriteLine(string message) { }

            void IDisposable.Dispose()
            {
                Trace.Listeners.Clear();
                Trace.Listeners.AddRange(_saved);
            }
        }

        private sealed class StubSidebarContext : ISidebarContext
        {
            public IWin32Window Owner => null;

            public CommandManager CommandManager { get; } = new CommandManager();

            public void UpdateStatusBar(string statusText) { }

            public void UpdateStatusBar(Image image, string statusText) { }

            public IUndoUnit CreateUndoUnit() => null;
        }

        private sealed class StubContentSourceSidebarContext : IContentSourceSidebarContext
        {
            private readonly ContentSourceInfo _contentSource;

            public StubContentSourceSidebarContext(Type contentSourceType)
            {
                _contentSource = new ContentSourceInfo(contentSourceType, false);
            }

#pragma warning disable 67 // the control subscribes; nothing in these tests raises it
            public event ContentResizedEventHandler ContentResized;
#pragma warning restore 67

            public ContentSourceInfo FindContentSource(string contentSourceId) =>
                _contentSource.Id == contentSourceId ? _contentSource : null;

            public ISmartContent FindSmartContent(string contentId) => null;

            public ISmartContent CloneSmartContent(string contentId, string newContentId) => null;

            public void RemoveSmartContent(string contentId) { }

            public void DeleteSmartContent(string contentId) { }

            public IExtensionData FindExtentsionData(string contentId) => null;

            public void SelectSmartContent(string contentId) { }

            public void OnSmartContentEdited(string contentId) { }

            public string AccountId => "test-account";

            public string ServiceName => "Test Service";

            public SupportsFeature SupportsImageUpload => SupportsFeature.Unknown;

            public SupportsFeature SupportsScripts => SupportsFeature.Unknown;

            public SupportsFeature SupportsEmbeds => SupportsFeature.Unknown;

            public string BlogName => "Test Blog";

            public string HomepageUrl => "https://example.com/";

            public Color? BodyBackgroundColor => null;

            public IPostInfo PostInfo => null;
        }
    }
}
