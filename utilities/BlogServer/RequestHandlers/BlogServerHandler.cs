using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml;
using BlogServer.Config;
using BlogServer.Model;
using BlogServer.WebServer;
using BlogServer.XmlRpc;

namespace BlogServer.RequestHandlers
{
	/// <summary>
	/// Implements a functional MovableType API endpoint.
	/// 
	/// Config:
	/// Requires a blogProperty attribute whose value is the name of a server 
	/// property that contains a BlogConfig.
	/// </summary>
	public class BlogServerHandler : XmlRpcHttpRequestHandler
	{
		private readonly BlogConfig _blogConfig;

		public BlogServerHandler(XmlElement configEl) : base(true)
		{
			string blogProperty = XmlUtil.ReadString(configEl, "@blogProperty", null);
			if (blogProperty == null)
				throw new ConfigurationException("BlogServerHandler requires a 'blogProperty' attribute");
			
			_blogConfig = ConfigProperties.Instance[blogProperty] as BlogConfig;
			if (_blogConfig == null)
				throw new ConfigurationException("Property '" + blogProperty + "' was missing or invalid");
		}
		
		// blogger.getUsersBlogs (appkey, username, password) returns array of struct [url, blogid, blogName]
		[XmlRpcMethod("blogger.getUsersBlogs")]
		public IDictionary[] GetUsersBlogs(string appkey, string username, string password)
		{
			return new IDictionary[] { Util.MakeDictionary(
			                           	"url", _blogConfig.HomepageUrl,
			                           	"blogid", _blogConfig.BlogId,
			                           	"blogName", _blogConfig.BlogName
			                           	) };
		}
		
		// blogger.deletePost (appkey, postid, username, password, publish) returns true
		[XmlRpcMethod("blogger.deletePost")]
		public bool DeletePost(string appkey, string postid, string username, string password, bool publish)
		{
			CheckCredentials(username, password);
			if (!_blogConfig.Blog.DeleteBlogPost(postid))
				throw new XmlRpcServerException(404, "Post '" + postid + "' was not found");
			return true;
		}
		
		// metaWeblog.newPost (blogid, username, password, struct, publish) returns string
		[XmlRpcMethod("metaWeblog.newPost")]
		public string NewPost(string blogid, string username, string password, BlogPost post, bool publish)
		{
			CheckBlogId(blogid);
			CheckCredentials(username, password);
			post.Published = publish;
			if (post.Date == DateTime.MinValue)
				post.Date = DateTime.UtcNow;
			return _blogConfig.Blog.CreateBlogPost(post);
		}

		// metaWeblog.editPost (postid, username, password, struct, publish) returns true
		[XmlRpcMethod("metaWeblog.editPost")]
		public bool EditPost(string postid, string username, string password, BlogPost post, bool publish)
		{
			CheckCredentials(username, password);
			if (_blogConfig.Blog.Contains(postid))
			{
				BlogPost existingPost = _blogConfig.Blog[postid];
				
				existingPost.Title = post.Title;
				existingPost.Description = post.Description;
				existingPost.Excerpt = post.Excerpt;
				existingPost.ExtendedDescription = post.ExtendedDescription;
				existingPost.Keywords = post.Keywords;
				existingPost.AllowComments = post.AllowComments;
				existingPost.AllowPings = post.AllowPings;
				if (post.Date != DateTime.MinValue)
					existingPost.Date = post.Date;
				if (post.Categories.Length > 0)
					existingPost.Categories = post.Categories;
				
				// ignore post.PingUrls, for now
				
				existingPost.Published = publish;
				
				_blogConfig.Blog.UpdateBlogPost(existingPost);
				return true;
			}
			else
			{
				throw new XmlRpcServerException(404, "BlogConfig post with id " + postid + "was not found");
			}
		}
		
		// metaWeblog.getPost (postid, username, password) returns struct
		[XmlRpcMethod("metaWeblog.getPost")]
		public BlogPost GetPost(string postid, string username, string password)
		{
			CheckCredentials(username, password);
			if (_blogConfig.Blog.Contains(postid))
				return _blogConfig.Blog[postid];
			else
				throw new XmlRpcServerException(404, "BlogConfig post with id " + postid + "was not found");
		}
		
