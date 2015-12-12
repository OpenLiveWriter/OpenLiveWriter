// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Shared static utility methods for web-publishing
    /// </summary>
    public sealed class WebPublishUtils
    {

        /// <summary>
        /// Create the appropriate file destination for the specified settings profile and initial path
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="initialPath"></param>
        /// <returns></returns>
        public static FileDestination CreateFileDestination(WebPublishSettings settings, string initialPath)
        {
            DestinationProfile destProfile = settings.Destination.Profile;
            if (destProfile.Type == DestinationProfile.DestType.WINDOWS)
            {
                return new LocalFileSystemDestination(initialPath);
            }
            else
            {
                return new WinInetFTPFileDestination(destProfile.FtpServer, initialPath, destProfile.UserName, destProfile.Password);
            }
        }

        /// <summary>
        /// Creates a destination that points to the destination's root folder.
        /// </summary>
        /// <returns></returns>
        public static FileDestination CreateRootDestination(WebPublishSettings settings)
        {
            FileDestination dest;
            if (settings.Destination.Profile.Type == DestinationProfile.DestType.FTP)
            {
                if (settings.PublishRootPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    dest = CreateFileDestination(settings, "/");
                else
                    dest = CreateFileDestination(settings, "");
            }
            else
                dest = CreateFileDestination(settings, "");
            return dest;
        }

        /// <summary>
        /// Translate an exception into an error message
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static WebPublishMessage ExceptionToErrorMessage(Exception e)
        {
            // parse out extended error info for use in error message construction
            string message = e.Message;
            if (e is SiteDestinationException && (e as SiteDestinationException).DestinationExtendedMessage != null)
            {
                message = (e as SiteDestinationException).DestinationExtendedMessage;
            }
            else if (e.InnerException != null)
            {
                message = e.InnerException.Message;
            }
            else
            {
                message = e.Message;
            }

            // trace for diagnostics
            Trace.WriteLine(e.ToString());

            // re-throw the exception so we can do normal processing
            try
            {
                throw e;
            }
            catch (LoginException)
            {
                return new WebPublishMessage(MessageId.LoginFailed);
            }
            catch (NoSuchDirectoryException ex)
            {
                WebPublishMessage msg = new WebPublishMessage(MessageId.NoSuchPublishFolder, ex.Path);
                return msg;
            }
            catch (SiteDestinationException ex)
            {
                if (ex.DestinationErrorCode == ERROR_INTERNET.NAME_NOT_RESOLVED)
                {
                    return new WebPublishMessage(MessageId.InvalidHostname);
                }
                else if (ex.DestinationErrorCode == ERROR_INTERNET.CANNOT_CONNECT)
                {
                    return new WebPublishMessage(MessageId.FtpServerUnavailable);
                }
                else if (ex.DestinationErrorCode == ERROR_INTERNET.TIMEOUT)
                {
                    return new WebPublishMessage(MessageId.ConnectionTimeout);
                }
                else
                {
                    return new WebPublishMessage(MessageId.PublishFailed, message);
                }
            }
            catch (Exception)
            {
                return new WebPublishMessage(MessageId.PublishFailed, message);
            }
        }
    }
}
