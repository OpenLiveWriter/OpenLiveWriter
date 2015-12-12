// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.HtmlEditor.Marshalling;
using OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.LiveClipboard;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors;
using OpenLiveWriter.PostEditor.Tables;
using OpenLiveWriter.PostEditor.Video;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Handles marshalling for the RichHtmlContentEditor.
    /// </summary>
    internal class ExtendedHtmlEditorMashallingHandler : HtmlEditorMarshallingHandler
    {
        IHtmlEditorHost _blogEditor;
        IContentSourceSite _insertionSite;
        private OpenLiveWriter.Interop.Com.IDropTarget _unhandledDropTarget;

        private bool _isPlaintextOnly = false;
        public bool IsPlainTextOnly
        {
            get
            {
                return _isPlaintextOnly;
            }
            set
            {
                _isPlaintextOnly = value;
                dataFormatRegistry.Clear();
                dataFormatRegistry.Register(CreateDataFormatFactories());
            }
        }

        internal ExtendedHtmlEditorMashallingHandler(IHtmlMarshallingTarget editorContext, IContentSourceSite sourceSite, IHtmlEditorHost blogEditor, OpenLiveWriter.Interop.Com.IDropTarget unhandledDropTarget)
            : base(editorContext)
        {
            _insertionSite = sourceSite;
            _blogEditor = blogEditor;
            _unhandledDropTarget = unhandledDropTarget;
        }

        protected override IDataFormatHandlerFactory[] CreateDataFormatFactories()
        {
            if (IsPlainTextOnly)
                return new IDataFormatHandlerFactory[]{
                new DelegateBasedDataFormatHandlerFactory(CreateEmlMessageFormatHandler, EmlMessageHandler.CanCreateFrom),
                new DelegateBasedDataFormatHandlerFactory(CreateInternalSmartContentFormatHandler, InternalSmartContentFormatHandler.CanCreateFrom),
                new DelegateBasedDataFormatHandlerFactory(CreateTextDataFormatHandler, CanCreateFromTextFilter),
                new DelegateBasedDataFormatHandlerFactory(CreateUnhandledFormatHandler, UnhandledDropTarget.CanCreateFrom) // This always needs to be the last handler
            };

            return new IDataFormatHandlerFactory[]{
                new DelegateBasedDataFormatHandlerFactory(CreateEmlMessageFormatHandler, EmlMessageHandler.CanCreateFrom),
                   new DelegateBasedDataFormatHandlerFactory(CreateInternalSmartContentFormatHandler, InternalSmartContentFormatHandler.CanCreateFrom ),
                new DelegateBasedDataFormatHandlerFactory(CreateLiveClipboardContentSourceFormatHandler, LiveClipboardContentSourceFormatHandler.CanCreateFrom ),
                new DelegateBasedDataFormatHandlerFactory(CreateLiveClipboardHtmlFormatHandler, LiveClipboardHtmlFormatHandler.CanCreateFrom ),
                new DelegateBasedDataFormatHandlerFactory(CreateContentSourceUrlFormatHandler, UrlContentSourcelFormatHandler.CanCreateFrom),
                new DelegateBasedDataFormatHandlerFactory(CreateUrlDataFormatHandler, CanCreateUrlFormatHandler),
                new DelegateBasedDataFormatHandlerFactory(CreateImageOnlyHtmlDataFormatHandler, CanCreateImageOnlyHtmlFormatHandler),
                new DelegateBasedDataFormatHandlerFactory(CreateImageFileFormatHandler, data => !_blogEditor.ShouldComposeHostHandlePhotos() && ImageFileFormatHandler.CanCreateFrom(data)),
                new DelegateBasedDataFormatHandlerFactory(CreateTableDataFormatHandler, CanCreateTableDataFormatHandler),
#if SUPPORT_FILES
                new DelegateBasedDataFormatHandlerFactory(CreateFileDataFormatHandler, new DataObjectFilter(CanCreateFileFormatHandler)),
#endif
                new DelegateBasedDataFormatHandlerFactory(CreateHtmlDataFormatHandler, CanCreateHtmlFormatHandler),
                new DelegateBasedDataFormatHandlerFactory(CreateImageClipboardFormatHandler, CanCreateImageDataFormatHandler),
                new DelegateBasedDataFormatHandlerFactory(CreateEmbedDataFormatHandler, CanCreateEmbedFormatHandler ),
                new DelegateBasedDataFormatHandlerFactory(CreateVideoFileFormatHandler, VideoFileFormatHandler.CanCreateFrom),
                new DelegateBasedDataFormatHandlerFactory(CreateImageFolderFormatHandler, data => !_blogEditor.ShouldComposeHostHandlePhotos() && ImageFolderFormatHandler.CanCreateFrom(data)),
                new DelegateBasedDataFormatHandlerFactory(CreateTextDataFormatHandler, CanCreateFromTextFilter),
                new DelegateBasedDataFormatHandlerFactory(CreateUnhandledFormatHandler, UnhandledDropTarget.CanCreateFrom) // This always needs to be the last handler
            };
        }

        // special text filter that screens out LiveClipboard data
        private bool CanCreateFromTextFilter(DataObjectMeister dataMeister)
        {
            return CanCreateTextFormatHandler(dataMeister) && (dataMeister.LiveClipboardData == null);
        }

        protected virtual DataFormatHandler CreateLiveClipboardContentSourceFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new LiveClipboardContentSourceFormatHandler(dataMeister, handlerContext, EditorContext, _insertionSite);
        }

        protected virtual DataFormatHandler CreateLiveClipboardHtmlFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new LiveClipboardHtmlFormatHandler(dataMeister, handlerContext, EditorContext, _insertionSite);
        }

        protected virtual DataFormatHandler CreateTableDataFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new TableDataFormatHandler(dataMeister, handlerContext, EditorContext);
        }

        protected virtual DataFormatHandler CreateInternalSmartContentFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new InternalSmartContentFormatHandler(dataMeister, handlerContext, EditorContext, _insertionSite);
        }

        protected virtual DataFormatHandler CreateContentSourceUrlFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new UrlContentSourcelFormatHandler(dataMeister, handlerContext, EditorContext, _insertionSite);
        }

        protected virtual DataFormatHandler CreateImageFileFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new ImageFileFormatHandler(dataMeister, handlerContext, EditorContext, _blogEditor);
        }

        protected virtual DataFormatHandler CreateVideoFileFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new VideoFileFormatHandler(dataMeister, handlerContext, EditorContext, _blogEditor);
        }

        protected virtual DataFormatHandler CreateImageFolderFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new ImageFolderFormatHandler(dataMeister, handlerContext, EditorContext, _blogEditor);
        }

        protected virtual DataFormatHandler CreateImageClipboardFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new ImageClipboardFormatHandler(dataMeister, handlerContext, EditorContext, _blogEditor);
        }

        protected virtual DataFormatHandler CreateUnhandledFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new UnhandledDropTarget(dataMeister, handlerContext, _unhandledDropTarget);
        }

        protected virtual DataFormatHandler CreateEmlMessageFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new EmlMessageHandler(dataMeister, handlerContext, _unhandledDropTarget);
        }

        protected virtual bool CanCreateEmbedFormatHandler(DataObjectMeister dataObject)
        {
            return EditorContext.MarshalHtmlSupported && EmbedFormatHandler.CanCreateFrom(dataObject);
        }

        protected virtual DataFormatHandler CreateEmbedDataFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new EmbedFormatHandler(dataMeister, handlerContext, EditorContext, _insertionSite);
        }

        protected virtual bool CanCreateTableDataFormatHandler(DataObjectMeister dataObject)
        {
            return EditorContext.MarshalHtmlSupported && TableDataFormatHandler.CanCreateFrom(dataObject);
        }

        protected virtual bool CanCreateImageDataFormatHandler(DataObjectMeister dataObject)
        {
            return EditorContext.MarshalImagesSupported && ImageClipboardFormatHandler.CanCreateFrom(dataObject);
        }

    }

    public class EmlMessageHandler : UnhandledDropTarget
    {
        public EmlMessageHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, OpenLiveWriter.Interop.Com.IDropTarget unhandledDropTarget)
            : base(dataObject, handlerContext, unhandledDropTarget)
        {

        }

        public new static bool CanCreateFrom(DataObjectMeister data)
        {
            if (data.FileData == null)
                return false;

            foreach (FileItem file in data.FileData.Files)
            {
                FileInfo fi = new FileInfo(file.ContentsPath);

                if (fi.Exists && string.Compare(fi.Extension, ".eml", true, CultureInfo.InvariantCulture) == 0)
                    return true;
            }

            return false;
        }
    }

    public class UnhandledDropTarget : DataFormatHandler
    {
        private OpenLiveWriter.Interop.Com.IDropTarget _unhandledDropTarget;
        private bool _hasCalledBeginDrag = false;
        private MK _mkKeyState;
        private DROPEFFECT _effect;
        private POINT _point;
        private IOleDataObject _oleDataObject;

        public UnhandledDropTarget(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, OpenLiveWriter.Interop.Com.IDropTarget unhandledDropTarget)
            : base(dataObject, handlerContext)
        {
            _unhandledDropTarget = unhandledDropTarget;
            _oleDataObject = MshtmlEditorDragAndDropTarget.ExtractOleDataObject(DataMeister.IDataObject);
        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            return true;
        }

        public override void BeginDrag()
        {

        }

        public override DragDropEffects ProvideDragFeedback(Point screenPoint, int keyState, DragDropEffects supportedEffects)
        {
            if (_unhandledDropTarget == null)
                return DragDropEffects.None;

            _mkKeyState = MshtmlEditorDragAndDropTarget.ConvertKeyState(keyState);
            _effect = MshtmlEditorDragAndDropTarget.ConvertDropEffect(supportedEffects);
            _point = new POINT();
            _point.x = screenPoint.X;
            _point.y = screenPoint.Y;

            // We have to call begin drag here because we need the location and key state which we dont have in BeginDrag()
            if (!_hasCalledBeginDrag)
            {
                _hasCalledBeginDrag = true;
                _unhandledDropTarget.DragEnter(_oleDataObject, _mkKeyState, _point, ref _effect);
            }

            _unhandledDropTarget.DragOver(_mkKeyState, _point, ref _effect);

            return MshtmlEditorDragAndDropTarget.ConvertDropEffect(_effect);
        }

        public override void EndDrag()
        {
            if (_unhandledDropTarget == null)
                return;
            _unhandledDropTarget.DragLeave();
        }

        public override bool DataDropped(DataAction action)
        {
            if (_unhandledDropTarget == null)
                return false;

            _unhandledDropTarget.Drop(_oleDataObject, _mkKeyState, _point, ref _effect);

            return _effect != DROPEFFECT.NONE;
        }

        public override bool InsertData(DataAction action, params object[] args)
        {
            return DataDropped(action);
        }
    }

    internal abstract class LiveClipboardDataFormatHandler : FreeTextHandler
    {
        public LiveClipboardDataFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
            : base(dataObject, handlerContext, editorContext)
        {
        }

        public override DragDropEffects ProvideDragFeedback(Point screenPoint, int keyState, DragDropEffects supportedEffects)
        {
            // call base to get insertion point behavior
            DragDropEffects effects = base.ProvideDragFeedback(screenPoint, keyState, supportedEffects);

            // override default effects
            if (effects != DragDropEffects.None)
                return ProvideCopyAsDefaultWithMoveOverride(keyState, supportedEffects);
            else
                return effects;
        }
    }

    internal class LiveClipboardContentSourceFormatHandler : LiveClipboardDataFormatHandler
    {
        IContentSourceSite _contentSourceSite;

        public LiveClipboardContentSourceFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IContentSourceSite sourceSite)
            : base(dataObject, handlerContext, editorContext)
        {
            _contentSourceSite = sourceSite;
        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            return data.LiveClipboardData != null &&
                    LiveClipboardManager.FindContentSourceForLiveClipboard(data.LiveClipboardData.Formats) != null;
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            // get the live clipboard data
            LiveClipboardData lcData = DataMeister.LiveClipboardData;
            if (lcData == null)
            {
                Trace.Fail("Unexpected failure to get LC data!");
                return false;
            }

            // lookup the content-source
            ContentSourceInfo contentSource =
                LiveClipboardManager.FindContentSourceForLiveClipboard(lcData.Formats);
            if (contentSource == null)
            {
                Trace.Fail("Unexpected failure to find content soure!");
                return false;
            }

            using (new WaitCursor())
            {
                try
                {
                    // HACK: drive the selection textRange to the caret so we can call generic
                    // content-source routines that work off the current selection
                    // Note that we do the same thing below for Images so we can use the common
                    // InsertImages method -- we may want to bake this into core marshalling
                    // or add MarkupRange parameters to the image and content-source routines
                    EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();

                    if (contentSource.Instance is SmartContentSource)
                    {
                        return InsertSmartContentFromLiveClipboard(contentSource, lcData.Document);
                    }
                    else if (contentSource.Instance is ContentSource)
                    {
                        return InsertSimpleContentFromLiveClipboard(contentSource, lcData.Document);
                    }
                    else
                    {
                        Debug.Fail("Unexpected content source type: " + contentSource.GetType());
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    ContentSourceManager.DisplayContentRetreivalError(EditorContext.FrameWindow, ex, contentSource);
                    return false;
                }
            }

        }

        private bool InsertSimpleContentFromLiveClipboard(ContentSourceInfo contentSource, XmlDocument lcDocument)
        {
            ContentSource lcContentSource = contentSource.Instance as ContentSource;
            if (lcContentSource == null)
            {
                Trace.Fail("Unexpected failure to get live clipboard content-source!");
                return false;
            }

            // create the content
            string content = String.Empty;
            if (lcContentSource.CreateContentFromLiveClipboard(EditorContext.FrameWindow, lcDocument, ref content) == DialogResult.OK)
            {
                _contentSourceSite.InsertContent(content, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool InsertSmartContentFromLiveClipboard(ContentSourceInfo contentSource, XmlDocument lcDocument)
        {
            SmartContentSource smartSource = contentSource.Instance as SmartContentSource;
            if (smartSource == null)
            {
                Trace.Fail("Unexpected failure to get live clipboard content-source!");
                return false;
            }

            // create the smart content
            IExtensionData extensionData = _contentSourceSite.CreateExtensionData(Guid.NewGuid().ToString());
            ISmartContent smartContent = new SmartContent(extensionData);
            if (smartSource.CreateContentFromLiveClipboard(EditorContext.FrameWindow, lcDocument, smartContent) == DialogResult.OK)
            {
                // generate html and insert it
                string content = smartSource.GenerateEditorHtml(smartContent, _contentSourceSite);
                if (content != null)
                {
                    _contentSourceSite.InsertContent(contentSource.Id, content, extensionData);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    internal class LiveClipboardHtmlFormatHandler : LiveClipboardDataFormatHandler
    {
        IContentSourceSite _contentSourceSite;

        public LiveClipboardHtmlFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IContentSourceSite sourceSite)
            : base(dataObject, handlerContext, editorContext)
        {
            _contentSourceSite = sourceSite;
        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            return data.LiveClipboardData != null && data.LiveClipboardData.HtmlPresentation != null;
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            using (new WaitCursor())
            {
                try
                {
                    string htmlPresentation = DataMeister.LiveClipboardData.HtmlPresentation;

                    // remove document tags and scripts
                    htmlPresentation = HtmlCleaner.RemoveScripts(htmlPresentation);

                    // insert the html
                    EditorContext.InsertHtml(begin, end, htmlPresentation, null);
                    return true;
                }
                catch (Exception e)
                {
                    //bugfix 1696, put exceptions into the trace log.
                    Trace.Fail("Exception while inserting HTML: " + e.Message, e.StackTrace);
                    return false;
                }
            }
        }
    }

    internal class InternalSmartContentFormatHandler : FreeTextHandler
    {
        IContentSourceSite _contentSourceSite;

        public InternalSmartContentFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IContentSourceSite sourceSite)
            : base(dataObject, handlerContext, editorContext)
        {
            _contentSourceSite = sourceSite;
        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            if (OleDataObjectHelper.GetDataPresentSafe(data.IDataObject, SmartContentDataObject.INTERNAL_SMART_CONTENT_DATAFORMAT))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override DragDropEffects ProvideDragFeedback(Point screenPoint, int keyState, DragDropEffects supportedEffects)
        {
            // call base to get insertion point behavior
            DragDropEffects effects = base.ProvideDragFeedback(screenPoint, keyState, supportedEffects);

            // override default effects
            if (effects != DragDropEffects.None)
                return ProvideMove(keyState, supportedEffects);
            else
                return effects;
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            // get a reference to the underlying data object
            IDataObject dataObject = DataMeister.IDataObject;

            string dataObjectInstanceId = dataObject.GetData(SmartContentDataObject.INSTANCE_ID_DATAFORMAT) as string;
            if (EditorContext.EditorId.Equals(dataObjectInstanceId))
            {
                // get the internal items out of the data object
                string smartContentElementId = (string)dataObject.GetData(SmartContentDataObject.INTERNAL_SMART_CONTENT_DATAFORMAT);

                IHTMLElement element = (EditorContext.HtmlDocument as IHTMLDocument3).getElementById(smartContentElementId);
                Debug.Assert(element != null, "Invalid smart content item id detected: " + smartContentElementId);
                if (element != null)
                {
                    MarkupRange elementRange = EditorContext.MarkupServices.CreateMarkupRange(element, true);
                    MarkupPointer insertionPoint = begin;
                    MarkupPointerMoveHelper.PerformImageBreakout(insertionPoint);

                    insertionPoint.PushCling(false);
                    insertionPoint.PushGravity(_POINTER_GRAVITY.POINTER_GRAVITY_Left);

                    //verify the insertion point is inside of the same content editable parent (bug 290729)
                    if (!IsValidInsertionPoint(element, insertionPoint))
                        return false;

                    try
                    {
                        if (InlineEditField.IsEditField(insertionPoint.CurrentScope))
                        {
                            // If we are in an edit field, since we strip the content wdown to plain text we have to use
                            // InsertHtml.  However, this means we need to move the content via a string.  This sucks because
                            // we break any markup ranges in that area.  This is why ignore once will not work in edit fields.
                            EditorContext.MarkupServices.Remove(begin, end);
                            string html = elementRange.HtmlText;
                            EditorContext.MarkupServices.Remove(elementRange.Start, elementRange.End);
                            EditorContext.InsertHtml(insertionPoint, insertionPoint, html, null);
                        }
                        else
                        {
                            EditorContext.MarkupServices.Remove(begin, end);
                            EditorContext.MarkupServices.Move(elementRange.Start, elementRange.End, insertionPoint);
                        }
                        return true;
                    }
                    finally
                    {
                        insertionPoint.PopCling();
                        insertionPoint.PopGravity();
                    }
                }
            }
            return false;
        }

        private bool IsValidInsertionPoint(IHTMLElement e, MarkupPointer p)
        {
            if (InlineEditField.IsEditField(p.CurrentScope))
                return true;

            IHTMLElement contentEditableParent = null;
            IHTMLElement parent = e;
            while (parent != null)
            {
                if ((parent as IHTMLElement3).isContentEditable)
                    contentEditableParent = parent;
                else if (contentEditableParent != null)
                    break; //we hit the top-most editable parent.

                parent = parent.parentElement;
            }

            if (contentEditableParent != null)
            {
                MarkupRange range = EditorContext.MarkupServices.CreateMarkupRange(contentEditableParent, false);
                return range.InRange(p);
            }
            else
                return false;
        }
    }

    internal class UrlContentSourcelFormatHandler : UrlHandler
    {
        IContentSourceSite _contentSourceSite;

        public UrlContentSourcelFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IContentSourceSite sourceSite)
            : base(dataObject, handlerContext, editorContext)
        {
            _contentSourceSite = sourceSite;
        }

        public static new bool CanCreateFrom(DataObjectMeister data)
        {
            if (!GlobalEditorOptions.SupportsFeature(ContentEditorFeature.UrlContentSourcePaste))
                return false;

            if (HasUrl(data))
            {
                return ContentSourceManager.FindContentSourceForUrl(ExtractUrl(data)) != null;
            }
            else
            {
                return false;
            }
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            // lookup the content-source
            ContentSourceInfo contentSource = ContentSourceManager.FindContentSourceForUrl(Url);

            if (contentSource != null)
            {
                // HACK: drive the selection textRange to the caret so we can call generic
                // content-source routines that work off the current selection
                // Note that we do the same thing below for Images so we can use the common
                // InsertImages method -- we may want to bake this into core marshalling
                // or add MarkupRange parameters to the image and content-source routines
                EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();

                try
                {
                    if (contentSource.Instance is SmartContentSource)
                    {
                        return InsertSmartContentFromUrl(contentSource, Url);
                    }
                    else if (contentSource.Instance is ContentSource)
                    {
                        return InsertSimpleContentFromUrl(contentSource, Url);
                    }
                    else
                    {
                        Debug.Fail("Unexpected content source type: " + contentSource.GetType());
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception inserting content: " + ex.ToString());
                    return false;
                }
            }
            else
            {
                Debug.Fail("No content source found during marshalling!");
                return false;
            }
        }

        private bool InsertSimpleContentFromUrl(ContentSourceInfo contentSource, string url)
        {
            string title = String.Empty;
            string content = String.Empty;

            if (UrlContentRetreivalWithProgress.ExecuteSimpleContentRetreival(
                EditorContext.FrameWindow, contentSource, url, ref title, ref content))
            {
                _contentSourceSite.InsertContent(content, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool InsertSmartContentFromUrl(ContentSourceInfo contentSource, string url)
        {
            SmartContentSource smartSource = contentSource.Instance as SmartContentSource;
            string title = String.Empty;
            IExtensionData extensionData = _contentSourceSite.CreateExtensionData(Guid.NewGuid().ToString());
            ISmartContent smartContent = new SmartContent(extensionData);

            if (UrlContentRetreivalWithProgress.ExecuteSmartContentRetreival(
                EditorContext.FrameWindow, contentSource, url, ref title, smartContent))
            {
                string content = smartSource.GenerateEditorHtml(smartContent, _contentSourceSite);
                if (content != null)
                {
                    _contentSourceSite.InsertContent(contentSource.Id, content, extensionData);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal class VideoFileFormatHandler : FreeTextHandler
    {
        IHtmlEditorHost _blogEditor;
        public VideoFileFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IHtmlEditorHost blogEditor)
            : base(dataObject, handlerContext, editorContext)
        {
            _blogEditor = blogEditor;
        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            return false;
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            //hack: drive the selection textRange to the caret (before calling InsertImages)
            EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();
            _blogEditor.InsertSmartContentFromFile(new string[] { DataMeister.FileData.Files[0].ContentsPath }, VideoContentSource.ID, HtmlInsertionOptions.Default, null);
            return true;
        }

        public override void Dispose()
        {
            _blogEditor = null;
            base.Dispose();
        }
    }

    internal abstract class ImageBaseFormatHandler : FreeTextHandler
    {
        IHtmlEditorHost _blogEditor;

        public ImageBaseFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IHtmlEditorHost blogEditor)
            : base(dataObject, handlerContext, editorContext)
        {
            _blogEditor = blogEditor;
        }

        private ImageInsertEntryPoint ImageInsertEntryPoint
        {
            get
            {
                switch (HandlerContext)
                {
                    case DataFormatHandlerContext.ClipboardPaste:
                        return ImageInsertEntryPoint.ClipboardPaste;
                    case DataFormatHandlerContext.DragAndDrop:
                        return ImageInsertEntryPoint.DragDrop;
                    default:
                        Trace.Fail("Unexpected data handler context: " + HandlerContext);
                        return ImageInsertEntryPoint.Inline;
                }
            }
        }

        protected bool DoInsertDataCore(List<string> files, DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            IHtmlEditorHost blogEditor = _blogEditor;
            ImageInsertEntryPoint imageInsertEntryPoint = ImageInsertEntryPoint;

            EditorContext.Invoke(delegate
            {
                //hack: drive the selection textRange to the caret (before calling InsertImages)
                EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();

                string[] fileArray = files.ToArray();
                blogEditor.InsertImages(fileArray, ImageInsertEntryPoint.DragDrop);
            });

            return true;
        }

        public override void Dispose()
        {
            _blogEditor = null;
            base.Dispose();
        }
    }

    internal class ImageFolderFormatHandler : ImageBaseFormatHandler
    {
        public ImageFolderFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IHtmlEditorHost blogEditor)
            : base(dataObject, handlerContext, editorContext, blogEditor)
        {

        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            // see if there are files at the top-level of the file data
            FileData fileData = data.FileData;
            if (fileData != null && fileData.Files.Length == 1 && fileData.Files[0].IsDirectory)
            {
                DirectoryLister lister = new DirectoryLister(fileData.Files[0].ContentsPath, false, true);
                foreach (string file in lister.GetFiles())
                {
                    if (PathHelper.IsPathImage(file))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            //hack: drive the selection textRange to the caret (before calling InsertImages)
            EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();

            List<string> files = new List<string>();

            DirectoryLister lister = new DirectoryLister(DataMeister.FileData.Files[0].ContentsPath, false, true);
            foreach (string file in lister.GetFiles())
            {
                if (PathHelper.IsPathImage(file))
                {
                    files.Add(Path.Combine(lister.RootPath, file));
                }
            }

            return DoInsertDataCore(files, action, begin, end);
        }
    }

    internal class ImageFileFormatHandler : ImageBaseFormatHandler
    {
        public ImageFileFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IHtmlEditorHost blogEditor)
            : base(dataObject, handlerContext, editorContext, blogEditor)
        {

        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            // see if there are files at the top-level of the file data
            FileData fileData = data.FileData;
            int photoFilesFound = 0;
            if (fileData != null && fileData.Files.Length > 0)
            {
                // if there are any directories in the file data then we can't create from
                foreach (FileItem file in fileData.Files)
                {
                    if (FileHelper.IsSystemFile(file.ContentsPath))
                        continue;

                    if (!PathHelper.IsPathImage(file.ContentsPath))
                        return false;

                    photoFilesFound++;
                }

                // Make sure we found at least one non-system file
                return photoFilesFound > 0;
            }
            else  // no file data
            {
                return false;
            }
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            // get the list of files from the data meister
            FileItem[] files = DataMeister.FileData.Files;

            List<string> filePaths = new List<string>(DataMeister.FileData.Files.Length);
            // create an array of file entities to insert
            for (int i = 0; i < files.Length; i++)
            {
                if (PathHelper.IsPathImage(files[i].ContentsPath))
                    filePaths.Add(files[i].ContentsPath);
            }

            return DoInsertDataCore(filePaths, action, begin, end);
        }
    }

    internal class ImageClipboardFormatHandler : FreeTextHandler
    {
        IHtmlEditorHost _blogEditor;
        public ImageClipboardFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IHtmlEditorHost blogEditor)
            : base(dataObject, handlerContext, editorContext)
        {
            _blogEditor = blogEditor;
        }

        /// <summary>
        /// Is there URL data in the passed data object?
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>true if there is url data, else false</returns>
        public static bool CanCreateFrom(DataObjectMeister data)
        {
            //return data.ImageData != null && !data.EnvironmentData.LooksLikeExcelTableSelection;
            return data.ImageData != null && (data.ImageData.Bitmap != null || data.ImageData.Dib != null || data.ImageData.GIF != null);
        }

        /// <summary>
        /// Instruct the handler to insert data into the presentation editor
        /// </summary>
        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            string imagePath = SaveImageDataToTempFile(DataMeister.ImageData);
            if (imagePath != null)
            {
                //hack: drive the selection textRange to the caret (before calling InsertImages)
                EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();

                _blogEditor.InsertImages(new string[] { imagePath }, ImageInsertEntryPoint.DragDrop);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the image from an ImageData object to a temporary file.
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        private string SaveImageDataToTempFile(ImageData imageData)
        {
            string savedImagePath = null;

            if (imageData.Bitmap != null)
            {
                //save the Bitmap data as a PNG
                savedImagePath = TempFileManager.Instance.CreateTempFile("image.png");
                SaveBitmapToFile(imageData.Bitmap, savedImagePath);
            }
            else if (imageData.Dib != null)
            {
                //save the DIB data as a PNG
                savedImagePath = TempFileManager.Instance.CreateTempFile("image.png");
                SaveDibToFile(imageData.Dib, savedImagePath);
            }
            else if (imageData.GIF != null)
            {
                //save the GIF data as a GID file
                savedImagePath = TempFileManager.Instance.CreateTempFile("image.gif");
                SaveGifToFile(imageData.GIF, savedImagePath);
            }

            //return the path to the temp file that the image was saved to.
            return savedImagePath;
        }

        /// <summary>
        /// Saves a Bitmap as a PNG.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="filepath"></param>
        private void SaveBitmapToFile(Bitmap bitmap, string filepath)
        {
            bitmap.Save(filepath);
        }

        /// <summary>
        /// Saves a DIB Stream to a file.
        /// </summary>
        /// <param name="dib"></param>
        /// <param name="filepath"></param>
        private void SaveDibToFile(Stream dib, string filepath)
        {
            DIBHelper.DibToFile(dib, filepath);
        }

        /// <summary>
        /// Saves a GIF Stream as a GIF.
        /// </summary>
        /// <param name="gif"></param>
        /// <param name="filepath"></param>
        private void SaveGifToFile(Stream gif, string filepath)
        {
            FileStream fileOut = new FileStream(filepath, FileMode.OpenOrCreate);
            using (fileOut)
            {
                StreamHelper.Transfer(gif, fileOut);
            }
        }

        public override void Dispose()
        {
            _blogEditor = null;
            base.Dispose();
        }
    }

    internal class EmbedFormatHandler : FreeTextHandler
    {
        // TODO: This really needs to insert a snapshot of the embed and then publish the embed to the blog
        //       correctly at publish time -- this is so that:
        //         (1) We don't have to deal with embeds that steal mouse clicks (need it to be
        //             selectable and moveable like any other content)
        ///        (2) We can implement a custom sidebar and other custom behavior like snapshotting
        ///        (3) We don't have to chase down bizzaro editor edge cases caused by complex or misbehaving embeds
        ///
        IContentSourceSite _contentSourceSite;

        public EmbedFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext, IContentSourceSite sourceSite)
            : base(dataObject, handlerContext, editorContext)
        {
            _contentSourceSite = sourceSite;
        }

        public static bool CanCreateFrom(DataObjectMeister data)
        {
            if (!GlobalEditorOptions.SupportsFeature(ContentEditorFeature.UrlContentSourcePaste))
                return false;

            return TextDataContainsEmbed(data);
        }

        protected override bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end)
        {
            //might be a video
            VideoProvider provider = VideoProviderManager.FindProviderFromEmbed(DataMeister.TextData.Text);
            if (provider != null)
            {
                // HACK: drive the selection textRange to the caret so we can call generic
                // content-source routines that work off the current selection
                EditorContext.MarkupServices.CreateMarkupRange(begin, end).ToTextRange().select();
                try
                {
                    return InsertSmartContentFromEmbed(DataMeister.TextData.Text);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception inserting content: " + ex.ToString());
                    return false;
                }
            }
            else //unknown embed
            {
                try
                {
                    using (new WaitCursor())
                    {
                        // insert the content
                        EditorContext.InsertHtml(begin, end, DataMeister.TextData.Text, null);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception while pasting EMBED: " + ex.ToString());
                    return false;
                }
            }
        }

        private bool InsertSmartContentFromEmbed(string embed)
        {
            ContentSourceInfo contentSource = ContentSourceManager.FindContentSource(typeof(VideoContentSource));

            if (contentSource != null)
            {
                SmartContentSource smartSource = contentSource.Instance as SmartContentSource;
                VideoContentSource videoSource = smartSource as VideoContentSource;
                IExtensionData extensionData = _contentSourceSite.CreateExtensionData(Guid.NewGuid().ToString());
                ISmartContent smartContent = new SmartContent(extensionData);
                videoSource.CreateContentFromEmbed(embed, smartContent);
                // generate html and insert it
                string content = videoSource.GenerateEditorHtml(smartContent, _contentSourceSite);
                if (content != null)
                {
                    _contentSourceSite.InsertContent(VideoContentSource.ID, content, extensionData);
                    return true;
                }
                else
                {
                    Trace.Fail("Video Source content generated from embed tag was empty");
                    return false;
                }
            }
            Trace.Fail("Cannot find the video plugin");
            return false;
        }

        private static bool TextDataContainsEmbed(DataObjectMeister data)
        {
            try
            {
                if (data.TextData != null)
                {
                    string textData = data.TextData.Text.Trim().ToLower(CultureInfo.InvariantCulture);
                    return ContainsEmbedOrObject(textData);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception checking for EMBED tag: " + ex.ToString());
                return false;
            }
        }

        private static bool ContainsEmbedOrObject(string html)
        {
            try
            {
                IElementPredicate predicate = new OrPredicate(new BeginTagPredicate("embed"), new BeginTagPredicate("object"), new BeginTagPredicate("iframe"));
                HtmlExtractor ex = new HtmlExtractor(html);
                return ex.Seek(predicate).Success;
            }
            catch
            {
                return false;
            }
        }
    }

}
