// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.IO;
using System.Text;
using System.Web;

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class FormData
    {
        private readonly ArrayList pairs;
        private readonly bool cullMissingValues;

        public FormData() : this(false)
        {
        }

        public FormData(bool cullMissingValues, params string[] parameters)
        {
            pairs = new ArrayList();

            this.cullMissingValues = cullMissingValues;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i += 2)
                {
                    string name = parameters[i];
                    string val = parameters[i + 1];
                    if (!cullMissingValues || (val != null && val != string.Empty))
                        Add(name, val);
                }
            }
        }

        public void Add(string name, string value)
        {
            if (!cullMissingValues || (value != null && value != string.Empty))
                pairs.Add(new NameValuePair(name, value));
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < pairs.Count; i++)
            {
                if (i != 0)
                    output.Append("&");

                NameValuePair pair = (NameValuePair)pairs[i];
                output.Append(HttpUtility.UrlEncode(pair.Name));
                if (pair.Value != null)
                {
                    output.Append("=");
                    output.Append(HttpUtility.UrlEncode(pair.Value));
                }
            }
            return output.ToString();
        }

        public Stream ToStream()
        {
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < pairs.Count; i++)
            {
                if (i != 0)
                    stream.WriteByte((byte)'&');

                NameValuePair pair = (NameValuePair)pairs[i];
                WriteBytes(stream, HttpUtility.UrlEncodeToBytes(pair.Name));
                if (pair.Value != null)
                {
                    stream.WriteByte((byte)'=');
                    WriteBytes(stream, HttpUtility.UrlEncodeToBytes(pair.Value));
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private void WriteBytes(Stream s, byte[] b)
        {
            s.Write(b, 0, b.Length);
        }

        private class NameValuePair
        {
            public readonly string Name;
            public readonly string Value;

            public NameValuePair(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

    }
}
