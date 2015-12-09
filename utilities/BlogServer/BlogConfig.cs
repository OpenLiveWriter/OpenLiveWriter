using System;
using System.Configuration;
using System.IO;
using System.Xml;
using BlogServer.Model;

namespace BlogServer
{
	public class BlogConfig
	{
		private	readonly Blog _blog;
		private readonly string _blogId;
		private readonly string _blogName;
		private readonly string _homepageUrl;
		private readonly string _username;
		private readonly string _password;
		private readonly string _uploadPath;
		private readonly string _uploadDir;

		public BlogConfig(XmlElement configEl)
		{
			_homepageUrl = XmlUtil.ReadString(configEl, "homepageUrl", null);
			if (_homepageUrl == null)
				throw new ConfigurationException(GetType().FullName + " property class requires a 'homepageUrl' subelement");

			string path = XmlUtil.ReadPath(configEl, "path", null);
			if (path == null)
				throw new ConfigurationException(GetType().FullName + " property class requires a 'path' subelement");
			
			_blog = new XmlBlog(path);
			_blogId = XmlUtil.ReadString(configEl, "id", Path.GetFileNameWithoutExtension(path));
			_blogName = XmlUtil.ReadString(configEl, "displayName", _blogId);
			
			_username = XmlUtil.ReadString(configEl, "username", string.Empty);
			_password = XmlUtil.ReadString(configEl, "password", string.Empty);
			
			_uploadPath = XmlUtil.ReadString(configEl, "uploadPath", null);
			_uploadDir = XmlUtil.ReadPath(configEl, "uploadDir", null);
			if (_uploadPath != null ^ _uploadDir != null)
			{
				throw new ConfigurationException(GetType().FullName +
				                                 " property class requires uploadPath and uploadDir to both be present to support newMediaObject");
			}
		}
		
		public Blog Blog
		{
			get { return _blog; }
		}
		
		public string BlogId
		{
			get { return _blogId; }
		}

		public string HomepageUrl
		{
			get { return _homepageUrl; }
		}

		public string BlogName
		{
			get { return _blogName; }
		}

		public bool CheckUsername(string username)
		{
			return _username == username;
		}

		public bool CheckPassword(string password)
		{
			return _password == password;
		}

		public string UploadPath
		{
			get { return _uploadPath; }
		}

		public string UploadDir
		{
			get { return _uploadDir; }
		}
	}
}
