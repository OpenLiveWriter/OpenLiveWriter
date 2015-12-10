// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Collections;
using Project31.Interop.Com;
using Project31.MshtmlEditor;
using mshtml;

namespace Onfolio.Core.HtmlEditor
{
    public class HtmlEditorElementBehaviorManager : IDisposable
    {
        public HtmlEditorElementBehaviorManager(IHtmlEditorComponentContext editorContext)
        {
            _editorContext = editorContext ;
            _editorContext.SelectionChanged +=new EventHandler(_editorContext_SelectionChanged);
        }

        public void Dispose()
        {
            if ( _activeBehaviors != null )
            {
                _editorContext.SelectionChanged -=new EventHandler(_editorContext_SelectionChanged);

                // all of the elements left in the activeBehaviors list are no longer in the document
                // so we should dispose and remove them from the list
                foreach ( HtmlEditorElementBehavior elementBehavior in _activeBehaviors.Values )
                    elementBehavior.DetachFromElement();
                _activeBehaviors.Clear();
                _activeBehaviors = null ;
            }
        }

        public void RegisterBehavior( string appliesToTag, Type behaviorType )
        {
            lock(this)
            {
                _globalElementBehaviors[appliesToTag] = behaviorType ;
            }
        }

        public void RegisterBehavior( IHTMLElement element, HtmlEditorElementBehavior behavior)
        {
            lock(this)
            {
                _activeBehaviors[element] = behavior;
                _elementBehaviors[element] = behavior;
            }
        }

        public void RefreshBehaviors(IHTMLDocument2 document)
        {
            lock(this)
            {
                // get interface needed for element enumeration
                IHTMLDocument3 document3 = (IHTMLDocument3)document ;

                // build new list of active behaviors
                Hashtable newActiveBehaviors = new Hashtable();

                //register global behaviors
                IDictionaryEnumerator behaviorEnumerator = _globalElementBehaviors.GetEnumerator() ;
                while (behaviorEnumerator.MoveNext())
                {
                    string tagName = (string)behaviorEnumerator.Key  ;
                    foreach( IHTMLElement element in document3.getElementsByTagName(tagName))
                    {
                        // if the element is already assigned an active behavior then move it
                        if ( _activeBehaviors.ContainsKey(element) )
                        {
                            newActiveBehaviors[element] = _activeBehaviors[element] ;
                            _activeBehaviors.Remove(element);
                        }
                        else // otherwise create a new behavior and add it
                        {
                            HtmlEditorElementBehavior elementBehavior = (HtmlEditorElementBehavior)Activator.CreateInstance(
                                (Type)behaviorEnumerator.Value, new object[] { element, _editorContext}  ) ;
                            newActiveBehaviors[element] = elementBehavior ;
                        }
                    }
                }

                //register element behaviors
                behaviorEnumerator = _elementBehaviors.GetEnumerator() ;
                while (behaviorEnumerator.MoveNext())
                {
                    IHTMLElement element = (IHTMLElement)behaviorEnumerator.Key  ;
                    // if the element is already assigned an active behavior then move it
                    if ( _activeBehaviors.ContainsKey(element) )
                    {
                        newActiveBehaviors[element] = _activeBehaviors[element] ;
                        _activeBehaviors.Remove(element);
                    }
                    else // otherwise create a new behavior and add it
                    {
                        Debug.Fail("illegal state: element behavior is not in the _activeBehaviors table");
                    }
                }

                // all of the elements left in the activeBehaviors list are no longer in the document
                // so we should dispose and remove them from the list
                foreach ( HtmlEditorElementBehavior elementBehavior in _activeBehaviors.Values )
                    elementBehavior.DetachFromElement();
                _activeBehaviors.Clear();

                // swap for new list
                _activeBehaviors = newActiveBehaviors ;
            }
        }

        private void _editorContext_SelectionChanged(object sender, EventArgs e)
        {
            // update 'active' state of behaviors based on whether they contain the selection
            MarkupRange selectedRange = _editorContext.GetSelectedMarkupRange() ;
            foreach ( HtmlEditorElementBehavior elementBehavior in _activeBehaviors.Values )
                elementBehavior.SelectionChanged(selectedRange);
        }

        private Hashtable _globalElementBehaviors = new Hashtable();
        private Hashtable _elementBehaviors = new Hashtable();
        private Hashtable _activeBehaviors = new Hashtable();
        private IHtmlEditorComponentContext _editorContext;

    }
}
