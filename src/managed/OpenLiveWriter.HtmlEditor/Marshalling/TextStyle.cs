// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public abstract class TextStyle : IEquatable<TextStyle>
    {
        protected delegate IHTMLElement ElementFactory();

        public abstract void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands);

        public override bool Equals(object obj)
        {
            return Equals(obj as TextStyle);
        }

        public abstract bool Equals(TextStyle obj);

        /// <summary>
        /// Wraps a MarkupRange in an element (or multiple elements if necessary to produce valid HTML).
        /// </summary>
        /// <param name="elementFactory">Creates elements to wrap the markupRange in as needed.</param>
        /// <param name="markupServices">The MarkupServices for the markupRange.</param>
        /// <param name="markupRange">The range to wrap.</param>
        protected void WrapInElement(ElementFactory elementFactory, MshtmlMarkupServices markupServices, MarkupRange markupRange)
        {
            Debug.Assert(markupRange.GetElements(ElementFilters.BLOCK_ELEMENTS, false).Length == 0,
                "Did not expect MarkupRange to contain block elements");

            MarkupPointer startPointer = markupRange.Start.Clone();
            MarkupPointer endPointer = markupRange.Start.Clone();
            MarkupContext context;

            while (endPointer.IsLeftOf(markupRange.End))
            {
                context = endPointer.Right(false);

                if (context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope ||
                    context.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope)
                {
                    if (!markupRange.InRange(context.Element))
                    {
                        // EnterScope example: <span>[markupRange.Start]Hello [endPointer]<i>Hello[markupRange.End]</i></span>
                        // ExitScope example: <span>[markupRange.Start]Hello [endPointer]</span><span>Hello[markupRange.End]</span>
                        InsertElement(elementFactory(), markupServices, startPointer, endPointer);
                        continue;
                    }
                }

                endPointer.Right(true);
            }

            InsertElement(elementFactory(), markupServices, startPointer, endPointer);
        }

        /// <summary>
        /// Wraps the range from startPointer to endPointer with the given element (as long as the range is non-empty)
        /// and then moves the pointers past the inserted element.
        /// </summary>
        /// <param name="element">The element to wrap the range in.</param>
        /// <param name="markupServices">The MarkupServices for the start and end pointers.</param>
        /// <param name="startPointer">Pointer to place the beginning of the element.</param>
        /// <param name="endPointer">Pointer to place the end of the element.</param>
        private void InsertElement(IHTMLElement element, MshtmlMarkupServices markupServices,
            MarkupPointer startPointer, MarkupPointer endPointer)
        {
            Debug.Assert(startPointer.IsLeftOfOrEqualTo(endPointer), "Expected start to be left of or equal to end!");
            Debug.Assert(HTMLElementHelper.ElementsAreEqual(startPointer.CurrentScope, endPointer.CurrentScope),
                "Expected start and end to be in the same scope, otherwise resulting HTML will be invalid!");

            if (startPointer.IsLeftOf(endPointer))
            {
                markupServices.InsertElement(element, startPointer, endPointer);
            }

            endPointer.Right(true);
            startPointer.MoveToPointer(endPointer);
        }

        public static bool operator ==(TextStyle lhs, TextStyle rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TextStyle lhs, TextStyle rhs)
        {
            return !lhs.Equals(rhs);
        }

        public abstract override int GetHashCode();
    }
}
