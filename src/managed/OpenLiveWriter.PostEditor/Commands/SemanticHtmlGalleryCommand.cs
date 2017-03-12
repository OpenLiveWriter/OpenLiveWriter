// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;
using AsyncOperation = OpenLiveWriter.CoreServices.AsyncOperation;

namespace OpenLiveWriter.PostEditor.Commands
{
    public delegate string TemplateHtmlDelegate();

    /// <summary>
    /// Asynchronously fetches gallery preview images for a particular blog and set of class ids
    /// </summary>
    internal class UpdateSemanticHtmlPreviewAsyncOperation : AsyncOperation
    {
        private readonly bool _isRtl;
        private readonly string _blogId;
        private readonly string[] _elementIds;
        private readonly string _html;
        private readonly int _previewImageWidth;
        private readonly int _previewImageHeight;
        private HtmlScreenCaptureCore _screenCapture;
        private readonly IBlogPostEditingSite _editingSite;

        internal string BlogId { get { return _blogId; } }
        private object _previewLock;

        internal UpdateSemanticHtmlPreviewAsyncOperation(IBlogPostEditingSite editingSite, string blogId, string[] elementIds, string templateHtml, string postBodyHtml, bool isRtl, ISynchronizeInvoke target, object previewLock, int previewImageWidth, int previewImageHeight)
            : base(target)
        {
            _blogId = blogId;
            _elementIds = elementIds;
            _isRtl = isRtl;

            _html = templateHtml.Replace(BlogEditingTemplate.POST_BODY_MARKER, postBodyHtml);

            _previewLock = previewLock;
            _previewImageWidth = previewImageWidth;
            _previewImageHeight = previewImageHeight;
            _editingSite = editingSite;
        }

        #region Overrides of AsyncOperation

        protected override void DoWork()
        {
            Trace.WriteLine("UpdateSemanticHtmlPreviewAsyncOperation started for blog(" + BlogId + ")");

            Size newSize = new Size(1500, 1000);
            _screenCapture = new HtmlScreenCaptureCore(_html, newSize.Width);
            _screenCapture.Ids = _elementIds;
            _screenCapture.HtmlScreenCaptureAvailable += new HtmlScreenCaptureAvailableHandlerCore(screenCapture_HtmlScreenCaptureAvailable);
            _screenCapture.MaximumHeight = newSize.Height;
            _screenCapture.CaptureHtml(45000);
        }

        #endregion

