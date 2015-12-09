// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace BlogRunner.Core.Tests
{
    public class SupportsEmbedsTest : BodyContentPostTest
    {
        public override string BodyContentString
        {
            get
            {
                return @"<embed src=""http://s3.amazonaws.com/slideshare/ssplayer2.swf?doc=inconvenient-truth-posters1319"" type=""application/x-shockwave-flash"" allowscriptaccess=""always"" allowfullscreen=""true"" width=""425"" height=""355"" />";
            }
        }

        public override void HandleContentResult(string result, ITestResults results)
        {
            if (result == null)
                throw new InvalidOperationException("Embed test markers were not found!");
            else if (result.ToLowerInvariant().Contains("<embed"))
                results.AddResult("supportsEmbeds", YES);
            else
                results.AddResult("supportsEmbeds", NO);
        }
    }
}
