# OpenLiveWriter x64 Migration Checklist

**Quick Reference Guide for x64 Migration Tasks**

## 🔴 CRITICAL - Must Fix Before x64 Release

### 1. User32.cs - Window Function Pointers
- [ ] Add conditional compilation for GetWindowLongPtr/SetWindowLongPtr
- [ ] Replace GetWindowLong (line 284) with GetWindowLongPtr
- [ ] Replace SetWindowLong (line 291) with SetWindowLongPtr
- [ ] Update GetKeyboardLayout (line 18) return type from `int` to `IntPtr`
- [ ] Update all call sites to use new signatures

**Files affected:** `src/managed/OpenLiveWriter.Interop/Windows/User32.cs`

**Code template:**
```csharp
#if WIN64
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
#else
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
#endif
```

### 2. WindowSubClasser.cs - Window Procedure Assignment
- [ ] Line 61: Replace `SetWindowLong` with `SetWindowLongPtr`
- [ ] Remove `.ToInt32()` call on window procedure pointer
- [ ] Test window subclassing functionality

**Current code (BROKEN on x64):**
```csharp
User32.SetWindowLong(_window.Handle, GWL.WNDPROC, m_baseWndProc.ToInt32());
```

**Fixed code:**
```csharp
User32.SetWindowLongPtr(_window.Handle, GWL.WNDPROC, m_baseWndProc);
```

**Files affected:** `src/managed/OpenLiveWriter.CoreServices/WindowSubClasser.cs`

### 3. FileContentsHelper.cs - Pointer Arithmetic
- [ ] Line 76: Replace `ToInt32()` with `ToInt64()` or use `IntPtr.Add()`
- [ ] Line 91: Replace `ToInt32()` with `ToInt64()` or use `IntPtr.Add()`
- [ ] Line 92: Replace `ToInt32()` with `ToInt64()` or use `IntPtr.Add()`
- [ ] Test drag-drop with file operations

**Current code (BROKEN on x64):**
```csharp
new IntPtr(globalMem.Memory.ToInt32() + Marshal.SizeOf(count));
IntPtr pAddr = new IntPtr(pDescriptors.ToInt32() + (i * totalSize));
IntPtr pFileNameAddr = new IntPtr(pAddr.ToInt32() + headerSize);
```

**Fixed code (Option 1 - ToInt64):**
```csharp
new IntPtr(globalMem.Memory.ToInt64() + Marshal.SizeOf(count));
IntPtr pAddr = new IntPtr(pDescriptors.ToInt64() + (i * totalSize));
IntPtr pFileNameAddr = new IntPtr(pAddr.ToInt64() + headerSize);
```

**Fixed code (Option 2 - IntPtr.Add, preferred):**
```csharp
IntPtr.Add(globalMem.Memory, Marshal.SizeOf(count));
IntPtr pAddr = IntPtr.Add(pDescriptors, i * totalSize);
IntPtr pFileNameAddr = IntPtr.Add(pAddr, headerSize);
```

**Files affected:** `src/managed/OpenLiveWriter.CoreServices/DataObject/FileContentsHelper.cs`

### 4. Kernel32.cs - Memory Size Parameters
- [ ] Lines 64-68: Change `SetProcessWorkingSetSize` parameters from `int` to `UIntPtr`
- [ ] Update all call sites with appropriate casts
- [ ] Test memory management features

**Current code:**
```csharp
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool SetProcessWorkingSetSize(
    IntPtr hProcess,
    int dwMinimumWorkingSetSize,
    int dwMaximumWorkingSetSize
);
```

**Fixed code:**
```csharp
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool SetProcessWorkingSetSize(
    IntPtr hProcess,
    UIntPtr dwMinimumWorkingSetSize,
    UIntPtr dwMaximumWorkingSetSize
);
```

**Files affected:** `src/managed/OpenLiveWriter.Interop/Windows/Kernel32.cs`

### 5. Gdi32.cs - BITMAP Structure
- [ ] Line 198: Change `BITMAP.bmBits` from `int` to `IntPtr`
- [ ] Update all code using BITMAP structure
- [ ] Test bitmap operations

