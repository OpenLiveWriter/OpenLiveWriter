// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContent method to enable creation of new content from an Insert dialog.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InsertableContentSourceAttribute : Attribute
    {
        /// <summary>
        /// The menu text
        /// </summary>
        [NotNull]
        private string menuText;

        /// <summary>
        /// The sidebar text
        /// </summary>
        [NotNull]
        private string sidebarText;

        /// <summary>
        /// Initialize a new instance of an InsertableContentSourceAttribute.
        /// </summary>
        /// <param name="menuText">Text used to describe the insertable content on the Insert menu.</param>
        public InsertableContentSourceAttribute([NotNull] string menuText)
        {
            this.MenuText = menuText;
            this.SidebarText = this.menuText;
        }

        /// <summary>
        /// Gets or sets the text used to describe the insertable content on the Insert menu.
        /// </summary>
        [NotNull]
        public string MenuText
        {
            get
            {
                return this.menuText;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(this.MenuText));
                }

                this.menuText = value;
            }
        }

        /// <summary>
        /// Gets or sets the text used to describe the insertable content on the Sidebar. This is optional and
        /// can be specified to provide a shorter name for the insertable content (there is
        /// less room on the Sidebar than in the Insert menu).
        /// </summary>
        [NotNull]
        public string SidebarText
        {
            get
            {
                return this.sidebarText;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(this.SidebarText));
                }

                this.sidebarText = value;
            }
        }
    }
}
