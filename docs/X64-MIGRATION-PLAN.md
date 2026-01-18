# x64 Migration Plan for Open Live Writer

## Executive Summary

This document outlines the specific considerations, challenges, and steps required to migrate Open Live Writer from x86 (Win32) to x64 (64-bit) architecture. The migration is complicated by dependencies on unmanaged C++ components, COM interop, and legacy MSHTML interfaces.

**Note:** WebView2 migration work is actively being done in the [`feature/webview2` branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2). See that branch for detailed documentation on the MSHTML → WebView2 replacement, which is a **prerequisite** for x64 migration.

## Current State

### Architecture Overview

Open Live Writer currently builds as a **32-bit (x86) application** with the following characteristics:

- **Managed Code**: .NET Framework 4.7.2 C# projects compiled for AnyCPU but running as 32-bit due to native dependencies
- **Unmanaged Code**: C++ Ribbon component (`OpenLiveWriter.Ribbon.vcxproj`) compiled as Win32 DLL
- **Platform Dependencies**: MSHTML COM components, Windows Ribbon Framework
- **Default Build**: All projects default to x86/Win32 platforms

### Current Build Configuration

**C++ Projects:**
- `OpenLiveWriter.Ribbon.vcxproj` - Win32 only (no x64 configuration exists)
- Platform Toolset: v143 (VS2022) with fallback support for v142/v144/v145
- Target: Windows 10 SDK

**C# Projects:**
- Platform: AnyCPU (would prefer x86 when mixed with native components)
- Framework: .NET Framework 4.7.2
- Mixed-mode dependencies force 32-bit process

**Build Settings:**
- `writer.build.settings` defines build architecture logic
- Default: `i386` (line 45: `<DefaultBuildArchitecture>i386</DefaultBuildArchitecture>`)
- Support exists for `amd64` architecture mapping to `x64` platform target (lines 50, 64)

---

## Why x64 Migration is Important

### Technical Benefits

1. **Memory Addressing**: Break the 2GB per-process memory limit for handling large blog posts with many images
2. **Performance**: 64-bit processors can use additional registers and optimizations
3. **Modern OS Integration**: Better integration with 64-bit Windows features and APIs
4. **Future-Proofing**: Industry trend toward deprecating 32-bit applications
5. **.NET 10 Compatibility**: Modern .NET versions provide better x64 support and tooling

### Strategic Benefits

1. **WebView2 Performance**: The Chromium-based WebView2 engine performs better as 64-bit
2. **Large Document Handling**: Edit blog posts with more images and embedded media
3. **Professional Image**: 64-bit signals modern, maintained software
4. **ARM64 Preparation**: x64 experience helps with future ARM64 native builds via emulation improvements

---

## Migration Blockers and Challenges

### 1. Unmanaged C++ Components

**OpenLiveWriter.Ribbon (Win32 DLL)**

**Current State:**
- Project file has only Win32 platform configuration (no x64)
- Uses Windows Ribbon Framework (UICC.exe compiler)
- Built as DLL loaded by managed code via P/Invoke

**Migration Requirements:**
- ✅ Add x64 platform configuration to `.vcxproj`
- ✅ Ensure Windows SDK supports x64 Ribbon Framework
- ✅ Update P/Invoke signatures in C# if needed (IntPtr sizing)
- ⚠️ Test UICC.exe ribbon compiler with x64 output
- ⚠️ Verify DLL loading mechanism handles platform-specific paths

**Steps:**
```xml
<!-- Add to OpenLiveWriter.Ribbon.vcxproj -->
<ItemGroup Label="ProjectConfigurations">
  <!-- Existing Win32 configs -->
  <ProjectConfiguration Include="Debug|x64">
    <Configuration>Debug</Configuration>
    <Platform>x64</Platform>
  </ProjectConfiguration>
  <ProjectConfiguration Include="Release|x64">
    <Configuration>Release</Configuration>
    <Platform>x64</Platform>
  </ProjectConfiguration>
</ItemGroup>

<!-- Add configuration property groups -->
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|x64'" Label="Configuration">
  <ConfigurationType>DynamicLibrary</ConfigurationType>
  <UseDebugLibraries>true</UseDebugLibraries>
  <PlatformToolset Condition="'$(PlatformToolset)'==''">v143</PlatformToolset>
  <CharacterSet>Unicode</CharacterSet>
</PropertyGroup>
<!-- Same for Release|x64 -->
```

---

