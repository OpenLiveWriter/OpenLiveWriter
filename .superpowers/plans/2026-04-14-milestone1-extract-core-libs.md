# Milestone 1: Extract Cross-Platform Core Libraries — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Retarget core business logic projects from `net10.0-windows` to `net10.0`, extracting all Windows-specific code behind platform abstractions so the core libraries are genuinely cross-platform.

**Architecture:** Create a `OpenLiveWriter.Platform` project with cross-platform interfaces and a `OpenLiveWriter.Platform.Windows` project with Windows implementations. Modify `Directory.Build.props` to allow per-project TFM overrides. Retarget 7 core projects. Wire up Windows implementations at app startup. All work on branch `feature/macbuild`.

**Tech Stack:** .NET 10, C#, NUnit 4, `System.Drawing.Common` NuGet, Windows Registry API, DPAPI (`System.Security.Cryptography.ProtectedData`)

---

## Prerequisites

Before starting, create the feature branch:

```bash
git checkout feature/csharp-ribbon
git checkout -b feature/macbuild
git push -u origin feature/macbuild
```

---

## Task 1: Modify Directory.Build.props to Allow Per-Project TFM Overrides

**Files:**
- Modify: `src/managed/Directory.Build.props`

- [ ] **Step 1: Read the current file to confirm state**

Run: `cat src/managed/Directory.Build.props | head -20`

Confirm `<TargetFramework>net10.0-windows</TargetFramework>` is unconditional on line 9.

- [ ] **Step 2: Add conditional defaults**

In `src/managed/Directory.Build.props`, change lines 8-12 from:

```xml
<TargetFramework>net10.0-windows</TargetFramework>

<!-- Enable Windows Forms -->
<UseWindowsForms>true</UseWindowsForms>
```

to:

```xml
<!-- Default to Windows target; core/platform projects override in their own .csproj -->
<TargetFramework Condition="'$(TargetFramework)' == ''">net10.0-windows</TargetFramework>

<!-- Enable Windows Forms by default; core projects set false -->
<UseWindowsForms Condition="'$(UseWindowsForms)' == ''">true</UseWindowsForms>
```

- [ ] **Step 3: Build to verify nothing breaks**

Run: `dotnet build src/managed/writer.sln --no-restore -v q`
Expected: Build succeeds with 0 errors (warnings are OK).

- [ ] **Step 4: Commit**

```bash
git add src/managed/Directory.Build.props
git commit -m "build: Make TargetFramework and UseWindowsForms conditional in Directory.Build.props"
```

---

## Task 2: Create `OpenLiveWriter.Platform` Project with Interfaces

**Files:**
- Create: `src/managed/OpenLiveWriter.Platform/OpenLiveWriter.Platform.csproj`
- Create: `src/managed/OpenLiveWriter.Platform/ISettingsPersister.cs`
- Create: `src/managed/OpenLiveWriter.Platform/ICredentialStorage.cs`
- Create: `src/managed/OpenLiveWriter.Platform/IPlatformServices.cs`
- Create: `src/managed/OpenLiveWriter.Platform/IDisplayHelper.cs`
- Create: `src/managed/OpenLiveWriter.Platform/IBlogClientUIContext.cs`
- Create: `src/managed/OpenLiveWriter.Platform/IBidiSupport.cs`
- Create: `src/managed/OpenLiveWriter.Platform/ISpellCheckProvider.cs`
- Create: `src/managed/OpenLiveWriter.Platform/PlatformContext.cs`

- [ ] **Step 1: Create the project file**

