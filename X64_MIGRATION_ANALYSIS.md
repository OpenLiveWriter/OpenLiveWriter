# OpenLiveWriter x64 Migration Analysis

**Date:** 2026-01-18  
**Version:** 1.0  
**Status:** Comprehensive Analysis Complete

## Executive Summary

This document provides a comprehensive analysis of x64 compatibility issues in the OpenLiveWriter codebase. The analysis identified **28 high-priority P/Invoke issues**, **35+ unsafe pointer cast locations**, minimal native DLL dependencies, and **3 registry paths** requiring Wow6432Node consideration.

### Key Findings
- **Critical Issues:** 9 high-severity P/Invoke declarations
- **Platform-dependent Code:** Multiple int/IntPtr conversion issues
- **Native Dependencies:** Primarily Windows system DLLs (all x64 compatible)
- **Registry Concerns:** Limited hardcoded paths, mostly using managed APIs

---

## 1. P/Invoke Declaration Analysis

### 1.1 Critical Issues (HIGH Severity - 9 issues)

#### User32.dll Issues

| File | Line | Function | Current Signature | Recommended Fix | Impact |
|------|------|----------|-------------------|----------------|--------|
| User32.cs | 18 | `GetKeyboardLayout()` | `int GetKeyboardLayout(int dwLayout)` | `IntPtr GetKeyboardLayout(int dwLayout)` | Returns HKL handle - will truncate on x64 |
| User32.cs | 284 | `GetWindowLong()` | `UInt32 GetWindowLong(IntPtr hWnd, int nIndex)` | Use `GetWindowLongPtr()` on x64 | Critical for GWL_WNDPROC, GWL_DLGPROC - returns truncated pointers |
| User32.cs | 291 | `SetWindowLong()` | `int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)` | Use `SetWindowLongPtr()` on x64 | Critical when setting window procedures |
| User32.cs | 426 | `SetWindowProc()` | `int SetWindowProc(IntPtr hWnd, IntPtr pfnWndProc)` | Return `IntPtr` instead of `int` | Returns previous window proc pointer |
| User32.cs | 430 | `SetWindowLong()` (2nd) | `int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)` | Return `IntPtr` instead of `int` | Returns previous value which may be pointer |

**Recommended Solution for GetWindowLong/SetWindowLong:**
```csharp
// Add conditional compilation for x64
#if WIN64
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#else
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#endif
```

#### Kernel32.dll Issues

| File | Line | Function | Current Signature | Recommended Fix | Impact |
|------|------|----------|-------------------|----------------|--------|
| Kernel32.cs | 64-68 | `SetProcessWorkingSetSize()` | `bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize)` | Change int parameters to `UIntPtr` (represents SIZE_T) | Memory size values truncated on x64 systems with >2GB |

**Code Location:**
```csharp
// Current (INCORRECT for x64):
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool SetProcessWorkingSetSize(
    IntPtr hProcess,
    int dwMinimumWorkingSetSize,  // ❌ Should be UIntPtr
    int dwMaximumWorkingSetSize   // ❌ Should be UIntPtr
);

// Recommended:
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool SetProcessWorkingSetSize(
    IntPtr hProcess,
    UIntPtr dwMinimumWorkingSetSize,  // ✅ SIZE_T on x64
    UIntPtr dwMaximumWorkingSetSize   // ✅ SIZE_T on x64
);
```

#### Gdi32.dll Issues

| File | Line | Struct/Function | Current Signature | Recommended Fix | Impact |
|------|------|-----------------|-------------------|----------------|--------|
| Gdi32.cs | 190-199 | `BITMAP` struct | `int bmBits` (field at line 198) | Change to `IntPtr bmBits` | Pointer to bitmap bits - truncated on x64 |

**Code Location:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BITMAP
{
    public int bmType;
    public int bmWidth;
    public int bmHeight;
    public int bmWidthBytes;
    public short bmPlanes;
    public short bmBitsPixel;
    public int bmBits;  // ❌ Should be IntPtr (pointer to bitmap data)
}

