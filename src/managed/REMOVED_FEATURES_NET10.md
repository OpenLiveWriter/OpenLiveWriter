# Removed Features (.NET 10 Migration)

This document lists features, files, and functionality that were removed or disabled during the .NET 10 migration.

---

## Disabled Features

_(None currently outstanding.)_

---

## Restored Features

### Auto-Update System
**Status:** Restored via [Velopack](https://github.com/velopack/velopack)  
**History:** Squirrel.Windows doesn't support .NET 10, so auto-update was disabled during the migration. Velopack (its modern successor for .NET 6+) now provides the install/update lifecycle: `VelopackApp.Build().Run()` handles install/update/uninstall hooks in `ApplicationMain`, and `PostEditor/Updates/UpdateManager.cs` checks for updates with `Velopack.UpdateManager` + `SimpleWebSource`. Installers are produced with the `vpk` CLI (see `PostBuild.CreateInstaller/createinstaller.cmd`).

---

## Deleted Files

These files were deleted as they were superseded by modern replacements:

| Deleted File | Replaced By |
|--------------|-------------|
| `ApplicationFramework/MenuBuilderEntry.cs` | `CommandMenuBuilderEntry.cs` |
| `PostEditor/ImageEditing/ImageEditingPropertyForm.cs` | `PictureEditingManager` (Ribbon UI) |
| `CoreServices/AppIconCache.cs` | `ShellHelper.GetXxxIcon()` |

---

## Excluded Files

These files are excluded from compilation (`<Compile Remove="..."/>`) due to incompatible APIs:

### CoreServices

| File | Reason |
|------|--------|
| `HTML/WebPageDownloader.cs` | Uses unavailable Project31 namespaces |
| `HTML/AsyncPageDownload.cs` | Uses unavailable Project31 namespaces |
| `DataObject/SearchReferrerChain.cs` | Uses unavailable Project31 namespaces |

### Designer Files (Design-Time Only)

| File | Reason |
|------|--------|
| `CoreServices/ComponentRootDesigner.cs` | VS Designer support only |
| `CoreServices/Diagnostics/UnexpectedErrorMessageDesigner.cs` | VS Designer support only |
| `FileDestinations/DisplayMessageDesigner.cs` | VS Designer support only |
| `FileDestinations/WebPublishMessageDesigner.cs` | VS Designer support only |

---

## Suppressed Warnings

### Global Suppressions (Directory.Build.props)

| Warning | Reason |
|---------|--------|
| `SYSLIB0003` | Code Access Security (required for COM interop) |
| `SYSLIB0050` | Type.IsSerializable (legacy compatibility) |
| `SYSLIB0051` | Formatter-based serialization (RegistryCodec fallback) |
| `CA1416` | Platform compatibility (Windows-only application) |
| `WFDEV004/006` | Deprecated WinForms controls |

### Localized Suppressions

| File | Warning | Reason |
|------|---------|--------|
| `RegistryCodec.cs` | `SYSLIB0011` | BinaryFormatter read-only fallback |
| `HttpRequestHelper.cs` | `SYSLIB0009` | WSSE auth for SixApart/TypePad blogs |
| `HttpRequestHelper.cs` | `SYSLIB0014` | WebRequest factory for legacy filter adapter |
| `GenericAtomClient.cs` | `SYSLIB0009` | Google login auth |

---

## HttpClient Migration Status

All HTTP requests have been migrated to `HttpClient`. The legacy `HttpWebRequest` pattern is only used internally for the `HttpRequestFilter` adapter.

### Fully Migrated Components

- ✅ `WebRequestWithCache.cs` - Uses HttpClientService
- ✅ `AsyncWebRequestWithCache.cs` - Uses HttpClientService
- ✅ `ContentTypeHelper.cs` - Uses HttpResponseMessage
- ✅ `TistoryBlogClient.cs` - Uses HttpClientXmlRestRequestHelper
- ✅ `YouTubeVideoService.cs` - Uses HttpClientService
- ✅ `YoutubeVideoPublisher.cs` - Uses HttpClientService
- ✅ `LiveJournalClient.cs` - FotobilderRequestManager uses HttpClientService
- ✅ `ResourceFileDownloader.cs` - Uses HttpClientService
- ✅ `PluginHttpRequest.cs` - Uses HttpClientService
- ✅ `HockeyAppProxy.cs` - Uses HttpClientService
- ✅ `GDataCaptchaForm.cs` - Uses HttpClientService
- ✅ `WebImageSource.cs` - Uses HttpClientService
- ✅ `DestinationValidator.cs` - Uses HttpRequestHelper.CheckUrlReachable
- ✅ `HttpRequestHelper.cs` - Added HttpClient-based methods, legacy filter adapter
- ✅ `HttpClientRedirectHelper.cs` - New HttpClient-based redirect helper
- ✅ `HttpClientXmlRestRequestHelper.cs` - New HttpClient-based XML REST helper
- ✅ `PostEditorMainControl.ValidateHtml` - Uses HttpClient
- ✅ `CloseTrackingHttpWebRequest.cs` - Rewritten for .NET 10 compatibility
- ✅ `RedirectHelper.cs` - Fully HttpClient-based, returns HttpResponseMessageWrapper
- ✅ `XmlRestRequestHelper.cs` - Fully HttpClient-based with legacy filter adapter
- ✅ `AtomClient.cs` - Uses HttpClient via RedirectHelper
- ✅ `AtomMediaUploader.cs` - Uses HttpClient via RedirectHelper
- ✅ `BloggerAtomClient.cs` - Uses HttpClient via RedirectHelper
- ✅ `XmlRpcClient.cs` - Added HttpClient support with Action<HttpRequestMessage> constructor

### Legacy HttpRequestFilter Pattern

The `HttpRequestFilter` delegate (`Action<HttpWebRequest>`) is part of the public API and is used throughout the codebase for authentication. Rather than changing this interface, we provide:

1. `HttpRequestHelper.ApplyLegacyFilter()` - Adapts legacy filters to `HttpRequestMessage`
2. A temporary `HttpWebRequest` is created to capture filter settings, then applied to `HttpRequestMessage`

This allows existing authentication code to work unchanged while using HttpClient internally.

### SYSLIB0014 Suppressions

Suppressions are localized to `HttpRequestHelper.cs` only:
- `CreateHttpWebRequest()` - Factory method for rare cases needing direct HttpWebRequest
- `ApplyLegacyFilter()` - Creates temporary HttpWebRequest to capture filter settings

### For New Code

- Use `HttpClientService.DefaultClient` for direct HttpClient access
- Use `HttpClientRedirectHelper` for redirect-following requests
- Use `HttpClientXmlRestRequestHelper` for XML REST requests
- Use `HttpRequestHelper.HttpClient` property for configured HttpClient instance
- Avoid `CreateHttpWebRequest()` - it exists only for backward compatibility
