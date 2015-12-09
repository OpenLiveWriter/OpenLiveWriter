using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml;
using BlogServer.WebServer;
using DynamicTemplate;

namespace BlogServer.RequestHandlers
{
	/// <summary>
	/// Serves up static files from disk, just like a normal webserver.
	/// 
	/// Currently limited to the following extensions:
	/// .htm, .html, .xhtml, .txt, .css, .jpg, .jpeg, .gif, .png, .xml
	/// 
	/// Any other type of file will cause a 403.
	/// 
	/// If the file is not found, the request is not handled.
	/// 
	/// Config:
	/// basePath attribute (required): Specifies the URL path prefix 
	///		which should be used when mapping URLs to local files.
	/// rootDir attribute (required): Specifies the local directory
	///		which should be used when mapping URLs to local files.
	/// 
	/// Example:
	/// http://server/foo maps to a directory c:\webroot, e.g.
	/// http://server/foo/bar.png maps to c:\webroot\bar.png.
	/// basePath would be "/foo" and rootDir would be "c:\webroot".
	/// </summary>
	public class FilesystemHandler : HttpRequestHandler
	{
		private string _basePath;
		private string _rootDir;
		private bool _allowDirectoryBrowsing = true;
		private Template _indexTemplate;

		public FilesystemHandler(XmlElement configEl)
		{
			_basePath = XmlUtil.ReadString(configEl, "@basePath", null);
			if (!_basePath.EndsWith("/"))
				_basePath += "/";
			
			string rootDir = XmlUtil.ReadPath(configEl, "@rootDir", null);
			_rootDir = Util.PathCanonicalize(rootDir);
			if (!_rootDir.EndsWith("\\"))
				_rootDir += "\\";
			
			_allowDirectoryBrowsing = XmlUtil.ReadBool(configEl, "@allowDirectoryBrowsing", false);
			
			_indexTemplate = Template.Compile(DIR_TEMPLATE,
				new ArgumentDescription(typeof(string), "path"),
				new ArgumentDescription(typeof(string[]), "dirs"),
				new ArgumentDescription(typeof(string[]), "files"));
		}
		
		public override bool Handle(Request request, Response response)
		{
			if (request.HttpMethod != "GET")
				return false;
			
			string[] chunks = request.Path.Split(new char[] {'?'}, 2);
			string path1 = HttpUtility.UrlDecode(chunks[0]);
			if (!path1.StartsWith(_basePath))
				return false;
			string relativePath = path1.Substring(_basePath.Length).Replace('/', '\\');
			string filePath = Util.PathCanonicalize(Path.Combine(_rootDir, relativePath));
			if (!filePath.StartsWith(_rootDir))
			{
				Debug.Fail("Unexpected file path requested: " + filePath);
				return false;
			}
			
			if (Directory.Exists(filePath))
			{
				if (!request.Path.EndsWith("/"))
				{
					response.SendRedirect(request.Path + "/", false);
					return true;
				}
				
				if (File.Exists(Path.Combine(filePath, "index.htm")))
				{
					response.SendFile(Path.Combine(filePath, "index.htm"), "text/html");
					return true;
				}
				else if (File.Exists(Path.Combine(filePath, "index.html")))
				{
					response.SendFile(Path.Combine(filePath, "index.html"), "text/html");
					return true;
				}
				else if (_allowDirectoryBrowsing)
				{
					OutputDirectory(filePath, request, response);
					return true;
				}
				else
					return false;
			}
			
			if (!File.Exists(filePath))
				return false;
			
			string contentType;
			switch (Path.GetExtension(filePath).ToLower())
			{
				case ".htm":
				case ".html":
				case ".xhtml":
					contentType = "text/html";
					break;
				case ".txt":
					contentType = "text/plain";
					break;
				case ".css":
					contentType = "text/css";
					break;
				case ".jpg":
				case ".jpeg":
					contentType = "image/jpeg";
					break;
				case ".gif":
					contentType = "image/gif";
					break;
				case ".png":
					contentType = "image/png";
					break;
				case ".xml":
					contentType = "text/xml";
					break;
				default:
					response.SendErrorCode(403, "Access to the requested file type is forbidden");
					return true;
			}
			
			response.SendFile(filePath, contentType);
			return true;
		}

		private void OutputDirectory(string dir, Request request, Response response)
		{
			string[] dirs = Directory.GetDirectories(dir);
			Cleanup(dirs);
			string[] files = Directory.GetFiles(dir);
			Cleanup(files);
			
			response.SendHtml(_indexTemplate.Execute(request.Path, dirs, files));
		}

		private void Cleanup(string[] paths)
		{
			for (int i = 0; i < paths.Length; i++)
				paths[i] = Path.GetFileName(paths[i]);
			Array.Sort(paths, new CaseInsensitiveComparer(CultureInfo.InvariantCulture));
		}

		private const string DIR_TEMPLATE = @"<html>
<head><title>Directory Index</title></head>
<body>
<h1>Index of <%= HtmlEncode(path) %></h1>
<ul>
	<% foreach (string dir in dirs) { %>
	<li><a href=""<%= UrlPathEncode(dir) %>""><b><%= HtmlEncode(dir) %>/</b></a></li>
	<% } %>
	<% foreach (string file in files) { %>
	<li><a href=""<%= UrlPathEncode(file) %>""><%= HtmlEncode(file) %></a></li>
	<% } %>
</ul>
</body>
</html>";
	}
}