// Recommended:
[StructLayout(LayoutKind.Sequential)]
public struct BITMAP
{
    public int bmType;
    public int bmWidth;
    public int bmHeight;
    public int bmWidthBytes;
    public short bmPlanes;
    public short bmBitsPixel;
    public IntPtr bmBits;  // ✅ Correct for x64
}
```

#### Shell32.dll Issues

| File | Line | Struct/Function | Current Signature | Recommended Fix | Impact |
|------|------|-----------------|-------------------|----------------|--------|
| Shell32.cs | 521-529 | `DROPFILES` struct | `uint pFiles` (field at line 523) | Change to `UIntPtr pFiles` | File offset that could exceed 32-bit on x64 |

**Code Location:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct DROPFILES
{
    public uint pFiles;  // ❌ Offset to file list - should be UIntPtr
    public POINT pt;
    public int fNC;
    public int fWide;
}

// Recommended:
[StructLayout(LayoutKind.Sequential)]
public struct DROPFILES
{
    public UIntPtr pFiles;  // ✅ Proper size for x64 offsets
    public POINT pt;
    public int fNC;
    public int fWide;
}
```

### 1.2 Medium Severity Issues (12 issues)

These are primarily type consistency issues where the return type should match Windows API conventions (e.g., `bool` instead of `int` for BOOL returns):

| File | Line | Function | Issue |
|------|------|----------|-------|
| User32.cs | 27 | `GetClassName()` | Returns `int`, should return `uint` for count |
| User32.cs | 30 | `GetIconInfo()` | Returns `int`, should return `bool` (BOOL) |
| User32.cs | 400 | `TrackPopupMenu()` | Returns `int`, should return `uint` for menu ID |
| User32.cs | 633 | `DrawMenuBar()` | Returns `int`, should return `bool` |
| User32.cs | 636 | `SetMenuInfo()` | Returns `int`, should return `bool` |
| Kernel32.cs | 196 | `UnmapViewOfFile()` | Returns `int`, should return `bool` |
| Kernel32.cs | 246 | `ReleaseMutex()` | Returns `int`, should return `bool` |
| Kernel32.cs | 264 | `SetEvent()` | Returns `int`, should return `bool` |
| Kernel32.cs | 267 | `ResetEvent()` | Returns `int`, should return `bool` |
| Kernel32.cs | 276 | `CloseHandle()` | Returns `int`, should return `bool` |
| Kernel32.cs | 292 | `GetDriveType()` | Returns `long`, should return `uint` |
| Shell32.cs | 28-29 | `FindExecutable()` | Returns `IntPtr`, should return `int` |

### 1.3 Correctly Implemented Examples ✅

These files demonstrate correct x64-safe patterns:

| File | Function | Why It's Correct |
|------|----------|------------------|
| Advapi32.cs | `RegOpenKeyEx()` | Uses `UIntPtr` for HKEY handles |
| ComCtl32.cs | `CreateToolbarEx()` | Uses `UIntPtr` for button IDs |
| Multiple | Various | Uses `IntPtr` for window handles (HWND) |

---

## 2. IntPtr vs int/long Issues Analysis

### 2.1 Unsafe Pointer Casts

Found **35+ locations** with potentially unsafe int/IntPtr conversions:

#### Critical Pointer Arithmetic Issues

| File | Line | Code Pattern | Issue |
|------|------|--------------|-------|
| WindowSubClasser.cs | 61 | `User32.SetWindowLong(_window.Handle, GWL.WNDPROC, m_baseWndProc.ToInt32())` | Window proc pointer cast to int - **FAILS ON x64** |
| FileContentsHelper.cs | 76 | `new IntPtr(globalMem.Memory.ToInt32() + Marshal.SizeOf(count))` | Pointer arithmetic using ToInt32() - truncates on x64 |
| FileContentsHelper.cs | 91 | `IntPtr pAddr = new IntPtr(pDescriptors.ToInt32() + (i * totalSize))` | Array offset calculation truncated |
| FileContentsHelper.cs | 92 | `IntPtr pFileNameAddr = new IntPtr(pAddr.ToInt32() + headerSize)` | Structure offset calculation truncated |
| PropVariant.cs | 558 | `valueData = (IntPtr)(int)value` | Explicit int-to-IntPtr cast |

**Most Critical Issue - WindowSubClasser.cs:**
```csharp
// Line 61 - CRITICAL BUG on x64:
User32.SetWindowLong(_window.Handle, GWL.WNDPROC, m_baseWndProc.ToInt32());
// ❌ m_baseWndProc is IntPtr, ToInt32() truncates on x64

// Should be:
User32.SetWindowLongPtr(_window.Handle, GWL.WNDPROC, m_baseWndProc);
// ✅ Using SetWindowLongPtr with full pointer value
```

