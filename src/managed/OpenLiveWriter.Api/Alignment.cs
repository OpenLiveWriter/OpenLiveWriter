// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    /// <summary>
    /// SmartContent object alignment.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// No alignment (display object inline with text).
        /// </summary>
        None,

        /// <summary>
        /// Left alignment (text wraps around the right side of the object).
        /// </summary>
        Left,

        /// <summary>
        /// Right alignment (text wraps around the left side of the object)
        /// </summary>
        Right,

        /// <summary>
        /// Places the smart content in the middle of the block, sometimes splitting the
        /// current block to accomplish the task.
        /// </summary>
        Center
    }
}