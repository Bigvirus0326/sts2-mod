# SpireMod — 슬레이 더 스파이어 2 모드

게임이 없는 곳(폰, 다른 PC)에서도 **Claude로 코드**, **GPT로 그림**을 만들고,
GitHub가 자동으로 빌드해 설치 파일을 만들어 주는 모드 프로젝트입니다.

[![Build mod](https://github.com/Bigvirus0326/sts2-mod/actions/workflows/build.yml/badge.svg)](https://github.com/Bigvirus0326/sts2-mod/actions/workflows/build.yml)
· 최신 빌드: [Releases → latest](https://github.com/Bigvirus0326/sts2-mod/releases/tag/latest)

## 어떻게 돌아가나
```
 폰/PC의 Claude ─(코드)─┐
                        ├─▶ GitHub (main) ─▶ Actions 자동 빌드 ─▶ Releases "latest" (SpireMod.zip)
 GPT ─────────(그림)─────┘                                               │
                                                     게임 PC에서 install.ps1 한 번 실행 ◀┘
```

## 1. Claude로 작업 (폰 OK)
Claude 앱에서 새 대화를 열고 이 저장소(`Bigvirus0326/sts2-mod`)를 연결한 뒤 그냥 말하면 됩니다.
> "공격력 12에 화상 2를 주는 '불꽃 베기' 카드 추가해줘"

Claude는 `CLAUDE.md`/`AGENTS.md` 규칙대로 코드를 쓰고, 푸시하고, 빌드 성공까지 확인합니다.
GPT가 만든 그림을 채팅에 첨부해 "이걸 불꽃 베기 카드 그림으로 넣어줘"라고 해도 됩니다.

## 2. GPT로 그림 넣기
방법 중 편한 것 하나:
- **ChatGPT(Codex)에 이 저장소 연결** → "AGENTS.md 규칙대로 `fire_slash` 카드 그림을 만들어 `art/cards/`에 넣어줘"
- **GitHub 앱/웹에서 직접 업로드** → `art/cards/` 폴더 열기 → Add file → Upload → 파일 이름을 `fire_slash.png`처럼
- **Claude 채팅에 첨부** → Claude가 맞는 폴더에 넣어 줌

크기는 신경 쓰지 않아도 됩니다. 빌드할 때 자동으로 게임 규격에 맞춰 잘립니다. 자세한 규칙은 [AGENTS.md](AGENTS.md#그림-규칙-gpt-담당).

## 3. 게임 PC에 설치
처음 한 번: 스팀 창작마당에서 **BaseLib** 모드 구독.
그 다음 업데이트할 때마다 PowerShell에 한 줄:
```powershell
irm https://raw.githubusercontent.com/Bigvirus0326/sts2-mod/main/install.ps1 | iex
```
(직접 하려면 Releases의 `SpireMod.zip`을 풀어 `Slay the Spire 2/mods/` 에 넣기.)
게임 실행 → 설정 → 모드 설정에서 SpireMod 확인.

## PC에서 직접 개발하고 싶다면 (선택)
.NET 9 SDK 설치 후 `python3 tools/process_art.py && dotnet build -c ExportRelease`.
PC에 게임이 깔려 있으면 실제 게임 DLL을 쓰고, 빌드 후 mods 폴더로 자동 복사합니다.