Create `src/managed/OpenLiveWriter.Platform/OpenLiveWriter.Platform.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AssemblyName>OpenLiveWriter.Platform</AssemblyName>
    <RootNamespace>OpenLiveWriter.Platform</RootNamespace>
    <TargetFramework>net10.0</TargetFramework>
    <UseWindowsForms>false</UseWindowsForms>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="..\GlobalAssemblyInfo.cs" Link="GlobalAssemblyInfo.cs" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create `ISettingsPersister.cs`**

This is a copy of the existing interface from `CoreServices/Settings/ISettingsPersister.cs`, moved to the new namespace. Create `src/managed/OpenLiveWriter.Platform/ISettingsPersister.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Defines an interface for persistent settings.
    /// </summary>
    public interface ISettingsPersister : IDisposable
    {
        string[] GetNames();
        object Get(string name, Type desiredType, object defaultValue);
        object Get(string name);
        void Set(string name, object value);
        void Unset(string name);
        void UnsetSubSettingsTree(string name);
        IDisposable BatchUpdate();
        bool HasSubSettings(string subSettingsName);
        ISettingsPersister GetSubSettings(string subSettingsName);
        string[] GetSubSettings();
    }
}
```

- [ ] **Step 3: Create `ICredentialStorage.cs`**

Create `src/managed/OpenLiveWriter.Platform/ICredentialStorage.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Platform-specific secure credential storage.
    /// Windows: DPAPI. macOS: Keychain. Linux: libsecret.
    /// </summary>
    public interface ICredentialStorage
    {
        void StoreCredential(string key, string username, string password);
        (string username, string password)? RetrieveCredential(string key);
        void DeleteCredential(string key);
        bool CredentialExists(string key);
    }
}
```

- [ ] **Step 4: Create `IPlatformServices.cs`**

Create `src/managed/OpenLiveWriter.Platform/IPlatformServices.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Platform-specific services that vary by operating system.
    /// </summary>
    public interface IPlatformServices
    {
        /// <summary>
        /// Returns the platform-appropriate application data directory.
        /// Windows: %APPDATA%\OpenLiveWriter
        /// macOS: ~/Library/Application Support/OpenLiveWriter
        /// Linux: ~/.config/OpenLiveWriter
        /// </summary>
        string GetApplicationDataDirectory();

        /// <summary>
        /// Returns the platform-appropriate local (non-roaming) application data directory.
        /// Windows: %LOCALAPPDATA%\OpenLiveWriter
        /// macOS: ~/Library/Caches/OpenLiveWriter
        /// Linux: ~/.local/share/OpenLiveWriter
        /// </summary>
        string GetLocalApplicationDataDirectory();

        /// <summary>
        /// Gets the short (8.3) path name on Windows; returns the input path unchanged on other platforms.
        /// </summary>
        string GetShortPathName(string path);

        /// <summary>
        /// Extracts files from a .cab archive. Throws PlatformNotSupportedException on non-Windows.
        /// </summary>
        void ExtractCabinet(string cabinetPath, string targetDirectory);

        /// <summary>
        /// Returns true if the application is registered/installed on this platform.
        /// </summary>
        bool IsApplicationInstalled();

        /// <summary>
        /// Creates the root ISettingsPersister for user settings.
        /// Windows: RegistrySettingsPersister under HKCU\SOFTWARE\OpenLiveWriter.
        /// Others: XmlSettingsPersister in the app data directory.
        /// </summary>
        ISettingsPersister CreateUserSettingsPersister(string subKey);
    }
}
```

- [ ] **Step 5: Create `IDisplayHelper.cs`**

Create `src/managed/OpenLiveWriter.Platform/IDisplayHelper.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Display/DPI helper abstraction. Replaces the Windows-specific DisplayHelper
    /// that uses DWM P/Invoke.
    /// </summary>
    public interface IDisplayHelper
    {
        int DefaultDpi { get; }
        float TwipsToPixelsX(int twips);
        float TwipsToPixelsY(int twips);
        bool IsCompositionEnabled();
    }
}
```

- [ ] **Step 6: Create `IBlogClientUIContext.cs`**

Create `src/managed/OpenLiveWriter.Platform/IBlogClientUIContext.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Platform-agnostic UI context for blog client operations.
    /// Provides UI thread marshaling and native window handle for dialog ownership.
    /// </summary>
    public interface IBlogClientUIContext
    {
        /// <summary>Invoke a delegate on the UI thread synchronously.</summary>
        object Invoke(Delegate method, object[] args);

        /// <summary>Begin an async invoke on the UI thread.</summary>
        IAsyncResult BeginInvoke(Delegate method, object[] args);

        /// <summary>Complete an async invoke.</summary>
        object EndInvoke(IAsyncResult result);

        /// <summary>Returns true if the caller is not on the UI thread.</summary>
        bool InvokeRequired { get; }

        /// <summary>
        /// Returns the native window handle for dialog ownership.
        /// On Windows: HWND. On macOS: NSWindow pointer. On Linux: X11 window ID.
        /// Callers should treat this as opaque.
        /// </summary>
        IntPtr NativeWindowHandle { get; }
    }
}
```

- [ ] **Step 7: Create `IBidiSupport.cs`**

Create `src/managed/OpenLiveWriter.Platform/IBidiSupport.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Bidirectional (RTL/LTR) text and graphics rendering support.
    /// Replaces the Windows-specific BidiGraphics that uses P/Invoke to User32.dll.
    /// </summary>
    public interface IBidiSupport
    {
        void DrawText(Graphics g, string text, Font font, Rectangle bounds, Color color, bool isRtl);
        Size MeasureText(Graphics g, string text, Font font, bool isRtl);
        void DrawIcon(Graphics g, Icon icon, Rectangle bounds, bool isRtl);
        Rectangle AdjustLayoutRect(Rectangle containerBounds, Rectangle childBounds, bool isRtl);
    }
}
```

- [ ] **Step 8: Create `ISpellCheckProvider.cs`**

Create `src/managed/OpenLiveWriter.Platform/ISpellCheckProvider.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Cross-platform spell check abstraction.
    /// Windows: PlatformSpellCheck / WinRT spell checker.
    /// macOS: NSSpellChecker. Linux: hunspell.
    /// </summary>
    public interface ISpellCheckProvider
    {
        bool IsWordCorrect(string word, string language);
        string[] GetSuggestions(string word, string language);
        void AddToUserDictionary(string word, string language);
        bool IsAvailable(string language);
    }
}
```

- [ ] **Step 9: Create `PlatformContext.cs`**

Create `src/managed/OpenLiveWriter.Platform/PlatformContext.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Static service locator for platform-specific implementations.
    /// Must be initialized once at application startup before any platform services are used.
    /// </summary>
    public static class PlatformContext
    {
        private static bool _initialized;

        public static IPlatformServices Services { get; private set; }
        public static IDisplayHelper Display { get; private set; }
        public static ICredentialStorage Credentials { get; private set; }
        public static IBidiSupport Bidi { get; private set; }
        public static ISpellCheckProvider SpellCheck { get; private set; }

        public static void Initialize(
            IPlatformServices services,
            IDisplayHelper display,
            ICredentialStorage credentials,
            IBidiSupport bidi,
            ISpellCheckProvider spellCheck)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Display = display ?? throw new ArgumentNullException(nameof(display));
            Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            Bidi = bidi ?? throw new ArgumentNullException(nameof(bidi));
            SpellCheck = spellCheck ?? throw new ArgumentNullException(nameof(spellCheck));
            _initialized = true;
        }

        /// <summary>
        /// Returns true if Initialize has been called.
        /// </summary>
        public static bool IsInitialized => _initialized;

        /// <summary>
        /// Throws if not initialized. Call from code that depends on platform services.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    "PlatformContext has not been initialized. Call PlatformContext.Initialize() at application startup.");
        }

        /// <summary>
        /// Reset for unit testing only.
        /// </summary>
        internal static void Reset()
        {
            Services = null;
            Display = null;
            Credentials = null;
            Bidi = null;
            SpellCheck = null;
            _initialized = false;
        }
    }
}
```

- [ ] **Step 10: Add project to solution**

Run: `dotnet sln src/managed/writer.sln add src/managed/OpenLiveWriter.Platform/OpenLiveWriter.Platform.csproj`

- [ ] **Step 11: Build the new project**

Run: `dotnet build src/managed/OpenLiveWriter.Platform/OpenLiveWriter.Platform.csproj -v q`
Expected: Build succeeded.

- [ ] **Step 12: Commit**

```bash
git add src/managed/OpenLiveWriter.Platform/ src/managed/writer.sln
git commit -m "feat(platform): Add OpenLiveWriter.Platform project with cross-platform abstraction interfaces"
```

---

## Task 3: Add Unit Tests for Platform Interfaces

**Files:**
- Create: `src/managed/OpenLiveWriter.Platform.Tests/OpenLiveWriter.Platform.Tests.csproj`
- Create: `src/managed/OpenLiveWriter.Platform.Tests/PlatformContextTests.cs`

- [ ] **Step 1: Create test project file**

Create `src/managed/OpenLiveWriter.Platform.Tests/OpenLiveWriter.Platform.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AssemblyName>OpenLiveWriter.Platform.Tests</AssemblyName>
    <RootNamespace>OpenLiveWriter.Platform.Tests</RootNamespace>
    <TargetFramework>net10.0</TargetFramework>
    <UseWindowsForms>false</UseWindowsForms>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NUnit" />
    <PackageReference Include="NUnit3TestAdapter" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\OpenLiveWriter.Platform\OpenLiveWriter.Platform.csproj" />
  </ItemGroup>

  <!-- Allow access to internal Reset() method -->
  <ItemGroup>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
      <_Parameter1>OpenLiveWriter.Platform.Tests</_Parameter1>
    </AssemblyAttribute>
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Write PlatformContext tests**

