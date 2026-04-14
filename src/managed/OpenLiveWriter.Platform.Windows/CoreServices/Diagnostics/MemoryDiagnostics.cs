// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// MemoryDiagnostics class.
    /// </summary>
    public class MemoryDiagnostics
    {
        private class RegisteredObject
        {
            private WeakReference weakReference;
            private StackTrace stackTrace;

            /// <summary>
            /// Initializes a new instance of the RegisteredObject class.
            /// </summary>
            /// <param name="value">The object being registered.</param>
            public RegisteredObject(object value)
            {
                Debug.WriteLine("Registering object "+value.GetType());
                weakReference = new WeakReference(value, false);
                stackTrace = new StackTrace(2, true);
            }

            public bool IsAlive
            {
                get
                {
                    return weakReference.IsAlive;
                }
            }

            public object Value
            {
                get
                {
                    return weakReference.Target;
                }
            }

            public StackTrace StackTrace
            {
                get
                {
                    return stackTrace;
                }
            }

            public override string ToString()
            {
                if (IsAlive)
                    return weakReference.Target.ToString();
                else
                    return base.ToString ();
            }

        }

        /// <summary>
        ///	The bitmap cache.
        /// </summary>
        [ThreadStatic]
        private static ArrayList objectList;

        /// <summary>
        /// Initializes a new instance of the MemoryDiagnostics class.
        /// </summary>
        private MemoryDiagnostics()
        {
        }

        public static void RegisterObject(object value)
        {
#if true
            if (objectList == null)
            {
                lock(typeof(MemoryDiagnostics))
                {
                    if (objectList == null)
                        objectList = new ArrayList();
                }
            }

            lock(objectList)
                objectList.Add(new RegisteredObject(value));
#endif
        }

        public static void GenerateLeakReport()
        {
            if (objectList == null)
                return;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            //	Get a stream writer on the file.
            bool firstLeak = true;
            string leaksFile = String.Format(CultureInfo.InvariantCulture, "c:\\{0} Leaks.log", ApplicationEnvironment.ProductName);
            if (File.Exists(leaksFile))
                File.Delete(leaksFile);
            using (StreamWriter streamWriter = File.AppendText(leaksFile))
            {
                foreach (RegisteredObject registeredObject in objectList)
                {
                    if (registeredObject.IsAlive)
                    {
                        if (firstLeak)
                        {
                            streamWriter.WriteLine("Memory Leak Report");
                            firstLeak = false;
                        }

                        streamWriter.WriteLine("Leak Detected");
                        streamWriter.WriteLine("Type:  "+registeredObject.Value.GetType().ToString());
                        streamWriter.WriteLine("Value: "+registeredObject.Value.ToString());
                        streamWriter.WriteLine("Stack Trace:");
                        streamWriter.WriteLine(registeredObject.StackTrace.ToString());
                    }
                }
                if (!firstLeak)
                    streamWriter.WriteLine("End Memory Leak Report");
            }
            if (!firstLeak)
                Process.Start(leaksFile);
        }
    }
}
