// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Interop.Com;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Provides easy access to all the system properties (property keys and their descriptions)
    /// </summary>
    public static class SystemProperties
    {
        /// <summary>
        /// System Properties
        /// </summary>
        public static class System
        {
            #region Properties

            /// <summary>
            /// <para>Name:     System.Title -- PKEY_Title</para>
            /// <para>Description: Title of item.
            ///</para>
            /// <para>Type:     String -- VT_LPWSTR  (For variants: VT_BSTR)  Legacy code may treat this as VT_LPSTR.</para>
            /// <para>FormatID: (FMTID_SummaryInformation) {F29F85E0-4FF9-1068-AB91-08002B27B3D9}, 2 (PIDSI_TITLE)</para>
            /// </summary>
            public static PropertyKey Title
            {
                get
                {
                    PropertyKey key = new PropertyKey(new Guid("{F29F85E0-4FF9-1068-AB91-08002B27B3D9}"), 2);

                    return key;
                }
            }

            /// <summary>
            /// AppUserModel Properties
            /// </summary>
            public static class AppUserModel
            {
                #region Properties

                /// <summary>
                /// <para>Name:     System.AppUserModel.ID -- PKEY_AppUserModel_ID</para>
                /// <para>Description: </para>
                /// <para>Type:     String -- VT_LPWSTR  (For variants: VT_BSTR)</para>
                /// <para>FormatID: {9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}, 5</para>
                /// </summary>
                public static PropertyKey ID
                {
                    get
                    {
                        PropertyKey key = new PropertyKey(new Guid("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"), 5);

                        return key;
                    }
                }

                #endregion
            }

            #endregion
        }
    }
}
