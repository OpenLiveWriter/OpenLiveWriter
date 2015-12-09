// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public interface IAutoReplaceFinder
    {
        string FindMatch(string text, out int length);
        int MaxLengthHint { get; }
    }

    public class AutoReplaceFinder : IAutoReplaceFinder
    {
        public AutoReplaceFinder(CanMatch canMatch)
        {
            _canMatch = canMatch;
        }

        public void Add(string text, string value)
        {
            _maxLengthHint = Math.Max(text.Length, _maxLengthHint);
            _trie.AddReverse(text.ToLower(CultureInfo.CurrentCulture), value);
        }

        public string FindMatch(string text, out int length)
        {
            if (text == null)
            {
                length = -1;
                return null;
            }
            return _trie.Find(StringHelper.Reverse(text).ToLower(CultureInfo.CurrentCulture), _canMatch, out length);
        }

        public int MaxLengthHint
        {
            get
            {

                return _maxLengthHint + 1;
            }
        }
        private int _maxLengthHint = -1;

        private readonly Trie<string> _trie = new Trie<string>();
        private readonly CanMatch _canMatch;
    }
}
