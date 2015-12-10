// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public delegate bool DataObjectFilter(DataObjectMeister data);
    public delegate DataFormatHandler DataFormatHandlerCreate(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext);

    /// <summary>
    /// Manages a list of factories for creating DataFormatHandlers.
    /// </summary>
    public class DataFormatHandlerRegistry : IDisposable, IDataFormatHandlerFactory
    {
        public DataFormatHandlerRegistry()
        {
        }

        /// <summary>
        /// Check whether we can create a handler from the passed context/meister
        /// </summary>
        /// <param name="dataMeister">data object meister</param>
        /// <returns>true if can create from, otherwise false</returns>
        public bool CanCreateFrom(DataObjectMeister dataMeister)
        {
            // cycle through our registered handlers and see if anyone can create from the data object
            foreach (IDataFormatHandlerFactory handlerFactory in GetDataFormatHandlers())
            {
                try
                {
                    // check if the handler can convert/handle the data
                    if (handlerFactory.CanCreateFrom(dataMeister))
                        return true;
                }
                catch (Exception e)
                {
                    Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected error querying data format handler factory {0}: {1}", handlerFactory.GetType().Name, e.Message));
                }
            }

            // no-one could create from this data object, return false
            return false;
        }

        /// <summary>
        /// Get a snapshot of the data format handlers (do this to hold the lock
        /// for as small an increment of time as possible)
        /// </summary>
        /// <returns></returns>
        private IList GetDataFormatHandlers()
        {
            lock (dataFormatHandlerFactories)
            {
                return new ArrayList(dataFormatHandlerFactories);
            }
        }

        public void Clear()
        {
            lock (dataFormatHandlerFactories)
            {
                dataFormatHandlerFactories.Clear();
            }
        }

        public void Register(DataFormatHandlerCreate create, DataObjectFilter filter)
        {
            // register the data format handler
            lock (dataFormatHandlerFactories)
            {
                dataFormatHandlerFactories.Add(new DelegateBasedDataFormatHandlerFactory(create, filter));
            }
        }

        /*public void Register( Type dataFormatHandlerType, DataObjectFilter filter )
        {
            // register the data format handler
            lock(dataFormatHandlerFactories)
            {
                dataFormatHandlerFactories.Add(new DataObjectFilterFormatFactory(filter, dataFormatHandlerType));
            }
        }*/

        public void Register(params IDataFormatHandlerFactory[] dataFormatFactories)
        {
            // register the data format handler
            lock (dataFormatHandlerFactories)
            {
                foreach (IDataFormatHandlerFactory dataFormatFactory in dataFormatFactories)
                    dataFormatHandlerFactories.Add(dataFormatFactory);
            }
        }

        /// <summary>
        /// Create a data format handler
        /// </summary>
        /// <param name="dataMeister">data to handle</param>
        /// <returns>new data format handler (or null if none could be created)</returns>
        public virtual DataFormatHandler CreateFrom(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            // cycle through our registered handlers and see who wants it
            foreach (IDataFormatHandlerFactory handlerFactory in GetDataFormatHandlers())
            {
                // check if the handler can convert/handle the data
                if (handlerFactory.CanCreateFrom(dataMeister))
                {
                    // create a data format handler and return it
                    DataFormatHandler dataFormatHandler = handlerFactory.CreateFrom(dataMeister, handlerContext);
                    return dataFormatHandler;
                }
            }

            // nobody wanted it!
            return null;
        }

        /// <summary>
        /// Registered data format handler factories.
        /// </summary>
        private IList dataFormatHandlerFactories = new ArrayList();

        private class DataObjectFilterFormatFactory : IDataFormatHandlerFactory
        {
            private DataObjectFilter _filter;
            private Type _dataFormatHandlerType;
            public DataObjectFilterFormatFactory(DataObjectFilter filter, Type dataFormatHandlerType)
            {
                // verify that the type is correct
                if (!typeof(DataFormatHandler).IsAssignableFrom(dataFormatHandlerType))
                    throw new ArgumentException(
                        "Registered type is not a subclass of DataFormatHandler!");
                _filter = filter;
                _dataFormatHandlerType = dataFormatHandlerType;
            }
            public bool CanCreateFrom(DataObjectMeister data)
            {
                return _filter(data);
            }

            public DataFormatHandler CreateFrom(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
            {
                DataFormatHandler dataFormatHandler;
                dataFormatHandler = Activator.CreateInstance(_dataFormatHandlerType) as DataFormatHandler;
                return dataFormatHandler;
            }

            public void Dispose()
            {
                _filter = null;
                _dataFormatHandlerType = null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (dataFormatHandlerFactories != null)
            {
                lock (dataFormatHandlerFactories)
                {
                    foreach (IDataFormatHandlerFactory factory in dataFormatHandlerFactories)
                        factory.Dispose();
                    dataFormatHandlerFactories.Clear();
                    dataFormatHandlerFactories = null;
                }
            }
        }

        #endregion

    }
}
