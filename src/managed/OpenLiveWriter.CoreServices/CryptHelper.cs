// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

namespace OpenLiveWriter.CoreServices
{
    public class CryptHelper
    {
        public static byte[] Encrypt(string str)
        {
            if (ApplicationEnvironment.IsPortableMode)
            {
                // TODO: Use real encryption!
#if DEBUG
                return Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.Unicode.GetBytes(str)));
#else
                throw new NotImplementedException();
#endif
            }
            else
            {
                byte[] bytes = null;
                byte[] bytesPlusNull = null;
                try
                {
                    bytes = new UnicodeEncoding(false, false).GetBytes(str);
                    bytesPlusNull = new byte[bytes.Length + 2];
                    Array.Copy(bytes, bytesPlusNull, bytes.Length);
                    byte[] encrypted = ProtectedData.Protect(bytesPlusNull, null, DataProtectionScope.CurrentUser);
                    return encrypted;
                }
                finally
                {
                    ZeroFill(bytes);
                    ZeroFill(bytesPlusNull);
                }
            }
        }

        public static string Decrypt(byte[] val)
        {
            if (ApplicationEnvironment.IsPortableMode)
            {
#if DEBUG
                return Encoding.Unicode.GetString(Convert.FromBase64String(Encoding.ASCII.GetString(val)));
#else
                throw new NotImplementedException();
#endif
            }
            else
            {
                byte[] clearBytes = null;
                try
                {
                    clearBytes = ProtectedData.Unprotect(val, null, DataProtectionScope.CurrentUser);
                    if (clearBytes.Length < 2
                        || (clearBytes.Length % 2) != 0
                        || clearBytes[clearBytes.Length - 1] != 0
                        || clearBytes[clearBytes.Length - 2] != 0)
                    {
                        throw new ArgumentException("Value could not be decrypted");
                    }

                    return Encoding.Unicode.GetString(clearBytes, 0, clearBytes.Length - 2);
                }
                finally
                {
                    ZeroFill(clearBytes);
                }
            }
        }

        private static void ZeroFill(byte[] buffer)
        {
            if (buffer == null)
                return;
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0;
        }
    }
}
