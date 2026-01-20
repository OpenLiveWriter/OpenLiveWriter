// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides application management service.
    /// </summary>
    public sealed class ApplicationManager
    {
        /// <summary>
        /// The ApplicationStyle object.  This is only used to color the Open Live Writer chrome,
        /// it is not used in the ContentEditor, so it is fine to be a thread static.
        /// </summary>
        [ThreadStatic]
        private static ApplicationStyle applicationStyle;

        /// <summary>
        /// The CommandManager object.
        /// </summary>
        [ThreadStatic]
        private static CommandManager commandManager;

        /// <summary>
        /// Gets or sets the ApplicationStyle object
        /// </summary>
        public static ApplicationStyle ApplicationStyle
        {
            get
            {
                if (applicationStyle == null)
                    applicationStyle = new ApplicationStyleSkyBlue();
                return applicationStyle;
            }
            set
            {
                applicationStyle = value;
                OnApplicationStyleChanged();
            }
        }

        /// <summary>
        /// Event raised when ApplicationStyle changes
        /// </summary>
        public static event EventHandler ApplicationStyleChanged;

        private static void OnApplicationStyleChanged()
        {
            ApplicationStyleChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the CommandManager object
        /// </summary>
        public static CommandManager CommandManager
        {
            get
            {
                return commandManager;
            }
            set
            {
                commandManager = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationManager class.
        /// </summary>
        private ApplicationManager()
        {
        }
    }
}
