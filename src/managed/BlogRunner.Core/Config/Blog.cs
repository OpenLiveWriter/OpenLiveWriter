// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Blog.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BlogRunner.Core.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// Class Blog.
    /// </summary>
    public class Blog
    {
        /// <summary>
        /// Gets or sets the homepage URL.
        /// </summary>
        /// <value>The homepage URL.</value>
        [XmlElement(ElementName = "homepageUrl")]
        public string HomepageUrl { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        [XmlElement(ElementName = "username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the API URL.
        /// </summary>
        /// <value>The API URL.</value>
        [XmlElement(ElementName = "apiUrl")]
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the blog identifier.
        /// </summary>
        /// <value>The blog identifier.</value>
        [XmlElement(ElementName = "blogId")]
        public string BlogId { get; set; }
    }

    /// <summary>
    /// Enum BlogApi
    /// </summary>
    public enum BlogApi { XmlRpc, AtomPub }
}
