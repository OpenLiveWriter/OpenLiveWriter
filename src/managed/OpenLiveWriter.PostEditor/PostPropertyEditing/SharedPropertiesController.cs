// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl;
using System.Globalization;
using System.Threading;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    /// <summary>
    /// This class has all of the controller logic that's common to both
    /// PostPropertiesBandControl and PostPropertiesForm.
    /// </summary>
    internal class SharedPropertiesController : IBlogPostEditor, INewCategoryContext
    {
        private readonly string DATETIME_PICKER_PROMPT;

        private string _blogId;
        private Blog _targetBlog;
        private IBlogClientOptions _clientOptions;
        private IBlogPostEditingContext editorContext;

        private delegate void SyncAction(SharedPropertiesController other);

        private bool isDirty;

        /// <summary>
        /// When non-zero, suppress marking ourselves dirty and syncing
        /// with the "other" controller.
        /// </summary>
        private int suspendCount = 0;

        private readonly IWin32Window parentFrame;
        private readonly Label labelCategory;
        private readonly CategoryDropDownControlM1 categoryDropDown;
        private readonly Label labelTags;
        private readonly AutoCompleteTextbox textTags;
        private readonly Label labelPageOrder;
        private readonly NumericTextBox textPageOrder;
        private readonly Label labelPageParent;
        private readonly PageParentComboBox comboPageParent;
        private readonly Label labelPublishDate;
        private readonly PublishDateTimePicker datePublishDate;
        private readonly List<PropertyField> fields;

        public SharedPropertiesController(IWin32Window parentFrame, Label labelCategory, CategoryDropDownControlM1 categoryDropDown, Label labelTags, AutoCompleteTextbox textTags,
            Label labelPageOrder, NumericTextBox textPageOrder, Label labelPageParent, PageParentComboBox comboPageParent, Label labelPublishDate, PublishDateTimePicker datePublishDate, List<PropertyField> fields,
            CategoryContext categoryContext)
        {
            this.parentFrame = parentFrame;
            this.labelCategory = labelCategory;
            this.categoryDropDown = categoryDropDown;
            this.labelPublishDate = labelPublishDate;
            this.datePublishDate = datePublishDate;
            this.fields = fields;
            this.labelTags = labelTags;
            this.textTags = textTags;
            this.labelPageOrder = labelPageOrder;
            this.textPageOrder = textPageOrder;
            this.labelPageParent = labelPageParent;
            this.comboPageParent = comboPageParent;

            this.categoryDropDown.AccessibleName = Res.Get(StringId.CategoryControlCategories).Replace("{0}", " ").Trim();
            this.datePublishDate.AccessibleName = Res.Get(StringId.PropertiesPublishDate);

            textTags.TextChanged += MakeDirty;
            textTags.ButtonClicked += (sender, args) =>
            {
                _targetBlog.RefreshKeywords();
                LoadTagValues();
            };

            comboPageParent.SelectedIndexChanged += MakeDirty;
            textPageOrder.TextChanged += MakeDirty;

            datePublishDate.ValueChanged += MakeDirty;

            DATETIME_PICKER_PROMPT = "'" + Res.Get(StringId.PublishDatePrompt).Replace("'", "''") + "'";
            // HACK ALERT - WinLive 209226: For RTL languages, we need to reverse the string, so it displays in the right direction
            // when right aligned. We need this hack because we are essentially using the CustomFormat string to display
            // the cue text. The control assumes this to be a valid format string and does a blind reverse on RTL languages
            // to mirror the date formatting. Unfortunately we don't need that mirroring for the cue text.
            // We reverse only if either the system (control panel setting) or UI is RTL, not if both are RTL.
            bool isUIRightToLeft = BidiHelper.IsRightToLeft;
            bool isSystemRightToLeft = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft;
            if ((isUIRightToLeft && !isSystemRightToLeft) || (!isUIRightToLeft && isSystemRightToLeft))
            {
                // Reverse the string
                char[] charArray = DATETIME_PICKER_PROMPT.ToCharArray();
                Array.Reverse(charArray);
                DATETIME_PICKER_PROMPT = new string(charArray);
            }
            datePublishDate.CustomFormat = DATETIME_PICKER_PROMPT;

            categoryDropDown.Initialize(parentFrame, categoryContext);

            InitializeFields();
            InitializeSync();
        }

        public SharedPropertiesController Other
        {
            get; set;
        }

        private void SyncChange(SyncAction action)
        {
            if (suspendCount > 0 || Other == null)
                return;

            using (Other.SuspendLogic())
                action(Other);
        }

        private void InitializeFields()
        {
            RegisterField(PropertyType.Page, labelPageOrder, textPageOrder,
              opts => opts.SupportsPageOrder,
              post => textPageOrder.Text = post.PageOrder,
              post => post.PageOrder = textPageOrder.Text);

            RegisterField2(PropertyType.Page, labelPageParent, comboPageParent,
                          opts => opts.SupportsPageParent,
                          (ctx, opts) =>
                          {
                              PostIdAndNameField none = new PostIdAndNameField(Res.Get(StringId.PropertiesNoPageParent));
                              PostIdAndNameField parent = ctx.BlogPost.PageParent;
                              BlogPageFetcher pageFetcher = new BlogPageFetcher(_blogId, 10000);
                              if (!parent.Equals(PostIdAndNameField.Empty))
                              {
                                  comboPageParent.Initialize(new object[] { none, parent }, parent, pageFetcher);
                              }
                              else
                              {
                                  comboPageParent.Initialize(none, pageFetcher);
                              }
                          },
                          post => post.PageParent = comboPageParent.SelectedItem as PostIdAndNameField);

            RegisterField(PropertyType.Post, labelCategory, categoryDropDown,
                          opts => opts.SupportsCategories,
                          post => { },
                          post => { });

            RegisterField2(PropertyType.Post, labelTags, textTags,
                          opts => opts.SupportsKeywords,
                          (ctx, opts) => textTags.Text = ctx.BlogPost.Keywords,
                          post => post.Keywords = textTags.TextValue);

            RegisterField(PropertyType.Post, labelPublishDate, datePublishDate,
                          opts => opts.SupportsCustomDate,
                          post =>
                          {
                              if (post.HasDatePublishedOverride)
                              {
                                  DatePublishedOverride = post.DatePublishedOverride;
                                  HasDatePublishedOverride = true;
                              }
                              else
                              {
                                  HasDatePublishedOverride = false;
                              }
                          },
                          post => post.DatePublishedOverride = HasDatePublishedOverride ? DatePublishedOverride : DateTime.MinValue);

        }

        private void InitializeSync()
        {
            textPageOrder.TextChanged += (sender, args) => SyncChange(other => other.textPageOrder.Text = textPageOrder.Text);

            textTags.TextChanged += (sender, args) => SyncChange(other => other.textTags.Text = textTags.TextValue);

            comboPageParent.SelectedIndexChanged +=
                (sender, args) =>
                {
                    SyncChange(other =>
                    {
                        other.comboPageParent.BeginUpdate();
                        try
                        {
                            other.comboPageParent.Items.Clear();
                            foreach (object o in comboPageParent.Items)
                                other.comboPageParent.Items.Add(o);
                            other.comboPageParent.SelectedIndex = comboPageParent.SelectedIndex;
                        }
                        finally
                        {
                            other.comboPageParent.EndUpdate();
                        }
                    });
                };

            datePublishDate.ValueChanged2 += HandlePublishDateValueChanged;
        }

        private void HandlePublishDateValueChanged(object sender, EventArgs args)
        {
            SyncChange(other =>
            {
                UpdateDateTimePickerFormat();
                // We are syncing to the 'other' control, don't trigger a ValueChanged2 event on it (to prevent
                // a reverse sync)
                other.datePublishDate.SetDateTimeAndChecked(datePublishDate.Checked, datePublishDate.Value);
                other.UpdateDateTimePickerFormat();
                other.datePublishDate.Tag = datePublishDate.Tag;
            });
        }

        private void RegisterField(PropertyType type, Control label, Control editor, PropertyField.ShouldShow shouldShow, PropertyField.Populate populate, PropertyField.Save save)
        {
            PropertyField field = new PropertyField(type, new[] { label, editor }, shouldShow, populate, save);
            fields.Add(field);
        }

        private void RegisterField2(PropertyType type, Control label, Control editor, PropertyField.ShouldShow shouldShow, PropertyField.PopulateFull populate, PropertyField.Save save)
        {
            PropertyField field = new PropertyField(type, new[] { label, editor }, shouldShow, populate, save);
            fields.Add(field);
        }

        private void LoadTagValues()
        {
            if (_targetBlog != null)
            {
                // Get all the keywords for the blog and set it as the source for the autocomplete
                List<string> keywords = new List<string>();
                BlogPostKeyword[] keywordList = _targetBlog.Keywords;
                if (_clientOptions.SupportsGetKeywords && keywordList != null)
                {
                    foreach (BlogPostKeyword blogPostKeyword in keywordList)
                    {
                        keywords.Add(blogPostKeyword.Name);
                    }
                }

                textTags.TagSource = new SimpleTagSource(keywords);

                SyncChange(other => other.LoadTagValues());
            }
        }

        public void Initialize(IBlogPostEditingContext context, IBlogClientOptions clientOptions)
        {
            Debug.Assert(_blogId == context.BlogId);

            using (SuspendLogic())
            {
                editorContext = context;

                using (Blog blog = new Blog(context.BlogId))
                    UpdateFieldsForBlog(blog);

                ((IBlogPostEditor)categoryDropDown).Initialize(context, clientOptions);

                foreach (PropertyField field in fields)
                    field.Initialize(context, clientOptions);
            }

            isDirty = false;
        }

        public void OnBlogChanged(Blog newBlog)
        {
            using (SuspendLogic())
            {
                _blogId = newBlog.Id;
                _targetBlog = newBlog;
                _clientOptions = newBlog.ClientOptions;

                ((IBlogPostEditor)categoryDropDown).OnBlogChanged(newBlog);
                UpdateFieldsForBlog(newBlog);

                // Tag stuff
                textTags.ShowButton = newBlog.ClientOptions.SupportsGetKeywords;
                if (newBlog.ClientOptions.SupportsGetKeywords)
                    LoadTagValues();

                textTags.DefaultText = newBlog.ClientOptions.KeywordsAsTags ? Res.Get(StringId.TagControlSetTags) : "";

            }
        }

        private void UpdateFieldsForBlog(Blog newBlog)
        {
            if (editorContext != null)
            {
                foreach (PropertyField field in fields)
                    field.UpdateControlVisibility(editorContext, newBlog.ClientOptions);
            }
        }

        public void OnBlogSettingsChanged(bool templateChanged)
        {
            using (SuspendLogic())
            {
                ((IBlogPostEditor)categoryDropDown).OnBlogSettingsChanged(templateChanged);
                if (editorContext != null)
                    using (Blog blog = new Blog(_blogId))
                        UpdateFieldsForBlog(blog);
            }
        }

        public bool IsDirty
        {
            get { return isDirty || ((IBlogPostEditor)categoryDropDown).IsDirty; }
        }

        public void MakeDirty(object sender, EventArgs args)
        {
            if (suspendCount <= 0)
                isDirty = true;
        }

        public IDisposable SuspendLogic()
        {
            suspendCount++;
            return new Unsuspender { Parent = this };
        }
        private class Unsuspender : IDisposable
        {
            public SharedPropertiesController Parent;
            public void Dispose() { Parent.suspendCount--; }
        }

        public void SaveChanges(BlogPost post, BlogPostSaveOptions options)
        {
            using (SuspendLogic())
            {
                ((IBlogPostEditor)categoryDropDown).SaveChanges(post, options);

                foreach (PropertyField field in fields)
                    field.SaveChanges(post);
            }
            isDirty = false;
        }

        public bool ValidatePublish()
        {
            if (!((IBlogPostEditor)categoryDropDown).ValidatePublish())
                return false;

            if (datePublishDate.Checked && (DatePublishedOverride > DateTimeHelper.UtcNow) && _clientOptions.FuturePublishDateWarning && PostEditorSettings.FuturePublishDateWarning)
            {
                using (FutureDateWarningForm warningForm = new FutureDateWarningForm())
                {
                    if (warningForm.ShowDialog(parentFrame) != DialogResult.Yes)
                        return false;
                }
            }

            return true;
        }

        public bool HasDatePublishedOverride
        {
            get
            {
                return datePublishDate.Checked;
            }
            set
            {
                datePublishDate.Checked = value;
                UpdateDateTimePickerFormat();
            }
        }

        public DateTime DatePublishedOverride
        {
            get
            {
                DateTime dateTime = DateTimeHelper.LocalToUtc(datePublishDate.Value);
                dateTime = dateTime.Subtract(TimeSpan.FromSeconds(dateTime.Second) + TimeSpan.FromMilliseconds(dateTime.Millisecond));
                return dateTime;
            }
            set
            {
                datePublishDate.Tag = true; // Signal the datetime picker has a value
                // Set the datetime and checked state without triggering the PublishDateTimePicker.ValueChanged2 event
                datePublishDate.SetDateTimeAndChecked(datePublishDate.Checked, DateTimeHelper.UtcToLocal(value));
            }
        }

        private void UpdateDateTimePickerFormat()
        {
            if (datePublishDate.Checked)
            {
                // if we are transitioning from unchecked to checked then
                // set the current date-time to Now
                if ((bool)datePublishDate.Tag == false)
                    DatePublishedOverride = DateTime.UtcNow;

                datePublishDate.Font = defaultFont;
                datePublishDate.CustomFormat = CultureHelper.GetDateTimeCombinedPattern(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, CultureHelper.GetShortDateTimePatternForDateTimePicker());
            }
            else
            {
                datePublishDate.CustomFormat = DATETIME_PICKER_PROMPT;
                datePublishDate.Tag = false; // signal the date time picker no longer has a value
                datePublishDate.Font = defaultFont;
            }
        }

        private Font defaultFont = Res.DefaultFont;

        public void OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {
            ((IBlogPostEditor)categoryDropDown).OnPublishSucceeded(blogPost, postResult);

            List<BlogPostKeyword> keywords = new List<BlogPostKeyword>();
            keywords.AddRange(_targetBlog.Keywords);
            string[] keywordList = blogPost.Keywords.Split(',');
            foreach (string str in keywordList)
            {
                string name = str.Trim();

                if (string.IsNullOrEmpty(name))
                    continue;

                keywords.Add(new BlogPostKeyword(name));
            }
            _targetBlog.Keywords = keywords.ToArray();

            LoadTagValues();

            isDirty = false;
        }

        public void OnClosing(CancelEventArgs e)
        {
            categoryDropDown.OnPostClosing(e);
        }

        public void OnPostClosing(CancelEventArgs e)
        {
            categoryDropDown.OnPostClosing(e);
        }

        public void OnClosed()
        {
            categoryDropDown.OnClosed();
        }

        public void OnPostClosed()
        {
            categoryDropDown.OnPostClosed();
        }

        public void NewCategoryAdded(BlogPostCategory category)
        {
            ((INewCategoryContext)categoryDropDown).NewCategoryAdded(category);
        }
    }

    [Flags]
    internal enum PropertyType
    {
        Post = 1,
        Page = 2,
        Both = Post | Page
    }

    internal class PropertyField
    {
        public delegate bool ShouldShow(IBlogClientOptions options);
        public delegate void Populate(BlogPost bp);
        public delegate void PopulateFull(IBlogPostEditingContext ctx, IBlogClientOptions opts);
        public delegate void Save(BlogPost bp);

        private PropertyType propertyType;
        private Control[] controls;
        private ShouldShow shouldShow;
        private PopulateFull populate;
        private Save save;

        public PropertyField(PropertyType type, Control[] controls, ShouldShow shouldShow, Populate populate, Save save)
            : this(type, controls, shouldShow, (ctx, opts) => populate(ctx.BlogPost), save)
        {
        }

        public PropertyField(PropertyType type, Control[] controls, ShouldShow shouldShow, PopulateFull populate, Save save)
        {
            this.propertyType = type;
            this.controls = controls;
            this.shouldShow = shouldShow;
            this.populate = populate;
            this.save = save;
        }

        public void Initialize(IBlogPostEditingContext editingContext, IBlogClientOptions options)
        {
            PropertyType requiredType = editingContext.BlogPost.IsPage ? PropertyType.Page : PropertyType.Post;

            bool isCorrectType = (propertyType & requiredType) == requiredType;
            if (isCorrectType && populate != null)
                populate(editingContext, options);
        }

        public void UpdateControlVisibility(IBlogPostEditingContext editingContext, IBlogClientOptions options)
        {
            PropertyType requiredType = editingContext.BlogPost.IsPage ? PropertyType.Page : PropertyType.Post;

            bool isCorrectType = (propertyType & requiredType) == requiredType;
            bool visible = isCorrectType && shouldShow(options);
            foreach (Control c in controls)
                if (c != null)
                    c.Visible = visible;
        }

        public void SaveChanges(BlogPost post)
        {
            PropertyType requiredType = post.IsPage ? PropertyType.Page : PropertyType.Post;

            bool isCorrectType = (propertyType & requiredType) == requiredType;
            if (isCorrectType && save != null)
                save(post);
        }
    }

    internal class BlogAuthorFetcher : BlogDelayedFetchHandler
    {
        public BlogAuthorFetcher(string blogId, int timeoutMs)
            : base(blogId, Res.Get(StringId.PropertiesAuthorsFetchingText), timeoutMs)
        {
        }

        protected override object BlogFetchOperation(Blog blog)
        {
            // refresh our author list
            blog.RefreshAuthors();

            // return what we've got
            return ConvertAuthors(blog.Authors);
        }

        protected override object[] GetDefaultItems(Blog blog)
        {
            return ConvertAuthors(blog.Authors);
        }

        private object[] ConvertAuthors(AuthorInfo[] authorList)
        {
            ArrayList authors = new ArrayList();
            foreach (AuthorInfo author in authorList)
            {
                authors.Add(new PostIdAndNameField(author.Id, author.Name));
            }
            return authors.ToArray();
        }
    }

    internal abstract class BlogDelayedFetchHandler : IDelayedFetchHandler
    {
        protected BlogDelayedFetchHandler(string blogId, string fetchingText, int timeoutMs)
        {
            _blogId = blogId;
            _fetchingText = fetchingText;
            _timeoutMs = timeoutMs;
        }

        public object[] FetchItems(IWin32Window owner)
        {
            object[] items = BlogClientHelper.PerformBlogOperationWithTimeout(_blogId, new BlogClientOperation(BlogFetchOperation), _timeoutMs) as object[];
            if (items != null)
            {
                return items;
            }
            else
            {
                // see if we have default items available
                object[] defaultItems = new object[0];
                try
                {
                    using (Blog blog = new Blog(_blogId))
                        defaultItems = GetDefaultItems(blog);
                }
                catch { }

                // if we have default items then return them in preference to showing an error
                if (defaultItems.Length > 0)
                {
                    return defaultItems;
                }
                else
                {
                    DisplayMessage.Show(MessageId.UnableToRetrieve, _fetchingText);
                    return null;
                }
            }
        }

        protected abstract object BlogFetchOperation(Blog blog);
        protected virtual object[] GetDefaultItems(Blog blog) { return new object[0]; }

        private readonly string _blogId;
        private readonly int _timeoutMs;
        private readonly string _fetchingText;
    }

}