### 2. MSHTML COM Interop (CRITICAL BLOCKER)

**Current State:**
- MSHTML.dll is 32-bit only on most systems
- Extensive use of 32-bit COM interfaces via OpenLiveWriter.Mshtml assembly
- ~32 custom-shimmed COM interfaces with explicit GUIDs and vtable layouts

**Problem:**
The MSHTML editing stack (`IMarkupServices`, `IMarkupPointer`, `IDisplayServices`, etc.) is fundamentally 32-bit:
- COM vtable layouts differ between x86 and x64
- MSHTML.dll may not be available or functional as 64-bit
- Third-party 32-bit ActiveX controls can't load in 64-bit process

**Solutions:**

**Option A: Dual-Process Architecture (Temporary Bridge)**
- Main application runs as x64
- Host MSHTML editor in separate 32-bit child process
- Use IPC (named pipes, WCF) for communication
- ❌ Cons: Complex, performance overhead, maintenance burden

**Option B: WebView2 Migration (RECOMMENDED)**
- Replace MSHTML with WebView2 (Edge/Chromium)
- WebView2 fully supports x64
- Already implemented in `feature/webview2` branch
- ✅ Pros: Modern, maintained, native x64 support, better performance
- ✅ Prerequisite: Merge WebView2 editor work BEFORE x64 migration

**Decision:**
🎯 **Block x64 migration until WebView2 migration is complete**. Attempting x64 with MSHTML is not viable.

---

### 3. Build System Updates

**writer.build.settings Changes:**

Current architecture logic defaults to i386. Need to:

1. Add x64 configuration support:
```xml
<!-- In writer.build.settings -->
<PropertyGroup>
  <!-- Support x64 as first-class platform -->
  <BuildArchitecture Condition="'$(x64)' == '1'">amd64</BuildArchitecture>
  <!-- Existing mappings -->
  <BuildArchitecture Condition="'$(amd64)' == '1'">amd64</BuildArchitecture>
  <BuildArchitecture Condition="'$(_BuildArch)' == 'x64'">amd64</BuildArchitecture>
</PropertyGroup>

<PropertyGroup Condition="'$(PlatformSpecificBuild)' == 'true'">
  <PlatformTarget>AnyCPU</PlatformTarget>
  <PlatformTarget Condition="'$(BuildArchitecture)' == 'amd64'">x64</PlatformTarget>
  <PlatformTarget Condition="'$(BuildArchitecture)' == 'i386'">x86</PlatformTarget>
</PropertyGroup>
```

2. Update build scripts (`build.ps1`, `build.cmd`):
```powershell
# Add parameter
param(
    [string]$Platform = "x86"  # or "x64"
)

# Pass to MSBuild
& msbuild /p:Platform=$Platform /p:Configuration=$Configuration
```

---

### 4. C# Platform Target Configuration

**All .csproj Files:**

Currently most projects use `AnyCPU` which becomes x86 when mixed with native dependencies.

**Required Changes:**
- Projects with native dependencies: Explicit `x86` or `x64` platform configurations
- Pure managed projects: Can remain `AnyCPU` with `Prefer32Bit=false`

**Projects needing platform-specific configs:**
- `OpenLiveWriter.PostEditor` (hosts Ribbon DLL)
- `OpenLiveWriter.ApplicationFramework` (if it loads native components)
- Any project with P/Invoke to platform-specific DLLs

```xml
<!-- Example .csproj changes -->
<PropertyGroup Condition="'$(Platform)' == 'x64'">
  <PlatformTarget>x64</PlatformTarget>
  <OutputPath>bin\x64\$(Configuration)\</OutputPath>
</PropertyGroup>
<PropertyGroup Condition="'$(Platform)' == 'x86'">
  <PlatformTarget>x86</PlatformTarget>
  <OutputPath>bin\x86\$(Configuration)\</OutputPath>
</PropertyGroup>
```

---

### 5. P/Invoke Signature Corrections

**Potential Issues:**
- `IntPtr` size differs (4 bytes in x86, 8 bytes in x64)
- Structure packing and alignment may differ
- Calling conventions (rare on x64, most use default)

**Affected Areas:**
- Windows API calls (Win32)
- Ribbon Framework interop
- Any native DLL interop

**Audit Required:**
Search codebase for:
```bash
grep -r "DllImport" --include="*.cs" | wc -l
```

