# AI 작업 규칙 (Claude · GPT/Codex 공용)

이 저장소는 **슬레이 더 스파이어 2(Slay the Spire 2)** 모드 **여러 개**를 함께 관리합니다.
여러 AI가 번갈아 편집합니다: **Claude = 코드·기획**, **GPT = 그림**. 모든 AI는 이 문서를 먼저 읽고 따르세요.
사용자는 주로 **휴대폰**으로 작업하므로, 답은 짧고 한국어로, 결과(빌드 성공 여부·다운로드 위치)를 중심으로 알려 주세요.

## 시작·마무리 절차 (모든 채팅/세션 공통)
- **시작:** [PROGRESS.md](PROGRESS.md)를 먼저 읽고 "지금 상태"와 "다음에 할 일"을 파악하세요. 사용자에게 다시 묻지 말고 거기서 이어가세요.
- **마무리:** 작업이 끝나면 PROGRESS.md의 "지금 상태"·"다음에 할 일"을 고치고 "기록" 맨 위에 한 줄 추가해서 같이 커밋하세요.
  (그림만 넣은 경우도 한 줄: `2026-10-27 — GPT: SpireMod fire_slash 카드 그림 추가`)

## 핵심 원칙
1. **빌드는 GitHub Actions가 합니다.** 로컬에 게임·.NET이 없어도 됩니다. `main`에 푸시하면 **바뀐 모드만** 자동 빌드되고,
   성공하면 Releases의 `<모드이름>-latest` 에 설치용 zip이 올라갑니다.
2. **푸시 후 Actions 결과를 꼭 확인**하고, 실패하면 로그를 보고 고친 뒤 다시 푸시하세요. 실패한 채로 끝내지 마세요.
3. **게임 원본 파일(sts2.dll, .pck, 추출한 게임 그림 등)은 절대 커밋하지 않습니다.** 저작권 문제. 컴파일은 NuGet 참조 스텁으로 합니다.
4. 사용자가 어떤 모드 얘기인지 말하지 않았고 모드가 여러 개면, 어느 모드인지 먼저 물어보세요.
5. 한 커밋에는 되도록 한 모드만. 그래야 그 모드만 다시 빌드됩니다.

## 폴더 구조
```
mods/<모드>/                  모드 하나 = 폴더 하나 (각자 독립 빌드·릴리스)
  Code/                       C# 코드 (진입점: Code/MainFile.cs)
  art/                        ✅ 그림 원본은 여기에만
  <모드>/localization/        게임 내 텍스트 (eng/, kor/ … JSON)
  <모드>/images/              ⚠️ 자동 생성 — 직접 넣지 말 것 (art/에서 변환, 깃에 안 올라감)
  <모드>.json                 모드 매니페스트 (이름·버전·의존성)
  <모드>.csproj               C# 프로젝트
templates/EmptyMod/           새 모드용 템플릿 (직접 고치지 말 것, 빌드 안 됨)
tools/new_mod.py              새 모드 만들기
tools/process_art.py          art/ → images/ 변환기
tools/changed_mods.py         바뀐 모드 판별 (CI용)
Directory.Build.props         모든 모드 공통 라이브러리 버전 ⚠️ 바꾸면 전체 재빌드
.github/workflows/build.yml   자동 빌드·릴리스 ⚠️ 바꾸면 전체 재빌드
```

## 새 모드 만들기
```
python3 tools/new_mod.py FireCards --description "불꽃 카드 모음"
```
이름은 영어 PascalCase. 만든 뒤 커밋·푸시하면 그 모드가 빌드되어 `FireCards-latest` 릴리스가 생깁니다.

## 그림 규칙 (GPT 담당)
원본을 해당 모드의 `art/` 아래 폴더에 PNG로 넣기만 하면, 빌드 때 자동으로 게임 규격 크기로 잘라 붙입니다.

