// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;

namespace OpenLiveWriter.CoreServices.HTML
{

    public sealed class HTMLSelectionHelper
    {
        public static bool SelectionIsImage(IHTMLSelectionObject selection)
        {
            // see if we have selected an image
            if (selection != null && selection.type == "Control")
            {
                object range = selection.createRange();
                if (range is IHTMLControlRange)
                {
                    IHTMLControlRange controlRange = (IHTMLControlRange)range;
                    if (controlRange.length > 0)
                    {
                        IHTMLElement selectedElement = controlRange.item(0);
                        if (selectedElement is IHTMLImgElement)
                            return true;
                    }
                }
            }

            // got this far, not an image!
            return false;
        }
    }
}
