// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.FileDestinations
{

    /// <summary>
    /// Web publish destination
    /// </summary>
    public class WebPublishDestination
    {
        /// <summary>
        /// Initialize web-publish destination w/ the id
        /// </summary>
        /// <param name="id"></param>
        public WebPublishDestination(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Destination-id
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
        }
        private string id;

        /// <summary>
        /// Is the destination valid?
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (Id != null)
                {
                    return DestinationProfileManager.HasProfile(Id);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Name of the destination
        /// </summary>
        public string Name
        {
            get
            {
                const string UNKNOWN = "(Unknown)";
                if (Profile != null)
                    return Profile.Name != null ? Profile.Name : UNKNOWN;
                else
                    return UNKNOWN;
            }
        }

        /// <summary>
        /// Profile information for the destination (returns null if destination is invalid)
        /// </summary>
        public DestinationProfile Profile
        {
            get
            {
                // update cached destination profile if none exists
                if (destinationProfile == null)
                {
                    if (IsValid)
                        destinationProfile = DestinationProfileManager.loadProfile(Id);

                }
                return destinationProfile;
            }
        }
        private DestinationProfile destinationProfile = null;

        /// <summary>
        /// Refresh the cached destination profile
        /// </summary>
        public void RefreshProfile()
        {
            destinationProfile = null;
        }

        /// <summary>
        /// Helper to get a destination profile manager
        /// </summary>
        private DestinationProfileManager DestinationProfileManager
        {
            get
            {
                if (destinationProfileManager == null)
                    destinationProfileManager = new DestinationProfileManager();
                return destinationProfileManager;
            }
        }
        private DestinationProfileManager destinationProfileManager;

    }
}