| 경로 (mods/<모드>/ 기준) | 용도 | 권장 원본 | 비고 |
|---|---|---|---|
| `art/mod_image.png` | 모드 목록 아이콘 | 정사각형 1024² | |
| `art/cards/<id>.png` | 카드 일러스트 | **가로형 4:3** (예: 1536×1152) | 가운데 기준으로 1000×760 / 250×190로 잘림. 중요한 요소는 가운데에 |
| `art/relics/<id>.png` | 유물 아이콘 | 정사각형, **투명 배경** | 256² / 94² + 흰 테두리(outline) 자동 생성 |
| `art/powers/<id>.png` | 버프·디버프 아이콘 | 정사각형, **투명 배경** | 256² / 64² |
| `art/potions/<id>.png` | 포션 | **투명 배경** | 최대 256 |
| `art/monsters/<id>.png` | 몬스터(적) 전신 | **투명 배경**, 세로로 넉넉히 (예: 1024×1024) | **왼쪽(플레이어 쪽)을 바라보게**. 화면 높이는 `art/monsters/sizes.json`에서 px로 지정(기본 300, 보스 380~450) |
| `art/other/` | 그 밖의 그림 | 아무거나 | 그대로 복사 |

- **파일 이름 = 코드의 id.** 영어 소문자+밑줄(snake_case) 권장: `fire_strike.png`. (대문자·공백은 자동 변환되지만 헷갈리니 피하기)
- 아이콘류는 반드시 배경 투명(알파) PNG. 텍스트·숫자·카드 테두리는 그리지 말 것(게임이 그려 줌).
- 화풍: 슬더스2 느낌의 손그림 회화풍, 진한 외곽선, 채도 높은 색. 실존 저작물/캐릭터 복제 금지.
- 같은 id로 다시 넣으면 교체됩니다. 커밋 메시지 예: `FireCards art: fire_strike 카드 그림 추가`

## 코드 규칙 (Claude 담당)
- .NET 9 / C#, [BaseLib](https://alchyr.github.io/BaseLib-Wiki/) + Harmony 사용. 게임 API는 얼리 액세스라 자주 바뀜.
- 새 카드/유물 등은 BaseLib 방식으로 `mods/<모드>/Code/Cards/`, `Code/Relics/` … 에 추가.
  이미지 경로는 `art/`의 id와 맞추기: 예) `res://<모드>/images/card_portraits/<id>.png`
- 텍스트는 하드코딩하지 말고 `mods/<모드>/<모드>/localization/<lang>/*.json` 에. 최소 `eng`, 가능하면 `kor`도.
- 각 모드의 Harmony ID·ModId는 모드 이름으로 고유해야 함 (템플릿이 자동으로 맞춰 줌).
- 참고 자료: [모드 템플릿 위키](https://github.com/Alchyr/ModTemplate-StS2/wiki), [BaseLib 위키](https://alchyr.github.io/BaseLib-Wiki/), 슬더스 디스코드 #sts2-modding
- 기능을 바꾸면 그 모드 `<모드>.json`의 `version`을 올리기 (v0.1.0 → v0.1.1).

## 게임 API 찾기 (게임·NuGet 없이)
Releases의 [`dev-api`](../../releases/tag/dev-api)에 `sts2-api.zip`이 있습니다 (게임 참조 스텁 + BaseLib 디컴파일, 시그니처만).
`curl -sSfL https://github.com/Bigvirus0326/sts2-mod/releases/download/dev-api/sts2-api.zip -o api.zip` 로 받아 grep하세요.
`Directory.Build.props`가 바뀌면 자동 갱신됩니다. 구현 예시가 필요하면 오픈소스 모드 [ActsFromThePast](https://github.com/Cany0udance/ActsFromThePast)(막·몬스터·전투)를 참고.

## 빌드가 깨졌을 때
- 게임 업데이트로 API가 바뀐 경우가 많음 → `Directory.Build.props`의 스텁/BaseLib 버전 확인.
- 그림 이름 충돌 → Actions 로그의 "문제:" 줄 확인.
- 컴파일 오류는 실행 결과의 주석(annotation)에 표시됨: `https://api.github.com/repos/Bigvirus0326/sts2-mod/check-runs/<job id>/annotations` (권한 없이 읽힘)
- 특정 모드만 다시 빌드: Actions → Build mods → Run workflow → 모드 이름 입력.
- 로컬 확인(선택): `python3 tools/process_art.py mods/<모드> && dotnet build mods/<모드>/<모드>.csproj -c ExportRelease`
