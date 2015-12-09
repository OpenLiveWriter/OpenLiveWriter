using System;
using System.IO;
using BlogServer.Config;
using BlogServer.WebServer;

namespace BlogServer
{
	public class AppMain
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: blogserver.exe <config_file.xml>");
				return;
			}
			
			FileAssociation.RegisterFileAssociation("Config", "BlogServer config file", ".bscx");
			
			string configFile = Path.Combine(Environment.CurrentDirectory, args[0]);
			ConfigProperties.Instance[ConfigProperties.BASE_DIR] = Path.GetDirectoryName(configFile);
			TrivialWebServer server = new ConfigReader(configFile).Create();
			
			Console.WriteLine("Listening on " + server.Endpoint.ToString());
			server.Accept();
		}
	}
}
