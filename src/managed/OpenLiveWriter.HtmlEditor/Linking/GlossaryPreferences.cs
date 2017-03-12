// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    class GlossaryPreferences : Preferences
    {
        public GlossaryPreferences() : base("LinkGlossary")
        {
        }

        public bool AutoLinkEnabled
        {
            get
            {
                return _autoLinkEnabled;
            }
            set
            {
                _autoLinkEnabled = value;
                Modified();
            }
        }

        public bool AutoLinkTermsOnlyOnce
        {
            get { return _autoLinkTermsOnlyOnce; }
            set { _autoLinkTermsOnlyOnce = value; Modified(); }
        }

        protected override void LoadPreferences()
        {
            _autoLinkEnabled = GlossarySettings.AutoLinkEnabled;
            _autoLinkTermsOnlyOnce = GlossarySettings.AutoLinkTermsOnlyOnce;
        }

        protected override void SavePreferences()
        {
            GlossarySettings.AutoLinkEnabled = _autoLinkEnabled;
            GlossarySettings.AutoLinkTermsOnlyOnce = _autoLinkTermsOnlyOnce;
        }

        private bool _autoLinkEnabled = true;
        private bool _autoLinkTermsOnlyOnce = true;

    }
}
