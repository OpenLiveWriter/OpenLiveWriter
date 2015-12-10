// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls.Wizard;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{

    internal class WizardSharePointAutoDetectionStep : WizardAutoDetectionStep
    {
        public WizardSharePointAutoDetectionStep(
            IBlogClientUIContext uiContext,
            TemporaryBlogSettings temporarySettings,
            WizardController.NextCallback nextHandler,
            IWizardAutoDetectionOperation autoDetectionOperation)

            : base(uiContext, temporarySettings, nextHandler, autoDetectionOperation)
        {
        }

        public event EventHandler AuthenticationErrorOccurred;

        protected virtual void OnAuthenticationErrorOccurred(EventArgs e)
        {
            if (AuthenticationErrorOccurred != null)
                AuthenticationErrorOccurred(this, e);
        }

        protected override void HandleAuthenticationError()
        {
            OnAuthenticationErrorOccurred(EventArgs.Empty);
            Wizard.back();
        }
    }
    internal class WizardAutoDetectionStep : WizardSubStep
    {
        public WizardAutoDetectionStep(
            IBlogClientUIContext uiContext,
            TemporaryBlogSettings temporarySettings,
            WizardController.NextCallback nextHandler,
            IWizardAutoDetectionOperation autoDetectionOperation)

            : base(new WeblogConfigurationWizardPanelAutoDetection(),
                    StringId.ConfigWizardDetectSettings,
                    null, null, nextHandler, null, null)
        {
            _uiContext = uiContext;
            _temporarySettings = temporarySettings;
            _autoDetectionOperation = autoDetectionOperation;
        }

        public override void Display()
        {
            base.Display();

            // create and start the operation
            _asyncOperation = _autoDetectionOperation.CreateOperation(
                _uiContext,
                AutoDetectionPanel.BrowserParentControl,
                _temporarySettings);
            _asyncOperation.Completed += new EventHandler(_asyncOperation_Completed);
            _asyncOperation.Cancelled += new EventHandler(_asyncOperation_Completed);

            // show the progress UI
            Wizard.NextEnabled = false;
            Wizard.CancelEnabled = false;
            AutoDetectionPanel.Start(_asyncOperation, Wizard);

            // start the operation
            _asyncOperation.Start();
        }

        public override void Back()
        {
            // base removes this panel from the wizard
            base.Back();

            // screen late calls to zombie detector
            if (_asyncOperation == null)
                return;

            if (!_asyncOperation.IsDone)
            {
                _asyncOperation.Cancel();
                ExitAsyncOperation();
            }

        }

        protected virtual void HandleAuthenticationError()
        {
            HandleDetectionError();
        }

        protected virtual void HandleFatalError()
        {
            HandleDetectionError();
        }

        private void HandleDetectionError()
        {
            // move back
            Wizard.back();

            // show error
            _autoDetectionOperation.ShowFatalError(_uiContext);
        }

        private void _asyncOperation_Completed(object sender, EventArgs e)
        {
            // screen late calls to zombie detector
            if (_asyncOperation == null)
                return;

            if (_autoDetectionOperation.AuthenticationErrorOccurred)
            {
                HandleAuthenticationError();
                //Wizard.back();
            }
            else if (_autoDetectionOperation.FatalErrorOccurred)
            {
                HandleFatalError();
            }
            else if (_autoDetectionOperation.WasCancelled)
            {
                Wizard.back();
            }
            else
            {
                // copy the detected settings
                _autoDetectionOperation.OperationCompleted();

                // exclude steps as appropriate
                WizardStep excludeStep = _autoDetectionOperation.ExcludedStepIfCompleted;
                if (excludeStep != null)
                    if (Wizard.StepExists(excludeStep))
                        Wizard.removeWizardStep(excludeStep);

                // go to the next step (might be a substep or might be default)
                Wizard.next();

                // show non fatal errors
                _autoDetectionOperation.ShowNonFatalErrors(AutoDetectionPanel.FindForm());

                // remove us from the wizard
                Wizard.removeWizardStep(this);
            }

            // clear the account detector
            ExitAsyncOperation();
        }

        private void ExitAsyncOperation()
        {
            Wizard.NextEnabled = true;
            Wizard.CancelEnabled = true;

            if (_asyncOperation != null)
            {
                _asyncOperation.Completed -= new EventHandler(_asyncOperation_Completed);
                _asyncOperation.Cancelled -= new EventHandler(_asyncOperation_Completed);
                _asyncOperation = null;
            }
        }

        private IWin32Window DialogOwner
        {
            get { return AutoDetectionPanel.FindForm(); }
        }

        private WeblogConfigurationWizardPanelAutoDetection AutoDetectionPanel
        {
            get { return Control as WeblogConfigurationWizardPanelAutoDetection; }
        }

        private IBlogClientUIContext _uiContext;
        protected TemporaryBlogSettings _temporarySettings;
        private IWizardAutoDetectionOperation _autoDetectionOperation;
        private AsyncOperation _asyncOperation;

    }
}
