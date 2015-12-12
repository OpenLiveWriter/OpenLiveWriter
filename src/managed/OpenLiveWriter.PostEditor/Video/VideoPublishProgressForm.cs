// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Video
{
    enum SmartContentOperationType
    {
        Publish,
        Close
    }

    internal partial class VideoPublishProgressForm : DelayedAnimatedProgressDialog
    {

        internal VideoPublishProgressForm()
        {
            InitializeComponent();
            labelRetrievingPost.AutoSize = true;
            labelRetrievingPost.Text = Res.Get(StringId.Plugin_Video_Publish_Message);
            Text = Res.Get(StringId.Plugin_Video_Publish_Message);
            buttonCancelForm.Text = Res.Get(StringId.CancelButton);
            progressAnimatedBitmap.Bitmaps = AnimationBitmaps;
            progressAnimatedBitmap.Interval = 2000 / AnimationBitmaps.Length;
            SetAnimatatedBitmapControl(progressAnimatedBitmap);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DisplayHelper.AutoFitSystemButton(buttonCancelForm, buttonCancelForm.Width, int.MaxValue);
        }

        private void buttonCancelForm_Click(object sender, System.EventArgs e)
        {
            Cancel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            CancelWithoutClose();
            base.OnClosing(e);
        }

        private Bitmap[] AnimationBitmaps
        {
            get
            {
                if (_animationBitmaps == null)
                {
                    ArrayList list = new ArrayList();
                    for (int i = 0; i < 12; i++)
                    {
                        string resourceName = String.Format(CultureInfo.InvariantCulture, "OpenPost.Images.GetRecentPostsAnimation.GetRecentPostsAnimation{0:00}.png", i);
                        list.Add(ResourceHelper.LoadAssemblyResourceBitmap(resourceName));
                    }
                    _animationBitmaps = (Bitmap[])list.ToArray(typeof(Bitmap));
                }
                return _animationBitmaps;
            }
        }
        private Bitmap[] _animationBitmaps;
    }

    internal class VideoSmartContentWaitOperation : CoreServices.AsyncOperation
    {
        private readonly VideoPublishStatus validState;
        private readonly VideoPublishStatus invalidState;

        public VideoSmartContentWaitOperation(ISynchronizeInvoke context, SmartContentOperationType checkType, IExtensionData[] extendionDataList)
            : base(context)
        {
            validState = VideoPublishStatus.Completed;
            invalidState = VideoPublishStatus.Error;

            if (checkType == SmartContentOperationType.Close)
                validState |= VideoPublishStatus.RemoteProcessing;

            _extensionDataList = extendionDataList;
        }

        protected override void DoWork()
        {
            while (true)
            {
                if (CancelRequested)
                {
                    return;
                }
                if (CheckVideos())
                {
                    return;
                }
                Thread.Sleep(2000);
            }
        }

        public bool CheckVideos()
        {
            for (int i = 0; i < _extensionDataList.Length; i++)
            {
                if (_extensionDataList[i].ObjectState is IStatusWatcher)
                {
                    PublishStatus status = ((IStatusWatcher)_extensionDataList[i].ObjectState).Status;
                    if ((status.Status & validState) != 0)
                    {
                        _extensionDataList[i] = null;
                    }
                    else if ((status.Status & invalidState) != 0)
                    {
                        _isSuccessful = false;
                        return true;
                    }
                }
                else
                {
                    _extensionDataList[i] = null;
                }
            }

            _extensionDataList = (IExtensionData[])ArrayHelper.Compact(_extensionDataList);

            if (_extensionDataList.Length == 0)
            {
                _isSuccessful = true;
                return true;
            }

            return false;

        }

        private volatile bool _isSuccessful;
        public bool IsSuccessful
        {
            get
            {
                return _isSuccessful;
            }
        }

        public bool DialogCancelled
        {
            get
            {
                return base.CancelRequested;
            }
        }

        private IExtensionData[] _extensionDataList;

    }
}
