// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Win32;

namespace OpenLiveWriter.CoreServices
{

    //usage:
    //Instrumentor.IncrementCounter(Instrumentor.COUNTERNAME);
    //handles creating the registry keys and opt in stuff, so you don't have to

    /// <summary>
    /// Summary description for Intrumentor.
    /// </summary>
    public class Instrumentor
    {
        public const string WRITER_OPENED = "wr";
        public const string LINKS = "lk";
        public const string IMAGES = "im";
        public const string MAPS = "ma";
        public const string PLUG_INS_COUNT = "pc";
        public const string PLUG_INS_LIST = "pl";
        public const string SOURCE_CODE_VIEW = "sv";
        public const string NO_STYLE_EDIT = "ns";
        public const string EDIT_OLD_POST = "ep";
        public const string DEFAULT_BLOG_PROVIDER = "db";

        private const string ALWAYS_SEND = "n";
        private const string OPT_IN_ONLY = "o";

        private readonly static string instrumentationReportKey;

        private static bool SearchLoggerWorks = false;

        static Instrumentor()
        {
            try
            {
                using (RegistryKey slVersionKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\MSN Apps\\SL", false))
                {
                    if (slVersionKey != null)
                    {
                        string version = (string)slVersionKey.GetValue("Version");
                        int minorVersionNum = Int32.Parse(version.Substring(version.LastIndexOf('.') + 1), CultureInfo.InvariantCulture);
                        int majorVersionNum = Int32.Parse(version.Substring(0,version.IndexOf('.')), CultureInfo.InvariantCulture);
                        if (majorVersionNum >= 3 && minorVersionNum >= 1458)
                        {
                            SearchLoggerWorks = true;
                        }
                    }
                }

                if (SearchLoggerWorks)
                {
                    string hkcu = string.Empty;
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        // Under Vista Protected Mode, this is the low-integrity root
                        hkcu = @"Software\AppDataLow\";
                    }
                    instrumentationReportKey = hkcu + @"Software\Microsoft\MSN Apps\Open Live Writer\SL";
                }
                else
                {
                    instrumentationReportKey = null;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception while writing instrumentation values to registry: " + ex.ToString());
            }
        }

        public static void IncrementCounter(string keyName)
        {
            string regKey = GetKeyName(keyName);

            //check that AL key exists--if not, add it
            //check that specific key exists--if not, add it
            //increment specific key
            try
            {
                if (SearchLoggerWorks)
                {
                    using ( RegistryKey reportingKey = Registry.CurrentUser.CreateSubKey(instrumentationReportKey) )
                    {
                        if ( reportingKey != null )
                        {
                            int currentVal = (int)reportingKey.GetValue(regKey, 0);
                            currentVal++;
                            reportingKey.SetValue(regKey, currentVal);
                        }
                        else
                        {
                            Debug.Fail("Unable to open Onfolio instrumentation reporting registry key") ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception while writing instrumentation values to registry: " + ex.ToString());
            }
        }

        public static void SetKeyValue(string keyName, object keyValue)
        {
            string regKey = GetKeyName(keyName);

            //check that AL key exists--if not, add it
            //check that specific key exists--if not, add it
            try
            {
                if (SearchLoggerWorks)
                {
                    using ( RegistryKey reportingKey = Registry.CurrentUser.CreateSubKey(instrumentationReportKey) )
                    {
                        if ( reportingKey != null )
                        {
                            reportingKey.SetValue(regKey, keyValue);
                        }
                        else
                        {
                            Debug.Fail("Unable to open Onfolio instrumentation reporting registry key") ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception while writing instrumentation values to registry: " + ex.ToString());
            }
        }

        public static void AddItemToList(string keyName, string item)
        {
            string regKey = GetKeyName(keyName);

            //check that AL key exists--if not, add it
            //check that specific key exists--if not, add it
            try
            {
                if (SearchLoggerWorks)
                {
                    using ( RegistryKey reportingKey = Registry.CurrentUser.CreateSubKey(instrumentationReportKey) )
                    {
                        if ( reportingKey != null )
                        {
                            string newVal = EscapeString(item);
                            String currentVal = (String)reportingKey.GetValue(regKey, "");
                            if (currentVal != "")
                            {
                                //add this item if it isn't in the list
                                if (currentVal.IndexOf(newVal) < 0)
                                {
                                    currentVal += "," + newVal;
                                }
                            }
                            else
                            {
                                //new list, set to item
                                currentVal = newVal;
                            }
                            reportingKey.SetValue(regKey, currentVal);
                        }
                        else
                        {
                            Debug.Fail("Unable to open Writer instrumentation reporting registry key") ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception while writing instrumentation values to registry: " + ex.ToString());
            }
        }

        private static string GetKeyName(string keyName)
        {
            string regKey = keyName;
            //the final letter of the key name determines whether it is opt-in only or not
            //right now everything is opt in only
            regKey += OPT_IN_ONLY;
            return regKey;
        }

        private static string EscapeString(string val)
        {
            if (val == null)
                return "";

            // optimize for common case
            if (val.IndexOf('\t') == -1 && val.IndexOf('\n') == -1 && val.IndexOf('\"') == -1)
                return "\"" + val + "\"";

            return "\"" + val.Replace("\"", "\"\"") + "\"";
        }
    }
}

