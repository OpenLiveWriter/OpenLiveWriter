// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace OpenLiveWriter.CoreServices.ResourceDownloading
{
    internal class XPPResourceFileProcessor
    {
        public XPPResourceFileProcessor(CabbedXmlResourceFileDownloader.XmlResourceFileProcessor processor)
        {
            _processor = processor;
        }
        private readonly CabbedXmlResourceFileDownloader.XmlResourceFileProcessor _processor;

        public object ProcessXmlResource(XmlDocument document)
        {
            XmlPreprocessor preprocessor = new XmlPreprocessor();
            preprocessor.Add("version", new Version(ApplicationEnvironment.ProductVersion));
            preprocessor.Add("language", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            preprocessor.Add("culture", CultureInfo.CurrentUICulture.Name);

            preprocessor.Munge(document);

            return _processor(document);
        }

    }
}
