// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.BlogClient
{

    public class BlogDescriptor
    {
        public BlogDescriptor(string id, string name, string homepageUrl)
        {
            Id = id;
            Name = name;
            HomepageUrl = homepageUrl;
        }

        public readonly string Id;

        public readonly string Name;

        public readonly string HomepageUrl;

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return (obj as BlogDescriptor).Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public class Comparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return string.Compare((x as BlogDescriptor).Name, (y as BlogDescriptor).Name, StringComparison.CurrentCulture);
            }
        }
    }

}
