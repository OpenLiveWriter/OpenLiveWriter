// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Manages single-instance application behavior. Example usage:
    ///
    /// public static void Main(string[] args)
    /// {
    ///		SingleInstanceApplicationManager.Run(
    ///			"MyAppID",
    ///			new SingleInstanceApplicationManager.LaunchAction(LaunchAction),
    ///			args);
    /// }
    ///
    /// private static MyAppForm form;
    ///
    /// private static bool LaunchAction(string[] args, bool isFirstInstance)
    /// {
    ///		if (isFirstInstance)
    ///		{
    ///			form = new MyAppForm();
    ///			Application.Run(form);
    ///			return true;
    ///		}
    ///		else
    ///		{
    ///			if (form != null && form.IsCreated && !form.IsDisposed)
    ///			{
    ///				form.Activate();
    ///				return true;
    ///			}
    ///			return false;
    ///		}
    /// }
    /// </summary>
    public class SingleInstanceApplicationManager
    {
        /// <summary>
        /// Delegate for "launching" the app. The isFirstInstance parameter
        /// will be true if the call represents the initial startup of the
        /// application; if false, then an additional launch was requested
        /// by another instance of the program.
        ///
        /// If isFirstInstance is false, the return value should be false if
        /// this process was unable to service the request AND it should be
        /// retried. If isFirstInstance is true, the return value is ignored.
        /// </summary>
        public delegate bool LaunchAction(string[] args, bool isFirstInstance);

        public static void Run(string appId, LaunchAction action, string[] args)
        {
            Mutex m_mutex = new Mutex(false, @"Local\" + appId);
            RunningObjectTable rot = new RunningObjectTable();

            for (int i = 0; i < 120; i++) // wait for ~60 seconds
            {
                if (m_mutex.WaitOne(1, false))
                {
                    // we are the first instance
                    try
                    {
                        using (rot.Register(appId, new OleCommandTargetImpl(action)))
                        {
                            action(args, true);
                        }
                        return;
                    }
                    finally
                    {
                        m_mutex.ReleaseMutex();
                    }
                }
                else
                {
                    // we are not the first instance
                    IOleCommandTargetWithExecParams instance = rot.GetObject(appId) as IOleCommandTargetWithExecParams;

                    if (instance != null)
                    {
                        try
                        {
                            object dummy = null;
                            object processId = null;
                            instance.Exec(Guid.Empty, 0, OLECMDEXECOPT.DODEFAULT, ref dummy, ref processId);
                            User32.AllowSetForegroundWindow((int)processId);

                            object objArgs = args;
                            object result = null;
                            instance.Exec(Guid.Empty, 1, OLECMDEXECOPT.DODEFAULT, ref objArgs, ref result);
                            if (result is bool && (bool)result)
                                return;
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.ToString());
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(instance);
                        }
                    }
                }

                Thread.Sleep(500);
            }

            Trace.WriteLine(appId + " could not start!");
        }

        internal class OleCommandTargetImpl : IOleCommandTargetWithExecParams
        {
            private readonly LaunchAction _action;

            public OleCommandTargetImpl(LaunchAction action)
            {
                _action = action;
            }

            public void QueryStatus(Guid pguidCmdGroup, uint cCmds, ref OLECMD prgCmds, IntPtr pCmdText)
            {
                throw new COMException("QueryStatus not Implemented", HRESULT.E_NOTIMPL);
            }

            public void Exec(Guid pguidCmdGroup, uint nCmdID, OLECMDEXECOPT nCmdexecopt, ref object pvaIn, ref object pvaOut)
            {
                if (nCmdID == 0)
                {
                    using (Process p = Process.GetCurrentProcess())
                        pvaOut = p.Id;
                }
                else if (nCmdID == 1)
                {
                    // WinLive 198331
                    // We could be on an MTA thread at this point, but action here involves writer UI
                    // and we need an STA compliant thread for it. We will spin up a new thread here
                    // which is explicitly set to STA and launch our action through it.
                    LaunchActionThreadWithState launchAction = new LaunchActionThreadWithState(_action, pvaIn as string[]);
                    Thread staThread = new Thread(new ThreadStart(launchAction.ThreadProc));
                    // Set to STA model
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start();
                    // Wait for the new thread to finish while pumping any COM messages
                    staThread.Join();

                    // Grab the return value
                    pvaOut = launchAction.ReturnValue;
                }
            }
        }

        /// <summary>
        /// Simple class to wrap a LaunchAction thread that can take parameters and return values
        /// </summary>
        internal class LaunchActionThreadWithState
        {
            private string[] _args;
            private readonly LaunchAction _launchAction;
            private bool _returnValue;

            public LaunchActionThreadWithState(LaunchAction launchAction, string[] args)
            {
                _args = args;
                _launchAction = launchAction;
                _returnValue = false;
            }

            public bool ReturnValue
            {
                get { return _returnValue; }
            }

            public void ThreadProc()
            {
                _returnValue = _launchAction(_args, false);
            }
        }
    }
}
