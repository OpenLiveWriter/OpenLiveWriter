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
        private string _title;
        private string _text;

        public WebPublishMessage(MessageId messageId, params object[] textFormatArgs)
        {
            _displayMessage = new DisplayMessage(messageId);
            _textFormatArgs = textFormatArgs;
        }

        /// <summary>
        /// Parameterless constructor for derived classes using InitializeComponent
        /// </summary>
        protected WebPublishMessage()
        {
            _textFormatArgs = Array.Empty<object>();
        }

        public string Title
        {
            get { return _displayMessage?.Title ?? _title; }
            protected set { _title = value; }
        }

        public string Text
        {
            get 
            { 
                if (_displayMessage != null)
                    return string.Format(CultureInfo.CurrentCulture, _displayMessage.Text, _textFormatArgs);
                return _textFormatArgs?.Length > 0 
                    ? string.Format(CultureInfo.CurrentCulture, _text, _textFormatArgs) 
                    : _text;
            }
            protected set { _text = value; }
        }

        protected object[] TextFormatArgs
        {
            get { return _textFormatArgs; }
            set { _textFormatArgs = value ?? Array.Empty<object>(); }
        }
    }
}
