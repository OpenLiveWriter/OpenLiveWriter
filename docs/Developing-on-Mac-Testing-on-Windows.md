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
   (`OpenLiveWriter.UnitTest` by default) in the VM.
4. **run**: starts the built exe, by default
   `C:\olw-build\OpenLiveWriter\src\managed\bin\<Config>\x64\Writer\OpenLiveWriter.exe`.

All guest output streams back to your Mac terminal, and guest exit codes
propagate, so the harness is safe to use in scripts.

## Caveats

- `prlctl exec` runs commands as SYSTEM in the guest. A few unit tests that
  initialize per-user application data fail in that context, and some tests
  assert exact line endings, which can differ depending on checkout line
  endings. A small number of such failures is environmental, not a harness
  bug.
- `run` starts the exe in the guest. If no interactive user is logged in at
  the VM console, the window may not be visible.

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
