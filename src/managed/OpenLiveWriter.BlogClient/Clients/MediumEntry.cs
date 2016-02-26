using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLiveWriter.BlogClient.Clients
{
	internal class MediumUser
	{
		public string id { get; set; }
		public string username { get; set; }
		public string name { get; set; }
		public string url { get; set; }
		public string imageUrl { get; set; }
	}

	internal class MediumPublication
	{
		public string id { get; set; }
		public string description { get; set; }
		public string name { get; set; }
		public string url { get; set; }
		public string imageUrl { get; set; }
	}

	internal class MediumContributor
	{
		public string publicationId { get; set; }
		public string userId { get; set; }
		public string role { get; set; }
	}

	internal class MediumPostRequest
	{
		public string title { get; set; }

		public string contentFormat
		{
			get { return contentFormat; }
			set
			{
				if (value != "markdown" && value != "html")
				{
					throw new ArgumentException("contentFormat value not valid. Must be either 'markdown' or 'html'");
				}
				else
				{
					contentFormat = value;
				}
			}
		}

		public string content { get; set; }
		public string canonicalUrl { get; set; }
		public List<string> tags { get; set; }
		public string publishStatus
		{
			get
			{
				return this.publishStatus;
			}
			set
			{
				switch(value)
				{
					case "public":

					case "draft":

					case "unlisted":

						this.publishStatus = value;
						break;

					default: throw new ArgumentException("publishStatus must be one of { 'public', 'draft', 'unlisted'");
				}
			}
		}
		public string license { get; set; }
	}

	internal class MediumPostResponse
	{
		public string id { get; set; }
		public string title { get; set; }
		public string authorId { get; set; }
		public string url { get; set; }
		public string canonicalUrl { get; set; }
		public string publishStatus { get; set; }
		public string license { get; set; }
		public string licenseUrl { get; set; }
		public int publishedAt { get; set; }
		public List<string> tags { get; set; }
	}

	internal class MediumDataWrapper<T>
	{
		public T data { get; set; }
	}

	internal class MediumEntry
	{
	}
}
