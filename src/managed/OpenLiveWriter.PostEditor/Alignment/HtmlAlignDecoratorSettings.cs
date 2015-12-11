// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor
{
    // Though center is a value, img tags do not support align=center
    public enum ImgAlignment { NONE, LEFT, RIGHT, BOTTOM, TOP, BASELINE, TEXTTOP, MIDDLE, ABSMIDDLE, CENTER };

    public class HtmlAlignDecoratorSettings
    {
        private static readonly string DEFAULT_ALIGNMENT = "DefaultAlignment";
        private IHTMLElement _element;
        IProperties Settings;
        private ImageDecoratorInvocationSource _invocationSource = ImageDecoratorInvocationSource.Unknown;

        public HtmlAlignDecoratorSettings(IHTMLElement element)
        {
            _element = element;
            Settings = null;
        }

        public HtmlAlignDecoratorSettings(IProperties settings, IHTMLElement element)
        {
            _element = element;
            Settings = settings;
        }

        public HtmlAlignDecoratorSettings(IProperties settings, IHTMLElement element, ImageDecoratorInvocationSource invocationSource)
        {
            _element = element;
            Settings = settings;
            _invocationSource = invocationSource;
        }

        public ImgAlignment DefaultAlignment
        {
            get
            {
                string align = Settings.GetString(DEFAULT_ALIGNMENT, ImgAlignment.NONE.ToString());
                try
                {
                    return (ImgAlignment)ImgAlignment.Parse(typeof(ImgAlignment), align);
                }
                catch (Exception)
                {
                    return ImgAlignment.NONE;
                }
            }
            set
            {
                Settings.SetString(DEFAULT_ALIGNMENT, value.ToString());
            }
        }

        public ImgAlignment Alignment
        {
            get
            {
                return GetAlignmentFromHtml();
            }
            set
            {
                SetImageHtmlFromAlignment(value);
            }
        }

        // Set the alignment of the image
        internal void SetImageHtmlFromAlignment(ImgAlignment value)
        {
            bool needToSelectImage = false;
            switch (value)
            {
                case ImgAlignment.NONE:
                    // If we removed the centering node, we need to reselect the image since the selection
                    // is invalidated/changed as a result of removing the node.
                    needToSelectImage = RemoveCenteringNode();

                    _element.removeAttribute("align", 0);
                    _element.style.display = "inline";
                    _element.style.styleFloat = null;

                    if (_element.style.marginLeft != null && _element.style.marginLeft.ToString() == "auto")
                        _element.style.marginLeft = 0;
                    if (_element.style.marginRight != null && _element.style.marginRight.ToString() == "auto")
                        _element.style.marginRight = 0;

                    break;
                case ImgAlignment.LEFT:
                case ImgAlignment.RIGHT:
                    // If we removed the centering node, we need to reselect the image since the selection
                    // is invalidated/changed as a result of removing the node.
                    needToSelectImage = RemoveCenteringNode();

                    _element.style.display = "inline";
                    _element.style.styleFloat = value.ToString().ToLower(CultureInfo.InvariantCulture);
                    if (_element.style.marginLeft != null && _element.style.marginLeft.ToString() == "auto")
                        _element.style.marginLeft = 0;
                    if (_element.style.marginRight != null && _element.style.marginRight.ToString() == "auto")
                        _element.style.marginRight = 0;
                    // For all other types of alignment we just set the align property on the image
                    _element.setAttribute("align", value.ToString().ToLower(CultureInfo.InvariantCulture), 0);

                    break;
                case ImgAlignment.CENTER:
                    _element.removeAttribute("align", 0);
                    _element.style.styleFloat = null;

                    if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.CenterImageWithParagraph))
                    {
                        IHTMLElement element = FindCenteringNode();
                        if (element == null)
                        {
                            // There is no existing centering node, we need to create a new one.
                            // Creating the new centering node invalidates/changes the existing selection
                            // so we need to reselect the image.
                            needToSelectImage = true;
                            element = CreateNodeForCentering();
                        }
                        if (element != null)
                        {
                            element.setAttribute("align", "center", 0);
                        }
                    }
                    else
                    {
                        _element.style.display = "block";
                        _element.style.styleFloat = "none";
                        _element.style.marginLeft = "auto";
                        _element.style.marginRight = "auto";
                    }
                    break;
                default:
                    Trace.Fail("Unknown image alignment: " + value.ToString());
                    break;
            }

            if (needToSelectImage)
            {
                // If we need to reselect the image, do it after we have set the right
                // alignment in the element above so that when the selection change event
                // refreshes the ribbon commands using the html doc, it sees the new values.
                SelectImage();
            }
        }

        private void SelectImage()
        {
            // We don't want a selection changed event to fire during the initial insert.
            if (_invocationSource == ImageDecoratorInvocationSource.InitialInsert)
                return;

            IHTMLTextContainer textContainer = ((IHTMLDocument2)_element.document).body as IHTMLTextContainer;
            IHTMLControlRange controlRange = textContainer.createControlRange() as IHTMLControlRange;
            controlRange.add(_element as IHTMLControlElement);
            controlRange.select();
        }

        // Detects the alignment of an element, which could be an image or smart content div
        internal ImgAlignment GetAlignmentFromHtml()
        {
            // Try and see if this is an img, if it is we will be able to read
            // the align attribute right off of it.
            IHTMLImgElement image = (_element as IHTMLImgElement);
            if (image != null)
            {
                // Check and see if the align attribute has been set
                string align = image.align;
                if (align != null)
                {
                    align = align.ToLower(CultureInfo.InvariantCulture).Trim();

                    // If it has been, then just check to see what type
                    // of alignment has already been set.
                    switch (align)
                    {
                        case "left":
                            return ImgAlignment.LEFT;
                        case "right":
                            return ImgAlignment.RIGHT;
                        case "top":
                            return ImgAlignment.TOP;
                        case "bottom":
                            return ImgAlignment.BOTTOM;
                        case "middle":
                            return ImgAlignment.MIDDLE;
                        case "absmiddle":
                            return ImgAlignment.ABSMIDDLE;
                        case "baseline":
                            return ImgAlignment.BASELINE;
                        case "texttop":
                            return ImgAlignment.TEXTTOP;
                    }
                }
            }

            // Check to see if the element has a float right on it
            if (_element.style.styleFloat == "right")
            {
                return ImgAlignment.RIGHT;
            }
            // Check to see if the element has a float left
            if (_element.style.styleFloat == "left")
            {
                return ImgAlignment.LEFT;
            }

            if ((_element.style.styleFloat == "none" || String.IsNullOrEmpty(_element.style.styleFloat)) && _element.style.display == "block" && _element.style.marginLeft as string == "auto" && _element.style.marginRight as string == "auto")
            {
                return ImgAlignment.CENTER;
            }

            IHTMLElement centeringNode = FindCenteringNode();
            if (centeringNode != null)
            {
                if (IsCenteringNode(centeringNode))
                    return ImgAlignment.CENTER;
            }

            // We didnt find anything, so no alignment could be found.
            return ImgAlignment.NONE;
        }

        private int CountVisibleElements(IHTMLElement node)
        {
            if (node == null)
                return 0;

            int count = 0;
            IHTMLElementCollection all = (IHTMLElementCollection)node.all;
            foreach (IHTMLElement child in all)
            {
                if (ElementFilters.IsVisibleEmptyElement(child))
                {
                    count++;
                }
            }
            return count;
        }

        internal IHTMLElement FindCenteringNode()
        {
            // Create markup services using the element's document that we are analyzing
            MshtmlMarkupServices MarkupServices = new MshtmlMarkupServices(_element.document as IMarkupServicesRaw);
            // Create a pointer and move it to before the beginning of its opening tag
            MarkupPointer start = MarkupServices.CreateMarkupPointer();
            start.MoveAdjacentToElement(_element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            // Find the block parent of this node.
            IHTMLElement blockParent = start.CurrentBlockScope();

            // Check to see if the block parent is actually a centering node.
            if (IsBodyElement((IHTMLElement3)blockParent) || !IsCenteringNode(blockParent))
            {
                blockParent = null;
            }

            // Make sure that if we do have a block parents, we are the only thing inside it.
            // Since we are going to edit the block, we dont want other stuff in there that
            // will also be changed
            if (blockParent != null)
            {
                string innerHtml = ((IHTMLElement)blockParent).innerText ?? "";
                if (!string.IsNullOrEmpty(innerHtml.Trim()))
                {
                    blockParent = null;
                }
                else
                {
                    int numElements = CountVisibleElements((IHTMLElement)blockParent);
                    if (numElements != 1)
                    {
                        blockParent = null;
                    }
                }
            }

            return blockParent;
        }

        private bool IsCenteringNode(IHTMLElement element)
        {
            return GlobalEditorOptions.SupportsFeature(ContentEditorFeature.CenterImageWithParagraph) && element != null &&
                element is IHTMLParaElement && element.getAttribute("align", 2) as string == "center";
        }

        private bool IsBodyElement(IHTMLElement3 element)
        {
            return element != null && element.contentEditable == "true" &&
                   (((IHTMLElement)element).className == "postBody" || element is IHTMLBodyElement);
        }

        private IHTMLElement CreateNodeForCentering()
        {
            // Create markup services using the element's document that we are analyzing
            MshtmlMarkupServices MarkupServices = new MshtmlMarkupServices(_element.document as IMarkupServicesRaw);
            MarkupPointer end = MarkupServices.CreateMarkupPointer();
            MarkupPointer start = MarkupServices.CreateMarkupPointer();

            // Find the element that we will want to wrap.
            IHTMLElement elementToEncapsulate = _element;

            // If the elements parent is an A, we will also want to
            // wrap the A and not just the image inside
            if (_element.parentElement.tagName == "A")
            {
                elementToEncapsulate = _element.parentElement;
            }

            // Move the starting pointer to before the beginning of the element we want to wrap
            start.MoveAdjacentToElement(elementToEncapsulate, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);

            // Find this elements parent
            IHTMLElement3 currentBlockScope = start.CurrentBlockScope() as IHTMLElement3;
            // If its parent is also the div that is around the post
            // we need to actually create a new div and just put it around the element

            // If it is splittable block, split it
            // e.g "<DIV>Blah<IMG/>Blah</DIV>" => "<DIV>Blah</DIV><DIV><IMG/></DIV><DIV>Blah</DIV>"
            if (!IsBodyElement(currentBlockScope))
            {
                // We are in a block that can be split so split it at the beginning and end
                MarkupHelpers.SplitBlockForInsertionOrBreakout(MarkupServices, null, start);
                end.MoveAdjacentToElement(elementToEncapsulate, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                MarkupHelpers.SplitBlockForInsertionOrBreakout(MarkupServices, null, end);

                // Position start back to the beginning of our element
                start.MoveAdjacentToElement(elementToEncapsulate, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            }

            // Now we can wrap it in an P tag (centering node)
            end.MoveAdjacentToElement(elementToEncapsulate, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            IHTMLElement centeringElement = MarkupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_P, string.Empty);
            MarkupServices.InsertElement(centeringElement, start, end);
            return centeringElement;
        }

        internal bool RemoveCenteringNode()
        {
            MshtmlMarkupServices MarkupServices = new MshtmlMarkupServices(_element.document as IMarkupServicesRaw);
            IHTMLElement element = FindCenteringNode();

            // We couldnt find a parent, so nothing to remove
            if (element == null) return false;

            MarkupPointer start = MarkupServices.CreateMarkupPointer();
            MarkupPointer end = MarkupServices.CreateMarkupPointer();
            MarkupPointer target = MarkupServices.CreateMarkupPointer();

            // Move the stuff inside the smart content container outside of itself
            start.MoveAdjacentToElement(element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            end.MoveAdjacentToElement(element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
            target.MoveAdjacentToElement(element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            MarkupServices.Move(start, end, target);

            // remove the empty smart content container
            start.MoveAdjacentToElement(element, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            end.MoveAdjacentToElement(element, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
            MarkupServices.Remove(start, end);

            return true;
        }
    }
}
