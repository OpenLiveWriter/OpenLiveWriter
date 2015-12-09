using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace BlogServer.Model
{
	public sealed class XmlBlog : Blog
	{
		private readonly string _xmlFilePath;
		private readonly XmlSerializer _serializer = new XmlSerializer(typeof (BlogPersist));

		public XmlBlog(string xmlFilePath)
		{
			_xmlFilePath = xmlFilePath;
			
			bool isNew = true;
			using (Stream s = new FileStream(_xmlFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
			{
				if (s.Length > 0)
				{
					BlogPersist persist = (BlogPersist) _serializer.Deserialize(s);
					Add(persist.BlogPosts);
					isNew = false;
				}
			}
			
			if (isNew)
			{
				Persist();
			}
		}

		protected override void Persist()
		{
			BlogPersist persist = GetBlogPersist();
			using (Stream s = new FileStream(_xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				_serializer.Serialize(s, persist);
			}
		}
	}
}
