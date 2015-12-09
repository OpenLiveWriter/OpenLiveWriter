// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for LightWeightHTMLDataObject.
    /// </summary>
    public class LightWeightHTMLDataObject : DataObjectBase
    {
        public LightWeightHTMLDataObject(LightWeightHTMLDocument lightWeightDocument)
        {
            IDataObject = new DataObject(LIGHTWEIGHTHTMLDOCUMENTFORMAT, lightWeightDocument);
        }

        public static string LIGHTWEIGHTHTMLDOCUMENTFORMAT = "Format_LightWeightHTMLDocument";

    }
}
