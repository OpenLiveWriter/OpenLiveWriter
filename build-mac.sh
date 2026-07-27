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
  <key>CFBundleIconFile</key>
  <string>AppIcon</string>
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

# App icon: render the committed 1024px source (packaging/mac/AppIcon-1024.png,
# the OLW cloud glyph from the Windows Writer.ico) into a full .icns and wire
# it into the bundle.
ICON_SRC="$ROOT/packaging/mac/AppIcon-1024.png"
if [[ -f "$ICON_SRC" ]]; then
  ICONSET="$OUT/AppIcon.iconset"
  rm -rf "$ICONSET"
  mkdir -p "$ICONSET"
  for spec in "16:icon_16x16.png" "32:icon_16x16@2x.png" "32:icon_32x32.png" "64:icon_32x32@2x.png" "128:icon_128x128.png" "256:icon_128x128@2x.png" "256:icon_256x256.png" "512:icon_256x256@2x.png" "512:icon_512x512.png" "1024:icon_512x512@2x.png"; do
    px="${spec%%:*}"
    name="${spec##*:}"
    sips -z "$px" "$px" "$ICON_SRC" --out "$ICONSET/$name" >/dev/null
  done
  mkdir -p "$CONTENTS/Resources"
  iconutil -c icns "$ICONSET" -o "$CONTENTS/Resources/AppIcon.icns"
  rm -rf "$ICONSET"
  echo "==> App icon: $CONTENTS/Resources/AppIcon.icns"
else
  echo "==> WARNING: $ICON_SRC not found — bundle will have no icon"
fi

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
