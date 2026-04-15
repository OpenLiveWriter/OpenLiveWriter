// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace OpenLiveWriter.Platform.Mac
{
    public class MacCredentialStorage : ICredentialStorage
    {
        private const string SERVICE_NAME = "OpenLiveWriter";

        public void StoreCredential(string key, string username, string password)
        {
            // Delete existing first (security add fails if duplicate)
            DeleteCredential(key);

            RunSecurity($"add-generic-password -a \"{EscapeArg(username)}\" -s \"{EscapeArg(SERVICE_NAME + "." + key)}\" -w \"{EscapeArg(password)}\" -U");
        }

        public (string username, string password)? RetrieveCredential(string key)
        {
            string serviceName = SERVICE_NAME + "." + key;

            // Get password
            string password = RunSecurity($"find-generic-password -s \"{EscapeArg(serviceName)}\" -w");
            if (password == null)
                return null;

            // Get username (from account field)
            string output = RunSecurity($"find-generic-password -s \"{EscapeArg(serviceName)}\" -g");
            if (output == null)
                return null;

            string username = null;
            foreach (string line in output.Split('\n'))
            {
                if (line.Contains("\"acct\""))
                {
                    int start = line.IndexOf("=\"") + 2;
                    int end = line.LastIndexOf("\"");
                    if (start > 1 && end > start)
                        username = line.Substring(start, end - start);
                }
            }

            if (username == null)
                return null;

            return (username, password.Trim());
        }

        public void DeleteCredential(string key)
        {
            RunSecurity($"delete-generic-password -s \"{EscapeArg(SERVICE_NAME + "." + key)}\"");
        }

        public bool CredentialExists(string key)
        {
            string result = RunSecurity($"find-generic-password -s \"{EscapeArg(SERVICE_NAME + "." + key)}\"");
            return result != null;
        }

        private static string RunSecurity(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo("security", arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                        return null;

                    return string.IsNullOrEmpty(output) ? error : output;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string EscapeArg(string arg) => arg?.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
