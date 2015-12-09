using System;
using System.Configuration;
using System.IO;
using System.Xml;
using BlogServer.Config;
using BlogServer.WebServer;
using DynamicTemplate;
using DynamicTemplate.Compiler;

namespace BlogServer.RequestHandlers
{
	/// <summary>
	/// Renders a blog homepage.
	/// 
	/// Config:
	/// Requires a blogProperty attribute whose value is the name of a server 
	/// property that contains a BlogConfig. The inner text (I recommend using 
	/// CDATA) of the element should be an ASP.NET-like template, that 
	/// references an implicit BlogConfig variable named "blogConfig".
	/// </summary>
	public class BlogHomepageHandler : HttpRequestHandler
	{
		private readonly BlogConfig _blogConfig;
		private readonly Template _template;

		public BlogHomepageHandler(XmlElement configEl)
		{
			string blogProperty = XmlUtil.ReadString(configEl, "@blogProperty", null);
			
			if (blogProperty == null)
				throw new ConfigurationException("BlogHomepageHandler requires a 'blogProperty' attribute");

			_blogConfig = ConfigProperties.Instance[blogProperty] as BlogConfig;
			if (_blogConfig == null)
				throw new ConfigurationException("Property '" + blogProperty + "' was missing or invalid");
			
			_template = Template.Compile(
				StringUtil.StripIndentation(configEl.InnerText), 
				new ArgumentDescription(typeof(BlogConfig), "blogConfig"));
		}
		
		public override bool Handle(Request request, Response response)
		{
			if (request.HttpMethod != "GET")
			{
				response.SendErrorCode(501, "Method not implemented");
				return true;
			}

			string result = _template.Execute(new object[] {_blogConfig});
			response.SendHtml(result);
			return true;
		}
	}
}
