// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Specialized;

namespace OpenLiveWriter.CoreServices
{

    public delegate bool CanMatch(string text, int charactersMatched);

    public class Trie<T> where T : class
    {
        public void Add(string text, T value)
        {
            Add(text, value, 0);
        }

        private void Add(string text, T value, int position)
        {
            if (position >= text.Length)
            {
                _value = value;
                return;
            }

            char letter = text[position];
            Trie<T> currentNode = GetChildNode(letter, true);
            currentNode.Add(text, value, position + 1);
        }

        public void AddReverse(string reverseText, T value)
        {
            AddReverse(reverseText, value, reverseText.Length - 1);
        }

        private void AddReverse(string reverseText, T value, int position)
        {
            if (position < 0)
            {
                _value = value;
                return;
            }

            char letter = reverseText[position];
            Trie<T> currentNode = GetChildNode(letter, true);
            currentNode.AddReverse(reverseText, value, position - 1);
        }

        public void DumpTree()
        {
            DumpTree(0);
        }

        private void DumpTree(int depth)
        {
            string indent = "";
            foreach (DictionaryEntry entry in _children)
            {
                for (int i = 0; i < depth; i++)
                    indent = indent + " ";
                Console.WriteLine(indent + entry.Key);
                ((Trie<T>)entry.Value).DumpTree(depth + 1);
            }
        }

        public T Find(string text, CanMatch canMatch, out int length)
        {
            return Find(text, 0, canMatch, out length);
        }

        private T Find(string text, int position, CanMatch canMatch, out int length)
        {
            if (position < text.Length)
            {
                Trie<T> node = GetChildNode(text[position], false);
                if (node != null)
                {
                    T childUrl = node.Find(text, position + 1, canMatch, out length);
                    if (childUrl != null)
                    {
                        length++;
                        return childUrl;
                    }
                }
            }

            if (_value != null && (canMatch == null || canMatch(text, position)))
            {
                length = 0;
                return _value;
            }

            length = -1;
            return null;
        }

        private Trie<T> GetChildNode(char letter, bool add)
        {
            if (_children == null)
            {
                if (!add)
                    return null;

                _children = new HybridDictionary();
            }

            Trie<T> childNode = (Trie<T>)_children[letter];
            if (childNode == null && add)
            {
                childNode = new Trie<T>();
                _children[letter] = childNode;
            }
            return childNode;
        }
        private HybridDictionary _children;

        private T _value;
    }

}
