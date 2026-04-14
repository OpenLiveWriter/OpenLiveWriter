// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Image Data contains Image data that was parsed from the DataObject.
    /// </summary>
    public class ImageData
    {
        /// <summary>
        /// Creates an ImageData
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the ImageData</param>
        /// <returns>The ImageData, null if no ImageData could be created</returns>
        public static ImageData Create(IDataObject iDataObject)
        {
            if (!OleDataObjectHelper.GetDataPresentSafe(iDataObject, DataFormats.Dib) && !OleDataObjectHelper.GetDataPresentSafe(iDataObject, DataFormats.EnhancedMetafile))
                return null;
            else
                return new ImageData(iDataObject);
        }

        /// <summary>
        /// The DeviceIndependentBitmap
        /// </summary>
        public Stream Dib
        {
            get
            {
                if (m_dib == null)
                {
                    if (OleDataObjectHelper.GetDataPresentSafe(m_iDataObject, DataFormats.Dib))
                        m_dib = (Stream)m_iDataObject.GetData(DataFormats.Dib);
                }
                return m_dib;
            }
        }
        private Stream m_dib = null;

        /// <summary>
        /// The GIF Format
        /// </summary>
        public Stream GIF
        {
            get
            {
                if (m_gif == null)
                {
                    if (OleDataObjectHelper.GetDataPresentSafe(m_iDataObject, GIF_FORMAT))
                        m_gif = (Stream)m_iDataObject.GetData(GIF_FORMAT);
                }
                return m_gif;
            }
        }
        private Stream m_gif;
        public static string GIF_FORMAT = "GIF";

        /// <summary>
        /// Bitmap
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                if (m_bitmap == null)
                {
                    if (OleDataObjectHelper.GetDataPresentSafe(m_iDataObject, DataFormats.Bitmap))
                    {
                        object data = m_iDataObject.GetData(DataFormats.Bitmap);
                        m_bitmap = (Bitmap)data;
                    }
                }
                return m_bitmap;
            }
        }
        private Bitmap m_bitmap = null;

        private ImageData(IDataObject iDataObject)
        {
            m_iDataObject = iDataObject;
        }

        private IDataObject m_iDataObject;
    }
}
