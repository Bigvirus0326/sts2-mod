#!/usr/bin/env python3
"""
두 커밋 사이에 바뀐 파일을 보고, 다시 빌드해야 할 모드 목록을 JSON으로 출력합니다. (CI용)

  python3 tools/changed_mods.py <base_sha> <head_sha>    -> ["SpireMod", "FireCards"]
  python3 tools/changed_mods.py --all                    -> 모든 모드

규칙
- mods/<이름>/ 아래만 바뀌면 그 모드만.
- 공통 파일(아래 GLOBAL)이 바뀌면 전부.
- base를 알 수 없으면(첫 푸시, 강제 푸시 등) 전부.
- 문서(*.md), templates/ 만 바뀌면 아무것도 빌드하지 않음.
"""
from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
GLOBAL = (
    ".github/workflows/",
    "tools/process_art.py",
    "Directory.Build.props",
    "shared/",
)


def all_mods() -> list[str]:
    mods = ROOT / "mods"
    return sorted(p.name for p in mods.iterdir() if (p / f"{p.name}.csproj").exists()) if mods.is_dir() else []


def changed_files(base: str, head: str) -> list[str] | None:
    if not base or set(base) == {"0"}:
        return None
    try:
        out = subprocess.run(["git", "diff", "--name-only", base, head], cwd=ROOT,
                             check=True, capture_output=True, text=True).stdout
    except subprocess.CalledProcessError:
        return None
    return [l for l in out.splitlines() if l]


def main() -> int:
    mods = all_mods()
    if "--all" in sys.argv or len(sys.argv) < 3:
        print(json.dumps(mods))
        return 0
    files = changed_files(sys.argv[1], sys.argv[2])
    if files is None or any(f.startswith(GLOBAL) for f in files):
        print(json.dumps(mods))
        return 0
    hit = set()
    for f in files:
        parts = f.split("/")
        if len(parts) >= 3 and parts[0] == "mods" and parts[1] in mods and not f.endswith(".md"):
            hit.add(parts[1])
    print(json.dumps(sorted(hit)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
