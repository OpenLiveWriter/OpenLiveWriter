// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    ///
    /// </summary>
    public interface IHelpProviderContext
    {
        /// <summary>
        ///
        /// </summary>
        HelpProvider HelpProvider { get; }
    }

    public class HelpProviderContext
    {
        public static void Bind(HelpProvider localHelpProvider, Control control)
        {
            IHelpProviderContext context = control.FindForm() as IHelpProviderContext;
            if (context != null)
            {
                if (localHelpProvider.GetShowHelp(control))
                {
                    string helpString = localHelpProvider.GetHelpString(control);
                    if (helpString != null)
                    {
                        context.HelpProvider.SetShowHelp(control, true);
                        context.HelpProvider.SetHelpString(control, helpString);
                    }
                }
            }
        }

    }

}
