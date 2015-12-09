// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using System.Diagnostics;
using OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    using Localization.Bidi;

    // TODO: Page Parent won't clear properly on switching blogs (does that even work? or do pages get converted to posts on switch?)

    internal partial class PostPropertiesForm : ApplicationDialog, IBlogPostEditor, INewCategoryContext
    {
        private SharedPropertiesController controller;

        private string _blogId;
        private List<PropertyField> fields = new List<PropertyField>();

        private readonly CommandManager commandManager;

        internal PostPropertiesForm(CommandManager commandManager, CategoryContext categoryContext)
        {
            this.commandManager = commandManager;
            InitializeComponent();

            this.Name = "PostPropertiesForm";

            Text = Res.Get(StringId.PostProperties);
            AccessibleName = Text;
            buttonClose.Text = Res.Get(StringId.CloseButtonNoMnemonic);

            this.labelPageOrder.Text = Res.Get(StringId.PropertiesPageOrder);
            this.labelPageParent.Text = Res.Get(StringId.PropertiesPageParent);
            this.labelCategories.Text = Res.Get(StringId.PropertiesCategories);
            this.labelPublishDate.Text = Res.Get(StringId.PropertiesPublishDate);
            this.labelSlug.Text = Res.Get(StringId.PropertiesSlug);
            this.labelComments.Text = Res.Get(StringId.PropertiesComments);
            this.labelPings.Text = Res.Get(StringId.PropertiesPings);
            this.labelTags.Text = Res.Get(StringId.PropertiesKeywords);
            this.labelAuthor.Text = Res.Get(StringId.PropertiesAuthor);
            this.labelPassword.Text = Res.Get(StringId.PropertiesPassword);
            this.labelTrackbacks.Text = Res.Get(StringId.PropertiesTrackbacks);
            this.labelExcerpt.Text = Res.Get(StringId.PropertiesExcerpt);
            this.textPassword.PasswordChar = Res.PasswordChar;

            ControlHelper.SetCueBanner(textTrackbacks, Res.Get(StringId.PropertiesCommaSeparated));

            SuppressAutoRtlFixup = true;

            comboComments.Items.AddRange(new[] {
                new CommentComboItem(BlogCommentPolicy.Unspecified),
                new CommentComboItem(BlogCommentPolicy.None),
                new CommentComboItem(BlogCommentPolicy.Open),
                new CommentComboItem(BlogCommentPolicy.Closed),
            });

            comboPings.Items.AddRange(new[] {
                new TrackbackComboItem(BlogTrackbackPolicy.Unspecified),
                new TrackbackComboItem(BlogTrackbackPolicy.Allow),
                new TrackbackComboItem(BlogTrackbackPolicy.Deny),
            });

            InitializePropertyFields();

            controller = new SharedPropertiesController(
                this, labelCategories, categoryDropDown, labelTags, textTags, labelPageOrder, textPageOrder,
                labelPageParent, comboPageParent, labelPublishDate, datePublishDate, fields, categoryContext);

            this.comboComments.SelectedIndexChanged += controller.MakeDirty;
            this.comboPings.SelectedIndexChanged += controller.MakeDirty;
            this.textSlug.TextChanged += controller.MakeDirty;
            this.textPassword.TextChanged += controller.MakeDirty;
            this.textExcerpt.TextChanged += controller.MakeDirty;
            this.textTrackbacks.TextChanged += controller.MakeDirty;

            SimpleTextEditorCommandHelper.UseNativeBehaviors(commandManager,
                                                             textExcerpt, textPageOrder, textPassword,
                                                             textSlug, textTags, textTrackbacks);
        }

        public void DisplayCategoryForm()
        {
            categoryDropDown.DisplayCategoryForm();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (commandManager.ProcessCmdKeyShortcut(keyData))
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // WinLive 53092: Avoid horizontal scrolling for languages with long label names.
            int maxLabelWidth = panel1.Width;
            foreach (Control control in flowLayoutPanel.Controls)
            {
                maxLabelWidth = Math.Max(maxLabelWidth, control.PreferredSize.Width);
            }
            this.Width += maxLabelWidth - panel1.Width;

            // WinLive 52981: Avoid clipping the author combobox for languages with tall comboboxes.
            this.panelComboAuthorIsolate.Height = this.comboAuthor.PreferredSize.Height;

            DisplayHelper.AutoFitSystemButton(buttonClose);

            if (Owner != null)
            {
                this.Left = BidiHelper.IsRightToLeft
                                ? this.Owner.Left + SystemInformation.VerticalScrollBarWidth * 2
                                : this.Owner.Right - this.Width - SystemInformation.VerticalScrollBarWidth * 2;

                Top = Owner.Top + 100;
            }
        }

        private void InitializePropertyFields()
        {
            RegisterField(PropertyType.Both, labelComments, comboComments,
                          opts => opts.SupportsCommentPolicy,
                          post => comboComments.SelectedItem = new CommentComboItem(post.CommentPolicy),
                          post => post.CommentPolicy = ((CommentComboItem)comboComments.SelectedItem).Value);

            RegisterField(PropertyType.Both, labelPings, comboPings,
                          opts => opts.SupportsPingPolicy,
                          post => comboPings.SelectedItem = new TrackbackComboItem(post.TrackbackPolicy),
                          post => post.TrackbackPolicy = ((TrackbackComboItem)comboPings.SelectedItem).Value);

            RegisterField(PropertyType.Both, labelAuthor, panelComboAuthorIsolate,
                          opts => opts.SupportsAuthor,
                          post => SetAuthor(post.Author),
                          post => post.Author = (PostIdAndNameField)comboAuthor.SelectedItem);

            RegisterField(PropertyType.Both, labelSlug, textSlug,
                          opts => opts.SupportsSlug,
                          post => textSlug.Text = post.Slug,
                          post => post.Slug = textSlug.Text);

            RegisterField(PropertyType.Both, labelPassword, textPassword,
                          opts => opts.SupportsPassword,
                          post => textPassword.Text = post.Password,
                          post => post.Password = textPassword.Text);

            RegisterField(PropertyType.Post, labelExcerpt, textExcerpt,
                          opts => opts.SupportsExcerpt,
                          post => textExcerpt.Text = post.Excerpt,
                          post => post.Excerpt = textExcerpt.Text);

            RegisterField(PropertyType.Post, labelTrackbacks, textTrackbacks,
                          opts => opts.SupportsTrackbacks,
                          post =>
                              textTrackbacks.Text = StringHelper.Join(ArrayHelper.Union(post.PingUrlsPending, post.PingUrlsSent), ", "),
                          post =>
                          {
                              List<string> sent = new List<string>(post.PingUrlsSent);
                              List<string> pending = new List<string>();
                              string[] pingUrls = StringHelper.Split(textTrackbacks.Text.Trim(), ",");
                              foreach (string url in pingUrls)
                                  if (!sent.Contains(url))
                                      pending.Add(url);
                              post.PingUrlsPending = pending.ToArray();
                          });
        }

        private void PostPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None)
            {
                e.Cancel = true;
                Hide();
                if (Owner != null && Owner.Visible)
                    Owner.Activate();
            }
        }

        private void flowLayoutPanel_ClientSizeChanged(object sender, EventArgs e)
        {
            // panel1 is used to make all the other controls stretch to fill the width of the flowLayoutPanel.  See
            // the MSDN article "How to: Anchor and Dock Child Controls in a FlowLayoutPanel Control" at
            // http://msdn.microsoft.com/en-us/library/ms171633(VS.80).aspx
            if (flowLayoutPanel.VerticalScroll.Visible)
                // The DisplayHelper.ScaleX(12) call is used to add horizontal padding between the controls in the
                // flowLayoutPanel and the vertical scroll bar.
                panel1.Width = flowLayoutPanel.ClientSize.Width - Convert.ToInt32(DisplayHelper.ScaleX(12));
            else
                panel1.Width = flowLayoutPanel.ClientSize.Width;
        }

        private void RegisterField(PropertyType type, Control label, Control editor, PropertyField.ShouldShow shouldShow, PropertyField.Populate populate, PropertyField.Save save)
        {
            PropertyField field = new PropertyField(type, new[] { label, editor }, shouldShow, populate, save);
            fields.Add(field);
        }

        private void SetAuthor(PostIdAndNameField author)
        {
            if (author != null)
            {
                BlogAuthorFetcher authorFetcher = new BlogAuthorFetcher(_blogId, 10000);
                if (!author.IsEmpty)
                    comboAuthor.Initialize(new object[] { new PostIdAndNameField(Res.Get(StringId.PropertiesDefault)), author }, author, authorFetcher);
                else
                    comboAuthor.Initialize(new PostIdAndNameField(Res.Get(StringId.PropertiesDefault)), authorFetcher);
            }
        }

        private class BlogAuthorFetcher : BlogDelayedFetchHandler
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
                List<PostIdAndNameField> authors = new List<PostIdAndNameField>();
                foreach (AuthorInfo author in authorList)
                    authors.Add(new PostIdAndNameField(author.Id, author.Name));
                return authors.ToArray();
            }
        }

        #region Implementation of IBlogPostEditor

        void IBlogPostEditor.Initialize(IBlogPostEditingContext context, IBlogClientOptions clientOptions)
        {
            Text = context.BlogPost.IsPage ? Res.Get(StringId.PageProperties) : Res.Get(StringId.PostProperties);
            controller.Initialize(context, clientOptions);
        }

        void IBlogPostEditor.OnBlogChanged(Blog newBlog)
        {
            // preserve dirty state
            using (controller.SuspendLogic())
            {
                _blogId = newBlog.Id;
                controller.OnBlogChanged(newBlog);

                // Clear out author whenever blog changes
                SetAuthor(PostIdAndNameField.Empty);

                // Comment policy hackery
                BlogCommentPolicy? commentPolicy = null;
                if (comboComments.SelectedIndex >= 0)
                    commentPolicy = ((CommentComboItem)comboComments.SelectedItem).Value;
                comboComments.Items.Remove(new CommentComboItem(BlogCommentPolicy.None));
                if (!newBlog.ClientOptions.CommentPolicyAsBoolean)
                    comboComments.Items.Insert(1, new CommentComboItem(BlogCommentPolicy.None));
                if (commentPolicy != null)
                    comboComments.SelectedItem = new CommentComboItem(commentPolicy.Value);
                if (comboComments.SelectedIndex == -1) // In no case should there be no selection at all
                    comboComments.SelectedIndex = 0;

                labelTags.Text = newBlog.ClientOptions.KeywordsAsTags
                                     ? Res.Get(StringId.PropertiesTags)
                                     : Res.Get(StringId.PropertiesKeywords);
            }
        }

        void IBlogPostEditor.OnBlogSettingsChanged(bool templateChanged)
        {
            // preserve dirty state
            using (controller.SuspendLogic())
                controller.OnBlogSettingsChanged(templateChanged);
        }

        bool IBlogPostEditor.IsDirty
        {
            get
            {
                return controller.IsDirty;
            }
        }

        public bool HasKeywords
        {
            get { return textTags.TextValue.Trim().Length > 0; }
        }

        void IBlogPostEditor.SaveChanges(BlogPost post, BlogPostSaveOptions options)
        {
            controller.SaveChanges(post, options);
        }

        bool IBlogPostEditor.ValidatePublish()
        {
            return controller.ValidatePublish();
        }

        void IBlogPostEditor.OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {
            using (controller.SuspendLogic())
            {
                controller.OnPublishSucceeded(blogPost, postResult);

                // slug can be modified by the service
                textSlug.Text = blogPost.Slug;
            }
        }

        void IBlogPostEditor.OnClosing(CancelEventArgs e)
        {
            controller.OnPostClosing(e);
        }

        void IBlogPostEditor.OnPostClosing(CancelEventArgs e)
        {
            controller.OnPostClosing(e);
        }

        void IBlogPostEditor.OnClosed()
        {
            controller.OnClosed();
        }

        void IBlogPostEditor.OnPostClosed()
        {
            controller.OnPostClosed();
        }

        #endregion

        #region Implementation of INewCategoryContext

        public void NewCategoryAdded(BlogPostCategory category)
        {
            controller.NewCategoryAdded(category);
        }

        #endregion

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        internal void Synchronize(SharedPropertiesController anotherController)
        {
            Debug.Assert(!ReferenceEquals(controller, anotherController));

            anotherController.Other = controller;
            controller.Other = anotherController;
        }
    }

    internal abstract class EnumComboItem<TEnum>
    {
        protected readonly TEnum value;

        protected EnumComboItem(TEnum value)
        {
            this.value = value;
        }

        public TEnum Value { get { return value; } }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != GetType()) return false;
            return value.Equals(((EnumComboItem<TEnum>)obj).value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return DisplayName;
        }

        protected abstract string DisplayName { get; }
    }

    internal class CommentComboItem : EnumComboItem<BlogCommentPolicy>
    {
        public CommentComboItem(BlogCommentPolicy value) : base(value)
        {
        }

        protected override string DisplayName
        {
            get
            {
                switch (value)
                {
                    case BlogCommentPolicy.Unspecified:
                        return Res.Get(StringId.PropertiesDefault);
                    case BlogCommentPolicy.None:
                        return Res.Get(StringId.PropertiesCommentPolicyNone);
                    case BlogCommentPolicy.Open:
                        return Res.Get(StringId.PropertiesCommentPolicyOpen);
                    case BlogCommentPolicy.Closed:
                        return Res.Get(StringId.PropertiesCommentPolicyClosed);
                    default:
                        Debug.Fail("Unexpected enum value");
                        return Res.Get(StringId.PropertiesDefault);
                }
            }
        }
    }

    internal class TrackbackComboItem : EnumComboItem<BlogTrackbackPolicy>
    {
        public TrackbackComboItem(BlogTrackbackPolicy value) : base(value)
        {
        }

        protected override string DisplayName
        {
            get
            {
                switch (value)
                {
                    case BlogTrackbackPolicy.Unspecified:
                        return Res.Get(StringId.PropertiesDefault);
                    case BlogTrackbackPolicy.Allow:
                        return Res.Get(StringId.PropertiesTrackbackPolicyAllow);
                    case BlogTrackbackPolicy.Deny:
                        return Res.Get(StringId.PropertiesTrackbackPolicyDeny);
                    default:
                        Debug.Fail("Unexpected enum value");
                        return Res.Get(StringId.PropertiesDefault);
                }
            }
        }
    }
}
