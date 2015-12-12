// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.SHDocVw;
using OLECMDEXECOPT = OpenLiveWriter.Interop.SHDocVw.OLECMDEXECOPT;
using OLECMDF = OpenLiveWriter.Interop.SHDocVw.OLECMDF;
using OLECMDID = OpenLiveWriter.Interop.SHDocVw.OLECMDID;

namespace OpenLiveWriter.BrowserControl
{
    /// <summary>
    /// Abstract base class for BrowserCommand implementations
    /// </summary>
    public abstract class ExplorerBrowserCommand : IBrowserCommand
    {
        /// <summary>
        /// Initialize a BrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public ExplorerBrowserCommand(AxWebBrowser browser)
        {
            Browser = browser;
        }

        /// <summary>
        /// Determine whether the current command is enabled.
        /// </summary>
        public abstract bool Enabled { get; }

        /// <summary>
        /// Execute the command
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Browser control used by subclasses to implement commands (initialized
        /// in constructor)
        /// </summary>
        protected internal AxWebBrowser Browser;
    }

    /// <summary>
    /// Base class implementation for BrowserCommands that must be invoked directly
    /// via method calls (i.e. they are not available via QueryStatusWB or ExecWB).
    /// This base class implements QueryStatus and leaves Execute to be implemented
    /// by subclasses.
    /// </summary>
    public abstract class DirectInvokeBrowserCommand : ExplorerBrowserCommand
    {
        /// <summary>
        /// Initialize a DirectInvokeBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public DirectInvokeBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Set the current enabled status of the command (this must be done via an
        /// external call because this command is not capable of calling QueryStatusWB
        /// to determine its own status)
        /// </summary>
        /// <param name="enabled">true if enabled, false if disabled</param>
        public void SetEnabled(bool enabled)
        {
            m_enabled = enabled;
        }

        /// <summary>
        /// Determine whether the current command is enabled.
        /// </summary>
        public override bool Enabled
        {
            get { return m_enabled; }
        }

        /// <summary>
        /// Private member used to track current enabled status (default to enabled)
        /// </summary>
        private bool m_enabled = true;
    }

    /// <summary>
    /// New Window browser button
    /// </summary>
    public class NewWindowBrowserCommand : DirectInvokeBrowserCommand
    {
        /// <summary>
        /// Initialize a NewWindowBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public NewWindowBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            // determine location to navigate to (either current URL or about:blank)
            string location = Browser.LocationURL;
            if (location.Length == 0)
                location = BrowserLocations.AboutBlank;

            // open a new window and navigate to the specified location
            object m = Type.Missing;
            object flags = BrowserNavConstants.navOpenInNewWindow;
            Browser.Navigate(location, ref flags, ref m, ref m, ref m);
        }
    }

