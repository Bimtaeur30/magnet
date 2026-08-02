# MAGNET SQUARE — Design Document

> Version 0.7 | 2026-07-21  
> 슈퍼센트 활동 프로젝트 | 3주 프로토타입 기준 문서

---

## 1. 문서 목적

- 하이퍼 캐주얼 블록 퍼즐 **MAGNET SQUARE**의 핵심 규칙·시스템·구현 방향을 정의한다.
- **v0.7:** 인게임 코어를 **Block Blast!** 와 동일한 규칙으로 피벗. (자석 흡착·정사각형 테두리 클리어·보드 회전 **폐기**)
- 팀원별 작업 범위와 **게임 전체 마일스톤**의 기준 문서로 사용한다.
- 세부 수치 밸런스·아트 스펙·광고/부스터 등 출시 이후 메타는 범위 밖이며, 필요 시 후속 문서에서 다룬다.

### 1.1 문서 위치 (AI·팀 공통)

| 종류 | 경로 | 용도 |
|------|------|------|
| 팀 설계·마일스톤 표 | `Docs/DESIGN.md` | 게임 전체 기능 영역·완료 기준 (M0~M10) |
| 팀 TODO | `Docs/TODO.md` | 미정 사항 (멤버별 `## [이름]` 섹션) |
| AI 협업 가이드 | `Docs/AI_COLLAB_GUIDE.md` | 프롬프트·워크플로 |
| AI 행동 규칙 | `CLAUDE.md` (루트) | Cursor 자동 로드 |
| **개인 구현 인덱스** | `Assets/_MemberWorkspace/[이름]/Docs/IMPLEMENTATIONS.md` | 담당 구현·Jira·상태 |
| **구현별 Phase 인덱스** | `.../Implementations/[slug]/phases.md` | 그 구현의 Phase 목록 |
| **Phase 계획** | `.../Implementations/[slug]/phaseN.md` | 그 Phase에서 **뭘 어떻게** 구현하는지 상세 |
| **Sequence 변경 기록** | `.../Implementations/[slug]/sequenceN.md` | **Phase와 1:1** — 뭐가 바뀌었는지 순서대로 |

새 AI 세션: `@Docs/DESIGN.md` + `@IMPLEMENTATIONS.md` + **진행 중 구현**의 `phases.md` + 해당 **`phaseN.md`·`sequenceN.md`** 만 읽는다 (전체 히스토리 X).

> **용어:** `DESIGN.md`의 **마일스톤(M0~M10)** = 게임 전체 로드맵. 개인 **Phase** = 특정 구현을 쪼갠 단계 (뭘 어떻게 구현하는지 상세). **Sequence** = 그 Phase에서 뭐가 바뀌었는지 순서대로 적는 기록 — Phase 파일과 **1:1**, 항목은 한 파일 안에 쌓음.

### 1.2 v0.6 → v0.7 폐기 규칙 (구현 deprecated)

| v0.6 규칙 | v0.7 처리 |
|-----------|-----------|
| 자석 축 `(0,0)`·Y축 흡착 | **폐기** — `MagnetSnapSimulator` 등 deprecated |
| 정사각형 테두리 클리어 + 달팽이 재조립 | **폐기** — `SquareClearDetector`, `ClearReassemblyService` deprecated |
| 배치마다 보드 90° 회전 | **폐기** — `BoardRotationService` deprecated |
| 4슬롯 후보 | **3슬롯** (Block Blast 기본) |
| PTY `BlockShapeSourceSO` 풀 | 인게임은 **Block Blast 표준 풀 SO** (`BlockBlastPoolSO`) |

---

## 2. 게임 개요

| 항목 | 내용 |
|------|------|
| 가제 | **MAGNET SQUARE** (프로젝트 코드명) |
| 장르 | 하이퍼 / 하이브리드 캐주얼 블록 퍼즐 |
| 한 줄 소개 | Block Blast!와 동일 — **8×8** 보드에 polyomino를 드래그해 놓고, **가로·세로 한 줄**을 채우면 터지는 블록 퍼즐 |
| 레퍼런스 | **Block Blast!** (인게임 코어 100%), 무한의 계단 (점수 기반 스킨 해금) |
| 플랫폼 | 모바일(iOS/Android) 우선, **세로 화면·단일 핸드** |
| 시점 | **탑다운 2D** (8×8 격자 보드 정면) |
| 세션 길이 | 1~3분 |
| 팀 | 개발자 4명 (기획·아트 별도 인력 없음) |
| 프로토타입 목표 | **거의 전체 기능** — 코어 루프 + 점수 + 스킨 해금/인벤토리/장착/저장까지 |

