// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Diagnostics;
using System.Threading;

namespace BlogRunnerReporter
{
    static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                string input = Path.GetFullPath(args[0]);
                string output = Path.GetFullPath(args[1]);
                string errors = Path.GetFullPath(args[2]);
                string report = Path.GetFullPath(args[3]);

                CheckPathExists("input", input);
                CheckPathExists("output", output);
                CheckPathExists("errors", errors);

                bool hasErrors = new FileInfo(errors).Length > 0;
                bool filesDiffer = !Compare(input, output);

                if (!hasErrors && !filesDiffer)
                    return 0;

                if (filesDiffer)
                {
                    string diffCommand = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        @"Beyond Compare 2\bc2.exe");

                    if (!File.Exists(diffCommand))
                    {
                        Console.Error.WriteLine("Warning: Beyond Compare 2 is not installed; diff report generation will be skipped");
                    }
                    else
                    {
                        string diffCommandArgs = string.Format(
                            CultureInfo.InvariantCulture,
                            @"/silent @bcscript.txt ""{0}"" ""{1}"" ""{2}""",
                            input,
                            output,
                            report);

                        Process p = Process.Start(diffCommand, diffCommandArgs);
                        p.WaitForExit();
                        Thread.Sleep(3000);
                    }
                }

                string notificationType =
                    (hasErrors && filesDiffer) ? "changes and errors" :
                    hasErrors ? "errors" :
                    "changes";

                using (Stream outputStream = File.OpenRead(output))
                {
                    using (Stream reportStream = File.Exists(report) ? File.OpenRead(report) : null)
                    {
                        using (Stream errorsStream = hasErrors ? File.OpenRead(errors) : null)
                        {
                            MailMessage msg = new MailMessage("wlwbuild@microsoft.com", "wlwbuild@microsoft.com");
                            if (filesDiffer)
                            {
                                msg.Attachments.Add(new Attachment(outputStream, "BlogProvidersB5.xml", "text/xml"));
                                if (reportStream != null)
                                    msg.Attachments.Add(new Attachment(reportStream, "diff.htm", "text/html"));
                            }
                            if (errorsStream != null)
                                msg.Attachments.Add(new Attachment(errorsStream, "errors.txt", "text/plain"));

                            msg.Subject = "Blog provider " + notificationType + " detected";
                            msg.Body = notificationType + " detected while running blog provider tests. Please see attached.";

                            SmtpClient client = new SmtpClient();
                            client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
                            client.Send(msg);
                        }
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return 2;
            }
        }

        private static bool Compare(string file1, string file2)
        {
            FileInfo f1 = new FileInfo(file1);
            FileInfo f2 = new FileInfo(file2);

            if (f1.Length != f2.Length)
                return false;

            using (Stream s1 = f1.OpenRead())
            using (Stream s2 = f2.OpenRead())
            {
                for (int i = 0; i < s1.Length; i++)
                    if (s1.ReadByte() != s2.ReadByte())
                        return false;
            }

            return true;
        }

        private static void CheckPathExists(string label, string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(label + " file does not exist: " + path);
        }
    }
}
