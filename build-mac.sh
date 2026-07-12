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

echo "==> Building Open Live Writer for $RID ($CONFIG)"

dotnet publish "$PROJECT" \
  -c "$CONFIG" \
  -r "$RID" \
  --self-contained true \
  -p:PublishSingleFile=false \
  -o "$OUT/publish"

PUBLISH_DIR="$OUT/publish"
BUNDLE_DIR="$OUT/$APP_NAME.app"
CONTENTS="$BUNDLE_DIR/Contents"
MACOS="$CONTENTS/MacOS"

rm -rf "$BUNDLE_DIR"
mkdir -p "$MACOS"

# Copy the published payload into the bundle's MacOS folder.
cp -R "$PUBLISH_DIR/"* "$MACOS/"

# Info.plist with CFBundleName "Open Live Writer".
cat > "$CONTENTS/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key>
  <string>en</string>
  <key>CFBundleExecutable</key>
  <string>$EXE_NAME</string>
  <key>CFBundleIdentifier</key>
  <string>$BUNDLE_ID</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundleName</key>
  <string>$APP_NAME</string>
  <key>CFBundleDisplayName</key>
  <string>$APP_NAME</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>1.0</string>
  <key>CFBundleVersion</key>
  <string>1</string>
  <key>LSMinimumSystemVersion</key>
  <string>12.0</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
EOF

chmod +x "$MACOS/$EXE_NAME"

echo "==> Published bundle: $BUNDLE_DIR"
echo "==> Run headless tests: dotnet test src/managed/OpenLiveWriter.EditorTests.Automated"

# Optional DMG creation (set OLW_CREATE_DMG=1 to enable).
if [[ "${OLW_CREATE_DMG:-}" == "1" ]]; then
  DMG_PATH="$OUT/$APP_NAME.dmg"
  DMG_STAGE="$OUT/dmg-stage"
  rm -rf "$DMG_STAGE" "$DMG_PATH"
  mkdir -p "$DMG_STAGE"
  cp -R "$BUNDLE_DIR" "$DMG_STAGE/"
  ln -s /Applications "$DMG_STAGE/Applications"
  hdiutil create -volname "$APP_NAME" -srcfolder "$DMG_STAGE" -ov -format UDZO "$DMG_PATH"
  rm -rf "$DMG_STAGE"
  echo "==> Created DMG: $DMG_PATH"
fi

# Optional code signing / notarization (requires certificates; not run in default CI).
# Set these env vars locally when you have Apple Developer credentials:
#   OLW_CODESIGN_IDENTITY   — "Developer ID Application: Your Name (TEAMID)"
#   OLW_NOTARY_PROFILE      — notarytool keychain profile name (from xcrun notarytool store-credentials)
# Example local signing (not executed automatically):
#   codesign --deep --force --options runtime --sign "$OLW_CODESIGN_IDENTITY" "$BUNDLE_DIR"
#   ditto -c -k --keepParent "$BUNDLE_DIR" "$OUT/$APP_NAME.zip"
#   xcrun notarytool submit "$OUT/$APP_NAME.zip" --keychain-profile "$OLW_NOTARY_PROFILE" --wait
#   xcrun stapler staple "$BUNDLE_DIR"

# TODO(M5): wire OLW_CODESIGN_IDENTITY / OLW_NOTARY_PROFILE when certs are available in CI.
