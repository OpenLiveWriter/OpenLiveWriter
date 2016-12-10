// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Windows.Forms;

    using JetBrains.Annotations;

    /// <summary>
    /// Base class for all Open Live Writer plugins.
    /// </summary>
    public class WriterPlugin
    {
        /// <summary>
        /// Gets the Global Plugin options (user preferences, defaults, etc.).2
        /// </summary>
        [CanBeNull]
        protected IProperties Options { get; private set; }

        /// <summary>
        /// Edit the plugins global options. This method can be called if the value of the
        /// HasEditableOptions property WriterPluginAttribute is true (default is false).
        /// </summary>
        /// <param name="dialogOwner">Owner for the options dialog.</param>
#pragma warning disable CC0057 // Unused parameters
        public virtual void EditOptions([NotNull] IWin32Window dialogOwner)
#pragma warning restore CC0057 // Unused parameters
        {
            throw new NotImplementedException(nameof(WriterPlugin.EditOptions));
        }

        /// <summary>
        /// Initialize the plugin. Default implementation saves a reference to the global pluginOptions
        /// which may be subsequently accessed using the Options property. If subclasses override this
        /// method they must call the base implementation to ensure that this reference is saved.
        /// </summary>
        /// <param name="pluginOptions">Plugin options.</param>
        public virtual void Initialize([CanBeNull] IProperties pluginOptions)
        {
            this.Options = pluginOptions;
        }
    }
}
