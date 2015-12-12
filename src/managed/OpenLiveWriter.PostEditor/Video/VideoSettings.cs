// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.Video
{
    public class VideoSettings
    {
        public static string selectedServiceName
        {
            get
            {
                return _settings.GetString(SELECTEDSERVICE, "");
            }
            set
            {
                _settings.SetString(SELECTEDSERVICE, value);
            }
        }

        public static VideoServiceSettings GetServiceSettings(string serviceId)
        {
            return new VideoServiceSettings(serviceId, _settings.GetSubSettings("Services"));
        }

        private const string SELECTEDSERVICE = "selectedServiceName";

        private static readonly SettingsPersisterHelper _settings = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("Video");
    }

    public class VideoServiceSettings
    {

        public VideoServiceSettings(string serviceId, SettingsPersisterHelper parentPersister)
        {
            _settings = parentPersister.GetSubSettings(serviceId);
        }

        public string SelectedRequestType
        {
            get
            {
                return _settings.GetString(REQUESTTYPE, "");
            }
            set
            {
                _settings.SetString(REQUESTTYPE, value);
            }
        }

        public string Username
        {
            get
            {
                return _settings.GetString(USERNAME, "");
            }
            set
            {
                _settings.SetString(USERNAME, value);
            }
        }

        private const string REQUESTTYPE = "RequestType";
        private const string USERNAME = "Username";
        private readonly SettingsPersisterHelper _settings;

    }
}
