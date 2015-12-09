using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace BlogRunner.Core.Config
{
    public class Provider
    {
        [XmlElement(ElementName="id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "blog")]
        public Blog Blog { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlIgnore] // This comes from the BlogProviders.xml definitions instead
        public string ClientType { get; set; }

        [XmlElement(ElementName = "overrides")]
        public XmlElement Overrides { get; set; }
    }
}
