// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    /// <summary>
    /// Factory interface for managing the creation of DataFormatHandlers.
    /// </summary>
    public interface IDataFormatHandlerFactory : IDisposable
    {
        bool CanCreateFrom(DataObjectMeister data);
        DataFormatHandler CreateFrom(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext);
    }
}
