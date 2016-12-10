// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System.Runtime.InteropServices;

    using JetBrains.Annotations;

    using Microsoft.Win32;

    /// <summary>
    /// Provides the ability to launch the Writer application either to create a new post, open an existing post, or Blog This
    /// for a Link, Snippet, Image, or Feed Item.
    /// </summary>
    public sealed class WriterApplication
    {
        /// <summary>
        /// The application
        /// </summary>
        private readonly IOpenLiveWriterApplication application;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriterApplication" /> class.
        /// </summary>
        public WriterApplication()
        {
            var applicationObject = new OpenLiveWriterApplicationClass();
            this.application = (IOpenLiveWriterApplication)applicationObject;
        }

        /// <summary>
        /// Interface IOpenLiveWriterApplication
        /// </summary>
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
        [Guid("D7E5B1EC-FEEA-476C-9CE1-BC5C3E2DB662")]
        private interface IOpenLiveWriterApplication
        {
            /// <summary>
            /// News the post.
            /// </summary>
            void NewPost();

            /// <summary>
            /// Opens the post.
            /// </summary>
            void OpenPost();

            /// <summary>
            /// Shows the options.
            /// </summary>
            /// <param name="optionsPage">The options page.</param>
            void ShowOptions([CanBeNull] string optionsPage);
        }

        /// <summary>
        /// Gets a value indicating whether Open Live Writer is currently installed.
        /// </summary>
        /// <value><c>true</c> if this instance is installed; otherwise, <c>false</c>.</value>
        public static bool IsInstalled
        {
            get
            {
                var writerApplicationClsidKey = $"CLSID\\{typeof(OpenLiveWriterApplicationClass).GUID:B}";
                using (var key = Registry.ClassesRoot.OpenSubKey(writerApplicationClsidKey))
                {
                    return key != null;
                }
            }
        }

        /// <summary>
        /// Launch Open Live Writer for editing a new post.
        /// </summary>
        public void NewPost()
        {
            this.application.NewPost();
        }

        /// <summary>
        /// Launch Open Live Writer for opening an existing post (shows the Open Post dialog).
        /// </summary>
        public void OpenPost()
        {
            this.application.OpenPost();
        }

        /// <summary>
        /// Show the Open Live Writer Preferences dialog.
        /// </summary>
        /// <param name="optionsPage">Preferences page to pre-select (valid values include "writer", and "webproxy").
        /// Defaults to "writer" if null or an empty string is specified.</param>
        public void ShowOptions([CanBeNull] string optionsPage)
        {
            this.application.ShowOptions(optionsPage);
        }

        /// <summary>
        /// Class OpenLiveWriterApplicationClass.
        /// </summary>
        [ComImport]
        [Guid("366FF6CE-CA04-433D-8522-741094458839")]
        private class OpenLiveWriterApplicationClass // implements IOpenLiveWriterApplication
        {
        }
    }
}
