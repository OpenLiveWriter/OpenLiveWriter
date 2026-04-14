// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Class which implements helper methods for dealing with OleDataObjects
    /// </summary>
    public static class OleDataObjectHelper
    {
        /// <summary>
        /// Helper function to populate the contents of a FORMATETC structure
        /// using the passed clipboard format and type(s).
        /// </summary>
        /// <param name="clipFormat">Name of clipboard format requested</param>
        /// <param name="types">type(s) requested</param>
        /// <param name="formatEtc">FORMATETC structure to be populated</param>
        public static void PopulateFORMATETC(
            string clipFormat, TYMED types, ref FORMATETC formatEtc)
        {
            OleDataObjectHelper.PopulateFORMATETC(-1, clipFormat, types, ref formatEtc);
        }

        /// <summary>
        /// Helper function to populate the contents of a FORMATETC structure
        /// using the passed clipboard format and type(s).
        /// </summary>
        /// <param name="lindex">Index of item to retrieve</param>
        /// <param name="clipFormat">Name of clipboard format requested</param>
        /// <param name="types">type(s) requested</param>
        /// <param name="formatEtc">FORMATETC structure to be populated</param>
        public static void PopulateFORMATETC(
            int lindex, string clipFormat, TYMED types, ref FORMATETC formatEtc)
        {
            // populate contents of FORMATETC structure
            formatEtc.cfFormat = (ushort)User32.RegisterClipboardFormat(clipFormat);
            formatEtc.ptd = IntPtr.Zero;
            formatEtc.dwAspect = DVASPECT.CONTENT;
            formatEtc.lindex = lindex;
            formatEtc.tymed = types;
        }

        public static bool GetDataPresentSafe(IDataObject obj, string format)
        {
            try
            {
                if (obj == null)
                    return false;

                string[] formats = obj.GetFormats();

                if (formats == null)
                    return false;

                return ArrayHelper.SearchForIndexOf(formats, format, (a, b) => a == b) != -1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool GetDataPresentSafe(IDataObject obj, Type format)
        {
            // asking for .NET object types in GetDataPresent when the drag source is
            // another .NET application can result in an exception related to the
            // conversion of System.__ComObject to System.Type (no idea why).
            try
            {
                if (obj == null)
                    return false;

                return obj.GetDataPresent(format) && obj.GetData(format) != null;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}

