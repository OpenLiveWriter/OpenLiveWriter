// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.ContentSources
{

    internal class UrlContentRetreivalWithProgress
    {
        public static bool ExecuteSimpleContentRetreival(
            IWin32Window dialogOwner, ContentSourceInfo contentSourceInfo, string url, ref string title, ref string newContent)
        {
            try
            {
                // if there is progress requested then just do it on the main UI thread
                if (contentSourceInfo.UrlContentSourceRequiresProgress)
                {
                    // create the progress dialog and the async operation
                    UrlContentRetreivalWithProgressDialog progressDialog = new UrlContentRetreivalWithProgressDialog(contentSourceInfo);
                    progressDialog.CreateControl();
                    SimpleUrlContentRetreivalAsyncOperation asyncOperation = new SimpleUrlContentRetreivalAsyncOperation(progressDialog, contentSourceInfo.Instance as ContentSource, url, title);

                    // execute and retreive results
                    if (ExecuteWithProgress(dialogOwner, progressDialog, asyncOperation, contentSourceInfo))
                    {
                        title = asyncOperation.Title;
                        newContent = asyncOperation.NewContent;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        (contentSourceInfo.Instance as ContentSource).CreateContentFromUrl(url, ref title, ref newContent);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ContentSourceManager.DisplayContentRetreivalError(dialogOwner, ex, contentSourceInfo);
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception executing network operation for content source: " + ex.ToString());
                return false;
            }
        }

        public static bool ExecuteSmartContentRetreival(
            IWin32Window dialogOwner, ContentSourceInfo contentSourceInfo, string url, ref string title, ISmartContent newContent)
        {
            try
            {
                if (contentSourceInfo.UrlContentSourceRequiresProgress)
                {
                    // create the progress dialog and the async operation
                    UrlContentRetreivalWithProgressDialog progressDialog = new UrlContentRetreivalWithProgressDialog(contentSourceInfo);
                    progressDialog.CreateControl();

                    SmartUrlContentRetreivalAsyncOperation asyncOperation = new SmartUrlContentRetreivalAsyncOperation(progressDialog, contentSourceInfo.Instance as SmartContentSource, url, title, newContent);

                    // execute and retreive results
                    if (ExecuteWithProgress(dialogOwner, progressDialog, asyncOperation, contentSourceInfo))
                    {
                        title = asyncOperation.Title;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        (contentSourceInfo.Instance as SmartContentSource).CreateContentFromUrl(url, ref title, newContent);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ContentSourceManager.DisplayContentRetreivalError(dialogOwner, ex, contentSourceInfo);
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception executing network operation for content source: " + ex.ToString());
                return false;
            }
        }

        private static bool ExecuteWithProgress(
            IWin32Window dialogOwner,
            UrlContentRetreivalWithProgressDialog progressDialog,
            UrlContentRetreivalAsyncOperation asyncOperation,
            ContentSourceInfo contentSourceInfo)
        {
            try
            {
                // show the progress dialog
                using (progressDialog)
                {
                    asyncOperation.Start();
                    progressDialog.ShowProgress(dialogOwner, asyncOperation);
                }

                //  handle the result
                if (asyncOperation.Error != null)
                {
                    ContentSourceManager.DisplayContentRetreivalError(dialogOwner, asyncOperation.Error, contentSourceInfo);
                    return false;
                }
                else if (asyncOperation.WasCancelled)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception executing network operation for content source: " + ex.ToString());
                return false;
            }
        }

    }

    internal abstract class UrlContentRetreivalAsyncOperation : OpenLiveWriter.CoreServices.AsyncOperation
    {
        public UrlContentRetreivalAsyncOperation(
            ISynchronizeInvoke invokeTarget, WriterPlugin contentSource, string url, string title)
            : base(invokeTarget)
        {
            _url = url;
            _title = title;
            _contentSource = contentSource;
        }

        public bool WasCancelled
        {
            get { return CancelRequested; }
        }

        public Exception Error
        {
            get { return _contentRetrievalException; }
        }

        public string Title
        {
            get { return _title; }
        }

        protected string Url
        {
            get { return _url; }
        }

        protected WriterPlugin ContentSource
        {
            get { return _contentSource; }
        }

        protected abstract void RetreiveContent(ref string title);

        protected override void DoWork()
        {
            try
            {
                RetreiveContent(ref _title);
            }
            catch (OperationCancelledException)
            {
                // WasCancelled = true
            }
            catch (Exception ex)
            {
                _contentRetrievalException = ex;
            }
        }

        private string _url;
        private string _title;
        private WriterPlugin _contentSource;
        private Exception _contentRetrievalException;
    }

    internal class SimpleUrlContentRetreivalAsyncOperation : UrlContentRetreivalAsyncOperation
    {
        public SimpleUrlContentRetreivalAsyncOperation(
            ISynchronizeInvoke invokeTarget, ContentSource contentSource, string url, string title)
            : base(invokeTarget, contentSource, url, title)
        {
        }

        public string NewContent
        {
            get { return _newContent; }
        }

        protected override void RetreiveContent(ref string title)
        {
            (ContentSource as ContentSource).CreateContentFromUrl(Url, ref title, ref _newContent);
        }

        private string _newContent;
    }

    internal class SmartUrlContentRetreivalAsyncOperation : UrlContentRetreivalAsyncOperation
    {
        public SmartUrlContentRetreivalAsyncOperation(
            ISynchronizeInvoke invokeTarget, SmartContentSource contentSource, string url, string title, ISmartContent newContent)
            : base(invokeTarget, contentSource, url, title)
        {
            _newContent = newContent;
        }

        protected override void RetreiveContent(ref string title)
        {
            (ContentSource as SmartContentSource).CreateContentFromUrl(Url, ref title, _newContent);
        }

        private ISmartContent _newContent;
    }
}

