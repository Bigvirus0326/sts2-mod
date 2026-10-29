# 작업 일지 (AI 인수인계용)

> **모든 AI(Claude·GPT)는 작업 시작 전에 이 파일을 읽고, 작업을 마치면 갱신해서 커밋하세요.**
> 새 채팅/다른 AI가 이 파일만 보고 바로 이어서 할 수 있어야 합니다. 짧게, 최신이 위로.

## 지금 상태 (항상 최신으로 덮어쓰기)
- 마지막 갱신: 2026-10-29 (Claude)
- 빌드: ✅ 두 모드 모두 클라우드 빌드 통과 (`MapleMonsters-latest`, `SpireMod-latest`)
- 실제 게임에서 로딩/플레이 확인: ❓ 아직 안 함 (사용자 PC에서 확인 필요)

| 모드 | 버전 | 상태 | 내용 |
|---|---|---|---|
| MapleMonsters | v0.3.0 | 기믹 추가 | 새 막 "메이플 월드"(1막 대체, 랜덤 등장) + 보스 3종(2페이즈). 메이플 월드의 법칙(체력 증가·넉백·데미지 상한·제한 시간)이 막의 모든 적에게 적용. 일반/엘리트·지도·배경·음악은 기본 1막(Overgrowth) 임시 차용. 그림은 임시(PLACEHOLDER) |
| SpireMod | v0.1.0 | 빈 모드 | 로딩 시 로그만 출력. 이름은 임시 |

### MapleMonsters 구조 메모
- `Code/Monsters/MapleMonster.cs`: 공통 기반 (이미지 한 장 = 몬스터 모습, Hit/BuffSelf/DebuffTargets/Block/HealSelf/Cycle 도우미)
- `Code/Encounters/MapleBossEncounter.cs`: 보스 전투. 새 보스 = `public sealed class XxxBoss : MapleBossEncounter<Xxx>;` 한 줄
- `Code/Acts/MapleWorldAct.cs`: 막 정의. 임시 차용 부분은 `Borrowed`(Overgrowth)
- 보스 지도 아이콘: 임시로 기본 게임 VantomBoss 것 차용 (TODO)
- 난이도 기믹 (사용자의 메이플 캐릭터 모드가 강해서 넣음): 수치는 `Code/MapleDifficulty.cs` 한 곳
  - `Code/Acts/MapleWorldRules.cs`: Hook.BeforeCombatStart Harmony 후처리로 막의 모든 적에게 적용 (일반 +25% / 엘리트 +35%·상한35·제한6턴 / 보스 +15%·상한25·제한8턴)
  - 넉백(사용자가 고른 기믹): KnockbackPower — 막히지 않은 한 방 피해 ≥ 일반12/엘리트10/보스10이면 손패 1장 날아감. 적 턴엔 플레이어에 KnockedBackPower → 다음 턴 뽑는 카드가 날아감(AfterCardDrawn)
  - 파워: DamageCapPower(데미지 상한), TimeLimitPower(제한 시간→매 턴 힘3), UndeadPower(첫 사망 시 40% 부활)
  - 보스 2페이즈: `MapleMonster.Phases(1페이즈[], 2페이즈[])`, 체력 50% 또는 부활 시 전환, 전환 시 디버프 해제
- 텍스트 키 형식: `MAPLEMONSTERS-<CLASS_SNAKE>.name`, `.moves.<MOVE_ID>`, 막은 `MAPLE_WORLD_ACT.title`, 전투는 `<X>_BOSS.title/.loss`

## 다음에 할 일
- [ ] 사용자 PC에서 MapleMonsters 설치 → 메이플 월드가 1막으로 뜨는지, 보스전이 정상인지 확인 (막이 랜덤이라 몇 판 걸릴 수 있음)
- [ ] 실제 플레이로 난이도 체감 확인 → MapleDifficulty.cs 수치 조정
- [ ] GPT로 보스 그림 3장 교체: `mods/MapleMonsters/art/monsters/{mushmom,zombie_mushmom,balrog}.png` (+ 파워 아이콘 `art/powers/{damage_cap,time_limit,undead}_power.png`)
- [ ] 메이플 엘리트·일반 몹 추가 → 빌려 쓰는 Overgrowth 전투 줄이기
- [ ] 전용 보스 지도 아이콘, 지도/전투 배경, (가능하면) 머쉬맘 소환 패턴
- [ ] 테스트 편의: 메이플 월드가 항상 1막으로 나오게 하는 설정(선택)

## 정해진 것 (바꾸려면 사용자와 상의)
- 역할: Claude = 코드·기획, GPT = 그림 (`mods/<모드>/art/` 규칙은 AGENTS.md)
- 한 저장소에 모드 여러 개(`mods/<이름>/`), 바뀐 모드만 빌드, 모드별 `<이름>-latest` 릴리스
- 빌드는 GitHub Actions에서만 (게임 DLL 대신 NuGet 참조 스텁). 게임 파일 커밋 금지
- 사용자는 주로 폰으로 작업 → 한국어로 짧게, 결과 위주로 보고
- MapleMonsters: 메이플 전용 막(Act) 방식, 보스부터 제작. 사용자의 메이플 캐릭터 모드가 일반 캐릭터보다 강하므로 특별 기믹으로 난이도를 올림. 넥슨 IP 팬 모드 → 공개 배포(창작마당) 시 내려갈 수 있음을 사용자에게 안내함

## 기록 (최신이 위)
- 2026-10-29 — MapleMonsters v0.3.0: 넉백 기믹. (기믹 후보 중 사용자는 "단순한데 특이한, 메이플스러운" 것을 원함 → 넉백 선택. 다른 후보: 리젠·몬스터 레벨업·점프 MISS·디스펠)
- 2026-10-26 — MapleMonsters v0.2.0: 난이도 기믹(몬스터 레벨·데미지 상한·제한 시간·보스 2페이즈·언데드 부활)
- 2026-10-26 — MapleMonsters 모드 추가: 메이플 월드 막 + 보스 3종, 몬스터 그림 파이프라인(art/monsters), localization이 .pck에 빠지던 문제 수정, 게임 API 목록(dev-api) 워크플로
- 2026-10-26 — 여러 모드 구조로 전환, 바뀐 모드만 빌드. art/ 그림이 .gitignore에 걸리던 버그 수정
- 2026-10-26 — 저장소 초기 세팅: 빈 모드 SpireMod, 클라우드 빌드(.dll + .pck), 그림 자동 변환, install.ps1
