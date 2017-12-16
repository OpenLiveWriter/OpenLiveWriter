using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace CodeSnippet.Helpers
{
	public static class ConfigHelper
	{
		public static T LoadConfig<T>(T config, string configFile)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(config.GetType());
			try
			{
				TextReader streamReader = new StreamReader(configFile);
				config = (T)xmlSerializer.Deserialize(streamReader);
				streamReader.Close();
			}
			catch (FileNotFoundException fileNotFoundException)
			{
                // Billkrat.2017.12.15 - eliminate compile warning
                Trace.WriteLine($"File [{configFile}] {fileNotFoundException.Message}{fileNotFoundException.Message}");
			}
			return config;
		}

		public static void StoreConfig<T>(T config, string configFile)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(config.GetType());
			TextWriter streamWriter = new StreamWriter(configFile);
			xmlSerializer.Serialize(streamWriter, config);
			streamWriter.Close();
		}
	}
}