		// metaWeblog.getRecentPosts (blogid, username, password, numberOfPosts) returns array of structs
		[XmlRpcMethod("metaWeblog.getRecentPosts")]
		public BlogPost[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
		{
			CheckBlogId(blogid);
			CheckCredentials(username, password);
			return _blogConfig.Blog.GetRecentPosts(numberOfPosts);
		}
		
		// metaWeblog.newMediaObject (blogid, username, password, struct) returns struct
		[XmlRpcMethod("metaWeblog.newMediaObject")]
		public IDictionary NewMediaObject(string blogid, string username, string password, NewMediaObjectArgs args)
		{
			CheckBlogId(blogid);
			CheckCredentials(username, password);
			
			string dir = _blogConfig.UploadDir;
			string path = _blogConfig.UploadPath;
			
			if (dir == null || path == null)
				throw new XmlRpcServerException(403, "This server is not configured for newMediaObject support.");
			
			if (path != null && !path.EndsWith("/"))
				path += "/";
			
			if (!Directory.Exists(dir))
				throw new XmlRpcServerException(500, "The newMediaObject upload directory does not exist");

			string fileExt = Util.GetExtensionForContentType(args.Type);
			if (fileExt == null)
				throw new XmlRpcServerException(403, "Couldn't determine an extension for the given content type");
			string fileName = Guid.NewGuid().ToString("d") + fileExt;
			string fullFileName = Path.Combine(dir, fileName);
			using (Stream s = new FileStream(fullFileName, FileMode.CreateNew, FileAccess.Write, FileShare.Write, 8192))
			{
				StreamHelper.Transfer(new MemoryStream(args.Bits), s);
			}
			
			return Util.MakeDictionary("url", path + HttpUtility.UrlPathEncode(fileName));
		}
		
		[XmlRpcSerializable]
		public class NewMediaObjectArgs
		{
			private string _name;
			private string _type;
			private byte[] _bits;

			[XmlRpcStructMember("name")]
			public string Name
			{
				get { return _name; }
				set { _name = value;}
			}

			[XmlRpcStructMember("type")]
			public string Type
			{
				get { return _type; }
				set { _type = value; }
			}

			[XmlRpcStructMember("bits")]
			public byte[] Bits
			{
				get { return _bits; }
				set { _bits = value; }
			}
		}
		
		// mt.getRecentPostTitles (blogid, username, password, numberOfPosts) returns array of structs
		[XmlRpcMethod("mt.getRecentPostTitles")]
		public IDictionary[] GetRecentPostTitles(string blogid, string username, string password, int numberOfPosts)
		{
			CheckBlogId(blogid);
			CheckCredentials(username, password);
			BlogPost[] posts = _blogConfig.Blog.GetRecentPosts(numberOfPosts);
			ArrayList results = new ArrayList();
			foreach (BlogPost post in posts)
			{
				results.Add(Util.MakeDictionary(
				            	"dateCreated", post.Date,
				            	"userid", _blogConfig.BlogId,
				            	"postid", post.Id,
				            	"title", post.Title
				            	));
			}
			return (IDictionary[]) results.ToArray(typeof (IDictionary));
		}
		
		// mt.getCategoryList
		[XmlRpcMethod("mt.getCategoryList")]
		public Category[] GetCategoryList(string blogid, string username, string password)
		{
			CheckBlogId(blogid);
			CheckCredentials(username, password);
			
			Hashtable categories = new Hashtable();
			foreach (BlogPost post in _blogConfig.Blog.GetRecentPosts(int.MaxValue))
			{
				foreach (Category category in post.Categories)
				{
					categories[category.Id] = category;
				}
			}

			ArrayList catList = new ArrayList(categories.Values);
			catList.Sort();
			return (Category[]) catList.ToArray(typeof (Category));
		}

		// mt.getPostCategories
		[XmlRpcMethod("mt.getPostCategories")]
		public Category[] GetPostCategories(string postid, string username, string password)
		{
			BlogPost post = _blogConfig.Blog[postid];
			if (post == null)
				throw new XmlRpcServerException(404, "BlogConfig post with id '" + postid + "' does not exist or was deleted");

			return post.Categories;
		}
		
		// mt.setPostCategories
		[XmlRpcMethod("mt.setPostCategories")]
		public bool SetPostCategories(string postid, string username, string password, Category[] categories)
		{
			BlogPost post = _blogConfig.Blog[postid];
			if (post == null)
				throw new XmlRpcServerException(404, "BlogConfig post with id '" + postid + "' does not exist or was deleted");
			
			post.Categories = categories;
			_blogConfig.Blog.UpdateBlogPost(post);
			
			return true;
		}
		
		// mt.supportedMethods
		// mt.supportedTextFilters
		// mt.getTrackbackPings
		// mt.publishPost

		private void CheckBlogId(string blogid)
		{
			if (blogid != _blogConfig.BlogId)
				throw new XmlRpcServerException(404, "Unknown blog id '" + blogid + "'");
		}
		
		private void CheckCredentials(string username, string password)
		{
			if (!_blogConfig.CheckUsername(username))
				throw new XmlRpcServerException(401, "Unknown username");
			if (!_blogConfig.CheckPassword(password))
				throw new XmlRpcServerException(401, "Incorrect password");
		}
	}
}
