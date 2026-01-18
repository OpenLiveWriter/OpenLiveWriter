# x64 Migration Documentation

This directory contains comprehensive analysis and guidance for migrating OpenLiveWriter to native x64 (64-bit) architecture.

## 📚 Document Guide

### Start Here

**New to the migration?** Start with:
1. **[X64_MIGRATION_SUMMARY.md](X64_MIGRATION_SUMMARY.md)** - Read this first for a quick overview (5 min read)

### For Developers

**Implementing the migration?** Use:
2. **[X64_MIGRATION_CHECKLIST.md](X64_MIGRATION_CHECKLIST.md)** - Step-by-step task list with code examples (15 min read)

### For Technical Deep Dive

**Need detailed analysis?** Reference:
3. **[X64_MIGRATION_ANALYSIS.md](X64_MIGRATION_ANALYSIS.md)** - Complete technical analysis with all findings (30 min read)

## 🎯 Quick Navigation

| I want to... | Go to... |
|--------------|----------|
| Understand the scope and impact | [Summary](X64_MIGRATION_SUMMARY.md) - Section "Critical Findings" |
| See what needs to be fixed | [Checklist](X64_MIGRATION_CHECKLIST.md) - Section "CRITICAL" |
| Get code examples for fixes | [Checklist](X64_MIGRATION_CHECKLIST.md) - Each task has code examples |
| Understand why something is broken | [Analysis](X64_MIGRATION_ANALYSIS.md) - Section 1 & 2 |
| See all P/Invoke issues | [Analysis](X64_MIGRATION_ANALYSIS.md) - Section 1 |
| Check registry compatibility | [Analysis](X64_MIGRATION_ANALYSIS.md) - Section 4 |
| Review native DLL dependencies | [Analysis](X64_MIGRATION_ANALYSIS.md) - Section 3 |
| Create a migration plan | [Checklist](X64_MIGRATION_CHECKLIST.md) + [Analysis](X64_MIGRATION_ANALYSIS.md) Section 6 |

## 🚨 Critical Issues Summary

**6 show-stopper issues** that MUST be fixed for x64:

| # | Component | File | Severity | Impact if not fixed |
|---|-----------|------|----------|---------------------|
| 1 | Window Subclassing | WindowSubClasser.cs:61 | 🔴 CRITICAL | Window subclassing crashes |
| 2 | File Drag-Drop | FileContentsHelper.cs:76,91,92 | 🔴 CRITICAL | Drag-drop operations crash |
| 3 | Window Functions | User32.cs:284,291 | 🔴 CRITICAL | Window data access fails |
| 4 | Keyboard Layout | User32.cs:18 | 🔴 CRITICAL | Keyboard handling may fail |
| 5 | Memory Management | Kernel32.cs:64-68 | 🟡 HIGH | Cannot use > 2GB working set |
| 6 | Bitmap Operations | Gdi32.cs:198 | 🟡 HIGH | Bitmap operations may fail |

See [Checklist](X64_MIGRATION_CHECKLIST.md) for detailed fix instructions.

## 📊 Analysis Stats

- **Files analyzed:** 2,346 C# source files
- **P/Invoke declarations:** 300+
- **Issues found:** 28 (9 critical, 12 medium, 7 low)
- **Files requiring changes:** 6 core interop files
- **Estimated effort:** 7-11 days

## 🛠️ Migration Workflow

```
1. READ:  X64_MIGRATION_SUMMARY.md (understand the problem)
          ↓
2. PLAN:  X64_MIGRATION_ANALYSIS.md Section 6 (migration action plan)
          ↓
3. IMPLEMENT: X64_MIGRATION_CHECKLIST.md (fix each issue)
          ↓
4. TEST:  X64_MIGRATION_CHECKLIST.md Section "Testing Requirements"
          ↓
5. VERIFY: Build as x64, run comprehensive tests
```

## 📋 Quick Checklist

Before starting migration, ensure:
- [ ] You've read the Summary document
- [ ] You understand the 6 critical issues
- [ ] You have a test environment with x64 Windows
- [ ] You can build both x86 and x64 configurations
- [ ] You have access to the files that need changes

## ⚡ Fast Track (For Experienced Developers)

If you're familiar with P/Invoke and x64 migration, you can jump directly to:

1. [Checklist - Section 1-6](X64_MIGRATION_CHECKLIST.md#-critical---must-fix-before-x64-release) - Fix the 6 critical issues
2. [Analysis - Section 9](X64_MIGRATION_ANALYSIS.md#9-code-examples-and-templates) - Copy/paste code templates
3. [Checklist - Testing](X64_MIGRATION_CHECKLIST.md#-testing-requirements) - Run comprehensive tests

## 🆘 Getting Help

If you encounter issues during migration:

1. Check the [Analysis document](X64_MIGRATION_ANALYSIS.md) for detailed explanations
2. Review the code examples in the [Checklist](X64_MIGRATION_CHECKLIST.md)
3. Refer to [Analysis - Appendix B](X64_MIGRATION_ANALYSIS.md#appendix-b-references-and-resources) for Microsoft documentation links

## 📝 Document Details

| Document | Size | Lines | Content |
|----------|------|-------|---------|
| X64_MIGRATION_SUMMARY.md | 5.6 KB | 143 | Executive summary |
| X64_MIGRATION_CHECKLIST.md | 11 KB | 338 | Actionable checklist |
| X64_MIGRATION_ANALYSIS.md | 27 KB | 703 | Technical analysis |

---

**Last Updated:** 2026-01-18  
**Status:** Analysis Complete - Ready for Implementation
