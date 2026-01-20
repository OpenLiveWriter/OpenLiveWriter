# .NET 10 Migration - Current Status

**Date**: January 19, 2026 (Updated)  
**Status**: ✅ FUNCTIONAL - Both Debug and Release builds work!

## Current State

### What's Working
- [x] All 19 core projects compile on .NET 10
- [x] SDK-style projects with central package management
- [x] Velopack integration (replaced Squirrel)
- [x] CustomMarshalers shim for MSHTML interop
- [x] Main executable (OpenLiveWriter.exe) builds
- [x] **PostEditorForm** - Main editor window displays
- [x] **Ribbon UI** - Full ribbon with commands
- [x] **WebView2 Editor** - Content editing works
- [x] **Debug build** - Works after Debug.Fail fixes
- [x] **Release build** - Works

### Previously Stubbed Components (Now Restored)
| Component | Status | Notes |
|-----------|--------|-------|
| PostEditorForm | ✅ Working | Stub deleted, real form works |
| PostEditorMainControl | ✅ Working | Full editor control |
| WeblogCommandManager | ✅ Working | Commands functional |
| BlogProviderButtons | ✅ Working | Provider buttons show |
| ContentEditor | ✅ Working | HTML editing works |
| ContextMenu→ContextMenuStrip | ✅ Fixed | .NET 10 compat fix |

### Features Needing Testing
| Feature | Status | Notes |
|---------|--------|-------|
| Create new post | ❓ Untested | Editor shows, need to verify |
| Save draft | ❓ Untested | |
| Publish to blog | ❓ Untested | User has blog config in registry |
| Blog wizard | ❓ Untested | For new blog setup |
| Preferences dialog | ❓ Untested | |
| Image insertion | ❓ Untested | |
| Spell check | ❓ Untested | |
| Auto-update | ❓ Untested | Velopack integration |

---

## Restoration Plan (Now Complete)

### ~~Phase 1: Core Editor~~ ✅ DONE
- ~~Re-enable BlogProviderButtons~~
- ~~Re-enable PostEditorMainControl.cs~~
- ~~Re-enable WeblogCommandManager.cs~~
- ~~Re-enable PostEditorForm.cs~~
- ~~Delete PostEditorFormStub.cs~~

### Phase 2: Testing
Test all major features with existing blog configuration.

---

## Critical .NET 10 Breaking Changes Fixed

| Issue | Fix Applied | Location |
|-------|-------------|----------|
| Squirrel incompatible | Replaced with Velopack | ApplicationMain.cs |
| CustomMarshalers removed | Created shim in Interop.Mshtml | CustomMarshalers.cs |
| System.Web.Services | Created stub for SharePoint SOAP | N/A |
| TaskDialog ambiguity | Fully qualified types | Various |
| **ContextMenu not supported** | Changed to ContextMenuStrip | HtmlSourceEditorControl.cs |
| **Debug.Fail crashes app** | Replaced with Debug.WriteLine | ResourcedPropertyLoader.cs |

### Debug.Fail/Assert Issue (CRITICAL)
In .NET Core/.NET 5+, `Debug.Fail` and `Debug.Assert` failures **terminate the process** instead of showing a dialog like in .NET Framework. 

**Root Cause**: The process terminates before custom TraceListeners can intercept the failure.

**Solution**: Replace `Debug.Fail` calls with `Debug.WriteLine` or remove them. ~350 calls exist in codebase; fix as encountered during testing.

**Files Fixed**:
- `src/managed/OpenLiveWriter.CoreServices/ResourcedPropertyLoader.cs` - Lines 127, 132

---

## MSHTML Status

**Decision**: WebView2 is the primary editor. MSHTML interop kept for compatibility.

Current state:
- WebView2 is integrated via `OpenLiveWriter.WebView2Shim`
- `OpenLiveWriter.Interop.Mshtml` kept with CustomMarshalers shim
- `OpenLiveWriter.Mshtml` kept but may be removable

## Next Steps

1. **Test Core Functionality**
   - Create and save a new post
   - Open existing posts
   - Publish to a blog (user has existing config)

2. **Test Configuration**
   - Blog wizard for new blog setup
   - Preferences dialog
   
3. **Fix Debug.Fail Calls as Encountered**
   - ~350 calls in codebase
   - Only fix those that actually trigger during normal use
   
4. **Clean Up Debug Tracing**
   - Remove excessive `[OLW-DEBUG]` logging added during troubleshooting

## Build Commands

```bash
# Quick build (Debug)
dotnet build src\managed\OpenLiveWriter\OpenLiveWriter.csproj -c Debug

# Quick build (Release)  
dotnet build src\managed\OpenLiveWriter\OpenLiveWriter.csproj -c Release

# Full build with native Ribbon DLL
.\build.cmd Debug x64
.\build.cmd Release x64

# Run the app
.\run.cmd
# or directly:
.\src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe
```

## Debug Tracing

Use DbgView with filter `[OLW-DEBUG]` to see debug output.

```csharp
System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] Your message here");
```

