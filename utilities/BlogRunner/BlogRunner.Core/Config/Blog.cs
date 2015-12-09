using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BlogRunner.Core.Config
{
    public class Blog
    {
        [XmlElement(ElementName = "homepageUrl")]
        public string HomepageUrl { get; set; }

        [XmlElement(ElementName = "username")]
        public string Username { get; set; }
        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        [XmlElement(ElementName = "apiUrl")]
        public string ApiUrl { get; set; }
        [XmlElement(ElementName = "blogId")]
        public string BlogId { get; set; }

        [XmlElement(ElementName = "include")]
        public string[] Include { get; set; }
        [XmlElement(ElementName = "exclude")]
        public string[] Exclude { get; set; }
    }

    public enum BlogApi { XmlRpc, AtomPub }
}
