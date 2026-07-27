#!/usr/bin/env bash
# Build, test, and run the Open Live Writer Windows app inside a Windows
# Parallels VM, driven from macOS via prlctl.
#
# Usage:
#   ./scripts/vm-test.sh            # sync + build (same as "all")
#   ./scripts/vm-test.sh sync       # robocopy sources from the Mac share into the VM
#   ./scripts/vm-test.sh build      # sync, then build the solution in the VM
#   ./scripts/vm-test.sh test       # run headless tests in the VM (extra args are
#                                   # passed through to dotnet test)
#   ./scripts/vm-test.sh run        # launch the built OpenLiveWriter.exe in the VM
#
# Configuration (env vars):
#   OLW_VM_NAME          Parallels VM name                 (default: "Windows 11")
#   OLW_VM_BUILD_DIR     VM-local build root               (default: C:\olw-build)
#   OLW_CONFIG           Build configuration               (default: Debug)
#   OLW_SRC_ROOT         Mac source root                   (default: this repo)
#   OLW_VM_TEST_PROJECT  Test project, repo-relative       (default: src\managed\OpenLiveWriter.UnitTest)
#   OLW_VM_TEST_USER     User to run tests as: "system" (default, via prlctl exec)
#                        or "current" (the logged-on desktop user; needed for
#                        WebView2 live tests and profile-dependent tests)
#   OLW_VM_EXE           Guest path to the built exe       (default: computed from OLW_CONFIG)
#   OLW_BLOGGER_CLIENT_ID / OLW_BLOGGER_CLIENT_SECRET
#                        Real Blogger OAuth credentials for the generated
#                        GoogleBloggerv3Secrets.json (default: placeholders)
#
# Requires Parallels Shared Folders: the Mac home directory must be visible
# in the guest as \\Mac\Home. See docs/Developing-on-Mac-Testing-on-Windows.md.

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"

OLW_VM_NAME="${OLW_VM_NAME:-Windows 11}"
OLW_VM_BUILD_DIR="${OLW_VM_BUILD_DIR:-C:\\olw-build}"
OLW_CONFIG="${OLW_CONFIG:-Debug}"
OLW_SRC_ROOT="${OLW_SRC_ROOT:-$ROOT}"
OLW_VM_TEST_PROJECT="${OLW_VM_TEST_PROJECT:-src\\managed\\OpenLiveWriter.UnitTest}"
OLW_VM_TEST_USER="${OLW_VM_TEST_USER:-system}"

die() {
  echo "error: $*" >&2
  exit 1
}

command -v prlctl >/dev/null 2>&1 || die "prlctl not found. Is Parallels Desktop installed?"