### 2.1 핵심 규칙 (Block Blast 동일)

1. **8×8 고정 격자** — 자석 축·특수 칸 없음. 보드는 회전하지 않음.
2. **3개 후보** — 하단 슬롯에 polyomino 3개. 하나 선택 → 보드 위 원하는 위치에 드래그·놓기.
3. **Line clear** — 가로 또는 세로 **한 줄(8칸)** 이 전부 채워지면 해당 줄 삭제. 한 배치 후 **연쇄** 검사.
4. **칸 단위 보드** — Domain·View 모두 **칸(`OccupiedCell`)** 이 진실. Shape(형태)는 드래그·조립용 논리/View일 뿐.
5. **게임오버** — 3개 후보가 **어떤 위치·회전(스폰 시 고정)으로도** 놓을 수 없을 때.

### 2.2 심사·방향성 메모

- 블록 퍼즐 장르 CPI 특성을 고려해, **접근성 높은 조작·즉각 재도전**을 우선한다.
- **스킨 해금**은 “다음 스킨까지 한 판 더” 리텐션 장치로 운용한다 (게임플레이 영향 없음).

---

## 3. 핵심 게임플레이 루프

**블록 1개 배치**의 고정 순서:

```
블록 선택 → 보드 위 2D 드래그·그리드 스냅·부착 (이후 칸 단위)
  → Line clear 판정 (행·열) → 삭제 → 재검사 (연쇄)
  → 점수·콤보 (§4.7)
  → 슬롯 소모 (해당 후보 null). 3슬롯 전부 소진 시 턴 종료·리필
  → 3개 모두 배치 불가 → 게임오버
```

| 단계 | 설명 |
|------|------|
| 1. 블록 후보 | 화면 하단 **3슬롯**에 Block Blast 표준 풀에서 뽑은 polyomino. 하나 선택 |
| 2. 2D 배치 | 터치·드래그로 보드 위 이동. 그리드에 스냅. **고스트 프리뷰**로 배치 가능/불가 표시. 손을 떼면 유효 위치에만 부착 |
| 3. 부착 | overlap·bounds 검증 통과 시 **칸 단위**로 보드에 등록. 플레이어는 형태 회전 불가 (스폰 시 회전만 적용) |
| 4. Line clear | 부착 직후, 꽉 찬 **행·열** 탐색 → 해당 칸 전부 삭제 → 다시 탐색 (연쇄). 연출 중 입력 잠금 |
| 5. 점수 | 클리어 없음: 배치 칸 수. 클리어 있음: 지워진 줄 수·콤보 (§4.7) |
| 6. 후보·턴 | 슬롯 1개 소모. **3개 전부 소진** 시 턴 종료 → 3개 리필. 리필 후 **3개 모두** 놓을 수 없으면 게임오버 |

> **턴(Turn):** 핸드(3후보) 전부 소진·리필 단위. `TurnIndex`·`TurnStarted`/`TurnEnded` 이벤트. `TurnEnded` 시 그 턴에 클리어가 없으면 콤보 리셋.

---

## 4. 핵심 시스템

### 4.1 보드

- **8×8 정사각 격자** (프로토타입 고정. `BoardConfigSO`로 N 변경 가능하나 기본값 `8`).
- **좌표:** `(0,0)` = 보드 **좌하단** (또는 좌상단 — 구현 시 `BoardCoordinates` 하나로 통일). 자석 축 없음.
- **점유:** `BoardGrid` — `Dictionary<Vector2Int, bool>`. 부착 후 **칸 단위** (`OccupiedCell`).

### 4.2 블록 공급

- 항상 **3개 후보** 동시 제시.
- **형태 풀:** Block Blast 표준 세트만 (`BlockBlastPoolSO`). PTY `BlockShapeSourceSO` 풀은 인게임에서 **사용하지 않음**.
- 스폰 시 형태별 **0/90/180/270°** 중 하나 적용 (플레이어 회전 없음).
- 1개 사용 시 해당 슬롯만 비움. **3개 전부 소진** 시 `Fill` (턴 종료·시작).
- **추첨:** Block Blast 풀에서 추첨 (현재 Phase). **보드 상태 기반 스폰 알고리즘·난이도 곡선은 별도 설계(TBD)** — 본 문서 범위 밖.
- **게임오버:** 3개 모두 어떤 위치에도 둘 수 없을 때.