**Common Fixes:**
```csharp
// Ensure IntPtr is used for pointers/handles (not int or long)
[DllImport("user32.dll")]
static extern IntPtr GetDC(IntPtr hWnd);  // ✅ Correct

// Use UIntPtr for size_t parameters
[DllImport("kernel32.dll")]
static extern bool VirtualProtect(IntPtr addr, UIntPtr size, uint protect, out uint old);

// Check structure layouts
[StructLayout(LayoutKind.Sequential, Pack = 8)]  // Pack may differ on x64
struct MyStruct { ... }
```

---

### 6. COM Interop Structure Layouts

**MSHTML Interfaces:**
All COM interfaces in `OpenLiveWriter.Mshtml/Mshtml_Interop/*.cs` use:
```csharp
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("...")]
```

**Considerations:**
- .NET handles vtable layout automatically for COM interfaces
- Should be architecture-agnostic IF the underlying COM component supports x64
- BUT: MSHTML doesn't reliably support x64, hence the WebView2 requirement

**Post-WebView2:**
- WebView2 COM interfaces are x64-compatible
- Minimal changes needed to WebView2 interop

---

### 7. Third-Party Dependencies

**NuGet Packages:**
Audit for platform-specific packages:
```bash
find . -name "packages.config" -exec grep -H "x86\|x64" {} \;
```

**Known Dependencies:**
- Windows Ribbon Framework - need x64 SDK components
- WebView2 Runtime - fully supports x64 ✅
- Any spell-checking libraries - verify x64 support

**Action Items:**
- [ ] Inventory all NuGet packages
- [ ] Check each for x64 support
- [ ] Replace or remove x86-only dependencies

---

## Migration Phases

### Phase 0: Prerequisites (BLOCKER)
**Timeline:** Complete before any x64 work

- [x] Complete WebView2 migration (see [`feature/webview2` branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2))
- [ ] Merge WebView2 branch to main
- [ ] Validate WebView2 editor in x86 build
- [ ] Remove or isolate remaining MSHTML dependencies

**Why:** MSHTML is the primary x64 blocker. Must be replaced first.

**Current Status:** The `feature/webview2` branch has a fully functional WebView2 editor with 34+ commits and comprehensive documentation. See:
- `docs/WEBVIEW2-BRANCH-REPORT.md` - Feature status and accomplishments
- `docs/WEBVIEW2-EDITOR-MIGRATION-PLAN.md` - Detailed migration plan
- `docs/NET10-MIGRATION-PLAN.md` - .NET 10 migration strategy

---

### Phase 1: Build Infrastructure (Week 1)
**Goal:** Enable x64 builds without breaking x86

**Tasks:**
1. Add x64 configurations to all projects
   - [ ] Update `OpenLiveWriter.Ribbon.vcxproj` with x64 platform
   - [ ] Add x64 configs to solution file
   - [ ] Update `writer.build.settings` for x64 support

2. Update build scripts
   - [ ] Modify `build.ps1` to accept `-Platform` parameter
   - [ ] Update `build.cmd` wrapper
   - [ ] Create `build-x64.ps1` convenience script

3. Verify toolchain
   - [ ] Confirm Windows SDK has x64 components
   - [ ] Test UICC.exe with x64 output
   - [ ] Verify MSBuild x64 targets installed

**Validation:**
```powershell
.\build.ps1 -Platform x64 -Configuration Debug
# Should produce x64 binaries in bin\x64\Debug\
```

---

### Phase 2: Native Code Migration (Week 2)
**Goal:** Get C++ components building as x64

**Tasks:**
1. Configure OpenLiveWriter.Ribbon for x64
   - [ ] Add x64 platform configurations
   - [ ] Update compiler settings (preprocessor defines may differ)
   - [ ] Test ribbon compiler output

2. Build and test
   - [ ] Compile as x64
   - [ ] Verify DLL size and exports
   - [ ] Test loading in x64 test harness

3. Fix any x64-specific issues
   - [ ] Check pointer arithmetic
   - [ ] Verify structure sizes
   - [ ] Test all ribbon functionality

**Validation:**
```bash
dumpbin /headers bin\x64\Release\OpenLiveWriter.Ribbon.dll | grep machine
# Should show: 8664 machine (x64)
```

---

### Phase 3: Managed Code Migration (Week 2-3)
**Goal:** Update C# projects for x64

**Tasks:**
1. Update project files
   - [ ] Add explicit x64 platform targets where needed
   - [ ] Update output paths
   - [ ] Fix any AnyCPU issues

