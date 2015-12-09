// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    /// <summary>
    /// Implements a DataFormatHandlerFactory using a set of delegates.
    /// </summary>
    public class DelegateBasedDataFormatHandlerFactory : IDataFormatHandlerFactory
    {
        private DataObjectFilter _filter;
        DataFormatHandlerCreate _create;
        public DelegateBasedDataFormatHandlerFactory(DataFormatHandlerCreate create, DataObjectFilter filter)
        {
            _filter = filter;
            _create = create;
        }

        public bool CanCreateFrom(DataObjectMeister data)
        {
            return _create != null && _filter != null && _filter(data);
        }

        public DataFormatHandler CreateFrom(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return _create(dataMeister, handlerContext);
        }

        public void Dispose()
        {
            _filter = null;
            _create = null;
        }
    }
}
