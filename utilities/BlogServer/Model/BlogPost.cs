using System;
using System.Collections;
using BlogServer.XmlRpc;

namespace BlogServer.Model
{
	[XmlRpcSerializable]
	public class BlogPost : ICloneable
	{
		private string _id;
		private string _title;
		private string _description;
		private string _extendedDescription;
		private string _excerpt;
		private DateTime _date;
		private int _allowComments;
		private int _allowPings;
		private string _convertBreaks;
		private string _keywords;
		private string[] _pingUrls;
		private Category[] _categories;
		private bool _published;

		public BlogPost()
		{
		}
		
		public BlogPost Clone()
		{
			BlogPost clone = (BlogPost) MemberwiseClone();
			clone.PingUrls = PingUrls;
			clone.Categories = Categories;
			return clone;
		}
		
		object ICloneable.Clone()
		{
			return this.Clone();
		}
		
		[XmlRpcStructMember("postid")]
		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}
		
		[XmlRpcStructMember("title")]
		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		[XmlRpcStructMember("description")]
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		[XmlRpcStructMember("mt_text_more")]
		public string ExtendedDescription
		{
			get { return _extendedDescription; }
			set { _extendedDescription = value; }
		}

		[XmlRpcStructMember("mt_excerpt")]
		public string Excerpt
		{
			get { return _excerpt; }
			set { _excerpt = value; }
		}

		[XmlRpcStructMember("dateCreated")]
		public DateTime TempDate
		{
			get { return _date; }
			set { _date = value; }
		}
		
		public DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}

		[XmlRpcStructMember("mt_allow_comments")]
		public int AllowComments
		{
			get { return _allowComments; }
			set { _allowComments = value; }
		}

		[XmlRpcStructMember("mt_allow_pings")]
		public int AllowPings
		{
			get { return _allowPings; }
			set { _allowPings = value; }
		}

		[XmlRpcStructMember("mt_convert_breaks")]
		public string ConvertBreaks
		{
			get { return _convertBreaks; }
			set { _convertBreaks = value; }
		}

		[XmlRpcStructMember("mt_keywords")]
		public string Keywords
		{
			get { return _keywords; }
			set { _keywords = value; }
		}

		[XmlRpcStructMember("mt_tb_ping_urls")]
		public string[] PingUrls
		{
			get { return _pingUrls == null ? null : (string[]) _pingUrls.Clone(); }
			set { _pingUrls = value == null ? null : (string[]) value.Clone(); }
		}

/*
		public string[] Categories
		{
			get { return _categories == null ? null : (string[]) _categories.Clone(); }
			set { _categories = value == null ? null : (string[]) value.Clone(); }
		}
*/
		public Category[] Categories
		{
			get { return _categories == null ? new Category[0] : (Category[]) _categories.Clone(); }
			set { _categories = value == null ? new Category[0] : (Category[]) value.Clone(); }
		}

		public bool Published
		{
			get { return _published; }
			set { _published = value; }
		}
	}
}