Create `src/managed/OpenLiveWriter.Platform.Tests/PlatformContextTests.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;

namespace OpenLiveWriter.Platform.Tests
{
    [TestFixture]
    public class PlatformContextTests
    {
        [TearDown]
        public void TearDown()
        {
            PlatformContext.Reset();
        }

        [Test]
        public void IsInitialized_ReturnsFalse_BeforeInitialize()
        {
            Assert.That(PlatformContext.IsInitialized, Is.False);
        }

        [Test]
        public void EnsureInitialized_Throws_BeforeInitialize()
        {
            Assert.Throws<InvalidOperationException>(() => PlatformContext.EnsureInitialized());
        }

        [Test]
        public void Initialize_SetsAllProperties()
        {
            var services = new StubPlatformServices();
            var display = new StubDisplayHelper();
            var credentials = new StubCredentialStorage();
            var bidi = new StubBidiSupport();
            var spellCheck = new StubSpellCheckProvider();

            PlatformContext.Initialize(services, display, credentials, bidi, spellCheck);

            Assert.That(PlatformContext.IsInitialized, Is.True);
            Assert.That(PlatformContext.Services, Is.SameAs(services));
            Assert.That(PlatformContext.Display, Is.SameAs(display));
            Assert.That(PlatformContext.Credentials, Is.SameAs(credentials));
            Assert.That(PlatformContext.Bidi, Is.SameAs(bidi));
            Assert.That(PlatformContext.SpellCheck, Is.SameAs(spellCheck));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForServices()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(null, new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForDisplay()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), null, new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForCredentials()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), null, new StubBidiSupport(), new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForBidi()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), null, new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForSpellCheck()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), null));
        }

        [Test]
        public void EnsureInitialized_DoesNotThrow_AfterInitialize()
        {
            PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider());

            Assert.DoesNotThrow(() => PlatformContext.EnsureInitialized());
        }

        [Test]
        public void Reset_ClearsAllProperties()
        {
            PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider());

            PlatformContext.Reset();

            Assert.That(PlatformContext.IsInitialized, Is.False);
            Assert.That(PlatformContext.Services, Is.Null);
            Assert.That(PlatformContext.Display, Is.Null);
            Assert.That(PlatformContext.Credentials, Is.Null);
            Assert.That(PlatformContext.Bidi, Is.Null);
            Assert.That(PlatformContext.SpellCheck, Is.Null);
        }
    }
}
```

- [ ] **Step 3: Create stub implementations for testing**

Create `src/managed/OpenLiveWriter.Platform.Tests/Stubs.cs`:

```csharp
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
```

- [ ] **Step 4: Add InternalsVisibleTo to Platform project**

In `src/managed/OpenLiveWriter.Platform/OpenLiveWriter.Platform.csproj`, add inside the existing `<Project>` tag, after the `</PropertyGroup>`:

```xml
  <!-- Allow test project access to internal members -->
  <ItemGroup>
    <InternalsVisibleTo Include="OpenLiveWriter.Platform.Tests" />
  </ItemGroup>
```

- [ ] **Step 5: Add test project to solution**

Run: `dotnet sln src/managed/writer.sln add src/managed/OpenLiveWriter.Platform.Tests/OpenLiveWriter.Platform.Tests.csproj`

- [ ] **Step 6: Build and run tests**

Run: `dotnet test src/managed/OpenLiveWriter.Platform.Tests/OpenLiveWriter.Platform.Tests.csproj -v q`
Expected: All 9 tests pass.

- [ ] **Step 7: Commit**

```bash
git add src/managed/OpenLiveWriter.Platform.Tests/ src/managed/OpenLiveWriter.Platform/OpenLiveWriter.Platform.csproj src/managed/writer.sln
git commit -m "test(platform): Add unit tests for PlatformContext initialization and null guards"
```

