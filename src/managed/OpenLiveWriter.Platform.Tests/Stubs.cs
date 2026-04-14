// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.Platform.Tests
{
    internal class StubPlatformServices : IPlatformServices
    {
        public string GetApplicationDataDirectory() => "/tmp/olw-test";
        public string GetLocalApplicationDataDirectory() => "/tmp/olw-test-local";
        public string GetShortPathName(string path) => path;
        public void ExtractCabinet(string cabinetPath, string targetDirectory) => throw new PlatformNotSupportedException();
        public bool IsApplicationInstalled() => false;
        public ISettingsPersister CreateUserSettingsPersister(string subKey) => throw new NotImplementedException();
    }

    internal class StubDisplayHelper : IDisplayHelper
    {
        public int DefaultDpi => 96;
        public float TwipsToPixelsX(int twips) => twips * 96f / 1440f;
        public float TwipsToPixelsY(int twips) => twips * 96f / 1440f;
        public bool IsCompositionEnabled() => true;
    }

    internal class StubCredentialStorage : ICredentialStorage
    {
        public void StoreCredential(string key, string username, string password) { }
        public (string username, string password)? RetrieveCredential(string key) => null;
        public void DeleteCredential(string key) { }
        public bool CredentialExists(string key) => false;
    }

    internal class StubBidiSupport : IBidiSupport
    {
        public void DrawText(Graphics g, string text, Font font, Rectangle bounds, Color color, bool isRtl) { }
        public Size MeasureText(Graphics g, string text, Font font, bool isRtl) => Size.Empty;
        public void DrawIcon(Graphics g, Icon icon, Rectangle bounds, bool isRtl) { }
        public Rectangle AdjustLayoutRect(Rectangle containerBounds, Rectangle childBounds, bool isRtl) => childBounds;
    }

    internal class StubSpellCheckProvider : ISpellCheckProvider
    {
        public bool IsWordCorrect(string word, string language) => true;
        public string[] GetSuggestions(string word, string language) => Array.Empty<string>();
        public void AddToUserDictionary(string word, string language) { }
        public bool IsAvailable(string language) => false;
    }
}
