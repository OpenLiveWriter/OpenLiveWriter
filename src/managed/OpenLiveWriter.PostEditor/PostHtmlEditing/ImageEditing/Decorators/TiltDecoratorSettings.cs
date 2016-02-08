// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    class TiltDecoratorSettings
    {

        public TiltDecoratorSettings(IProperties settings)
        {
            _settings = settings;
        }
        private readonly IProperties _settings;

        public int TiltDegrees
        {
            get
            {
                return _settings.GetInt(TILTDEGREES, 0);
            }
            set
            {
                _settings.SetInt(TILTDEGREES, value);
            }
        }

        private const string TILTDEGREES = "TiltDegrees";
    }
}