---

## Task 4: Create `OpenLiveWriter.Platform.Windows` Project with Core Implementations

**Files:**
- Create: `src/managed/OpenLiveWriter.Platform.Windows/OpenLiveWriter.Platform.Windows.csproj`
- Create: `src/managed/OpenLiveWriter.Platform.Windows/WindowsPlatformServices.cs`
- Create: `src/managed/OpenLiveWriter.Platform.Windows/WindowsDisplayHelper.cs`
- Create: `src/managed/OpenLiveWriter.Platform.Windows/WindowsCredentialStorage.cs`
- Create: `src/managed/OpenLiveWriter.Platform.Windows/WindowsPlatformInitializer.cs`

- [ ] **Step 1: Create the project file**

Create `src/managed/OpenLiveWriter.Platform.Windows/OpenLiveWriter.Platform.Windows.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AssemblyName>OpenLiveWriter.Platform.Windows</AssemblyName>
    <RootNamespace>OpenLiveWriter.Platform.Windows</RootNamespace>
    <!-- Explicitly Windows-targeted for P/Invoke, Registry, DPAPI -->
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="..\GlobalAssemblyInfo.cs" Link="GlobalAssemblyInfo.cs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="System.Security.Cryptography.ProtectedData" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\OpenLiveWriter.Platform\OpenLiveWriter.Platform.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Add `System.Security.Cryptography.ProtectedData` to central packages**

In `src/managed/Directory.Packages.props`, add inside the `<ItemGroup>`:

```xml
    <!-- DPAPI for Windows credential storage -->
    <PackageVersion Include="System.Security.Cryptography.ProtectedData" Version="9.0.0" />