        private void screenCapture_HtmlScreenCaptureAvailable(object sender, HtmlScreenCaptureAvailableEventArgsCore e)
        {
            if (e.CaptureCompleted && e.Bitmap != null)
            {
                // Ensure that the path exists
                try
                {
                    // Problem because this check is pumping messages on bg thread?
                    string directory = BlogEditingTemplate.GetBlogTemplateDir(_blogId);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    Trace.Fail(ex.ToString());
                    return;
                }

                // Now we should have all of the element images
                foreach (string elementId in _screenCapture.Ids)
                {
                    using (Bitmap elementBitmap = _screenCapture.GetElementCaptureProperties(elementId).Bitmap)
                    {
                        // Warning. elementBitmap may be null.
                        string path = SemanticHtmlPreviewManager.GetPreviewBitmapPath(_blogId, elementId);

                        try
                        {
                            // We take the lock here to ensure that we don't try to read a halfway-written file on the UI thread.
                            lock (_previewLock)
                            {
                                ElementCaptureProperties properties = _screenCapture.GetElementCaptureProperties(elementId);
                                using (Bitmap previewBitmap = GetGalleryPreviewImageFromElementImage(elementBitmap, _previewImageWidth, _previewImageHeight, properties.BackgroundColor, properties.Padding, _isRtl))
                                {
                                    previewBitmap.Save(path);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Failed to get preview image for blog(" + _blogId + ") and element(" + elementId + "): " + ex);
                            try
                            {
                                File.Delete(path);
                            }
                            catch (Exception ex2)
                            {
                                Trace.WriteLine("Failed to delete preview image after failing to get it:" + ex2);
                            }
                            return;
                        }
                    }
                }

                try
                {
                    _editingSite.FrameWindow.Invoke(new ThreadStart(() => _editingSite.CommandManager.Invalidate(
                                                                              CommandId.SemanticHtmlGallery)), null);

                }
                catch (Exception ex)
                {
                    Trace.WriteLine("NotifyWeblogStylePreviewChanged failed: " + ex.Message);
                }
            }
        }

        private static Bitmap GetGalleryPreviewImageFromElementImage(Bitmap elementBitmap, int width, int height, Color background, Padding padding, bool isRtl)
        {
            Bitmap previewBitmap = null;
            try
            {
                previewBitmap = new Bitmap((int)DisplayHelper.ScaleX(width), (int)DisplayHelper.ScaleY(height));
                using (Graphics g = Graphics.FromImage(previewBitmap))
                {
                    int left = isRtl ? previewBitmap.Width - elementBitmap.Width : 0;

                    g.Clear(background);
                    if (elementBitmap != null)
                        g.DrawImageUnscaled(elementBitmap, left, (previewBitmap.Height - elementBitmap.Height - padding.Vertical + 1) / 2);
                    return previewBitmap;
                }
            }
            catch (Exception)
            {
                if (previewBitmap != null)
                    previewBitmap.Dispose();
                throw;
            }
        }
    }

    internal class SemanticHtmlPreviewManager
    {
        private readonly IBlogPostEditingSite _editingSite;
        private readonly int _width;
        private readonly int _height;

        private readonly object _previewLock;
        public object Lock { get { return _previewLock; } }

        internal class BlogPreviewInfo
        {
            [ThreadStatic]
            private Dictionary<string, Bitmap> _bitmaps;

            internal BlogPreviewInfo(string blogId, string[] elementIds, bool isRtl, TemplateHtmlDelegate templateHtmlDelegate, string postBodyHtml)
            {
                IsRtl = isRtl;
                _blogId = blogId;
                _elementIds = elementIds;
                _templateHtmlLazyLoader = new LazyLoader<string>(delegate
                                                                     {
                                                                         return templateHtmlDelegate();
                                                                     });

                string style = isRtl ? "dir=\"rtl\" style=\"direction: rtl; text-align: right" : "dir=\"ltr\" style=\"direction: ltr; text-align: left";
                style += "; width: 500px; text-indent: 0px; margin: 0px; overflow: visible; vertical-align: baseline; white-space: nowrap; line-height: normal; position: static\"";
                _postBodyHtml = postBodyHtml.Replace("{style}", style);

                _bitmaps = new Dictionary<string, Bitmap>(elementIds.Length);
            }

            ~BlogPreviewInfo()
            {
                foreach (Bitmap bitmap in _bitmaps.Values)
                {
                    bitmap.Dispose();
                }
            }

            public void SetBitmap(string elementId, Bitmap bitmap)
            {
                if (_bitmaps.ContainsKey(elementId))
                {
                    Bitmap oldBitmap = _bitmaps[elementId];
                    oldBitmap.Dispose();
                    _bitmaps.Remove(elementId);
                }

                _bitmaps.Add(elementId, bitmap);
            }

            public Bitmap GetBitmap(string elementId)
            {
                Bitmap bitmap = _bitmaps[elementId];
                if (bitmap != null)
                {
                    return bitmap.Clone() as Bitmap;
                }
                return null;
            }

            public void Clear()
            {
                foreach (string elementId in ElementIds)
                {
                    // Remove from cache
                    if (_bitmaps.ContainsKey(elementId))
                    {
                        if (_bitmaps[elementId] != null)
                            _bitmaps[elementId].Dispose();
                        _bitmaps.Remove(elementId);
                    }

                    // Remove from disk
                    string path = GetPreviewBitmapPath(_blogId, elementId);
                    File.Delete(path);
                }
            }

            public bool BitmapCached(string elementId)
            {
                return _bitmaps.ContainsKey(elementId);
            }

            private readonly LazyLoader<string> _templateHtmlLazyLoader;
            public string[] ElementIds { get { return _elementIds; } }
            public string TemplateHtml { get { return _templateHtmlLazyLoader.Value; } }
            public string PostBodyHtml { get { return _postBodyHtml; } }

            public bool IsRtl { get; set; }
            private readonly string _blogId;
            private readonly string _postBodyHtml;
            private readonly string[] _elementIds;
        }

        internal SemanticHtmlPreviewManager(IBlogPostEditingSite editingSite, TemplateHtmlDelegate templateHtmlDelegate, int width, int height)
        {
            _previewLock = new object();
            _editingSite = editingSite;
            _templateHtmlDelegate = templateHtmlDelegate;
            _width = width;
            _height = height;

            string previewText = HtmlServices.HtmlEncode(Res.Get(StringId.SemanticHtmlPreviewText));

            _postBodyHtml =
                @"<h1 {style} id=" + PreviewId_H1 + @">" + previewText + @"</h1>
                 <h2 {style} id=" + PreviewId_H2 + @">" + previewText + @"</h2>
                 <h3 {style} id=" + PreviewId_H3 + @">" + previewText + @"</h3>
                 <h4 {style} id=" + PreviewId_H4 + @">" + previewText + @"</h4>
                 <h5 {style} id=" + PreviewId_H5 + @">" + previewText + @"</h5>
                 <h6 {style} id=" + PreviewId_H6 + @">" + previewText + @"</h6>
                 <p  {style} id=" + PreviewId_P + @">" + previewText + @"</p>";
        }

        private Dictionary<string, UpdateSemanticHtmlPreviewAsyncOperation> _asyncOps = new Dictionary<string, UpdateSemanticHtmlPreviewAsyncOperation>();
        private Dictionary<string, BlogPreviewInfo> _blogPreviewInfo = new Dictionary<string, BlogPreviewInfo>();

        internal Bitmap GetPreviewBitmap(string blogId, string elementId)
        {
            Debug.Assert(_blogPreviewInfo.ContainsKey(blogId), "Need to register blog for preview");
            Debug.Assert(((ISynchronizeInvoke)_editingSite).InvokeRequired == false, "This should only be called on the UI thread");

            string path = GetPreviewBitmapPath(blogId, elementId);

            try
            {
                lock (Lock)
                {
                    BlogPreviewInfo info = _blogPreviewInfo[blogId];

                    if (info.BitmapCached(elementId))
                    {
                        return info.GetBitmap(elementId);
                    }
                    else if (File.Exists(path))
                    {
                        MemoryStream ms;
                        using (FileStream fs = File.OpenRead(path))
                        {
                            ms = new MemoryStream((int)fs.Length);
                            StreamHelper.Transfer(fs, ms);
                            ms.Seek(0, SeekOrigin.Begin);
                        }
                        info.SetBitmap(elementId, new Bitmap(ms));
                        return info.GetBitmap(elementId);
                    }

                    if (!_asyncOps.ContainsKey(blogId))
                    {
                        // Fetch asynchronously
                        UpdateSemanticHtmlPreviewAsyncOperation asyncOperation = new UpdateSemanticHtmlPreviewAsyncOperation(_editingSite, blogId, info.ElementIds, info.TemplateHtml, info.PostBodyHtml, info.IsRtl, _editingSite.FrameWindow, Lock, _width, _height);
                        _asyncOps.Add(blogId, asyncOperation);

                        asyncOperation.Failed += new ThreadExceptionEventHandler(asyncOperation_Failed);
                        asyncOperation.Completed += new EventHandler(asyncOperation_Completed);
                        asyncOperation.Start();
                    }
                    // else we're already fetching
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to get preview bitmap for blog(" + blogId + ") and element(" + elementId + "): " + ex);
            }

            return null;
        }

        void asyncOperation_Completed(object sender, EventArgs e)
        {
            UpdateSemanticHtmlPreviewAsyncOperation asyncOperation = (UpdateSemanticHtmlPreviewAsyncOperation)sender;
            lock (Lock)
            {
                _asyncOps.Remove(asyncOperation.BlogId);
            }
        }

        void asyncOperation_Failed(object sender, ThreadExceptionEventArgs e)
        {
            UpdateSemanticHtmlPreviewAsyncOperation asyncOperation = (UpdateSemanticHtmlPreviewAsyncOperation)sender;
            Trace.WriteLine("UpdateSemanticHtmlPreviewAsyncOperation failed for blog(" + asyncOperation.BlogId + "): " + e.Exception);

            // Note: We intentionally do not remove this async op from asyncOps in order to prevent an infinite loop of failures.
        }

        public static string GetPreviewBitmapPath(string blogId, string elementId)
        {
            string blogTemplateDir = BlogEditingTemplate.GetBlogTemplateDir(blogId);
            string previewBitmapFilename = elementId.Replace(PreviewGuid, ".bmp");
            return Path.Combine(blogTemplateDir, previewBitmapFilename);
        }

        private const string PreviewGuid = "-D983805A-53FF-452D-AEA5-45CD81EF54DD";
        public const string PreviewId_H1 = "H1" + PreviewGuid;
        public const string PreviewId_H2 = "H2" + PreviewGuid;
        public const string PreviewId_H3 = "H3" + PreviewGuid;
        public const string PreviewId_H4 = "H4" + PreviewGuid;
        public const string PreviewId_H5 = "H5" + PreviewGuid;
        public const string PreviewId_H6 = "H6" + PreviewGuid;
        public const string PreviewId_P = "P" + PreviewGuid;

        private string _postBodyHtml;

        private readonly TemplateHtmlDelegate _templateHtmlDelegate;

        /// <summary>
        /// Associates a set of element ids and template html with a blog for the purpose of gallery preview image extraction
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="elementIds"></param>
        public void RegisterBlog(string blogId, string[] elementIds, bool isRtl)
        {
            if (!_blogPreviewInfo.ContainsKey(blogId))
                _blogPreviewInfo.Add(blogId, new BlogPreviewInfo(blogId, elementIds, isRtl, _templateHtmlDelegate, _postBodyHtml));
        }

        public void UnregisterBlog(string blogId)
        {

            try
            {
                lock (Lock)
                {
                    if (_blogPreviewInfo.ContainsKey(blogId))
                    {
                        _blogPreviewInfo[blogId].Clear();
                        _blogPreviewInfo.Remove(blogId);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to unregister blog: " + ex);
            }
        }
    }

    public class SemanticHtmlGalleryCommand : GalleryCommand<string>
    {
        private string[] _ids;
        private IBlogPostEditingSite _editingSite;
        private List<SemanticHtmlElementInfo> _elements;
        private SemanticHtmlPreviewManager _previewManager;
        private IHtmlEditorComponentContext _componentContext;

        public SemanticHtmlGalleryCommand(CommandId commandId, IBlogPostEditingSite editingSite, TemplateHtmlDelegate templateHtmlDelegate, CommandManager commandManager, IHtmlEditorComponentContext componentContext)
            : base(commandId)
        {
            _editingSite = editingSite;
            _componentContext = componentContext;

            _elements = new List<SemanticHtmlElementInfo>(7);
            _elements.Add(new SemanticHtmlElementInfo("p", _ELEMENT_TAG_ID.TAGID_P, Res.Get(StringId.Paragraph), SemanticHtmlPreviewManager.PreviewId_P, CommandId.ApplySemanticParagraph));
            _elements.Add(new SemanticHtmlElementInfo("h1", _ELEMENT_TAG_ID.TAGID_H1, Res.Get(StringId.Heading1), SemanticHtmlPreviewManager.PreviewId_H1, CommandId.ApplySemanticHeader1));
            _elements.Add(new SemanticHtmlElementInfo("h2", _ELEMENT_TAG_ID.TAGID_H2, Res.Get(StringId.Heading2), SemanticHtmlPreviewManager.PreviewId_H2, CommandId.ApplySemanticHeader2));
            _elements.Add(new SemanticHtmlElementInfo("h3", _ELEMENT_TAG_ID.TAGID_H3, Res.Get(StringId.Heading3), SemanticHtmlPreviewManager.PreviewId_H3, CommandId.ApplySemanticHeader3));
            _elements.Add(new SemanticHtmlElementInfo("h4", _ELEMENT_TAG_ID.TAGID_H4, Res.Get(StringId.Heading4), SemanticHtmlPreviewManager.PreviewId_H4, CommandId.ApplySemanticHeader4));
            _elements.Add(new SemanticHtmlElementInfo("h5", _ELEMENT_TAG_ID.TAGID_H5, Res.Get(StringId.Heading5), SemanticHtmlPreviewManager.PreviewId_H5, CommandId.ApplySemanticHeader5));
            _elements.Add(new SemanticHtmlElementInfo("h6", _ELEMENT_TAG_ID.TAGID_H6, Res.Get(StringId.Heading6), SemanticHtmlPreviewManager.PreviewId_H6, CommandId.ApplySemanticHeader6));

            _previewManager = new SemanticHtmlPreviewManager(_editingSite, templateHtmlDelegate, RibbonHelper.InGalleryImageWidth, RibbonHelper.InGalleryImageHeightWithoutLabel);

            _ids = new string[_elements.Count];
            for (int i = 0; i < _ids.Length; i++)
            {
                _ids[i] = _elements[i].HtmlId;

                int index = i;
                OverridableCommand command = new OverridableCommand(_elements[i].CommandId);
                command.Execute += (sender, e) => SelectedIndex = index;
                commandManager.Add(command);
            }
        }

        public override int OverrideProperty(ref PropertyKey key, PropVariantRef overrideValue)
        {
            // In this case the override applies to each of the commands associated with each gallery item as well.
            // Normally, you can't disable individual gallery items, but these items have shortcuts associated with
            // them that must be disabled/enabled according to the override.
            foreach (var v in _elements)
            {
                _editingSite.CommandManager.OverrideProperty((uint)v.CommandId, ref key, overrideValue);
            }

            return base.OverrideProperty(ref key, overrideValue);
        }

        public override int CancelOverride(ref PropertyKey key)
        {
            // In this case the override applies to each of the commands associated with each gallery item as well.
            // Normally, you can't disable individual gallery items, but these items have shortcuts associated with
            // them that must be disabled/enabled according to the override.
            foreach (var v in _elements)
            {
                _editingSite.CommandManager.CancelOverride((uint)v.CommandId, ref key);
            }

            return base.CancelOverride(ref key);
        }

        public override bool Enabled
        {
            set
            {
                base.Enabled = value;

                foreach (var v in _elements)
                {
                    _editingSite.CommandManager.SetEnabled(v.CommandId, value);
                }
            }
        }

        public string SelectedStyle
        {
            set
            {
                for (int i = 0; i < _elements.Count; i++)
                {
                    if (_elements[i].HtmlName == value)
                    {
                        if (selectedIndex != i)
                        {
                            selectedIndex = i;

                            InvalidateSelectedItemProperties();
                            OnStateChanged(EventArgs.Empty);
                        }

                        break;
                    }
                }
            }
        }

        public override void Invalidate()
        {
            items.Clear();
            base.Invalidate();
        }

        private string _blogId;
        private bool _isRtl;
        public void SetAccountId(string blogId, bool isRtl, bool forceRefresh)
        {
            if (_blogId != blogId || forceRefresh)
            {
                if (forceRefresh)
                    _previewManager.UnregisterBlog(blogId);
                _blogId = blogId;
                _isRtl = isRtl;
                items.Clear();
                LoadItems();
            }
        }

        internal class SemanticHtmlElementInfo
        {
            internal SemanticHtmlElementInfo(string htmlName, _ELEMENT_TAG_ID tagId, string displayName, string htmlId, CommandId commandId)
            {
                _htmlName = htmlName;
                _tagId = tagId;
                _displayName = displayName;
                _htmlId = htmlId;
                _commandId = commandId;
            }

            public CommandId CommandId { get { return _commandId; } }
            public string HtmlName { get { return _htmlName; } }
            public string DisplayName { get { return _displayName; } }
            public _ELEMENT_TAG_ID TagId { get { return _tagId; } }
            public string HtmlId { get { return _htmlId; } }

            private CommandId _commandId;
            private string _htmlId;
            private string _htmlName;
            private _ELEMENT_TAG_ID _tagId;
            private string _displayName;

        }

        public class SemanticHtmlGalleryItem : GalleryItem, IHtmlFormattingStyle
        {
            public SemanticHtmlGalleryItem(string elementName, _ELEMENT_TAG_ID elementTagId, string label, Bitmap bitmap)
                : base(label, bitmap, label)
            {
                this.elementName = elementName;
                this.elementTagId = elementTagId;
            }

            #region Implementation of IHtmlFormattingStyle

            public string DisplayName
            {
                get { return Label; }
            }

            private string elementName;
            public string ElementName
            {
                get { return elementName; }
            }

            private _ELEMENT_TAG_ID elementTagId;
            public _ELEMENT_TAG_ID ElementTagId
            {
                get { return elementTagId; }
            }

            #endregion
        }

        public override void LoadItems()
        {
            if (items.Count == 0)
            {
                items.Clear();

                _previewManager.RegisterBlog(_blogId, _ids, _isRtl);
                foreach (var v in _elements)
                {
                    items.Add(new SemanticHtmlGalleryItem(v.HtmlName, v.TagId, v.DisplayName, _previewManager.GetPreviewBitmap(_blogId, v.HtmlId)));
                }

                base.LoadItems();

                OnStateChanged(EventArgs.Empty);
            }
        }
    }
}
