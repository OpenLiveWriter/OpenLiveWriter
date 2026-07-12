#!/usr/bin/env bash
# Capture Avalonia shell screenshots + layout dumps for visual review.
# Usage:
#   ./scripts/ui-review.sh           # build, capture, print paths, open folder (macOS)
#   ./scripts/ui-review.sh --no-open # skip opening Finder
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

OPEN_FOLDER=1
for arg in "$@"; do
  case "$arg" in
    --no-open) OPEN_FOLDER=0 ;;
    -h|--help)
      echo "Usage: $0 [--no-open]"
      echo "Writes PNGs + layout dumps to artifacts/ui-review/"
      exit 0
      ;;
  esac
done

OUT="$ROOT/artifacts/ui-review"
mkdir -p "$OUT"

echo "==> Building + running UiReview capture harness"
dotnet test "$ROOT/src/managed/OpenLiveWriter.EditorTests.Automated" \
  --filter "Category=UiReview" \
  --nologo \
  -v minimal

echo
echo "==> Artifacts"
if [[ -d "$OUT" ]]; then
  ls -la "$OUT" || true
  echo
  echo "Index: $OUT/INDEX.md"
  if [[ -f "$OUT/INDEX.md" ]]; then
    cat "$OUT/INDEX.md"
  fi
else
  echo "No output directory at $OUT — capture may have failed."
  exit 1
fi

if [[ "$OPEN_FOLDER" -eq 1 ]] && [[ "$(uname -s)" == "Darwin" ]]; then
  open "$OUT"
fi
