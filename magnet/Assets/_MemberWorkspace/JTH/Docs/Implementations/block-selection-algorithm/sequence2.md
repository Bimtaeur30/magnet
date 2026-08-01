# Sequence — Phase 2 (block-selection-algorithm)

> **Phase:** [phase2.md](phase2.md) 와 1:1.

## 1 — 2026-08-01 · BoardHealth + BlameTracker + TuningSO

**바뀐 것**

- 생성: `Scripts/Domain/BlockSelection/Health/HealthZone.cs`
- 생성: `Scripts/Domain/BlockSelection/Health/BoardHealthResult.cs`
- 생성: `Scripts/Domain/BlockSelection/Health/BoardHealthCalculator.cs`
- 생성: `Scripts/Domain/BlockSelection/Blame/TurnFeedback.cs`
- 생성: `Scripts/Domain/BlockSelection/Blame/BlameTracker.cs`
- 생성: `Scripts/Data/BlockSelectionTuningSO.cs`
- 생성: `ScriptableObjects/BlockSelection/DefaultBlockSelectionTuning.asset`
- 수정: `Docs/INSPECTOR_TOOLTIPS.md` — TuningSO 필드 22종 표 추가

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Health/HealthZone.cs`
  - 심볼: `HealthZone` — enum `TooEmpty / Sweet / TooDirty` (추가)
    - 설명: BoardHealth 구간. 너무 비움 / 적정 / 너무 참.
    - 이유: 티어 게이트(Trap=TooDirty, ComboBreak=TooEmpty 등)가 소비할 구간 표현 (SPEC §12.3).
    - 영향: Phase 4 티어 셀렉터가 게이트 조건으로 사용.

- 파일: `Scripts/Domain/BlockSelection/Health/BoardHealthResult.cs`
  - 심볼: `BoardHealthResult.FillRate` — get 프로퍼티 (추가)
    - 설명: 점유 칸 / 전체 칸 (0~1).
    - 이유: 구간 매핑의 1차 기준 + 디버그 로그(§20) 노출용.
  - 심볼: `BoardHealthResult.DeadZoneCount` — get 프로퍼티 (추가)
    - 설명: 1~3칸 크기의 고립 빈 구역 개수.
    - 이유: BlameTracker가 턴 전후 비교로 "새 dead zone" blame을 계산 (§13.1).
  - 심볼: `BoardHealthResult.BigPieceSlots` — get 프로퍼티 (추가)
    - 설명: 3×3·1×5(가로/세로)가 들어갈 수 있는 피벗 수 합.
    - 이유: 큰 블록 자리 여유 지표(§12.1) + blame의 bigSlotLost 판정 입력.
  - 심볼: `BoardHealthResult.PlacementFreedom` — get 프로퍼티 (추가)
    - 설명: 테스트 피스 집합의 피스당 평균 합법 배치 수 (회전 포함).
    - 이유: 판의 배치 자유도 지표(§12.1) + blame의 freedomDrop 판정 입력.
  - 심볼: `BoardHealthResult.Score` — get 프로퍼티 (추가)
    - 설명: 지표 4종의 가중 합 (0~1).
    - 이유: Easy/Pressure 게이트가 쓰는 종합 건강 점수 (§12.2).
  - 심볼: `BoardHealthResult.Zone` — get 프로퍼티 (추가)
    - 설명: fillRate·Score로 매핑된 `HealthZone`.
    - 이유: 티어 게이트의 주 입력.
  - 심볼: `BoardHealthResult(fillRate, deadZoneCount, bigPieceSlots, placementFreedom, score, zone)` — 생성자 (추가)
    - 설명: 전 필드를 받아 불변 readonly struct로 고정.
    - 이유: 계산 결과 스냅샷 — 턴 전후 2개를 들고 비교하는 BlameTracker 사용 패턴에 맞춤.

- 파일: `Scripts/Domain/BlockSelection/Health/BoardHealthCalculator.cs`
  - 심볼: `BoardHealthCalculator.MaxDeadZoneSize` — const int 3 (추가)
    - 설명: dead zone으로 치는 빈 영역 크기 상한.
    - 이유: SPEC §12.1 "1~3칸" 고정 정의 — 튜닝 대상 아님.
  - 심볼: `BoardHealthCalculator.EmptySideFillComponentMax` — const float 0.5 (추가)
    - 설명: TooEmpty 쪽 fill 성분의 상한 계수.
    - 이유: SPEC §12.2 공식(`r / 0.12 * 0.5`)의 고정 상수를 이름으로 표기.
  - 심볼: `BoardHealthCalculator.Square3x3 / Line1x5Horizontal / Line1x5Vertical` — static readonly Vector2Int[] (추가)
    - 설명: bigPieceSlots 판정용 고정 모양 3종.
    - 이유: 자명한 모양이라 SO 주입 없이 내부 상수 — placementFreedom의 주입식과 달리 중복 위험 없음.
  - 심볼: `BoardHealthCalculator.Directions` — static readonly Vector2Int[] (추가)
    - 설명: flood-fill 4방향 오프셋.
    - 이유: dead zone 연결성은 4방향(대각 제외) 정의.
  - 심볼: `BoardHealthCalculator.Compute(board, freedomProbePieces, tuning)` — public static (추가)
    - 설명: 지표 4종 계산 → 가중 합 점수 → 구간 매핑, `BoardHealthResult` 반환. 보드는 읽기만.
    - 이유: Phase 4~6 티어 셀렉터·생성기와 BlameTracker가 공유할 단일 진입점.
    - 영향: BlameTracker가 턴 전후 결과를 받아 비교. Phase 6 Orchestrator가 매 리필 시 호출 예정.
  - 심볼: `BoardHealthCalculator.ComputeFillRate(board)` — private static (추가)
    - 설명: 전 칸 순회로 점유 수를 세어 비율 반환.
    - 이유: `BoardGrid`가 점유 수를 캐시하지 않음 — 64칸 순회는 턴당 1회라 충분히 쌈.
  - 심볼: `BoardHealthCalculator.CountDeadZones(board)` — private static (추가)
    - 설명: 미방문 빈 칸마다 flood-fill로 영역 크기를 재고 크기 ≤3이면 카운트.
    - 이유: §12.1 deadZoneCount 정의 구현. 보드 벽도 경계라 크기 ≤3 빈 영역은 정의상 둘러싸임.
  - 심볼: `BoardHealthCalculator.FloodFillEmptyRegion(board, visited, start, queue)` — private static (추가)
    - 설명: BFS로 연결된 빈 칸 수를 반환. queue는 호출자가 재사용.
    - 이유: 영역 크기 측정 분리 — CountDeadZones의 이중 루프와 탐색 로직 분리.
  - 심볼: `BoardHealthCalculator.CountBigPieceSlots(board)` — private static (추가)
    - 설명: 고정 모양 3종의 합법 피벗 수 합. 빈 8×8 = 100 (36+32+32).
    - 이유: §12.1 bigPieceSlots — 1×5는 회전 2방향을 가로/세로 상수 2개로 표현.
  - 심볼: `BoardHealthCalculator.ComputePlacementFreedom(board, probePieces)` — private static (추가)
    - 설명: 피스마다 회전 4종(시그니처 중복 제거) 합법 배치 수를 합산해 피스 수로 평균. 빈 목록이면 0.
    - 이유: §12.1 placementFreedom. 테스트 피스는 파라미터 주입 — 모양 데이터의 소스(PTY `BlockShapeSourceSO`)를 Domain에 중복하지 않기 위해.
  - 심볼: `BoardHealthCalculator.BuildSignature(offsets, builder)` — private static (추가)
    - 설명: offsets를 (x,y) 정렬 후 문자열로 직렬화.
    - 이유: 대칭 모양의 180° 회전이 순서만 다른 동일 집합이라 순서 무관 비교가 필요 — `PlacementSolver`의 시그니처는 순서 보존이라 재사용 불가.
  - 심볼: `BoardHealthCalculator.CountPlacements(board, cellOffsets)` — private static (추가)
    - 설명: 전 피벗을 `PlacementService.CanPlace`로 검사해 합법 배치 수 반환.
    - 이유: `CanPlaceAnywhere`는 bool만 반환 — 개수가 필요해 같은 루프를 카운트 버전으로.
  - 심볼: `BoardHealthCalculator.ComputeScore(fillRate, deadZoneCount, bigPieceSlots, placementFreedom, tuning)` — private static (추가)
    - 설명: 성분 4종을 정규화(clamp01) 후 튜닝 가중치로 합산.
    - 이유: §12.2 종합 점수 공식. 정규화 상한 3종은 튜닝 SO로 노출.
  - 심볼: `BoardHealthCalculator.FillComponent(fillRate, tuning)` — private static (추가)
    - 설명: §12.2 구간별 공식 — TooEmpty 쪽 선형(×0.5), Sweet 1.0, TooDirty 쪽 `FillDirtyFalloff` 폭으로 선형 감소.
    - 이유: fill 성분만 비선형(스윗스팟) 구조라 별도 함수.
  - 심볼: `BoardHealthCalculator.ResolveZone(fillRate, score, tuning)` — private static (추가)
    - 설명: fill 경계 2종 먼저 판정, 이후 score<TooEmptyScoreMax → TooEmpty, score<TooDirtyScoreMax → TooDirty, 나머지 Sweet.
    - 이유: 스펙 §12.3 리터럴 순서(score 조건이 fill 조건과 OR)면 꽉 찬 보드(score 낮음)가 TooEmpty로 판정돼 Trap 게이트(TooDirty)가 안 열림 — fill 우선으로 재배열 (phase2.md 설계 결정).

- 파일: `Scripts/Domain/BlockSelection/Blame/TurnFeedback.cs`
  - 심볼: `TurnFeedback.IsGoodTurn` — get 프로퍼티 (추가)
    - 설명: 3피스 전부 배치 + 이번 턴 delta ≤ 임계값.
    - 이유: §13.4 긍정 피드백 UI 판정 데이터 — UI는 범위 밖, 데이터만 노출.
  - 심볼: `TurnFeedback.LastTurnDelta` — get 프로퍼티 (추가)
    - 설명: 이번 턴에 새로 쌓인 blame 원값 (감쇠 전).
    - 이유: §13.4 GoodTurn 기준값을 이벤트에 실을 수 있게.
  - 심볼: `TurnFeedback.TotalBlame` — get 프로퍼티 (추가)
    - 설명: 감쇠 반영 누적 blame.
    - 이유: 디버그 로그(§20)·티어 게이트 입력의 스냅샷.
  - 심볼: `TurnFeedback(isGoodTurn, lastTurnDelta, totalBlame)` — 생성자 (추가)
    - 설명: 판정 결과를 불변 readonly struct로 고정.
    - 이유: 턴 종료 이벤트 페이로드로 쓰일 값 타입 (Phase 7).

- 파일: `Scripts/Domain/BlockSelection/Blame/BlameTracker.cs`
  - 심볼: `BlameTracker._tuning` — readonly 필드 `BlockSelectionTuningSO` (추가)
    - 설명: blame 단가·감쇠율·GoodTurn 임계값 공급원.
    - 이유: 수치를 코드에 박지 않고 SO 튜닝 — `ScoreSession`이 `ScoreConfigSO`를 받는 기존 관례와 동일.
  - 심볼: `BlameTracker.Total` — 프로퍼티 (추가)
    - 설명: 감쇠 포함 누적 blame.
    - 이유: 티어 게이트(§13.3 임계값 비교)의 입력.
  - 심볼: `BlameTracker.LastTurnDelta` — 프로퍼티 (추가)
    - 설명: 직전 턴에 새로 쌓인 blame 원값.
    - 이유: §13.4 — GoodTurn 판정·UI 데이터로 별도 노출 요구.
  - 심볼: `BlameTracker(tuning)` — 생성자 (추가)
    - 설명: 튜닝 SO를 보관하고 0에서 시작.
    - 이유: 세션 단위 상태 객체 — Phase 7에서 Bootstrap이 생성·보유.
  - 심볼: `BlameTracker.OnTurnEnded(boardBefore, boardAfter, healthBefore, healthAfter, allPiecesPlaced)` — 메서드 (추가)
    - 설명: §13.1 증가 4종(새 dead zone × 단가, 중앙 새 점유 칸 × 단가, bigSlots 감소 시 flat 1회, freedom 감소분 × 계수) 합산 → `LastTurnDelta` 기록 → `Total = Total × decay + delta` → GoodTurn 판정한 `TurnFeedback` 반환.
    - 이유: 턴 종료 훅 하나로 blame 갱신과 UI 판정 데이터 생산을 묶음 (SPEC §5 흐름). bigSlotLost를 슬롯당이 아닌 flat으로 한 근거는 phase2.md 설계 결정.
    - 영향: Phase 7에서 `TurnBootstrap`(또는 `BoardPlacementBootstrap`)이 턴 종료 시 호출 예정.
  - 심볼: `BlameTracker.Reset()` — 메서드 (추가)
    - 설명: `Total`·`LastTurnDelta`를 0으로.
    - 이유: 재시작(Relife 세션 포함) 시 이전 판의 blame이 넘어오면 안 됨.
  - 심볼: `BlameTracker.CountCenterCellsGained(before, after)` — private static (추가)
    - 설명: 중앙 2×2(8×8이면 (3,3)~(4,4))에서 after만 점유한 칸 수.
    - 이유: §13.1 "중앙 영역 점유 증가" — 중앙을 막으면 큰 블록 자리가 죽는 실수 신호.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `BlockSelectionTuningSO` — ScriptableObject, CreateAssetMenu "Magnet/Block Selection Tuning" (추가)
    - 설명: 블록 선택 알고리즘 수치 튜닝 SO. 이하 22개 프로퍼티 전부 `[field: SerializeField]` + 한국어 Tooltip.
    - 이유: SPEC §17 — 코드는 공식만, 수치는 SO에서 튜닝. Tooltip 문구는 `Docs/INSPECTOR_TOOLTIPS.md`에 동기화.
  - 심볼: `TooEmptyFillMax / TooDirtyFillMin / TooEmptyScoreMax / TooDirtyScoreMax` — 프로퍼티 4종 (추가)
    - 설명: 구간 매핑 경계 (기본 0.12 / 0.55 / 0.35 / 0.40).
    - 이유: §12.3 시작값 표 그대로.
  - 심볼: `FillDirtyFalloff` — 프로퍼티 (추가)
    - 설명: TooDirty 쪽 fill 성분이 0까지 떨어지는 fillRate 폭 (기본 0.35 → fill 0.90에서 0).
    - 이유: §12.2 공식의 하드코딩 상수 `/0.35`를 튜닝 필드로 승격.
  - 심볼: `FillWeight / DeadZoneWeight / BigSlotWeight / FreedomWeight` — 프로퍼티 4종 (추가)
    - 설명: healthScore 성분 가중치 (기본 0.4 / 0.2 / 0.2 / 0.2, 합 1).
    - 이유: §12.2 `w_fill·w_dead·w_big·w_free`.
  - 심볼: `DeadZoneNormalizeMax / BigSlotNormalizeMax / FreedomNormalizeMax` — 프로퍼티 3종 (추가)
    - 설명: 지표 → 0~1 정규화 상한 (기본 6 / 100 / 100. 100은 빈 8×8 보드 기준값).
    - 이유: §12.2의 `normalize()`가 상한 미정의라 튜닝 필드로 구체화.
  - 심볼: `BlamePerDeadZone / BlamePerCenterCell / BlamePerBigSlotLost / BlamePerFreedomDrop` — 프로퍼티 4종 (추가)
    - 설명: §13.1 이벤트별 blame 단가 (기본 20 / 4 / 10 / 0.5).
    - 이유: 권장 범위(15~25, 3~5, 8~12)의 중앙값으로 시작.
  - 심볼: `BlameDecayRate` — 프로퍼티 (추가)
    - 설명: 매 턴 누적 blame에 곱하는 감쇠율 (기본 0.7).
    - 이유: §13.2 권장 0.65~0.75.
  - 심볼: `BlameComboBreakThreshold / BlamePressureThreshold / BlameTrapThreshold / EasyBlameMax / GoodTurnBlameDeltaMax` — 프로퍼티 5종 (추가)
    - 설명: §13.3 용도별 임계값 (기본 25 / 35 / 55 / 15 / 5).
    - 이유: Blame 스펙의 일부라 지금 정의 — GoodTurn만 이번 Phase가 소비, 나머지는 Phase 4~6 티어 게이트가 소비 예정.

**검증**

- `read_console` 컴파일 에러 0 (Unity 자동 리로드 후 확인).
- `execute_code` 시나리오 6종 (임시 스크립트 파일 없이, 프로브 피스 = 1x3·2x2·L3):
  1. 빈 보드 → fill=0, dead=0, big=100(36+32+32), zone=TooEmpty ✅
  2. 행 0~1 채움(25%) → fill=0.25, big=64, score=0.89, zone=Sweet ✅
  3. 열 0~4 채움(62.5%) → zone=TooDirty ✅
  4. 꽉 찬 보드 - 구멍(1칸 + 3칸 + 4칸) → dead=2 (4칸 제외), score=0.166이어도 zone=TooDirty (ResolveZone 재배열 검증) ✅
  5. 빈 보드 → dead zone 1개 + 중앙 2×2 점유 턴 → delta=60 (dead 20 + center 16 + bigSlot 10 + freedom 14), good=False ✅
  6. 무변화 턴 → delta=0, Total=42 (=60×0.7 감쇠), good=True ✅
- `DefaultBlockSelectionTuning.asset` 생성 확인 (`ScriptableObjects/BlockSelection/`).

**메모**

- `Compute`에 넘길 실전 프로브 피스 집합(1x1 제외 16종)은 Phase 7 Bootstrap이 `BlockShapeSourceSO`에서 구성한다 — Domain은 목록의 출처를 모름.
- 티어 확률(`p_trap`, `p_comboBreak`, `p_hospitality`, `p_pressure`)·sampleCount·relifeTurnCount는 TuningSO에 아직 없음 — 소비자가 생기는 Phase 4~6에서 추가.
