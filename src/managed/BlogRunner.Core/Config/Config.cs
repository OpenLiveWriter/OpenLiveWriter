// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Config
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    using JetBrains.Annotations;

    /// <summary>
    /// Class Config.
    /// </summary>
    [XmlRoot(ElementName = "config")]
    public class Config
    {
        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="providersPath">The providers path.</param>
        /// <returns>A BlogRunner.Core.Config.Config.</returns>
        public static Config Load(string path, string providersPath)
        {
            var ser = new XmlSerializer(typeof(Config));
            Config config;
            using (Stream s = File.OpenRead(path))
            {
                config = (Config)ser.Deserialize(s);
            }

            var providersXml = new XmlDocument();
            providersXml.Load(providersPath);

            foreach (var p in config.Providers)
            {
                var providerId = p.Id;
                var el = (XmlText)providersXml.SelectSingleNode(
                    $"/providers/provider/id[text()='{providerId}']/../clientType/text()");
                if (el == null)
                {
                    Console.Error.WriteLine($"Unknown provider ID: {providerId}");
                    throw new ArgumentException($"Unknown provider ID: {providerId}");
                }

                p.ClientType = el.Value;
            }

            return config;
        }

        /// <summary>
        /// The providers
        /// </summary>
        [XmlArray(ElementName = "providers")]
        [XmlArrayItem(ElementName = "provider")]
        public Provider[] Providers;

        /// <summary>
        /// Gets the provider by identifier.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        /// <returns>Provider.</returns>
        [CanBeNull]
        public Provider GetProviderById(string providerId)
        {
            return this.Providers.FirstOrDefault(p => p.Id == providerId);
        }
    }
}
