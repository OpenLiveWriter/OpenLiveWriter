// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageSizeSpinnerCommand : SpinnerCommand
    {
        private const int DefaultMinValue = 1;
        private const int DefaultMaxValue = 4096;
        private const int DefaultIncrement = 1;
        private const uint DefaultDecimalPlaces = 0;
        private const string DefaultRepresentativeString = "4096";
        private const string DefaultFormatString = "";

        public ImageSizeSpinnerCommand(CommandId commandId)
            : base(commandId, DefaultMinValue, DefaultMaxValue, DefaultMinValue, DefaultIncrement,
                   DefaultDecimalPlaces, DefaultRepresentativeString, DefaultFormatString)
        {
        }
    }
}
