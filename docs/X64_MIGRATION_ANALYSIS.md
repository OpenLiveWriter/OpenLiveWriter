# X64 Migration Analysis for OpenLiveWriter

## Executive Summary

This document provides a comprehensive analysis of x64 (64-bit) migration issues in the OpenLiveWriter codebase. The analysis focuses on four critical areas:

1. **P/Invoke declarations** that need attention for x64 compatibility
2. **IntPtr vs int issues** where pointer types are incorrectly declared
3. **Native DLL references** and their marshalling characteristics
4. **Registry paths** and Wow6432Node considerations

**Severity Levels:**
- 🔴 **CRITICAL**: Will cause crashes or data corruption on x64
- 🟡 **HIGH**: Will likely cause issues on x64
- 🟢 **MEDIUM**: May cause issues in certain scenarios
- 🔵 **LOW**: Minor improvements for x64 compatibility

---

## 1. P/Invoke Declarations - Critical Issues

### 🔴 CRITICAL: SetWindowLong/GetWindowLong Functions

**Location:** `/src/managed/OpenLiveWriter.Interop/Windows/User32.cs`

#### Issue #1: GetWindowLong (Line 284)
```csharp
[DllImport("user32.dll")]
public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
```

**Problem:** Returns `UInt32` but can return pointer values on x64 systems. When accessing GWLP_WNDPROC, GWLP_HINSTANCE, or GWLP_USERDATA, the function returns a pointer which will be truncated from 64 bits to 32 bits.

**Impact:** Data loss, crashes when dereferencing truncated pointers.

**Fix Required:**
```csharp
// For x86
[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
public static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

// For x64
[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

// Wrapper that calls the correct version
public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
{
    if (IntPtr.Size == 8)
        return GetWindowLongPtr64(hWnd, nIndex);
    else
        return GetWindowLongPtr32(hWnd, nIndex);
}
```

**Current Usage Locations:**
- `/src/managed/OpenLiveWriter.Controls/InformationBox.cs:187`
- `/src/managed/OpenLiveWriter.Controls/ApplicationDialog.cs:30`
- `/src/managed/OpenLiveWriter.Controls/DialogHelper.cs:87`
- `/src/managed/OpenLiveWriter.Controls/MiniForm.cs:42`
- `/src/managed/OpenLiveWriter.CoreServices/ControlHelper.cs:194`
- `/src/managed/OpenLiveWriter.ApplicationFramework/CommandContextMenuMiniForm.cs:67`
- `/src/managed/OpenLiveWriter.PostEditor/ImageInsertion/InsertImageDialog.cs:240`

#### Issue #2: SetWindowLong (Lines 291, 430)
```csharp
[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
public static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

[DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int nValue);
```

**Problem:** Same as GetWindowLong - returns/accepts 32-bit values but needs to handle 64-bit pointers on x64.

**Impact:** Setting GWLP_WNDPROC or other pointer values will corrupt the value on x64.

**Fix Required:**
```csharp
// For x86
[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

// For x64
[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

// Wrapper
public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
{
    if (IntPtr.Size == 8)
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    else
        return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
}
```

**Current Usage Locations:**
- `/src/managed/OpenLiveWriter.Controls/InformationBox.cs:187`
- `/src/managed/OpenLiveWriter.Controls/MiniForm.cs:42`
- `/src/managed/OpenLiveWriter.ApplicationFramework/CommandContextMenuMiniForm.cs:67`

#### Issue #3: SetWindowProc (Line 426)
```csharp
[DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
public static extern int SetWindowProc(IntPtr hWnd, int nIndex, WndProcDelegate lpWndProc);
```

**Problem:** Returns `int` but should return `IntPtr` (the previous window procedure).

**Impact:** Cannot properly chain window procedures on x64.

**Critical Usage:** `/src/managed/OpenLiveWriter.CoreServices/WindowSubClasser.cs:61`
```csharp
User32.SetWindowLong(_window.Handle, GWL.WNDPROC, m_baseWndProc.ToInt32());
```
This is **extremely dangerous** - calling `ToInt32()` on a 64-bit pointer will throw an overflow exception!

