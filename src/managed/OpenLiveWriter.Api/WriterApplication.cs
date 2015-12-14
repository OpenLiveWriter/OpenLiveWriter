// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Provides the ability to launch the Writer application either to create a new post, open an existing post, or Blog This
    /// for a Link, Snippet, Image, or Feed Item.
    /// </summary>
    public sealed class WriterApplication
    {
        /// <summary>
        /// Is Open Live Writer currently installed.
        /// </summary>
        public static bool IsInstalled
        {
            get
            {
                string writerApplicationClsidKey = String.Format(CultureInfo.InvariantCulture, "CLSID\\{0}", typeof(OpenLiveWriterApplicationClass).GUID.ToString("B"));
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(writerApplicationClsidKey))
                    return key != null;
            }
        }

        /// <summary>
        /// Create a new instance of the WriterApplication class.
        /// </summary>
        public WriterApplication()
        {
            OpenLiveWriterApplicationClass applicationObject = new OpenLiveWriterApplicationClass();
            _application = (IOpenLiveWriterApplication)applicationObject;
        }
        private IOpenLiveWriterApplication _application;

        /// <summary>
        /// Launch Open Live Writer for editing a new post.
        /// </summary>
        public void NewPost()
        {
            _application.NewPost();
        }

        /// <summary>
        /// Launch Open Live Writer for opening an existing post (shows the Open Post dialog).
        /// </summary>
        public void OpenPost()
        {
            _application.OpenPost();
        }

        /// <summary>
        /// Show the Open Live Writer Preferences dialog.
        /// </summary>
        /// <param name="optionsPage">Preferences page to pre-select (valid values include "writer", and "webproxy").
        /// Defaults to "writer" if null or an empty string is specified.</param>
        public void ShowOptions(string optionsPage)
        {
            _application.ShowOptions(optionsPage);
        }

        [ComImport]
        [Guid("366FF6CE-CA04-433D-8522-741094458839")]
        private class OpenLiveWriterApplicationClass { }  // implements IOpenLiveWriterApplication

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
        [Guid("D7E5B1EC-FEEA-476C-9CE1-BC5C3E2DB662")]
        private interface IOpenLiveWriterApplication
        {
            void NewPost();
            void OpenPost();
            void ShowOptions(string optionsPage);

        }
    }
}