2. Audit P/Invoke calls
   - [ ] Search for all DllImport attributes
   - [ ] Verify IntPtr usage (not int for pointers)
   - [ ] Check structure layouts
   - [ ] Test each interop call

3. Update dependent assemblies
   - [ ] Rebuild all projects as x64
   - [ ] Verify assembly references resolve correctly
   - [ ] Check for platform-specific assembly loading

**Validation:**
```powershell
# Check assemblies are x64
Get-ChildItem -Recurse -Filter "*.dll" | ForEach-Object {
    [Reflection.Assembly]::LoadFile($_.FullName).GetName().ProcessorArchitecture
}
# Should show: Amd64 for platform-specific, MSIL for AnyCPU
```

---

### Phase 4: Integration Testing (Week 3-4)
**Goal:** Verify application works end-to-end as x64

**Tasks:**
1. Functional testing
   - [ ] Launch application
   - [ ] Test WebView2 editor (formatting, images, links)
   - [ ] Test ribbon UI (all buttons, dialogs)
   - [ ] Test blog posting
   - [ ] Test plugin system
   - [ ] Test image editing

2. Performance testing
   - [ ] Measure startup time vs. x86
   - [ ] Test large document handling (should improve)
   - [ ] Monitor memory usage
   - [ ] Profile editor performance

3. Compatibility testing
   - [ ] Test on Windows 10 x64
   - [ ] Test on Windows 11 x64
   - [ ] Verify installer works
   - [ ] Test upgrade from x86 version (side-by-side or replace?)

**Test Matrix:**
| Feature | x86 Baseline | x64 Result | Notes |
|---------|-------------|-----------|-------|
| App Launch | 2s | ___ | |
| WebView2 Editor | Works | ___ | |
| Image Insert | Works | ___ | |
| Ribbon UI | Works | ___ | |
| Blog Post | Works | ___ | |
| Large Doc (50 images) | Slow/OOM? | ___ | Should improve |

---

### Phase 5: Deployment (Week 4)
**Goal:** Ship x64 builds to users

**Tasks:**
1. Installer updates
   - [ ] Update installer to detect x64 Windows
   - [ ] Decide: x64-only, x86-only, or both?
   - [ ] Update Program Files path (Program Files vs. Program Files (x86))
   - [ ] Test installer upgrade scenarios

2. Distribution
   - [ ] Create x64 NuGet packages
   - [ ] Update Chocolatey package
   - [ ] Build separate x64 installer or universal?
   - [ ] Update download page documentation

3. Documentation
   - [ ] Update build instructions
   - [ ] Note x64 system requirements
   - [ ] Document any breaking changes
   - [ ] Create migration guide for users

**Installer Strategy Options:**

**Option A: x64-Only (Recommended)**
- Simplest: one installer, one build
- Aligns with industry trend
- Windows 10/11 x64 is dominant platform
- ✅ Recommended for new major version

**Option B: Dual Installers**
- Maintain both x86 and x64 builds
- More testing and maintenance burden
- Users might install wrong version
- ❌ Not recommended unless specific x86 need exists

**Option C: Universal Installer**
- Detect OS architecture and install appropriate version
- Complex installer logic
- ⚠️ Only if must support both

---

## Technical Considerations

### Memory Improvements

**x86 Limitations:**
- ~2GB addressable memory per process (3GB with /LARGEADDRESSAWARE)
- WebView2 Chromium process can consume significant memory
- Large blog posts with images hit limits

**x64 Benefits:**
- Theoretical limit: 8TB on Windows (practical: limited by RAM)
- WebView2 can use more memory for rendering
- Better handling of large documents

**Expected Impact:**
- Can edit blog posts with 100+ high-res images
- Faster rendering of complex layouts
- Better multi-tab performance (if added)

---

### Performance Considerations

**Potential Improvements:**
- ✅ More CPU registers available (16 vs. 8 general-purpose on x64)
- ✅ Better SIMD performance for image processing
- ✅ Native 64-bit pointer arithmetic (no thunking)
- ✅ WebView2 Chromium optimized for x64

**Potential Regressions:**
- ⚠️ Larger pointer sizes (8 vs. 4 bytes) increase memory footprint slightly
- ⚠️ Slightly larger code size
- ⚠️ May lose some cache efficiency

**Expected Net Result:**
~5-15% performance improvement on x64 systems for typical workloads, with significant improvements for memory-intensive tasks.

---

### Debugging and Diagnostics

