// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Factory used for generating element behavior instances
    /// </summary>
    public class ElementBehaviorFactoryForExistingBehavior : IElementBehaviorFactoryRaw
    {
        /// <summary>
        /// Initialize the factory with an already existing IElementBehavior
        /// </summary>
        /// <param name="behavior">behavior we should return</param>
        public ElementBehaviorFactoryForExistingBehavior(IElementBehaviorRaw behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException("behavior", "behavior cannot be null");

            renderingBehavior = behavior;
        }

        /// <summary>
        /// Return the behavior we were passed in our constructor. Note that this method
        /// insures that it is called only once via an assertion.
        /// </summary>
        /// <param name="bstrBehavior"></param>
        /// <param name="bstrBehaviorUrl"></param>
        /// <param name="pSite"></param>
        /// <param name="ppBehavior"></param>
        public void FindBehavior(string bstrBehavior, string bstrBehaviorUrl, IElementBehaviorSite pSite, ref IElementBehaviorRaw ppBehavior)
        {
            // Fix bug 519990: Setting ppBehavior to null, even in the failure case,
            // causes Writer to crash when an embedded Google Map is pasted into the
            // editor. If there is no behavior, DON'T TOUCH ppBehavior!

            // remind users of this class not to allow this to be called more than once
            Debug.Assert(findBehaviorCalled == false);

            // update call state
            findBehaviorCalled = true;

            // return rendering behavior
            ppBehavior = renderingBehavior;
        }

        /// <summary>
        /// Rendering behavior we are going to return
        /// </summary>
        private readonly IElementBehaviorRaw renderingBehavior;

        /// <summary>
        /// State variable to verify that FindBehavior is called only once on this factory
        /// </summary>
        private bool findBehaviorCalled = false;
    }

}
