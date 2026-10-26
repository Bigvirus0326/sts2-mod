#!/usr/bin/env bash
# 게임 PC(Linux/macOS/Steam Deck)에서 최신 빌드를 받아 mods 폴더에 설치합니다.
#   curl -sSL https://raw.githubusercontent.com/Bigvirus0326/sts2-mod/main/install.sh | bash
set -euo pipefail
REPO=Bigvirus0326/sts2-mod
MOD=SpireMod

if [[ -n "${STS2_DIR:-}" ]]; then GAME="$STS2_DIR"
elif [[ "$(uname)" == Darwin ]]; then GAME="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2"
else GAME="$HOME/.local/share/Steam/steamapps/common/Slay the Spire 2"; fi
[[ -d "$GAME" ]] || { echo "게임 폴더를 못 찾았어요. STS2_DIR=경로 로 지정해 다시 실행하세요."; exit 1; }

if [[ "$(uname)" == Darwin ]]; then MODS="$GAME/SlayTheSpire2.app/Contents/MacOS/mods"; else MODS="$GAME/mods"; fi
mkdir -p "$MODS"
tmp=$(mktemp -d)
curl -sSfL "https://github.com/$REPO/releases/download/latest/$MOD.zip" -o "$tmp/$MOD.zip"
rm -rf "$MODS/$MOD"
unzip -q "$tmp/$MOD.zip" -d "$MODS"
rm -rf "$tmp"
echo "설치 완료: $MODS/$MOD"
[[ -d "$MODS/BaseLib" ]] || echo "※ BaseLib 모드도 mods 폴더에 필요해요."
