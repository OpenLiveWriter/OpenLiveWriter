// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// Summary description for DynamicExceptionMessageRegistry.
    /// </summary>
    public class DynamicExceptionMessageRegistry
    {
        #region Singleton

        private static readonly DynamicExceptionMessageRegistry _singleton = new DynamicExceptionMessageRegistry();

        public static DynamicExceptionMessageRegistry Instance
        {
            get { return _singleton; }
        }

        #endregion

        private ArrayList handlers = ArrayList.Synchronized(new ArrayList());

        public void AddMessage(IDynamicExceptionMessage message)
        {
            handlers.Add(message);
        }

        /// <summary>
        /// Returns true if matched.  If no match, out param "message"
        /// will be null.  If match, out param "message" will be either
        /// the message that should be shown, or null if no message
        /// should be shown to the user.
        /// </summary>
        internal bool GetMessage(out ExceptionMessage message, Exception e)
        {
            Type t = e.GetType();
            for (int i = 0; i < handlers.Count; i++)
            {
                IDynamicExceptionMessage msg = (IDynamicExceptionMessage)handlers[i];
                if (msg.AppliesTo(t, e))
                {
                    message = msg.GetMessage(e);
                    return true;
                }
            }

            message = null;
            return false;
        }

        static DynamicExceptionMessageRegistry()
        {
            DynamicExceptionMessageRegistry.Instance.AddMessage(
                new COMErrorCodeDynamicExceptionMessage(unchecked((int)0x800704C7), null) // operation cancelled
                );
        }
    }
}
