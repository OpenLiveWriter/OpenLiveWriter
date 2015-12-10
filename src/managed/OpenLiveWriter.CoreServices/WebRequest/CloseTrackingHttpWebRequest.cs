// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using OpenLiveWriter.CoreServices.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// If you use this to wrap an HttpWebRequest, you'll get asserts
    /// if you get the response but forget to close it (or dispose the
    /// response stream--you just need to do one or the other).
    /// </summary>
    internal class CloseTrackingHttpWebRequest : GenericProxy
    {
        [Conditional("DEBUG")]
        public static void Wrap(ref HttpWebRequest request)
        {
            request = (HttpWebRequest)new CloseTrackingHttpWebRequest(request).GetTransparentProxy();
        }

        private CloseTrackingHttpWebRequest(HttpWebRequest wrapped) : base(wrapped)
        {
        }

        protected override void OnAfterInvoke(MethodInfo method, object wrapped, object[] args, ref object outVal)
        {
            if (method.Name == "GetResponse")
            {
                outVal = new CloseTrackingHttpWebResponse((HttpWebResponse)outVal).GetTransparentProxy();
            }
        }

        #region response
        private class CloseTrackingHttpWebResponse : GenericProxy
        {
            private IntPtr pStackTrace;

            public CloseTrackingHttpWebResponse(HttpWebResponse wrapped) : base(wrapped)
            {
                pStackTrace = Marshal.StringToCoTaskMemUni(Environment.StackTrace.ToString());
            }

            ~CloseTrackingHttpWebResponse()
            {
                string stackTrace = pStackTrace == IntPtr.Zero ? "NULL" : Marshal.PtrToStringUni(pStackTrace);
                FreeStackTrace();

                if (!ApplicationDiagnostics.AutomationMode)
                    Debug.Fail("Unclosed response\r\n" + stackTrace);
                else
                    Debug.WriteLine("Unclosed response\r\n" + stackTrace);
            }

            private void FreeStackTrace()
            {
                IntPtr pTemp = pStackTrace;
                pStackTrace = IntPtr.Zero;
                Marshal.FreeCoTaskMem(pTemp);
            }

            protected override void OnAfterInvoke(MethodInfo method, object wrapped, object[] args, ref object outVal)
            {
                switch (method.Name)
                {
                    case "Close":
                        OnClosed();
                        break;
                    case "GetResponseStream":
                        CloseTrackingStream closeTrackingStream = new CloseTrackingStream(outVal);
                        closeTrackingStream.Disposed += new EventHandler(diagStream_Disposed);
                        outVal = closeTrackingStream.GetTransparentProxy();
                        break;
                }
            }

            private void diagStream_Disposed(object sender, EventArgs e)
            {
                OnClosed();
            }

            private void OnClosed()
            {
                FreeStackTrace();
                GC.SuppressFinalize(this);
            }
        }
        #endregion

        #region stream
        public class CloseTrackingStream : GenericProxy
        {
            public CloseTrackingStream(object wrapped) : base(wrapped)
            {
            }

            public event EventHandler Disposed;

            protected override void OnBeforeInvoke(MethodInfo method, object wrapped, object[] args)
            {
            }

            protected override void OnAfterInvoke(MethodInfo method, object wrapped, object[] args, ref object outVal)
            {
                switch (method.Name)
                {
                    case "Close":
                    case "Dispose":
                        if (Disposed != null)
                            Disposed(this, EventArgs.Empty);
                        break;
                }
            }
        }
        #endregion
    }

    internal abstract class GenericProxy : RealProxy
    {
        private object _wrapped;

        public GenericProxy(object wrapped) : base(wrapped.GetType())
        {
            _wrapped = wrapped;
        }

        public override IMessage Invoke(IMessage msg)
        {
            MethodCallMessageWrapper mc = new MethodCallMessageWrapper((IMethodCallMessage)msg);

            MethodInfo mi = (MethodInfo)mc.MethodBase;

            try
            {
                OnBeforeInvoke(mi, _wrapped, mc.Args);
                object outVal = mi.Invoke(_wrapped, mc.Args);
                OnAfterInvoke(mi, _wrapped, mc.Args, ref outVal);
                return new ReturnMessage(outVal, mc.Args, mc.Args.Length, mc.LogicalCallContext, mc);
            }
            catch (TargetInvocationException e)
            {
                return new ReturnMessage(e.InnerException, (IMethodCallMessage)msg);
            }
            catch (Exception e)
            {
                return new ReturnMessage(e, (IMethodCallMessage)msg);
            }
        }

        protected virtual void OnBeforeInvoke(MethodInfo method, object wrapped, object[] args) { }
        protected virtual void OnAfterInvoke(MethodInfo method, object wrapped, object[] args, ref object outVal) { }
    }
}
