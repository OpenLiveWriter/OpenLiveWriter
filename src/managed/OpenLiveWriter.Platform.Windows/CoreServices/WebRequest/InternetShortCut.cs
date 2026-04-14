// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.SHDocVw;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// InternetShortcut is a class for generating and manipulating internet shortcut files
    /// </summary>
    public class InternetShortcut
    {
        /// <summary>
        /// Constructs a new instance of internetshortcut
        /// </summary>
        public InternetShortcut()
        {
            // Note that we tried marking this class with the Comimport and Guid attributes,
            // but when we did that, the type couldn't be loaded.  The below works around that.
            Type isType = Type.GetTypeFromCLSID(new Guid("fbf23b40-e3f0-101b-8488-00aa003e56f8")) ;
            internetShortcut = Activator.CreateInstance(isType) ;
        }

        /// <summary>
        /// Writes a url file representing the current navigation state of a browser
        /// </summary>
        /// <param name="browser">The IWebBrowser2 for which to write the url file</param>
        /// <param name="filePath">The path to the url file</param>
        public void WriteForBrowser( IWebBrowser2 browser, string filePath )
        {
            IObjectWithSite site = internetShortcut as IObjectWithSite ;
            site.SetSite( browser ) ;
            IPersistFile file = internetShortcut as IPersistFile ;
            file.Save( filePath, false ) ;
        }

        /// <summary>
        /// holds the internetshortcut object (COM)
        /// </summary>
        private object internetShortcut ;
    }
}
