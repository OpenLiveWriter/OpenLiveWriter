// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Base class for all Open Live Writer plugins.
    /// </summary>
    public class WriterPlugin
    {
        /// <summary>
        /// Initialize the plugin. Default implementation saves a reference to the global pluginOptions
        /// which may be subsequently accessed using the Options property. If subclasses override this
        /// method they must call the base implementation to ensure that this reference is saved.
        /// </summary>
        /// <param name="pluginOptions">Plugin options.</param>
        public virtual void Initialize(IProperties pluginOptions)
        {
            _options = pluginOptions;
        }

        /// <summary>
        /// Edit the plugins global options. This method can be called if the value of the
        /// HasEditableOptions property WriterPluginAttribute is true (default is false).
        /// </summary>
        /// <param name="dialogOwner">Owner context for the options dialog.</param>
        public virtual void EditOptions(IBlogClientUIContext dialogOwner)
        {
            throw new NotImplementedException("WriterPlugin.EditOptions");
        }

        /// <summary>
        /// Global Plugin options (user preferences, defaults, etc.).2
        /// </summary>
        protected IProperties Options
        {
            get { return _options; }
        }

        private IProperties _options;
    }
}
