// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// Summary description for IProgressCategoryProvider.
    /// </summary>
    public interface IProgressCategoryProvider
    {
        /// <summary>
        /// Should the user-interface show categories
        /// </summary>
        bool ShowCategories { get; }

        /// <summary>
        /// The current category that is being processed
        /// </summary>
        ProgressCategory CurrentCategory { get; }

        /// <summary>
        /// category changed
        /// </summary>
        event EventHandler ProgressCategoryChanged;
    }

    /// <summary>
    /// Progress category
    /// </summary>
    public class ProgressCategory
    {
        /// <summary>
        /// Initialize a progress category
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="icon">icon</param>
        public ProgressCategory(string name, Image icon)
        {
            Name = name;
            _icon = icon;
        }

        /// <summary>
        /// Category name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Category icon
        /// </summary>
        public Image Icon
        {
            get
            {
                return _icon;
            }
        }
        private readonly Image _icon;
    }
}
