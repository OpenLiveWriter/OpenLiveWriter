// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Config
{
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Class Provider.
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// The identifier
        /// </summary>
        [XmlElement(ElementName = "id")]
        public string Id;

        /// <summary>
        /// The blog
        /// </summary>
        [XmlElement(ElementName = "blog")]
        public Blog Blog;

        /// <summary>
        /// The name
        /// </summary>
        [XmlElement(ElementName = "name")]
        public string Name;

        /// <summary>
        /// The client type
        /// </summary>
        [XmlIgnore] // This comes from the BlogProviders.xml definitions instead
        public string ClientType;

        /// <summary>
        /// The overrides
        /// </summary>
        [XmlElement(ElementName = "overrides")]
        public XmlElement Overrides;

        /// <summary>
        /// The exclude
        /// </summary>
        [XmlElement(ElementName = "exclude")]
        public string[] Exclude;
    }
}
