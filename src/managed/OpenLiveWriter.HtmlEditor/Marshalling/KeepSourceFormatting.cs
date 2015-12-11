// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class KeepSourceFormatting
    {
        private IHTMLDocument2 sourceDocument;
        private MshtmlMarkupServices sourceMarkupServices;
        private MarkupRange sourceRange;
        private IHTMLDocument2 destinationDocument;
        private MshtmlMarkupServices destinationMarkupServices;
        private MarkupRange destinationRange;
        private IHTMLElementFilter isNonSupportedStrikeThroughElement;
        private IHTMLElementFilter isBrElement;
        private IHTMLElementFilter supportsAlignmentAttribute;
        private IHTMLElementFilter supportsPercentageWidthAttribute;
        private IHTMLElementFilter supportsPixelWidthAttribute;
        private IHTMLElementFilter supportsPercentageHeightAttribute;
        private IHTMLElementFilter supportsPixelHeightAttribute;

        public KeepSourceFormatting(MarkupRange sourceRange, MarkupRange destinationRange)
        {
            Debug.Assert(sourceRange.Start.Container.GetOwningDoc() == destinationRange.Start.Container.GetOwningDoc(),
                "Ranges must share an owning document!");

            if (sourceRange == null)
            {
                throw new ArgumentNullException("sourceRange");
            }

            if (!sourceRange.Positioned)
            {
                throw new ArgumentException("sourceRange must be positioned.");
            }

            if (sourceRange.Start.IsRightOf(sourceRange.End))
            {
                throw new ArgumentException("sourceRange start must be before range end.");
            }

            if (destinationRange == null)
            {
                throw new ArgumentNullException("destinationRange");
            }

            if (!destinationRange.Positioned)
            {
                throw new ArgumentException("destinationRange must be positioned.");
            }

            if (destinationRange.Start.IsRightOf(destinationRange.End))
            {
                throw new ArgumentException("destinationRange start must be before range end.");
            }

            this.sourceDocument = sourceRange.Start.Container.Document;
            this.sourceMarkupServices = new MshtmlMarkupServices((IMarkupServicesRaw)this.sourceDocument);
            this.sourceRange = sourceRange.Clone();
            this.sourceRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            this.sourceRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

            this.destinationDocument = destinationRange.Start.Container.Document;
            this.destinationMarkupServices = new MshtmlMarkupServices((IMarkupServicesRaw)this.destinationDocument);
            this.destinationRange = destinationRange.Clone();
            this.destinationRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            this.destinationRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
        }

        /// <summary>
        /// Pastes the source HTML over the destination HTML and makes necessary modifications to keep the source
        /// formatting.
        /// </summary>
        /// <returns>A MarkupRange containing the pasted HTML or null if unsuccessful.</returns>
        public MarkupRange Execute()
        {
            NormalizeSourceHtml();
            PasteSourceOverDestination();
            return FixupDestinationFormatting();
        }

        /// <summary>
        /// Gets the source HTML into a good state before attempting to do the paste.
        /// </summary>
        private void NormalizeSourceHtml()
        {
            var elementsToRemove = new List<IHTMLElement>();
            var elementsToReplace = new Dictionary<IHTMLElement, _ELEMENT_TAG_ID>();

            sourceRange.WalkRange(
                delegate (MarkupRange currentRange, MarkupContext context, string text)
                    {
                        if (IsBeginTag(context))
                        {
                            Debug.Assert(context.Element != null, "Element should not be null!");

                            // Some tags are useless and should be removed.
                            if (ShouldRemoveElement(context.Element))
                            {
                                elementsToRemove.Add(context.Element);
                                return true;
                            }

                            // Some tags aren't natively supported by MSHTML's editing commands and should be replaced.
                            _ELEMENT_TAG_ID replacement;
                            if (TryGetReplacement(context.Element, out replacement))
                            {
                                elementsToReplace.Add(context.Element, replacement);
                                return true;
                            }
                        }

                        // Continue walking range.
                        return true;

                    }, false);

            elementsToRemove.ForEach(e => HTMLElementHelper.RemoveElement(e));

            MarkupRange replacementRange = sourceMarkupServices.CreateMarkupRange();
            replacementRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            replacementRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            replacementRange.Start.Cling = false;
            replacementRange.End.Cling = false;

            foreach (KeyValuePair<IHTMLElement, _ELEMENT_TAG_ID> keyValuePair in elementsToReplace)
            {
                IHTMLElement sourceElement = keyValuePair.Key;
                _ELEMENT_TAG_ID replacementTag = keyValuePair.Value;

                // Position the range around the outside of the source element.
                replacementRange.MoveToElement(sourceElement, true);

                // Create the replacement element.
                IHTMLElement replacementElement = sourceMarkupServices.CreateElement(replacementTag, string.Empty);
                HTMLElementHelper.CopyAttributes(sourceElement, replacementElement);

                // Do the replacement.
                sourceMarkupServices.RemoveElement(sourceElement);
                sourceMarkupServices.InsertElement(replacementElement, replacementRange.Start, replacementRange.End);
            }

            // Walking through the range skips input elements, so we need to explicitly enumerate them. Specifically
            // we're looking for <input type="hidden" />.
            IHTMLDocument3 document3 = (IHTMLDocument3)sourceRange.Start.Container.Document;
            foreach (IHTMLElement element in document3.getElementsByTagName("input"))
            {
                IHTMLInputElement inputElement = (IHTMLInputElement)element;
                if (String.Compare(inputElement.type, "hidden", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    HTMLElementHelper.RemoveElement(element);
                }
            }
        }

        /// <summary>
        /// Returns true if the element is useless and should be removed.
        /// </summary>
        /// <param name="element">The element to consider removing.</param>
        /// <returns>true if the element should be removed and false otherwise.</returns>
        private bool ShouldRemoveElement(IHTMLElement element)
        {
            if (element is IHTMLCommentElement)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the element is not natively supported by MSHTML's editing commands and should therefore be
        /// replaced.
        /// </summary>
        /// <param name="element">The element to consider replacing.</param>
        /// <param name="tagId">The tag to replace the element with.</param>
        /// <returns>true if the element should be replaced and false otherwise.</returns>
        private bool TryGetReplacement(IHTMLElement element, out _ELEMENT_TAG_ID tagId)
        {
            if (element != null)
            {
                if (IsNonSupportedStrikeThroughElement(element))
                {
                    tagId = _ELEMENT_TAG_ID.TAGID_STRIKE;
                    return true;
                }
            }

            tagId = _ELEMENT_TAG_ID.TAGID_UNKNOWN;
            return false;
        }

        /// <summary>
        /// MSHTML does not natively support adding/removing the &lt;s&gt; tag which is the equivalent of the
        /// &lt;strike&gt; tag.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns>true if the element is a &lt;s&gt; tag and false otherwise.</returns>
        private bool IsNonSupportedStrikeThroughElement(IHTMLElement element)
        {
            if (isNonSupportedStrikeThroughElement == null)
            {
                isNonSupportedStrikeThroughElement = ElementFilters.CreateElementNameFilter(this.sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_S));
            }

            return isNonSupportedStrikeThroughElement(element);
        }

        /// <summary>
        /// Removes the destination range in a manner similar to HtmlEditorControl.InsertHtml and then pastes in the
        /// source range.
        /// </summary>
        private void PasteSourceOverDestination()
        {
            // Try to be like HtmlEditorControl.InsertHtml. Any changes in this code may also need to be changed in
            // HtmlEditorControl.InsertHtml! We do this because the destination can change (and therefore the styles
            // applied to that destination) based on what happens when we remove and collapse the destinationRange.
            MarkupPointerMoveHelper.PerformImageBreakout(destinationRange.Start);
            MarkupPointerMoveHelper.PerformImageBreakout(destinationRange.End);

            // Delete the destination since we're pasting over it.
            destinationRange.RemoveContent();
            destinationRange.Collapse(true);

            // If the source contains only inline elements, we don't need to split the destination.
            if (sourceRange.ContainsElements(ElementFilters.IsBlockOrTableElement))
            {
                MarkupRange bounds = destinationMarkupServices.CreateMarkupRange(destinationDocument.body, false);
                MarkupHelpers.SplitBlockForInsertionOrBreakout(destinationMarkupServices, bounds, destinationRange.Start);
                destinationRange.Collapse(true);
            }

            destinationRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            destinationRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

            // Paste the source selection into the destination.
            destinationMarkupServices.Copy(sourceRange.Start, sourceRange.End, destinationRange.Start);
        }

        /// <summary>
        /// Copies any missing source formatting over to the destination.
        /// </summary>
        private MarkupRange FixupDestinationFormatting()
        {
            // We'll use the concept of a "fixup segment" to describe a destination segment that needs to be
            // reformatted to match its corresponding source segment. This is very similar to what MSHTML does
            // internally.
            var openFixupSegments = new Dictionary<Type, FixupSegment>();
            var registeredFixupSegments = new List<FixupSegment>();

            // We'll use these to track our current range as we scan through the source and destination ranges.
            MarkupRange currentSource = sourceMarkupServices.CreateMarkupRange(sourceRange.Start.Clone(), sourceRange.Start.Clone());
            MarkupRange currentDestination = destinationMarkupServices.CreateMarkupRange(destinationRange.Start.Clone(), destinationRange.Start.Clone());

            MarkupContext sourceContext = MoveNext(currentSource);
            MarkupContext destinationContext = MoveNext(currentDestination);

            // Loop through the source and destination at the same time, scanning to the right.
            while (currentSource.End.IsLeftOfOrEqualTo(sourceRange.End))
            {
                Trace.Assert(sourceContext.Context == destinationContext.Context, "Mismatched contexts!");
                Trace.Assert((sourceContext.Element == null && destinationContext.Element == null) ||
                             (sourceContext.Element != null && destinationContext.Element != null &&
                              String.Compare(sourceContext.Element.tagName, destinationContext.Element.tagName, StringComparison.OrdinalIgnoreCase) == 0),
                             "Mismatched tags!");

                // If it is an image, add attribute marker to suppress applying default values for image decorators
                if (sourceContext.Element != null && destinationContext.Element != null &&
                    string.Compare(sourceContext.Element.tagName, "IMG", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    destinationContext.Element.setAttribute("wlNoDefaultDecorator", "true", 0);
                }

                if (IsBeginTag(sourceContext))
                {
                    // Fix up all but text-related formatting.
                    CopyMinimumCss(sourceContext.Element, destinationContext.Element);
                    RemoveTextRelatedInlineCss(destinationContext.Element);
                    MoveCssPropertiesToHtmlAttributes(sourceContext.Element, destinationContext.Element);
                }

                if (IsText(sourceContext))
                {
                    // Fix up text-related formatting.
                    TextStyles sourceStyles = new TextStyles(currentSource.Start);
                    TextStyles destinationStyles = new TextStyles(currentDestination.Start);

                    IEnumerator<TextStyle> sourceEnumerator = sourceStyles.GetEnumerator();
                    IEnumerator<TextStyle> destinationEnumerator = destinationStyles.GetEnumerator();
                    while (sourceEnumerator.MoveNext() && destinationEnumerator.MoveNext())
                    {
                        TextStyle sourceStyle = sourceEnumerator.Current;
                        TextStyle destinationStyle = destinationEnumerator.Current;

                        FixupSegment fixupSegment;
                        if (openFixupSegments.TryGetValue(destinationStyle.GetType(), out fixupSegment))
                        {
                            // We've moved into a new range, so we may want to end an open fixup segment.
                            if (ShouldEndFixupSegment(sourceStyle, destinationStyle, fixupSegment))
                            {
                                registeredFixupSegments.Add(fixupSegment);
                                openFixupSegments.Remove(destinationStyle.GetType());
                                fixupSegment = null;
                            }
                        }

                        // We've moved into a new range, so we may want to start a new fixup segment.
                        if (ShouldStartFixupSegment(sourceStyle, destinationStyle, fixupSegment))
                        {
                            openFixupSegments[destinationStyle.GetType()] = new FixupSegment(currentDestination, sourceStyle);
                        }
                        else if (fixupSegment != null)
                        {
                            // There's an open fixup segment for this style and we don't want to start a new one.
                            // That must mean we want to extend the current one.
                            fixupSegment.RangeToFixup.End.MoveToPointer(currentDestination.End);
                        }
                    }
                }
                else if (IsEndTag(sourceContext) && !IsInlineElement(sourceContext.Element))
                {
                    // We're moving out of a block element and <font> tags cannot wrap block elements so we need to
                    // end any open fixup segments.
                    EndOpenFixupSegments(openFixupSegments, registeredFixupSegments);
                }

                // Move along.
                sourceContext = MoveNext(currentSource);
                destinationContext = MoveNext(currentDestination);
            }

            EndOpenFixupSegments(openFixupSegments, registeredFixupSegments);

            ExecuteRegisteredFixupSegments(registeredFixupSegments);

            return destinationRange;
        }

        /// <summary>
        /// Advances the provided range to the next forward markup position.
        /// </summary>
        /// <param name="range">The range to move forward.</param>
        /// <returns>The markup context that was just moved into.</returns>
        private MarkupContext MoveNext(MarkupRange range)
        {
            range.Start.MoveToPointer(range.End);
            return range.End.Right(true);
        }

        /// <summary>
        /// Returns true if the current context is an opening tag, assuming the context was retrieved by moving forward.
        /// </summary>
        /// <param name="currentContext">The markup context to examine.</param>
        /// <returns>true if the current context is an opening tag and false otherwise.</returns>
        private bool IsBeginTag(MarkupContext currentContext)
        {
            return currentContext.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope;
        }

        /// <summary>
        /// Copies over the minimum amount of CSS necessary to make the destination element formatted just like the
        /// source element.
        /// </summary>
        /// <param name="sourceElement">The element to copy CSS formatting from.</param>
        /// <param name="destinationElement">The element to copy CSS formatting to.</param>
        public void CopyMinimumCss(IHTMLElement sourceElement, IHTMLElement destinationElement)
        {
            Debug.Assert(sourceElement != null, "Source element must not be null!");
            Debug.Assert(destinationElement != null, "Destination element must not be null!");

            // This is a very long function (see Table of Contents) because there is no convenient way to loop over
            // all the different interfaces and all the different CSS properties while accounting for all the special
            // cases.

            // -----------------
            // Table of Contents
            // -----------------
            //
            //      1.  Interfaces
            //              There are several MSHTML interfaces that deal with CSS properties. The IHTMLStyle
            //              interfaces can retrieve and set *only* inline styles. The IHTMLCurrentStyle interfaces can
            //              retrieve the cascaded styles of the element.
            //
            //      2.  CSS 2.1
            //              This section copies over *all* the CSS 2.1 properties that IE8 implements (see
            //              http://www.w3.org/TR/CSS21/propidx.html for a full list). The CSS properties are listed
            //              alphabetically. IE8 implements all required CSS properties, but does not implement aural
            //              properties (which are optional according to the spec). IE7 does not support a few of the
            //              CSS 2.1 properties, so we can only copy those properties on IE8+.
            //
            //      3.  CSS 3
            //              This section copies over all the CSS 3 properties that IE implements (not many). All of
            //              these have been around since IE 5.5.
            //
            //      See http://msdn.microsoft.com/en-us/library/cc351024(VS.85).aspx for more info about IE's CSS
            //      compatibility.

            // --------------
            // 1.  Interfaces
            // --------------

            // The inline style will return null or empty for all CSS properties that are not explicitly set inline.
            IHTMLStyle destinationInlineStyle = destinationElement.style;
            IHTMLStyle2 destinationInlineStyle2 = (IHTMLStyle2)destinationInlineStyle;
            IHTMLStyle3 destinationInlineStyle3 = (IHTMLStyle3)destinationInlineStyle;
            IHTMLStyle4 destinationInlineStyle4 = (IHTMLStyle4)destinationInlineStyle;
            IHTMLStyle5 destinationInlineStyle5 = (IHTMLStyle5)destinationInlineStyle;

            // IHTMLStyle6 is IE8 only, but we currently support IE7 and IE8.
            IHTMLStyle6 destinationInlineStyle6 = destinationInlineStyle as IHTMLStyle6;

            // The cascaded style will return a valid value for all CSS properties.
            IHTMLCurrentStyle destinationCascadedStyle = ((IHTMLElement2)destinationElement).currentStyle;
            IHTMLCurrentStyle2 destinationCascadedStyle2 = (IHTMLCurrentStyle2)destinationCascadedStyle;
            IHTMLCurrentStyle3 destinationCascadedStyle3 = (IHTMLCurrentStyle3)destinationCascadedStyle;
            IHTMLCurrentStyle4 destinationCascadedStyle4 = (IHTMLCurrentStyle4)destinationCascadedStyle;

            // IHTMLCurrentStyle5 is IE8 only, but we currently support IE7 and IE8.
            IHTMLCurrentStyle5 destinationCascadedStyle5 = destinationCascadedStyle as IHTMLCurrentStyle5;

            // The cascaded style will return a valid value for all CSS properties.
            IHTMLCurrentStyle sourceCascadedStyle = ((IHTMLElement2)sourceElement).currentStyle;
            IHTMLCurrentStyle2 sourceCascadedStyle2 = (IHTMLCurrentStyle2)sourceCascadedStyle;
            IHTMLCurrentStyle3 sourceCascadedStyle3 = (IHTMLCurrentStyle3)sourceCascadedStyle;
            IHTMLCurrentStyle4 sourceCascadedStyle4 = (IHTMLCurrentStyle4)sourceCascadedStyle;

            // IHTMLCurrentStyle5 is IE8 only, but we currently support IE7 and IE8.
            IHTMLCurrentStyle5 sourceCascadedStyle5 = sourceCascadedStyle as IHTMLCurrentStyle5;

            // We will always use these three interfaces together, so the use of this bool is just for convenience.
            bool isAtLeastIE8 = destinationInlineStyle6 != null && destinationCascadedStyle5 != null && sourceCascadedStyle5 != null;

            // -----------
            // 2.  CSS 2.1
            // -----------

            // Can't copy to inlineStyle.background directly, so we'll do each background style separately.

            if (String.Compare(destinationCascadedStyle.backgroundAttachment, sourceCascadedStyle.backgroundAttachment, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.backgroundAttachment = sourceCascadedStyle.backgroundAttachment;
            }

            // Only inline the background color on a block element since we'll use highlighting to get the proper background color on inline elements.
            if (!IsInlineElement(sourceElement))
            {
                if (String.Compare(destinationCascadedStyle.backgroundColor as string, sourceCascadedStyle.backgroundColor as string, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle.backgroundColor = sourceCascadedStyle.backgroundColor;
                }
            }

            if (String.Compare(destinationCascadedStyle.backgroundImage, sourceCascadedStyle.backgroundImage, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.backgroundImage = sourceCascadedStyle.backgroundImage;
            }

            // Can't copy to inlineStyle.backgroundPosition directly, so we'll do each backgroundPosition style separately.

            // We check if the destination already has an inline style specified using em units because we'll be
            // removing the font-size property which will cause the calculated size to change. Therefore we need to
            // convert the ems to an absolute unit.
            if (IsEms(sourceCascadedStyle.backgroundPositionX as string))
            {
                int backgroundPositionXInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBackgroundPositionX, sourceElement, null, false);
                destinationInlineStyle.backgroundPositionX = String.Format(CultureInfo.InvariantCulture, "{0}px", backgroundPositionXInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.backgroundPositionX as string, sourceCascadedStyle.backgroundPositionX as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.backgroundPositionX as string))
            {
                destinationInlineStyle.backgroundPositionX = sourceCascadedStyle.backgroundPositionX;
            }

            if (IsEms(sourceCascadedStyle.backgroundPositionY as string))
            {
                int backgroundPositionYInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBackgroundPositionY, sourceElement, null, true);
                destinationInlineStyle.backgroundPositionY = String.Format(CultureInfo.InvariantCulture, "{0}px", backgroundPositionYInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.backgroundPositionY as string, sourceCascadedStyle.backgroundPositionY as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.backgroundPositionY as string))
            {
                destinationInlineStyle.backgroundPositionY = sourceCascadedStyle.backgroundPositionY;
            }

            if (String.Compare(destinationCascadedStyle.backgroundRepeat, sourceCascadedStyle.backgroundRepeat, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.backgroundRepeat = sourceCascadedStyle.backgroundRepeat;
            }

            // Can't copy to inlineStyle.border directly, so we'll do each border style separately.

            if (String.Compare(destinationCascadedStyle.borderCollapse, sourceCascadedStyle.borderCollapse, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.borderCollapse = sourceCascadedStyle.borderCollapse;
            }

            // Can't copy to inlineStyle.borderColor directly, so we'll do each borderColor style separately.

            if (isAtLeastIE8)
            {
                if (IsEms(sourceCascadedStyle5.borderSpacing))
                {
                    int borderXInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBorderSpacingX, sourceElement, null, false);
                    int borderYInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBorderSpacingY, sourceElement, null, true);
                    destinationInlineStyle6.borderSpacing = String.Format(CultureInfo.InvariantCulture, "{0}px {1}px", borderXInPixels, borderYInPixels);
                }
                else if (String.Compare(destinationCascadedStyle5.borderSpacing, sourceCascadedStyle5.borderSpacing, StringComparison.OrdinalIgnoreCase) != 0 ||
                    IsEms(destinationInlineStyle6.borderSpacing))
                {
                    destinationInlineStyle6.borderSpacing = sourceCascadedStyle5.borderSpacing;
                }
            }

            // Can't copy to inlineStyle.borderStyle directly, so we'll do each borderStyle style separately.
            // Can't copy to inlineStyle.borderWidth directly, so we'll do each borderWidth style separately.
            // Can't copy to inlineStyle.borderBottom directly, so we'll do each borderBottom style separately.

            // Optimization: only copy over the color if there is actually a border.
            if (String.Compare(destinationCascadedStyle.borderBottomColor as string, sourceCascadedStyle.borderBottomColor as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderBottomStyle, "none", StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderBottomColor = sourceCascadedStyle.borderBottomColor;
            }

            if (String.Compare(destinationCascadedStyle.borderBottomStyle, sourceCascadedStyle.borderBottomStyle, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderBottomStyle = sourceCascadedStyle.borderBottomStyle;
            }

            if (IsEms(sourceCascadedStyle.borderBottomWidth as string))
            {
                int borderBottomInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBorderBottom, sourceElement,
                    HTMLElementHelper.LastChanceBorderWidthPointSize, true);
                destinationInlineStyle.borderBottomWidth = String.Format(CultureInfo.InvariantCulture, "{0}px", borderBottomInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.borderBottomWidth as string, sourceCascadedStyle.borderBottomWidth as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderBottomStyle, "none", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.borderBottomWidth as string))
            {
                destinationInlineStyle.borderBottomWidth = sourceCascadedStyle.borderBottomWidth;
            }

            // Can't copy to inlineStyle.borderLeft directly, so we'll do each borderLeft style separately.

            // Optimization: only copy over the color if there is actually a border.
            if (String.Compare(destinationCascadedStyle.borderLeftColor as string, sourceCascadedStyle.borderLeftColor as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderLeftStyle, "none", StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderLeftColor = sourceCascadedStyle.borderLeftColor;
            }

            if (String.Compare(destinationCascadedStyle.borderLeftStyle, sourceCascadedStyle.borderLeftStyle, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderLeftStyle = sourceCascadedStyle.borderLeftStyle;
            }

            if (IsEms(sourceCascadedStyle.borderLeftWidth as string))
            {
                int borderLeftInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBorderLeft, sourceElement,
                    HTMLElementHelper.LastChanceBorderWidthPointSize, false);
                destinationInlineStyle.borderLeftWidth = String.Format(CultureInfo.InvariantCulture, "{0}px", borderLeftInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.borderLeftWidth as string, sourceCascadedStyle.borderLeftWidth as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderLeftStyle, "none", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.borderLeftWidth as string))
            {
                destinationInlineStyle.borderLeftWidth = sourceCascadedStyle.borderLeftWidth;
            }

            // Can't copy to inlineStyle.borderRight directly, so we'll do each borderRight style separately.

            // Optimization: only copy over the color if there is actually a border.
            if (String.Compare(destinationCascadedStyle.borderRightColor as string, sourceCascadedStyle.borderRightColor as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderRightStyle, "none", StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderRightColor = sourceCascadedStyle.borderRightColor;
            }

            if (String.Compare(destinationCascadedStyle.borderRightStyle, sourceCascadedStyle.borderRightStyle, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderRightStyle = sourceCascadedStyle.borderRightStyle;
            }

            if (IsEms(sourceCascadedStyle.borderRightWidth as string))
            {
                int borderRightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBorderRight, sourceElement,
                    HTMLElementHelper.LastChanceBorderWidthPointSize, false);
                destinationInlineStyle.borderRightWidth = String.Format(CultureInfo.InvariantCulture, "{0}px", borderRightInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.borderRightWidth as string, sourceCascadedStyle.borderRightWidth as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderRightStyle, "none", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.borderRightWidth as string))
            {
                destinationInlineStyle.borderRightWidth = sourceCascadedStyle.borderRightWidth;
            }

            // Can't copy to inlineStyle.borderTop directly, so we'll do each borderTop style separately.

            // Optimization: only copy over the color if there is actually a border.
            if (String.Compare(destinationCascadedStyle.borderTopColor as string, sourceCascadedStyle.borderTopColor as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderTopStyle, "none", StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderTopColor = sourceCascadedStyle.borderTopColor;
            }

            if (String.Compare(destinationCascadedStyle.borderTopStyle, sourceCascadedStyle.borderTopStyle, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.borderTopStyle = sourceCascadedStyle.borderTopStyle;
            }

            if (IsEms(sourceCascadedStyle.borderTopWidth as string))
            {
                int borderTopInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBorderTop, sourceElement,
                    HTMLElementHelper.LastChanceBorderWidthPointSize, true);
                destinationInlineStyle.borderTopWidth = String.Format(CultureInfo.InvariantCulture, "{0}px", borderTopInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.borderTopWidth as string, sourceCascadedStyle.borderTopWidth as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.borderTopStyle, "none", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.borderTopWidth as string))
            {
                destinationInlineStyle.borderTopWidth = sourceCascadedStyle.borderTopWidth;
            }

            // Can't copy to inlineStyle.borderTop directly, so we'll do each borderTop style separately.

            if (IsEms(sourceCascadedStyle.bottom as string))
            {
                int bottomInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringBottom, sourceElement, null, true);
                destinationInlineStyle2.bottom = String.Format(CultureInfo.InvariantCulture, "{0}px", bottomInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.bottom as string, sourceCascadedStyle.bottom as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle2.bottom as string))
            {
                destinationInlineStyle2.bottom = sourceCascadedStyle.bottom;
            }

            if (isAtLeastIE8)
            {
                if (String.Compare(destinationCascadedStyle5.captionSide, sourceCascadedStyle5.captionSide, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle6.captionSide = sourceCascadedStyle5.captionSide;
                }
            }

            if (String.Compare(destinationCascadedStyle.clear, sourceCascadedStyle.clear, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.clear = sourceCascadedStyle.clear;
            }

            // Can't copy to inlineStyle.clip directly, so we'll do each clip style separately.

            string clipTop = sourceCascadedStyle.clipTop as string;
            string clipRight = sourceCascadedStyle.clipRight as string;
            string clipBottom = sourceCascadedStyle.clipBottom as string;
            string clipLeft = sourceCascadedStyle.clipLeft as string;

            if (IsEms(clipTop) || IsEms(clipRight) || IsEms(clipBottom) || IsEms(clipLeft))
            {
                if (IsEms(clipTop))
                {
                    int clipTopInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringClipTop, sourceElement, null, true);
                    clipTop = String.Format(CultureInfo.InvariantCulture, "{0}px", clipTopInPixels);
                }

                if (IsEms(clipRight))
                {
                    int clipRightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringClipRight, sourceElement, null, false);
                    clipRight = String.Format(CultureInfo.InvariantCulture, "{0}px", clipRightInPixels);
                }

                if (IsEms(clipBottom))
                {
                    int clipBottomInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringClipBottom, sourceElement, null, true);
                    clipBottom = String.Format(CultureInfo.InvariantCulture, "{0}px", clipBottomInPixels);
                }

                if (IsEms(clipLeft))
                {
                    int clipLeftInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringClipLeft, sourceElement, null, false);
                    clipLeft = String.Format(CultureInfo.InvariantCulture, "{0}px", clipLeftInPixels);
                }

                destinationInlineStyle.clip = String.Format(CultureInfo.InvariantCulture, "rect({0} {1} {2} {3})", clipTop, clipRight, clipBottom, clipLeft);
            }
            else if (String.Compare(destinationCascadedStyle.clipBottom as string, sourceCascadedStyle.clipBottom as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                String.Compare(destinationCascadedStyle.clipLeft as string, sourceCascadedStyle.clipLeft as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                String.Compare(destinationCascadedStyle.clipRight as string, sourceCascadedStyle.clipRight as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                String.Compare(destinationCascadedStyle.clipTop as string, sourceCascadedStyle.clipTop as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.clip))
            {
                destinationInlineStyle.clip = String.Format(CultureInfo.InvariantCulture, "rect({0} {1} {2} {3})", clipTop, clipRight, clipBottom, clipLeft);
            }

            // We don't want to copy over color as it be handled separately.

            // The content, counterIncrement and counterReset properties will have already been generated by the time
            // we paste, so we don't do anything with them.

            if (String.Compare(destinationCascadedStyle.cursor, sourceCascadedStyle.cursor, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.cursor = sourceCascadedStyle.cursor;
            }

            if (String.Compare(destinationCascadedStyle.direction, sourceCascadedStyle.direction, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.direction = sourceCascadedStyle.direction;
            }

            if (String.Compare(destinationCascadedStyle.display, sourceCascadedStyle.display, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.display = sourceCascadedStyle.display;
            }

            if (isAtLeastIE8)
            {
                if (String.Compare(destinationCascadedStyle5.emptyCells, sourceCascadedStyle5.emptyCells, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle6.emptyCells = sourceCascadedStyle5.emptyCells;
                }
            }

            if (String.Compare(destinationCascadedStyle.styleFloat, sourceCascadedStyle.styleFloat, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.styleFloat = sourceCascadedStyle.styleFloat;
            }

            // We don't want to copy over font, fontFamily, fontSize, fontStyle, fontVariant or fontWeight as they
            // will all be handled separately.

            if (IsEms(sourceCascadedStyle.height as string))
            {
                int heightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringHeight, sourceElement, null, true);
                destinationInlineStyle.height = String.Format(CultureInfo.InvariantCulture, "{0}px", heightInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.height as string, sourceCascadedStyle.height as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.height as string))
            {
                destinationInlineStyle.height = sourceCascadedStyle.height;
            }

            if (IsEms(sourceCascadedStyle.left as string))
            {
                int leftInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringLeft, sourceElement, null, false);
                destinationInlineStyle.left = String.Format(CultureInfo.InvariantCulture, "{0}px", leftInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.left as string, sourceCascadedStyle.left as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.left as string))
            {
                destinationInlineStyle.left = sourceCascadedStyle.left;
            }

            if (IsEms(sourceCascadedStyle.letterSpacing as string))
            {
                int positionInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringLetterSpacing, sourceElement, null, false);
                destinationInlineStyle.letterSpacing = String.Format(CultureInfo.InvariantCulture, "{0}px", positionInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.letterSpacing as string, sourceCascadedStyle.letterSpacing as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.letterSpacing as string))
            {
                destinationInlineStyle.letterSpacing = sourceCascadedStyle.letterSpacing;
            }

            // The line-height property is the only CSS property whose percentage refers to the font-size of the
            // element itself. Since we'll be removing the font-size property, we need to convert the percentage to an
            // absolute unit.
            if (IsPercentage(sourceCascadedStyle.lineHeight as string) || IsEms(sourceCascadedStyle.lineHeight as string))
            {
                int lineHeightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringLineHeight, sourceElement, null, true);
                destinationInlineStyle.lineHeight = String.Format(CultureInfo.InvariantCulture, "{0}px", lineHeightInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.lineHeight as string, sourceCascadedStyle.lineHeight as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsPercentage(destinationInlineStyle.lineHeight as string) || IsEms(destinationInlineStyle.lineHeight as string))
            {
                destinationInlineStyle.lineHeight = sourceCascadedStyle.lineHeight;
            }

            // Can't copy to inlineStyle.listStyle directly, so we'll do each listStyle separately.

            if (String.Compare(destinationCascadedStyle.listStyleImage, sourceCascadedStyle.listStyleImage, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.listStyleImage = sourceCascadedStyle.listStyleImage;
            }

            if (String.Compare(destinationCascadedStyle.listStylePosition, sourceCascadedStyle.listStylePosition, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.listStylePosition = sourceCascadedStyle.listStylePosition;
            }

            if (String.Compare(destinationCascadedStyle.listStyleType, sourceCascadedStyle.listStyleType, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.listStyleType = sourceCascadedStyle.listStyleType;
            }

            if (IsEms(sourceCascadedStyle.marginBottom as string))
            {
                int marginBottomInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMarginBottom, sourceElement, null, true);
                destinationInlineStyle.marginBottom = String.Format(CultureInfo.InvariantCulture, "{0}px", marginBottomInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.marginBottom as string, sourceCascadedStyle.marginBottom as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.marginBottom as string, "auto", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.marginBottom as string))
            {
                destinationInlineStyle.marginBottom = sourceCascadedStyle.marginBottom;
            }

            if (IsEms(sourceCascadedStyle.marginLeft as string))
            {
                int marginLeftInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMarginLeft, sourceElement, null, false);
                destinationInlineStyle.marginLeft = String.Format(CultureInfo.InvariantCulture, "{0}px", marginLeftInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.marginLeft as string, sourceCascadedStyle.marginLeft as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.marginLeft as string, "auto", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.marginLeft as string))
            {
                destinationInlineStyle.marginLeft = sourceCascadedStyle.marginLeft;
            }

            if (IsEms(sourceCascadedStyle.marginRight as string))
            {
                int marginRightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMarginRight, sourceElement, null, false);
                destinationInlineStyle.marginRight = String.Format(CultureInfo.InvariantCulture, "{0}px", marginRightInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.marginRight as string, sourceCascadedStyle.marginRight as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.marginRight as string, "auto", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.marginRight as string))
            {
                destinationInlineStyle.marginRight = sourceCascadedStyle.marginRight;
            }

            if (IsEms(sourceCascadedStyle.marginTop as string))
            {
                int marginTopInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMarginTop, sourceElement, null, true);
                destinationInlineStyle.marginTop = String.Format(CultureInfo.InvariantCulture, "{0}px", marginTopInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.marginTop as string, sourceCascadedStyle.marginTop as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.marginTop as string, "auto", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.marginTop as string))
            {
                destinationInlineStyle.marginTop = sourceCascadedStyle.marginTop;
            }

            if (IsEms(sourceCascadedStyle4.maxHeight as string))
            {
                int maxHeightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMaxHeight, sourceElement, null, true);
                destinationInlineStyle5.maxHeight = String.Format(CultureInfo.InvariantCulture, "{0}px", maxHeightInPixels);
            }
            else if (String.Compare(destinationCascadedStyle4.maxHeight as string, sourceCascadedStyle4.maxHeight as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle5.maxHeight as string))
            {
                destinationInlineStyle5.maxHeight = sourceCascadedStyle4.maxHeight;
            }

            if (IsEms(sourceCascadedStyle4.maxWidth as string))
            {
                int maxWidthInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMaxWidth, sourceElement, null, false);
                destinationInlineStyle5.maxWidth = String.Format(CultureInfo.InvariantCulture, "{0}px", maxWidthInPixels);
            }
            else if (String.Compare(destinationCascadedStyle4.maxWidth as string, sourceCascadedStyle4.maxWidth as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle5.maxWidth as string))
            {
                destinationInlineStyle5.maxWidth = sourceCascadedStyle4.maxWidth;
            }

            if (IsEms(sourceCascadedStyle3.minHeight as string))
            {
                int minHeightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMinHeight, sourceElement, null, true);
                destinationInlineStyle4.minHeight = String.Format(CultureInfo.InvariantCulture, "{0}px", minHeightInPixels);
            }
            else if (String.Compare(destinationCascadedStyle3.minHeight as string, sourceCascadedStyle3.minHeight as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle4.minHeight as string))
            {
                destinationInlineStyle4.minHeight = sourceCascadedStyle3.minHeight;
            }

            if (IsEms(sourceCascadedStyle4.minWidth as string))
            {
                int minWidthInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringMinWidth, sourceElement, null, false);
                destinationInlineStyle5.minWidth = String.Format(CultureInfo.InvariantCulture, "{0}px", minWidthInPixels);
            }
            else if (String.Compare(destinationCascadedStyle4.minWidth as string, sourceCascadedStyle4.minWidth as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle5.minWidth as string))
            {
                destinationInlineStyle5.minWidth = sourceCascadedStyle4.minWidth;
            }

            if (isAtLeastIE8)
            {
                if (String.Compare(destinationCascadedStyle5.orphans as string, sourceCascadedStyle5.orphans as string, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle6.orphans = sourceCascadedStyle5.orphans;
                }

                if (String.Compare(destinationCascadedStyle5.outlineStyle, sourceCascadedStyle5.outlineStyle, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle6.outlineStyle = sourceCascadedStyle5.outlineStyle;
                }

                if (String.Compare(sourceCascadedStyle5.outlineStyle, "none", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (String.Compare(destinationCascadedStyle5.outlineColor as string, sourceCascadedStyle5.outlineColor as string, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        destinationInlineStyle6.outlineColor = sourceCascadedStyle5.outlineColor;
                    }

                    // Attempting to query the outline-width directly will throw a COMException with HRESULT E_FAIL.
                    string sourceOutlineWidth = HTMLElementHelper.CSSUnitStringOutlineWidth(sourceElement);
                    string destinationOutlineWidth = HTMLElementHelper.CSSUnitStringOutlineWidth(destinationElement);

                    if (IsEms(sourceOutlineWidth))
                    {
                        int outlineWidthInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringOutlineWidth, sourceElement,
                            HTMLElementHelper.LastChanceBorderWidthPointSize, false);
                        destinationInlineStyle6.outlineWidth = String.Format(CultureInfo.InvariantCulture, "{0}px", outlineWidthInPixels);
                    }
                    else if (String.Compare(destinationOutlineWidth, sourceOutlineWidth, StringComparison.OrdinalIgnoreCase) != 0 || IsEms(destinationOutlineWidth))
                    {
                        destinationInlineStyle6.outlineWidth = sourceOutlineWidth;
                    }
                }
            }

            if (String.Compare(destinationCascadedStyle.overflow, sourceCascadedStyle.overflow, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.overflow = sourceCascadedStyle.overflow;
            }

            if (IsEms(sourceCascadedStyle.paddingBottom as string))
            {
                int paddingBottomInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringPaddingBottom, sourceElement, null, true);
                destinationInlineStyle.paddingBottom = String.Format(CultureInfo.InvariantCulture, "{0}px", paddingBottomInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.paddingBottom as string, sourceCascadedStyle.paddingBottom as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.paddingBottom as string))
            {
                destinationInlineStyle.paddingBottom = sourceCascadedStyle.paddingBottom;
            }

            if (IsEms(sourceCascadedStyle.paddingLeft as string))
            {
                int paddingLeftInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringPaddingLeft, sourceElement, null, false);
                destinationInlineStyle.paddingLeft = String.Format(CultureInfo.InvariantCulture, "{0}px", paddingLeftInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.paddingLeft as string, sourceCascadedStyle.paddingLeft as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.paddingLeft as string))
            {
                destinationInlineStyle.paddingLeft = sourceCascadedStyle.paddingLeft;
            }

            if (IsEms(sourceCascadedStyle.paddingRight as string))
            {
                int paddingRightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringPaddingRight, sourceElement, null, false);
                destinationInlineStyle.paddingRight = String.Format(CultureInfo.InvariantCulture, "{0}px", paddingRightInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.paddingRight as string, sourceCascadedStyle.paddingRight as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.paddingRight as string))
            {
                destinationInlineStyle.paddingRight = sourceCascadedStyle.paddingRight;
            }

            if (IsEms(sourceCascadedStyle.paddingTop as string))
            {
                int paddingTopInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringPaddingTop, sourceElement, null, true);
                destinationInlineStyle.paddingTop = String.Format(CultureInfo.InvariantCulture, "{0}px", paddingTopInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.paddingTop as string, sourceCascadedStyle.paddingTop as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.paddingTop as string))
            {
                destinationInlineStyle.paddingTop = sourceCascadedStyle.paddingTop;
            }

            if (String.Compare(destinationCascadedStyle.pageBreakAfter, sourceCascadedStyle.pageBreakAfter, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.pageBreakAfter = sourceCascadedStyle.pageBreakAfter;
            }

            if (String.Compare(destinationCascadedStyle.pageBreakBefore, sourceCascadedStyle.pageBreakBefore, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.pageBreakBefore = sourceCascadedStyle.pageBreakBefore;
            }

            if (isAtLeastIE8)
            {
                if (String.Compare(destinationCascadedStyle5.pageBreakInside, sourceCascadedStyle5.pageBreakInside, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle6.pageBreakInside = sourceCascadedStyle5.pageBreakInside;
                }
            }

            if (String.Compare(destinationCascadedStyle.position, sourceCascadedStyle.position, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.position = sourceCascadedStyle.position;
            }

            if (isAtLeastIE8)
            {
                if (String.Compare(destinationCascadedStyle5.quotes, sourceCascadedStyle5.quotes, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle6.quotes = sourceCascadedStyle5.quotes;
                }
            }

            if (IsEms(sourceCascadedStyle.right as string))
            {
                int rightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringRight, sourceElement, null, false);
                destinationInlineStyle2.right = String.Format(CultureInfo.InvariantCulture, "{0}px", rightInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.right as string, sourceCascadedStyle.right as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle2.right as string))
            {
                destinationInlineStyle2.right = sourceCascadedStyle.right;
            }

            if (String.Compare(destinationCascadedStyle.tableLayout, sourceCascadedStyle.tableLayout, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.tableLayout = sourceCascadedStyle.tableLayout;
            }

            if (String.Compare(destinationCascadedStyle.textAlign, sourceCascadedStyle.textAlign, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.textAlign = sourceCascadedStyle.textAlign;
            }

            // We don't want to copy over textDecoration as it will be handled separately.

            if (IsEms(sourceCascadedStyle.textIndent as string))
            {
                int textIndentInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringTextIndent, sourceElement, null, false);
                destinationInlineStyle.textIndent = String.Format(CultureInfo.InvariantCulture, "{0}px", textIndentInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.textIndent as string, sourceCascadedStyle.textIndent as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.textIndent as string))
            {
                destinationInlineStyle.textIndent = sourceCascadedStyle.textIndent;
            }

            if (String.Compare(destinationCascadedStyle.textTransform, sourceCascadedStyle.textTransform, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.textTransform = sourceCascadedStyle.textTransform;
            }

            if (IsEms(sourceCascadedStyle.top as string))
            {
                int topInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringTop, sourceElement, null, true);
                destinationInlineStyle.top = String.Format(CultureInfo.InvariantCulture, "{0}px", topInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.top as string, sourceCascadedStyle.top as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.top as string))
            {
                destinationInlineStyle.top = sourceCascadedStyle.top;
            }

            if (String.Compare(destinationCascadedStyle.unicodeBidi, sourceCascadedStyle.unicodeBidi, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.unicodeBidi = sourceCascadedStyle.unicodeBidi;
            }

            if (IsEms(sourceCascadedStyle.verticalAlign as string))
            {
                int verticalAlignInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringVerticalAlign, sourceElement, null, true);
                destinationInlineStyle.verticalAlign = String.Format(CultureInfo.InvariantCulture, "{0}px", verticalAlignInPixels);
            }
            else if ((String.Compare(destinationCascadedStyle.verticalAlign as string, sourceCascadedStyle.verticalAlign as string, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(sourceCascadedStyle.verticalAlign as string, "auto", StringComparison.OrdinalIgnoreCase) != 0) ||
                IsEms(destinationInlineStyle.verticalAlign as string))
            {
                destinationInlineStyle.verticalAlign = sourceCascadedStyle.verticalAlign;
            }

            if (String.Compare(destinationCascadedStyle.visibility, sourceCascadedStyle.visibility, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.visibility = sourceCascadedStyle.visibility;
            }

            if (String.Compare(destinationCascadedStyle3.whiteSpace, sourceCascadedStyle3.whiteSpace, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.whiteSpace = sourceCascadedStyle3.whiteSpace;
            }

            if (isAtLeastIE8)
            {
                if (String.Compare(destinationCascadedStyle5.widows as string, sourceCascadedStyle5.widows as string, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    destinationInlineStyle6.widows = sourceCascadedStyle5.widows;
                }
            }

            if (IsEms(sourceCascadedStyle.width as string))
            {
                int widthInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringWidth, sourceElement, null, false);
                destinationInlineStyle.width = String.Format(CultureInfo.InvariantCulture, "{0}px", widthInPixels);
            }
            else if (String.Compare(destinationCascadedStyle.width as string, sourceCascadedStyle.width as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.width as string))
            {
                destinationInlineStyle.width = sourceCascadedStyle.width;
            }

            if (IsEms(sourceCascadedStyle3.wordSpacing as string))
            {
                int wordSpacingInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringWordSpacing, sourceElement, null, false);
                destinationInlineStyle.wordSpacing = String.Format(CultureInfo.InvariantCulture, "{0}px", wordSpacingInPixels);
            }
            else if (String.Compare(destinationCascadedStyle3.wordSpacing as string, sourceCascadedStyle3.wordSpacing as string, StringComparison.OrdinalIgnoreCase) != 0 ||
                IsEms(destinationInlineStyle.wordSpacing as string))
            {
                destinationInlineStyle.wordSpacing = sourceCascadedStyle3.wordSpacing;
            }

            if (String.Compare(destinationCascadedStyle.zIndex as string, sourceCascadedStyle.zIndex as string, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle.zIndex = sourceCascadedStyle.zIndex;
            }

            // ---------
            // 3.  CSS 3
            // ---------

            if (String.Compare(destinationCascadedStyle.rubyAlign, sourceCascadedStyle.rubyAlign, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.rubyAlign = sourceCascadedStyle.rubyAlign;
            }

            if (String.Compare(destinationCascadedStyle.rubyOverhang, sourceCascadedStyle.rubyOverhang, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.rubyOverhang = sourceCascadedStyle.rubyOverhang;
            }

            if (String.Compare(destinationCascadedStyle.rubyPosition, sourceCascadedStyle.rubyPosition, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.rubyPosition = sourceCascadedStyle.rubyPosition;
            }

            if (String.Compare(destinationCascadedStyle2.textAlignLast, sourceCascadedStyle2.textAlignLast, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle3.textAlignLast = sourceCascadedStyle2.textAlignLast;
            }

            if (String.Compare(destinationCascadedStyle.textJustify, sourceCascadedStyle.textJustify, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.textJustify = sourceCascadedStyle.textJustify;
            }

            if (String.Compare(destinationCascadedStyle3.textOverflow, sourceCascadedStyle3.textOverflow, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle4.textOverflow = sourceCascadedStyle3.textOverflow;
            }

            if (String.Compare(destinationCascadedStyle.wordBreak, sourceCascadedStyle.wordBreak, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle2.wordBreak = sourceCascadedStyle.wordBreak;
            }

            if (String.Compare(destinationCascadedStyle2.wordWrap, sourceCascadedStyle2.wordWrap, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle3.wordWrap = sourceCascadedStyle2.wordWrap;
            }

            if (String.Compare(destinationCascadedStyle2.writingMode, sourceCascadedStyle2.writingMode, StringComparison.OrdinalIgnoreCase) != 0)
            {
                destinationInlineStyle3.writingMode = sourceCascadedStyle2.writingMode;
            }
        }

        /// <summary>
        /// Returns whether or not a percent sign appears in the CSS units somewhere.
        /// </summary>
        /// <param name="cssUnits">The CSS value string to search.</param>
        /// <returns>true if a percent sign appears and false otherwise.</returns>
        private bool IsPercentage(string cssUnits)
        {
            return !String.IsNullOrEmpty(cssUnits) && cssUnits.IndexOf("%", StringComparison.OrdinalIgnoreCase) > 0;
        }

        /// <summary>
        /// Returns whether or not "em" appears in the CSS units somewhere. This is not future proof if the CSS spec
        /// were to add a value (like "auto" or "center") that includes the string "em" in it.
        /// </summary>
        /// <param name="cssUnits">The CSS value string to search.</param>
        /// <returns>true if a percent sign appears and false otherwise.</returns>
        private bool IsEms(string cssUnits)
        {
            return !String.IsNullOrEmpty(cssUnits) && cssUnits.IndexOf("em", StringComparison.OrdinalIgnoreCase) > 0;
        }

        /// <summary>
        /// Removes any inline text-related CSS (font-family, font-size, font-style, font-variant, font-weight, color,
        /// text-decoration, background-color).
        /// </summary>
        /// <param name="element">The element to remove inline text-related CSS from.</param>
        private void RemoveTextRelatedInlineCss(IHTMLElement element)
        {
            IHTMLStyle inlineStyle = element.style;

            // Remove any inline text styles. We'll add these back later.
            if (!String.IsNullOrEmpty(inlineStyle.fontFamily))
            {
                inlineStyle.fontFamily = string.Empty;
            }

            if (!String.IsNullOrEmpty(inlineStyle.fontSize as string))
            {
                inlineStyle.fontSize = string.Empty;
            }

            if (!String.IsNullOrEmpty(inlineStyle.fontStyle))
            {
                inlineStyle.fontStyle = string.Empty;
            }

            if (!String.IsNullOrEmpty(inlineStyle.fontVariant))
            {
                inlineStyle.fontVariant = string.Empty;
            }

            if (!String.IsNullOrEmpty(inlineStyle.fontWeight))
            {
                inlineStyle.fontWeight = string.Empty;
            }

            if (!String.IsNullOrEmpty(inlineStyle.color as string))
            {
                inlineStyle.color = string.Empty;
            }

            if (!String.IsNullOrEmpty(inlineStyle.textDecoration))
            {
                inlineStyle.textDecoration = string.Empty;
            }

            // Leave background color on block elements as users can only edit background colors on inline elements ("highlighting").
            if (IsInlineElement(element))
            {
                inlineStyle.backgroundColor = string.Empty;
            }
        }

        /// <summary>
        /// Moves a few CSS properties (text-align, width and height) to their respective HTML attributes (when
        /// applicable) to provide better compatibility with MSHTML's editing commands.
        /// </summary>
        /// <param name="sourceElement">The element from which the CSS property originated.</param>
        /// <param name="destinationElement">The element that will receive the HTML attributes.</param>
        private void MoveCssPropertiesToHtmlAttributes(IHTMLElement sourceElement, IHTMLElement destinationElement)
        {
            IHTMLStyle inlineStyle = destinationElement.style;

            if (!String.IsNullOrEmpty(inlineStyle.textAlign))
            {
                if (SupportsAlignmentAttribute(destinationElement))
                {
                    // The CSS text-align and HTML align attribute have a 1-to-1 mapping, so no conversion needed.
                    destinationElement.setAttribute("align", inlineStyle.textAlign, 0);
                    inlineStyle.textAlign = null;
                }
            }

            string width = inlineStyle.width as string;
            if (!String.IsNullOrEmpty(width) && String.Compare(width, "auto", StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (IsPercentage(width) && SupportsPercentageWidthAttribute(destinationElement))
                {
                    destinationElement.setAttribute("width", width, 0);
                    inlineStyle.width = string.Empty;
                }
                else if (SupportsPixelWidthAttribute(destinationElement))
                {
                    // Make sure we convert any relative units (e.g. em, percentages, etc) to pixels because that is
                    // what the HTML width attribute is expecting. We calculate this off the source element to ensure
                    // it is correct.
                    int widthInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringWidth, sourceElement, null, false);

                    destinationElement.setAttribute("width", widthInPixels.ToString(CultureInfo.InvariantCulture), 0);
                    inlineStyle.width = string.Empty;
                }
            }

            string height = inlineStyle.height as string;
            if (!String.IsNullOrEmpty(height) && String.Compare(height, "auto", StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (IsPercentage(height) && SupportsPercentageHeightAttribute(destinationElement))
                {
                    destinationElement.setAttribute("height", height, 0);
                    inlineStyle.height = string.Empty;
                }
                else if (SupportsPixelHeightAttribute(destinationElement))
                {
                    // Make sure we convert any relative units (e.g. em, percentages, etc) to pixels because that is
                    // what the HTML height attribute is expecting. We calculate this off the source element to ensure
                    // it is correct.
                    int heightInPixels = (int)HTMLElementHelper.CSSUnitStringToPixelSize(HTMLElementHelper.CSSUnitStringHeight, sourceElement, null, true);

                    destinationElement.setAttribute("height", heightInPixels.ToString(CultureInfo.InvariantCulture), 0);
                    inlineStyle.height = string.Empty;
                }
            }
        }

        private bool SupportsAlignmentAttribute(IHTMLElement element)
        {
            if (supportsAlignmentAttribute == null)
            {
                // Per the HTML 4.01 and XHTML Transitional DTD, the following elements support the HTML
                // align="LEFT|CENTER|RIGHT|JUSTIFY" attribute.
                supportsAlignmentAttribute = ElementFilters.CreateCompoundElementFilter(
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_DIV)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_P)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_H1)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_H2)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_H3)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_H4)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_H5)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_H6)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_COLGROUP)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_COL)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_THEAD)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TBODY)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TFOOT)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TR)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TH)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TD)));
            }

            return supportsAlignmentAttribute(element);
        }

        private bool SupportsPercentageWidthAttribute(IHTMLElement element)
        {
            if (this.supportsPercentageWidthAttribute == null)
            {
                // Per the HTML 4.01 and XHTML Transitional DTD, the following elements support the HTML width="X"
                // attribute (where X is in pixels).
                this.supportsPercentageWidthAttribute = ElementFilters.CreateCompoundElementFilter(
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IMG)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TABLE)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_COLGROUP)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_COL)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TH)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TD)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_HR)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IFRAME)));
            }

            return this.supportsPercentageWidthAttribute(element);
        }

        private bool SupportsPixelWidthAttribute(IHTMLElement element)
        {
            if (this.supportsPixelWidthAttribute == null)
            {
                // Per the HTML 4.01 and XHTML Transitional DTD, the following elements support the HTML width="X"
                // attribute (where X is in pixels).
                this.supportsPixelWidthAttribute = ElementFilters.CreateCompoundElementFilter(
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IMG)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_APPLET)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_OBJECT)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TABLE)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_COLGROUP)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_COL)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TH)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TD)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_HR)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IFRAME)));
            }

            return this.supportsPixelWidthAttribute(element);
        }

        private bool SupportsPercentageHeightAttribute(IHTMLElement element)
        {
            if (this.supportsPercentageHeightAttribute == null)
            {
                // Per the HTML 4.01 and XHTML Transitional DTD, the following elements support the HTML height="X"
                // attribute (where X is in pixels).
                this.supportsPercentageHeightAttribute = ElementFilters.CreateCompoundElementFilter(
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IMG)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TH)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TD)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IFRAME)));
            }

            return this.supportsPercentageHeightAttribute(element);
        }

        private bool SupportsPixelHeightAttribute(IHTMLElement element)
        {
            if (this.supportsPixelHeightAttribute == null)
            {
                // Per the HTML 4.01 and XHTML Transitional DTD, the following elements support the HTML height="X"
                // attribute (where X is in pixels).
                this.supportsPixelHeightAttribute = ElementFilters.CreateCompoundElementFilter(
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IMG)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_APPLET)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_OBJECT)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TH)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_TD)),
                    ElementFilters.CreateElementNameFilter(sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_IFRAME)));
            }

            return this.supportsPixelHeightAttribute(element);
        }

        /// <summary>
        /// Returns true if the current currentContext is text.
        /// </summary>
        /// <param name="currentContext">The markup currentContext to examine.</param>
        /// <returns>true if the current currentContext is text and false otherwise.</returns>
        private bool IsText(MarkupContext currentContext)
        {
            return currentContext.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text;
        }

        /// <summary>
        /// Determines whether a fixup segment should end based on the provided source and destination text-formatting.
        /// </summary>
        /// <param name="sourceStyle">The current text-formatting at the source.</param>
        /// <param name="destinationStyle">The current text-formatting at the destination.</param>
        /// <param name="fixupSegment">The fixup segment that is currently open for this TextStyle.</param>
        /// <returns>true if the provided fixup segment should be ended and false otherwise.</returns>
        private bool ShouldEndFixupSegment(TextStyle sourceStyle, TextStyle destinationStyle, FixupSegment fixupSegment)
        {
            // Can't end a non-existent fixup segment.
            if (fixupSegment == null)
            {
                return false;
            }

            // If we've moved into a range where the source and copy styles match then we should end the fixup segment.
            if (destinationStyle == sourceStyle)
            {
                return true;
            }

            // If we've moved into a range where the open fixup segment doesn't apply anymore then we should end the
            // fixup segment.
            if (fixupSegment.SourceTextStyle != sourceStyle)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a fixup segment should start based on the provided source and destination text-formatting.
        /// </summary>
        /// <param name="sourceStyle">The current text-formatting at the source.</param>
        /// <param name="destinationStyle">The current text-formatting at the destination.</param>
        /// <param name="fixupSegment">The fixup segment that is currently open for this TextStyle.</param>
        /// <returns>true if the provided fixup segment should be ended and a new fixup segment started, and false
        /// otherwise.</returns>
        private bool ShouldStartFixupSegment(TextStyle sourceStyle, TextStyle destinationStyle, FixupSegment fixupSegment)
        {
            // If we've moved into a range where the source and copy styles don't match (and there's not already an
            // open fixup segment that will make them match) then we should start a fixup segment.
            if (sourceStyle != destinationStyle && (fixupSegment == null || fixupSegment.SourceTextStyle != sourceStyle))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the current context is an ending tag, assuming the context was retrieved by moving forward.
        /// </summary>
        /// <param name="currentContext">The markup context to examine.</param>
        /// <returns>true if the current context is an ending tag and false otherwise.</returns>
        private bool IsEndTag(MarkupContext currentContext)
        {
            return currentContext.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope;
        }

        /// <summary>
        /// Returns true if the provided element is an inline element.
        /// </summary>
        /// <param name="element">The element to examine.</param>
        /// <returns>true if the provided element is an inline element and false otherwise.</returns>
        private bool IsInlineElement(IHTMLElement element)
        {
            const string html = "html";
            const string inline = "inline";

            if (element != null &&
                String.Compare(((IHTMLElement2)element).scopeName, html, StringComparison.OrdinalIgnoreCase) != 0)
            {
                // This is a custom element (e.g. <o:p></o:p> from Word), so check if its set to display inline.
                return String.Compare(((IHTMLElement2)element).currentStyle.display, inline,
                    StringComparison.OrdinalIgnoreCase) == 0;
            }

            return element != null && (ElementFilters.IsInlineElement(element) || IsBrElement(element));
        }

        /// <summary>
        /// Returns true if the provided element is a lt;br&gt; element.
        /// </summary>
        /// <param name="element">The element to examine.</param>
        /// <returns>true if the provided element is a lt;br&gt; element and false otherwise.</returns>
        private bool IsBrElement(IHTMLElement element)
        {
            if (isBrElement == null)
            {
                isBrElement = ElementFilters.CreateElementNameFilter(this.sourceMarkupServices.GetNameForTagId(_ELEMENT_TAG_ID.TAGID_BR));
            }

            return isBrElement(element);
        }

        /// <summary>
        /// Removes all the open fixup segments from the provided list and registers them to be executed at a later time.
        /// </summary>
        /// <param name="openFixupSegments">The list of fixup segments that have not been registered.</param>
        /// <param name="registeredFixupSegments">The list of fixup segments that have been registered.</param>
        private void EndOpenFixupSegments(IDictionary<Type, FixupSegment> openFixupSegments, ICollection<FixupSegment> registeredFixupSegments)
        {
            foreach (FixupSegment fixupSegment in openFixupSegments.Values)
            {
                registeredFixupSegments.Add(fixupSegment);
            }

            openFixupSegments.Clear();
        }

        /// <summary>
        /// Executes each fix up segment.
        /// </summary>
        /// <param name="registeredFixupSegments">A list of fixup segments to be executed.</param>
        private void ExecuteRegisteredFixupSegments(ICollection<FixupSegment> registeredFixupSegments)
        {
            // Creating a text range is expensive, so we just create one and reuse it.
            IHTMLBodyElement body = (IHTMLBodyElement)destinationDocument.body;
            IHTMLTxtRange textRange = body.createTextRange();

            // We can QI the IHTMLTxtRange for IOleCommandTarget which then lets us pass each fixup segment a
            // command set that they can execute against.
            IOleCommandTargetWithExecParams target = (IOleCommandTargetWithExecParams)textRange;
            MshtmlCoreCommandSet commands = new MshtmlCoreCommandSet(target);

            foreach (FixupSegment fixupSegment in registeredFixupSegments)
            {
                destinationMarkupServices.MoveRangeToPointers(fixupSegment.RangeToFixup.Start, fixupSegment.RangeToFixup.End, textRange);
                fixupSegment.DoFixup(destinationMarkupServices, commands);
            }

            registeredFixupSegments.Clear();
        }
    }
}
