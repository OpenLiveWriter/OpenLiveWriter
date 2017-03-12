// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.ApplicationFramework
{

    public interface IMainFrameWindow : IWin32Window, ISynchronizeInvoke, IMiniFormOwner
    {
        string Caption { set; }

        Point Location { get; }
        Size Size { get; }

        event EventHandler LocationChanged;

        event EventHandler SizeChanged;

        event EventHandler Deactivate;

        event LayoutEventHandler Layout;

        void Activate();

        void Update();

        void PerformLayout();
        void Invalidate();

        void Close();

        void OnKeyboardLanguageChanged();
    }

    public interface IStatusBar
    {
        void SetWordCountMessage(string msg);
        void PushStatusMessage(string msg);
        void PopStatusMessage();
        void SetStatusMessage(string msg);
    }

    public class NullStatusBar : IStatusBar
    {
        private int msgCount;

        public void SetWordCountMessage(string msg)
        {
        }

        public void PushStatusMessage(string msg)
        {
            msgCount++;
        }

        public void PopStatusMessage()
        {
            msgCount--;
            Debug.Assert(msgCount >= 0);
        }

        public void SetStatusMessage(string msg)
        {
        }
    }

    public class StatusMessage
    {
        public StatusMessage(string blogPostStatus)
            : this(null, blogPostStatus, null)
        {
        }

        public StatusMessage(Image icon, string blogPostStatus, string wordCountValue)
        {
            Icon = icon;
            BlogPostStatus = blogPostStatus;
            WordCountValue = wordCountValue;
        }

        public void ConsumeValues(StatusMessage externalMessage)
        {
            if (BlogPostStatus == null)
            {
                BlogPostStatus = externalMessage.BlogPostStatus;
                Icon = externalMessage.Icon;
            }

            if (WordCountValue == null)
                WordCountValue = externalMessage.WordCountValue;
        }

        private Image _icon;
        public Image Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
            }
        }

        private string _blogPostStatus;
        public string BlogPostStatus
        {
            get
            {
                return _blogPostStatus;
            }
            set
            {
                _blogPostStatus = value;
            }
        }

        private string _wordCountValue;
        public string WordCountValue
        {
            get
            {
                return _wordCountValue;
            }
            set
            {
                _wordCountValue = value;
            }
        }
    }

    public class DesignModeMainFrameWindow : SameThreadSimpleInvokeTarget, IMainFrameWindow
    {

        public string Caption
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }

        public Point Location { get { return Point.Empty; } }
        public Size Size { get { return Size.Empty; } }

        public void Activate()
        {
        }

        public void Update()
        {
        }

        public void AddOwnedForm(Form form)
        {
        }

        public void RemoveOwnedForm(Form form)
        {
        }

        public void SetStatusBarMessage(StatusMessage message)
        {
        }

        public void PushStatusBarMessage(StatusMessage message)
        {
        }

        public void PopStatusBarMessage()
        {
        }

        public void PerformLayout()
        {
        }

        public void Invalidate()
        {

        }

        public void Close()
        {
        }

        public IntPtr Handle
        {
            get
            {
                return User32.GetForegroundWindow();
            }
        }

        public event EventHandler SizeChanged;
        protected void OnSizeChanged()
        {
            // prevent compiler warnings
            if (SizeChanged != null)
                SizeChanged(this, EventArgs.Empty);
        }

        public event EventHandler LocationChanged;
        protected void OnLocationChanged()
        {
            // prevent compiler warnings
            if (LocationChanged != null)
                LocationChanged(this, EventArgs.Empty);
        }

        public event EventHandler Deactivate;
        protected void OnDeactivate()
        {
            // prevent compiler warnings
            if (Deactivate != null)
                Deactivate(this, EventArgs.Empty);
        }

        public event LayoutEventHandler Layout;
        protected void OnLayout(LayoutEventArgs ea)
        {
            if (Layout != null)
                Layout(this, ea);
        }

        public void OnKeyboardLanguageChanged()
        {
        }
    }
}


