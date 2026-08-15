#!/usr/bin/env bash
# Capture side-by-side ribbon screenshots of two Open Live Writer instances
# (e.g. the new .NET 10 build and the installed 0.6.2) inside the Parallels
# Windows VM, then generate a section-by-section comparison report.
#
# Usage:
#   scripts/qa/ribbon-compare.sh [outdir]
#
# Configuration (env vars):
#   OLW_VM_NAME   Parallels VM name (default: "Windows 11")
#   OLW_OLD_EXE   Guest path to the reference build (default: installed 0.6.2)
#   OLW_NEW_EXE   Guest path to the new build (default: harness Debug output)
#
# Output: <outdir>/old.png, <outdir>/new.png, <outdir>/compare-*.png,
#         <outdir>/report.txt

set -euo pipefail

VM="${OLW_VM_NAME:-Windows 11}"
OLD_EXE="${OLW_OLD_EXE:-C:\\Users\\doug\\AppData\\Local\\OpenLiveWriter\\app-0.6.2\\OpenLiveWriter.exe}"
NEW_EXE="${OLW_NEW_EXE:-C:\\olw-build\\OpenLiveWriter\\src\\managed\\OpenLiveWriter\\bin\\Debug\\OpenLiveWriter.exe}"
OUT="${1:-/tmp/ribbon-compare}"
mkdir -p "$OUT"

command -v prlctl >/dev/null 2>&1 || { echo "error: prlctl not found" >&2; exit 1; }
command -v python3 >/dev/null 2>&1 || { echo "error: python3 not found" >&2; exit 1; }

# Launch a GUI app as the logged-on user; prlctl stays attached, so drop it.
launch() {
  prlctl exec "$VM" --current-user cmd /c "start \"\" \"$1\"" >/dev/null 2>&1 &
  local pid=$!
  sleep 15
  kill "$pid" 2>/dev/null || true
}

is_running() {
  prlctl exec "$VM" cmd /c "tasklist /fi \"imagename eq OpenLiveWriter.exe\" /fo csv /nh" 2>/dev/null | grep -qi "OpenLiveWriter.exe"
}

# Maximize and foreground the window of the process whose ExecutablePath
# matches $1, via PowerShell as the logged-on user.
focus_window() {
  local exe_path="$1"
  local ps enc
  ps="$(cat <<EOF
\$ErrorActionPreference = 'SilentlyContinue'
\$sig = @"
using System;
using System.Runtime.InteropServices;
public class QaWin {
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
}
"@
Add-Type -TypeDefinition \$sig
\$proc = Get-CimInstance Win32_Process | Where-Object { \$_.ExecutablePath -eq '$exe_path' } | Select-Object -First 1
if (-not \$proc) { 'NO_PROCESS'; exit }
\$p = Get-Process -Id \$proc.ProcessId
[QaWin]::ShowWindow(\$p.MainWindowHandle, 3) | Out-Null
[QaWin]::SetForegroundWindow(\$p.MainWindowHandle) | Out-Null
'FOCUSED'
EOF
)"
  enc="$(printf '%s' "$ps" | iconv -f UTF-8 -t UTF-16LE | base64)"
  prlctl exec "$VM" --current-user powershell -NoProfile -EncodedCommand "$enc" 2>/dev/null | grep -vE "CLIXML|^<Objs|^</Objs" | head -1
}

# Ensure both instances are running.
if ! is_running; then
  launch "$OLD_EXE"
fi
# If only one instance is up we cannot tell which; just try launching both.
COUNT=$(prlctl exec "$VM" cmd /c 'tasklist /fi "imagename eq OpenLiveWriter.exe" /fo csv /nh' 2>/dev/null | grep -c "OpenLiveWriter.exe" || true)
if [ "${COUNT:-0}" -lt 2 ]; then
  launch "$NEW_EXE"
fi

echo "==> Capturing reference (old): $OLD_EXE"
focus_window "$OLD_EXE"
sleep 2
prlctl capture "$VM" --file "$OUT/old.png" >/dev/null

echo "==> Capturing new build: $NEW_EXE"
focus_window "$NEW_EXE"
sleep 2
prlctl capture "$VM" --file "$OUT/new.png" >/dev/null

echo "==> Analyzing"
python3 "$(cd "$(dirname "$0")" && pwd)/ribbon_compare.py" "$OUT/old.png" "$OUT/new.png" "$OUT"