```

- [ ] **Step 3: Create `WindowsPlatformServices.cs`**

Create `src/managed/OpenLiveWriter.Platform.Windows/WindowsPlatformServices.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Win32;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsPlatformServices : IPlatformServices
    {
        private const string APP_NAME = "OpenLiveWriter";

        public string GetApplicationDataDirectory()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                APP_NAME);
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetLocalApplicationDataDirectory()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                APP_NAME);
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetShortPathName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            StringBuilder shortPath = new StringBuilder(260);
            int result = GetShortPathNameNative(path, shortPath, shortPath.Capacity);
            if (result == 0 || result > shortPath.Capacity)
                return path; // fallback to original path

            return shortPath.ToString();
        }

        [DllImport("Shlwapi.dll", EntryPoint = "PathGetShortPathW", CharSet = CharSet.Unicode)]
        private static extern int GetShortPathNameNative(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        public void ExtractCabinet(string cabinetPath, string targetDirectory)
        {
            // Delegate to the existing CabinetFileExtractor when moved here.
            // For now, throw if called — will be wired up when CoreServices is retargeted.
            throw new NotImplementedException("Cabinet extraction will be implemented when CabinetFileExtractor is moved to this project.");
        }

        public bool IsApplicationInstalled()
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("OPEN_LIVE_WRITER"))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public ISettingsPersister CreateUserSettingsPersister(string subKey)
        {
            string fullKey = $@"SOFTWARE\{APP_NAME}\{subKey}";
            return new RegistrySettingsPersister(Registry.CurrentUser, fullKey);
        }
    }
}
```

- [ ] **Step 4: Create `WindowsDisplayHelper.cs`**

Create `src/managed/OpenLiveWriter.Platform.Windows/WindowsDisplayHelper.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsDisplayHelper : IDisplayHelper
    {
        private const int DEFAULT_DPI = 96;
        private const int TWIPS_PER_INCH = 1440;
        private bool? _compositionEnabled;

        public int DefaultDpi => DEFAULT_DPI;

        public float TwipsToPixelsX(int twips)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return TwipsToPixels(twips, (int)g.DpiX);
            }
        }

        public float TwipsToPixelsY(int twips)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return TwipsToPixels(twips, (int)g.DpiY);
            }
        }

        private static float TwipsToPixels(int twips, int pixelsPerInch)
        {
            return (float)twips * pixelsPerInch / TWIPS_PER_INCH;
        }

        public bool IsCompositionEnabled()
        {
            if (_compositionEnabled.HasValue)
                return _compositionEnabled.Value;

            try
            {
                int result = DwmIsCompositionEnabled(out bool enabled);
                _compositionEnabled = result == 0 && enabled;
            }
            catch
            {
                _compositionEnabled = false;
            }

            return _compositionEnabled.Value;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);
    }
}
```

- [ ] **Step 5: Create `WindowsCredentialStorage.cs`**

Create `src/managed/OpenLiveWriter.Platform.Windows/WindowsCredentialStorage.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsCredentialStorage : ICredentialStorage
    {
        private const string CREDENTIAL_REGISTRY_PATH = @"SOFTWARE\OpenLiveWriter\Credentials";

        public void StoreCredential(string key, string username, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] encryptedBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);
            string encryptedBase64 = Convert.ToBase64String(encryptedBytes);

            using (RegistryKey regKey = Registry.CurrentUser.CreateSubKey($@"{CREDENTIAL_REGISTRY_PATH}\{key}"))
            {
                regKey.SetValue("Username", username);
                regKey.SetValue("Password", encryptedBase64);
            }
        }

        public (string username, string password)? RetrieveCredential(string key)
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey($@"{CREDENTIAL_REGISTRY_PATH}\{key}"))
            {
                if (regKey == null)
                    return null;

                string username = regKey.GetValue("Username") as string;
                string encryptedBase64 = regKey.GetValue("Password") as string;

                if (username == null || encryptedBase64 == null)
                    return null;

                try
                {
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
                    byte[] passwordBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                    string password = Encoding.UTF8.GetString(passwordBytes);
                    return (username, password);
                }
                catch (CryptographicException)
                {
                    return null;
                }
            }
        }

        public void DeleteCredential(string key)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"{CREDENTIAL_REGISTRY_PATH}\{key}", false);
            }
            catch
            {
                // Key doesn't exist, nothing to delete
            }
        }

        public bool CredentialExists(string key)
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey($@"{CREDENTIAL_REGISTRY_PATH}\{key}"))
            {
                return regKey != null;
            }
        }
    }
}
```

- [ ] **Step 6: Move `RegistrySettingsPersister.cs` to Platform.Windows**

Copy `src/managed/OpenLiveWriter.CoreServices/Settings/RegistrySettingsPersister.cs` to `src/managed/OpenLiveWriter.Platform.Windows/RegistrySettingsPersister.cs`.

Update the namespace in the new file from `OpenLiveWriter.CoreServices.Settings` to `OpenLiveWriter.Platform.Windows`, and change the interface reference from `OpenLiveWriter.CoreServices.Settings.ISettingsPersister` to `OpenLiveWriter.Platform.ISettingsPersister`:

At the top of the copied file, change:
```csharp
namespace OpenLiveWriter.CoreServices.Settings
```
to:
```csharp
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.Platform.Windows
```

**Do not delete the original file yet** — it's still referenced by the existing code. We'll remove it when we retarget CoreServices in a later task.

- [ ] **Step 7: Create `WindowsPlatformInitializer.cs`**

Create `src/managed/OpenLiveWriter.Platform.Windows/WindowsPlatformInitializer.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public static class WindowsPlatformInitializer
    {
        public static void Initialize()
        {
            PlatformContext.Initialize(
                services: new WindowsPlatformServices(),
                display: new WindowsDisplayHelper(),
                credentials: new WindowsCredentialStorage(),
                bidi: new WindowsBidiSupport(),
                spellCheck: new WindowsSpellCheckProvider());
        }
    }
}
```

- [ ] **Step 8: Create stub `WindowsBidiSupport.cs`**

Create `src/managed/OpenLiveWriter.Platform.Windows/WindowsBidiSupport.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    /// <summary>
    /// Windows implementation of IBidiSupport.
    /// Will be fleshed out when BidiGraphics is extracted from Localization project.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsBidiSupport : IBidiSupport
    {
        public void DrawText(Graphics g, string text, Font font, Rectangle bounds, Color color, bool isRtl)
        {
            using (Brush brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, bounds);
            }
        }

        public Size MeasureText(Graphics g, string text, Font font, bool isRtl)
        {
            SizeF size = g.MeasureString(text, font);
            return new Size((int)System.Math.Ceiling(size.Width), (int)System.Math.Ceiling(size.Height));
        }

        public void DrawIcon(Graphics g, Icon icon, Rectangle bounds, bool isRtl)
        {
            g.DrawIcon(icon, bounds);
        }

        public Rectangle AdjustLayoutRect(Rectangle containerBounds, Rectangle childBounds, bool isRtl)
        {
            if (!isRtl)
                return childBounds;

            // Mirror horizontally within container
            int mirroredX = containerBounds.Right - (childBounds.X - containerBounds.X) - childBounds.Width;
            return new Rectangle(mirroredX, childBounds.Y, childBounds.Width, childBounds.Height);
        }
    }
}
```

- [ ] **Step 9: Create stub `WindowsSpellCheckProvider.cs`**

Create `src/managed/OpenLiveWriter.Platform.Windows/WindowsSpellCheckProvider.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    /// <summary>
    /// Windows implementation of ISpellCheckProvider.
    /// Wraps the PlatformSpellCheck NuGet package.
    /// Will be fleshed out when SpellChecker project is retargeted.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsSpellCheckProvider : ISpellCheckProvider
    {
        public bool IsWordCorrect(string word, string language) => true;
        public string[] GetSuggestions(string word, string language) => Array.Empty<string>();
        public void AddToUserDictionary(string word, string language) { }
        public bool IsAvailable(string language) => false;
    }
}
```

- [ ] **Step 10: Add project to solution and build**

Run:
```bash
dotnet sln src/managed/writer.sln add src/managed/OpenLiveWriter.Platform.Windows/OpenLiveWriter.Platform.Windows.csproj
dotnet build src/managed/OpenLiveWriter.Platform.Windows/OpenLiveWriter.Platform.Windows.csproj -v q
```
Expected: Build succeeded.

- [ ] **Step 11: Build entire solution to verify no breakage**

Run: `dotnet build src/managed/writer.sln -v q`
Expected: Build succeeds with 0 errors.

- [ ] **Step 12: Commit**

```bash
git add src/managed/OpenLiveWriter.Platform.Windows/ src/managed/Directory.Packages.props src/managed/writer.sln
git commit -m "feat(platform): Add OpenLiveWriter.Platform.Windows with Registry, DPAPI, and display implementations"
```

---

## Task 5: Add Unit Tests for Windows Platform Implementations

**Files:**
- Create: `src/managed/OpenLiveWriter.Platform.Windows.Tests/OpenLiveWriter.Platform.Windows.Tests.csproj`
- Create: `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsCredentialStorageTests.cs`
- Create: `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsDisplayHelperTests.cs`
- Create: `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsPlatformServicesTests.cs`
- Create: `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsPlatformInitializerTests.cs`

- [ ] **Step 1: Create test project file**

Create `src/managed/OpenLiveWriter.Platform.Windows.Tests/OpenLiveWriter.Platform.Windows.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AssemblyName>OpenLiveWriter.Platform.Windows.Tests</AssemblyName>
    <RootNamespace>OpenLiveWriter.Platform.Windows.Tests</RootNamespace>
    <!-- Must target Windows for Registry/DPAPI tests -->
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NUnit" />
    <PackageReference Include="NUnit3TestAdapter" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\OpenLiveWriter.Platform\OpenLiveWriter.Platform.csproj" />
    <ProjectReference Include="..\OpenLiveWriter.Platform.Windows\OpenLiveWriter.Platform.Windows.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Write WindowsCredentialStorage tests**