**FileContentsHelper.cs Pointer Arithmetic Issues:**
```csharp
// Lines 76, 91, 92 - UNSAFE on x64:
new IntPtr(globalMem.Memory.ToInt32() + Marshal.SizeOf(count));  // ❌
IntPtr pAddr = new IntPtr(pDescriptors.ToInt32() + (i * totalSize));  // ❌
IntPtr pFileNameAddr = new IntPtr(pAddr.ToInt32() + headerSize);  // ❌

// Should be using ToInt64() or pointer arithmetic:
new IntPtr(globalMem.Memory.ToInt64() + Marshal.SizeOf(count));  // ✅
IntPtr pAddr = IntPtr.Add(pDescriptors, i * totalSize);  // ✅ Better approach
IntPtr pFileNameAddr = IntPtr.Add(pAddr, headerSize);  // ✅
```

#### Message Parameter Conversions

Multiple files convert `IntPtr` message parameters to `int`:

| File | Line | Pattern | Safety Level |
|------|------|---------|--------------|
| MshtmlControl.cs | 1500-1501 | `msg.WParam.ToInt32()`, `msg.LParam.ToInt32()` | ⚠️ Safe for message values, but type inconsistent |
| TaskDialog.cs | 558, 592, 597, 605 | `wParam.ToInt32()` for button IDs, timer ticks | ✅ Safe - values not pointers |
| CategoryDropDownControl*.cs | 280-281, 352-353 | `m.WParam.ToInt32()`, `m.LParam.ToInt32()` | ⚠️ Depends on message type |

**Analysis:** Most message parameter conversions are safe because the values are IDs/flags, not pointers. However, some messages do pass pointers in LPARAM (e.g., WM_SETTEXT), which would fail on x64 if converted to int.

#### Low-Word/High-Word Extraction

| File | Line | Code | Analysis |
|------|------|------|----------|
| MessageHelper.cs | 20, 30 | `HIWORD()` and `LOWORD()` using `ToInt32()` | ✅ Safe - only extracting 16-bit values |
| AutoCompleteTextbox.cs | 279 | `p.ToInt32() & 0xFFFF` | ✅ Safe - extracting X coordinate |

### 2.2 Pointer Arithmetic Using ToInt64()

Some files correctly use `ToInt64()` instead of `ToInt32()`:

| File | Line | Code | Status |
|------|------|------|--------|
| TaskDialogMarshallers.cs | 53, 72 | `new IntPtr(buffer.ToInt64() + i * elementSize)` | ✅ Correct for x64 |

---

## 3. Native DLL Dependencies

### 3.1 Direct P/Invoke Dependencies

All dependencies are standard Windows system DLLs available in both x86 and x64:

| DLL | Functions | x64 Available | Notes |
|-----|-----------|---------------|-------|
| **user32.dll** | 99 imports | ✅ Yes | Core Windows UI functions |
| **kernel32.dll** | 50 imports | ✅ Yes | Core Windows system functions |
| **gdi32.dll** | 24 imports | ✅ Yes | Graphics Device Interface |
| **propsys.dll** | 23 imports | ✅ Yes | Property system (Vista+) |
| **shell32.dll** | 19 imports | ✅ Yes | Shell functions |
| **wininet.dll** | 20 imports | ✅ Yes | Internet functions |
| **ole32.dll** | 17 imports | ✅ Yes | OLE/COM functions |
| **oleaut32.dll** | 7 imports | ✅ Yes | OLE Automation |
| **shlwapi.dll** | 5 imports | ✅ Yes | Shell lightweight utility |
| **urlmon.dll** | 5 imports | ✅ Yes | URL monikers |
| **uxtheme.dll** | 5 imports | ✅ Yes | Visual styles |
| **advapi32.dll** | 3 imports | ✅ Yes | Advanced Windows APIs |
| **gdiplus.dll** | 3 imports | ✅ Yes | GDI+ |
| **mapi32.dll** | 3 imports | ✅ Yes | MAPI email |
| **psapi.dll** | 3 imports | ✅ Yes | Process Status API |
| **comctl32.dll** | 1 import | ✅ Yes | Common controls |
| **crypt32.dll** | 2 imports | ✅ Yes | Cryptography |
| **dbghelp.dll** | 1 import | ✅ Yes | Debug help |
| **mpr.dll** | 1 import | ✅ Yes | Multiple Provider Router |
| **winmm.dll** | 1 import | ✅ Yes | Multimedia |

