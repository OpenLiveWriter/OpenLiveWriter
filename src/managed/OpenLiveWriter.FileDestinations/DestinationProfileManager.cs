// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Manages the storage of publish and destination settings in the Windows Registry.
    /// </summary>
    public class DestinationProfileManager
    {

        public DestinationProfileManager() : this(FILE_DESTINATIONS_KEY)
        {

        }

        public DestinationProfileManager(string destinationKey)
        {
            _destinationKey = destinationKey;
        }

        #region Destination Settings Management

        public bool HasProfile(string key)
        {
            if (key == string.Empty)
                return false;

            SettingsPersisterHelper settings = SettingsRoot;
            return settings.HasSubSettings(_destinationKey + @"\" + key);
        }

        /// <summary>
        /// Returns the profile associated with the key, or null if no such profile exists.
        /// </summary>
        /// <param name="key">The name of the registry key to define the profile under.</param>
        /// <returns></returns>
        public DestinationProfile loadProfile(String key)
        {
            SettingsPersisterHelper settings = getProfileSettings(key);

            if (settings.GetNames().Length == 0)
            {
                return null;
            }
            DestinationProfile profile = new DestinationProfile();
            profile.Id = key;
            profile.Name = settings.GetString(PROFILE_NAME_KEY, "");
            profile.WebsiteURL = settings.GetString(PROFILE_WEBSITE_URL_KEY, "");
            profile.FtpServer = settings.GetString(PROFILE_FTP_SERVER_KEY, "");
            profile.UserName = settings.GetString(PROFILE_FTP_USER_KEY, "");
            profile.FtpPublishPath = settings.GetString(PROFILE_FTP_PUBLISH_DIR_KEY, "");
            profile.LocalPublishPath = settings.GetString(PROFILE_PUBLISH_DIR_KEY, "");
            profile.Type = (DestinationProfile.DestType)settings.GetInt32(PROFILE_DESTINATION_TYPE_KEY, 0);

            //load the decrypted password
            try
            {
                profile.Password = settings.GetEncryptedString(PROFILE_FTP_PASSWORD_KEY);
            }
            catch (Exception e)
            {
                Trace.Fail("Failed to decrypt password: " + e);
            }

            return profile;
        }

        /// <summary>
        /// Saves a destination profile into the registry.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="profile"></param>
        public void saveProfile(String key, DestinationProfile profile)
        {
            SettingsPersisterHelper settings = getProfileSettings(key);
            profile.Id = key;
            settings.SetString(PROFILE_NAME_KEY, profile.Name);
            settings.SetString(PROFILE_WEBSITE_URL_KEY, profile.WebsiteURL);
            settings.SetString(PROFILE_FTP_SERVER_KEY, profile.FtpServer);
            settings.SetString(PROFILE_FTP_PUBLISH_DIR_KEY, profile.FtpPublishPath);
            settings.SetString(PROFILE_FTP_USER_KEY, profile.UserName);
            settings.SetString(PROFILE_PUBLISH_DIR_KEY, profile.LocalPublishPath);
            settings.SetInt32(PROFILE_DESTINATION_TYPE_KEY, (short)profile.Type);

            //save an encrypted password
            try
            {
                settings.SetEncryptedString(PROFILE_FTP_PASSWORD_KEY, profile.Password);
            }
            catch (Exception e)
            {
                //if an exception occurs, just leave the password empty
                Trace.Fail("Failed to encrypt password: " + e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Remove the destination profile associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        public void deleteProfile(string key)
        {
            SettingsPersisterHelper settings = SettingsRoot;
            settings = settings.GetSubSettings(_destinationKey);
            settings.SettingsPersister.UnsetSubSettingsTree(key);
        }

        /// <summary>
        /// Returns all of the available destination profiles.
        /// </summary>
        /// <returns>an array of all available profiles</returns>
        public DestinationProfile[] Profiles
        {
            get
            {
                SettingsPersisterHelper settings = SettingsRoot;
                settings = settings.GetSubSettings(_destinationKey);
                string[] keys = settings.GetSubSettingNames();
                DestinationProfile[] profiles = new DestinationProfile[keys.Length];
                for (int i = 0; i < profiles.Length; i++)
                {
                    profiles[i] = loadProfile(keys[i]);
                }
                return profiles;
            }
        }

        /// <summary>
        /// Returns ID profile designated as the default profile
        /// </summary>
        public string DefaultProfileId
        {
            get
            {
                SettingsPersisterHelper settings = SettingsRoot;
                settings = settings.GetSubSettings(_destinationKey);
                string key = settings.GetString(PROFILE_DEFAULT_PROFILE_KEY, null);
                return key;
            }
            set
            {
                SettingsPersisterHelper settings = SettingsRoot;
                settings = settings.GetSubSettings(_destinationKey);
                settings.SetString(PROFILE_DEFAULT_PROFILE_KEY, value);
            }
        }

        /// <summary>
        /// Returns the registry subtree for a specified profile.
        /// </summary>
        /// <param name="profileKey"></param>
        /// <returns></returns>
        private SettingsPersisterHelper getProfileSettings(string profileKey)
        {
            SettingsPersisterHelper settings = SettingsRoot;
            settings = settings.GetSubSettings(_destinationKey + @"\" + profileKey);
            return settings;
        }

        #endregion

        #region Class Configuration (location of settings, etc)

        /// <summary>
        /// Allow configuration of the settings root to be used by the DestinationProfileManager
        /// (defaults to product settings key)
        /// </summary>
        public static SettingsPersisterHelper SettingsRoot
        {
            get
            {
                return ApplicationEnvironment.UserSettingsRoot;
            }

        }
        private string _destinationKey;

        #endregion

        #region Constants

        // root key
        private const string FILE_DESTINATIONS_KEY = @"FileDestinations";

        //destination settings keys
        private const string PROFILE_NAME_KEY = "Name";
        private const string PROFILE_WEBSITE_URL_KEY = "URL";
        private const string PROFILE_FTP_SERVER_KEY = "FtpServer";
        private const string PROFILE_FTP_USER_KEY = "FtpUserName";
        private const string PROFILE_FTP_PASSWORD_KEY = "FtpPassword";
        private const string PROFILE_FTP_PUBLISH_DIR_KEY = "FtpPublishDir";
        private const string PROFILE_PUBLISH_DIR_KEY = "PublishDir";
        private const string PROFILE_DESTINATION_TYPE_KEY = "DestinationType";
        private const string PROFILE_RSS_FEED_KEY = "RssFeed";
        private const string PROFILE_DEFAULT_PROFILE_KEY = "DefaultProfile";

        #endregion

    }

}