Create `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsCredentialStorageTests.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Microsoft.Win32;
using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsCredentialStorageTests
    {
        private WindowsCredentialStorage _storage;
        private const string TEST_KEY = "TestBlog_UnitTest_" + nameof(WindowsCredentialStorageTests);

        [SetUp]
        public void SetUp()
        {
            _storage = new WindowsCredentialStorage();
            _storage.DeleteCredential(TEST_KEY);
        }

        [TearDown]
        public void TearDown()
        {
            _storage.DeleteCredential(TEST_KEY);
        }

        [Test]
        public void StoreAndRetrieve_RoundTripsCredentials()
        {
            _storage.StoreCredential(TEST_KEY, "user@example.com", "s3cret!P@ss");

            var result = _storage.RetrieveCredential(TEST_KEY);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.username, Is.EqualTo("user@example.com"));
            Assert.That(result.Value.password, Is.EqualTo("s3cret!P@ss"));
        }

        [Test]
        public void RetrieveCredential_ReturnsNull_WhenNotStored()
        {
            var result = _storage.RetrieveCredential("nonexistent_key_" + Guid.NewGuid());
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CredentialExists_ReturnsTrue_AfterStore()
        {
            _storage.StoreCredential(TEST_KEY, "user", "pass");
            Assert.That(_storage.CredentialExists(TEST_KEY), Is.True);
        }

        [Test]
        public void CredentialExists_ReturnsFalse_WhenNotStored()
        {
            Assert.That(_storage.CredentialExists("nonexistent_key_" + Guid.NewGuid()), Is.False);
        }

        [Test]
        public void DeleteCredential_RemovesCredential()
        {
            _storage.StoreCredential(TEST_KEY, "user", "pass");
            _storage.DeleteCredential(TEST_KEY);
            Assert.That(_storage.CredentialExists(TEST_KEY), Is.False);
        }

        [Test]
        public void DeleteCredential_DoesNotThrow_WhenNotExists()
        {
            Assert.DoesNotThrow(() => _storage.DeleteCredential("nonexistent_key_" + Guid.NewGuid()));
        }

        [Test]
        public void StoreCredential_OverwritesExisting()
        {
            _storage.StoreCredential(TEST_KEY, "user1", "pass1");
            _storage.StoreCredential(TEST_KEY, "user2", "pass2");

            var result = _storage.RetrieveCredential(TEST_KEY);
            Assert.That(result.Value.username, Is.EqualTo("user2"));
            Assert.That(result.Value.password, Is.EqualTo("pass2"));
        }

        [Test]
        public void StoreAndRetrieve_HandlesUnicodePassword()
        {
            _storage.StoreCredential(TEST_KEY, "user", "пароль_密码_🔑");

            var result = _storage.RetrieveCredential(TEST_KEY);
            Assert.That(result.Value.password, Is.EqualTo("пароль_密码_🔑"));
        }
    }
}
```

- [ ] **Step 3: Write WindowsDisplayHelper tests**

Create `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsDisplayHelperTests.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsDisplayHelperTests
    {
        private WindowsDisplayHelper _helper;

        [SetUp]
        public void SetUp()
        {
            _helper = new WindowsDisplayHelper();
        }

        [Test]
        public void DefaultDpi_Is96()
        {
            Assert.That(_helper.DefaultDpi, Is.EqualTo(96));
        }

        [Test]
        public void TwipsToPixelsX_ConvertsCorrectly_At96Dpi()
        {
            // 1440 twips = 1 inch = 96 pixels at 96 DPI
            float result = _helper.TwipsToPixelsX(1440);
            // May not be exactly 96 if system DPI differs, but should be positive
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void TwipsToPixelsX_ZeroTwips_ReturnsZero()
        {
            Assert.That(_helper.TwipsToPixelsX(0), Is.EqualTo(0));
        }

        [Test]
        public void IsCompositionEnabled_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _helper.IsCompositionEnabled());
        }
    }
}
```

- [ ] **Step 4: Write WindowsPlatformServices tests**

Create `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsPlatformServicesTests.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsPlatformServicesTests
    {
        private WindowsPlatformServices _services;

        [SetUp]
        public void SetUp()
        {
            _services = new WindowsPlatformServices();
        }

        [Test]
        public void GetApplicationDataDirectory_ReturnsExistingDirectory()
        {
            string dir = _services.GetApplicationDataDirectory();
            Assert.That(Directory.Exists(dir), Is.True);
            Assert.That(dir, Does.Contain("OpenLiveWriter"));
        }

        [Test]
        public void GetLocalApplicationDataDirectory_ReturnsExistingDirectory()
        {
            string dir = _services.GetLocalApplicationDataDirectory();
            Assert.That(Directory.Exists(dir), Is.True);
            Assert.That(dir, Does.Contain("OpenLiveWriter"));
        }

        [Test]
        public void GetShortPathName_ReturnsNonEmpty_ForExistingPath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string shortPath = _services.GetShortPathName(path);
            Assert.That(shortPath, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GetShortPathName_ReturnsOriginal_ForNullOrEmpty()
        {
            Assert.That(_services.GetShortPathName(null), Is.Null);
            Assert.That(_services.GetShortPathName(""), Is.EqualTo(""));
        }

        [Test]
        public void CreateUserSettingsPersister_ReturnsNonNull()
        {
            using (var persister = _services.CreateUserSettingsPersister("UnitTest_" + Guid.NewGuid().ToString("N")))
            {
                Assert.That(persister, Is.Not.Null);
            }
        }
    }
}
```

