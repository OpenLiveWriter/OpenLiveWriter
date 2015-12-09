// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
    // This interface allows contact with the BlogPostHtmlEditor which
    // will make the editor as dirty when it ImageEditFinished is called.
    public interface IImageTargetEditor
    {
        void ImageEditFinished();
    }
}