**Total:** 20 system DLLs, **all x64 compatible** ✅

### 3.2 Dynamic DLL Loading

| File | Line | DLL Loaded | Purpose | x64 Status |
|------|------|------------|---------|------------|
| DisplayHelper.cs | 394 | `dwmapi.dll` | Desktop Window Manager detection | ✅ Available on x64 Windows |
| PostEditorMainControl.cs | 470 | Native resource DLL (variable) | Localization resources | ⚠️ Need to ensure x64 versions exist |

### 3.3 Managed DLL References

Found 3 pre-built interop DLLs:
- `OpenLiveWriter.Interop.SHDocVw.dll` (appears twice in different locations)
- `shdocvw.dll` reference

**Action Required:** Verify these interop assemblies are AnyCPU or have x64 versions.

---

## 4. Registry Access Analysis

### 4.1 Hardcoded Registry Paths

| File | Line | Registry Path | Wow6432Node Impact |
|------|------|---------------|-------------------|
| Instrumentor.cs | 44 | `SOFTWARE\Microsoft\MSN Apps\SL` | ⚠️ May redirect to Wow6432Node on x64 |
| FileAssociation.cs | 16, 24 | `SOFTWARE\Classes\{extension}` | ⚠️ May redirect to Wow6432Node on x64 |

**Analysis:**
- **Instrumentor.cs**: Reads from HKLM\SOFTWARE. On x64 Windows, a 32-bit process will be redirected to `HKLM\SOFTWARE\Wow6432Node\Microsoft\MSN Apps\SL`
- **FileAssociation.cs**: Writes to HKCU\SOFTWARE\Classes. HKCU is **not redirected** by Wow6432Node, so this should work correctly.

### 4.2 Registry Access Using Managed APIs

The codebase primarily uses .NET's `Microsoft.Win32.Registry` and `RegistryKey` classes, which handle Wow6432Node redirection automatically:

| File | API Usage | Redirection Handling |
|------|-----------|---------------------|
| RegistryHelper.cs | `Registry.LocalMachine`, `Registry.CurrentUser`, etc. | ✅ Automatic |
| Multiple files | `RegistryKey.OpenSubKey()`, `CreateSubKey()` | ✅ Automatic |

**RegistryHelper.cs correctly uses `UIntPtr` for HKEY values** (line 57), which is x64-safe.

### 4.3 Recommendations for Registry Access

1. **For x86-specific registry keys on x64 systems**, explicitly use:
   ```csharp
   RegistryKey key = RegistryKey.OpenBaseKey(
       RegistryHive.LocalMachine, 
       RegistryView.Registry32  // Explicitly access 32-bit registry
   );
   ```

2. **For x64-specific registry keys**:
   ```csharp
   RegistryKey key = RegistryKey.OpenBaseKey(
       RegistryHive.LocalMachine, 
       RegistryView.Registry64  // Explicitly access 64-bit registry
   );
   ```

3. **For platform-appropriate access (recommended)**:
   ```csharp
   RegistryKey key = RegistryKey.OpenBaseKey(
       RegistryHive.LocalMachine, 
       RegistryView.Default  // Uses appropriate view for current process
   );
   ```

---

## 5. Build Configuration Analysis

### 5.1 Current Build Configuration

From repository memory:
```powershell
.\build.ps1 '/p:PlatformToolset=v144'
```

**Analysis:**
- Uses v144 platform toolset (Visual Studio 2022)
- No platform target specified in build command

### 5.2 Recommended Platform Targets

For .NET managed code projects (*.csproj), set `PlatformTarget`:

| Target | Use Case | Registry Redirection | Max Memory |
|--------|----------|---------------------|------------|
| **AnyCPU** (Prefer32Bit=false) | Recommended for most scenarios | Runs as x64 on x64 systems | Full 64-bit addressing |
| **AnyCPU** (Prefer32Bit=true) | Legacy compatibility | Runs as x86 on x64 systems | 2GB limit, Wow6432Node active |
| **x64** | Force 64-bit only | No redirection | Full 64-bit addressing |
| **x86** | Force 32-bit only | Wow6432Node active | 2GB limit |