| ID (canonical) | 형태 | 칸 수 |
|----------------|------|-------|
| `1x1` | ■ | 1 |
| `1x2` | ■■ | 2 |
| `1x3` | ■■■ | 3 |
| `2x2` | 2×2 | 4 |
| `L3` | L 3칸 | 3 |
| `L4` | L 4칸 | 4 |
| `T4` | T 4칸 | 4 |
| `Z4` | Z 4칸 | 4 |

### 4.3 배치

- 플레이어는 **2D 드래그**로 보드 위 격자 위치 결정.
- **검증:** pivot + `CellOffsets` 기준 overlap 없음, bounds 내.
- 부착 완료 시 Domain·View **칸 단위** 분해. 이후 위치 변경은 **line clear 삭제**로만.
- **프리뷰(고스트):** 스냅 위치·배치 불가 표시.

### 4.4 Line clear

- **발동:** 한 **행** 또는 **열**의 8칸이 전부 점유.
- **처리:** 해당 줄의 모든 칸 **삭제** (재조립·달팽이 없음). Domain 연쇄 until no full line.
- **Presentation:** 줄 삭제 FX (파티클 등). **칸 단위**로 풀 반환 — Shape 통째 삭제 아님.
- **판정 시점:** 부착 직후 및 각 연쇄 웨이브 후.

### 4.5 Presentation — Block 풀링·조립

- **런타임 단위:** 칸 1개 `Block` (SpriteRenderer).
- **BlockPool:** `Block` 프리팹 1종 풀링 (`GameLib.ObjectPool`).
- **ShapeAssembler:** 드래그·고스트·슬롯 표시 시 풀에서 Block rent → `IBlockShape.CellOffsets`에 맞게 **조립**.
- **부착:** 조립된 Block을 격자 칸(`OccupiedCellView`)으로 **분리** — 이후 line clear는 해당 칸 Block만 pool return.
- `ShapeBlock`(기존)은 조립 역할로 **리팩터** 또는 `ShapeAssembler`로 대체.

### 4.6 게임오버 조건

1. **3개 후보** × 모든 pivot × (스폰 시 고정된 회전) brute-force — **하나도** 배치 불가.

> v0.6의 “경계 이탈 게임오버”“x축만 배치 불가” 규칙은 **폐기**.

### 4.7 스테이지 · 공격력

- **스테이지:** 적 처치 시 `EnemyManager`가 증가. HUD·GameOver·베스트 저장의 진행도는 Stage.
- **공격력:** 배치/라인클리어 공식(`ScoreSession` / `ScoreConfigSO`) 결과는 `EnemyAttackRequestEvent.Damage`로만 사용. UI·저장 진행도와 분리.
- **콤보(UI):** 체인 첫 클리어는 콤보 0. **그다음** 클리어부터 콤보 1. `ComboChangedEvent`.
- 게임오버 `GameOverEvent.FinalStage`. 베스트는 `ISaveService.SubmitStage` / `BestStage`.

### 4.8 스킨 시스템 (코스메틱)

| 기능 | 설명 |
|------|------|
| 해금 | 스킨 `unlockType`/`unlockValue` 조건 충족 시 해금 (`Stage` 등) |
| 인벤토리 | 보유·잠금 상태 목록 UI |
| 장착 | 선택 스킨을 **Block** 칸 비주얼에 적용 |
| 저장 | 보유 목록 + 현재 장착 스킨 **영구 저장** |
| 영향 | 게임플레이 수치에 **영향 없음** |

### 4.9 추후 확장 (프로토타입 범위 밖)

- 보드 상태 기반 스폰 알고리즘·난이도 곡선 (Block Blast 내부 로직 — **별도 설계**)
- 컬러 보너스, 일일 시드, 광고 부스터·코인 등

---

## 5. UX · 화면 구성

### 5.1 인게임 레이아웃 (세로)

```
┌─────────────────────┐
│  Stage / Best       │  상단
├─────────────────────┤
│                     │
│    8×8 Board        │  중앙 — 고정 격자
│                     │
├─────────────────────┤
│  [B][B][B]          │  하단 — 3후보 + 드래그
└─────────────────────┘
```

