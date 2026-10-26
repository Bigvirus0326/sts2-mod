#!/usr/bin/env bash
# 게임 PC(Linux/macOS/Steam Deck)에서 이 저장소의 모드들(최신 빌드)을 mods 폴더에 설치합니다.
#   curl -sSL https://raw.githubusercontent.com/Bigvirus0326/sts2-mod/main/install.sh | bash
# 일부만:  ... | MODS="SpireMod,FireCards" bash
set -euo pipefail
REPO=Bigvirus0326/sts2-mod

if [[ -n "${STS2_DIR:-}" ]]; then GAME="$STS2_DIR"
elif [[ "$(uname)" == Darwin ]]; then GAME="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2"
else GAME="$HOME/.local/share/Steam/steamapps/common/Slay the Spire 2"; fi
[[ -d "$GAME" ]] || { echo "게임 폴더를 못 찾았어요. STS2_DIR=경로 로 지정해 다시 실행하세요."; exit 1; }
if [[ "$(uname)" == Darwin ]]; then MODS_DIR="$GAME/SlayTheSpire2.app/Contents/MacOS/mods"; else MODS_DIR="$GAME/mods"; fi
mkdir -p "$MODS_DIR"

if [[ -n "${MODS:-}" ]]; then
  names=$(echo "$MODS" | tr ',' '\n')
else
  names=$(curl -sSf "https://api.github.com/repos/$REPO/releases?per_page=100" \
    | python3 -c "import sys,json;[print(r['tag_name'][:-7]) for r in json.load(sys.stdin) if r['tag_name'].endswith('-latest')]")
fi
[[ -n "$names" ]] || { echo "설치할 모드를 찾지 못했어요."; exit 1; }

tmp=$(mktemp -d)
for MOD in $names; do
  MOD=$(echo "$MOD" | xargs)
  curl -sSfL "https://github.com/$REPO/releases/download/$MOD-latest/$MOD.zip" -o "$tmp/$MOD.zip"
  rm -rf "$MODS_DIR/$MOD"
  unzip -q "$tmp/$MOD.zip" -d "$MODS_DIR"
  echo "[$MOD] 설치 완료: $MODS_DIR/$MOD"
done
rm -rf "$tmp"
[[ -d "$MODS_DIR/BaseLib" ]] || echo "※ BaseLib 모드도 필요해요 (창작마당 구독 시 무시)."
