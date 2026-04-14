// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    public class HockeyAppProxy
    {
        private const string HOCKEYAPP_BASE = "https://rink.hockeyapp.net";
        private const string HOCKEYAPP_APP_UPLOAD_CRASH_FORMAT_STRING = "{0}/api/2/apps/{1}/crashes";
        private const string HOCKEYAPP_APP_ID = "8dcc16e8fccd4284a28f7516a9074c16";
        private const string DATA_PACKAGE_NAME = "openlivewriter";
        private const string INFO_CONTACT = "olw@microsoft.com";
        private const string INFO_SDK = "OLW Custom SDK";
        private const string INFO_SDK_VERSION = "0.1";

        private Uri endpoint;
        private string osVersion;

        private static readonly Lazy<HockeyAppProxy> instance = new Lazy<HockeyAppProxy>(() => new HockeyAppProxy());

        public static HockeyAppProxy Current
        {
            get
            {
                return instance.Value;
            }
        }

        private HockeyAppProxy()
        {
            endpoint = new Uri(string.Format(HOCKEYAPP_APP_UPLOAD_CRASH_FORMAT_STRING,
                HOCKEYAPP_BASE,
                HOCKEYAPP_APP_ID));
            osVersion = Environment.OSVersion.ToString();
            if (string.IsNullOrEmpty(osVersion))
            {
                osVersion = "OS info unavailable";
            }
        }

        public string PackageName
        {
            get
            {
                return DATA_PACKAGE_NAME;
            }
        }
        public string Version
        {
            get
            {

                return ApplicationEnvironment.ProductVersion;
            }
        }
        public string OS
        {
            get
            {
                return osVersion;
            }
        }
        public DateTime Date
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public string Contact
        {
            get
            {
                return INFO_CONTACT;
            }
        }

        public string SDK
        {
            get
            {
                return INFO_SDK;
            }
        }

        public string SDKVersion
        {
            get
            {
                return INFO_SDK_VERSION;
            }
        }

        public async void UploadExceptionAsync(Exception exception)
        {
            var crashData = CreateCrashData(exception);
            await SendDataAsync(crashData);
        }

        private string CreateCrashData(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Package: {0}", this.PackageName).AppendLine();
            builder.AppendFormat("Version: {0}", this.Version).AppendLine();
            builder.AppendFormat("OS: {0}", this.OS).AppendLine();
            builder.AppendFormat("Date: {0}", this.Date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)).AppendLine();
            builder.AppendLine();
            builder.Append(StackTraceToString(exception));
            return builder.ToString();
        }

        private string StackTraceToString(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(exception.GetType().ToString() + ": ");
            builder.Append(string.IsNullOrEmpty(exception.Message) ? "No reason" : exception.Message);
            builder.AppendLine();
            builder.AppendLine(string.IsNullOrEmpty(exception.StackTrace) ? " at unknown location" : exception.StackTrace);
            return builder.ToString();
        }

        private Task SendDataAsync(string crashData, string description = null)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("raw={0}", Uri.EscapeDataString(crashData));
            if (this.Contact != null)
            {
                builder.AppendFormat("&contact={0}", Uri.EscapeDataString(this.Contact));
            }
            if (description != null)
            {
                builder.AppendFormat("&description={0}", Uri.EscapeDataString(description));
            }

            builder.AppendFormat("&sdk={0}", Uri.EscapeDataString(this.SDK));
            builder.AppendFormat("&sdk_version={0}", Uri.EscapeDataString(this.SDKVersion));

            string rawData = builder.ToString();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint.ToString());
                request.Content = new StringContent(rawData, Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Headers.TryAddWithoutValidation("User-Agent", ApplicationEnvironment.UserAgent);

                using var response = HttpClientService.DefaultClient.SendAsync(request).GetAwaiter().GetResult();
                // Just ensure the request completes, don't require success
            }
            catch (HttpRequestException)
            {
                // Silently ignore failures to send crash reports
            }

            return Task.FromResult(true);
        }
    }
}
