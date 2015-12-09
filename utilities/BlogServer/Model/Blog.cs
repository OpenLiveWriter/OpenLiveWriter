using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using BlogServer.XmlRpc;

namespace BlogServer.Model
{
	public abstract class Blog
	{
		protected ArrayList _postList = new ArrayList();
		protected Hashtable _postTable = new Hashtable();
		private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
		
		protected BlogPersist GetBlogPersist()
		{
			using (Lock(false))
			{
				return new BlogPersist((BlogPost[]) _postList.ToArray(typeof(BlogPost)));
			}
		}
		
		protected void Add(params BlogPost[] posts)
		{
			foreach (BlogPost post in posts)
			{
				if (post.Id == null)
					throw new ArgumentException("One or more posts had a null ID");
				if (_postTable.ContainsKey(post.Id))
					throw new InvalidOperationException("Duplicate post ID detected: " + post.Id);
				_postTable.Add(post.Id, post);
				_postList.Add(post);
			}
		}
		
		public BlogPost this[int index]
		{
			get
			{
				BlogPost blogPost;

				using (Lock(false))
					blogPost = (BlogPost) _postList[index];
				
				if (blogPost != null)
					blogPost = blogPost.Clone();
				
				return blogPost;
			}
		}
		
		public BlogPost this[string postId]
		{
			get
			{
				BlogPost blogPost;
				
				using (Lock(false))
					blogPost = (BlogPost) _postTable[postId];
				
				if (blogPost != null)
					blogPost = blogPost.Clone();
				
				return blogPost;
			}
		}
		
		public int Count
		{
			get 
			{
				using (Lock(false))
					return _postList.Count;
			}
		}
		
		public BlogPost[] GetRecentPosts(int numberOfPosts)
		{
			ArrayList results = new ArrayList();
			using (Lock(false))
			{
				numberOfPosts = Math.Min(numberOfPosts, Count);
				for (int i = 0; i < numberOfPosts; i++)
					results.Add(this[Count - i - 1].Clone());
			}
			return (BlogPost[]) results.ToArray(typeof (BlogPost));
		}
		
		public BlogPost[] GetRecentPublishedPosts(int numberOfPosts)
		{
			ArrayList results = new ArrayList();
			using (Lock(false))
			{
				for (int i = 0; i < Count && results.Count < numberOfPosts; i++)
				{
					BlogPost post = this[Count - i - 1];
					if (post.Published)
						results.Add(post.Clone());
				}
			}
			return (BlogPost[]) results.ToArray(typeof (BlogPost));
		}

		protected abstract void Persist();
		
		public bool Contains(string postid)
		{
			using (Lock(false))
			{
				return _postTable.ContainsKey(postid);
			}
		}

		public string CreateBlogPost(BlogPost newPost)
		{
			string newId = Guid.NewGuid().ToString("d");
			newPost.Id = newId;
			
			using (Lock(true))
			{
				Add(newPost.Clone());
			}
			
			return newId;
		}
		
		public void UpdateBlogPost(BlogPost existingPost)
		{
			using (Lock(true))
			{
				if (!_postTable.ContainsKey(existingPost.Id))
					throw new XmlRpcServerException(404, "Cannot edit a post that doesn't exist (was it deleted?)");
				
				// pass-by-value semantics
				BlogPost clone = existingPost.Clone();
				_postTable[existingPost.Id] = clone;
				for (int i = 0; i < _postList.Count; i++)
				{
					if (((BlogPost) _postList[i]).Id == clone.Id)
					{
						_postList[i] = clone;
						break;
					}
				}
			}
		}
		
		private IDisposable Lock(bool write)
		{
			if (write)
				_rwLock.AcquireWriterLock(Timeout.Infinite);
			else
				_rwLock.AcquireReaderLock(Timeout.Infinite);
			
			return new LockReleaser(this, write);
		}

		private class LockReleaser : IDisposable
		{
			private readonly Blog _blog;
			private readonly bool _write;

			public LockReleaser(Blog blog, bool write)
			{
				_blog = blog;
				_write = write;
			}

			public void Dispose()
			{
				if (_write)
				{
					_blog.Persist();
					_blog._rwLock.ReleaseWriterLock();
				}
				else
					_blog._rwLock.ReleaseReaderLock();
			}
		}
		
		public class BlogPersist
		{
			private BlogPost[] _blogPosts;

			public BlogPersist()
			{
			}

			public BlogPersist(BlogPost[] blogPosts)
			{
				_blogPosts = blogPosts;
			}

			public BlogPost[] BlogPosts
			{
				get { return _blogPosts; }
				set { _blogPosts = value; }
			}
		}

		public bool DeleteBlogPost(string postid)
		{
			using (Lock(true))
			{
				if (_postTable.ContainsKey(postid))
				{
					BlogPost post = (BlogPost) _postTable[postid];
					Debug.Assert(_postList.Contains(post));
					_postList.Remove(post);
					_postTable.Remove(postid);
					return true;
				}
				return false;
			}
		}
	}
}
