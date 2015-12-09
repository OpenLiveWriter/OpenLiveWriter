using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace BlogRunner.Core.Config
{
    [XmlRoot(ElementName="config")]
    public class Config
    {
        public static Config Load(string path, string providersPath)
        {
            var ser = new XmlSerializer(typeof(Config));
            Config config;
            using (Stream s = File.OpenRead(path))
                config = (Config) ser.Deserialize(s);

            XmlDocument providersXml = new XmlDocument();
            providersXml.Load(providersPath);

            foreach (Provider p in config.Providers)
            {
                string providerId = p.Id;
                XmlText el = (XmlText) providersXml.SelectSingleNode("/providers/provider/id[text()='" + providerId + "']/../clientType/text()");
                if (el == null)
                {
                    Console.Error.WriteLine("Unknown provider ID: " + providerId);
                    throw new ArgumentException("Unknown provider ID: " + providerId);
                }
                p.ClientType = el.Value;
            }

            return config;
        }

        [XmlArray(ElementName="providers")]
        [XmlArrayItem(ElementName="provider")]
        public Provider[] Providers { get; set; }

        public Provider GetProviderById(string providerId)
        {
            foreach (Provider p in Providers)
                if (p.Id == providerId)
                    return p;
            return null;
        }
    }
}
