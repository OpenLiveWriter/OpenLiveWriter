// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace BlogRunner.Core.Tests
{
    public class SupportsScriptsTest : BodyContentPostTest
    {
        public override string BodyContentString
        {
            get
            {
                return @"<script language=""javascript"">document.write('foo!');</script>";
            }
        }

        public override void HandleContentResult(string result, ITestResults results)
        {
            if (result == null)
            {
                Debug.Fail("Scripts gone");
                results.AddResult("supportsScripts", "Unknown");
            }
            else if (result.ToLowerInvariant().Contains("script"))
                results.AddResult("supportsScripts", YES);
            else
                results.AddResult("supportsScripts", NO);
        }
    }
}
