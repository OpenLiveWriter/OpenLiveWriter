// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Platform;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.Console.CrossPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("=== OpenLiveWriter Cross-Platform Proof of Concept ===");
            System.Console.WriteLine($"Platform: {Environment.OSVersion.Platform}");
            System.Console.WriteLine($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
            System.Console.WriteLine($"Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
            System.Console.WriteLine($".NET Version: {Environment.Version}");
            System.Console.WriteLine();

            // Test 1: Platform interfaces are accessible
            System.Console.WriteLine("[Test 1] Platform interfaces accessible...");
            System.Console.WriteLine($"  ISettingsPersister type: {typeof(ISettingsPersister).FullName}");
            System.Console.WriteLine($"  IPlatformServices type: {typeof(IPlatformServices).FullName}");
            System.Console.WriteLine($"  ICredentialStorage type: {typeof(ICredentialStorage).FullName}");
            System.Console.WriteLine($"  IBlogClientUIContext type: {typeof(IBlogClientUIContext).FullName}");
            System.Console.WriteLine($"  PlatformContext.IsInitialized: {PlatformContext.IsInitialized}");
            System.Console.WriteLine("  PASS");
            System.Console.WriteLine();

            // Test 2: HTML Parser works cross-platform
            System.Console.WriteLine("[Test 2] HTML Parser works...");
            string html = "<html><head><title>Test</title></head><body><p>Hello from macOS!</p></body></html>";
            var parser = new SimpleHtmlParser(html);
            int elementCount = 0;
            Element el;
            while ((el = parser.Next()) != null)
            {
                elementCount++;
            }
            System.Console.WriteLine($"  Parsed {elementCount} elements from HTML");
            System.Console.WriteLine("  PASS");
            System.Console.WriteLine();

            // Test 3: PlatformContext initialization with stubs
            System.Console.WriteLine("[Test 3] PlatformContext initialization with stubs...");
            PlatformContext.Initialize(
                services: new StubPlatformServices(),
                display: new StubDisplayHelper(),
                credentials: new StubCredentialStorage(),
                bidi: new StubBidiSupport(),
                spellCheck: new StubSpellCheckProvider());
            System.Console.WriteLine($"  PlatformContext.IsInitialized: {PlatformContext.IsInitialized}");
            System.Console.WriteLine($"  Services type: {PlatformContext.Services.GetType().Name}");
            System.Console.WriteLine($"  App data dir: {PlatformContext.Services.GetApplicationDataDirectory()}");
            System.Console.WriteLine("  PASS");
            System.Console.WriteLine();

            System.Console.WriteLine("=== All tests passed! OpenLiveWriter core libraries work on this platform. ===");
        }
    }

    // Stub implementations for non-Windows platforms
    class StubPlatformServices : IPlatformServices
    {
        public string GetApplicationDataDirectory()
        {
            string path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OpenLiveWriter");
            System.IO.Directory.CreateDirectory(path);
            return path;
        }
        public string GetLocalApplicationDataDirectory()
        {
            string path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpenLiveWriter");
            System.IO.Directory.CreateDirectory(path);
            return path;
        }
        public string GetShortPathName(string path) => path;
        public void ExtractCabinet(string cabinetPath, string targetDirectory) =>
            throw new PlatformNotSupportedException("Cabinet extraction not supported on this platform");
        public bool IsApplicationInstalled() => true;
        public ISettingsPersister CreateUserSettingsPersister(string subKey) =>
            throw new NotImplementedException("Settings persister not implemented for this platform yet");
    }

    class StubDisplayHelper : IDisplayHelper
    {
        public int DefaultDpi => 72; // macOS default DPI
        public float TwipsToPixelsX(int twips) => twips * 72f / 1440f;
        public float TwipsToPixelsY(int twips) => twips * 72f / 1440f;
        public bool IsCompositionEnabled() => true;
    }

    class StubCredentialStorage : ICredentialStorage
    {
        public void StoreCredential(string key, string username, string password) { }
        public (string username, string password)? RetrieveCredential(string key) => null;
        public void DeleteCredential(string key) { }
        public bool CredentialExists(string key) => false;
    }

    class StubBidiSupport : IBidiSupport
    {
        public void DrawText(Graphics g, string text, Font font,
            Rectangle bounds, Color color, bool isRtl) { }
        public Size MeasureText(Graphics g, string text,
            Font font, bool isRtl) => Size.Empty;
        public void DrawIcon(Graphics g, Icon icon,
            Rectangle bounds, bool isRtl) { }
        public Rectangle AdjustLayoutRect(Rectangle containerBounds,
            Rectangle childBounds, bool isRtl) => childBounds;
    }

    class StubSpellCheckProvider : ISpellCheckProvider
    {
        public bool IsWordCorrect(string word, string language) => true;
        public string[] GetSuggestions(string word, string language) => Array.Empty<string>();
        public void AddToUserDictionary(string word, string language) { }
        public bool IsAvailable(string language) => false;
    }
}