**Recommendation:** Use `<PlatformTarget>AnyCPU</PlatformTarget>` with `<Prefer32Bit>false</Prefer32Bit>` to run as native x64 on x64 systems while maintaining x86 compatibility.

---

## 6. Migration Action Plan

### Phase 1: Critical P/Invoke Fixes (Required for x64)

**Priority: CRITICAL**

1. **Update User32.cs**:
   - Add `GetWindowLongPtr/SetWindowLongPtr` with conditional compilation
   - Update `GetKeyboardLayout()` return type to `IntPtr`
   - Update all call sites to use new signatures

2. **Update Kernel32.cs**:
   - Change `SetProcessWorkingSetSize()` parameters to `UIntPtr`
   - Update all call sites

3. **Update Gdi32.cs**:
   - Change `BITMAP.bmBits` to `IntPtr`
   - Update all usages of BITMAP struct

4. **Update Shell32.cs**:
   - Change `DROPFILES.pFiles` to `UIntPtr`
   - Update drag-drop code

5. **Fix WindowSubClasser.cs** (Line 61):
   - Replace `SetWindowLong` with `SetWindowLongPtr`
   - Remove `.ToInt32()` call

6. **Fix FileContentsHelper.cs** (Lines 76, 91, 92):
   - Replace `.ToInt32()` with `.ToInt64()` or `IntPtr.Add()`

### Phase 2: Type Consistency Improvements (Recommended)

**Priority: MEDIUM**

1. Update BOOL return types from `int` to `bool`:
   - User32: `GetIconInfo()`, `DrawMenuBar()`, `SetMenuInfo()`
   - Kernel32: `UnmapViewOfFile()`, `ReleaseMutex()`, `SetEvent()`, `ResetEvent()`, `CloseHandle()`

2. Update return type consistency:
   - User32: `GetClassName()` → `uint`
   - User32: `TrackPopupMenu()` → `uint`
   - Kernel32: `GetDriveType()` → `uint`
   - Shell32: `FindExecutable()` → `int`

### Phase 3: Verification and Testing

**Priority: HIGH**

1. **Build Testing**:
   - Build as AnyCPU (Prefer32Bit=false)
   - Build as explicit x64
   - Verify all projects compile

2. **Runtime Testing on x64**:
   - Test window subclassing (WindowSubClasser.cs)
   - Test drag-drop operations (FileContentsHelper.cs)
   - Test registry access
   - Test all P/Invoke-dependent features

3. **Registry Testing**:
   - Verify Instrumentor.cs can read MSN Apps registry key
   - Test file associations
   - Confirm no Wow6432Node issues

### Phase 4: Build System Updates

**Priority: MEDIUM**

1. Update .csproj files:
   ```xml
   <PropertyGroup>
     <PlatformTarget>AnyCPU</PlatformTarget>
     <Prefer32Bit>false</Prefer32Bit>
   </PropertyGroup>
   ```

2. Add explicit x64 build configuration

3. Update build scripts to support both x86 and x64 outputs

---

## 7. Risk Assessment

### 7.1 High Risk Areas

| Component | Risk Level | Issue | Mitigation |
|-----------|----------|-------|------------|
| WindowSubClasser.cs | 🔴 **CRITICAL** | Window procedure pointer truncation | Must use SetWindowLongPtr |
| FileContentsHelper.cs | 🔴 **CRITICAL** | Pointer arithmetic truncation | Use ToInt64() or IntPtr.Add() |
| User32 GetWindowLong/SetWindowLong | 🔴 **CRITICAL** | Pointer truncation for window data | Add conditional GetWindowLongPtr |
| Kernel32 SetProcessWorkingSetSize | 🟡 **HIGH** | Memory size truncation | Change to UIntPtr parameters |
| BITMAP struct | 🟡 **HIGH** | Bitmap data pointer truncation | Change bmBits to IntPtr |

### 7.2 Medium Risk Areas

| Component | Risk Level | Issue | Mitigation |
|-----------|----------|-------|------------|
| Registry access | 🟡 **MEDIUM** | Wow6432Node redirection | Use RegistryView explicit API |
| DROPFILES struct | 🟡 **MEDIUM** | Offset truncation | Change pFiles to UIntPtr |
| Return type consistency | 🟢 **LOW** | Type mismatches | Update signatures for correctness |

### 7.3 Low Risk Areas

- Message parameter conversions (safe for most message types)
- HIWORD/LOWORD extraction (only uses 16-bit values)
- System DLL dependencies (all have x64 versions)