### 5.2 추가 화면

- **메인 메뉴** — 플레이 시작, 인벤토리 진입
- **인벤토리** — 스킨 목록, 장착, 잠금/해금 표시
- **게임오버** — 최종 점수, 베스트, 재시작
- **스킨 해금 알림** — 해금 시 토스트/팝업

### 5.3 피드백·조작

- 드래그·그리드 스냅, 고스트 프리뷰, line clear FX·SFX (LitMotion).
- 터치 드래그 → 릴리즈 시 부착. 한 손 조작.
- 배치 불가 임박 시 경고 연출.

---

## 6. 기술 아키텍처 (프로젝트 규칙)

### 6.1 스택

| 영역 | 선택 |
|------|------|
| Unity | 6000.3.x, URP 2D |
| DI | **Reflect** |
| Async | **UniTask** (신규 Coroutine 금지) |
| Tween | **LitMotion** (DOTween 금지) |
| 이벤트 | **EventChannelSO** + `GameEvent` 파생 클래스 |
| 데이터 | **ScriptableObject** (보드, BlockBlastPool, 점수 테이블) |
| 풀링 | **GameLib ObjectPool** — `Block` 칸 1개 |

### 6.2 책임 분리 (SOLID)

| 레이어 | 책임 |
|--------|------|
| Data | `BoardConfigSO`, `BlockBlastPoolSO`, `SkinSO`, `ScoreConfigSO` |
| Domain / Rules | 격자, grid placement 검증, line clear, 게임오버 — **순수 로직 우선** |
| Application | 턴 FSM, 점수·스킨·세이브 오케스트레이션 |
| Presentation | BlockPool, ShapeAssembler, 보드·칸 View, FX, UI |
| Input | 2D 드래그 → 배치 요청 |

- MonoBehaviour는 표현·입력·DI 바인딩. 규칙 판정은 테스트 가능한 클래스로 분리.
- 오브젝트 간 통신은 `EventChannelSO`.

### 6.2.1 Reflex vs SerializeField (확정)

| 대상 | 연결 |
|------|------|
| ScriptableObject·**프로젝트 에셋** | Inspector `[SerializeField]` |
| **씬 GO·MonoBehaviour**·런타임 서비스 | Reflex `[Inject]` |

### 6.2.2 보드 좌표 (v0.7)

- **격자:** `gx, gy ∈ [0 .. N-1]`, 기본 `N = 8`. **자석 축 없음.**
- **변환:** `BoardCoordinates.GridToWorld` / `WorldToGrid` + `IsInBounds`.
- **점유:** `BoardGrid` — `Dictionary<Vector2Int, bool>`.

> v0.6 `(0,0)=자석, [-4..4]` 좌표계는 **마이그레이션 대상**.

### 6.3 폴더·소유권

- 개인 코드: `Assets/_MemberWorkspace/[username]/`
- **타 멤버 Workspace 수정 금지**
- 공용 문서: `Docs/DESIGN.md`, `Docs/TODO.md` 등

### 6.4 주요 이벤트 (v0.7)

| 이벤트 | 발생 시점 |
|--------|-----------|
| `BlockPlacedEvent` | 블록 부착 완료 |
| `StageClearEvent` | 스테이지 진행 (적 처치/스폰) |
| `ComboChangedEvent` | 콤보 갱신 |
| `GameOverEvent` | 게임 종료 (`FinalStage`) |
| `BestStageUpdatedEvent` | 베스트 스테이지 갱신 |
| `TurnStarted` / `TurnEnded` | 핸드 리필 |

> **Deprecated:** `SquareClearedEvent`, `BoardRotatedEvent`, `BoundaryViolationEvent` (v0.6)

---

## 7. 팀 역할 분배

| 역할 | 담당 범위 | 멤버 | 담당 마일스톤 |
|------|-----------|------|---------------|
| **코어 게임플레이 (리드)** | 격자, 2D 배치, line clear, 게임오버, 턴 FSM, BlockPool, **점수 로직** | **JTH** | M0(공동), **M1–M6**, SCRUM-23 |
| **시스템** | 블록 풀 SO, Save/Load | PTY 등 | M2, **M7–M8** |
| **UI / 인벤토리** | HUD, 인벤토리, 장착 UI | KTJ 등 | **M7**, **M9** |
| **인게임 클라이언트** | 입력, 스킨, 이펙트, QA | — | M3 협업, **M10** |

