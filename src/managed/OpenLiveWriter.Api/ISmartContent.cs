// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using JetBrains.Annotations;

    /// <summary>
    /// Interface used to manipulate the state, layout, and supporting files of SmartContent objects.
    /// </summary>
    public interface ISmartContent
    {
        /// <summary>
        /// Gets the property-set that represents the state of a SmartContent object.
        /// </summary>
        [CanBeNull]
        IProperties Properties
        {
            get;
        }

        /// <summary>
        /// Gets the supporting-files used by the SmartContent object.
        /// </summary>
        [CanBeNull]
        ISupportingFiles Files
        {
            get;
        }

        /// <summary>
        /// Gets the layout options for SmartContent object.
        /// </summary>
        [CanBeNull]
        ILayoutStyle Layout
        {
            get;
        }
    }
}

