// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.JumpList
{
    /// <summary>
    /// Known category to display
    /// </summary>
    public enum JumpListKnownCategoryType
    {
        /// <summary>
        /// Don't display either known category. You must have at least one
        /// user task or custom category link in order to not see the
        /// default 'Recent' known category
        /// </summary>
        Neither = 0,

        /// <summary>
        /// Display the 'Recent' known category
        /// </summary>
        Recent,

        /// <summary>
        /// Display the 'Frequent' known category
        /// </summary>
        Frequent,
    }
}
