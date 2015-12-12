// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Net;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Summary description for DestinationValidator.
    /// </summary>
    public class DestinationValidator : IDisposable
    {
        private FileDestination _destination;

        public DestinationValidator(FileDestination destination)
        {
            _destination = destination;
        }

        public void Validate(string urlMapping)
        {
            string testFilename = Guid.NewGuid().ToString() + ".txt";
            string fromPath = TempFileManager.Instance.CreateTempFile(testFilename);

            try
            {
                _destination.Connect();
                try
                {
                    if (urlMapping != null)
                        _destination.DoTransfer(fromPath, testFilename, true);
                }
                catch (SiteDestinationException siteException)
                {
                    throw new DestinationServerFailedException(siteException);
                }
            }
            catch (LoginException)
            {
                throw new DestinationLoginFailedException();
            }
            catch (SiteDestinationException siteException)
            {
                throw new DestinationServerFailedException(siteException);
            }

            if (urlMapping != null)
            {
                try
                {
                    try
                    {
                        if (!urlMapping.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                            urlMapping = urlMapping + "/";
                        HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest(urlMapping + testFilename, true);
                        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                        resp.Close();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("destination HTTP verification failed: " + e.Message);
                        throw new DestinationUrlMappingFailedException();
                    }
                }
                finally
                {
                    try
                    {
                        //delete the temp file from the remote server.
                        _destination.DeleteFile(testFilename);
                    }
                    catch (Exception e)
                    {
                        Trace.Fail("destination FTP verification warning: unable to delete test file: " + e.Message);
                    }
                }
            }
        }

        public void Dispose()
        {
            _destination.Disconnect();
        }

        public class DestinationValidationException : Exception
        {
        }

        public class DestinationServerFailedException : DestinationValidationException
        {
            public DestinationServerFailedException(SiteDestinationException siteException) : base()
            {
                _siteException = siteException;
            }
            private SiteDestinationException _siteException;

            public SiteDestinationException SiteException
            {
                get { return _siteException; }
                set { _siteException = value; }
            }

        }

        public class DestinationUrlMappingFailedException : DestinationValidationException
        {
        }

        public class DestinationLoginFailedException : DestinationValidationException
        {

        }
    }

}
