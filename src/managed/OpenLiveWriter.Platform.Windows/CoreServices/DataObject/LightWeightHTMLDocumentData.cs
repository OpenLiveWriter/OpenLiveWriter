// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for LightWeightHTMLDocumentData.
    /// </summary>
    public class LightWeightHTMLDocumentData
    {
        private LightWeightHTMLDocument _document;

        public static LightWeightHTMLDocumentData Create(IDataObject iDataObject)
        {
            string[] loser = iDataObject.GetFormats();

            if (OleDataObjectHelper.GetDataPresentSafe(iDataObject, LightWeightHTMLDataObject.LIGHTWEIGHTHTMLDOCUMENTFORMAT))
            {
                LightWeightHTMLDocument document = (LightWeightHTMLDocument)iDataObject.GetData(LightWeightHTMLDataObject.LIGHTWEIGHTHTMLDOCUMENTFORMAT);
                return new LightWeightHTMLDocumentData(document);
            }
            else
                return null;

        }

        private LightWeightHTMLDocumentData(LightWeightHTMLDocument document)
        {
            _document = document;
        }

        public LightWeightHTMLDocument LightWeightHTMLDocument
        {
            get
            {
                return _document;
            }
        }
    }
}