### 7.1 Jira (SCRUM) 연동

| Jira | 제목 (v0.7) | 마일스톤 | 구현 범위 |
|------|-------------|----------|-----------|
| SCRUM-17 | 인게임-블록 좌표·8×8 격자 | M1 | 8×8, 자석 제거, 좌표 단순화 |
| SCRUM-18 | 인게임-블록 공급 | M2 | `BlockBlastPoolSO`, 3슬롯, 턴 리필, 풀 추첨 (**스폰 알고리즘 TBD**) |
| SCRUM-19 | 인게임-2D 배치·BlockPool | M3 | 2D 드래그, grid placement, ShapeAssembler |
| SCRUM-20 | 인게임-Line clear | M5 | 행·열 fill → 칸 삭제, 연쇄 |
| SCRUM-21 | ~~보드 회전~~ | — | **Cancelled** — 턴 FSM은 SCRUM-18에 흡수 |
| SCRUM-22 | 인게임-게임 오버 | M4 | 3후보 전부 배치 불가 |
| SCRUM-23 | 인게임-점수 관리 | M7 | line clear 점수·콤보 (**HUD 제외**) |

---

## 8. 게임 마일스톤 (M0~M10)

| 마일스톤 | 목표 | Owner | 완료 기준 | 상태 |
|----------|------|-------|-----------|------|
| **M0** | 공통 기반 | 공동 | Reflex, EventChannelSO, asmdef | ✅ |
| **M1** | 8×8 보드 | JTH | 격자 렌더, BoardConfigSO=8, 자석 없음 | ⬜ |
| **M2** | 블록 풀·공급 | JTH | BlockBlastPoolSO, 3슬롯·턴 리필 | ⬜ |
| **M3** | 2D 배치 | JTH | 드래그, ghost, grid placement, BlockPool | ⬜ |
| **M4** | 게임오버 | JTH | 3후보 배치 불가 판정 | ⬜ |
| **M5** | Line clear | JTH | 행·열 삭제, 연쇄, FX 이벤트 | ⬜ |
| **M6** | — | — | *(v0.6 회전 마일스톤 폐기 — M5 이후 M7로)* | — |
| **M7** | 점수·베스트 | 시스템+UI | HUD, 베스트, GO UI | ⬜ |
| **M8** | 스킨·저장 | PTY | 해금, Save | ⬜ |
| **M9** | 인벤토리 UI | KTJ | 목록, 장착 | ⬜ |
| **M10** | 폴리시 | — | 스킨 Block 반영, line clear FX | ⬜ |

---

## 9. 리스크 · 오픈 이슈

| 리스크 | 대응 |
|--------|------|
| v0.6 코드 대량 deprecated | Phase 단위 교체, Bootstrap 순서부터 |
| 좌표계 마이그레이션 | SCRUM-17 Phase 3에서 일괄 |
| 스폰 알고리즘 미설계 | 우선 균등 추첨으로 코어 검증, 알고리즘은 별도 Phase |
| 4인 병렬 통합 | M0 이벤트·SO 합의 유지 |

### 확정 (v0.7)

- [x] 인게임 코어 = Block Blast (8×8, 3슬롯, line clear, 2D 배치)
- [x] Block Blast 표준 형태 풀 SO
- [x] Block 1종 풀링 + ShapeAssembler
- [x] 보드 8×8
- [x] JTH ↔ 코어 게임플레이 (M1–M5)

### TBD

- [ ] 보드 좌표 원점 (좌하 vs 좌상) — Phase 3에서 확정
- [ ] 스킨 해금 간격 N점
- [ ] **보드 상태 기반 스폰 알고리즘** (Block Blast 내부 로직 — 별도 grill-me)
- [ ] KTJ / PMS / PTY 역할 매핑

---

## 10. 변경 이력

| 버전 | 날짜 | 내용 |
|------|------|------|
| 0.1 | 2026-07-06 | 최초 작성 |
| 0.6 | 2026-07-08 | 자석 흡착·테두리 클리어·회전 규칙 |
| **0.7** | **2026-07-21** | **Block Blast 피벗** — 자석/테두리클리어/회전 폐기, 8×8·3슬롯·line clear·BlockPool. 스폰 알고리즘은 TBD |
