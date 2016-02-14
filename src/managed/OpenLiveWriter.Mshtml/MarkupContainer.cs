// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// This class is a convenience wrapper for the MSHTML IMarkupContainer interface.
    /// </summary>
    public class MarkupContainer
    {
        internal readonly IMarkupContainerRaw Container;
        private readonly MshtmlMarkupServices MarkupServices;

        internal MarkupContainer(MshtmlMarkupServices markupServices, IMarkupContainerRaw container)
        {
            MarkupServices = markupServices;
            Container = container;
        }

        private IHTMLDocument2 document;
        /// <summary>
        /// Returns the document object hosted by this container.
        /// </summary>
        public IHTMLDocument2 Document
        {
            get
            {
                if (document == null)
                {
                    //create a temp pointer that can walk the document until it finds an element
                    MarkupPointer p = MarkupServices.CreateMarkupPointer();
                    p.MoveToContainer(this, true);

                    IHTMLElement currentElement = p.CurrentScope;
                    if (currentElement == null)
                    {
                        MarkupContext markupContext = new MarkupContext();
                        p.Right(true, markupContext);
                        while (markupContext.Element == null && markupContext.Context != _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None)
                            p.Right(true, markupContext);
                        currentElement = markupContext.Element;
                    }
                    if (currentElement != null)
                        document = (IHTMLDocument2)currentElement.document;
                }
                return document;
            }
        }

        /// <summary>
        /// Retrieves the top-level document associated with this object.
        /// Note: Use the GetDocument method to retrieve the document object hosted by this container.
        /// </summary>
        public IHTMLDocument2 GetOwningDoc()
        {
            IHTMLDocument2 doc;
            Container.OwningDoc(out doc);
            return doc;
        }

        /// <summary>
        /// Create an unposition text range within this container.
        /// </summary>
        /// <returns></returns>
        public IHTMLTxtRange CreateTextRange()
        {
            return (Document.body as IHTMLBodyElement).createTextRange();
        }

        /// <summary>
        /// Create a text range spanning the specified markup positions.
        /// </summary>
        /// <param name="start">the start point of the text range</param>
        /// <param name="end">the end point of the text range</param>
        /// <returns></returns>
        public IHTMLTxtRange CreateTextRange(MarkupPointer start, MarkupPointer end)
        {
            // when switching between wysiwyg and source view sometimes .body is null
            // and this throws a null ref exception that we catch (can be detected by enabling
            // exception breaking in the debugger)
            IHTMLTxtRange range = (Document.body as IHTMLBodyElement).createTextRange();
            MarkupServices.MoveRangeToPointers(start, end, range);
            return range;
        }

        /// <summary>
        /// Get the container's HTML content.
        /// </summary>
        public string HtmlText
        {
            get
            {
                return MarkupServices.GetHtmlText(CreateMarkupPointer(POSITION.DOCUMENT_START), CreateMarkupPointer(POSITION.DOCUMENT_END));
            }
        }

        /// <summary>
        /// Create a markup pointer positioned within the container.
        /// </summary>
        /// <returns></returns>
        public MarkupPointer CreateMarkupPointer(POSITION initialPosition)
        {
            MarkupPointer p = MarkupServices.CreateMarkupPointer();
            switch (initialPosition)
            {
                case POSITION.DOCUMENT_START:
                    p.MoveToContainer(this, true);
                    break;

                case POSITION.BODY_START:
                    p.MoveAdjacentToElement(Document.body, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                    break;

                case POSITION.BODY_END:
                    p.MoveAdjacentToElement(Document.body, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                    break;

                case POSITION.DOCUMENT_END:
                    p.MoveToContainer(this, false);
                    break;
            }

            return p;
        }

        public enum POSITION { DOCUMENT_START = 0, BODY_START = 1, BODY_END = 2, DOCUMENT_END = 3 };
    }
}
