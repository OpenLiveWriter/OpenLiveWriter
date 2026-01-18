# Open Live Writer Documentation

This directory contains technical documentation for Open Live Writer development, migration planning, and testing.

## Documentation Index

### Migration and Planning

- **[X64-MIGRATION-PLAN.md](X64-MIGRATION-PLAN.md)** (20 KB, 628 lines)
  - Comprehensive x64 architecture migration plan
  - Identifies MSHTML as critical blocker requiring WebView2 migration first
  - 5-phase migration roadmap with timeline estimates
  - Platform-specific build configuration requirements
  - P/Invoke signature corrections and COM interop considerations
  - Risk assessment and success criteria

- **[MSHTML-INTERFACES-DOCUMENTATION.md](MSHTML-INTERFACES-DOCUMENTATION.md)** (19 KB, 601 lines)
  - Complete catalog of 32 MSHTML COM interfaces used in OLW
  - Explains purpose and necessity of each shimmed interface
  - Documents core markup manipulation, selection, rendering, and editing behavior interfaces
  - WebView2 migration strategy with interface mapping table
  - Reference for understanding legacy editor architecture

### Testing and Quality Assurance

- **[TESTING-CHECKLIST.md](TESTING-CHECKLIST.md)** (18 KB, 717 lines)
  - Comprehensive testing procedures for all application features
  - WebView2 editor testing (text, formatting, links, images, tables)
  - Edit ↔ Source view round-trip validation
  - x64-specific testing (memory handling, P/Invoke, performance benchmarks)
  - Blog integration, installation, and upgrade scenarios
  - Edge cases, accessibility, and regression testing

### Development Guides

- **[Hacking - Static Site Client.md](Hacking%20-%20Static%20Site%20Client.md)**
  - Guide for developing static site blog clients

- **[Connecting to Blogger From a Local Build.md](Connecting%20to%20Blogger%20From%20a%20Local%20Build.md)**
  - Instructions for testing with Blogger API during development

## Quick Navigation by Task

### If you're working on x64 migration:
1. Read [X64-MIGRATION-PLAN.md](X64-MIGRATION-PLAN.md) - Understand the architecture requirements and blockers
2. Note: **WebView2 migration must be completed BEFORE x64 work** (MSHTML is x86-only)
3. Use [TESTING-CHECKLIST.md](TESTING-CHECKLIST.md) section "x64-Specific Testing" for validation

### If you're working on WebView2 migration:
1. See the **[feature/webview2 branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2)** for active work
2. Read that branch's `docs/WEBVIEW2-BRANCH-REPORT.md` for current status
3. Read that branch's `docs/WEBVIEW2-EDITOR-MIGRATION-PLAN.md` for implementation details
4. Read [MSHTML-INTERFACES-DOCUMENTATION.md](MSHTML-INTERFACES-DOCUMENTATION.md) - Understand what MSHTML interfaces need replacement
5. Check the "WebView2 Migration Strategy" section for interface mappings
6. Use [TESTING-CHECKLIST.md](TESTING-CHECKLIST.md) section "WebView2 Editor Testing" for validation

### If you're testing a build:
1. Use [TESTING-CHECKLIST.md](TESTING-CHECKLIST.md) as your primary guide
2. Follow appropriate sections based on build type (x86, x64, WebView2 enabled/disabled)
3. Use test sign-off template to document results

### If you're onboarding to the project:
1. Review [X64-MIGRATION-PLAN.md](X64-MIGRATION-PLAN.md) for current architecture overview
2. Read [MSHTML-INTERFACES-DOCUMENTATION.md](MSHTML-INTERFACES-DOCUMENTATION.md) to understand editor internals
3. Follow [Connecting to Blogger From a Local Build.md](Connecting%20to%20Blogger%20From%20a%20Local%20Build.md) to set up testing

## Key Findings

### Critical Architectural Dependencies

1. **MSHTML is x64 Migration Blocker**
   - 32 custom COM interfaces rely on 32-bit MSHTML.dll
   - COM vtable layouts differ between x86 and x64
   - MSHTML.dll is not reliably available or functional as 64-bit
   - **Solution**: Complete WebView2 migration BEFORE attempting x64
   - **Active Work**: See [feature/webview2 branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2)

2. **Native Code Dependency**
   - OpenLiveWriter.Ribbon.vcxproj (C++) is only unmanaged component
   - Currently Win32-only, needs x64 configuration added
   - Uses Windows Ribbon Framework (UICC.exe compiler)

3. **Build System Ready for x64**
   - `writer.build.settings` already has amd64→x64 mapping logic
   - Build scripts need minor updates to accept Platform parameter
   - Most C# projects use AnyCPU and are architecture-agnostic

### Migration Timeline Estimate

- **Prerequisites**: WebView2 migration (active in [feature/webview2 branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2))
- **Phase 1**: Build infrastructure (1 week)
- **Phase 2**: Native code migration (1 week)
- **Phase 3**: Managed code migration (1-2 weeks)
- **Phase 4**: Integration testing (1-2 weeks)
- **Phase 5**: Deployment (1 week)
- **Total**: 5-6 weeks after WebView2 merge

## Documentation Statistics

| Document | Size | Lines | Sections |
|----------|------|-------|----------|
| X64-MIGRATION-PLAN.md | 20 KB | 628 | 46 |
| MSHTML-INTERFACES-DOCUMENTATION.md | 19 KB | 601 | 40 |
| TESTING-CHECKLIST.md | 18 KB | 717 | 85 |
| **Total** | **57 KB** | **1,946** | **171** |

## Contributing to Documentation

When adding or updating documentation:
- Use clear, descriptive filenames in UPPERCASE-WITH-HYPHENS.md format
- Include file size and purpose in this README
- Add appropriate sections to the "Quick Navigation by Task" guide
- Keep technical accuracy high - reference actual code paths and line numbers
- Update this README's table of contents

## References

- [Building, Testing, and Debugging](https://github.com/OpenLiveWriter/OpenLiveWriter/wiki/Building,-Testing,-and-Debugging) - Wiki
- [WebView2 Documentation](https://docs.microsoft.com/en-us/microsoft-edge/webview2/)
- [Windows SDK for x64 Development](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)

---

*Documentation last updated: 2026-01-18 (as part of x64 migration planning initiative)*
