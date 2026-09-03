# TODO



각자 자기 섹션만 수정하세요. 팀 공통 작업은 `Common`에 적습니다.



## Common



- [ ] `Docs/DESIGN.md` v0.7 Block Blast 피벗 — Jira(SCRUM-17~23) 설명 동기화 (JTH)



## JTH



**역할:** 인게임 코어 로직 (Jira SCRUM 기준) · **UI/HUD/인벤토리 담당 아님**  

**Jira:** [SCRUM Backlog](https://bimtaeur30.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog) · `hwanji203@gmail.com`  

**기획:** v0.7 Block Blast 피벗 (2026-07-21 grill-me). 스폰 알고리즘은 **`hybrid-spawn-algorithm`** (핸드오프 체인 + 특수 티어 병합, 2026-08-02 grill-me).



- [x] (공통) M0 — Reflect DI, EventChannelSO, asmdef/공유 폴더 합의 → `common-bootstrap`

- [ ] SCRUM-17 — 8×8 격자, 자석 축 제거, 좌표 단순화 (`block-coordinates` Phase 3)

- [ ] SCRUM-18 — BlockBlastPoolSO, 3슬롯·턴 리필 (`random-block-spawn` Phase 6). *스폰 알고리즘 TBD*

- [ ] SCRUM-19 — 2D 드래그·grid placement·BlockPool/ShapeAssembler (`block-placement` Phase 6)

- [ ] SCRUM-20 — Line clear 행·열 삭제·연쇄 (`line-clear` Phase 1). *clear-reassembly deprecated*

- [ ] SCRUM-22 — 3후보 전부 배치 불가 게임오버 (`game-over` Phase 1)

- [x] Relife 오퍼·수락 이벤트 (`relife` Phase 1). UI는 범위 밖

- [x] SCRUM-21 — ~~보드 90° 회전~~ → **Cancelled** (턴 FSM은 SCRUM-18)

- [ ] SCRUM-23 — line clear 점수·콤보 공식 (`score-logic` Phase 5)

- [x] `blocked-ring-dim` — **Deprecated** (v0.6 테두리 UX)

**QA 피드백 정리 (2026-09-02, 커서·Cowork 분업용 — 파일 겹침 없게 트랙 3개로 쪼갬. 트랙 안은 순서대로, 트랙끼리는 병렬 가능):**

- **Track 1 — 드래그/배치 입력** (`Input/BlockDragInput.cs`) — **완료 (Cowork)**
  - [x] `qa-tap-instant-place` — 원인: `OnBlockSelected`가 선택 즉시 `UpdateViews()`를 호출해 스테이징 시작 위치가 바로 유효 프리뷰로 잡힘. 수정: `_hasMoved` 플래그 추가, 실제 드래그(임계 이동량 이상) 없이 released되면 배치하지 않고 슬롯으로 되돌림
  - [x] `qa-block-falls-offscreen` — 원인: `DragSensitivityRamp`가 원점에서 멀어질수록 배율이 커져 드래그가 보드 밖으로 클램프 없이 튕겨나감. 수정: `BlockDragInput.ClampPivot`으로 보드+여유 2칸 범위로 피벗 이동 제한
  - Unity 에디터에서 실제 터치/드래그로 확인 필요 (탭만 했을 때 안 놓이는지, 빠르게 내려긋기 했을 때 화면 안에 남아있는지)

- **Track 2 — 점수/세이브 → 커서로 이관 (가장 까다로운 트랙)**
  - [ ] `qa-score-keeps-climbing` — `TurnBootstrap`이 `ScoreSession`을 `Awake()`에서 한 번만 만들고 어디서도 `Reset()`을 호출하지 않음. **확인 필요:** 재시작이 씬 전체 리로드인지(그럼 Awake가 다시 돌아 문제 없음), 아니면 씬 유지한 채 재시작인지(그럼 점수가 이전 판에서 누적됨 — 진짜 버그). 씬 리로드가 아니면 `GameOverEvent`/재시작 시점에 `_scoreSession.Reset()` 호출 추가
  - [ ] `qa-best-score-missing` — PTY 소유 파일(`_MemberWorkspace/PTY/Scripts/Save/BestScoreDisplay.cs`, `SaveBridge.cs`). `SaveBridge.OnGameOver`는 `SubmitScore`를 정상 호출하는데, `BestScoreDisplay`는 `BestStage`만 표시하고 있어서 **베스트 점수를 보여주는 UI 자체가 없음**. PTY 워크스페이스라 JTH가 직접 수정하지 않음 — PTY와 상의하거나 직접 작업 필요
  - JTH 소유 파일 밖(PTY workspace)이라 Cowork에서 진행 안 함

- **Track 3 — 피드백 트리거 신규 프레젠터** — **코드 완료 (Cowork), 씬 배선 필요**
  - [x] `qa-combo-sfx-tier` / `qa-big-clear-sfx` / `qa-haptics` — `Data/GameFeedbackConfigSO.cs` + `Bootstrap/GameFeedbackBootstrap.cs` 신규 추가. 기존 파일 수정 없음
  - **남은 작업(유니티 에디터, 코드 아님):**
    1. `Create > Magnet > Game Feedback Config`로 `GameFeedbackConfigSO` 에셋 생성 (`_Shared/ScriptableObjects/` 등)
    2. 콤보 티어 목록(콤보 수·사운드), 대량 클리어 줄 수 임계값·사운드는 사운드 담당이 채움 (코드 수정 불필요, 비워두면 그냥 스킵됨)
    3. 인게임 씬에 `GameFeedbackBootstrap` 컴포넌트 추가하고 `inGameChannel`/`magnetGameChannel`/`soundChannel`/`config` 4개 Inspector 참조 연결
  - 진동은 `Handheld.Vibrate()` 단발이라 세기 구분은 안 됨(강도 차등 필요하면 별도 네이티브 진동 플러그인 도입 — 범위 밖)

- **Track 4 — 시작 보드 프리필 재도입** (신규 구현)
  - [x] `board-start-prefill` — "처음에 비워져 있음" 대응. **칸-확률 채움(FillProbability 0.6) + Normal 번들 모양 구멍** 방식으로 재도입(2026-09-03). 분석 문서의 "피스 단위 배치·피스 단위 색" 결론은 폐기 — 튜닝 단순·예측 가능 우선(`START_BOARD_PREFILL_ANALYSIS.md` 갱신). `Implementations/board-start-prefill/` phase1 완료. 같은 커밋에 퍼펙트/올클리어 이벤트(`HandOptimalSolver`, `PerfectClearEvent`/`AllClearEvent`, 점유율 0.4 게이트) 동반. **남은 것: Unity 에디터 컴파일·플레이 확인**

**QA 피드백 중 JTH 범위 밖 (담당자 전달 필요 — UI/스킨/사운드디자인):**

- 메인 화면 버튼 3개 중 1개만 작동 / 메인 화면을 로딩 화면으로 대체 (UI)
- 화면 전환 Fade 너무 길고 게임 톤과 안 맞음 (UI)
- 하단 슬롯 블록 UI 크기가 작고 실제 배치와 안 맞음 (UI)
- 젤리 터짐 이펙트 퀄리티 (스킨 FX)
- 점수 변경 시 이펙트 부족 / 올클리어·퍼펙트 이펙트 부족 (HUD 연출, UI)
- 스킨 선택 화면 흰 배경 거슬림 / 선택된 스킨이 슬라이더에 표시 안 됨 (스킨 UI)
- 용암 이펙트 퀄리티 (스킨 테마 이펙트)
- 슬롯에 블록 UI 등장 시 커지는 이펙트 필요 (UI 연출)
- BGM이 보스전 음악 같음 (사운드 디자인)



## KTJ



## PMS



## PTY



