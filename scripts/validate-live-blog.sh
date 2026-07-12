#!/usr/bin/env bash
# Run the opt-in LiveBlog integration tests when OLW_LIVEBLOG_* env vars are set.
# Does not fail when vars are missing — prints guidance and exits 0 so default CI
# pipelines can call this script without breaking.
#
# Usage:
#   OLW_LIVEBLOG_ENDPOINT=https://blog.example.com/xmlrpc.php \
#   OLW_LIVEBLOG_BLOGID=1 OLW_LIVEBLOG_USER=me OLW_LIVEBLOG_PASS=secret \
#   ./scripts/validate-live-blog.sh
#
# Optional: OLW_LIVEBLOG_PUBLISH=true to exercise the published path (default: draft).

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
TEST_PROJECT="$ROOT/src/managed/OpenLiveWriter.EditorTests.Automated"

required=(OLW_LIVEBLOG_ENDPOINT OLW_LIVEBLOG_BLOGID OLW_LIVEBLOG_USER OLW_LIVEBLOG_PASS)
missing=()
for var in "${required[@]}"; do
  if [[ -z "${!var:-}" ]]; then
    missing+=("$var")
  fi
done

if [[ ${#missing[@]} -gt 0 ]]; then
  echo "Live blog validation skipped — set these env vars to run:"
  printf '  %s\n' "${required[@]}"
  echo ""
  echo "Example:"
  echo "  OLW_LIVEBLOG_ENDPOINT=https://blog.example.com/xmlrpc.php \\"
  echo "  OLW_LIVEBLOG_BLOGID=1 OLW_LIVEBLOG_USER=me OLW_LIVEBLOG_PASS=secret \\"
  echo "  $0"
  exit 0
fi

echo "==> Running LiveBlog tests (explicit)..."
dotnet test "$TEST_PROJECT" \
  --filter "Category=LiveBlog" \
  -- NUnit.Explicit=true
