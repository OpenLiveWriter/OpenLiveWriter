using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients
{
	[BlogClient("Medium", "Medium")]
	public class MediumClient : BlogClientBase, IBlogClient
	{
		private const string MediumApiUri = "https://api.medium.com/v1";
		private readonly Uri _postApiUri;
		private IBlogClientOptions _clientOptions;

		public bool IsSecure
		{
			get
			{
				try
				{
					return String.Equals(_postApiUri.Scheme, "https", StringComparison.InvariantCultureIgnoreCase);
				}
				catch
				{
					return true;
				}
			}
		}

		public IBlogClientOptions Options
		{
			get
			{
				return _clientOptions;
			}
		}

		public MediumClient(Uri postApiUrl, IBlogCredentialsAccessor credentials) : base(credentials)
		{
			_postApiUri = new Uri(UrlHelper.SafeToAbsoluteUri(postApiUrl));
			var clientOptions = new BlogClientOptions
			{
				SupportsCategories = true,
				SupportsMultipleCategories = true
			};

			_clientOptions = clientOptions;
		}

		public string AddCategory(string blogId, BlogPostCategory category)
		{
			throw new NotImplementedException();
		}

		public void DeletePage(string blogId, string pageId)
		{
			throw new NotImplementedException();
		}

		public void DeletePost(string blogId, string postId, bool publish)
		{
			throw new NotImplementedException();
		}

		public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
		{
			throw new NotImplementedException();
		}

		public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
		{
			throw new NotImplementedException();
		}

		public bool? DoesFileNeedUpload(IFileUploadContext uploadContext)
		{
			throw new NotImplementedException();
		}

		public bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
		{
			throw new NotImplementedException();
		}

		public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
		{
			throw new NotImplementedException();
		}

		public AuthorInfo[] GetAuthors(string blogId)
		{


			return null;
		}

		public BlogPostCategory[] GetCategories(string blogId)
		{
			throw new NotImplementedException();
		}

		public BlogInfo[] GetImageEndpoints()
		{
			throw new NotImplementedException();
		}

		public BlogPostKeyword[] GetKeywords(string blogId)
		{
			throw new NotImplementedException();
		}

		public BlogPost GetPage(string blogId, string pageId)
		{
			throw new NotImplementedException();
		}

		public PageInfo[] GetPageList(string blogId)
		{
			throw new NotImplementedException();
		}

		public BlogPost[] GetPages(string blogId, int maxPages)
		{
			throw new NotImplementedException();
		}

		public BlogPost GetPost(string blogId, string postId)
		{
			throw new NotImplementedException();
		}

		public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
		{
			throw new NotImplementedException();
		}

		public BlogInfo[] GetUsersBlogs()
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(String.Concat(MediumApiUri, "/users/" + "/publications"));

			httpWebRequest.Headers.Add("Authorization", "Bearer: " + Credentials.Password);

			var response = (HttpWebResponse)httpWebRequest.GetResponse();

			if((int) response.StatusCode != 200)
			{
				throw new WebException("Failure loading user's blogs!!!");
			}

			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				var blogs = JsonConvert.DeserializeObject<MediumDataWrapper<List<MediumPublication>>>(sr.ReadToEnd());
				List<BlogInfo> blogInfos = new List<BlogInfo>();
				blogs.data.ForEach((publication) => blogInfos.Add(new BlogInfo(publication.id, publication.name, publication.url)));
				return blogInfos.ToArray();
			}

			return null;
		}

		public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
		{
			throw new NotImplementedException();
		}

		public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
		{
			throw new NotImplementedException();
		}

		public void OverrideOptions(IBlogClientOptions newClientOptions)
		{
			_clientOptions = newClientOptions;
		}

		public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
		{
			throw new NotImplementedException();
		}

		public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
		{
			throw new NotImplementedException();
		}

		protected override void VerifyCredentials(TransientCredentials tc)
		{
			throw new NotImplementedException();
		}
	}
}
