// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContent method to enable creation of new content from an Insert dialog.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InsertableContentSourceAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance of an InsertableContentSourceAttribute.
        /// </summary>
        /// <param name="menuText">Text used to describe the insertable content on the Insert menu.</param>
        public InsertableContentSourceAttribute(string menuText)
        {
            MenuText = menuText;
            SidebarText = _menuText;
        }

        /// <summary>
        /// Text used to describe the insertable content on the Insert menu.
        /// </summary>
        public string MenuText
        {
            get
            {
                return _menuText;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("InsertableContentSource.MenuText");

                _menuText = value;
            }
        }
        private string _menuText;

        /// <summary>
        /// Text used to describe the insertable content on the Sidebar. This is optional and
        /// can be specified to provide a shorter name for the insertable content (there is
        /// less room on the Sidebar than in the Insert menu).
        /// </summary>
        public string SidebarText
        {
            get
            {
                return _sidebarText;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("InsertableContentSource.SidebarText");

                _sidebarText = value;
            }
        }
        private string _sidebarText;
    }
}
