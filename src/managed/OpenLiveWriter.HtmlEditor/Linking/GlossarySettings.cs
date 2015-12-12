// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    public class GlossarySettings
    {
        private static readonly SettingsPersisterHelper Settings =
            ApplicationEnvironment.UserSettingsRoot.GetSubSettings("LinkGlossary");

        public static bool AutoLinkEnabled
        {
            get
            {
                if (!_enabledInit)
                {
                    _enabled = Settings.GetBoolean(AUTO, true);
                    _enabledInit = true;
                }
                return _enabled;
            }
            set
            {
                Settings.SetBoolean(AUTO, value);
                _enabled = value;
            }
        }

        public static bool AutoLinkTermsOnlyOnce
        {

            get
            {
                if (!_linkOnceInit)
                {
                    _linkOnce = Settings.GetBoolean(ONLYONCE, true);
                    _linkOnceInit = true;
                }
                return _linkOnce;
            }
            set
            {
                Settings.SetBoolean(ONLYONCE, value);
                _linkOnce = value;
            }
        }

        public static bool Initialized
        {
            get
            {
                if (!_initInit)
                {
                    _init = Settings.GetBoolean(INIT, false);
                    _initInit = true;
                }
                return _init;
            }
            set
            {
                Settings.SetBoolean(INIT, value);
                _init = value;
            }
        }

        private static bool _enabled = true;
        private static bool _enabledInit = false;

        private static bool _linkOnce = true;
        private static bool _linkOnceInit = false;

        private static bool _init = false;
        private static bool _initInit = false;

        private const string AUTO = "AutoLinkEnabled";
        private const string ONLYONCE = "AutoLinkTermsOnlyOnce";
        private const string INIT = "Initialized";

    }
}
