// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using BlogRunner.Core.Config;
using System.IO;
using System.Globalization;

namespace BlogRunner.Core
{
    public interface ITestResults
    {
        void AddResult(string key, string value);
    }

    public class TestResultImpl : ITestResults
    {
        public delegate void Func(string key, string val);

        private Dictionary<string, string> results = new Dictionary<string, string>();

        public void AddResult(string key, string value)
        {
            results.Add(key, value);
        }

        public void ForEach(Func func)
        {
            List<string> keys = new List<string>(results.Keys);
            keys.Sort(StringComparer.CurrentCultureIgnoreCase);
            foreach (string key in keys)
            {
                func(key, results[key]);
            }
        }

        public void Dump(TextWriter output)
        {
            ForEach(delegate (string key, string value)
            {
                output.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", key, value));
            });
        }
    }
}
