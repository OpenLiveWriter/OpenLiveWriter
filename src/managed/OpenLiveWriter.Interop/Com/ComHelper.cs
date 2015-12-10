// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Helper functions for using and imlementing COM interfaces
    /// </summary>
    public class ComHelper
    {
        /// <summary>
        /// Return the specified HRESULT to the caller (common HRESULT values are
        /// defined in the ComTypes.HRESULT structure). The only way to do this is
        /// by throwing an exception (see Nathan pg. 656 for rationale / code upon
        /// which this implementation is based). Note that we could also accomplish
        /// this by using [PreserveSig] in the interface definition but this would
        /// make the interface definition specific to the implementation.
        /// </summary>
        public static void Return(int hResult)
        {
            throw new COMException(String.Empty, hResult);
        }

        public static bool SUCCEEDED(int hResult)
        {
            return hResult >= 0;
        }

        public static bool FAILED(int hResult)
        {
            return hResult < 0;
        }

        public static void Chk(int hResult)
        {
            if (FAILED(hResult))
                Marshal.ThrowExceptionForHR(hResult);
        }

        /// <summary>
        /// Query the specified service (which the passed source object knows about)
        /// for the requested COM interface. This implementation assumes that the
        /// source implements IServiceProvider, that the source knows about the
        /// specified service, and that the service implements the requested
        /// interface. If any of these conditions is not true the method will return
        /// null and will Assert in Debug mode. This method should be used in cases
        /// where the caller fully expects and assumes that it will work correctly
        /// under all known scenarios.
        /// </summary>
        /// <param name="source">source object</param>
        /// <param name="serviceID">SID of service to query</param>
        /// <param name="desiredIID">requested interface</param>
        /// <returns>reference to requested interface, null on failure</returns>
        public static object QueryService(object source, Guid serviceID, Guid requestedIID)
        {
            // cast the source object to a service-provider
            IServiceProvider sp =
                source as IServiceProvider;

            // if the source object isn't a service provider return null
            if (sp == null)
            {
                return null;
            }

            // query for the requested interface
            object obj = null;
            try
            {
                sp.QueryService(ref serviceID, ref requestedIID, out obj);
            }
            catch (COMException e)
            {
                Debug.Assert(false, "QueryService failure: " + e.Message);
            }

            // return interface (returns null if interface not found)
            return obj;
        }

        /// <summary>
        /// Register the class as an implementor of the specified category
        /// </summary>
        /// <param name="guid">class guid</param>
        /// <param name="categoryIID">category iid</param>
        public static void RegisterImplementedCategory(
            Guid clsGuid, Guid categoryIID)
        {
            // constants used for registration
            const string CLSID = "CLSID";
            const string REG_FMT = "B";
            const string IMPLEMENTED_CATEGORIES = "Implemented Categories";

            // create a sub-key for listing the categories we implement
            RegistryKey rkCategories = Registry.ClassesRoot.CreateSubKey(
                CLSID + @"\" +
                clsGuid.ToString(REG_FMT) + @"\" +
                IMPLEMENTED_CATEGORIES);

            //  write the implemented category
            using (rkCategories)
                rkCategories.CreateSubKey(categoryIID.ToString("B"));

            /*
            // "New school" COM-based method for registering as a category
            // implementor. Why don't we use this? First, it is incompatible
            // with Win95 and NT prior to SP3. Second, when trying to use it
            // to unregister a category we got a mysterious FileNotFound exception
            // that the ICatManager documentattion implies should never happen.
            // This was enough to scare us off of the bus.....
            //
            ICatRegister cr = (ICatRegister) new StdComponentCategoriesMgr();
            cr.RegisterClassImplCategories(
                ref guid, 1, new Guid[] { categoryIID } );
            */

        }
    }

}
