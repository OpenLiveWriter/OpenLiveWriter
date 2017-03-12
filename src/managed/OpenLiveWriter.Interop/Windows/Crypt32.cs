// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Crypto API Interop
    /// </summary>
    public sealed class Crypt32
    {

        /// <summary>
        /// Encrypt a string into a byte array using the Windows Cryto API. This
        /// method provides a high-level wrapper for the use of CryptProtectData
        /// </summary>
        /// <param name="str">string to encrypt</param>
        /// <param name="description">categorical description of item being encrypted</param>
        /// <param name="dwFlags">flags (see API docs for CryptProtectData)</param>
        /// <param name="promptText">text to prompt the user with when the data is being encrypted (null for no prompt)</param>
        /// <returns>byte array containing encrtyped version of string</returns>
        [Obsolete("Use CryptHelper")]
        unsafe public static byte[] CryptProtectString(
            string str, string description, uint dwFlags, string promptText)
        {
            // output buffer
            DATA_BLOB dataOut = new DATA_BLOB();

            try
            {
                // prompt struct
                CRYPTPROTECT_PROMPTSTRUCT promptStruct = new CRYPTPROTECT_PROMPTSTRUCT();
                promptStruct.cbSize = (uint)Marshal.SizeOf(promptStruct);
                if (promptText != null)
                {
                    promptStruct.dwPromptFlags = CRYPTPROTECT.PROMPT_ON_PROTECT;
                    promptStruct.szPrompt = promptText;
                }

                // pin buffer for encrypt
                fixed (char* data = str)
                {
                    // input buffer
                    DATA_BLOB dataIn = new DATA_BLOB();
                    dataIn.cbData = (uint)((str.Length + 1) * 2);
                    dataIn.pbData = new IntPtr(data);

                    // attempt to encrypt the data
                    if (!CryptProtectData(
                        ref dataIn,         // data-input
                        description,    // description of data
                        IntPtr.Zero,        // no entropy data
                        IntPtr.Zero,        // reserved
                        ref promptStruct,   // prompt-info
                        dwFlags,            // flags
                        ref dataOut))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "Error encryting " + description);
                    }
                }

                // return the encrtyped data as a managed byte-array
                byte[] encryptedData = new byte[dataOut.cbData];
                Marshal.Copy(dataOut.pbData, encryptedData, 0, (int)dataOut.cbData);
                return encryptedData;
            }
            finally
            {
                if (dataOut.pbData != IntPtr.Zero)
                    Kernel32.LocalFree(dataOut.pbData);
            }
        }

        /// <summary>
        /// Decrypt a string previously encrtyped into a byte array with CryptProtectString.
        /// This method provides a high-level wrapper for the use of CryptUnprotectData.
        /// </summary>
        /// <param name="encryptedStr">byte array containing the encrtyped string</param>
        /// <param name="description">out parameter for the description of the encrtyped item</param>
        /// <param name="flags">flags (see API docs for CryptUnprotectData)</param>
        /// <param name="promptText">text to prompt user with (null for no prompt)</param>
        /// <returns>decrypted value of string</returns>
        [Obsolete("Use CryptHelper")]
        unsafe public static string CryptUnprotectString(
            byte[] encryptedStr, out string description, uint flags, string promptText)
        {
            // out variables
            IntPtr dataDescription = IntPtr.Zero;
            DATA_BLOB dataOut = new DATA_BLOB();

            try
            {
                // prompt struct
                CRYPTPROTECT_PROMPTSTRUCT promptStruct = new CRYPTPROTECT_PROMPTSTRUCT();
                promptStruct.cbSize = (uint)Marshal.SizeOf(promptStruct);
                if (promptText != null)
                {
                    promptStruct.dwPromptFlags = CRYPTPROTECT.PROMPT_ON_UNPROTECT;
                    promptStruct.szPrompt = promptText;
                }

                // pin buffer for decryption
                fixed (byte* data = encryptedStr)
                {
                    // encrypted data
                    DATA_BLOB encrtypedData = new DATA_BLOB();
                    encrtypedData.cbData = (uint)encryptedStr.Length;
                    encrtypedData.pbData = new IntPtr(data);

                    // unencrypt
                    if (!CryptUnprotectData(
                        ref encrtypedData,
                        out dataDescription,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        ref promptStruct,
                        flags,
                        ref dataOut))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "Error decrypting data");
                    }

                    // return the string
                    description = Marshal.PtrToStringUni(dataDescription);
                    return Marshal.PtrToStringUni(dataOut.pbData);
                }
            }
            finally
            {
                if (dataDescription != IntPtr.Zero)
                    Kernel32.LocalFree(dataDescription);

                if (dataOut.pbData != IntPtr.Zero)
                    Kernel32.LocalFree(dataOut.pbData);
            }
        }

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern bool CryptProtectData(
            ref DATA_BLOB pDataIn,
            [MarshalAs(UnmanagedType.LPWStr)] string szDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
            uint dwFlags,
            ref DATA_BLOB pDataOut
            );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern bool CryptUnprotectData(
            ref DATA_BLOB pDataIn,
            out IntPtr ppszDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
            uint dwFlags,
            ref DATA_BLOB pDataOut
            );

        // <summary>
        /// Unit test for cryto utility methods
        /// </summary>
        [Obsolete]
        public static void TestCrypt()
        {
            // description and data
            string description = "Test Encryption Data";
            string data = "TestData";

            // encrypt data
            byte[] encrtypedData = CryptProtectString(data, description, 0, null);

            // decrypt data
            string extractedDescription;
            string decryptedData = CryptUnprotectString(encrtypedData, out extractedDescription, 0, null);

            // verify results
            Debug.Assert(description == extractedDescription);
            Debug.Assert(data == decryptedData);
        }
    }

    /// <summary>
    /// Crypto constants
    /// </summary>
    public struct CRYPTPROTECT
    {
        // prompt on unprotect
        public const uint PROMPT_ON_UNPROTECT = 0x1;

        // prompt on protect
        public const uint PROMPT_ON_PROTECT = 0x2;

        // for remote-access situations where ui is not an option
        // if UI was specified on protect or unprotect operation, the call
        // will fail and GetLastError() will indicate ERROR_PASSWORD_RESTRICTION
        public const uint UI_FORBIDDEN = 0x1;

        // per machine protected data -- any user on machine where CryptProtectData
        // took place may CryptUnprotectData
        public const uint LOCAL_MACHINE = 0x4;

        // Generate an Audit on protect and unprotect operations
        public const uint AUDIT = 0x10;

        // Verify the protection of a protected blob
        public const uint VERIFY_PROTECTION = 0x40;
    }

    /// <summary>
    /// Cryto data-blob
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DATA_BLOB
    {
        public uint cbData;
        public IntPtr pbData;
    }

    /// <summary>
    /// Crypto prompt-struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPTPROTECT_PROMPTSTRUCT
    {
        public uint cbSize;
        public uint dwPromptFlags;
        public IntPtr hwndApp;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szPrompt;
    }

}
