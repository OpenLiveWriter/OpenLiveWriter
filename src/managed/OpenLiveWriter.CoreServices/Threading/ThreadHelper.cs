// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace OpenLiveWriter.CoreServices.Threading
{
    public class ThreadHelper
    {
        [ThreadStatic]
        private static bool cancelled;

        /// <param name="ts">The ThreadStart.</param>
        /// <param name="name">The name of the thread, for debugging purposes.</param>
        /// <param name="inheritCulture">True if the new thread should inherit the UI culture of the calling thread. Should usually be true!</param>
        /// <param name="singleThreadedApartment">True if the thread should be STA.</param>
        /// <param name="background">True if the thread should not stop the process from exiting.</param>
        public static Thread NewThread(ThreadStart ts, string name, bool inheritCulture, bool singleThreadedApartment, bool background)
        {
            Thread t = new Thread(ts);
            NewThreadCore(t, name, inheritCulture, singleThreadedApartment, background);
            return t;
        }

        public static Thread NewThread(ParameterizedThreadStart ts, string name, bool inheritCulture, bool singleThreadedApartment, bool background)
        {
            Thread t = new Thread(ts);
            NewThreadCore(t, name, inheritCulture, singleThreadedApartment, background);
            return t;
        }

        private static void NewThreadCore(Thread t, string name, bool inheritCulture, bool singleThreadedApartment, bool background)
        {
            if (name != null)
                t.Name = name;

            if (inheritCulture)
            {
                t.CurrentCulture = CultureInfo.CurrentCulture;
                t.CurrentUICulture = CultureInfo.CurrentUICulture;
            }

            if (singleThreadedApartment)
                t.SetApartmentState(ApartmentState.STA);

            t.IsBackground = background;
        }

        /// <summary>
        /// Throw ThreadInterruptedException if interrupted.
        /// </summary>
        public static void CheckInterrupted()
        {
            //Debug.WriteLine("ThreadInterrupted?");

            if (cancelled)
                throw new ThreadInterruptedException();

            try
            {
                Thread.Sleep(0);
            }
            catch (ThreadInterruptedException)
            {
                Debug.WriteLine("ThreadInterrupted!");

                // persists interrupt.  normally the interrupt bit gets cleared after TIE is thrown.
                cancelled = true;
                throw;
            }
        }

        public static bool Interrupted
        {
            get
            {
                if (cancelled)
                    return true;

                try
                {
                    CheckInterrupted();
                    return false;
                }
                catch (ThreadInterruptedException)
                {
                    return true;
                }
            }
        }
    }
}
