// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Summary description for WinMm.
    /// </summary>
    public class WinMm
    {

        public static void PlaySoundFile(string filePath)
        {
            PlaySound(filePath, 0, SND.ALIAS|SND.NOWAIT|SND.FILENAME);
        }

        public static void PlaySystemSound(string soundName)
        {
            PlaySound(soundName, 0, SND.ASYNC|SND.NOWAIT|SND.ALIAS);
        }

        [DllImport("WinMm.dll")]
        public static extern void PlaySound(
                            string pszSound, uint hmod, uint fdwSound);

        /// <summary>
        /// Enumeration of internet states
        /// </summary>
        public struct SND
        {
            public const uint SYNC			= 0x0000;  /* play synchronously (default) */
            public const uint ASYNC			= 0x0001;  /* play asynchronously */
            public const uint NODEFAULT		= 0x0002;  /* silence (!default) if sound not found */
            public const uint MEMORY		= 0x0004;  /* pszSound points to a memory file */
            public const uint LOOP			= 0x0008;  /* loop the sound until next sndPlaySound */
            public const uint NOSTOP		= 0x0010;  /* don't stop any currently playing sound */

            public const uint NOWAIT		= 0x00002000; /* don't wait if the driver is busy */
            public const uint ALIAS			= 0x00010000; /* name is a registry alias */
            public const uint ALIAS_ID		= 0x00110000; /* alias is a predefined ID */
            public const uint FILENAME		= 0x00020000; /* name is file name */
            public const uint RESOURCE		= 0x00040004; /* name is resource name or atom */
            public const uint PURGE         = 0x0040;  /* purge non-static events for task */
            public const uint APPLICATION   = 0x0080;  /* look for application specific association */
        }
    }
}
