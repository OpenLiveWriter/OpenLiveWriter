// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace BlogRunner.Core.Config
{
    public class Provider
    {
        [XmlElement(ElementName = "id")]
        public string Id;

        [XmlElement(ElementName = "blog")]
        public Blog Blog;

        [XmlElement(ElementName = "name")]
        public string Name;

        [XmlIgnore] // This comes from the BlogProviders.xml definitions instead
        public string ClientType;

        [XmlElement(ElementName = "overrides")]
        public XmlElement Overrides;

        [XmlElement(ElementName = "exclude")]
        public string[] Exclude;
    }
}
