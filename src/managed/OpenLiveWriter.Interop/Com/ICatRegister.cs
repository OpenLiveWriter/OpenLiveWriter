// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// System implemented standard component categories manager
    /// (CLSID_StdComponentCategoriesMgr)
    /// </summary>
    /// <example>
    /// [ComRegisterFunction()]
    /// public static void RegisterServer(Type t)
    /// {
    ///		ICatRegister cr = (ICatRegister) new StdComponentCategoriesMgr();
    ///		Guid clsidThis = new Guid( CLSID_MyComponent );
    ///		Guid catid = new Guid( CATID_TheCategory );
    ///		cr.RegisterClassImplCategories(
    ///			ref clsidThis, 1, new Guid[] { catid } );
    ///	}
    /// </example>
    [ComImport]
    [Guid("0002E005-0000-0000-C000-000000000046")]
    class StdComponentCategoriesMgr { }

    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [Guid("0002E012-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICatRegister
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="cCategories"></param>
        /// <param name="rgCategoryInfo"></param>
        void RegisterCategories(
            int cCategories,
            IntPtr rgCategoryInfo);

        /// <summary>
        ///
        /// </summary>
        /// <param name="cCategories"></param>
        /// <param name="rgcatid"></param>
        void UnRegisterCategories(
            int cCategories,
            IntPtr rgcatid);

        /// <summary>
        ///
        /// </summary>
        /// <param name="rclsid"></param>
        /// <param name="cCategories"></param>
        /// <param name="rgcatid"></param>
        void RegisterClassImplCategories(
            [In] ref Guid rclsid,
            int cCategories,
            [In, MarshalAs(UnmanagedType.LPArray)] Guid[] rgcatid);

        /// <summary>
        ///
        /// </summary>
        /// <param name="rclsid"></param>
        /// <param name="cCategories"></param>
        /// <param name="rgcatid"></param>
        void UnRegisterClassImplCategories(
            [In] ref Guid rclsid,
            int cCategories,
            [In, MarshalAs(UnmanagedType.LPArray)] Guid[] rgcatid);

        /// <summary>
        ///
        /// </summary>
        /// <param name="rclsid"></param>
        /// <param name="cCategories"></param>
        /// <param name="rgcatid"></param>
        void RegisterClassReqCategories(
            [In] ref Guid rclsid,
            int cCategories,
            [In, MarshalAs(UnmanagedType.LPArray)] Guid[] rgcatid);

        /// <summary>
        ///
        /// </summary>
        /// <param name="rclsid"></param>
        /// <param name="cCategories"></param>
        /// <param name="rgcatid"></param>
        void UnRegisterClassReqCategories(
            [In] ref Guid rclsid,
            int cCategories,
            [In, MarshalAs(UnmanagedType.LPArray)] Guid[] rgcatid);
    }

}
