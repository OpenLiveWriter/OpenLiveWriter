# Developing on Mac, Testing on Windows

Open Live Writer is a Windows-only WinForms app, but you can do your day-to-day
editing on macOS and delegate builds, tests, and runs to a Windows 11 VM
running under Parallels Desktop. The `scripts/vm-test.sh` harness drives the
VM from your Mac terminal via `prlctl`, so you never have to leave macOS.

## Prerequisites

- **Parallels Desktop** (tested with Parallels 26) with `prlctl` on your PATH
  (installed with Parallels).
- A **Windows 11 VM** with **Parallels Tools** installed. An ARM64 VM on Apple
  silicon works fine (see the emulation note below).
- In the guest:
  - **.NET 10 SDK** (https://dotnet.microsoft.com/download)
  - **Git for Windows** (optional for the harness itself, needed for normal dev)
  - **Visual Studio 2022** (only needed for debugging or building the C++
    Ribbon project)
- In the Parallels VM configuration, enable **Shared Folders** (and Shared
  Profile) so your Mac home directory is visible in the guest as
  `\\Mac\Home`. The harness computes the guest path from your Mac checkout
  location, so the checkout must live somewhere under your Mac home directory.

## Quick start

From the repo root on your Mac:

```bash
./scripts/vm-test.sh          # sync sources into the VM and build (Debug)
./scripts/vm-test.sh test     # run the headless unit tests in the VM
./scripts/vm-test.sh run      # launch OpenLiveWriter.exe in the VM
```

Other subcommands:

```bash
./scripts/vm-test.sh sync     # just mirror the sources into the VM
./scripts/vm-test.sh build    # sync + restore + build
```

Configuration is via environment variables:

| Variable | Default | Description |
|---|---|---|
| `OLW_VM_NAME` | `Windows 11` | Parallels VM name |
| `OLW_VM_BUILD_DIR` | `C:\olw-build` | VM-local build root |
| `OLW_CONFIG` | `Debug` | Build configuration |
| `OLW_SRC_ROOT` | this repo | Mac source root (must be under `$HOME`) |
| `OLW_VM_TEST_PROJECT` | `src\managed\OpenLiveWriter.UnitTest` | Test project to run |
| `OLW_VM_TEST_USER` | `system` | User to run tests as: `system` (default, via `prlctl exec`) or `current` (the logged-on desktop user) |
| `OLW_VM_EXE` | computed | Guest path to the built exe |
| `OLW_BLOGGER_CLIENT_ID` / `OLW_BLOGGER_CLIENT_SECRET` | placeholders | Real Blogger OAuth credentials baked into the generated `GoogleBloggerv3Secrets.json` |

Example: `OLW_CONFIG=Release ./scripts/vm-test.sh build`

## How it works

1. **sync**: your Mac checkout is visible in the guest via Parallels Shared
   Folders (for example `\\Mac\Home\Documents\Code\OpenLiveWriter`). Building
   directly on the share is slow and flaky, so the harness robocopies the
   sources into a VM-local directory, `C:\olw-build\OpenLiveWriter` by
   default, using `/MIR`. `.git`, `bin`, `obj`, `artifacts`, and
   `node_modules` are excluded, so build outputs never travel back to the Mac.
   Because `/MIR` deletes extra files at the destination, the destination is
   hard-coded to a dedicated subdirectory and the script refuses to run if it
   resolves to an empty path, a drive root, or the source share itself.
2. **build**: generates the files that are gitignored but required by the
   build and not produced by any current MSBuild target:
   `src\managed\GlobalAssemblyVersionInfo.cs` (from `version.txt`),
   `GoogleBloggerv3Secrets.json` (placeholder Blogger credentials), and the
   market XML resources (`Master.xml` copied from `intl\markets`,
   `Markets.xml` produced by running MarketXmlGenerator). It then runs
   `dotnet restore` and `dotnet msbuild src\managed\writer.sln` with the same
   flags as `build.ps1` (`/nologo /maxcpucount /verbosity:minimal
   /p:Configuration=$OLW_CONFIG`).
3. **test**: runs `dotnet test` on the headless test project
   (`OpenLiveWriter.UnitTest` by default) in the VM. Any extra arguments
   after `test` are passed through to `dotnet test` (for example
   `./scripts/vm-test.sh test --filter Category=WebView2`). By default tests
   run as SYSTEM (like any `prlctl exec` command); set
   `OLW_VM_TEST_USER=current` to run them as the logged-on desktop user via
   `prlctl exec --current-user`. WebView2 live tests need the interactive
   user (a real desktop session and user profile); the SYSTEM account has no
   Personal folder. The logged-on user has no `dotnet` on PATH, so that mode
   invokes `C:\Program Files\dotnet\dotnet.exe` directly.
4. **run**: starts the built exe, by default
   `C:\olw-build\OpenLiveWriter\src\managed\OpenLiveWriter\bin\<Config>\OpenLiveWriter.exe`.
   Because `prlctl exec` runs as SYSTEM in session 0 by default (where GUI
   windows are invisible and Open Live Writer exits due to the SYSTEM
   profile having no Personal folder), the harness launches the app with
   `prlctl exec --current-user`, which authenticates as the logged-on
   console user via Parallels Tools, so the window appears on the VM
   desktop. This needs the VM account to match your Mac user credentials;
   if it does not, launch the exe manually in the VM or use
   `prlctl exec --user <name> --password <pwd>`.

All guest output streams back to your Mac terminal, and guest exit codes
propagate, so the harness is safe to use in scripts.

## Caveats

- `prlctl exec` runs commands as SYSTEM in the guest. Tests that need a real
  user profile or desktop session (WebView2 live tests, per-user application
  data) fail in that context; run them with `OLW_VM_TEST_USER=current`.
- `run` needs a user logged in at the VM console and Parallels Tools able
  to authenticate as that user with your Mac credentials; otherwise there
  is no desktop session to show the window in and the launch fails.
- Always kill running app instances before `build`: a running
  OpenLiveWriter.exe locks the dlls in `bin\Debug`, and the build then
  fails with MSB3021/MSB3027 copy errors (or worse, silently leaves stale
  binaries that you end up testing by mistake).

## ARM64 Windows and x64 emulation

On Apple silicon the VM runs Windows 11 ARM64. Open Live Writer builds x64
only (see README.md), so the app runs under the Windows x64 emulation layer.
This works, but builds and test runs are noticeably slower than on native x64
hardware. Budget extra time for the first `dotnet restore` and build.

## Debugging with Visual Studio 2022 in the VM

1. Run `./scripts/vm-test.sh sync` (or `build`) so the VM-local copy is
   current.
2. In the VM, open `C:\olw-build\OpenLiveWriter\src\managed\writer.sln` in
   Visual Studio 2022.
3. Set `OpenLiveWriter` as the startup project and press F5.

Remember the VM-local copy is a mirror: edit on the Mac, re-sync, then debug.
Edits made only in the VM-local copy are deleted by the next `sync`.

## Blog testing docs

If you are exercising real blog accounts from the app running in the VM, see
`docs/Connecting to Blogger From a Local Build.md` and the manual test plans
under `testplan/` (for example `observeBlogAccount.md`). The opt-in LiveBlog
integration tests (`scripts/validate-live-blog.sh`) can also be run in the VM
by setting `OLW_VM_TEST_PROJECT` and the `OLW_LIVEBLOG_*` variables in the
guest.
