// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Resources;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Generic exception class for Writer -- implements the storage
    /// of exception messages (with optional arguments) in a resource file.
    /// </summary>
    public class ResourceFileException : ApplicationException
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="type">Exception type (drawn from a class level string constant)</param>
        /// <param name="innerException">Exception that preceded this one</param>
        /// <param name="arguments">Array of arguments for the exception message string</param>
        public ResourceFileException(Exception innerException, Type type, string exceptionType, params object[] arguments)
            : base(GetExceptionMessage(getResourceManager(type), type, exceptionType, arguments), innerException)
        {
        }

        /// <summary>
        /// This is a thread safe method that retrieves a resource manager for
        /// a given type.
        /// </summary>
        /// <param name="t">The type for which you'd like a resource manager</param>
        /// <returns>A ResourceManager for the type</returns>
        private static ResourceManager getResourceManager(Type t)
        {
            string assemblyName = t.Assembly.FullName;

            lock (typeof(ResourceFileException))
            {
                // This uses a hashtable to keep the resource managers that
                // it has already retrieved.
                if (!m_resourceManagers.ContainsKey(assemblyName))  // Check if its not in the hashtable
                {
                    // get a new resource manager for this type
                    ResourceManager resMgr =
                        new ResourceManager(t.Namespace + "." + EXCEPTION_FILENAME, t.Assembly);

                    // add it to the hashtable
                    m_resourceManagers.Add(assemblyName, resMgr);
                }

                // return the resource manager from the hash table
                return (ResourceManager)m_resourceManagers[assemblyName];
            }
        }

        // the hash table that will hold the resource managers by type
        private static Hashtable m_resourceManagers = new Hashtable();

        // standard filename for the exceptions resource file name
        private static string EXCEPTION_FILENAME = "Exceptions";

        /// <summary>
        /// format an exception message based on its type and optional arguments
        /// looks up the message type in the resource file and handles lookup
        /// errors with an assertion in Debug builds and silently in Release builds.
        /// </summary>
        /// <param name="type">exception type (this value must be in the resource file)</param>
        /// <param name="arguments">optional array of arguments</param>
        /// <returns>the formatted message</returns>
        private static string GetExceptionMessage(
            ResourceManager resourceManager,
            Type type,
            string exceptionType,
            object[] arguments)
        {

            // fetch the string from the resource file
            string message = resourceManager.GetString(type.Name + "." + exceptionType);

            // check for a non-existent message
            if (message == null)
            {
                Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, "Invalid exception type: {0}", type));
                return "";
            }

            // if we have arguments then we need to format the message
            if (arguments != null && arguments.GetLength(0) > 0)
            {
                // try to format the message -- if we pass an incorrect format
                // string or number of arguments then Assert if we are in debug
                // mode, otherwise handle the error gracefully by returning
                // the 'unformatted' message
                try
                {
                    return String.Format(CultureInfo.CurrentCulture, message, arguments);
                }
                catch
                {
                    Debug.Assert(false, "Invalid argument list passed to Format");
                    return message;
                }
            }

            // no arguments -- just return the message
            else
            {
                return message;
            }

        }
    }
}
