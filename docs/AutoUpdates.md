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
| `CheckForBetaUpdates` | `false` | `true` = GitHub prereleases / nightly website feed |
| `UpdateFeedType` | `github` | `github` or `website` |
| `GitHubRepoUrl` | `https://github.com/OpenLiveWriter/OpenLiveWriter` | Repo whose Releases feed the check |
| `CheckUpdatesUrl` | `https://openlivewriter.com/releases/stable` | Website feed (stable) |
| `CheckBetaUpdatesUrl` | `https://openlivewriter.com/releases/nightly` | Website feed (beta/nightly) |

## Publishing a Windows release

Tag with `win-v*` to run `.github/workflows/windows-release.yml`, which
builds, packs with `vpk pack --channel stable`, and publishes the Velopack
artifacts (`releases.stable.json`, nupkg, `OpenLiveWriterSetup.exe`) to a
GitHub Release. The `GithubSource` in the app finds the update there.

Tag names should match the version in `version.txt` (e.g. `win-v0.7.0`
when `version.txt` is `0.7.0.0`), because Velopack compares release
versions, not tag names; the tag is just the trigger.

The workflow signs via SignClient when `SignClientUser`/`SignClientSecret`
secrets are set; otherwise it publishes unsigned builds (SmartScreen
warning on install).

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
