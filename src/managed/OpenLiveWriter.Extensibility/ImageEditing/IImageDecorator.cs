// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.Extensibility.ImageEditing
{
#if SUPPORT_PLUGINS
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public class ImageDecoratorAttribute : Attribute
    {
        public ImageDecoratorAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The display name to show in the image effects list.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// The unique ID to assign a decorator.
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// The decorator group to assign a decorator to.
        /// </summary>
        [Obsolete("Groups need to have separate names and IDs in order to be properly localizable. This should become GroupId.", true)]
        public string Group
        {
            get { return group; }
            set { group = value; }
        }

        /// <summary>
        /// If true, then the decorator supports an editor control that should be display if the user
        /// wants to customize the decorator's behavior.
        /// </summary>
        public bool Editable
        {
            get { return editable; }
            set { editable = value; }
        }

        /// <summary>
        /// If true, then the decorator and its current settings can be applied by default if the user
        /// saves an set of applied image effects as the default.  If false, then the decorator will not
        /// be included in the set of effects to apply to images by default.
        /// </summary>
        public bool Defaultable
        {
            get { return defaultable; }
            set { defaultable = value; }
        }

        private string name;
        private string id;
        private string group;
        private bool editable;
        private bool defaultable;
    }
#endif

    /// <summary>
    /// Interace implemented by classes that want to add behaviors or effects to embedded images.
    /// </summary>
    public interface IImageDecorator
    {
        /// <summary>
        /// Apply the image decorator.
        /// </summary>
        /// <param name="context"></param>
        void Decorate(ImageDecoratorContext context);

        /// <summary>
        /// Create an editor for customizing the decorator settings.
        /// </summary>
        /// <returns></returns>
        ImageDecoratorEditor CreateEditor(CommandManager commandManager);
    }

    public interface IImageDecoratorOriginalSizeAdjuster : IImageDecorator
    {
        /// <summary>
        /// Allows decorators to change the conceptual "original" size
        /// of the image. This is needed for crop support.
        /// </summary>
        void AdjustOriginalSize(IProperties properties, ref Size size);
    }
}
