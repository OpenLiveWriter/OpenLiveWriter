// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;

namespace OpenLiveWriter.PostEditor
{
    public static class GlobalEditorOptions
    {
        private static bool _initialized = false;
        private static IContentTarget _contentTarget;
        private static ISettingsProvider _settingsProvider;

        public static IContentTarget ContentTarget
        {
            get
            {
                return _contentTarget;
            }
        }

        public static void Init(IContentTarget contentTarget, ISettingsProvider settingsProvider)
        {
            Debug.Assert(!_initialized, "Initializing GlobalEditorOptions more than once!");
            _initialized = true;
            _contentTarget = contentTarget;
            _settingsProvider = settingsProvider;
        }

        public static bool SupportsFeature(ContentEditorFeature featureName)
        {
            Debug.Assert(_contentTarget != null, "Attempting to check for features without specifying a ContentTarget.");
            return _contentTarget.SupportsFeature(featureName);
        }

        /// <summary>
        /// If getting the settings fails, this method throws an InvalidSettingException.
        /// </summary>
        public static T GetSetting<T>(ContentEditorSetting setting)
        {
            T value = default(T);

            ManagedPropVariant managedPropVariant = _settingsProvider.GetSetting(setting);
            PropVariant propVariant = ManagedPropVariant.ToPropVariant(managedPropVariant);

            if (!propVariant.IsError() && !propVariant.IsNull())
            {
                value = (T)propVariant.Value;
            }
            else
            {
                Debug.Fail("PROPVARIANT unexpectedly null or error!");
                throw new ContentEditorSettingException(setting);
            }

            propVariant.Clear();
            managedPropVariant.Clear();

            return value;
        }
    }
}
