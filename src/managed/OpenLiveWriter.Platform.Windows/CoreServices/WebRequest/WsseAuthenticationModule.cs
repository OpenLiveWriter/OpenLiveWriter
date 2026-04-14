// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public class WsseAuthenticationModule : IAuthenticationModule
    {
        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            if (!challenge.StartsWith(AuthenticationType + " ", StringComparison.OrdinalIgnoreCase))
                return null;
            return PreAuthenticate(request, credentials);
        }

        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            NetworkCredential credential = credentials.GetCredential(request.RequestUri, AuthenticationType);
            string username = credential.UserName;
            string password = credential.Password;

            string created, nonce;
            string passwordDigest = GeneratePasswordDigest(password, out created, out nonce);

            request.Headers.Add("X-WSSE", string.Format(
                                              CultureInfo.InvariantCulture,
                                              "UsernameToken Username=\"{0}\", PasswordDigest=\"{1}\", Created=\"{2}\", Nonce=\"{3}\"",
                                              username,
                                              passwordDigest,
                                              created,
                                              nonce));

            return new Authorization("WSSE profile=\"UsernameToken\"", true);
        }

        public bool CanPreAuthenticate
        {
            get { return true; }
        }

        public string AuthenticationType
        {
            get { return "WSSE"; }
        }

        private static string GeneratePasswordDigest(string password, out string created, out string nonce)
        {
            byte[] nonceBytes = new byte[30];
            RandomNumberGenerator.Fill(nonceBytes);
            nonce = Convert.ToBase64String(nonceBytes);
            created = DateTimeHelper.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

            byte[] hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(nonce + created + password));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