**Current code:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BITMAP
{
    // ... other fields ...
    public int bmBits;  // ❌ Wrong on x64
}
```

**Fixed code:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BITMAP
{
    // ... other fields ...
    public IntPtr bmBits;  // ✅ Correct
}
```

**Files affected:** `src/managed/OpenLiveWriter.Interop/Windows/Gdi32.cs`

### 6. Shell32.cs - DROPFILES Structure
- [ ] Line 523: Change `DROPFILES.pFiles` from `uint` to `UIntPtr`
- [ ] Update drag-drop code using DROPFILES
- [ ] Test drag-drop operations

**Current code:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct DROPFILES
{
    public uint pFiles;  // ❌ Wrong on x64
    // ... other fields ...
}
```

**Fixed code:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct DROPFILES
{
    public UIntPtr pFiles;  // ✅ Correct
    // ... other fields ...
}
```

**Files affected:** `src/managed/OpenLiveWriter.Interop/Windows/Shell32.cs`

---

## 🟡 HIGH PRIORITY - Recommended Fixes

### 7. Type Consistency - BOOL Return Values
- [ ] User32.cs line 30: `GetIconInfo()` → return `bool` instead of `int`
- [ ] User32.cs line 633: `DrawMenuBar()` → return `bool` instead of `int`
- [ ] User32.cs line 636: `SetMenuInfo()` → return `bool` instead of `int`
- [ ] Kernel32.cs line 196: `UnmapViewOfFile()` → return `bool` instead of `int`
- [ ] Kernel32.cs line 246: `ReleaseMutex()` → return `bool` instead of `int`
- [ ] Kernel32.cs line 264: `SetEvent()` → return `bool` instead of `int`
- [ ] Kernel32.cs line 267: `ResetEvent()` → return `bool` instead of `int`
- [ ] Kernel32.cs line 276: `CloseHandle()` → return `bool` instead of `int`

### 8. Type Consistency - Other Return Values
- [ ] User32.cs line 27: `GetClassName()` → return `uint` instead of `int`
- [ ] User32.cs line 400: `TrackPopupMenu()` → return `uint` instead of `int`
- [ ] Kernel32.cs line 292: `GetDriveType()` → return `uint` instead of `long`
- [ ] Shell32.cs lines 28-29: `FindExecutable()` → return `int` instead of `IntPtr`

---

## 🟢 MEDIUM PRIORITY - Build System Updates

### 9. Project Configuration
- [ ] Add `<PlatformTarget>AnyCPU</PlatformTarget>` to all .csproj files
- [ ] Add `<Prefer32Bit>false</Prefer32Bit>` to all .csproj files
- [ ] Create explicit x64 build configuration
- [ ] Update build scripts to support x64
- [ ] Verify all managed DLL references are AnyCPU or have x64 versions

### 10. Conditional Compilation
- [ ] Define WIN64 symbol for x64 builds
- [ ] Update build configurations to set WIN64 appropriately
- [ ] Test both x86 and x64 builds

---

## ✅ TESTING REQUIREMENTS

### Pre-Migration Baseline
- [ ] Test all features on x86 Windows (32-bit process)
- [ ] Test all features on x64 Windows (32-bit process via WoW64)
- [ ] Document baseline behavior

### Post-Migration Validation
- [ ] Build as AnyCPU succeeds
- [ ] Build as x64 succeeds
- [ ] Build as x86 still succeeds (for compatibility)

### Functional Testing (x64 Native Build)
- [ ] Window operations
  - [ ] Window creation and destruction
  - [ ] Window subclassing (WindowSubClasser.cs)
  - [ ] Window procedure callbacks
  - [ ] Window properties (Get/SetWindowLongPtr)
- [ ] File operations
  - [ ] Drag-drop file lists
  - [ ] File descriptors (FileContentsHelper.cs)
  - [ ] File I/O operations
- [ ] Memory operations
  - [ ] Process working set size adjustments
  - [ ] Large memory allocations (if applicable)
- [ ] Registry operations
  - [ ] Read from HKLM\SOFTWARE
  - [ ] Write to HKCU\SOFTWARE\Classes
  - [ ] Verify correct registry view
