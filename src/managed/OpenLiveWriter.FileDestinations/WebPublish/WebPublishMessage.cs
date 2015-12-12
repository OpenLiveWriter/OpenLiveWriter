// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.FileDestinations
{
    public class WebPublishMessage
    {
        private DisplayMessage _displayMessage;
        private object[] _textFormatArgs;

        public WebPublishMessage(MessageId messageId, params object[] textFormatArgs)
        {
            _displayMessage = new DisplayMessage(messageId);
            _textFormatArgs = textFormatArgs;
        }

        public string Title
        {
            get { return _displayMessage.Title; }
        }

        public string Text
        {
            get { return string.Format(CultureInfo.CurrentCulture, _displayMessage.Text, _textFormatArgs); }
        }
    }
}
