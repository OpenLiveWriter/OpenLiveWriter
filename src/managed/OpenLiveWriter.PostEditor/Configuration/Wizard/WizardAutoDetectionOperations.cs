// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Controls.Wizard;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor;
using System.Globalization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{

    internal interface IWizardAutoDetectionOperation
    {
        OpenLiveWriter.CoreServices.AsyncOperation CreateOperation(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, TemporaryBlogSettings temporarySettings);

        bool FatalErrorOccurred { get; }

        bool AuthenticationErrorOccurred { get; }

        void ShowFatalError(IWin32Window owner);

        bool WasCancelled { get; }

        void OperationCompleted();

        WizardStep ExcludedStepIfCompleted { get; }

        void ShowNonFatalErrors(IWin32Window owner);
    }

    internal class WizardWeblogAndSettingsAutoDetectionOperation : IWizardAutoDetectionOperation
    {
        public WizardWeblogAndSettingsAutoDetectionOperation(WizardStep editWithStyleStep)
        {
            _editWithStyleStep = editWithStyleStep;
        }

        public virtual OpenLiveWriter.CoreServices.AsyncOperation CreateOperation(IBlogClientUIContext uiContext, Control parentControl, TemporaryBlogSettings temporarySettings)
        {
            _temporarySettings = temporarySettings;

            // create and start the account detector
            _blogServiceDetector = new BlogServiceDetector(
                uiContext,
                parentControl,
                _temporarySettings,
                new BlogCredentialsAccessor(_temporarySettings.Id, _temporarySettings.Credentials));

            return _blogServiceDetector;
        }

        public bool FatalErrorOccurred
        {
            get
            {
                return _blogServiceDetector.ErrorOccurred;
            }
        }

        public void Cancel()
        {
            if (_blogServiceDetector == null)
                return;

            Debug.Assert(_blogServiceDetector != null && !_blogServiceDetector.IsDone);
            if (!_blogServiceDetector.IsDone)
                _blogServiceDetector.Cancel();
        }

        public bool IsDone
        {
            get
            {
                if (_blogServiceDetector == null)
                    return true;
                return _blogServiceDetector.IsDone;
            }
        }

        public bool AuthenticationErrorOccurred
        {
            get { return _blogServiceDetector.AuthenticationErrorOccurred; }
        }

        public void ShowFatalError(IWin32Window owner)
        {
            _blogServiceDetector.ShowLastError(owner);
        }

        public bool WasCancelled
        {
            get
            {
                return _blogServiceDetector.WasCancelled;
            }
        }

        public void OperationCompleted()
        {
            _temporarySettings.ClearProvider();

            // copy the detected settings
            _temporarySettings.SetProvider(_blogServiceDetector.ProviderId, _blogServiceDetector.ServiceName, _blogServiceDetector.PostApiUrl, _blogServiceDetector.ClientType);
            if (_temporarySettings.BlogName == String.Empty)
                _temporarySettings.BlogName = _blogServiceDetector.BlogName;
            if (string.IsNullOrEmpty(_temporarySettings.HomepageUrl))
                _temporarySettings.HomepageUrl = ((IBlogSettingsDetectionContext)_blogServiceDetector).HomepageUrl;
            _temporarySettings.HostBlogId = _blogServiceDetector.HostBlogId;
            _temporarySettings.HostBlogs = _blogServiceDetector.UsersBlogs;
            _temporarySettings.ManifestDownloadInfo = _blogServiceDetector.ManifestDownloadInfo;
            _temporarySettings.ClientType = _blogServiceDetector.ClientType;

            // values that the service detector attempts to retreive -- if the attempt fails
            // for any reason then don't update the value (i.e. a failed attempt to get the
            // list of categories should not be construed as "there are no categories")
            if (_blogServiceDetector.Categories != null)
                _temporarySettings.Categories = _blogServiceDetector.Categories;
            if (_blogServiceDetector.Keywords != null)
                _temporarySettings.Keywords = _blogServiceDetector.Keywords;
            if (_blogServiceDetector.FavIcon != null)
                _temporarySettings.FavIcon = _blogServiceDetector.FavIcon;
            if (_blogServiceDetector.Image != null)
                _temporarySettings.Image = _blogServiceDetector.Image;
            if (_blogServiceDetector.WatermarkImage != null)
                _temporarySettings.WatermarkImage = _blogServiceDetector.WatermarkImage;

            if (_blogServiceDetector.OptionOverrides != null)
                _temporarySettings.OptionOverrides = _blogServiceDetector.OptionOverrides;
            if (_blogServiceDetector.HomePageOverrides != null)
                _temporarySettings.HomePageOverrides = _blogServiceDetector.HomePageOverrides;

            if (_blogServiceDetector.ButtonDescriptions != null)
                _temporarySettings.ButtonDescriptions = _blogServiceDetector.ButtonDescriptions;

            if (_blogServiceDetector.AvailableImageEndpoints != null)
                _temporarySettings.AvailableImageEndpoints = _blogServiceDetector.AvailableImageEndpoints;

            if (_blogServiceDetector.BlogTemplateFiles != null)
                _temporarySettings.TemplateFiles = _blogServiceDetector.BlogTemplateFiles;

            if (_blogServiceDetector.PostBodyBackgroundColor != null)
                _temporarySettings.UpdatePostBodyBackgroundColor(_blogServiceDetector.PostBodyBackgroundColor.Value);
        }

        public WizardStep ExcludedStepIfCompleted
        {
            get
            {
                return _editWithStyleStep;
            }
        }

        public void ShowNonFatalErrors(IWin32Window owner)
        {
            // if a template error occurred then show it now
            if (_blogServiceDetector.TemplateDownloadFailed)
                DisplayMessage.Show(MessageId.TemplateDownloadFailed, owner);

        }

        protected BlogServiceDetectorBase _blogServiceDetector;
        protected TemporaryBlogSettings _temporarySettings;
        private WizardStep _editWithStyleStep;
    }

    /// <summary>
    /// Provides auto-detection support for SharePoint blogs.
    /// </summary>
    internal class WizardSharePointAutoDetectionOperation : WizardWeblogAndSettingsAutoDetectionOperation
    {
        public WizardSharePointAutoDetectionOperation(WizardStep editWithStyleStep) : base(editWithStyleStep)
        {
        }

        public override OpenLiveWriter.CoreServices.AsyncOperation CreateOperation(IBlogClientUIContext uiContext, Control parentConrol, TemporaryBlogSettings temporarySettings)
        {
            _temporarySettings = temporarySettings;

            // create and start the account detector
            _blogServiceDetector = new SharePointBlogDetector(
                uiContext,
                parentConrol,
                _temporarySettings.Id, _temporarySettings.HomepageUrl,
                new BlogCredentialsAccessor(_temporarySettings.Id, _temporarySettings.Credentials),
                _temporarySettings.Credentials);

            return _blogServiceDetector;
        }
    }

    internal class WizardSettingsAutoDetectionOperation : IWizardAutoDetectionOperation
    {
        private IBlogClientUIContext _uiContext;

        public WizardSettingsAutoDetectionOperation(WizardStep editWithStyleStep)
        {
            _editWithStyleStep = editWithStyleStep;
        }

        public OpenLiveWriter.CoreServices.AsyncOperation CreateOperation(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, TemporaryBlogSettings temporarySettings)
        {
            // save references
            _uiContext = uiContext;
            _temporarySettings = temporarySettings;

            // create operation
            _hostOperation = new MultipartAsyncOperation(uiContext);
            _hostOperation.AddProgressOperation(new ProgressOperation(DetectWeblogSettings), 50);
            _blogEditingTemplateDetector = new BlogEditingTemplateDetector(_uiContext, hiddenBrowserParentControl, temporarySettings, false);
            _hostOperation.AddProgressOperation(new ProgressOperation(_blogEditingTemplateDetector.DetectTemplate), 50);
            return _hostOperation;
        }

        // settings detection is never a fatal error
        public bool FatalErrorOccurred { get { return false; } }
        public bool AuthenticationErrorOccurred { get { return false; } }

        public void ShowFatalError(IWin32Window owner) { }

        public bool WasCancelled
        {
            get
            {
                return (_hostOperation as IProgressHost).CancelRequested;
            }
        }

        public void OperationCompleted()
        {
            _temporarySettings.TemplateFiles = _blogEditingTemplateDetector.BlogTemplateFiles;
            if (_blogEditingTemplateDetector.PostBodyBackgroundColor != null)
                _temporarySettings.UpdatePostBodyBackgroundColor(_blogEditingTemplateDetector.PostBodyBackgroundColor.Value);
        }

        public WizardStep ExcludedStepIfCompleted
        {
            get { return _editWithStyleStep; }
        }

        public void ShowNonFatalErrors(IWin32Window owner)
        {
            // if a template error occurred then show it now
            if (_blogEditingTemplateDetector.ExceptionOccurred)
                DisplayMessage.Show(MessageId.TemplateDownloadFailed, owner);
        }

        private object DetectWeblogSettings(IProgressHost progressHost)
        {
            using (BlogClientUIContextScope uiContextScope = new BlogClientUIContextScope(_uiContext))
            {
                try
                {
                    BlogSettingsDetector blogSettingsDetector = new BlogSettingsDetector(_temporarySettings);
                    blogSettingsDetector.DetectSettings(progressHost);
                }
                catch (OperationCancelledException)
                {
                    // WasCancelled == true
                }
                catch (BlogClientOperationCancelledException)
                {
                    _hostOperation.Cancel();
                    // WasCancelled == true
                }
                catch (Exception ex)
                {
                    Trace.Fail("Error occurred while downloading weblog posts and  categories: " + ex.ToString());
                }

                return this;
            }
        }

        private TemporaryBlogSettings _temporarySettings;
        private BlogEditingTemplateDetector _blogEditingTemplateDetector;
        private MultipartAsyncOperation _hostOperation;
        WizardStep _editWithStyleStep;

    }

}

