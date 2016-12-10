// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    /// <summary>
    ///  Layout options for SmartContent object.
    /// </summary>
    public interface ILayoutStyle
    {
        /// <summary>
        /// Gets or sets the alignment of object relative to text.
        /// </summary>
        Alignment Alignment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the margin (in pixels) above the object.
        /// </summary>
        int TopMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the margin (in pixels) to the right of object.
        /// </summary>
        int RightMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the margin (in pixels) below the object.
        /// </summary>
        int BottomMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the margin (in pixels) to the left of the object.
        /// </summary>
        int LeftMargin
        {
            get;
            set;
        }
    }
}