---

## 8. Compatibility Matrix

| Scenario | x86 Build on x86 | x86 Build on x64 | x64 Build on x64 | Status |
|----------|------------------|------------------|------------------|--------|
| Window subclassing | ✅ Works | ⚠️ May fail | ❌ **FAILS** | Needs fix |
| File operations | ✅ Works | ✅ Works | ❌ **FAILS** (pointer arithmetic) | Needs fix |
| Registry access | ✅ Works | ⚠️ Redirected | ⚠️ May miss x86 keys | Manageable |
| P/Invoke calls | ✅ Works | ✅ Works | ⚠️ Some truncation | Needs fixes |
| Memory operations | ✅ Works | ✅ Works (< 2GB) | ⚠️ Fails (SetProcessWorkingSetSize) | Needs fix |

---

## 9. Code Examples and Templates

### 9.1 Conditional Compilation Template

```csharp
// Use this pattern for platform-dependent P/Invoke declarations

#if WIN64
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#else
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#endif

// Helper wrapper for easier usage
public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
{
    return GetWindowLongPtr(hWnd, nIndex);
}

public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
{
    return SetWindowLongPtr(hWnd, nIndex, dwNewLong);
}
```

### 9.2 Safe Pointer Arithmetic Template

```csharp
// ❌ WRONG - Truncates on x64:
IntPtr newPtr = new IntPtr(oldPtr.ToInt32() + offset);

// ✅ CORRECT - Option 1 (ToInt64):
IntPtr newPtr = new IntPtr(oldPtr.ToInt64() + offset);

// ✅ CORRECT - Option 2 (IntPtr.Add):
IntPtr newPtr = IntPtr.Add(oldPtr, offset);

// For negative offsets:
IntPtr newPtr = IntPtr.Subtract(oldPtr, offset);
```

### 9.3 Registry Access Template

```csharp
// For platform-appropriate access (recommended)
using (RegistryKey baseKey = RegistryKey.OpenBaseKey(
    RegistryHive.LocalMachine, 
    RegistryView.Default))  // Uses current process bitness
{
    using (RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\MyApp"))
    {
        // Access registry values
    }
}

// For explicit 32-bit access on x64 systems
using (RegistryKey baseKey = RegistryKey.OpenBaseKey(
    RegistryHive.LocalMachine, 
    RegistryView.Registry32))
{
    using (RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\MyApp"))
    {
        // Explicitly accesses Wow6432Node on x64
    }
}
```

---

## 10. Testing Checklist

### 10.1 Pre-Migration Testing (Establish Baseline)
- [ ] Test all features on x86 Windows
- [ ] Test all features on x64 Windows (x86 process)
- [ ] Document any existing issues

### 10.2 Post-Fix Testing (x64 Native)
- [ ] Build as x64 succeeds
- [ ] Window operations work correctly
  - [ ] Window subclassing
  - [ ] Window procedure callbacks
  - [ ] Window property storage/retrieval
- [ ] File operations work correctly
  - [ ] Drag-drop with file lists
  - [ ] File descriptor operations
- [ ] Memory operations work correctly
  - [ ] Process working set adjustment
  - [ ] Large memory allocations (> 2GB)
- [ ] Registry operations work correctly
  - [ ] Read from HKLM\SOFTWARE
  - [ ] Write to HKCU\SOFTWARE\Classes
  - [ ] Verify correct registry view accessed
- [ ] Graphics operations work correctly
  - [ ] Bitmap manipulation
  - [ ] GDI operations
- [ ] UI features work correctly
  - [ ] All dialogs
  - [ ] Menu operations
  - [ ] Keyboard input
  - [ ] Mouse input

### 10.3 Regression Testing
- [ ] All unit tests pass (x86)
- [ ] All unit tests pass (x64)
- [ ] No new memory leaks
- [ ] No performance degradation

---

## 11. Summary and Recommendations

### 11.1 Immediate Actions Required

1. **Fix critical P/Invoke issues** (Phase 1 - ~2-3 days effort)
   - Focus on User32 GetWindowLong/SetWindowLong
   - Fix WindowSubClasser.cs
   - Fix FileContentsHelper.cs pointer arithmetic

2. **Update build configuration** (Phase 4 - ~1 day effort)
   - Set PlatformTarget to AnyCPU
   - Disable Prefer32Bit
   - Test builds

