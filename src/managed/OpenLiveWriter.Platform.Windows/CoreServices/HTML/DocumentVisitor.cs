// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Text;
using System.Windows.Forms;
using mshtml;

namespace OpenLiveWriter.CoreServices.HTML
{
    /// <summary>
    /// Visits elements in a document.  Override and provide your own behavior.
    /// </summary>
    public abstract class DocumentVisitor
    {
        private struct StackItem
        {
            public object element;
            public bool begin;

            public StackItem(object element)
            {
                this.element = element;
                this.begin = true;
            }
        }

        public DocumentVisitor()
        {
        }

        public virtual void Visit(IHTMLDocument3 document)
        {
            if (Application.MessageLoop)
                Application.DoEvents();

            //			while (((IHTMLDocument2)document).readyState != "interactive")
            //			{
            //				Application.DoEvents();
            //				Thread.Sleep(50);
            //			}

            //Trace.WriteLine("Begin visit: " + ((IHTMLDocument2)document).readyState + "\r\n" + document.documentElement.innerHTML);

            Stack stack = new Stack();
            stack.Push(new StackItem(document.documentElement));

            while (stack.Count > 0)
            {
                StackItem stackItem = (StackItem)stack.Pop();

                if (stackItem.element == null)
                    continue;
                else if (stackItem.element is IHTMLTextElement)
                    OnText((IHTMLTextElement)stackItem.element);
                else if (stackItem.element is IHTMLCommentElement)
                    OnComment((IHTMLCommentElement)stackItem.element);
                else if (stackItem.element is IHTMLElement)
                {
                    IHTMLElement el = (IHTMLElement)stackItem.element;

                    if (stackItem.begin)
                    {
                        if (!OnElementBegin(el))
                        {
                            //							Trace.WriteLine(Indent + "<" + el + "/> (skipping)");
                            continue;
                        }

                        //						Trace.WriteLine(Indent + "<" + el + "> (children: " + ((IHTMLElementCollection)el.children).length);
                        IncreaseIndent();

                        stackItem.begin = false;
                        // return item to the stack, so we can call OnElementEnd on the way out
                        stack.Push(stackItem);

                        // DO NOT iterate over the DOM this way.  We miss a lot of nodes in Release mode.
                        //						IHTMLElementCollection children = (IHTMLElementCollection)el.children;
                        //						StackItem[] items = new StackItem[children.length];
                        //						int i = 0;
                        //						foreach (IHTMLElement child in children)
                        //							items[i++] = new StackItem(child);
                        //						Array.Reverse(items);

                        ArrayList items = new ArrayList();
                        for (IHTMLDOMNode child = ((IHTMLDOMNode)el).firstChild; child != null; child = child.nextSibling)
                        {
                            IHTMLElement childElement = child as IHTMLElement;
                            if (childElement != null)
                                items.Add(new StackItem(childElement));
                        }
                        items.Reverse();

                        foreach (StackItem si in items)
                            stack.Push(si);
                    }
                    else
                    {
                        DecreaseIndent();
                        //						Trace.WriteLine(Indent + "</" + el.tagName + ">");
                        OnElementEnd(el);
                    }
                }
            }

            //Trace.WriteLine("End visit");
        }

        /// <summary>
        /// Called when an element is encountered.  If false is returned,
        /// OnElementEnd will not be called for this element, and the
        /// element's children will not be visited.
        /// </summary>
        protected virtual bool OnElementBegin(IHTMLElement el) { return true; }

        /// <summary>
        /// Called when exiting an element's scope.
        /// </summary>
        protected virtual void OnElementEnd(IHTMLElement el) { }

        protected virtual void OnComment(IHTMLCommentElement el) { }

        protected virtual void OnText(IHTMLTextElement el) { }

        private const int INDENT_SPACES = 4;
        private StringBuilder indent = new StringBuilder("");
        private void IncreaseIndent()
        {
            indent.Append(' ', INDENT_SPACES);
        }

        private void DecreaseIndent()
        {
            indent.Remove(indent.Length - INDENT_SPACES, INDENT_SPACES);
        }

        private string Indent
        {
            get
            {
                return indent.ToString();
            }
        }

    }
}
