// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.PostEditor.Video
{
    internal class CategoryItem : IComparable<CategoryItem>
    {
        private string _categoryId;
        internal string CategoryId
        {
            get
            {
                return _categoryId;
            }
            set
            {
                _categoryId = value;
            }
        }

        private string _categoryName;
        internal string CategoryName
        {
            get
            {
                return _categoryName;
            }
            set
            {
                _categoryName = value;
            }
        }

        internal CategoryItem(string categoryId, string categoryName)
        {
            _categoryName = categoryName;
            _categoryId = categoryId;
        }

        public override string ToString()
        {
            return CategoryName;
        }

        public int CompareTo(CategoryItem other)
        {
            return this.CategoryName.CompareTo(other.CategoryName);
        }

    }

    internal class SecurityItem
    {
        private string _securityId;
        internal string SecurityId
        {
            get
            {
                return _securityId;
            }
            set
            {
                _securityId = value;
            }
        }

        private string _securityName;
        internal string SecurityName
        {
            get
            {
                return _securityName;
            }
            set
            {
                _securityName = value;
            }
        }

        internal SecurityItem(string securityId, string securityName)
        {
            _securityName = securityName;
            _securityId = securityId;
        }

        public override string ToString()
        {
            return SecurityName;
        }
    }
}
