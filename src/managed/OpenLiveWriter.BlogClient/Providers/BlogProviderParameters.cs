// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text;

namespace OpenLiveWriter.BlogClient.Providers
{

    public class BlogProviderParameters
    {
        public static bool UrlContainsParameters(string serverUrl)
        {
            return ExtractParameterList(serverUrl).Length > 0;
        }

        public static string ExtractParameterList(string serverUrl)
        {
            // pick out the <param> values in the server url string
            StringBuilder parameters = new StringBuilder();
            int paramLoc = 0;
            while (paramLoc != -1)
            {
                paramLoc = serverUrl.IndexOf('<', paramLoc);
                if (paramLoc != -1)
                {
                    int endParamLoc = serverUrl.IndexOf('>', paramLoc);
                    if (endParamLoc != -1)
                    {
                        // space delmit
                        if (parameters.Length > 0) parameters.Append(" ");
                        parameters.Append(serverUrl, paramLoc, endParamLoc - paramLoc + 1);
                        paramLoc = endParamLoc;
                    }
                }
            }
            return parameters.ToString();
        }

    }
}