- [ ] Graphics operations
  - [ ] Bitmap creation and manipulation (BITMAP struct)
  - [ ] GDI operations
- [ ] UI operations
  - [ ] All dialogs render correctly
  - [ ] Menu operations work
  - [ ] Keyboard input works
  - [ ] Mouse input works

### Performance Testing
- [ ] No performance degradation on x86
- [ ] Performance maintained or improved on x64
- [ ] Memory usage within acceptable limits

### Regression Testing
- [ ] All existing unit tests pass on x86
- [ ] All existing unit tests pass on x64
- [ ] No new memory leaks detected
- [ ] No new crashes or exceptions

---

## 📋 VERIFICATION STEPS

### Build Verification
```powershell
# Test x86 build
.\build.ps1 '/p:PlatformToolset=v144' '/p:Platform=x86'

# Test x64 build
.\build.ps1 '/p:PlatformToolset=v144' '/p:Platform=x64'

# Test AnyCPU build
.\build.ps1 '/p:PlatformToolset=v144' '/p:Platform=AnyCPU'
```

### Code Analysis
```powershell
# Run static analysis for P/Invoke issues
# Use Visual Studio Code Analysis or FxCop
```

### Runtime Verification
```powershell
# On x64 system, verify process is running as native x64
# Check Task Manager → Details → Platform column should show "64-bit"
```

---

## 📊 PROGRESS TRACKING

**Status Legend:**
- ❌ Not started
- 🚧 In progress
- ✅ Complete
- ⚠️ Blocked

| Task # | Description | Status | Assignee | Notes |
|--------|-------------|--------|----------|-------|
| 1 | User32.cs window functions | ❌ | | |
| 2 | WindowSubClasser.cs fix | ❌ | | |
| 3 | FileContentsHelper.cs fix | ❌ | | |
| 4 | Kernel32.cs memory params | ❌ | | |
| 5 | Gdi32.cs BITMAP struct | ❌ | | |
| 6 | Shell32.cs DROPFILES struct | ❌ | | |
| 7 | Type consistency - BOOL | ❌ | | |
| 8 | Type consistency - Other | ❌ | | |
| 9 | Project configuration | ❌ | | |
| 10 | Conditional compilation | ❌ | | |
| 11 | Testing - Pre-migration | ❌ | | |
| 12 | Testing - Post-migration | ❌ | | |

---

## 🚨 KNOWN RISKS

| Risk | Severity | Mitigation |
|------|----------|------------|
| Window subclassing fails on x64 | 🔴 CRITICAL | Task #2 must be completed |
| File drag-drop crashes on x64 | 🔴 CRITICAL | Task #3 must be completed |
| Pointer truncation in window data | 🔴 CRITICAL | Task #1 must be completed |
| Memory size limits on x64 | 🟡 HIGH | Task #4 should be completed |
| Bitmap operations fail | 🟡 HIGH | Task #5 should be completed |
| Registry redirection issues | 🟢 MEDIUM | Already using managed APIs (low risk) |

---

## 📚 QUICK REFERENCE

### Safe Patterns
```csharp
✅ IntPtr handle = GetSomeHandle();
✅ UIntPtr size = new UIntPtr(1024);
✅ IntPtr newPtr = IntPtr.Add(oldPtr, offset);
✅ IntPtr newPtr = new IntPtr(oldPtr.ToInt64() + offset);
```

### Unsafe Patterns (DO NOT USE)
```csharp
❌ int handle = GetSomeHandle().ToInt32();  // Truncates on x64
❌ User32.SetWindowLong(hwnd, GWL.WNDPROC, proc.ToInt32());  // Fails on x64
❌ IntPtr newPtr = new IntPtr(oldPtr.ToInt32() + offset);  // Truncates on x64
❌ public int bmBits;  // Should be IntPtr for pointers
```

### Conditional Compilation
```csharp
#if WIN64
    // x64-specific P/Invoke
#else
    // x86-specific P/Invoke
#endif
```

---

**For detailed analysis, see [X64_MIGRATION_ANALYSIS.md](X64_MIGRATION_ANALYSIS.md)**
