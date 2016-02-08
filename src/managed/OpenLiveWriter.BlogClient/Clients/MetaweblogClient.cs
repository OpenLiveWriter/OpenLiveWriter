// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.BlogClient.Clients
{

    [BlogClient("Metaweblog", "MetaWeblog")]
    public class MetaweblogClient : BloggerCompatibleClient
    {
        public MetaweblogClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(postApiUrl, credentials)
        {
        }

        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsFileUpload = true;
        }

        public override BlogPostCategory[] GetCategories(string blogId)
        {
            if (Options.SupportsHierarchicalCategories)
            {
                return WordPressGetCategories(blogId);
            }
            else if (Options.SupportsCategoriesInline)
            {
                return MetaweblogGetCategories(blogId);
            }
            else
            {
                return MovableTypeGetCategories(blogId);
            }
        }

        public override BlogPostKeyword[] GetKeywords(string blogId)
        {
            if (Options.SupportsGetKeywords)
            {
                return WordPressGetKeywords(blogId);
            }
            return new BlogPostKeyword[] { };
        }

        private BlogPostKeyword[] WordPressGetKeywords(string blogId)
        {
            // call the method
            XmlNode result = CallMethod("wp.getTags",
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            ArrayList keywords = new ArrayList();
            try
            {
                XmlNodeList keywordNodes = result.SelectNodes("array/data/value/struct");
                foreach (XmlNode keywordNode in keywordNodes)
                {
                    string keywordTitle = NodeText(keywordNode.SelectSingleNode("member[name='name']/value"));
                    if (string.IsNullOrEmpty(keywordTitle))
                        keywordTitle = NodeText(keywordNode.SelectSingleNode("member[name='title']/value"));

                    // fix bug 722918: Server Side Tags: Tag with ampersand in retrieved post shows as &amp;
                    keywordTitle = HtmlUtils.UnEscapeEntities(keywordTitle, HtmlUtils.UnEscapeMode.Default);

                    keywords.Add(new BlogPostKeyword(keywordTitle));
                }
            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing GetKeywords response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException("wp.getKeywords", ex.Message, response);
            }

            // return list of categories
            return (BlogPostKeyword[])keywords.ToArray(typeof(BlogPostKeyword));
        }

        private BlogPostCategory[] MetaweblogGetCategories(string blogId)
        {
            // call the method
            XmlNode result = CallMethod("metaWeblog.getCategories",
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            // parse the results
            return ParseCategories(result, "metaWeblog.getCategories");
        }

        private BlogPostCategory[] MovableTypeGetCategories(string blogId)
        {
            return EnhancedGetCategories("mt.getCategoryList", blogId, false);
        }

        private BlogPostCategory[] WordPressGetCategories(string blogId)
        {
            return EnhancedGetCategories("wp.getCategories", blogId, true);
        }

        private BlogPostCategory[] EnhancedGetCategories(string methodName, string blogId, bool allowParent)
        {
            // call the method
            XmlNode result = CallMethod(methodName,
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            // parse the results
            ArrayList categories = new ArrayList();
            try
            {
                XmlNodeList categoryNodes = result.SelectNodes("array/data/value/struct");
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    // parse out the category name
                    string categoryId = NodeText(categoryNode.SelectSingleNode("member[name='categoryId']/value"));
                    string categoryTitle = NodeText(categoryNode.SelectSingleNode("member[name='categoryName']/value"));

                    if (allowParent)
                    {
                        string categoryParent = NodeText(categoryNode.SelectSingleNode("member[name='parentId']/value"));
                        categories.Add(new BlogPostCategory(categoryId, categoryTitle, categoryParent));
                    }
                    else
                    {
                        categories.Add(new BlogPostCategory(categoryId, categoryTitle));
                    }
                }
            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing GetCategories response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException(methodName, ex.Message, response);
            }

            // return list of categories
            return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
        }

        public virtual BlogPostCategory ParseCategory(XmlNode categoryNode)
        {
            // parse out the category name
            //note: spec calls for description, htmlURL, rssURL only
            //but tested metaweblog implementers seem to all have title, and at least MSDN blogs has category id
            //thus, the twisted logic here
            String catID, catName;
            catID = catName = GetNodeValue(categoryNode, "member[name='description']/value");
            String categoryTitle = GetNodeValue(categoryNode, "member[name='title']/value");
            if (categoryTitle != String.Empty && categoryTitle != null)
            {
                catID = catName = categoryTitle;
            }

            // look for various permutations of category-id
            string categoryId = GetNodeValue(categoryNode, "member[name='categoryid']/value");
            if (categoryId == null)
                categoryId = GetNodeValue(categoryNode, "member[name='categoryId']/value");

            // set if we got it
            if (categoryId != String.Empty && categoryId != null)
            {
                catID = categoryId;
            }

            // Some weblog providers (such as Drupal) actually return the mt category struct for
            // metaWeblog.getCategories. In this case the parsing above would have failed to
            // extract either a name or an id. Parse out the values here if necesssary

            // populate the name field if we haven't gotten it another way
            if (catName == null)
            {
                catName = GetNodeValue(categoryNode, "member[name='categoryName']/value");

                // our failsafe for category-id is always category name
                if (catID == null)
                    catID = catName;
            }

            // optional parent specifier
            string parent = GetNodeValue(categoryNode, "member[name='parentId']/value");
            if (parent == null)
                parent = String.Empty;

            // validate (a null category name downstream will result in unexpected error
            // dialogs -- better to show the user an error here indicating that the
            // response was malformed)
            if (catName == null)
                throw new ArgumentException("Category Name Not Specified");

            // add the category
            return new BlogPostCategory(catID, catName, parent);
        }

        private string GetNodeValue(XmlNode xmlNode, string xpath)
        {
            XmlNode nodeValue = xmlNode.SelectSingleNode(xpath);
            if (nodeValue != null)
                return nodeValue.InnerText;
            return null;
        }

        public override BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            // call the method
            XmlNode result = CallMethod("metaWeblog.getRecentPosts",
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true),
                new XmlRpcInt(maxPosts));

            // parse results
            return ParsePosts(result, "metaWeblog.getRecentPosts", includeCategories, now);
        }

        public override string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            bool addCategoriesOutOfBand = AddCategoriesIfNecessary(blogId, post, newCategoryContext);

            string result;
            if (Options.SupportsCategoriesInline)
            {
                result = MetaweblogNewPost(blogId, post, publish);
                if (Options.SupportsHierarchicalCategories)
                {
                    MovableTypeSetPostCategories(result, ArrayHelper.Concat(post.Categories, post.NewCategories));
                }
            }
            else
            {
                result = MovableTypeNewPost(blogId, post, publish);
            }

            // if we succeeded then note addition of categories if appropriate
            if (!addCategoriesOutOfBand)
            {
                foreach (BlogPostCategory category in post.NewCategories)
                    newCategoryContext.NewCategoryAdded(category);
            }

            return result;
        }

        private bool AddCategoriesIfNecessary(string blogId, BlogPost post, INewCategoryContext newCategoryContext)
        {
            // this blog doesn't support adding categories
            if (!Options.SupportsNewCategories)
                return false;

            // no new categories to add
            if (post.NewCategories.Length == 0)
                return false;

            // we support inline category addition and we don't require a special
            // api for heirarchical categories (inline api can't handle parent specification)
            if (Options.SupportsNewCategoriesInline && !Options.SupportsHierarchicalCategories)
                return false;

            // add the categories and update their ids
            ArrayList newCategories = new ArrayList();
            foreach (BlogPostCategory category in post.NewCategories)
            {
                string categoryId = AddCategory(blogId, category);
                BlogPostCategory newCategory = new BlogPostCategory(categoryId, category.Name, category.Parent);
                newCategories.Add(newCategory);
                newCategoryContext.NewCategoryAdded(newCategory);
            }
            post.NewCategories = newCategories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[];
            return true;
        }

        private string MetaweblogNewPost(string blogId, BlogPost post, bool publish)
        {
            // call the method
            XmlNode result = CallMethod("metaWeblog.newPost",
                                         new XmlRpcString(blogId),
                                         new XmlRpcString(Username),
                                         new XmlRpcString(Password, true),
                                         GeneratePostStruct(blogId, post, publish),
                                         new XmlRpcBoolean(publish));

            // return the blog id
            return result.InnerText;
        }

        private string MovableTypeNewPost(string blogId, BlogPost post, bool publish)
        {
            // See http://www.jayallen.org/journey/2003/10/tip_xmlrpc_and_movable_type
            // for explanation of MT's weird publish flag behavior and why it's
            // a good idea to do the song and dance below.

            // create the post (don't publish it yet because
            // we need to add the categories)
            string postId = MetaweblogNewPost(blogId, post, false);

            // add the categories to the post
            MovableTypeSetPostCategories(postId, post.Categories);

            if (publish)
            {
                // call the metaweblog edit post for publishing
                post.Id = postId;
                MetaweblogEditPost(blogId, post, publish);
            }

            // return the post-id
            return postId;
        }

        private void MovableTypeSetPostCategories(string postId, BlogPostCategory[] categories)
        {
            // call method
            XmlNode result = CallMethod("mt.setPostCategories",
                new XmlRpcString(postId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true),
                GetCategoriesArray(categories));

            // confirm we had a successful return value
            Trace.Assert(result.InnerText == "1", "Unexpected error return value from mt.setPostCategories");
        }

        private XmlRpcArray GetCategoriesArray(BlogPostCategory[] categories)
        {
            ArrayList categoryValues = new ArrayList();
            foreach (BlogPostCategory category in categories)
            {
                // create the struct for the category
                XmlRpcStruct categoryStruct = new XmlRpcStruct(
                    new XmlRpcMember[] { new XmlRpcMember("categoryId", category.Id) });

                // add it to the list
                categoryValues.Add(categoryStruct);
            }

            // return the array
            return new XmlRpcArray((XmlRpcValue[])categoryValues.ToArray(typeof(XmlRpcValue)));
        }

        /// <summary>
        /// Edit an existing entry
        /// </summary>
        /// <param name="blog">blog</param>
        /// <param name="postId">post id</param>
        /// <param name="account">account to post to</param>
        /// <param name="entry">entry to post</param>
        /// <param name="publish">publish now?</param>
        /// <returns>was the entry successfully edited</returns>
        public override bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            bool addCategoriesOutOfBand = AddCategoriesIfNecessary(blogId, post, newCategoryContext);

            bool result;
            if (Options.SupportsCategoriesInline)
            {
                result = MetaweblogEditPost(blogId, post, publish);
                if (Options.SupportsHierarchicalCategories)
                    MovableTypeSetPostCategories(post.Id, ArrayHelper.Concat(post.Categories, post.NewCategories));
            }
            else
            {
                result = MovableTypeEditPost(blogId, post, publish);
            }

            // if we succeeded then note addition of categories if appropriate
            if (!addCategoriesOutOfBand)
            {
                foreach (BlogPostCategory category in post.NewCategories)
                    newCategoryContext.NewCategoryAdded(category);
            }

            return result;
        }

        private bool MetaweblogEditPost(string blogId, BlogPost post, bool publish)
        {
            // call the method
            XmlNode result = CallMethod("metaWeblog.editPost",
                                         new XmlRpcString(post.Id),
                                         new XmlRpcString(Username),
                                         new XmlRpcString(Password, true),
                                         GeneratePostStruct(blogId, post, publish),
                                         new XmlRpcBoolean(publish));

            return (result.InnerText == "1");
        }

        private bool MovableTypeEditPost(string blogId, BlogPost post, bool publish)
        {
            // add the categories to the post
            MovableTypeSetPostCategories(post.Id, post.Categories);

            // call the base to do the rest of the edit
            return MetaweblogEditPost(blogId, post, publish);
        }

        public override BlogPost GetPost(string blogId, string postId)
        {
            // call method
            XmlNode result = CallMethod("metaWeblog.getPost",
                new XmlRpcString(postId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            // try get return the post struct
            XmlNode postStruct = result.SelectSingleNode("struct");
            if (postStruct != null)
            {
                return ParseBlogPost(postStruct, true);
            }
            else
            {
                throw new BlogClientInvalidServerResponseException("metaWeblog.getPost", "No post struct returned from server", result.OuterXml);
            }
        }

        public override BlogPost GetPage(string blogId, string pageId)
        {
            if (Options.SupportsPages)
            {
                // call method
                XmlNode result = CallMethod("wp.getPage",
                    new XmlRpcString(blogId),
                    new XmlRpcString(pageId),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true));

                // try get return the post struct
                XmlNode postStruct = result.SelectSingleNode("struct");
                if (postStruct != null)
                {
                    // parse page info (pages don't include categories by design)
                    return ParseBlogPost(postStruct, false);
                }
                else
                {
                    throw new BlogClientInvalidServerResponseException("wp.getPage", "No post struct returned from server", result.OuterXml);
                }
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("GetPage");
            }
        }

        public override BlogPost[] GetPages(string blogId, int maxPages)
        {
            if (Options.SupportsPages)
            {
                // call the method
                XmlNode result = CallMethod("wp.getPages",
                    new XmlRpcString(blogId),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true),
                    new XmlRpcInt(maxPages));

                // parse results
                return ParsePosts(result, "wp.getPages", false, DateTime.Now);
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("GetPages");
            }
        }

        public override PageInfo[] GetPageList(string blogId)
        {
            if (Options.SupportsPages)
            {
                // call the method
                XmlNode result = CallMethod("wp.getPageList",
                    new XmlRpcString(blogId),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true));

                // parse the results
                ArrayList pages = new ArrayList();
                try
                {
                    XmlNodeList pageNodes = result.SelectNodes("array/data/value/struct");
                    foreach (XmlNode pageNode in pageNodes)
                    {
                        // page id
                        string pageId = GetNodeValue(pageNode, "member[name='page_id']/value");
                        if (pageId == null)
                            throw new BlogClientInvalidServerResponseException("wp.getPageInfo", "page_id member of struct not supplied", result.OuterXml);

                        // page title
                        string pageTitle = GetNodeValue(pageNode, "member[name='page_title']/value");
                        if (pageTitle == null)
                            throw new BlogClientInvalidServerResponseException("wp.getPageInfo", "page_title member of struct not supplied", result.OuterXml);

                        // page date published
                        DateTime pageDatePublished = ParseBlogDate(pageNode.SelectSingleNode("member[name='dateCreated']/value"));

                        // page parent
                        string pageParentId = GetNodeValue(pageNode, "member[name='page_parent_id']/value");
                        if (pageParentId == null)
                            pageParentId = String.Empty;

                        // add page
                        pages.Add(new PageInfo(pageId, pageTitle, pageDatePublished, pageParentId));
                    }
                }
                catch (Exception ex)
                {
                    string response = result != null ? result.OuterXml : "(empty response)";
                    Trace.Fail("Exception occurred while parsing GetPageList response: " + response + "\r\n" + ex.ToString());
                    throw new BlogClientInvalidServerResponseException("wp.getPageList", ex.Message, response);
                }

                // return list of pages
                return pages.ToArray(typeof(PageInfo)) as PageInfo[];
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("GetPageList");
            }
        }

        protected override string NewPage(string blogId, BlogPost page, bool publish)
        {
            if (Options.SupportsPages)
            {
                // call the method
                XmlNode result = CallMethod("wp.newPage",
                    new XmlRpcString(blogId),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true),
                    GeneratePostStruct(blogId, page, publish),
                    new XmlRpcBoolean(publish));

                // return the post id
                return result.InnerText;
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("NewPage");
            }
        }

        protected override bool EditPage(string blogId, BlogPost page, bool publish)
        {
            if (Options.SupportsPages)
            {
                // call the method
                XmlNode result = CallMethod("wp.editPage",
                    new XmlRpcString(blogId),
                    new XmlRpcString(page.Id),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true),
                    GeneratePostStruct(blogId, page, publish),
                    new XmlRpcBoolean(publish));

                return (result.InnerText == "1");
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("EditPage");
            }
        }

        public override void DeletePage(string blogId, string pageId)
        {
            if (Options.SupportsPages)
            {
                XmlNode result = CallMethod("wp.deletePage",
                    new XmlRpcString(blogId),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true),
                    new XmlRpcString(pageId));
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("DeletePage");
            }
        }

        public override AuthorInfo[] GetAuthors(string blogId)
        {
            if (Options.SupportsAuthor)
            {
                // call the method
                XmlNode result = CallMethod("wp.getAuthors",
                    new XmlRpcString(blogId),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true));

                // parse the results
                ArrayList authors = new ArrayList();
                try
                {
                    XmlNodeList authorNodes = result.SelectNodes("array/data/value/struct");
                    foreach (XmlNode authorNode in authorNodes)
                    {
                        // page id
                        string authorId = GetNodeValue(authorNode, "member[name='user_id']/value");
                        if (authorId == null)
                            throw new BlogClientInvalidServerResponseException("wp.getAuthors", "user_id member of struct not supplied", result.OuterXml);

                        // page name
                        string authorName = GetNodeValue(authorNode, "member[name='display_name']/value");
                        if (authorName == null)
                            throw new BlogClientInvalidServerResponseException("wp.getAuthors", "user_login member of struct not supplied", result.OuterXml);

                        // add page
                        authors.Add(new AuthorInfo(authorId, authorName));
                    }
                }
                catch (Exception ex)
                {
                    string response = result != null ? result.OuterXml : "(empty response)";
                    Trace.Fail("Exception occurred while parsing GetAuthors response: " + response + "\r\n" + ex.ToString());
                    throw new BlogClientInvalidServerResponseException("wp.getAuthors", ex.Message, response);
                }

                // return list of authors
                return authors.ToArray(typeof(AuthorInfo)) as AuthorInfo[];
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("GetAuthors");
            }
        }

        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            string uploadFileName = uploadContext.FormatFileName(CleanUploadFilename(uploadContext.PreferredFileName));

            // call the method
            XmlNode result;
            using (Stream fileContents = uploadContext.GetContents())
            {
                result = CallMethod("metaWeblog.newMediaObject",
                                             new XmlRpcString(uploadContext.BlogId),
                                             new XmlRpcString(Username),
                                             new XmlRpcString(Password, true),
                                             new XmlRpcStruct(new XmlRpcMember[]
                                                                   {
                                                                       new XmlRpcMember( "name", uploadFileName ),
                                                                       new XmlRpcMember( "type", MimeHelper.GetContentType(Path.GetExtension(uploadContext.PreferredFileName),MimeHelper.APP_OCTET_STREAM )),
                                                                       new XmlRpcMember( "bits", new XmlRpcBase64( StreamHelper.AsBytes(fileContents) ) ),
                                                                   }
                                                ));
            }

            // return the url to the file on the server
            XmlNode urlNode = result.SelectSingleNode("struct/member[name='url']/value");
            if (urlNode != null)
            {
                return urlNode.InnerText;
            }
            else
            {
                throw new BlogClientInvalidServerResponseException("metaWeblog.newMediaObject", "No URL returned from server", result.OuterXml);
            }
        }

        protected virtual string CleanUploadFilename(string filename)
        {
            string cleanFilename = filename.Replace("#", "_"); //avoids bug 494107
            return cleanFilename;
        }

        public override string AddCategory(string blogId, BlogPostCategory category)
        {
            // call the method
            XmlNode result = CallMethod("wp.newCategory",
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true),
                new XmlRpcStruct(new XmlRpcMember[]
                        {
                            new XmlRpcMember("name", category.Name),
                            new XmlRpcMember("parent_id", ParseCategoryParent(category.Parent) ),
                        })
                );

            // return the category id
            return result.InnerText;
        }

        private static int ParseCategoryParent(string parent)
        {
            int val = 0;
            if (!string.IsNullOrEmpty(parent))
            {
                if (!int.TryParse(parent, out val))
                {
                    Trace.Fail("Unexpected non-integer category parent ID: " + parent);
                }
            }
            return val;
        }

        public override BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
        {
            // call the method
            XmlNode result = CallMethod("wp.suggestCategories",
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true),
                new XmlRpcString(partialCategoryName));

            return ParseCategories(result, "wp.suggestCategories");
        }

        private BlogPostCategory[] ParseCategories(XmlNode result, string methodName)
        {
            // parse the results
            ArrayList categories = new ArrayList();
            try
            {
                XmlNodeList categoryNodes = result.SelectNodes("array/data/value/struct");
                foreach (XmlNode categoryNode in categoryNodes)
                    categories.Add(ParseCategory(categoryNode));
            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing " + methodName + " response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException(methodName, ex.Message, response);
            }

            // return list of categories
            return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
        }

        protected virtual XmlRpcStruct GeneratePostStruct(string blogId, BlogPost post, bool publish)
        {

            ArrayList members = new ArrayList();

            // title
            members.Add(new XmlRpcMember("title", new XmlRpcString(GetPostTitleForXmlValue(post))));

            // set the contents
            if (Options.SupportsExtendedEntries && !post.IsPage)
            {
                //set the main and extended contents as separate fields
                members.Add(new XmlRpcMember("description", new XmlRpcString(post.MainContents)));
                members.Add(new XmlRpcMember("mt_text_more", new XmlRpcString(post.ExtendedContents)));
            }
            else
            {
                //merge the main and extended contents into a single field
                string contents = post.MainContents;
                if (post.ExtendedContents != null)
                    contents += post.ExtendedContents;
                members.Add(new XmlRpcMember("description", new XmlRpcString(contents)));
            }

            // allow comments field
            if (Options.SupportsCommentPolicy)
            {
                if (post.CommentPolicy != BlogCommentPolicy.Unspecified)
                {
                    int policy = -1;
                    if (Options.CommentPolicyAsBoolean)
                    {
                        policy = post.CommentPolicy == BlogCommentPolicy.Open ? 1 : Options.AllowPolicyFalseValue;
                    }
                    else
                    {
                        switch (post.CommentPolicy)
                        {
                            case BlogCommentPolicy.None:
                                policy = 0;
                                break;
                            case BlogCommentPolicy.Open:
                                policy = 1;
                                break;
                            case BlogCommentPolicy.Closed:
                                policy = 2;
                                break;
                            default:
                                Trace.Fail("Unexpected blog comment policy: " + post.CommentPolicy.ToString());
                                break;
                        }
                    }

                    if (policy != -1)
                        members.Add(new XmlRpcMember("mt_allow_comments", new XmlRpcInt(policy)));
                }
            }

            // trackback policy field
            if (Options.SupportsPingPolicy)
            {
                if (post.TrackbackPolicy != BlogTrackbackPolicy.Unspecified)
                {
                    members.Add(new XmlRpcMember("mt_allow_pings",
                        new XmlRpcInt((post.TrackbackPolicy == BlogTrackbackPolicy.Allow) ? 1 : Options.AllowPolicyFalseValue)));
                }
            }

            // keywords field
            if (Options.SupportsKeywords)
            {
                members.Add(new XmlRpcMember("mt_keywords", new XmlRpcString(post.Keywords)));
            }

            // slug field
            if (Options.SupportsSlug)
            {
                members.Add(new XmlRpcMember("wp_slug", new XmlRpcString(post.Slug)));
                members.Add(new XmlRpcMember("mt_basename", new XmlRpcString(post.Slug)));
            }

            // password field
            if (Options.SupportsPassword)
            {
                members.Add(new XmlRpcMember("wp_password", new XmlRpcString(post.Password)));
            }

            // author field
            if (Options.SupportsAuthor)
            {
                if (!post.Author.IsEmpty)
                {
                    members.Add(new XmlRpcMember("wp_author_id", new XmlRpcString(post.Author.Id)));
                }
            }

            // page specific fields
            if (post.IsPage)
            {
                if (Options.SupportsPageParent)
                {
                    string pageParentId = !post.PageParent.IsEmpty ? post.PageParent.Id : "0";
                    members.Add(new XmlRpcMember("wp_page_parent_id", new XmlRpcString(pageParentId)));
                }

                if (Options.SupportsPageOrder && (post.PageOrder != String.Empty))
                {
                    members.Add(new XmlRpcMember("wp_page_order", new XmlRpcString(post.PageOrder)));
                }

                if (Options.SupportsPageTrackbacks)
                {
                    AddTrackbacks(post, members);
                }
            }
            // fields that don't apply to pages
            else
            {
                // generate categories for the post
                XmlRpcArray categories = GenerateCategoriesForPost(post);
                if (categories != null)
                    members.Add(new XmlRpcMember("categories", categories));

                // generate post date fields
                GeneratePostDateFields(post, members);

                if (Options.SupportsExcerpt)
                {
                    members.Add(new XmlRpcMember("mt_excerpt", new XmlRpcString(post.Excerpt)));
                }

                // trackback field
                if (Options.SupportsTrackbacks)
                {
                    AddTrackbacks(post, members);
                }
            }

            // return the struct
            return new XmlRpcStruct((XmlRpcMember[])members.ToArray(typeof(XmlRpcMember)));
        }

        protected virtual void GeneratePostDateFields(BlogPost post, ArrayList members)
        {
            if (post.HasDatePublishedOverride && Options.SupportsCustomDate)
            {
                DateTime date = post.DatePublishedOverride;

                DateTime utcDate = date;
                if (Options.UseLocalTime)
                    date = DateTimeHelper.UtcToLocal(date);

                string format = Options.PostDateFormat;
                members.Add(new XmlRpcMember("dateCreated", new XmlRpcFormatTime(date, format)));
                members.Add(new XmlRpcMember("date_created_gmt", new XmlRpcFormatTime(utcDate, format)));
            }
        }

        protected virtual XmlRpcArray GenerateCategoriesForPost(BlogPost post)
        {
            ArrayList categoryXmlValues = new ArrayList();

            foreach (BlogPostCategory category in post.Categories)
                categoryXmlValues.Add(new XmlRpcString(category.Name));

            // if we support adding categories then add them too
            if (Options.SupportsNewCategories)
                foreach (BlogPostCategory category in post.NewCategories)
                    categoryXmlValues.Add(new XmlRpcString(category.Name));

            return new XmlRpcArray((XmlRpcValue[])categoryXmlValues.ToArray(typeof(XmlRpcValue)));
        }

        private XmlRpcArray GenerateNewCategoriesForPost(BlogPostCategory[] newCategories)
        {
            ArrayList categoryValues = new ArrayList();
            foreach (BlogPostCategory category in newCategories)
            {
                // create the struct for the category
                XmlRpcStruct categoryStruct = new XmlRpcStruct(new XmlRpcMember[]
                    {
                        new XmlRpcMember( "description", category.Name ),
                        new XmlRpcMember( "parent", category.Parent)
                    });

                // add it to the list
                categoryValues.Add(categoryStruct);
            }

            // return the array
            return new XmlRpcArray((XmlRpcValue[])categoryValues.ToArray(typeof(XmlRpcValue)));
        }

        protected override BlogClientProviderException ExceptionForFault(string faultCode, string faultString)
        {
            if (faultCode.IndexOf("403", StringComparison.OrdinalIgnoreCase) != -1)
                return new BlogClientAuthenticationException(faultCode, faultString);
            else if (faultCode.IndexOf("3001", StringComparison.OrdinalIgnoreCase) != -1)
                return new BlogClientAccessDeniedException(faultCode, faultString);
            else
                return null;
        }

        private BlogPost[] ParsePosts(XmlNode result, string methodName, bool includeCategories, DateTime? now)
        {
            // parse results
            ArrayList posts = new ArrayList();
            try
            {
                XmlNodeList postNodes = result.SelectNodes("array/data/value/struct");
                foreach (XmlNode postNode in postNodes)
                {
                    BlogPost blogPost = ParseBlogPost(postNode, includeCategories);

                    // If FuturePublishDateWarning is set, then there may be already published (e.g. live) posts that
                    // have a future date, so we will include those as well.
                    if (!now.HasValue || Options.FuturePublishDateWarning || blogPost.DatePublished.CompareTo(now.Value) < 0)
                        posts.Add(blogPost);
                }

            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing " + methodName + " response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException(methodName, ex.Message, response);
            }

            // return list of posts
            return (BlogPost[])posts.ToArray(typeof(BlogPost));
        }

        private BlogPost ParseBlogPost(XmlNode postNode, bool includeCategories)
        {
            // create blog post
            BlogPost blogPost = new BlogPost();

            // get node values
            blogPost.Id = NodeText(postNode.SelectSingleNode("member[name='postid']/value"));
            if (blogPost.Id == String.Empty)
                blogPost.Id = NodeText(postNode.SelectSingleNode("member[name='page_id']/value"));

            SetPostTitleFromXmlValue(blogPost, NodeText(postNode.SelectSingleNode("member[name='title']/value")));

            // attempt to discover the permalink
            blogPost.Permalink = NodeText(postNode.SelectSingleNode("member[name='permaLink']/value"));
            if (blogPost.Permalink == String.Empty)
                blogPost.Permalink = NodeText(postNode.SelectSingleNode("member[name='permalink']/value"));
            if (blogPost.Permalink == String.Empty)
                blogPost.Permalink = NodeText(postNode.SelectSingleNode("member[name='link']/value"));

            // contents and extended contents
            string mainContents = NodeText(postNode.SelectSingleNode("member[name='description']/value"));
            string extendedContents = NodeText(postNode.SelectSingleNode("member[name='mt_text_more']/value"));
            blogPost.SetContents(mainContents, extendedContents);

            // date published
            XmlNode dateCreatedNode = postNode.SelectSingleNode("member[name='date_created_gmt']/value");
            if (dateCreatedNode == null)
                dateCreatedNode = postNode.SelectSingleNode("member[name='dateCreated']/value");
            blogPost.DatePublished = ParseBlogDate(dateCreatedNode);

            // extract comment field
            if (Options.SupportsCommentPolicy)
            {
                string commentPolicy = NodeText(postNode.SelectSingleNode("member[name='mt_allow_comments']/value"));
                if (commentPolicy != String.Empty)
                {
                    switch (commentPolicy)
                    {
                        case "0":
                            blogPost.CommentPolicy = BlogCommentPolicy.None;
                            break;
                        case "1":
                            blogPost.CommentPolicy = BlogCommentPolicy.Open;
                            break;
                        case "2":
                            blogPost.CommentPolicy = BlogCommentPolicy.Closed;
                            break;
                        default:
                            Trace.Fail("Unexpected value for mt_allow_comments: " + commentPolicy);
                            break;
                    }
                }
            }

            // extract trackback field
            if (Options.SupportsPingPolicy)
            {
                string allowTrackbacks = NodeText(postNode.SelectSingleNode("member[name='mt_allow_pings']/value"));
                if (allowTrackbacks != String.Empty)
                    blogPost.TrackbackPolicy = (allowTrackbacks == "1") ? BlogTrackbackPolicy.Allow : BlogTrackbackPolicy.Deny;
            }

            // extract keywords field
            if (Options.SupportsKeywords)
            {
                string keywords = NodeText(postNode.SelectSingleNode("member[name='mt_keywords']/value"));
                blogPost.Keywords = HtmlUtils.UnEscapeEntities(keywords, HtmlUtils.UnEscapeMode.Default);
            }

            // extract excerpt field
            if (Options.SupportsExcerpt)
            {
                blogPost.Excerpt = NodeText(postNode.SelectSingleNode("member[name='mt_excerpt']/value"));
            }

            // extract ping url array
            if (Options.SupportsTrackbacks)
            {

                XmlNode pingUrlsNode = postNode.SelectSingleNode("member[name='mt_tb_ping_urls']/value");
                if (pingUrlsNode != null)
                {
                    ArrayList sentPingUrls = new ArrayList();
                    if (Options.TrackbackDelimiter == TrackbackDelimiter.ArrayElement)
                    {
                        XmlNodeList pingUrlNodes = pingUrlsNode.SelectNodes("array/data/value");
                        foreach (XmlNode node in pingUrlNodes)
                            sentPingUrls.Add(node.InnerText.Trim());
                    }
                    else
                    {
                        string delimiter = null;
                        if (Options.TrackbackDelimiter == TrackbackDelimiter.Space)
                            delimiter = " ";
                        else if (Options.TrackbackDelimiter == TrackbackDelimiter.Comma)
                            delimiter = ",";
                        sentPingUrls.AddRange(StringHelper.Split(pingUrlsNode.InnerText, delimiter));
                    }

                    blogPost.PingUrlsSent = sentPingUrls.ToArray(typeof(string)) as string[];
                }
            }

            // extract slug
            if (Options.SupportsSlug)
            {
                blogPost.Slug = NodeText(postNode.SelectSingleNode("member[name='wp_slug']/value"));
                if (blogPost.Slug == String.Empty)
                    blogPost.Slug = NodeText(postNode.SelectSingleNode("member[name='mt_basename']/value"));
            }

            // extract password
            if (Options.SupportsPassword)
            {
                blogPost.Password = NodeText(postNode.SelectSingleNode("member[name='wp_password']/value"));
            }

            // extract author
            if (Options.SupportsAuthor)
            {
                string authorId = NodeText(postNode.SelectSingleNode("member[name='wp_author_id']/value"));

                // account for different display name syntax in various calls
                string authorName = NodeText(postNode.SelectSingleNode("member[name='wp_author_display_name']/value"));

                blogPost.Author = new PostIdAndNameField(authorId, authorName);
            }

            // extract page parent
            if (Options.SupportsPageParent)
            {
                string pageParentId = NodeText(postNode.SelectSingleNode("member[name='wp_page_parent_id']/value"));
                // convert 0 to empty string for parent-id
                if (pageParentId == "0")
                    pageParentId = String.Empty;

                string pageParentTitle = NodeText(postNode.SelectSingleNode("member[name='wp_page_parent_title']/value"));
                blogPost.PageParent = new PostIdAndNameField(pageParentId, pageParentTitle);
            }

            // extract page order
            if (Options.SupportsPageOrder)
            {
                blogPost.PageOrder = NodeText(postNode.SelectSingleNode("member[name='wp_page_order']/value"));
            }

            // extract categories
            if (includeCategories && Options.SupportsCategories)
            {
                if (Options.SupportsCategoriesInline && !Options.SupportsHierarchicalCategories)
                {
                    blogPost.Categories = MetaweblogExtractCategories(blogPost.Id, postNode);
                }
                else
                {
                    blogPost.Categories = MovableTypeExtractCategories(blogPost.Id, postNode);
                }
            }

            // allow post-processing of extracted blog post
            BlogPostReadFilter(blogPost);

            // return the post
            return blogPost;
        }

        private BlogPostCategory[] MetaweblogExtractCategories(string postId, XmlNode postNode)
        {
            // extract categories from the post
            ArrayList categories = new ArrayList();
            XmlNode categoriesNode = postNode.SelectSingleNode("member[name='categories']/value");
            if (categoriesNode != null)
            {
                XmlNodeList categoryNodes = categoriesNode.SelectNodes("array/data/value");
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    categories.Add(
                        new BlogPostCategory(categoryNode.InnerText, categoryNode.InnerText));
                }
            }

            return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
        }

        private BlogPostCategory[] MovableTypeExtractCategories(string postId, XmlNode postNode)
        {
            // call the method
            XmlNode result = CallMethod("mt.getPostCategories",
                new XmlRpcString(postId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            // parse out the data
            ArrayList categories = new ArrayList();
            XmlNodeList categoryNodes = result.SelectNodes("array/data/value/struct");
            if (categoryNodes != null)
            {
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    // parse out the category name
                    XmlNode categoryId = categoryNode.SelectSingleNode("member[name='categoryId']/value");
                    XmlNode categoryTitle = categoryNode.SelectSingleNode("member[name='categoryName']/value");

                    // add the category
                    categories.Add(new BlogPostCategory(categoryId.InnerText, categoryTitle.InnerText));
                }

                return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
            }
            else
            {
                throw new BlogClientInvalidServerResponseException("mt.getPostCategories", "No categories returned from server", result.OuterXml);
            }
        }


        protected virtual void BlogPostReadFilter(BlogPost blogPost)
        {
        }

        private void AddTrackbacks(BlogPost post, ArrayList members)
        {
            if (post.PingUrlsPending.Length > 0)
            {
                if (Options.TrackbackDelimiter == TrackbackDelimiter.ArrayElement)
                {
                    members.Add(new XmlRpcMember("mt_tb_ping_urls", ArrayFromStrings(post.PingUrlsPending)));
                }
                else
                {
                    char delimiter;
                    switch (Options.TrackbackDelimiter)
                    {
                        case TrackbackDelimiter.Space:
                            delimiter = ' ';
                            break;
                        case TrackbackDelimiter.Comma:
                        default:
                            delimiter = ',';
                            break;
                    }
                    StringBuilder trackbackBuilder = new StringBuilder();
                    foreach (string pingUrl in post.PingUrlsPending)
                        trackbackBuilder.AppendFormat("{0}{1}", pingUrl, delimiter);
                    string trackbacks = trackbackBuilder.ToString().TrimEnd(delimiter);

                    members.Add(new XmlRpcMember("mt_tb_ping_urls", trackbacks));
                }
            }
        }
    }
}
