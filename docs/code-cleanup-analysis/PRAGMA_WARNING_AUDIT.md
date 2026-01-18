# Pragma Warning Disable Audit

**Date:** 2026-01-18  
**Total Count:** 6 pragma warning disable statements

## Executive Summary

Found 6 `#pragma warning disable` statements across 6 files in the codebase. Analysis shows:
- **4 should be kept** - Required for backward compatibility, test infrastructure, or plugin API compatibility
- **2 should be reviewed** - Potential candidates for refactoring or removal

## Detailed Analysis

### 1. TestHtmlEditor.cs (Line 55) ✅ KEEP

**File:** `src/managed/OpenLiveWriter.Tests/PostEditor/Tables/TestHtmlEditor.cs`  
**Warning Code:** CS0067 (Event is declared but never used)

```csharp
#pragma warning disable CS0067
        public event EventHandler IsDirtyEvent;
        public bool SuspendAutoSave { get; }
```

**Analysis:**  
This is a test stub implementing an interface (likely `IHtmlEditor` or similar). The event is required by the interface contract but is intentionally not used in the test implementation.

**Recommendation:** **KEEP**  
- Required for interface implementation in test code
- No matching restore statement (acceptable for test files)
- Removing would break compilation

---

### 2. ApplicationDiagnostics.cs (Lines 52-54) ⚠️ REVIEW

**File:** `src/managed/OpenLiveWriter.CoreServices/Diagnostics/ApplicationDiagnostics.cs`  
**Warning Code:** 0067 (Event is declared but never used)

```csharp
#pragma warning disable 0067
        public static event EventHandler TestModeChanged;
#pragma warning restore 0067
```

**Analysis:**  
Static event that appears to be part of test infrastructure. The warning suggests no code currently raises this event, though external code may subscribe to it.

**Recommendation:** **REVIEW**  
- Search codebase for any subscribers: `TestModeChanged +=`
- If no subscribers found, consider removing the event entirely
- If external plugins use it, must keep for API compatibility

**Action Item:** Run search for `TestModeChanged` usage before deciding

---

### 3. BaseForm.cs (Lines 19-21) ✅ KEEP

**File:** `src/managed/OpenLiveWriter.CoreServices/BaseForm.cs`  
**Warning Code:** 612, 618 (Member is obsolete)

```csharp
#pragma warning disable 612, 618
            AutoScale = false;
#pragma warning restore 612, 618
```

**Analysis:**  
Uses the obsolete `AutoScale` property. This is intentional legacy code needed for WinForms backward compatibility. The `AutoScale` property was deprecated in favor of `AutoScaleMode`, but setting it to `false` is still necessary for correct form scaling behavior in some scenarios.

**Recommendation:** **KEEP**  
- Required for backward compatibility with WinForms scaling
- Standard pattern in legacy WinForms applications
- No viable alternative without potentially breaking form layouts

---

### 4. Program.cs (Lines 23-28) ✅ KEEP

**File:** `src/managed/Canvas/Program.cs`  
**Warning Code:** 0618 (Member is obsolete)

```csharp
// ignore this error just this once since we need to look at a special path 
// depending on the test scenario...
#pragma warning disable 0618
            string s = ConfigurationManager.AppSettings["binPath"];
            AppDomain.CurrentDomain.AppendPrivatePath(s);
#pragma warning restore 0618
```

**Analysis:**  
Uses obsolete `AppDomain.AppendPrivatePath()` method. The comment indicates this is specifically for test scenarios requiring custom binary paths. This is a test utility (Canvas project).

**Recommendation:** **KEEP**  
- Test utility code with specific requirement
- Modern alternative would require significant refactoring (custom `AssemblyLoadContext`)
- Canvas appears to be a test/diagnostic tool where this pattern is acceptable

---

### 5. PluginHttpRequest.cs (Lines 78-80) ✅ KEEP (for now)

**File:** `src/managed/OpenLiveWriter.Api/PluginHttpRequest.cs`  
**Warning Code:** 612, 618 (Member is obsolete)

```csharp
// TODO: Some plugins (like Flickr4Writer) cast this to a WebProxy
// Since the fix for this returns an explicit IWebProxy, we'll need to have
// the Flickr4Writer plugin fixed, then alter this to use the correct call.
#pragma warning disable 612,618
                proxy = System.Net.WebProxy.GetDefaultProxy();
#pragma warning restore 612, 618
```