**Tools for x64 Development:**
- Visual Studio 2022: Full x64 debugging support
- WinDbg (x64): For native debugging
- Process Monitor: Verify DLL loading from correct paths
- Dependency Walker (x64): Check x64 DLL dependencies

**Common x64 Debugging Issues:**
- Mixed-mode debugging may be slower
- Ensure symbols for x64 are available
- Check for x64-specific race conditions (different memory layout)

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| MSHTML x64 incompatibility | HIGH | CRITICAL | **Already mitigated**: Use WebView2 instead |
| Ribbon DLL x64 build issues | MEDIUM | HIGH | Test early, use Windows SDK samples as reference |
| P/Invoke signature bugs | MEDIUM | MEDIUM | Systematic audit, extensive testing |
| Performance regression | LOW | MEDIUM | Benchmark before/after, profile hotspots |
| Third-party dependency breaks | LOW | HIGH | Audit early, find x64 alternatives |
| User confusion (two versions) | MEDIUM | LOW | Ship x64-only OR auto-detect in installer |

---

## Success Criteria

### Build Success
- ✅ Clean x64 build with zero errors and zero warnings
- ✅ All projects compile as x64
- ✅ Output binaries verified as x64 (dumpbin/CorFlags)

### Functional Success
- ✅ Application launches and shows UI
- ✅ All ribbon buttons functional
- ✅ WebView2 editor works (format, images, links, source view)
- ✅ Can create, edit, save, and publish blog posts
- ✅ Plugins load and function
- ✅ No crashes in typical workflows

### Performance Success
- ✅ Startup time <= x86 baseline
- ✅ Can handle 50+ image blog posts without memory errors
- ✅ Editor performance >= x86 baseline

### Deployment Success
- ✅ Installer creates working installation
- ✅ Upgrade from x86 preserves settings and data
- ✅ Uninstall is clean

---

## Rollback Plan

If critical x64 issues are discovered late:

1. **Keep x86 builds working in parallel** during development
2. Ship x86 as primary with x64 as "beta" initially
3. Use feature flags to enable/disable x64-specific code paths
4. Monitor telemetry for x64-specific crashes

**Rollback Triggers:**
- >5% crash rate increase on x64 vs. x86
- Critical functionality broken with no quick fix
- Major performance regression (>20% slower)

---

## Timeline and Dependencies

### Critical Path

```
Prerequisites (WebView2 migration) - BLOCKS ALL
    ↓
Phase 1: Build Infrastructure (1 week)
    ↓
Phase 2: Native Code + Phase 3: Managed Code (2 weeks, parallel)
    ↓
Phase 4: Integration Testing (1-2 weeks)
    ↓
Phase 5: Deployment (1 week)
```

**Total Estimated Time:** 5-6 weeks (assuming WebView2 complete)

### Dependencies

**Hard Dependencies (BLOCKERS):**
1. ✅ WebView2 migration complete - **CRITICAL**
2. ✅ Visual Studio 2022 with C++ x64 tools
3. ✅ Windows 10 SDK (x64 components)

**Soft Dependencies:**
1. .NET Framework 4.7.2 runtime (has x64 support)
2. WebView2 Runtime x64 (auto-installs)

---

## Post-Migration Opportunities

Once x64 is stable:

1. **ARM64 Support**: x64 codebase can run on ARM64 Windows via emulation, native ARM64 is easier from x64 base
2. **.NET 10 Migration**: Modern .NET has better x64 tooling and performance
3. **Advanced Features**: Use extra memory for local caching, better image processing
4. **Modern APIs**: Can use newer Windows APIs that prefer/require x64

---

## Conclusion

The x64 migration is **feasible but BLOCKED by MSHTML**. The recommended path is:

1. ✅ **Complete WebView2 migration first** (already done in `feature/webview2` branch)
2. Merge WebView2 to main and stabilize
3. Begin x64 migration using this plan
4. Target 5-6 week timeline for x64 after WebView2 merge

**Do NOT attempt x64 migration with MSHTML** - it will fail due to COM interop issues.

---

## References

- [feature/webview2 branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2) - Active WebView2 migration work
- [Windows SDK for x64 Development](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)
- [.NET Framework x64 Support](https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/)
- [Visual Studio C++ x64 Development](https://docs.microsoft.com/en-us/cpp/build/configuring-programs-for-64-bit-visual-cpp)
- OpenLiveWriter build system: `writer.build.settings`, `build.ps1`

---

*Last Updated: Based on analysis of current codebase state and WebView2 migration context.*
