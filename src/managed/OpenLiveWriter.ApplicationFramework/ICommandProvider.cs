// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Interface for command providers.
    /// </summary>
    public interface ICommandProvider
    {
        /// <summary>
        /// Returns the command list for the command provider.
        /// </summary>
        CommandList CommandList
        {
            get;
        }
    }
}