**Fix Required:** Replace with SetWindowLongPtr wrapper and change code to:
```csharp
User32.SetWindowLongPtr(_window.Handle, GWL.WNDPROC, m_baseWndProc);
```

### 🔴 CRITICAL: GWL Constants Need Update

**Location:** `/src/managed/OpenLiveWriter.Interop/Windows/User32.cs:1038-1047`

```csharp
public struct GWL
{
    public const int WNDPROC = -4;
    public const int HINSTANCE = -6;
    public const int HWNDPARENT = -8;
    public const int STYLE = -16;
    public const int EXSTYLE = -20;
    public const int USERDATA = -21;
    public const int ID = -12;
}
```

**Problem:** On x64, the constants for pointer-returning values should use GWLP_* names.

**Fix Required:** Add GWLP constants:
```csharp
public struct GWLP
{
    public const int WNDPROC = -4;
    public const int HINSTANCE = -6;
    public const int HWNDPARENT = -8;
    public const int USERDATA = -21;
    public const int ID = -12;
}

// Non-pointer values remain in GWL
public struct GWL
{
    public const int STYLE = -16;
    public const int EXSTYLE = -20;
}
```

---

## 2. IntPtr vs int Issues

### 🟡 HIGH: SetProcessWorkingSetSize

**Location:** `/src/managed/OpenLiveWriter.Interop/Windows/Kernel32.cs:64-68`

```csharp
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool SetProcessWorkingSetSize(
    IntPtr hProcess,
    int dwMinimumWorkingSetSize,
    int dwMaximumWorkingSetSize
);
```

**Problem:** Working set sizes should be `SIZE_T` which is 32-bit on x86 and 64-bit on x64. Using `int` limits to 2GB on x64.

**Impact:** Cannot set working sets larger than 2GB on x64.

**Fix Required:**
```csharp
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool SetProcessWorkingSetSize(
    IntPtr hProcess,
    IntPtr dwMinimumWorkingSetSize,
    IntPtr dwMaximumWorkingSetSize
);
```

### 🔵 LOW: GetKeyboardLayout

**Location:** `/src/managed/OpenLiveWriter.Interop/Windows/User32.cs:18` (approximate)

**Issue:** If GetKeyboardLayout exists with `int` parameter, it should technically be HKL (handle to keyboard layout).

**Impact:** Minor, HKL values are typically small.

**Recommendation:** Document or change to `UIntPtr` for correctness.

---

## 3. Native DLL References Analysis

### ✅ Overall Status: GOOD

The codebase demonstrates **good practices** for native interop:

#### Strengths:
1. **SetLastError** is properly used across critical APIs:
   - Kernel32.cs: Memory, process, and file operations
   - User32.cs: Window and message operations
   - Advapi32.cs: Registry and security operations
   - Gdi32.cs: Graphics operations

2. **CharSet specifications** are consistent:
   - CharSet.Auto used appropriately for version-neutral APIs
   - CharSet.Unicode used for explicit Unicode APIs
   - CharSet.Ansi used where required by legacy APIs

3. **Calling Conventions**: Default StdCall is correct for Windows APIs

4. **Handle types**: Most HWND, HANDLE types correctly use IntPtr

### 🔵 LOW: Minor Issues

**Location:** `/src/managed/OpenLiveWriter.Interop/Windows/Gdi32.cs:35`

```csharp
[DllImport("gdi32.dll")]
public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
```

**Issue:** Missing `SetLastError = true` for better error diagnostics.

**Fix:**
```csharp
[DllImport("gdi32.dll", SetLastError = true)]
public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
```

### Summary of P/Invoke Files:

Reviewed Files (all in `/src/managed/OpenLiveWriter.Interop/`):
- ✅ Com/Ole32.cs - Correct COM marshalling
- ✅ Windows/Shell32.cs - Correct handle usage
- ✅ Windows/Shlwapi.cs - Correct string marshalling
- ✅ Windows/Kernel32.cs - Mostly correct (except SetProcessWorkingSetSize)
- ✅ Windows/Gdi32.cs - Correct (minor SetLastError issue)
- ⚠️ Windows/User32.cs - **CRITICAL issues with SetWindowLong/GetWindowLong**
- ✅ Windows/Advapi32.cs - Correct registry and security APIs
- ✅ Windows/ComCtl32.cs - Correct control library marshalling
- ✅ Windows/WinInet.cs - Correct internet API marshalling
- ✅ Windows/UrlMon.cs - Correct URL moniker marshalling

