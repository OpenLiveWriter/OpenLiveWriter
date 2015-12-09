using System;
using System.Collections.Generic;
using System.Text;
using BlogRunner.Core.Config;
using System.IO;

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
            ForEach((key, value) =>
            {
                output.WriteLine(string.Format("{0}: {1}", key, value));
            });
        }
    }
}