    /// <summary>
    /// Command for browser Back button
    /// </summary>
    public class GoBackBrowserCommand : DirectInvokeBrowserCommand
    {
        /// <summary>
        /// Initialize a GoBackBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public GoBackBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            // go back (ignore excpetion thrown if the history list doesn't have
            // a page to go back to)
            try
            {
                Browser.GoBack();
            }
            catch
            {
                Debug.Assert(false, "Invalid call to GoBack");
            }
        }
    }

    /// <summary>
    /// Command for browser Forward button
    /// </summary>
    public class GoForwardBrowserCommand : DirectInvokeBrowserCommand
    {
        /// <summary>
        /// Initialize a GoForwardBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public GoForwardBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            // go forward (ignore excpetion thrown if the history list doesn't have
            // a page to go forward to)
            try
            {
                Browser.GoForward();
            }
            catch
            {
                Debug.Assert(false, "Invalid call to GoForward");
            }
        }
    }

    /// <summary>
    /// Command for browser Stop button
    /// </summary>
    public class StopBrowserCommand : DirectInvokeBrowserCommand
    {
        /// <summary>
        /// Initialize a StopBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public StopBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            Browser.Stop();
        }
    }

    /// <summary>
    /// Command for browser Home button
    /// </summary>
    public class GoHomeBrowserCommand : DirectInvokeBrowserCommand
    {
        /// <summary>
        /// Initialize a GoHomeBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public GoHomeBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            Browser.GoHome();
        }
    }

    /// <summary>
    /// Command for browser Search button. Goes to the default search page.
    /// </summary>
    public class GoSearchBrowserCommand : DirectInvokeBrowserCommand
    {
        /// <summary>
        /// Initialize a GoSearchBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public GoSearchBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            Browser.GoSearch();
        }
    }

    /// <summary>
    /// Abstract base class for commands that are implemented by calling IShellUIHelper
    /// Provides a static helper for getting an interface to IShellUIHelper. Defers
    /// implementation of Execute to subclasses.
    /// </summary>
    public abstract class ShellUIHelperBrowserCommand : DirectInvokeBrowserCommand
    {
        /// <summary>
        /// Initialize a ShellUIHelperBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public ShellUIHelperBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Helper function that encapsulates getting an interface to an
        /// IShellUIHelper. Currently just creates and returns one, however
        /// we could change the implementation to cache a single IShellUIHelper
        /// globally (or per-thread).
        /// </summary>
        /// <returns>interface to IShellUIHelper</returns>
        protected static IShellUIHelper ShellUIHelper
        {
            get { return new ShellUIHelperClass(); }
        }
    }

    /// <summary>
    /// Command for Add Favorites...
    /// </summary>
    public class AddFavoriteBrowserCommand : ShellUIHelperBrowserCommand
    {
        /// <summary>
        /// Initialize an AddFavoriteBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public AddFavoriteBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            // determine the current URL and Title
            string url = Browser.LocationURL;
            object title = Browser.LocationName;

            // add to favorites
            ShellUIHelper.AddFavorite(url, ref title);
        }
    }

    /// <summary>
    /// Command for Organize Favorites...
    /// </summary>
    public class OrganizeFavoritesBrowserCommand : ShellUIHelperBrowserCommand
    {
        /// <summary>
        /// Initialize an OrganizeFavoritesBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public OrganizeFavoritesBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            // constant for calling Organize Favorites dialog
            const string ORGANIZE_FAVORITES = "OrganizeFavorites";

            // call organize favorites dialog
            object objNull = null;
            ShellUIHelper.ShowBrowserUI(ORGANIZE_FAVORITES, ref objNull);
        }
    }

    /// <summary>
    /// Command for Languages dialog
    /// </summary>
    public class LanguagesBrowserCommand : ShellUIHelperBrowserCommand
    {
        /// <summary>
        /// Initialize an LanguagesBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        public LanguagesBrowserCommand(AxWebBrowser browser)
            : base(browser)
        {
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            // constant for calling Languages dialog
            const string LANGUAGE_DIALOG = "LanguageDialog";

            // call organize favorites dialog
            object objNull = null;
            ShellUIHelper.ShowBrowserUI(LANGUAGE_DIALOG, ref objNull);
        }
    }

    /// <summary>
    /// Implementation of BrowserCommand for commands that can be accessed
    /// using IWebBrowser2.ExecWB.
    /// </summary>
    public class StandardBrowserCommand : ExplorerBrowserCommand
    {
        /// <summary>
        /// Initialize a NativeBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        /// <param name="cmdID">unique ID of command</param>
        public StandardBrowserCommand(AxWebBrowser browser, OLECMDID cmdID)
            : base(browser)
        {
            m_cmdID = cmdID;
        }

        /// <summary>
        /// Determine whether the current command is enabled.
        /// </summary>
        public override bool Enabled
        {
            get { return IsEnabled(m_cmdID); }
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        public override void Execute()
        {
            object input = null;
            object output = null;
            Browser.ExecWB(
                m_cmdID,
                OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT,
                ref input, ref output);
        }

        /// <summary>
        /// Helper function used to determine if a given command-id is enabled
        /// </summary>
        /// <param name="cmdID">command id</param>
        /// <returns>true if enabled, otherwise false</returns>
        protected bool IsEnabled(OLECMDID cmdID)
        {
            // query the underlying command
            OLECMDF cmdf = Browser.QueryStatusWB(cmdID);

            // return the appropriate value
            if ((cmdf & OLECMDF.OLECMDF_ENABLED) > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// ID of native browser command
        /// </summary>
        private OLECMDID m_cmdID;
    }

    /// <summary>
    /// Implementation of BrowserCommand for command that can be accessed through the
    /// IE private command group (find, view source, internet options). Note that
    /// this interface is verified to work through IE 6.0 but is not officially
    /// documented and guaranteed to work in future versions.
    /// </summary>
    public class PrivateBrowserCommand : ExplorerBrowserCommand
    {
        // constants defining known private browser commands. Constants are used
        // rather than an enum to keep the command-set 'open' for additional (as yet
        // undiscovered) private commands.
        public const int Find = 1;
        public const int ViewSource = 2;
        public const int InternetOptions = 3;

        // constants used for validation of private commandID (these should be
        // updated when new private commands are added)
        private const int PrivateCommandMin = 1;
        private const int PrivateCommandMax = 3;

        /// <summary>
        /// Initialize a PrivateBrowserCommand
        /// </summary>
        /// <param name="browser">reference to underlying browser control</param>
        /// <param name="cmdID">unique ID of command</param>
        public PrivateBrowserCommand(AxWebBrowser browser, uint cmdID)
            : base(browser)
        {
            // make sure the cmdID is valid
            Debug.Assert(cmdID >= PrivateCommandMin && cmdID <= PrivateCommandMax,
                "Invalid private command ID");

            // save the command ID
            m_cmdID = cmdID;
        }

        /// <summary>
        /// Determine whether the current command is enabled
        /// </summary>
        public override bool Enabled
        {
            get
            {
                // get the command target
                IOleCommandTargetWithExecParams target = GetCommandTarget();

                // if there is a target, query it for its status
                if (target != null)
                {
                    if (target is IHTMLDocument2)
                    {
                        // command to be queried
                        OLECMD oleCmd = new OLECMD();
                        oleCmd.cmdID = m_cmdID;

                        // query for the command's status
                        target.QueryStatus(
                            CGID_IWebBrowserPrivate,
                            1,
                            ref oleCmd,
                            IntPtr.Zero);

                        // check to see if the command is enabled
                        if ((oleCmd.cmdf & OpenLiveWriter.Interop.Com.OLECMDF.ENABLED) > 0)
                            return true;
                        else
                            return false;
                    }
                    else // not an HTML document
                        return false;
                }
                // no valid command target
                else
                    return false;
            }
        }

        /// <summary>
        /// Execute the command with an input and output parameter
        /// </summary>
        public override void Execute()
        {
            // get the command target
            IOleCommandTargetWithExecParams target = GetCommandTarget();

            // if there is a target, execute the command on it
            if (target != null)
            {
                object input = null;
                object output = null;
                try
                {
                    target.Exec(
                        CGID_IWebBrowserPrivate,
                        m_cmdID,
                        OpenLiveWriter.Interop.Com.OLECMDEXECOPT.DODEFAULT,
                        ref input,
                        ref output);
                }
                catch (COMException e)
                {
                    // The InternetOptions command throws a spurious exception
                    // every time it is invoked -- ignore it.
                    if (m_cmdID != InternetOptions)
                        throw e;
                }
            }
            // should never try to execute a command if there is no target!
            // (caller should have detected this via QueryStatus)
            else
            {
                Debug.Assert(false,
                    "Attempted to execute a command when there is no valid target");
            }
        }

        /// <summary>
        /// Helper function to get the appropriate command target for the command
        /// </summary>
        /// <returns>IOleCommandTarget if a valid one can be found for the command's
        /// context, return null if no command target can be located</returns>
        private IOleCommandTargetWithExecParams GetCommandTarget()
        {
            // get the document and try to get its command target
            object document = Browser.Document;
            if (document != null)
            {
                // return as IOleCommandTarget (returns null if document
                // is not an HTML document or doesn't support IOleCommandTarget)
                return document as IOleCommandTargetWithExecParams;
            }
            else
            {
                Debug.Assert(false,
                    "No current document for the browser! (not sure if this " +
                    "condition is actually invalid, asserting here to proactively " +
                    "detect when this occurs).");

                // no current document, no valid command target
                return null;
            }
        }

        /// <summary>
        /// ID of private command
        /// </summary>
        private uint m_cmdID;

        /// <summary>
        /// Command Group ID for private WebBrowser commands
        /// </summary>
        private static Guid CGID_IWebBrowserPrivate =
            new Guid(0xED016940, 0xBD5B, 0x11cf, 0xBA, 0x4E, 0x00, 0xC0, 0x4F, 0xD7, 0x08, 0x16);
    }

    /// <summary>
    /// Structure containing commonly used browser locations
    /// </summary>
    internal struct BrowserLocations
    {
        public static readonly string AboutBlank = "about:blank";
    }
}