---

## 4. Registry Paths and Wow6432Node Considerations

### 🟢 MEDIUM: Missing RegistryView Specification

**Location:** `/src/managed/OpenLiveWriter.CoreServices/RegistryHelper.cs`

#### Current Implementation:
```csharp
private static RegistryKey GetRootRegistryKey(UIntPtr hkey)
{
    if (hkey == HKEY.CLASSES_ROOT)
        return Registry.ClassesRoot;
    else if (hkey == HKEY.CURRENT_USER)
        return Registry.CurrentUser;
    else if (hkey == HKEY.LOCAL_MACHINE)
        return Registry.LocalMachine;
    // ...
}

public static string GetAppUserModelID(string progId)
{
    using (RegistryKey progIdKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\" + progId))
    {
        // ...
    }
}
```

**Problem:** When a 64-bit process accesses `HKEY_LOCAL_MACHINE\SOFTWARE\Classes`, it accesses the 64-bit registry view. If the application was previously 32-bit and stored data in the 32-bit view, that data won't be accessible without explicit `RegistryView.Registry32`.

**Impact:** 
- Loss of access to 32-bit registry entries from previous installations
- Incompatibility between 32-bit and 64-bit builds
- Plugin discovery may fail if plugins registered in 32-bit view

**Recommendation:** Add explicit registry view support:

```csharp
// Option 1: Always use 32-bit view for backward compatibility
public static string GetAppUserModelID(string progId, RegistryView view = RegistryView.Registry32)
{
    using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
    using (RegistryKey progIdKey = baseKey.OpenSubKey(@"SOFTWARE\Classes\" + progId))
    {
        if (progIdKey != null)
            return progIdKey.GetValue("AppUserModelID") as string;
    }
    return null;
}

// Option 2: Check both views (try 64-bit first, fall back to 32-bit)
public static string GetAppUserModelID(string progId)
{
    // Try native view first
    var result = GetAppUserModelIDFromView(progId, RegistryView.Registry64);
    if (result != null)
        return result;
    
    // Fall back to 32-bit view for backward compatibility
    return GetAppUserModelIDFromView(progId, RegistryView.Registry32);
}

private static string GetAppUserModelIDFromView(string progId, RegistryView view)
{
    try
    {
        using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
        using (RegistryKey progIdKey = baseKey.OpenSubKey(@"SOFTWARE\Classes\" + progId))
        {
            if (progIdKey != null)
                return progIdKey.GetValue("AppUserModelID") as string;
        }
    }
    catch (Exception ex)
    {
        Trace.WriteLine($"Exception reading registry view {view} for {progId}: {ex.Message}");
    }
    return null;
}
```

### Registry Access Locations:

Files with registry access (potential Wow6432Node issues):
- ✅ `/src/managed/OpenLiveWriter.CoreServices/RegistryHelper.cs:103` - SOFTWARE\Classes path
- ⚠️ `/src/managed/OpenLiveWriter.CoreServices/RegistryMonitor.cs` - May need view specification
- ⚠️ `/src/managed/OpenLiveWriter.CoreServices/ApplicationEnvironment.cs` - Settings storage
- ⚠️ `/src/managed/OpenLiveWriter.PostEditor/PostEditorPluginManager.cs` - Plugin registration
- ⚠️ `/src/managed/OpenLiveWriter/ApplicationMain.cs` - Application settings
- ⚠️ `/src/managed/OpenLiveWriter.PostEditor/JumpList/JumpList.cs` - Shell integration

**Note:** The codebase does **NOT** currently use explicit Wow6432Node paths, which is good. However, it also doesn't use RegistryView, which means behavior will differ between x86 and x64 builds.

---

## 5. Unmanaged Code Analysis

### Native C++ Projects

Located in `/src/unmanaged/`:
- OpenLiveWriter.Filter
- OpenLiveWriter.Shortcuts  
- OpenLiveWriter.CppUtils

