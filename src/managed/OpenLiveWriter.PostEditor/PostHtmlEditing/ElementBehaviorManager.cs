// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;
using System.Diagnostics;
using System.Collections.Generic;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class ElementBehaviorDefinition
    {
        /// <summary>
        /// Register an element behavior definition
        /// </summary>
        /// <param name="styleName">name of html element or css class name to apply behavior to</param>
        /// <param name="uniqueId">class id for behavior, actual value is irrelevant, this just signals MSHTML
        /// that it should query the behavior via the IElementBehaviorFactory registered on the editing site</param>
        /// <param name="behaviorCreator">callback function for creating behavior instances</param>
        public ElementBehaviorDefinition(string targetClass, Guid clsid, ElementBehaviorCreator createBehavior)
        {
            TargetClass = targetClass;
            Clsid = clsid.ToString("D");
            HtmlId = String.Format(CultureInfo.InvariantCulture, "{0}_WRITER_BEHAVIOR", TargetClass.ToUpper(CultureInfo.InvariantCulture).Replace(" ", "_"));
            _createBehavior = createBehavior;
        }

        public readonly string TargetClass;
        public readonly string Clsid;
        public readonly string HtmlId;

        public ElementBehaviorCreator CreateBehavior
        {
            get
            {
                return _createBehavior;
            }
        }

        private readonly ElementBehaviorCreator _createBehavior;
    }

    public interface IElementBehaviorManager
    {
        string BehaviorStyles { get; }
        string BehaviorObjectTags { get; }
    }

    public class NullElementBehaviorManager : IElementBehaviorManager
    {

        #region IElementBehaviorManager Members

        public string BehaviorStyles
        {
            get
            {
                return String.Empty;
            }
        }

        public string BehaviorObjectTags
        {
            get
            {
                return String.Empty;
            }
        }

        #endregion

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class ElementBehaviorManager : IElementBehaviorManager, IDisposable
    {
        public void RegisterBehavior(ElementBehaviorDefinition behaviorDefinition)
        {
            _behaviorDefinitions.Add(behaviorDefinition);
            _getBehaviorStyles.Clear();
            _getBehaviorObjectTags.Clear();
        }

        public ElementBehaviorManager()
        {
            _getBehaviorStyles = new LazyLoader<string>(() =>
            {
                StringBuilder stylesBuilder = new StringBuilder();
                foreach (ElementBehaviorDefinition behaviorDefinition in _behaviorDefinitions)
                {
                    stylesBuilder.Append(behaviorDefinition.TargetClass + " { behavior: url(#default#" +
                                         behaviorDefinition.HtmlId + ") }\r\n");
                }
                return stylesBuilder.ToString();
            });

            _getBehaviorObjectTags = new LazyLoader<string>(() =>
            {
                StringBuilder objectTagsBuilder = new StringBuilder();
                foreach (ElementBehaviorDefinition behaviorDefinition in _behaviorDefinitions)
                {
                    objectTagsBuilder.Append("<object id=\"" + behaviorDefinition.HtmlId + "\" clsid=\"clsid:" +
                                             behaviorDefinition.Clsid +
                                             "\" style=\"visibility: hidden\" width=\"0px\" height=\"0px\"> </object>\r\n");
                }
                return objectTagsBuilder.ToString();
            });
        }

        // HTML required to make runtime bound behaviors work (GUID value is irrelevant)
        /*
            <STYLE>
                img { behavior: url(#default#IMG_BEHAVIOR) }
            </STYLE>

            <object id="IMG_BEHAVIOR" clsid="clsid:3C0C37AD-21B5-41f4-A25E-59259B0ED874" style="visibility: hidden" width="0px" height="0px"> </object>
        */

        private LazyLoader<string> _getBehaviorStyles;
        public string BehaviorStyles
        {
            get
            {
                return _getBehaviorStyles;
            }
        }

        private LazyLoader<string> _getBehaviorObjectTags;
        public string BehaviorObjectTags
        {
            get
            {
                return _getBehaviorObjectTags;
            }
        }

        public MshtmlElementBehavior CreateBehavior(string htmlId)
        {
            foreach (ElementBehaviorDefinition behaviorDefinition in _behaviorDefinitions)
            {
                if (behaviorDefinition.HtmlId == htmlId)
                {
                    MshtmlElementBehavior behavior = behaviorDefinition.CreateBehavior();

                    if (behavior != null)
                    {
                        _behaviors.Add(behavior);
                        behavior.Disposed += new EventHandler(behavior_Disposed);
                    }

                    return behavior;
                }
            }

            // didn't find a matching behavior
            return null;
        }

        private void behavior_Disposed(object sender, EventArgs e)
        {
            MshtmlElementBehavior behavior = sender as MshtmlElementBehavior;
            if (behavior != null)
            {
                behavior.Disposed -= new EventHandler(behavior_Disposed);
                _behaviors.Remove(behavior);
            }
        }

        public override int GetHashCode()
        {
            return (BehaviorObjectTags + BehaviorStyles).GetHashCode();
        }

        private List<MshtmlElementBehavior> _behaviors = new List<MshtmlElementBehavior>();
        private ArrayList _behaviorDefinitions = new ArrayList();

        internal bool ContainsBehavior(string behaviorClassname)
        {
            // Look through all the behaviors to see if any match the same
            // behavior that we are looking for.
            foreach (ElementBehaviorDefinition registeredBehavior in _behaviorDefinitions)
            {
                if (behaviorClassname == "#default#" + registeredBehavior.HtmlId)
                    return true;
            }

            return false;
        }

        public void DisposeCreatedBehaviors()
        {
            // Make a copy of the List<T> because calling Dispose on the behavior will modify the List<T>
            // through the behavior_Disposed event handler.
            MshtmlElementBehavior[] behaviors = _behaviors.ToArray();

            foreach (MshtmlElementBehavior behavior in behaviors)
            {
                if (behavior != null)
                    behavior.Dispose();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            Debug.Assert(disposeManagedResources, "ElementBehaviorManager was not disposed correctly.");

            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    DisposeCreatedBehaviors();
                }

                _disposed = true;
            }
        }
        private bool _disposed;

        ~ElementBehaviorManager()
        {
            Dispose(false);
        }
    }

    public delegate MshtmlElementBehavior ElementBehaviorCreator();
}
