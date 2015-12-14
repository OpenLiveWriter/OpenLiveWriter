// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Controls.Wizard;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{

    public class WeblogConfigurationWizardController : WizardController, IBlogClientUIContext, IDisposable
    {
        #region Creation and Initialization and Disposal

        public static string Welcome(IWin32Window owner)
        {
            TemporaryBlogSettings temporarySettings = TemporaryBlogSettings.CreateNew();
            using (WeblogConfigurationWizardController controller = new WeblogConfigurationWizardController(temporarySettings))
            {
                return controller.WelcomeWeblog(owner);
            }
        }

        public static string Add(IWin32Window owner, bool permitSwitchingWeblogs)
        {
            bool switchToWeblog;
            return Add(owner, permitSwitchingWeblogs, out switchToWeblog);
        }

        public static string Add(IWin32Window owner, bool permitSwitchingWeblogs, out bool switchToWeblog)
        {
            TemporaryBlogSettings temporarySettings = TemporaryBlogSettings.CreateNew();

            temporarySettings.IsNewWeblog = true;
            temporarySettings.SwitchToWeblog = true;

            using (WeblogConfigurationWizardController controller = new WeblogConfigurationWizardController(temporarySettings))
            {
                return controller.AddWeblog(owner, ApplicationEnvironment.ProductNameQualified, permitSwitchingWeblogs, out switchToWeblog);
            }
        }

        public static string AddBlog(IWin32Window owner, Uri blogToAdd)
        {
            TemporaryBlogSettings temporarySettings = TemporaryBlogSettings.CreateNew();

            temporarySettings.IsNewWeblog = true;
            temporarySettings.SwitchToWeblog = true;

            string username;
            string password;
            Uri homepageUrl;

            ParseAddBlogUri(blogToAdd, out username, out password, out homepageUrl);
            temporarySettings.HomepageUrl = homepageUrl.ToString();
            temporarySettings.Credentials.Username = username;
            temporarySettings.Credentials.Password = password;
            temporarySettings.SavePassword = false;

            using (WeblogConfigurationWizardController controller = new WeblogConfigurationWizardController(temporarySettings))
            {
                bool dummy;
                return controller.AddWeblogSkipType(owner, ApplicationEnvironment.ProductNameQualified, false, out dummy);
            }
        }

        public static bool EditTemporarySettings(IWin32Window owner, TemporaryBlogSettings settings)
        {
            using (WeblogConfigurationWizardController controller = new WeblogConfigurationWizardController(settings))
                return controller.EditWeblogTemporarySettings(owner);
        }

        private WeblogConfigurationWizardController(TemporaryBlogSettings settings)
            : base()
        {
            _temporarySettings = settings;
        }

        public void Dispose()
        {
            //clear any cached credential information that may have been set by the wizard
            ClearTransientCredentials();

            System.GC.SuppressFinalize(this);
        }

        ~WeblogConfigurationWizardController()
        {
            Debug.Fail("Wizard controller was not disposed");
        }

        private string WelcomeWeblog(IWin32Window owner)
        {
            _preventSwitchingToWeblog = true;
            // welcome is the same as add with one additional step on the front end
            WizardStep wizardStep;
            addWizardStep(
                wizardStep = new WizardStep(new WeblogConfigurationWizardPanelWelcome(),
                StringId.ConfigWizardWelcome,
                null, null, new NextCallback(OnWelcomeCompleted), null, null));
            wizardStep.WantsFocus = false;

            addWizardStep(
                new WizardStep(new WeblogConfigurationWizardPanelConfirmation(),
                StringId.ConfigWizardComplete,
                new DisplayCallback(OnConfirmationDisplayed),
                new VerifyStepCallback(OnValidatePanel),
                new NextCallback(OnConfirmationCompleted),
                null,
                null));

            bool switchToWeblog;
            return ShowBlogWizard(ApplicationEnvironment.ProductNameQualified, owner, out switchToWeblog);
        }

        private string AddWeblogSkipType(IWin32Window owner, string caption, bool permitSwitchingWeblogs, out bool switchToWeblog)
        {
            _preventSwitchingToWeblog = !permitSwitchingWeblogs;

            _temporarySettings.IsSpacesBlog = false;
            _temporarySettings.IsSharePointBlog = false;

            AddBasicInfoSubStep();

            AddConfirmationStep();

            return ShowBlogWizard(caption, owner, out switchToWeblog);
        }

        private string AddWeblog(IWin32Window owner, string caption, bool permitSwitchingWeblogs, out bool switchToWeblog)
        {
            _preventSwitchingToWeblog = !permitSwitchingWeblogs;

            AddChooseBlogTypeStep();
            AddConfirmationStep();

            return ShowBlogWizard(caption, owner, out switchToWeblog);
        }

        private string ShowBlogWizard(string caption, IWin32Window owner, out bool switchToWeblog)
        {
            // blog id to return
            string blogId = null;
            if (ShowDialog(owner, caption) == DialogResult.OK)
            {
                // save the blog settings
                using (BlogSettings blogSettings = BlogSettings.ForBlogId(_temporarySettings.Id))
                {
                    _temporarySettings.Save(blogSettings);
                    blogId = blogSettings.Id;
                }

                // note the last added weblog (for re-selection in subsequent invocations of the dialog)
                WeblogConfigurationWizardSettings.LastServiceName = _temporarySettings.ServiceName;
            }

            switchToWeblog = _temporarySettings.SwitchToWeblog;

            return blogId;
        }

        private bool EditWeblogTemporarySettings(IWin32Window owner)
        {
            // first step conditional on blog type
            if (_temporarySettings.IsSharePointBlog)
                AddSharePointBasicInfoSubStep(true);
            else
                AddBasicInfoSubStep();

            AddConfirmationStep();

            if (ShowDialog(owner, Res.Get(StringId.UpdateAccountConfigurationTitle)) == DialogResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddChooseBlogTypeStep()
        {
            addWizardStep(
                new WizardStep(new WeblogConfigurationWizardPanelBlogType(),
                StringId.ConfigWizardChooseWeblogType,
                new DisplayCallback(OnChooseBlogTypeDisplayed),
                null,
                new NextCallback(OnChooseBlogTypeCompleted),
                null,
                null));
        }

        private void AddBasicInfoSubStep()
        {
            WeblogConfigurationWizardPanelBasicInfo panel = new WeblogConfigurationWizardPanelBasicInfo();
            addWizardSubStep(
                new WizardSubStep(panel,
                StringId.ConfigWizardBasicInfo,
                new DisplayCallback(OnBasicInfoDisplayed),
                new VerifyStepCallback(OnValidatePanel),
                new NextCallback(OnBasicInfoCompleted),
                null,
                null));
        }

        private void AddSharePointBasicInfoSubStep(bool showAuthenticationStep)
        {
            addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelSharePointBasicInfo(),
                StringId.ConfigWizardSharePointHomepage,
                new DisplayCallback(OnBasicInfoDisplayed),
                new VerifyStepCallback(OnValidatePanel),
                new NextCallback(OnSharePointBasicInfoCompleted),
                new NextCallback(OnSharePointBasicInfoUndone),
                null));

            _authenticationRequired = showAuthenticationStep;
        }

        private void AddGoogleBloggerOAuthSubStep()
        {
            addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelGoogleBloggerAuthentication(_temporarySettings.Id, this),
                null,
                new DisplayCallback(OnBasicInfoDisplayed),
                new VerifyStepCallback(OnValidatePanel),
                new NextCallback(OnGoogleBloggerOAuthCompleted),
                null,
                new BackCallback(OnGoogleBloggerOAuthBack)));
        }

        private void AddConfirmationStep()
        {
            addWizardStep(
                new WizardStep(new WeblogConfigurationWizardPanelConfirmation(),
                StringId.ConfigWizardComplete,
                new DisplayCallback(OnConfirmationDisplayed),
                new VerifyStepCallback(OnValidatePanel),
                new NextCallback(OnConfirmationCompleted),
                null,
                null));
        }

        private DialogResult ShowDialog(IWin32Window owner, string title)
        {
            using (new WaitCursor())
            {
                DialogResult result;

                using (_wizardForm = new WeblogConfigurationWizard(this))
                {
                    using (new BlogClientUIContextScope(_wizardForm))
                    {
                        _owner = _wizardForm;

                        _wizardForm.Text = title;

                        // Show in taskbar if it's a top-level window.  This is true during welcome
                        if (owner == null)
                        {
                            _wizardForm.ShowInTaskbar = true;
                            _wizardForm.StartPosition = FormStartPosition.CenterScreen;
                        }

                        result = _wizardForm.ShowDialog(owner);

                        _owner = null;
                    }
                }
                _wizardForm = null;
                if (_detectionOperation != null && !_detectionOperation.IsDone)
                    _detectionOperation.Cancel();

                return result;
            }
        }

        #endregion

        #region Welcome Panel
        private void OnWelcomeCompleted(Object stepControl)
        {
            //setup the next steps based on which choice the user selected.
            addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelBlogType(),
                StringId.ConfigWizardChooseWeblogType,
                new DisplayCallback(OnChooseBlogTypeDisplayed),
                null,
                new NextCallback(OnChooseBlogTypeCompleted),
                null,
                null));
        }
        #endregion

        #region Choose Blog Type Panel

        private void OnChooseBlogTypeDisplayed(Object stepControl)
        {
            // Fixes for 483356: In account configuration wizard, hitting back in select provider or success screens causes anomalous behavior
            // Need to clear cached credentials and cached blogname otherwise they'll be used downstream in the wizard...
            ClearTransientCredentials();
            _temporarySettings.BlogName = string.Empty;

            // Bug 681904: Insure that the next and cancel are always available when this panel is displayed.
            NextEnabled = true;
            CancelEnabled = true;

            // get reference to panel
            WeblogConfigurationWizardPanelBlogType panelBlogType = stepControl as WeblogConfigurationWizardPanelBlogType;

            // notify it that it is being displayed (reset dirty state)
            panelBlogType.OnDisplayPanel();
        }

        private void OnChooseBlogTypeCompleted(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelBlogType panelBlogType = stepControl as WeblogConfigurationWizardPanelBlogType;

            // if the user is changing types then blank out the blog info
            if (panelBlogType.UserChangedSelection)
            {
                _temporarySettings.HomepageUrl = String.Empty;
                _temporarySettings.Credentials.Clear();
            }

            // set the user's choice
            _temporarySettings.IsSharePointBlog = panelBlogType.IsSharePointBlog;
            _temporarySettings.IsGoogleBloggerBlog = panelBlogType.IsGoogleBloggerBlog;

            // did this bootstrap a custom account wizard?
            _providerAccountWizard = panelBlogType.ProviderAccountWizard;

            // add the next wizard sub step as appropriate
            if (_temporarySettings.IsSharePointBlog)
            {
                AddSharePointBasicInfoSubStep(false);
            }
            else if (_temporarySettings.IsGoogleBloggerBlog)
            {
                AddGoogleBloggerOAuthSubStep();
            }
            else
            {
                AddBasicInfoSubStep();
            }
        }

        #endregion

        private static void ParseAddBlogUri(Uri blogToAdd, out string username, out string password, out Uri homepageUrl)
        {
            // The URL is in the format http://username:password@blogUrl/;
            // We use the Uri class to extract the username:password (comes as a single string) and then parse it.
            // We strip the username:password from the remaining url and return it.

            username = null;
            password = null;

            string[] userInfoSplit = System.Web.HttpUtility.UrlDecode(blogToAdd.UserInfo).Split(':');
            if (userInfoSplit.Length > 0)
            {
                username = userInfoSplit[0];
                if (userInfoSplit.Length > 1)
                {
                    password = userInfoSplit[1];
                }
            }

            homepageUrl = new Uri(blogToAdd.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
        }

        #region Basic Info Panel

        private void OnBasicInfoDisplayed(Object stepControl)
        {
            // Fixes for 483356: In account configuration wizard, hitting back in select provider or success screens causes anomalous behavior
            // Need to clear cached credentials and cached blogname otherwise they'll be used downstream in the wizard...
            _temporarySettings.BlogName = string.Empty;

            // get reference to data interface for panel
            IAccountBasicInfoProvider basicInfo = stepControl as IAccountBasicInfoProvider;

            // populate basic data
            basicInfo.ProviderAccountWizard = _providerAccountWizard;
            basicInfo.AccountId = _temporarySettings.Id;
            basicInfo.HomepageUrl = _temporarySettings.HomepageUrl;
            basicInfo.ForceManualConfiguration = _temporarySettings.ForceManualConfig;
            basicInfo.Credentials = _temporarySettings.Credentials;
            basicInfo.SavePassword = basicInfo.Credentials.Password != String.Empty && (_temporarySettings.SavePassword ?? true);
        }

        private delegate void PerformBlogAutoDetection();
        private void OnBasicInfoCompleted(Object stepControl)
        {
            OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(PerformWeblogAndSettingsAutoDetectionSubStep));
        }

        private void OnBasicInfoAndAuthenticationCompleted(IAccountBasicInfoProvider basicInfo, PerformBlogAutoDetection performBlogAutoDetection)
        {
            // copy the settings
            _temporarySettings.HomepageUrl = basicInfo.HomepageUrl;
            _temporarySettings.ForceManualConfig = basicInfo.ForceManualConfiguration;
            _temporarySettings.Credentials = basicInfo.Credentials;
            _temporarySettings.SavePassword = basicInfo.SavePassword;

            // clear the transient credentials so we don't accidentally use cached credentials
            ClearTransientCredentials();

            if (!_temporarySettings.ForceManualConfig)
            {
                // perform auto-detection
                performBlogAutoDetection();
            }
            else
            {
                PerformSelectProviderSubStep();
            }
        }

        private void OnSharePointBasicInfoCompleted(Object stepControl)
        {
            if (_authenticationRequired)
                AddSharePointAuthenticationStep((IAccountBasicInfoProvider)stepControl);
            else
                OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(PerformSharePointAutoDetectionSubStep));
        }

        private void OnSharePointBasicInfoUndone(Object stepControl)
        {
            if (_authenticationRequired && !_authenticationStepAdded)
            {
                AddSharePointAuthenticationStep((IAccountBasicInfoProvider)stepControl);
                next();
            }
        }

        private void AddSharePointAuthenticationStep(IAccountBasicInfoProvider basicInfoProvider)
        {
            if (!_authenticationStepAdded)
            {
                addWizardSubStep(new WizardSubStep(new WeblogConfigurationWizardPanelSharePointAuthentication(basicInfoProvider),
                                                   StringId.ConfigWizardSharePointLogin,
                                                   new WizardController.DisplayCallback(OnSharePointAuthenticationDisplayed),
                                                   new VerifyStepCallback(OnValidatePanel),
                                                   new WizardController.NextCallback(OnSharePointAuthenticationComplete),
                                                   null,
                                                   new WizardController.BackCallback(OnSharePointAuthenticationBack)));
                _authenticationStepAdded = true;
            }
        }

        #endregion

        #region Weblog and Settings Auto Detection

        private void PerformWeblogAndSettingsAutoDetectionSubStep()
        {
            // Clear the provider so the user will be forced to do autodetection
            // until we have successfully configured a publishing interface
            _temporarySettings.ClearProvider();
            _detectionOperation = new WizardWeblogAndSettingsAutoDetectionOperation(_editWithStyleStep);

            // performe the step
            addWizardSubStep(new WizardAutoDetectionStep(
                (IBlogClientUIContext)this,
                _temporarySettings,
                new NextCallback(OnWeblogAndSettingsAutoDetectionCompleted),
                _detectionOperation));
        }

        private WizardWeblogAndSettingsAutoDetectionOperation _detectionOperation;

        private void PerformSharePointAutoDetectionSubStep()
        {
            // Clear the provider so the user will be forced to do autodetection
            // until we have successfully configured a publishing interface
            _temporarySettings.ClearProvider();

            AddAutoDetectionStep();
        }

        private void AddAutoDetectionStep()
        {
            _detectionOperation = new WizardSharePointAutoDetectionOperation(_editWithStyleStep);

            WizardSharePointAutoDetectionStep sharePointDetectionStep =
                new WizardSharePointAutoDetectionStep(
                (IBlogClientUIContext)this,
                _temporarySettings,
                new NextCallback(OnWeblogAndSettingsAutoDetectionCompleted),
                _detectionOperation);

            if (!_authenticationStepAdded)
                sharePointDetectionStep.AuthenticationErrorOccurred += new EventHandler(sharePointDetectionStep_AuthenticationErrorOccurred);

            addWizardSubStep(sharePointDetectionStep);
        }
        private void sharePointDetectionStep_AuthenticationErrorOccurred(object sender, EventArgs e)
        {
            this._authenticationRequired = true;
        }

        private void OnSharePointAuthenticationDisplayed(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelSharePointAuthentication panelBlogType = stepControl as WeblogConfigurationWizardPanelSharePointAuthentication;

            // set value
            panelBlogType.Credentials = _temporarySettings.Credentials;
            panelBlogType.SavePassword = _temporarySettings.Credentials.Password != String.Empty;
        }

        private void OnSharePointAuthenticationComplete(Object stepControl)
        {
            OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(PerformSharePointAutoDetectionSubStep));
        }

        private void OnSharePointAuthenticationBack(Object stepControl)
        {
            _authenticationStepAdded = false;
        }

        private void OnGoogleBloggerOAuthCompleted(Object stepControl)
        {
            OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(PerformWeblogAndSettingsAutoDetectionSubStep));
        }

        private void OnGoogleBloggerOAuthBack(Object stepControl)
        {
            var panel = (WeblogConfigurationWizardPanelGoogleBloggerAuthentication)stepControl;
            panel.CancelAuthorization();
        }

        private void OnWeblogAndSettingsAutoDetectionCompleted(Object stepControl)
        {
            // if we weren't able to identify a specific weblog
            if (_temporarySettings.HostBlogId == String.Empty)
            {
                // if we have a list of weblogs then show the blog list
                if (_temporarySettings.HostBlogs.Length > 0)
                {
                    PerformSelectBlogSubStep();
                }
                else // kick down to select a provider
                {
                    PerformSelectProviderSubStep();
                }
            }
            else
            {
                PerformSelectImageEndpointIfNecessary();
            }
        }

        private void PerformSelectImageEndpointIfNecessary()
        {
            if (_temporarySettings.HostBlogId != string.Empty
                && _temporarySettings.AvailableImageEndpoints != null
                && _temporarySettings.AvailableImageEndpoints.Length > 0)
            {
                /*
                if (_temporarySettings.AvailableImageEndpoints.Length == 1)
                {
                    IDictionary optionOverrides = _temporarySettings.OptionOverrides;
                    optionOverrides[BlogClientOptions.IMAGE_ENDPOINT] = _temporarySettings.AvailableImageEndpoints[0].Id;
                    _temporarySettings.OptionOverrides = optionOverrides;
                }
                else
                    PerformSelectImageEndpointSubStep();
                */

                // currently we always show the image endpoint selection UI if we find at least one.
                PerformSelectImageEndpointSubStep();
            }
        }

        #endregion

        #region Select Provider Panel

        void PerformSelectProviderSubStep()
        {
            addWizardSubStep(new WizardSubStep(
                new WeblogConfigurationWizardPanelSelectProvider(),
                StringId.ConfigWizardSelectProvider,
                new DisplayCallback(OnSelectProviderDisplayed),
                new VerifyStepCallback(OnValidatePanel),
                new NextCallback(OnSelectProviderCompleted),
                null,
                null));
        }

        void OnSelectProviderDisplayed(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelSelectProvider panelSelectProvider = stepControl as WeblogConfigurationWizardPanelSelectProvider;

            // show the panel
            panelSelectProvider.ShowPanel(
                _temporarySettings.ServiceName,
                _temporarySettings.HomepageUrl,
                _temporarySettings.Id,
                _temporarySettings.Credentials);
        }

        void OnSelectProviderCompleted(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelSelectProvider panelSelectProvider = stepControl as WeblogConfigurationWizardPanelSelectProvider;

            // record the provider and blog info
            IBlogProviderDescription provider = panelSelectProvider.SelectedBlogProvider;
            _temporarySettings.SetProvider(provider.Id, provider.Name, provider.PostApiUrl, provider.ClientType);
            _temporarySettings.HostBlogId = String.Empty;
            if (panelSelectProvider.TargetBlog != null)
                _temporarySettings.SetBlogInfo(panelSelectProvider.TargetBlog);
            _temporarySettings.HostBlogs = panelSelectProvider.UsersBlogs;

            // If we don't yet have a HostBlogId then the user needs to choose from
            // among available weblogs
            if (_temporarySettings.HostBlogId == String.Empty)
            {
                PerformSelectBlogSubStep();
            }
            else
            {
                // if we have not downloaded an editing template yet for this
                // weblog then execute this now
                PerformSettingsAutoDetectionSubStepIfNecessary();
            }
        }

        #endregion

        #region Select Blog Panel

        void PerformSelectBlogSubStep()
        {
            addWizardSubStep(new WizardSubStep(
                new WeblogConfigurationWizardPanelSelectBlog(),
                StringId.ConfigWizardSelectWeblog,
                new DisplayCallback(OnSelectBlogDisplayed),
                new VerifyStepCallback(OnValidatePanel),
                new NextCallback(OnSelectBlogCompleted),
                null,
                null));
        }

        void OnSelectBlogDisplayed(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelSelectBlog panelSelectBlog = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // show the panel
            panelSelectBlog.ShowPanel(_temporarySettings.HostBlogs, _temporarySettings.HostBlogId);
        }

        private void OnSelectBlogCompleted(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelSelectBlog panelSelectBlog = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // get the selected blog
            _temporarySettings.SetBlogInfo(panelSelectBlog.SelectedBlog);

            // if we have not downloaded an editing template yet for this
            // weblog then execute this now
            PerformSettingsAutoDetectionSubStepIfNecessary();
        }

        #endregion

        #region Select Image Endpoint Panel

        void PerformSelectImageEndpointSubStep()
        {
            WeblogConfigurationWizardPanelSelectBlog panel = new WeblogConfigurationWizardPanelSelectBlog();
            panel.LabelText = Res.Get(StringId.CWSelectImageEndpointText);
            addWizardSubStep(new WizardSubStep(
                                panel,
                                StringId.ConfigWizardSelectImageEndpoint,
                                new DisplayCallback(OnSelectImageEndpointDisplayed),
                                new VerifyStepCallback(OnValidatePanel),
                                new NextCallback(OnSelectImageEndpointCompleted),
                                null,
                                null));
        }

        void OnSelectImageEndpointDisplayed(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelSelectBlog panelSelectImageEndpoint = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // show the panel
            panelSelectImageEndpoint.ShowPanel(_temporarySettings.AvailableImageEndpoints, _temporarySettings.OptionOverrides[BlogClientOptions.IMAGE_ENDPOINT] as string);
        }

        private void OnSelectImageEndpointCompleted(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelSelectBlog panelSelectBlog = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // get the selected blog
            IDictionary optionOverrides = _temporarySettings.HomePageOverrides;
            optionOverrides[BlogClientOptions.IMAGE_ENDPOINT] = panelSelectBlog.SelectedBlog.Id;
            _temporarySettings.HomePageOverrides = optionOverrides;
        }

        #endregion

        #region Weblog Settings Auto Detection

        private void PerformSettingsAutoDetectionSubStepIfNecessary()
        {
            if (_temporarySettings.TemplateFiles.Length == 0)
            {
                PerformSettingsAutoDetectionSubStep();
            }
        }

        private void PerformSettingsAutoDetectionSubStep()
        {
            // performe the step
            addWizardSubStep(new WizardAutoDetectionStep(
                (IBlogClientUIContext)this,
                _temporarySettings, new NextCallback(OnPerformSettingsAutoDetectionCompleted),
                new WizardSettingsAutoDetectionOperation(_editWithStyleStep)));
        }

        private void OnPerformSettingsAutoDetectionCompleted(object stepControl)
        {
            PerformSelectImageEndpointIfNecessary();
        }

        #endregion

        #region Confirmation Panel

        void OnConfirmationDisplayed(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelConfirmation panelConfirmation = stepControl as WeblogConfigurationWizardPanelConfirmation;

            // show the panel
            panelConfirmation.ShowPanel(_temporarySettings, _preventSwitchingToWeblog);
        }

        void OnConfirmationCompleted(Object stepControl)
        {
            // get reference to panel
            WeblogConfigurationWizardPanelConfirmation panelConfirmation = stepControl as WeblogConfigurationWizardPanelConfirmation;

            // save settings
            _temporarySettings.BlogName = panelConfirmation.WeblogName;
            _temporarySettings.SwitchToWeblog = panelConfirmation.SwitchToWeblog;
        }

        #endregion

        #region Generic Helpers

        private bool OnValidatePanel(Object panelControl)
        {
            WeblogConfigurationWizardPanel wizardPanel = panelControl as WeblogConfigurationWizardPanel;
            return wizardPanel.ValidatePanel();
        }

        /// <summary>
        /// Clear any cached credential information for the blog
        /// </summary>
        private void ClearTransientCredentials()
        {
            //clear any cached credential information associated with this blog (fixes bug 373063)
            new BlogCredentialsAccessor(_temporarySettings.Id, _temporarySettings.Credentials).TransientCredentials = null;
        }

        #endregion

        #region Private Members

        private IWin32Window _owner = null;
        private WeblogConfigurationWizard _wizardForm;
        private TemporaryBlogSettings _temporarySettings;
        private bool _preventSwitchingToWeblog = false;
        private WizardStep _editWithStyleStep = null;
        private IBlogProviderAccountWizardDescription _providerAccountWizard;
        private bool _authenticationRequired = false;
        private bool _authenticationStepAdded;

        #endregion

        #region IBlogClientUIContext Members

        IntPtr IWin32Window.Handle { get { return _wizardForm.Handle; } }
        bool ISynchronizeInvoke.InvokeRequired { get { return _wizardForm.InvokeRequired; } }
        IAsyncResult ISynchronizeInvoke.BeginInvoke(Delegate method, object[] args) { return _wizardForm.BeginInvoke(method, args); }
        object ISynchronizeInvoke.EndInvoke(IAsyncResult result) { return _wizardForm.EndInvoke(result); }
        object ISynchronizeInvoke.Invoke(Delegate method, object[] args) { return _wizardForm.Invoke(method, args); }

        #endregion

    }

    internal interface IAccountBasicInfoProvider
    {
        IBlogProviderAccountWizardDescription ProviderAccountWizard { set; }
        string AccountId { set; }
        string HomepageUrl { get; set; }
        bool SavePassword { get; set; }
        IBlogCredentials Credentials { get; set; }
        bool ForceManualConfiguration { get; set; }
        bool IsDirty(TemporaryBlogSettings settings);
        BlogInfo BlogAccount { get; }
    }
}
