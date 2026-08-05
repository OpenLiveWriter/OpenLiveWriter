#!/usr/bin/env bash
# Build and publish Open Live Writer for macOS (Apple Silicon).
# Produces a self-contained .app bundle under ./artifacts/mac-arm64/
#
# TODO(M5): code signing (codesign) and notarization (xcrun notarytool submit).

set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$ROOT/src/managed/OpenLiveWriter.App.Avalonia/OpenLiveWriter.App.Avalonia.csproj"
OUT="$ROOT/artifacts/mac-arm64"
CONFIG="${OLW_CONFIG:-Release}"
RID="${OLW_RID:-osx-arm64}"
APP_NAME="Open Live Writer"
BUNDLE_ID="org.openlivewriter.app"
EXE_NAME="OpenLiveWriter.App.Avalonia"
PACK_ID="${OLW_PACK_ID:-OpenLiveWriter}"
OLW_CHANNEL="${OLW_CHANNEL:-osx}"

# Versioning: OLW_VERSION is the marketing version (CFBundleShortVersionString);
# OLW_BUILD_NUMBER is the monotonic build id (CFBundleVersion). CI stamps both
# from the tag / run number; local builds fall back to version.txt, the same
# source the managed assemblies use, so the two platforms agree instead of the
# Mac bundle reporting a stale hardcoded number. CFBundleShortVersionString
# takes at most three components, so trim a fourth if version.txt has one.
DEFAULT_VERSION="$(head -n 1 "$ROOT/version.txt" 2>/dev/null | tr -d '\r' | cut -d. -f1-3)"
VERSION="${OLW_VERSION:-${DEFAULT_VERSION:-0.0.0}}"
BUILD_NUMBER="${OLW_BUILD_NUMBER:-$(git -C "$ROOT" rev-list --count HEAD 2>/dev/null || echo 1)}"

echo "==> Building Open Live Writer for $RID ($CONFIG)"

dotnet publish "$PROJECT" \
  -c "$CONFIG" \
  -r "$RID" \
  --self-contained true \
  -p:PublishSingleFile=false \
  -o "$OUT/publish"

PUBLISH_DIR="$OUT/publish"
BUNDLE_DIR="$OUT/$APP_NAME.app"
RELEASES_DIR="$OUT/Releases"

# App icon: render the committed 1024px source (packaging/mac/AppIcon-1024.png,
# the OLW cloud glyph from the Windows Writer.ico) into a full .icns for vpk.
ICNS="$OUT/AppIcon.icns"
ICON_SRC="$ROOT/packaging/mac/AppIcon-1024.png"
ICON_ARGS=()
if [[ -f "$ICON_SRC" ]]; then
  ICONSET="$OUT/AppIcon.iconset"
  rm -rf "$ICONSET"; mkdir -p "$ICONSET"
  for spec in "16:icon_16x16.png" "32:icon_16x16@2x.png" "32:icon_32x32.png" "64:icon_32x32@2x.png" "128:icon_128x128.png" "256:icon_128x128@2x.png" "256:icon_256x256.png" "512:icon_256x256@2x.png" "512:icon_512x512.png" "1024:icon_512x512@2x.png"; do
    sips -z "${spec%%:*}" "${spec%%:*}" "$ICON_SRC" --out "$ICONSET/${spec##*:}" >/dev/null
  done
  iconutil -c icns "$ICONSET" -o "$ICNS"
  rm -rf "$ICONSET"
  ICON_ARGS=(--icon "$ICNS")
else
  echo "==> WARNING: $ICON_SRC not found, bundle will have no icon"
fi

# vpk generates its own Info.plist, but gets the version keys wrong for our
# scheme: it writes the 4-part assembly version into CFBundleShortVersionString
# (the key takes at most three components) and the full SemVer, letters and all,
# into CFBundleVersion (which Apple expects to be numeric). Supply our own.
PLIST="$OUT/Info.plist"
cat > "$PLIST" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key><string>en</string>
  <key>CFBundleExecutable</key><string>$EXE_NAME</string>
  <key>CFBundleIdentifier</key><string>$BUNDLE_ID</string>
  <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
  <key>CFBundleName</key><string>$APP_NAME</string>
  <key>CFBundleDisplayName</key><string>$APP_NAME</string>
  <key>CFBundleIconFile</key><string>AppIcon</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleSignature</key><string>????</string>
  <key>CFBundleShortVersionString</key><string>$VERSION</string>
  <key>CFBundleVersion</key><string>$BUILD_NUMBER</string>
  <key>LSMinimumSystemVersion</key><string>12.0</string>
  <key>NSHighResolutionCapable</key><true/>
  <key>NSPrincipalClass</key><string>NSApplication</string>
</dict>
</plist>
EOF

# Velopack version must be 3-part SemVer2; vpk rejects a 4-part version
# outright. Alpha builds therefore publish as <semver>-alpha.<build> so each
# one sorts above the last and installed clients actually see an update.
PACK_VERSION="${OLW_PACK_VERSION:-$VERSION-alpha.$BUILD_NUMBER}"

# vpk 0.0.1251 targets .NET 9; roll forward when only a newer runtime is present.
export DOTNET_ROLL_FORWARD="${DOTNET_ROLL_FORWARD:-Major}"

SIGN_ARGS=()   # expanded with the ${a[@]+...} guard below: macOS bash 3.2
               # treats "${empty[@]}" as unbound under set -u
[[ -n "${OLW_CODESIGN_IDENTITY:-}" ]] && SIGN_ARGS+=(--signAppIdentity "$OLW_CODESIGN_IDENTITY")
[[ -n "${OLW_INSTALL_IDENTITY:-}" ]] && SIGN_ARGS+=(--signInstallIdentity "$OLW_INSTALL_IDENTITY")
[[ -n "${OLW_NOTARY_PROFILE:-}" ]] && SIGN_ARGS+=(--notaryProfile "$OLW_NOTARY_PROFILE")

echo "==> Packing $PACK_VERSION with Velopack"
rm -rf "$RELEASES_DIR" "$BUNDLE_DIR"
vpk pack \
  --packId "$PACK_ID" \
  --packVersion "$PACK_VERSION" \
  --packTitle "$APP_NAME" \
  --packAuthors ".NET Foundation" \
  --packDir "$PUBLISH_DIR" \
  --mainExe "$EXE_NAME" \
  --plist "$PLIST" \
  --channel "$OLW_CHANNEL" \
  --outputDir "$RELEASES_DIR" \
  ${ICON_ARGS[@]+"${ICON_ARGS[@]}"} ${SIGN_ARGS[@]+"${SIGN_ARGS[@]}"}

# Unpack the portable zip so the runnable .app sits where it always has. It is
# the same bundle vpk ships, so what you run locally carries the update
# metadata (sq.version) that the installed build has.
ditto -x -k "$RELEASES_DIR/$PACK_ID-$OLW_CHANNEL-Portable.zip" "$OUT/unpacked"
rm -rf "$OUT/__MACOSX"
mv "$OUT/unpacked/$APP_NAME.app" "$BUNDLE_DIR"
rm -rf "$OUT/unpacked"

echo "==> Bundle:    $BUNDLE_DIR"
echo "==> Installer: $RELEASES_DIR/$PACK_ID-$OLW_CHANNEL-Setup.pkg"
echo "==> Update feed assets in $RELEASES_DIR"
echo "==> Run headless tests: dotnet test src/managed/OpenLiveWriter.EditorTests.Automated"
