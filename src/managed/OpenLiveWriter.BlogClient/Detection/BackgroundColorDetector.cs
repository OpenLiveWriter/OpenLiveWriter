// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.BrowserControl;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.BlogClient.Detection
{
    class BackgroundColorDetector
    {
        /// <summary>
        /// Detect the background color of a post body from a URI where
        /// the post body element contains BlogEditingTemplate.POST_BODY_MARKER.
        /// This must be done in a browser (as opposed to a simple DOM) because
        /// the page elements don't layout relative to each other unless they
        /// are being displayed in a browser.
        /// </summary>
        public static Color? DetectColor(string uri, Color? defaultColor)
        {
            return BrowserOperationInvoker.InvokeAfterDocumentComplete(uri, "BackgroundColorDetector", 700, 700, defaultColor,
                delegate (ExplorerBrowserControl browser)
                {
                    IHTMLDocument2 document = browser.Document as IHTMLDocument2;
                    IHTMLElement[] elements = HTMLDocumentHelper.FindElementsContainingText(document, BlogEditingTemplate.POST_BODY_MARKER);
                    if (elements.Length == 1)
                    {
                        IHTMLElement postBody = elements[0];
                        if (postBody.offsetHeight < 300)
                            postBody.style.height = 300;
                        return HTMLColorHelper.GetBackgroundColor(postBody, true, null, Color.White);
                    }
                    return defaultColor;
                });
        }
    }

}