**Analysis:**  
Uses obsolete `WebProxy.GetDefaultProxy()`. This is part of the plugin API (`OpenLiveWriter.Api` namespace). The TODO comment explicitly states that third-party plugins (like Flickr4Writer) cast the result to `WebProxy`, preventing the use of the modern `IWebProxy`-returning alternative.

**Recommendation:** **KEEP (for now)**  
- Plugin API compatibility requirement
- Breaking change for existing plugins
- Related TODO comment (#5) tracks future refactoring
- Could be addressed when breaking API changes are acceptable

**Future Action:** Coordinate with plugin authors before updating

---

### 6. MapContentSource.cs (Lines 198-200) ⚠️ REVIEW

**File:** `src/managed/OpenLiveWriter.InternalWriterPlugin/MapContentSource.cs`  
**Warning Code:** 612, 618 (Member is obsolete)

```csharp
#pragma warning disable 612, 618
                HtmlScreenCapture screenCapture = new HtmlScreenCapture(
                    new Uri(previewUrl, true), newSize.Width);
#pragma warning restore 612, 618
```

**Analysis:**  
Uses obsolete `Uri` constructor overload: `Uri(string, bool dontEscape)`. The `dontEscape` parameter has been deprecated in favor of proper escaping before construction.

**Recommendation:** **REVIEW**  
- Likely can be refactored to use `Uri(string)` constructor
- May need to ensure `previewUrl` is properly escaped before passing to constructor
- Low risk change, but should verify map preview functionality still works
- This is internal plugin code, not public API

**Suggested Fix:**
```csharp
// Ensure previewUrl is properly escaped, then use:
HtmlScreenCapture screenCapture = new HtmlScreenCapture(
    new Uri(previewUrl), newSize.Width);
```

**Action Item:** Test map preview functionality after refactoring

---

## Summary Table

| File | Line | Warning | Status | Priority |
|------|------|---------|--------|----------|
| TestHtmlEditor.cs | 55 | CS0067 | ✅ KEEP | N/A |
| ApplicationDiagnostics.cs | 52-54 | 0067 | ⚠️ REVIEW | Medium |
| BaseForm.cs | 19-21 | 612, 618 | ✅ KEEP | N/A |
| Program.cs | 23-28 | 0618 | ✅ KEEP | N/A |
| PluginHttpRequest.cs | 78-80 | 612, 618 | ✅ KEEP | Low |
| MapContentSource.cs | 198-200 | 612, 618 | ⚠️ REVIEW | High |

## Recommendations Summary

### Can Be Removed (After Verification): 2
1. **ApplicationDiagnostics.cs** - If `TestModeChanged` event has no subscribers
2. **MapContentSource.cs** - After refactoring to use non-obsolete Uri constructor

### Must Be Kept: 4
1. **TestHtmlEditor.cs** - Interface requirement
2. **BaseForm.cs** - WinForms compatibility
3. **Program.cs** - Test utility requirement  
4. **PluginHttpRequest.cs** - Plugin API compatibility

## Action Items

1. ✅ **High Priority:** Refactor MapContentSource.cs to remove obsolete Uri constructor
   - Verify map preview functionality works correctly
   - Estimated effort: 1-2 hours

2. ✅ **Medium Priority:** Verify TestModeChanged event usage
   - Search for subscribers: `grep -r "TestModeChanged +=" --include="*.cs"`
   - If unused, remove event and pragma
   - Estimated effort: 30 minutes

3. 📋 **Low Priority:** Track PluginHttpRequest.cs for future API breaking change
   - Coordinate with plugin developers
   - Update when major version bump is planned
   - Estimated effort: Part of larger API v2 planning

4. 📋 **Documentation:** Add inline comments explaining why each remaining pragma is necessary
   - Helps future maintainers understand design decisions
   - Estimated effort: 15 minutes

## Notes

- All pragma warnings use proper restore statements except TestHtmlEditor.cs (which is acceptable in test files)
- Warning codes use both numeric (0067, 0618) and symbolic (CS0067) formats - both are valid
- No evidence of suppressed warnings for actual code issues; all appear to be intentional suppressions for deprecated APIs or design-required patterns
