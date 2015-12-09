using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using BlogServer.WebServer;

namespace BlogServer.Config
{
	public class ConfigReader
	{
		private readonly DiffManager _diffMgr;
		private XmlElement _configRoot;
		
		public ConfigReader(string configFilePath)
		{
			_diffMgr = new DiffManager(configFilePath);
			_configRoot = _diffMgr.ConfigRoot;
			PrintConfig();
		}

		private void PrintConfig()
		{
			/*
			XmlTextWriter xw = new XmlTextWriter(Console.Out);
			xw.Formatting = Formatting.Indented;
			xw.Indentation = 2;
			xw.IndentChar = ' ';
			xw.WriteNode(new XmlNodeReader(_configRoot), false);
			Console.WriteLine();
			*/
		}

		internal TrivialWebServer Create()
		{
			lock (this)
			{
				ReloadConfig();
				TrivialWebServer server = new TrivialWebServer(new IPEndPoint(IPAddress.Any, Port), LocalOnly);
				server.HandleConnectionBegin += new EventHandler(server_HandleConnectionBegin);
				ConfigureProperties();
				ConfigurePaths(server);
				return server;
			}
		}
		
		internal void Reconfigure(TrivialWebServer tws)
		{
			lock (this)
			{
				tws.ClearHandlers();
				ConfigureProperties();
				ConfigurePaths(tws);
			}
		}
			
		private void server_HandleConnectionBegin(object sender, EventArgs e)
		{
			lock (this)
			{
				if (ReloadConfig())
				{
					Console.WriteLine("Configuration file change detected, reloading");
					PrintConfig();
					Reconfigure((TrivialWebServer) sender);
				}
			}
		}

		private bool ReloadConfig()
		{
			if (_diffMgr.MaybeReload())
			{
				_configRoot = _diffMgr.ConfigRoot;
				return true;
			}
			return false;
		}

		private void ConfigureProperties()
		{
			foreach (XmlElement prop in _configRoot.SelectNodes("property"))
			{
				string name = XmlUtil.ReadString(prop, "@name", null);
				if (name == null)
					throw new ConfigurationException("Property element requires a name attribute");
				
				object value = XmlUtil.ReadString(prop, "@value", null);

				if (value == null)
					value = XmlUtil.ReadPath(prop, "@path", null);
				
				if (value == null)
				{
					string clazz = XmlUtil.ReadString(prop, "@className", null);
					if (clazz == null)
						throw new ConfigurationException("Property element requires either a 'value' or 'className' attribute");
					ConstructorInfo type = Type.GetType(clazz, true).GetConstructor(new Type[] {typeof (XmlElement)});
					value = type.Invoke(new object[] {prop});
				}
				
				ConfigProperties.Instance[name] = value;
			}
		}

		private void ConfigurePaths(TrivialWebServer server)
		{
			foreach (XmlElement pathEl in _configRoot.SelectNodes("path"))
			{
				string pattern = XmlUtil.ReadString(pathEl, "@value", null);
				if (pattern != null)
					pattern = "^" + Regex.Escape(pattern) + "$";
				else
					pattern = XmlUtil.ReadString(pathEl, "@pattern", null);
				
				if (pattern == null)
					throw new ConfigurationException("Path element requires either 'value' or 'pattern'");

				HttpRequestPath path = new HttpRequestPath();
				path.Register(new Regex(pattern), server);
				
				bool hasSeenHandler = false;
				XmlNode childNode = pathEl.FirstChild;
				while (childNode != null)
				{
					XmlElement childEl = childNode as XmlElement;
					if (childEl == null)
					{
						childNode = childNode.NextSibling;
						continue;
					}

					bool isFilter;
					switch (childEl.Name)
					{
						case "filter":
							isFilter = true;
							if (hasSeenHandler)
								throw new ConfigurationException("All filters for a given path must come before all the handlers in that path");
							break;
						case "handler":
							isFilter = false;
							hasSeenHandler = true;
							break;
						default:
							throw new ConfigurationException("Element '" + childEl.Name + "' was not expected here");
					}

					string clazz = XmlUtil.ReadString(childEl, "@className", null);
					Type filterType = Type.GetType(clazz, true);
					ConstructorInfo constructor = filterType.GetConstructor(new Type[] {typeof(XmlElement)});
					if (constructor == null)
						throw new ConfigurationException("Class " + filterType.FullName + " does not have a constructor that takes XmlElement");
					object filterOrHandler = constructor.Invoke(new object[] {childEl});
					if (isFilter)
					{
						if (!(filterOrHandler is HttpRequestFilter))
							throw new ConfigurationException("Class " + filterType.FullName + " does not derive from HttpRequestFilter");
						path.AddFilter((HttpRequestFilter) filterOrHandler);
					}
					else
					{
						if (!(filterOrHandler is HttpRequestHandler))
							throw new ConfigurationException("Class " + filterType.FullName + " does not derive from HttpRequestHandler");
						path.AddHandler((HttpRequestHandler) filterOrHandler);
					}

					childNode = childNode.NextSibling;
				}
			}
		}

		private bool LocalOnly
		{
			get { return !XmlUtil.ReadBool(_configRoot, "@allowRemote", true); }
		}

		private int Port
		{
			get { return XmlUtil.ReadInt(_configRoot, "@port", 80); }
		}
	}
}

#if false
Proposed syntax:

<server port="8080" allowRemote="false">
	<path value="/api">
		<filter class="TraceFilter"/>
		<handler class="FailXmlRpcHandler">
			<method name="metaWeblog.newPost" faultCode="999" faultString="Intentional Error"/>
		</handler>
		<handler class="ProxyHandler" url="http://unknown.wordpress.com/rpc/metaweblog"/>
	</path>
	<path pattern="/static/.*">
		<handler class="StaticHandler" path="${installdir}\static"/>
	</path>
	<path pattern="/rsd">
		<handler class="StringHandler" contentType="text/xml"><![CDATA[
			<rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
			<service>
				<engineName>WordPress</engineName>
				<engineLink>http://wordpress.org/</engineLink>
				<homePageLink>http://unknown.wordpress.com</homePageLink>
				<apis>
				<api name="WordPress" blogID="1" preferred="false" apiLink="http://unknown.wordpress.com/xmlrpc.php" />
				<api name="Movable Type" blogID="1" preferred="true" apiLink="http://unknown.wordpress.com/xmlrpc.php" />
				<api name="MetaWeblog" blogID="1" preferred="false" apiLink="http://unknown.wordpress.com/xmlrpc.php" />
				<api name="Blogger" blogID="1" preferred="false" apiLink="http://unknown.wordpress.com/xmlrpc.php" />
				</apis>
			</service>
			</rsd>
		]]></handler>
	</path>
</server>
#endif