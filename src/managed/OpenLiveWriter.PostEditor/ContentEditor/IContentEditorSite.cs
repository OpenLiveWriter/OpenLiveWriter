// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// IContentEditorSite should be implemented by the window
    /// that is hosting a ContentEditor.
    /// IMPORTANT: IContentEditorSite must also implement IUIFramework: http://msdn.microsoft.com/en-us/library/dd371467(VS.85).aspx
    /// IMPORTANT: IContentEditorSite must also implement IDropTarget, so if something is dragged into the
    ///            editor that it cannot handle, it cna pass it to the the hosting window
    /// </summary>
    [Guid("086D5E46-28DF-48bf-8973-3E66CB1C5C13")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IContentEditorSite : IUIFramework, IDropTarget, IETWProvider
    {
        /// <summary>
        /// Gets the handle for the window that will be
        /// the parent of the ContentEditor
        /// </summary>
        /// <returns></returns>
        IntPtr GetWindowHandle();

        /// <summary>
        /// Adds a message to the status bar of the window.
        /// </summary>
        /// <param name="message">
        /// This is a message the user should be shown
        /// </param>
        /// <param name="message">
        /// This is the number of words in the post if real
        /// time word count is turned on
        /// </param>
        void SetStatusBarMessage(string message, string wordCountValue);

        /// <summary>
        /// This is called back once the document that has been
        /// loaded into the editor has finished loading.
        /// THis would be a good time for the window to start loading
        /// anything it has delayed.
        /// </summary>
        void OnDocumentComplete();

        // Focus event callbacks
        void OnGotFocus();
        void OnLostFocus();

        /// <summary>
        /// Callback that occurs when the keyboard's language changes.
        /// </summary>
        void OnKeyboardLanguageChanged();

        /// <summary>
        /// Gets the subject of the email that the user has entered.
        /// </summary>
        string GetDocumentTitle();
    }

    [Guid("CF7F4F35-571B-4905-985E-3FC0E684AE2C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IETWProvider
    {
        void WriteEvent(string eventName);
    }
}
