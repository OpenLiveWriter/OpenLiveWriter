// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.JumpList
{
    /// <summary>
    /// Represents an instance of the Windows taskbar
    /// </summary>
    public class TaskbarManager
    {
        // Hide the default constructor
        private TaskbarManager()
        {

        }

        // Best practice recommends defining a private object to lock on
        private static Object syncLock = new Object();

        private static volatile TaskbarManager instance;
        /// <summary>
        /// Represents an instance of the Windows Taskbar
        /// REQUIRES Win 7!
        /// </summary>
        public static TaskbarManager Instance
        {
            get
            {
                PlatformHelper.ThrowIfNotWin7OrHigher();

                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new TaskbarManager();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the application user model id. Use this to explicitly
        /// set the application id when generating custom jump lists
        /// </summary>
        public string ApplicationId
        {
            get
            {
                PlatformHelper.ThrowIfNotWin7OrHigher();

                return GetCurrentProcessAppId();
            }
            set
            {
                PlatformHelper.ThrowIfNotWin7OrHigher();

                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value", "Application Id cannot be an empty or null string.");
                else
                {
                    SetCurrentProcessAppId(value);
                    ApplicationIdSetProcessWide = true;
                }
            }
        }

        /// <summary>
        /// Sets the application user model id for an individual window
        /// </summary>
        /// <param name="appId">The app id to set</param>
        /// <param name="windowHandle">Window handle for the window that needs a specific application id</param>
        /// <remarks>AppId specifies a unique Application User Model ID (AppID) for the application or individual
        /// top-level window whose taskbar button will hold the custom JumpList built through the methods <see cref="OpenLiveWriter.PostEditor.JumpList"/> class.
        /// By setting an appId for a specific window, the window will not be grouped with it's parent window/application. Instead it will have it's own taskbar button.</remarks>
        public void SetApplicationIdForSpecificWindow(IntPtr windowHandle, string appId)
        {
            ShellHelper.SetWindowAppId(windowHandle, appId);
        }

        /// <summary>
        /// Sets the current process' explicit application user model id.
        /// </summary>
        /// <param name="appId">The application id.</param>
        private void SetCurrentProcessAppId(string appId)
        {
            Shell32.SetCurrentProcessExplicitAppUserModelID(appId);
        }

        /// <summary>
        /// Gets the current process' explicit application user model id.
        /// </summary>
        /// <returns>The app id or null if no app id has been defined.</returns>
        private string GetCurrentProcessAppId()
        {
            string appId = string.Empty;
            Shell32.GetCurrentProcessExplicitAppUserModelID(out appId);
            return appId;
        }

        /// <summary>
        /// Indicates if the user has set the application id for the whole process (all windows)
        /// </summary>
        internal bool ApplicationIdSetProcessWide
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether this feature is supported on the current platform.
        /// </summary>
        public static bool IsPlatformSupported
        {
            get
            {
                // We need Windows 7 onwards ...
                return PlatformHelper.RunningOnWin7OrHigher();
            }
        }

    }
}
