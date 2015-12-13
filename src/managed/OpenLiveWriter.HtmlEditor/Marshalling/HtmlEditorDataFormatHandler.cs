// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    /// <summary>
    /// Summary description for HtmlEditorDataFormatHandler.
    /// </summary>
    public abstract class HtmlEditorDataFormatHandler : DataFormatHandler
    {
        protected HtmlEditorDataFormatHandler(DataObjectMeister dataObject, DataFormatHandlerContext handlerContext, IHtmlMarshallingTarget editorContext)
            : base(dataObject, handlerContext)
        {
            this.editorContext = editorContext;
        }

        public override bool InsertData(DataAction action, params object[] args)
        {
            Debug.Assert(args.Length == 2 && args[0] is MarkupPointer && args[1] is MarkupPointer, "Illegal insertion parameters: expected (DataAction action, MarkupPointer begin, MarkupPointer end)");
            return DoInsertData(action, (MarkupPointer)args[0], (MarkupPointer)args[1]);
        }

        /// <summary>
        /// Instruct the handler to insert data into the presentation editor
        /// </summary>
        protected abstract bool DoInsertData(DataAction action, MarkupPointer begin, MarkupPointer end);

        /// <summary>
        /// Presentation editor context for handling data
        /// </summary>
        protected IHtmlMarshallingTarget EditorContext { get { return editorContext; } }

        // private members accessed via properties by subclasses
        private IHtmlMarshallingTarget editorContext;
    }
}
