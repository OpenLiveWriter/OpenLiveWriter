// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    public class BlogPostKeyword : ICloneable
    {
        public readonly string Name;

        public BlogPostKeyword(string name)
        {
            Name = name;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new BlogPostKeyword(Name);
        }

        #endregion
    }
}
