// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace OpenLiveWriter.CoreServices.ResourceDownloading
{
    public class CabbedXmlResourceFileDownloader
    {
        public static CabbedXmlResourceFileDownloader Instance
        {
            get { return _instance; }
        }

        private static readonly CabbedXmlResourceFileDownloader _instance;

        static CabbedXmlResourceFileDownloader()
        {
            _instance = new CabbedXmlResourceFileDownloader();
        }

        public object ProcessLocalResource(Assembly assembly, string name, XmlResourceFileProcessor processor)
        {
            try
            {
                processor = new XPPResourceFileProcessor(processor).ProcessXmlResource;

                return ProcessAssemblyResource(assembly, name, processor);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception processing local resource " + name + " : " + ex);
                return null;
            }
        }

        private static object ProcessAssemblyResource(Assembly assembly, string name, XmlResourceFileProcessor processor)
        {

            // calculate the full resource path name
            string assemblyResourcePath =
                String.Format(CultureInfo.InvariantCulture, "{0}.{1}", assembly.GetName().Name, name);

            // return the resource stream
            Stream stream = assembly.GetManifestResourceStream(assemblyResourcePath);

            if (stream == null)
                return null;

            XmlDocument document = new XmlDocument();
            document.Load(stream);

            return processor(document);
        }

        public delegate object XmlResourceFileProcessor(XmlDocument resourceDocument);
    }
}
