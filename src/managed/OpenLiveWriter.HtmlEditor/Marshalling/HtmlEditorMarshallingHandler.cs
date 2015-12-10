// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using OpenLiveWriter.HtmlEditor.Marshalling.Data_Handlers;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    /// <summary>
    /// Summary description for HtmlEditorMarshallingHandler.
    /// </summary>
    public class HtmlEditorMarshallingHandler : IDataFormatHandlerFactory
    {
        IHtmlMarshallingTarget editorContext;
        protected DataFormatHandlerRegistry dataFormatRegistry;
        public HtmlEditorMarshallingHandler(IHtmlMarshallingTarget editorContext)
        {
            this.editorContext = editorContext;
            dataFormatRegistry = new DataFormatHandlerRegistry();

            dataFormatRegistry.Register(CreateDataFormatFactories());

        }

        protected virtual IDataFormatHandlerFactory[] CreateDataFormatFactories()
        {
            return new IDataFormatHandlerFactory[]{
                new DelegateBasedDataFormatHandlerFactory(new DataFormatHandlerCreate(CreateUrlDataFormatHandler), new DataObjectFilter(CanCreateUrlFormatHandler)),
                new DelegateBasedDataFormatHandlerFactory(new DataFormatHandlerCreate(CreateImageOnlyHtmlDataFormatHandler), new DataObjectFilter(CanCreateImageOnlyHtmlFormatHandler)),
                new DelegateBasedDataFormatHandlerFactory(new DataFormatHandlerCreate(CreateFileDataFormatHandler), new DataObjectFilter(CanCreateFileFormatHandler)),
                new DelegateBasedDataFormatHandlerFactory(new DataFormatHandlerCreate(CreateHtmlDataFormatHandler), new DataObjectFilter(CanCreateHtmlFormatHandler)),
                new DelegateBasedDataFormatHandlerFactory(new DataFormatHandlerCreate(CreateTextDataFormatHandler), new DataObjectFilter(CanCreateTextFormatHandler))
           };
        }

        public DataFormatHandler CreateFrom(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return dataFormatRegistry.CreateFrom(dataMeister, handlerContext);
        }

        protected virtual bool CanCreateUrlFormatHandler(DataObjectMeister dataObject)
        {
            return editorContext.MarshalUrlSupported && UrlHandler.CanCreateFrom(dataObject);
        }

        protected virtual bool CanCreateImageOnlyHtmlFormatHandler(DataObjectMeister dataObject)
        {
            return editorContext.MarshalImagesSupported && ImageOnlyHtmlHandler.CanCreateFrom(dataObject);
        }

        protected virtual bool CanCreateFileFormatHandler(DataObjectMeister dataObject)
        {
            return editorContext.MarshalFilesSupported && FileHandler.CanCreateFrom(dataObject);
        }

        protected virtual bool CanCreateHtmlFormatHandler(DataObjectMeister dataObject)
        {
            return (editorContext.MarshalHtmlSupported || editorContext.MarshalTextSupported) && HtmlHandler.CanCreateFrom(dataObject);
        }

        protected virtual bool CanCreateTextFormatHandler(DataObjectMeister dataObject)
        {
            return editorContext.MarshalTextSupported && PlainTextHandler.CanCreateFrom(dataObject);
        }

        protected virtual DataFormatHandler CreateUrlDataFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new UrlHandler(dataMeister, handlerContext, editorContext);
        }

        protected virtual DataFormatHandler CreateImageOnlyHtmlDataFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new ImageOnlyHtmlHandler(dataMeister, handlerContext, editorContext);
        }

        protected virtual DataFormatHandler CreateFileDataFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new FileHandler(dataMeister, handlerContext, editorContext);
        }

        protected virtual DataFormatHandler CreateHtmlDataFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new HtmlHandler(dataMeister, handlerContext, editorContext);
        }

        protected virtual DataFormatHandler CreateTextDataFormatHandler(DataObjectMeister dataMeister, DataFormatHandlerContext handlerContext)
        {
            return new PlainTextHandler(dataMeister, handlerContext, editorContext);
        }

        public virtual bool CanCreateFrom(DataObjectMeister data)
        {
            return dataFormatRegistry.CanCreateFrom(data);
        }

        protected IHtmlMarshallingTarget EditorContext
        {
            get
            {
                return editorContext;
            }
        }

        public void Dispose()
        {
            if (dataFormatRegistry != null)
            {
                dataFormatRegistry.Dispose();
                dataFormatRegistry = null;
            }
        }
    }
}
