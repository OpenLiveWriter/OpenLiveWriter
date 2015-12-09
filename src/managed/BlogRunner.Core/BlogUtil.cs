// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using BlogRunner.Core.Config;
using OpenLiveWriter.Extensibility.BlogClient;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.IO;
using OpenLiveWriter.HtmlParser.Parser;
using System.Net.Cache;

namespace BlogRunner.Core
{
    public class BlogUtil
    {
        public static string ShortGuid
        {
            get
            {
                byte[] bytes = Guid.NewGuid().ToByteArray();
                long longVal = BitConverter.ToInt64(bytes, 0) ^ BitConverter.ToInt64(bytes, 8);
                return Convert.ToBase64String(BitConverter.GetBytes(longVal)).TrimEnd('=');
            }
        }
    }
}
