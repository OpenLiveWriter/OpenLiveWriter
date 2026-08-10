# Auto Updates (Velopack)

Open Live Writer checks for updates with Velopack. Two feed types are
supported, selected by `UpdateSettings.UpdateFeedType`:

| Feed type | Source | Used for |
|---|---|---|
| `github` (default) | GitHub Releases via Velopack's `GithubSource` | Production and beta checks |
| `website` | Static Velopack feed via `SimpleWebSource` | Feeds hosted on openlivewriter.com |

## Settings

All settings live in the registry under
`HKCU\Software\OpenLiveWriter\Updates` (see
`src/managed/OpenLiveWriter.PostEditor/Updates/UpdateSettings.cs`):

| Value | Default | Meaning |
|---|---|---|
| `AutoUpdate` | `true` | Check for updates at startup |
| `CheckForBetaUpdates` | `true` | `true` = GitHub prereleases / nightly website feed. Defaults on while the project ships alphas, which are published as prereleases |
| `UpdateFeedType` | `github` | `github` or `website` |
| `GitHubRepoUrl` | `https://github.com/OpenLiveWriter/OpenLiveWriter` | Repo whose Releases feed the check |
| `CheckUpdatesUrl` | `https://openlivewriter.com/releases/stable` | Website feed (stable) |
| `CheckBetaUpdatesUrl` | `https://openlivewriter.com/releases/nightly` | Website feed (beta/nightly) |

## Publishing a release

Dispatch `.github/workflows/release.yml` on the org repo against
`develop/windows`, with `dry_run=false`. One run builds both platforms and
publishes a single GitHub Release carrying both installers and both update
feeds. There is no `win-v*` tag trigger any more, and no separate macOS
release workflow.

`version.txt` drives everything; nothing reads a version off a tag. It holds
`MAJOR.MINOR.PATCH.BUILD`, where you maintain the semver and the workflow
stamps `BUILD` from the run number, commits it, and tags `v<semver>`:

| Derived value | Example | Why it differs |
|---|---|---|
| assemblies | `0.7.0.4` | `AssemblyVersion` / `FileVersion` |
| tag and release | `v0.7.0` | one release per semver, refreshed in place |
| Velopack package | `0.7.0-alpha.4` | vpk rejects 4-part versions outright |
| `CFBundleShortVersionString` | `0.7.0` | the key takes at most three components |
| `CFBundleVersion` | `4` | |

Velopack compares package versions, not tag names, so the `-alpha.<build>`
suffix is what makes each build sort above the last. That is also why
`CheckForBetaUpdates` defaults on: with prereleases excluded, an installed
alpha would never see a newer alpha.

## Signing

Builds are currently **unsigned**, so Windows shows a SmartScreen warning on
install and macOS Gatekeeper blocks a downloaded `.pkg` outright.

The previous Windows path called the .NET Foundation signing service through
SignClient. That service is gone (`codesign.dotnetfoundation.org` answers
HTTP 500), so the plumbing was removed rather than left looking functional.

The intended replacement is SignPath Foundation, which is free for OSS and
issues an OV-level certificate. Note it submits artifacts to a service and
signs nested files there, so it will not slot into vpk's `--signTemplate`;
the release job needs a submit/await/download step around `vpk pack`.

macOS signing is already wired: set `OLW_CODESIGN_IDENTITY`,
`OLW_INSTALL_IDENTITY` and `OLW_NOTARY_PROFILE` and `build-mac.sh` passes
them to vpk, which signs the app, signs the `.pkg` and notarizes.

## Website feed layout

When `UpdateFeedType` is `website`, the app expects the Velopack output of
`vpk pack` hosted statically at the configured URL:

```
releases/stable/
  releases.stable.json
  RELEASES-stable
  OpenLiveWriter-<version>-stable-full.nupkg
  OpenLiveWriter-stable-Setup.exe
releases/nightly/
  ...same shape for the nightly channel...
```

Upload the contents of the `Releases\` directory produced by `vpk pack`
to the matching path on the web host. Deltas and full packages are both
consumed by `SimpleWebSource`.

## Notes

- The old Squirrel feeds (`openlivewriter.azureedge.net`, `olw.blob.core.windows.net`)
  are upstream infrastructure and no longer used.
- Update checks only run when the app was installed by the Velopack
  installer (`UpdateManager.IsInstalled`); Debug/xcopy runs skip the check.
- Installed base of Squirrel-era builds (<= 0.6.2) cannot auto-update to a
  Velopack build; those users need a one-time manual reinstall.
