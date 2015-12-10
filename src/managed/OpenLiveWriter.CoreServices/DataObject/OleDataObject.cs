// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Com;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Class which encapsulates a native OleDataObject (OLE IDataObject interface).
    /// Instances of the class can be created from either a native Ole IDataObject
    /// or from a .NET IDataObject (in which case the native Ole IDataObject is
    /// extracted from the .NET IDataObject. This is sometimes necessary because
    /// the default .NET bridging between IOleDataObject to IDataObject obfuscates
    /// and in some cases corrupts the source data (this is the result of a combination
    /// of bugs and oversights).
    /// </summary>
    public class OleDataObject
    {
        /// <summary>
        /// Create an OleDataObject from a native Ole data object
        /// </summary>
        /// <param name="odo">native ole data object</param>
        /// <returns>newly created OleDataObject</returns>
        public static OleDataObject CreateFrom(IOleDataObject odo)
        {
            OleDataObject oleDataObject = new OleDataObject();
            oleDataObject.m_dataObject = odo;
            return oleDataObject;
        }

        /// <summary>
        /// Create an OleDataObject from a .NET IDataObject.
        /// </summary>
        /// <param name="ido">IDataObject to extract OleDataObject from</param>
        /// <returns>A new instance of OleDataObject mapped to the inner
        /// OleDataObject of the specified IDataObject or null if unable
        /// to extract OleDataObject from IDataObject</returns>
        public static OleDataObject CreateFrom(IDataObject ido)
        {
            // initialize OleDataObject
            OleDataObject oleDataObject = new OleDataObject();

            // attempt to convert to concrete DataObject class
            DataObject dataObject = ido as DataObject;
            if (dataObject == null)
            {
                return null;
            }

            // To extract an OleDataObject from a DataObject, we first need to
            // get the "innerData" field of the DataObject. This field is of type
            // System.Windows.Forms.UnsafeNativeMethods.OleConverter. Next, we
            // need to get the "innerData" field of the OleConverter, which is of
            // type System.Windows.Forms.UnsafeNativeMethods.IOleDataObject
            const string INNER_DATA_FIELD = "innerData";
            object innerData = oleDataObject.GetField(dataObject, INNER_DATA_FIELD);
            object innerInnerData = oleDataObject.GetField(innerData, INNER_DATA_FIELD);

            // attempt to convert the 'private' ole data object contained in
            // innerData into an instance of our locally defined IOleDataObject
            oleDataObject.m_dataObject = innerInnerData as IOleDataObject;
            if (oleDataObject.m_dataObject != null)
            {
                return oleDataObject;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Private default constructor (to create new instances of OleDataObject
        /// call OleDataObject.CreateFrom)
        /// </summary>
        private OleDataObject()
        {
        }

        /// <summary>
        /// Get the underlying IOleDataObject
        /// </summary>
        public IOleDataObject IOleDataObject
        {
            get { return m_dataObject; }
        }

        /// <summary>
        /// Determines whether the data object is capable of rendering the data described
        /// in the passed clipboard format and storage type(s). Objects attempting a
        /// paste or drop operation can call this method before calling GetData
        /// to get an indication of whether the operation may be successful
        /// </summary>
        /// <param name="clipFormat">Name of clipboard format requested</param>
        /// <param name="types">type(s) requested</param>
        /// <returns>true if the subseqent call to GetData would likely be
        /// successful, otherwise false</returns>
        public bool QueryGetData(string clipFormat, TYMED types)
        {
            // populate contents of FORMATETC structure
            FORMATETC formatEtc = new FORMATETC();
            OleDataObjectHelper.PopulateFORMATETC(clipFormat, types, ref formatEtc);

            // try to successfully query for the data
            int result = m_dataObject.QueryGetData(ref formatEtc);

            // data is available
            if (result == HRESULT.S_OK)
                return true;

            // data is not available
            else if (result == DV_E.FORMATETC)
                return false;

            // unexpected error
            else
            {
                Marshal.ThrowExceptionForHR(result);
                return false; // keep compiler happy
            }
        }

        /// <summary>
        /// Extract the date from within an OleDataObject. Pass in the requested
        /// clipboard format and type (or types ORed together) that you want
        /// the data in. The method will return an OleStgMedium for the type(s)
        /// requested if it is available, otherwise it will return null.
        /// If a single type is requested then the return value can be safely
        /// cast to the requested OleStgMedium subclasss. If multiple types
        /// are requested then the return value will represent the object's
        /// preferred storage representation and client code will need to use
        /// the 'is' operator to determine what type was returned.
        /// </summary>
        /// <param name="clipFormat">Name of clipboard format requested</param>
        /// <param name="types">type(s) requested</param>
        /// <returns>OleStgMedium instance if format and requested storage type
        /// are available, otherwise null</returns>
        public OleStgMedium GetData(string clipFormat, TYMED types)
        {
            return GetData(-1, clipFormat, types);
        }

        /// <summary>
        /// Extract the date from within an OleDataObject. Pass in the requested
        /// clipboard format and type (or types ORed together) that you want
        /// the data in. The method will return an OleStgMedium for the type(s)
        /// requested if it is available, otherwise it will return null.
        /// If a single type is requested then the return value can be safely
        /// cast to the requested OleStgMedium subclasss. If multiple types
        /// are requested then the return value will represent the object's
        /// preferred storage representation and client code will need to use
        /// the 'is' operator to determine what type was returned.
        /// </summary>
        /// <param name="lindex">Index of item to retreive</param>
        /// <param name="clipFormat">Name of clipboard format requested</param>
        /// <param name="types">type(s) requested</param>
        /// <returns>OleStgMedium instance if format and requested storage type
        /// are available, otherwise null</returns>
        public OleStgMedium GetData(int lindex, string clipFormat, TYMED types)
        {
            // populate contents of FORMATETC structure
            FORMATETC formatEtc = new FORMATETC();
            OleDataObjectHelper.PopulateFORMATETC(lindex, clipFormat, types, ref formatEtc);

            // attempt to get the data using the requested format
            STGMEDIUM stgMedium = new STGMEDIUM();
            if (m_dataObject != null)
            {
                int result = m_dataObject.GetData(ref formatEtc, ref stgMedium);

                // check for errors
                if (result != HRESULT.S_OK)
                {
                    // data format not supported (expected error condition)
                    if (result == DV_E.FORMATETC)
                        return null;

                    // unexpected error condition
                    else
                        Marshal.ThrowExceptionForHR(result);
                }

                // return the correct OleStgMedium subclass depending upon the type
                switch (stgMedium.tymed)
                {
                    case TYMED.NULL:
                        return null;
                    case TYMED.HGLOBAL:
                        return new OleStgMediumHGLOBAL(stgMedium);
                    case TYMED.FILE:
                        return new OleStgMediumFILE(stgMedium);
                    case TYMED.GDI:
                        return new OleStgMediumGDI(stgMedium);
                    case TYMED.MFPICT:
                        return new OleStgMediumMFPICT(stgMedium);
                    case TYMED.ENHMF:
                        return new OleStgMediumENHMF(stgMedium);
                    case TYMED.ISTREAM:
                        return new OleStgMediumISTREAM(stgMedium);
                    case TYMED.ISTORAGE:
                        return new OleStgMediumISTORAGE(stgMedium);
                    default:
                        Debug.Assert(false, "Invalid TYMED value");
                        return null;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Converter helper function to extract a field from an object
        /// This code Asserts the ReflectionPermission so that it can reflect over
        /// private types and private members. Assert is used rather than Demand
        /// so that callers are not required to have this permission. Note that the
        /// calling code must have SecurityPermission.Assertion in order for this
        /// to work. We need to do a bit more research on SecurityPermissions to
        /// determine whether Assert or Demand is the correct SecurityAction here.
        /// </summary>
        /// <param name="source">object to get field value from</param>
        /// <param name="fieldName">name of field</param>
        /// <returns>value contained in fieldName for source object</returns>
        /// [ReflectionPermission(SecurityAction.Assert, TypeInformation=true, MemberAccess=true)]
        private object GetField(object source, string fieldName)
        {
            // get the FieldInfo
            FieldInfo fld = source.GetType().GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance);

            // retreive and return the value
            if (fld != null)
                return fld.GetValue(source);
            else
                return null;
        }

        /// <summary>
        /// Underlying native OLE IDataObject.
        /// </summary>
        private IOleDataObject m_dataObject = null;

    }
}
