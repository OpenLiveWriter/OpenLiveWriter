#!/usr/bin/env bash
# Create a distributable DMG from an existing .app bundle.
# The DMG contains the app plus an /Applications symlink for drag-install.
#
# Usage: packaging/mac/create-dmg.sh "<app bundle>" "<dmg output>"
#
# Called by build-mac.sh when OLW_CREATE_DMG=1, and directly by CI
# (.github/workflows/mac-build.yml) after signing/notarization/stapling so
# the DMG ships the final stapled app.

set -euo pipefail

APP_BUNDLE="${1:?usage: create-dmg.sh <app-bundle> <dmg-output>}"
DMG_PATH="${2:?usage: create-dmg.sh <app-bundle> <dmg-output>}"

APP_NAME="$(basename "$APP_BUNDLE" .app)"
DMG_STAGE="$(dirname "$DMG_PATH")/dmg-stage"

rm -rf "$DMG_STAGE" "$DMG_PATH"
mkdir -p "$DMG_STAGE"
cp -R "$APP_BUNDLE" "$DMG_STAGE/"
ln -s /Applications "$DMG_STAGE/Applications"
hdiutil create -volname "$APP_NAME" -srcfolder "$DMG_STAGE" -ov -format UDZO "$DMG_PATH"
rm -rf "$DMG_STAGE"
echo "==> Created DMG: $DMG_PATH"
