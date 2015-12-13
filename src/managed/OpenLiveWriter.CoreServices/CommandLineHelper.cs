// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.CoreServices
{
#if FALSE // imprve code coverage number
    ///<summary>
    /// Summary description for CommandLineHelper.
    /// </summary>
    public class CommandLineHelper
    {
        public static string DropFirstArg(string cmdLine, out string firstArg)
        {
            char[] ws = {' ', '\t'};

            cmdLine = cmdLine.TrimStart(ws);

            const int STATE_BEGIN = 0;
            const int STATE_INARG = 1;
            const int STATE_INQARG = 2;

            int state = STATE_BEGIN;

            for (int i = 0; i < cmdLine.Length; i++)
            {
                char c = cmdLine[i];
                switch (state)
                {
                    case STATE_BEGIN:
                    switch (c)
                    {
                        case '"':
                            state = STATE_INQARG;
                            break;
                        case ' ':
                        case '\t':
                            break;
                        default:
                            state = STATE_INARG;
                            break;
                    }
                        break;
                    case STATE_INARG:
                    switch (c)
                    {
                        case '"':
                            state = STATE_INQARG;
                            break;
                        case ' ':
                        case '\t':
                            firstArg = cmdLine.Substring(0, i);
                            return cmdLine.Substring(i+1).TrimStart(ws);
                    }
                        break;
                    case STATE_INQARG:
                    switch (c)
                    {
                        case '"':
                            firstArg = cmdLine.Substring(0, i+1);
                            return cmdLine.Substring(i+1).TrimStart(ws);
                    }
                        break;
                }
            }
            firstArg = cmdLine;
            return "";
        }
    }
#endif
}