3. **Comprehensive testing** (Phase 3 - ~3-5 days effort)
   - Test on x64 systems
   - Verify all P/Invoke-dependent features

### 11.2 Long-term Recommendations

1. Add automated tests for x64-specific issues
2. Use code analysis tools (e.g., PlatformInvoke analyzer)
3. Consider adding CI builds for both x86 and x64
4. Document platform-specific requirements

### 11.3 Estimated Effort

| Phase | Effort | Priority |
|-------|--------|----------|
| Phase 1: Critical fixes | 2-3 days | 🔴 CRITICAL |
| Phase 2: Type consistency | 1-2 days | 🟡 MEDIUM |
| Phase 3: Testing | 3-5 days | 🔴 CRITICAL |
| Phase 4: Build system | 1 day | 🟡 MEDIUM |
| **Total** | **7-11 days** | - |

### 11.4 Success Criteria

- ✅ All critical P/Invoke issues resolved
- ✅ Application builds and runs as native x64
- ✅ All features work correctly on x64
- ✅ No pointer truncation issues
- ✅ Registry access works correctly
- ✅ Performance maintained or improved

---

## Appendix A: Detailed File Inventory

### A.1 Files Requiring Changes

| Priority | File | Issues | Lines Affected |
|----------|------|--------|----------------|
| 🔴 CRITICAL | src/managed/OpenLiveWriter.Interop/Windows/User32.cs | GetWindowLong/SetWindowLong, GetKeyboardLayout | 18, 284, 291, 426, 430 |
| 🔴 CRITICAL | src/managed/OpenLiveWriter.CoreServices/WindowSubClasser.cs | SetWindowLong int truncation | 61 |
| 🔴 CRITICAL | src/managed/OpenLiveWriter.CoreServices/DataObject/FileContentsHelper.cs | Pointer arithmetic ToInt32 | 76, 91, 92 |
| 🟡 HIGH | src/managed/OpenLiveWriter.Interop/Windows/Kernel32.cs | SetProcessWorkingSetSize parameters | 64-68 |
| 🟡 HIGH | src/managed/OpenLiveWriter.Interop/Windows/Gdi32.cs | BITMAP.bmBits field | 198 |
| 🟡 HIGH | src/managed/OpenLiveWriter.Interop/Windows/Shell32.cs | DROPFILES.pFiles field | 523 |
| 🟢 MEDIUM | src/managed/OpenLiveWriter.Interop/Windows/User32.cs | Return type consistency | Multiple |
| 🟢 MEDIUM | src/managed/OpenLiveWriter.Interop/Windows/Kernel32.cs | Return type consistency | Multiple |

### A.2 Files with Correct Patterns (Reference Examples)

| File | Correct Pattern |
|------|-----------------|
| src/managed/OpenLiveWriter.Interop/Windows/Advapi32.cs | Uses UIntPtr for HKEY |
| src/managed/OpenLiveWriter.CoreServices/RegistryHelper.cs | Uses UIntPtr for registry handles |
| src/managed/OpenLiveWriter.Interop/Windows/TaskDialog/TaskDialogMarshallers.cs | Uses ToInt64() for pointer arithmetic |

---

## Appendix B: References and Resources

### B.1 Microsoft Documentation

- [Platform Invoke (P/Invoke)](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [Marshaling Data with Platform Invoke](https://docs.microsoft.com/en-us/dotnet/framework/interop/marshaling-data-with-platform-invoke)
- [32-bit and 64-bit Application Data in the Registry](https://docs.microsoft.com/en-us/windows/win32/winprog64/registry-redirector)
- [GetWindowLongPtr function](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongptrw)

### B.2 Common x64 Migration Patterns

- Always use `IntPtr` for handles (HWND, HBITMAP, HICON, etc.)
- Use `UIntPtr` for SIZE_T parameters
- Use GetWindowLongPtr/SetWindowLongPtr instead of GetWindowLong/SetWindowLong
- Use ToInt64() for pointer arithmetic on x64
- Use conditional compilation for platform-specific P/Invoke

### B.3 Code Analysis Tools

- Visual Studio Code Analysis
- FxCop / .NET Compiler Platform Analyzers
- PlatformInvoke analyzer (Roslyn-based)

---

**End of Analysis**

*For questions or clarification, please refer to the specific sections above or contact the development team.*