**Action Required:** Verify these projects:
1. Have x64 configurations defined
2. Use correct calling conventions (WINAPI)
3. Use ULONG_PTR, SIZE_T instead of DWORD for size/pointer types
4. Properly handle pointer arithmetic
5. Are compiled with /LARGEADDRESSAWARE flag for x64

---

## 6. Platform Configuration Analysis

### Build Configuration

**Action Required:** Check `.csproj` files for:
1. `<PlatformTarget>` settings - should support AnyCPU, x86, x64
2. `<Prefer32Bit>false</Prefer32Bit>` - ensure this is set for libraries
3. Platform-specific conditional compilation if needed

**Example Configuration:**
```xml
<PropertyGroup Condition="'$(Platform)' == 'x64'">
  <PlatformTarget>x64</PlatformTarget>
  <Prefer32Bit>false</Prefer32Bit>
</PropertyGroup>
```

---

## 7. Priority Fix List

### Immediate (Before x64 Release):

1. 🔴 **CRITICAL** - Fix `GetWindowLong`/`SetWindowLong` in User32.cs
   - Add `GetWindowLongPtr`/`SetWindowLongPtr` wrappers
   - Update all calling code to use new wrappers
   - **URGENT**: Fix WindowSubClasser.cs line 61 `.ToInt32()` call

2. 🔴 **CRITICAL** - Add GWLP_* constants
   - Separate pointer-returning constants from value-returning constants

3. 🟡 **HIGH** - Fix `SetProcessWorkingSetSize` in Kernel32.cs
   - Change int parameters to IntPtr

### Short Term:

4. 🟢 **MEDIUM** - Add RegistryView support to RegistryHelper.cs
   - Implement dual-view lookup for backward compatibility
   - Test plugin discovery in both x86 and x64 builds

5. 🔵 **LOW** - Add SetLastError to CreateCompatibleDC in Gdi32.cs

### Validation:

6. ✅ **Verify** - Unmanaged C++ projects have x64 configurations
7. ✅ **Verify** - .csproj files have correct platform targets
8. ✅ **Test** - Run full application on x64 with all features

---

## 8. Testing Recommendations

### Unit Tests:

Create tests for:
1. Window procedure subclassing on x64
2. Registry access in both x86 and x64 views
3. Working set size setting with large values (>2GB)
4. Plugin loading from both registry views

### Integration Tests:

1. Install 32-bit version, then upgrade to 64-bit version
2. Verify all settings and plugins are accessible
3. Test window subclassing with complex UI scenarios
4. Verify memory allocation and process management

### Stress Tests:

1. Large working set sizes (x64 only)
2. Long-running window procedure chains
3. Registry operations under high load

---

## 9. References

### Microsoft Documentation:

- [Porting 32-bit Code to 64-bit](https://docs.microsoft.com/en-us/windows/win32/winprog64/porting-32-bit-code-to-64-bit-windows)
- [GetWindowLongPtr function](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongptrw)
- [Registry Redirector](https://docs.microsoft.com/en-us/windows/win32/winprog64/registry-redirector)

### Common x64 Pitfalls:

1. Truncating pointers to 32-bit integers
2. Assuming sizeof(pointer) == sizeof(long) == 4
3. Not using platform-specific API versions (*Ptr functions)
4. Registry redirection surprises
5. Mixed-mode assemblies and platform targets

---

## 10. Conclusion

OpenLiveWriter has **good foundational interop practices** but requires **critical fixes** before x64 deployment:

**Risk Assessment:**
- 🔴 **HIGH RISK**: Window procedure subclassing will crash on x64
- 🟡 **MEDIUM RISK**: Registry access may lose data from x86 installations
- 🟢 **LOW RISK**: Most P/Invoke is already x64-compatible

**Estimated Effort:**
- Critical fixes: 2-4 hours
- Testing: 4-8 hours
- Registry compatibility: 2-4 hours
- **Total: 8-16 hours**

**Next Steps:**
1. Create branch for x64 migration fixes
2. Implement SetWindowLongPtr wrapper and update all usages
3. Add comprehensive tests
4. Perform migration testing (x86 → x64 upgrade path)
5. Release x64 build to beta testers

---

*Analysis Date: 2026-01-18*
*Analyst: GitHub Copilot*
*Codebase Version: Current HEAD*
