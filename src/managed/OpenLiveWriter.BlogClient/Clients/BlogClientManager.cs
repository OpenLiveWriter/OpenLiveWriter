// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Net;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients
{

    public sealed class BlogClientManager
    {
        public static bool IsValidClientType(string typeName)
        {
            // scan for a client type with a matching name
            string typeNameUpper = typeName.ToUpperInvariant();
            foreach (ClientTypeDefinition clientTypeDefinition in ClientTypes)
            {
                if (clientTypeDefinition.Name.ToUpperInvariant() == typeNameUpper)
                    return true;
            }

            // none found
            return false;
        }

        public static IBlogClient CreateClient(BlogAccount blogAccount, IBlogCredentialsAccessor credentials)
        {
            return CreateClient(blogAccount.ClientType, blogAccount.PostApiUrl, credentials);
        }

        public static IBlogClient CreateClient(string clientType, string postApiUrl, IBlogCredentialsAccessor credentials)
        {
            Debug.Assert(clientType != "WindowsLiveSpaces", "Use of WindowsLiveSpaces client is deprecated");

            // scan for a client type with a matching name
            string clientTypeUpper = clientType.ToUpperInvariant();
            foreach (ClientTypeDefinition clientTypeDefinition in ClientTypes)
            {
                if (clientTypeDefinition.Name.ToUpperInvariant() == clientTypeUpper)
                {
                    return (IBlogClient)clientTypeDefinition.Constructor.Invoke(new object[] {
                        new Uri(postApiUrl), credentials  });
                }
            }

            // didn't find a match!
            throw new ArgumentException(
                String.Format(CultureInfo.CurrentCulture, "Client type {0} not found.", clientType));
        }

        public static IBlogClient CreateClient(IBlogSettingsAccessor settings)
        {
            return CreateClient(settings.ClientType, settings.PostApiUrl, settings.Credentials, settings.ProviderId, settings.OptionOverrides, settings.UserOptionOverrides, settings.HomePageOverrides);
        }

        public static IBlogClient CreateClient(string clientType, string postApiUrl, IBlogCredentialsAccessor credentials, string providerId, IDictionary optionOverrides, IDictionary userOptionOverrides, IDictionary homepageOptionOverrides)
        {
            // create blog client reflecting the settings
            IBlogClient blogClient = CreateClient(clientType, postApiUrl, credentials);

            // if there is a provider associated with the client then use it to override options
            // as necessary for this provider
            IBlogProvider provider = BlogProviderManager.FindProvider(providerId);
            if (provider != null)
            {
                IBlogClientOptions providerOptions = provider.ConstructBlogOptions(blogClient.Options);
                blogClient.OverrideOptions(providerOptions);
            }

            if (homepageOptionOverrides != null)
            {
                OptionOverrideReader homepageOptionsReader = new OptionOverrideReader(homepageOptionOverrides);
                IBlogClientOptions homepageOptions = BlogClientOptions.ApplyOptionOverrides(new OptionReader(homepageOptionsReader.Read), blogClient.Options, true);
                blogClient.OverrideOptions(homepageOptions);
            }

            // if there are manifest overrides then apply them
            if (optionOverrides != null)
            {
                OptionOverrideReader manifestOptionsReader = new OptionOverrideReader(optionOverrides);
                IBlogClientOptions manifestOptions = BlogClientOptions.ApplyOptionOverrides(new OptionReader(manifestOptionsReader.Read), blogClient.Options, true);
                blogClient.OverrideOptions(manifestOptions);
            }

            // if there are user overrides then apply them
            if (userOptionOverrides != null)
            {
                OptionOverrideReader userOptionsReader = new OptionOverrideReader(userOptionOverrides);
                IBlogClientOptions userOptions = BlogClientOptions.ApplyOptionOverrides(new OptionReader(userOptionsReader.Read), blogClient.Options, true);
                blogClient.OverrideOptions(userOptions);
            }

            // return the blog client
            return blogClient;
        }

        private class OptionOverrideReader
        {
            public OptionOverrideReader(IDictionary optionOverrides)
            {
                _optionOverrides = optionOverrides;
            }

            public string Read(string optionName)
            {
                return _optionOverrides[optionName] as string;
            }

            private IDictionary _optionOverrides;
        }

        private static IList ClientTypes
        {
            get
            {
                lock (_classLock)
                {
                    if (_clientTypes == null)
                    {
                        _clientTypes = new ArrayList();
                        AddClientType(typeof(LiveJournalClient));
                        AddClientType(typeof(MetaweblogClient));
                        AddClientType(typeof(MovableTypeClient));
                        AddClientType(typeof(GenericAtomClient));
                        AddClientType(typeof(GoogleBloggerv3Client));
                        AddClientType(typeof(BloggerAtomClient));
                        AddClientType(typeof(SharePointClient));
                        AddClientType(typeof(WordPressClient));
                    }
                    return _clientTypes;
                }
            }
        }
        private static IList _clientTypes;
        private static object _classLock = new object();

        private static void AddClientType(Type clientType)
        {
            try
            {
                _clientTypes.Add(new ClientTypeDefinition(clientType));
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception adding Blog ClientType: " + ex.ToString());
            }
        }

        private class ClientTypeDefinition
        {
            public ClientTypeDefinition(Type clientType)
            {
                // determine the name from the custom attribute
                BlogClientAttribute[] blogClientAttributes = clientType.GetCustomAttributes(typeof(BlogClientAttribute), false) as BlogClientAttribute[];
                if (blogClientAttributes.Length != 1)
                    throw new ArgumentException("You must provide a single BlogClientAttribute for all registered blog client types.");
                _name = blogClientAttributes[0].TypeName;

                // get the constructor used for creation
                _constructor = clientType.GetConstructor(new Type[] { typeof(Uri), typeof(IBlogCredentialsAccessor) });
                if (_constructor == null)
                    throw new ArgumentException("You must implement a public constructor with the signature (Uri,IBlogCredentials,PostFormatOptions) for all registered blog client types.");

                // record the type
                _type = clientType;
            }

            public string Name
            {
                get { return _name; }
            }
            private string _name;

            public ConstructorInfo Constructor
            {
                get { return _constructor; }
            }
            private ConstructorInfo _constructor;

            public Type Type
            {
                get { return _type; }
            }
            private Type _type;
        }
    }

}
