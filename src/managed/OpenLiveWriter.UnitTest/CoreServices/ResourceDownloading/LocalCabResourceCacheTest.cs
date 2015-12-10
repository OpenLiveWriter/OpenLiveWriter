// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.ResourceDownloading;

namespace OpenLiveWriter.UnitTest.CoreServices.ResourceDownloading
{
    [TestFixture]
    public class LocalCabResourceCacheTest
    {

        [SetUp]
        public void SetUp()
        {

        }

        private static string GetSupportingCab(string cabName)
        {
            string tempPath = Path.Combine(TempFileManager.Instance.CreateTempDir(), cabName);
            ResourceHelper.SaveAssemblyResourceToFile(string.Format(CultureInfo.InvariantCulture, "CoreServices.ResourceDownloading.SupportingCabs.{0}", cabName), tempPath);
            return UrlHelper.CreateUrlFromPath(tempPath);
        }

        [Test]
        public void DisableCabDownload()
        {
            LocalCabResourceCache cache = new LocalCabResourceCache(TempFileManager.Instance.CreateTempDir(), GetSupportingCab("Unsigned_1532.0311.cab"), false);
            cache.Refresh(10000, false);

            if (cache.RecentlyRefreshed)
                throw new Exception("Cache appears to have downloaded even though downloading was disabled!");
        }

        [Test]
        public void CoreCacheTests()
        {
            string cacheDir = TempFileManager.Instance.CreateTempDir();

            LocalCabResourceCache cache = new LocalCabResourceCache(cacheDir, GetSupportingCab("Unsigned_1529.0310.cab"), true);
            SetEnforceSigning(cache, false);
            cache.Refresh(10000, true);

            if (!cache.RecentlyRefreshed)
                throw new Exception("Empty cache wasn't refreshed. Cache should be marked as recently refreshed!");

            if (cache.CabVersion != new Version("12.0.1529.310"))
                throw new Exception("Empty cache wasn't refreshed. Cab version is incorrect");

            using (Stream blogProviders = cache.GetResource("BlogProvidersB5.xml"))
            {
                if (blogProviders == null)
                    throw new Exception("Cache doesn't contain blog providers file!");
            }

            cache = new LocalCabResourceCache(cacheDir, GetSupportingCab("Unsigned_1532.0311.cab"), true);
            cache.Refresh(10000, false);

            if (!cache.RecentlyRefreshed)
                throw new Exception("Old cache wasn't refreshed. Cache should be marked as recently refreshed!");

            if (cache.CabVersion != new Version("12.0.1532.311"))
                throw new Exception("Old cache wasn't refreshed. Cab version is incorrect");

            cache = new LocalCabResourceCache(cacheDir, GetSupportingCab("Unsigned_1529.0310.cab"), true);
            cache.Refresh(10000, false);

            if (cache.CabVersion == new Version("12.0.1529.310"))
                throw new Exception("Old cab file should be ignored- rolling back to old version is not allowed!");
        }

        [Test]
        public void SigningIsEnforced()
        {
            LocalCabResourceCache cache = new LocalCabResourceCache(TempFileManager.Instance.CreateTempDir(), GetSupportingCab("Unsigned_1532.0311.cab"), true);

            SetEnforceSigning(cache, true);

            cache.Refresh(10000, false);

            if (cache.RecentlyRefreshed)
                throw new Exception("Signing wasn't enforced!");
        }

        private static void SetEnforceSigning(LocalCabResourceCache cache, bool enforce)
        {
            FieldInfo signingField = cache.GetType().GetField("_enforceSigning", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            signingField.SetValue(cache, enforce);
        }

        [Test]
        public void RefreshDoesntThrow()
        {
            LocalCabResourceCache cache =
                new LocalCabResourceCache(TempFileManager.Instance.CreateTempDir(),
                                          UrlHelper.CreateUrlFromPath(@"c:\foo.cab"), true);
            cache.Refresh(10000, false);
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