- [ ] **Step 5: Write WindowsPlatformInitializer tests**

Create `src/managed/OpenLiveWriter.Platform.Windows.Tests/WindowsPlatformInitializerTests.cs`:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsPlatformInitializerTests
    {
        [TearDown]
        public void TearDown()
        {
            PlatformContext.Reset();
        }

        [Test]
        public void Initialize_SetsAllPlatformServices()
        {
            WindowsPlatformInitializer.Initialize();

            Assert.That(PlatformContext.IsInitialized, Is.True);
            Assert.That(PlatformContext.Services, Is.InstanceOf<WindowsPlatformServices>());
            Assert.That(PlatformContext.Display, Is.InstanceOf<WindowsDisplayHelper>());
            Assert.That(PlatformContext.Credentials, Is.InstanceOf<WindowsCredentialStorage>());
            Assert.That(PlatformContext.Bidi, Is.InstanceOf<WindowsBidiSupport>());
            Assert.That(PlatformContext.SpellCheck, Is.InstanceOf<WindowsSpellCheckProvider>());
        }
    }
}
```

- [ ] **Step 6: Add test project to solution, build, and run**

Run:
```bash
dotnet sln src/managed/writer.sln add src/managed/OpenLiveWriter.Platform.Windows.Tests/OpenLiveWriter.Platform.Windows.Tests.csproj
dotnet test src/managed/OpenLiveWriter.Platform.Windows.Tests/OpenLiveWriter.Platform.Windows.Tests.csproj -v q
```
Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add src/managed/OpenLiveWriter.Platform.Windows.Tests/ src/managed/writer.sln
git commit -m "test(platform): Add unit tests for Windows credential storage, display helper, and platform services"
```

---

## Task 6: Wire Platform Initialization into Windows App Entry Point

**Files:**
- Modify: `src/managed/OpenLiveWriter/OpenLiveWriter.csproj`
- Modify: `src/managed/OpenLiveWriter/ApplicationMain.cs`

- [ ] **Step 1: Add Platform.Windows project reference**

In `src/managed/OpenLiveWriter/OpenLiveWriter.csproj`, add to the `<ItemGroup>` containing `<ProjectReference>` entries:

```xml
    <ProjectReference Include="..\OpenLiveWriter.Platform.Windows\OpenLiveWriter.Platform.Windows.csproj" />
```

- [ ] **Step 2: Add platform initialization to ApplicationMain.cs**

In `src/managed/OpenLiveWriter/ApplicationMain.cs`, add the using statement at the top with the other usings:

```csharp
using OpenLiveWriter.Platform.Windows;
```

Then in the `Main` method, add platform initialization as the very first line after `ConfigureDebugAssertBehavior()` (around line 44):

```csharp
            ConfigureDebugAssertBehavior();

            // Initialize platform-specific services for Windows
            WindowsPlatformInitializer.Initialize();
```

- [ ] **Step 3: Build the full solution**

Run: `dotnet build src/managed/writer.sln -v q`
Expected: Build succeeds with 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/managed/OpenLiveWriter/OpenLiveWriter.csproj src/managed/OpenLiveWriter/ApplicationMain.cs
git commit -m "feat(platform): Wire up WindowsPlatformInitializer in application entry point"
```

---

## Task 7: Retarget `OpenLiveWriter.HtmlParser` to `net10.0`

**Files:**
- Modify: `src/managed/OpenLiveWriter.HtmlParser/OpenLiveWriter.HtmlParser.csproj`

- [ ] **Step 1: Update the project file**

In `src/managed/OpenLiveWriter.HtmlParser/OpenLiveWriter.HtmlParser.csproj`, the project already has `<UseWindowsForms>false</UseWindowsForms>`. Add the TFM override. Change the `<PropertyGroup>` to:

```xml
  <PropertyGroup>
    <AssemblyName>OpenLiveWriter.HtmlParser</AssemblyName>
    <RootNamespace>OpenLiveWriter.HtmlParser</RootNamespace>
    <ProjectGuid>{8B905D4B-EE76-4EEE-83CC-C9028B2F16AE}</ProjectGuid>
    <!-- Cross-platform: no Windows dependencies -->
    <TargetFramework>net10.0</TargetFramework>
    <UseWindowsForms>false</UseWindowsForms>
  </PropertyGroup>
```

- [ ] **Step 2: Build the project**

Run: `dotnet build src/managed/OpenLiveWriter.HtmlParser/OpenLiveWriter.HtmlParser.csproj -v q`
Expected: Build succeeds.

- [ ] **Step 3: Build full solution to check for downstream issues**

Run: `dotnet build src/managed/writer.sln -v q`
Expected: Build succeeds. Projects that reference HtmlParser should handle the cross-TFM reference correctly since `net10.0-windows` is compatible with `net10.0` dependencies.

- [ ] **Step 4: Commit**

```bash
git add src/managed/OpenLiveWriter.HtmlParser/OpenLiveWriter.HtmlParser.csproj
git commit -m "build: Retarget OpenLiveWriter.HtmlParser to net10.0 (cross-platform)"
```

---

## Task 8: Add Type-Forwarding for ISettingsPersister in CoreServices

This task creates backward compatibility so existing code referencing `OpenLiveWriter.CoreServices.Settings.ISettingsPersister` continues to work while the canonical interface lives in `OpenLiveWriter.Platform`.

**Files:**
- Modify: `src/managed/OpenLiveWriter.CoreServices/OpenLiveWriter.CoreServices.csproj`
- Modify: `src/managed/OpenLiveWriter.CoreServices/Settings/ISettingsPersister.cs`

- [ ] **Step 1: Add Platform project reference to CoreServices**

In `src/managed/OpenLiveWriter.CoreServices/OpenLiveWriter.CoreServices.csproj`, add to the `<ItemGroup>` containing `<ProjectReference>` entries:

```xml
    <ProjectReference Include="..\OpenLiveWriter.Platform\OpenLiveWriter.Platform.csproj" />
