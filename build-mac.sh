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

# TODO(M5): codesign --deep --force --sign "Developer ID Application: ..." "$BUNDLE_DIR"
# TODO(M5): xcrun notarytool submit ... --wait
