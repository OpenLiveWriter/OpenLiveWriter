// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Globalization;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    public class ExceptionMessage
    {
        private string title;
        private string messageFormat;
        private bool unexpected;

        public ExceptionMessage(string title, string messageFormat, bool unexpected)
        {
            this.title = title;
            this.messageFormat = messageFormat;
            this.unexpected = unexpected;
        }

        public ExceptionMessage(string messageFormat, bool unexpected)
        {
            this.messageFormat = messageFormat;
            this.unexpected = unexpected;
        }

        public string Title
        {
            get { return title; }
        }

        public string GetTitle(string defaultTitle)
        {
            return title != null ? title : defaultTitle;
        }

        public string MessageFormat
        {
            get { return messageFormat; }
        }

        public string GetMessage(params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, messageFormat, args);
        }

        public bool Unexpected
        {
            get { return unexpected; }
        }
    }
}