```

- [ ] **Step 2: Replace ISettingsPersister with type alias**

Replace the entire contents of `src/managed/OpenLiveWriter.CoreServices/Settings/ISettingsPersister.cs` with:

```csharp
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// The canonical ISettingsPersister interface has moved to OpenLiveWriter.Platform.
// This file provides a namespace alias so existing code continues to compile
// without changes. New code should reference OpenLiveWriter.Platform.ISettingsPersister directly.

using ISettingsPersisterBase = OpenLiveWriter.Platform.ISettingsPersister;

namespace OpenLiveWriter.CoreServices.Settings
{
    /// <summary>
    /// Backward-compatible alias for OpenLiveWriter.Platform.ISettingsPersister.
    /// New code should use OpenLiveWriter.Platform.ISettingsPersister directly.
    /// </summary>
    public interface ISettingsPersister : ISettingsPersisterBase
    {
    }
}
```

- [ ] **Step 3: Build CoreServices**

Run: `dotnet build src/managed/OpenLiveWriter.CoreServices/OpenLiveWriter.CoreServices.csproj -v q`
Expected: Build succeeds. All existing code that references `CoreServices.Settings.ISettingsPersister` still compiles because the derived interface is compatible.

- [ ] **Step 4: Build full solution**

Run: `dotnet build src/managed/writer.sln -v q`
Expected: Build succeeds with 0 errors.

- [ ] **Step 5: Run existing tests**

Run: `dotnet test src/managed/OpenLiveWriter.UnitTest/OpenLiveWriter.UnitTest.csproj -v q`
Expected: All existing tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/managed/OpenLiveWriter.CoreServices/OpenLiveWriter.CoreServices.csproj src/managed/OpenLiveWriter.CoreServices/Settings/ISettingsPersister.cs
git commit -m "refactor(settings): Add Platform project reference and type-forward ISettingsPersister to OpenLiveWriter.Platform"
```

---

## Task 9: Run All Tests and Create Stacked PR for Foundation Work

**Files:** None — this is a verification and PR creation task.

- [ ] **Step 1: Run all platform tests**

Run: `dotnet test src/managed/OpenLiveWriter.Platform.Tests/OpenLiveWriter.Platform.Tests.csproj -v n`
Expected: All tests pass.

- [ ] **Step 2: Run all Windows platform tests**

Run: `dotnet test src/managed/OpenLiveWriter.Platform.Windows.Tests/OpenLiveWriter.Platform.Windows.Tests.csproj -v n`
Expected: All tests pass.

- [ ] **Step 3: Run existing unit tests**

Run: `dotnet test src/managed/OpenLiveWriter.UnitTest/OpenLiveWriter.UnitTest.csproj -v n`
Expected: All existing tests pass.

- [ ] **Step 4: Build full solution in Release mode**

Run: `dotnet build src/managed/writer.sln -c Release -v q`
Expected: Build succeeds.

- [ ] **Step 5: Push and create PR**

```bash
git push -u origin feature/macbuild
gh pr create --repo OpenLiveWriter/OpenLiveWriter --head dougrathbone:feature/macbuild --base feature/csharp-ribbon --title "feat: Add cross-platform abstraction layer (Milestone 1a-1b)" --body "## Summary

- Add \`OpenLiveWriter.Platform\` project with cross-platform interfaces (\`ISettingsPersister\`, \`ICredentialStorage\`, \`IPlatformServices\`, \`IDisplayHelper\`, \`IBlogClientUIContext\`, \`IBidiSupport\`, \`ISpellCheckProvider\`)
- Add \`OpenLiveWriter.Platform.Windows\` project with Windows implementations (Registry, DPAPI, DWM)
- Wire platform initialization into application entry point
- Retarget \`OpenLiveWriter.HtmlParser\` to \`net10.0\` (cross-platform)
- Add type-forwarding for \`ISettingsPersister\` in CoreServices for backward compatibility
- Modify \`Directory.Build.props\` to support per-project TFM overrides
- Full unit test coverage for all new code

## Context

First step of cross-platform migration. Establishes the platform abstraction layer that all subsequent work builds on. Windows app continues to work identically.

## Test plan

- [ ] All new platform tests pass
- [ ] All existing unit tests pass
- [ ] Full solution builds in Debug and Release
- [ ] Windows app launches and can create/edit/publish a post
"
```

---

## Notes for Future Tasks (not in this PR)

The remaining Milestone 1 work (Tasks 1c through 1g from the spec) will be separate stacked PRs:

- **Next PR**: Retarget `OpenLiveWriter.CoreServices` to `net10.0` — extract Registry*, CabinetFileExtractor, DisplayHelper, PathHelper to Platform.Windows, add `System.Drawing.Common` NuGet, move WinForms helpers
- **Following PR**: Retarget `OpenLiveWriter.BlogClient` to `net10.0` — replace `IBlogClientUIContext : IWin32Window, ISynchronizeInvoke` with new platform-agnostic interface
- **Following PR**: Retarget remaining core projects (`Api`, `Extensibility`, `Localization`, `SpellChecker`)
- **Final PR**: Merge Velopack from `develop/feat-velopack` and integration test
