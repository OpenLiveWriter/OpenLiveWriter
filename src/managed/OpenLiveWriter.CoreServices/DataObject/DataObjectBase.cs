// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// DataObject base is a simple class that holds an IDataObject and delegates
    /// calls to it.  Since it implements the IDataObject interface, you can
    /// derive from it and be an IDataObject yourself.  When using it it,
    /// be sure you set the IDataObject prior to any methods being called on it.
    ///
    /// Since all public methods simple delegate, see documentation on IDataObject
    /// for reference on methods.
    /// </summary>
    public class DataObjectBase : IDataObject
    {
        public object GetData(string format)
        {
            Validate();
            return m_dataObject.GetData(format);
        }

        public object GetData(Type format)
        {
            Validate();
            return m_dataObject.GetData(format);
        }

        public object GetData(string format, bool autoConvert)
        {
            Validate();
            return m_dataObject.GetData(format, autoConvert);
        }

        public bool GetDataPresent(string format)
        {
            Validate();
            return OleDataObjectHelper.GetDataPresentSafe(m_dataObject, format);
        }

        public bool GetDataPresent(Type format)
        {
            Validate();
            return OleDataObjectHelper.GetDataPresentSafe(m_dataObject, format);
        }

        public bool GetDataPresent(string format, bool autoConvert)
        {
            Validate();
            return m_dataObject.GetDataPresent(format, autoConvert);
        }

        public string[] GetFormats()
        {
            Validate();
            return m_dataObject.GetFormats();
        }

        public string[] GetFormats(bool autoConvert)
        {
            Validate();
            return m_dataObject.GetFormats(autoConvert);
        }

        public void SetData(object data)
        {
            Validate();
            m_dataObject.SetData(data);
        }

        public void SetData(Type format, object data)
        {
            Validate();
            m_dataObject.SetData(format, data);
        }

        public void SetData(string format, object data)
        {
            Validate();
            m_dataObject.SetData(format, data);
        }

        public void SetData(string format, bool autoConvert, object data)
        {
            Validate();
            m_dataObject.SetData(format, autoConvert, data);
        }

        /// <summary>
        /// The IDataObject which is contained by the DataObjectBase.
        /// </summary>
        protected IDataObject IDataObject
        {
            get
            {
                return m_dataObject;
            }

            set
            {
                m_dataObject = value;
            }
        }
        private IDataObject m_dataObject;

        /// <summary>
        /// Validates that the dataobject isn't null.
        /// </summary>
        private void Validate()
        {
            Debug.Assert(m_dataObject != null, "Set the dataObject before using the data object base");
        }

    }
}
