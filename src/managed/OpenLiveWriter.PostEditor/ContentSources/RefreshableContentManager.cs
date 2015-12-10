// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    internal class RefreshableContentManager : IDisposable
    {
        private BlogPostExtensionDataList _extensionDataList;
        private OpenLiveWriter.PostEditor.ContentEditor _editor;
        private Timer _refreshableContentTimer;
        public const int INTERVAL_TICK = 3000;
        private readonly List<IExtensionData> _missedCallbackExtensionData;

        //
        // IMPORTANT: Make sure that you add the content source id of any source that has content with callbacks, otherwise
        //            there might be problems when copy/cut/PasteSpecialForm where the content will no longer get call backs
        //
        internal readonly static List<string> ContentSourcesWithRefreshableContent = new List<string>(new string[] { Video.VideoContentSource.ID });

        internal RefreshableContentManager(BlogPostExtensionDataList extensionDataList, OpenLiveWriter.PostEditor.ContentEditor editor)
        {
            _missedCallbackExtensionData = new List<IExtensionData>();
            _extensionDataList = extensionDataList;
            _editor = editor;

            _editor.UndoExecuted += new EventHandler(_editorContentsChanged);
            _editor.RedoExecuted += new EventHandler(_editorContentsChanged);
            _editor.PasteExecuted += new EventHandler(_editorContentsChanged);
            _editor.PasteSpecialExecuted += new EventHandler(_editorContentsChanged);

            _extensionDataList.RefreshableCallbackTriggered += _extensionDataList_RefreshableCallbackTriggered;

            _refreshableContentTimer = new Timer();
            _refreshableContentTimer.Interval = INTERVAL_TICK;
            _refreshableContentTimer.Tick += refreshableContentTimer_Tick;
        }

        void _editorContentsChanged(object sender, EventArgs e)
        {
            if (_missedCallbackExtensionData.Count == 0) return;
            IExtensionData[] extensionData = _missedCallbackExtensionData.ToArray();
            _missedCallbackExtensionData.Clear();
            _missedCallbackExtensionData.AddRange(((IContentSourceSite)_editor).UpdateContent(extensionData));
        }

        void _extensionDataList_RefreshableCallbackTriggered(object sender, EventArgs e)
        {
            _refreshableContentTimer.Stop();

            IExtensionData extensionData = _extensionDataList.GetExtensionDataWithMinCallback();

            if (extensionData == null)
            {
                return;
            }

            Int32 refreshMillisec =
                Convert.ToInt32(extensionData.RefreshCallBack.Value.Subtract(DateTimeHelper.UtcNow).TotalMilliseconds);
            _refreshableContentTimer.Interval = Math.Max(refreshMillisec, INTERVAL_TICK);
            _refreshableContentTimer.Start();
        }

        internal void refreshableContentTimer_Tick(object sender, EventArgs e)
        {
            if (_extensionDataList.Count == 0)
            {
                _refreshableContentTimer.Stop();
                return;
            }

            // Find only the smart content that is still inside the html
            IExtensionData extensionData = _extensionDataList.GetExtensionDataWithMinCallback();

            if (extensionData == null)
            {
                _refreshableContentTimer.Stop();
                Trace.Fail("RefreshableContentManager tick without any extensionData with a callback.");
                return;
            }

            Debug.Assert(extensionData.RefreshCallBack != null, "GetExtensionDataWithMinCallback returned an extensionData without a callback time");

            extensionData.RefreshCallBack = null;

            IExtensionData[] missingExentinsionData = ((IContentSourceSite)_editor).UpdateContent(new IExtensionData[] { extensionData });

            if (missingExentinsionData.Length == 0) return;
            Debug.Assert(missingExentinsionData.Length == 1, "Only one IExtensionData was supposed to update, but more then 1 was returned as being missing.");
            Debug.Assert(!_missedCallbackExtensionData.Contains(missingExentinsionData[0]), "We should not have tried to update smart content that was not in the editor.");

            _missedCallbackExtensionData.Add(missingExentinsionData[0]);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _extensionDataList.RefreshableCallbackTriggered -= _extensionDataList_RefreshableCallbackTriggered;
            _editor.UndoExecuted -= _editorContentsChanged;
            _editor.RedoExecuted -= _editorContentsChanged;
            _editor.PasteExecuted -= _editorContentsChanged;
            _editor.PasteSpecialExecuted -= _editorContentsChanged;
            _extensionDataList = null;
            _editor = null;
            _missedCallbackExtensionData.Clear();
            _refreshableContentTimer.Dispose();
            _refreshableContentTimer = null;
        }

        #endregion
    }

}
