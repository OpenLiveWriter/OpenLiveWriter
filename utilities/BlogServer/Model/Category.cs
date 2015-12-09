using System;
using System.Globalization;
using BlogServer.XmlRpc;

namespace BlogServer.Model
{
	[XmlRpcSerializable]
	public class Category : IComparable
	{
		private string _id;
		private string _name;
		private bool _isPrimary;

		public Category()
		{
		}

		public Category(string id, string name, bool isPrimary)
		{
			_id = id;
			_name = name;
			_isPrimary = isPrimary;
		}
		
		public Category(string name) : this(name, name, false)
		{
		}
		
		[XmlRpcStructMember("categoryId")]
		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[XmlRpcStructMember("categoryName")]
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		[XmlRpcStructMember("isPrimary")]
		public bool IsPrimary
		{
			get { return _isPrimary; }
			set { _isPrimary = value; }
		}

		public int CompareTo(object obj)
		{
			return string.Compare(Name, ((Category) obj).Name, true, CultureInfo.InvariantCulture);
		}
	}
}
