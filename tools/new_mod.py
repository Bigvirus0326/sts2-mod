#!/usr/bin/env python3
"""
새 모드를 templates/EmptyMod 에서 만들어 mods/<이름>/ 에 둡니다.

  python3 tools/new_mod.py FireCards
  python3 tools/new_mod.py FireCards --description "불꽃 카드 모음"

이름은 영어 PascalCase(예: FireCards). 코드 네임스페이스·DLL·폴더 이름으로 쓰입니다.
"""
from __future__ import annotations

import argparse
import json
import re
import shutil
import sys
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
TEMPLATE = ROOT / "templates" / "EmptyMod"
TOKEN = "ModTemplate"
TEXT_EXT = {".cs", ".csproj", ".json", ".sln", ".godot", ".cfg", ".md", ".txt", ".import", ".tscn", ".tres"}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("name", help="모드 이름 (영어 PascalCase, 예: FireCards)")
    ap.add_argument("--description", default="", help="모드 설명")
    ap.add_argument("--author", default="", help="제작자 (기본: 템플릿 값)")
    a = ap.parse_args()

    if not re.fullmatch(r"[A-Z][A-Za-z0-9]{1,40}", a.name):
        sys.exit("이름은 영어 대문자로 시작하는 PascalCase여야 합니다 (예: FireCards)")
    dst = ROOT / "mods" / a.name
    if dst.exists():
        sys.exit(f"이미 있습니다: {dst.relative_to(ROOT)}")

    shutil.copytree(TEMPLATE, dst)

    # 파일/폴더 이름 치환 (깊은 경로부터)
    for p in sorted(dst.rglob("*"), key=lambda x: len(x.parts), reverse=True):
        if TOKEN in p.name:
            p.rename(p.with_name(p.name.replace(TOKEN, a.name)))

    # 내용 치환
    for p in dst.rglob("*"):
        if p.is_file() and p.suffix.lower() in TEXT_EXT:
            s = p.read_text(encoding="utf-8")
            if TOKEN in s:
                p.write_text(s.replace(TOKEN, a.name), encoding="utf-8", newline="")

    # 솔루션 GUID 새로 발급
    sln = dst / f"{a.name}.sln"
    s = sln.read_text(encoding="utf-8")
    old = re.search(r'"\{([0-9A-F-]{36})\}"', s).group(1)
    sln.write_text(s.replace(old, str(uuid.uuid4()).upper()), encoding="utf-8", newline="")

    # 매니페스트
    mf = dst / f"{a.name}.json"
    data = json.loads(mf.read_text(encoding="utf-8"))
    if a.description:
        data["description"] = a.description
    if a.author:
        data["author"] = a.author
    data["version"] = "v0.1.0"
    mf.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(f"만들었습니다: mods/{a.name}/  (코드: mods/{a.name}/Code, 그림: mods/{a.name}/art)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
