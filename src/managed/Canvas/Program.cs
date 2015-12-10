// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Configuration;

namespace OpenLiveWriter.Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

// ignore this error just this once since we need to look at a special path depending on the test scenario...
#pragma warning disable 0618

            string s = ConfigurationManager.AppSettings["binPath"];
            AppDomain.CurrentDomain.AppendPrivatePath(s);

#pragma warning restore 0618

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CanvasForm());
        }
    }
}