# The Mac source root must live under the Mac home directory, because only
# the home directory is shared into the guest by default.
case "$OLW_SRC_ROOT" in
  "$HOME"/*) ;;
  *) die "OLW_SRC_ROOT ($OLW_SRC_ROOT) is not under your Mac home directory ($HOME).
   Parallels Shared Folders only exposes \\Mac\\Home by default. Either move the
   checkout under $HOME or share the parent directory in the VM configuration." ;;
esac

SRC_REL="${OLW_SRC_ROOT#"$HOME"/}"
GUEST_SRC="\\\\Mac\\Home\\${SRC_REL//\//\\}"

# Dedicated VM-local destination. /MIR mirrors into it, so it must never be
# empty, a root, or the source share itself.
VM_DIR="${OLW_VM_BUILD_DIR}\\OpenLiveWriter"
case "$VM_DIR" in
  ""|"\\"|"/"|?:\\|?:)
    die "refusing to mirror into unsafe destination: '$VM_DIR'" ;;
esac
[ "$VM_DIR" = "$GUEST_SRC" ] && die "refusing to mirror the source share onto itself: '$VM_DIR'"

SOLUTION='src\managed\writer.sln'
GUEST_EXE="${OLW_VM_EXE:-${VM_DIR}\\src\\managed\\OpenLiveWriter\\bin\\${OLW_CONFIG}\\OpenLiveWriter.exe}"

# Run a command in the guest. Output streams back to this terminal.
vm_exec() {
  prlctl exec "$OLW_VM_NAME" cmd /c "$1"
}

do_sync() {
  echo "==> Syncing $GUEST_SRC -> $VM_DIR (robocopy /MIR)"
  # robocopy exit codes 0-7 are success; 8+ is failure. Use "if errorlevel"
  # (runtime check) since %ERRORLEVEL% would expand before robocopy runs.
  vm_exec "robocopy \"$GUEST_SRC\" \"$VM_DIR\" /MIR /XD .git bin obj artifacts node_modules /NFL /NDL /NP /R:2 /W:2 & if errorlevel 8 (exit /b 1) else (exit /b 0)"
  echo "==> Sync complete"
}

do_build() {
  do_sync
  # GlobalAssemblyVersionInfo.cs is generated, not committed (see
  # writer.build.targets). MarketXmlGenerator includes it unconditionally, so
  # generate it up front: on a fresh VM-local copy the C++ Ribbon project that
  # normally produces it may build too late (or not at all).
  local version b64
  version="$(head -n 1 "$OLW_SRC_ROOT/version.txt" | tr -d '\r')"
  b64="$(printf '[assembly: System.Reflection.AssemblyVersion("%s")]\n[assembly: System.Reflection.AssemblyFileVersion("%s")]\n' "$version" "$version" | base64)"
  vm_exec "powershell -NoProfile -Command \"[IO.File]::WriteAllText('${VM_DIR}\\src\\managed\\GlobalAssemblyVersionInfo.cs', [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('${b64}')))\""
  # GoogleBloggerv3Secrets.json is also generated (gitignored) and embedded by
  # the BlogClient project. Generate a placeholder; override with
  # OLW_BLOGGER_CLIENT_ID / OLW_BLOGGER_CLIENT_SECRET (see
  # docs/Connecting to Blogger From a Local Build.md).
  local secrets_b64
  secrets_b64="$(printf '{ "installed": { "client_id": "%s", "client_secret": "%s" } }\n' "${OLW_BLOGGER_CLIENT_ID:-PASTE_YOUR_CLIENT_ID_HERE}" "${OLW_BLOGGER_CLIENT_SECRET:-PASTE_YOUR_CLIENT_SECRET_HERE}" | base64)"
  vm_exec "powershell -NoProfile -Command \"[IO.File]::WriteAllText('${VM_DIR}\\src\\managed\\OpenLiveWriter.BlogClient\\Clients\\GoogleBloggerv3Secrets.json', [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('${secrets_b64}')))\""
  echo "==> Building $SOLUTION ($OLW_CONFIG) in the VM"
  # CoreServices embeds Marketization\Markets.xml and Master.xml, which are
  # generated (gitignored): Master.xml is copied from intl\markets and
  # Markets.xml is produced by MarketXmlGenerator. No build target does this
  # yet on this branch, so do it here before the solution build.
  vm_exec "cd /d \"$VM_DIR\" && dotnet build src\\managed\\MarketXmlGenerator\\MarketXmlGenerator.csproj --nologo --verbosity minimal --configuration $OLW_CONFIG && src\\managed\\MarketXmlGenerator\\bin\\$OLW_CONFIG\\MarketXmlGenerator.exe intl\\markets src\\managed\\OpenLiveWriter.CoreServices\\Marketization\\Markets.xml && copy /y intl\\markets\\Master.xml src\\managed\\OpenLiveWriter.CoreServices\\Marketization\\ >nul"
  vm_exec "cd /d \"$VM_DIR\" && dotnet restore $SOLUTION && dotnet msbuild $SOLUTION /nologo /maxcpucount /verbosity:minimal /p:Configuration=$OLW_CONFIG"
  echo "==> Build complete: $GUEST_EXE"
}

do_test() {
  echo "==> Running tests ($OLW_VM_TEST_PROJECT) in the VM"
  case "$OLW_VM_TEST_USER" in
    system)
      vm_exec "cd /d \"$VM_DIR\" && dotnet test \"$OLW_VM_TEST_PROJECT\" --nologo --verbosity minimal --configuration $OLW_CONFIG $*"
      ;;
    current)
      # Run as the logged-on desktop user (needed for WebView2 live tests and
      # tests that touch the user profile; SYSTEM has no Personal folder).
      # dotnet is not on the logged-on user's PATH, so use the full path.
      echo "==> (as the logged-on user)"
      prlctl exec "$OLW_VM_NAME" --current-user cmd /c "cd /d \"$VM_DIR\" && \"C:\\Program Files\\dotnet\\dotnet.exe\" test \"$OLW_VM_TEST_PROJECT\" --nologo --verbosity minimal --configuration $OLW_CONFIG $*"
      ;;
    *)
      die "unknown OLW_VM_TEST_USER '$OLW_VM_TEST_USER' (expected system or current)" ;;
  esac
}

do_run() {
  echo "==> Launching $GUEST_EXE in the VM (as the logged-on user)"
  # prlctl exec runs as SYSTEM in session 0 by default: GUI windows are
  # invisible there, and Open Live Writer exits because the SYSTEM profile
  # has no Personal (Documents) folder. --current-user authenticates as the
  # logged-on console user via Parallels Tools, so the window appears on
  # the VM desktop. prlctl stays attached while the GUI app runs, so give
  # it a few seconds to spawn and then drop the channel.
  prlctl exec "$OLW_VM_NAME" --current-user cmd /c "start \"\" \"$GUEST_EXE\"" &
  local prl_pid=$!
  sleep 10
  kill "$prl_pid" 2>/dev/null || true
  echo "==> Launched (check the VM window)"
}

case "${1:-all}" in
  sync)  do_sync ;;
  build) do_build ;;
  test)  shift; do_test "$@" ;;
  run)   do_run ;;
  all)
    do_build
    echo
    echo "Next steps:"
    echo "  $0 test   # run headless tests in the VM"
    echo "  $0 run    # launch Open Live Writer in the VM"
    ;;
  -h|--help)
    sed -n '2,24p' "$0"
    ;;
  *)
    die "unknown subcommand '$1' (expected sync, build, test, run, or all)"
    ;;
esac
