# sequence40 — Phase 40 변경 기록

> Phase 계획: [phase40.md](phase40.md)

## 1 — 2026-08-13 · 라인필 히트맵 스폰 배선

**바뀐 것** — Unique dirty를 점유 칸≥40으로 바꾸고, Normal/Easy는 라인필 히트맵 최고 점수로 고른다. 접대·올클·Clean·Area·Death 게이트를 선택 경로에서 제거한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/LineFillHeatmap.cs`
  - 심볼: `LineFillHeatmap.Build` — 메서드 (추가)
    - 설명: 보드 행·열마다 `(n−empty)`를 찬 칸에 맞닿은 빈 칸에만 더해 `int[,]` 히트맵을 만든다. 완전 빈/찬 줄은 0.
    - 이유: Normal 선택 점수 — Area MaxArea 대신 라인 채움 근접도를 씀.
  - 심볼: `LineFillHeatmap.SumCells` — 메서드 (추가)
    - 설명: pivot+offsets 칸의 히트맵 합을 반환한다.
    - 이유: 배치 한 수의 점수 계산.
  - 심볼: `LineFillHeatmap.ApplyLine` — 메서드 (추가)
    - 설명: 한 행 또는 한 열에 대해 인접 빈칸에만 점수를 가산한다.
    - 이유: Build의 행/열 공통 루프.
- 파일: `Scripts/Domain/AreaBundleSpawn/HeatmapHandScorer.cs`
  - 심볼: `HeatmapHandScorer.ScoreBest` — 메서드 (추가)
    - 설명: 3! 순서를 돌며 매 수 직전 히트맵을 재계산하고, 각 피스는 최고 이득 자리에 둔다. 못 두면 0·보드 불변. 최고 합과 Explain 경로를 반환.
    - 이유: 완주·콤보 없이 손 전체를 히트맵으로 랭킹.
  - 심볼: `HeatmapHandScorer.ScoreOrder` — 메서드 (추가)
    - 설명: 고정 순서에서 PlaceAndClear를 누적하며 점수를 합산한다.
    - 이유: 순열별 시뮬.
  - 심볼: `HeatmapHandScorer.TryBestPlacement` — 메서드 (추가)
    - 설명: 두기 가능한 모든 pivot 중 히트맵 합이 최대인 자리를 고른다.
    - 이유: 한 피스 greedy 최적 자리.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator.Select` — 메서드 (수정)
    - 설명: Relife Easy → dirty(`occ≥UniqueMinOccupied`)+pUnique Unique → 실패/스킵 시 Normal 히트맵 → Easy. 접대·올클·Clean체인·Death 없음.
    - 이유: grill 확정 cascade 단순화.
  - 심볼: `AreaBundleOrchestrator.TrySelectUniqueDynamic` — 메서드 (수정)
    - 설명: UniqueUnlockGenerator만 호출해 Unique 결과를 만든다(Area·Death 미사용).
    - 이유: Unique 경로 유지·주변 게이트 제거.
  - 심볼: `AreaBundleOrchestrator.SelectNormalOrEasy` — 메서드 (추가)
    - 설명: Normal 리스트 히트맵 최고를 고르고, 후보 없으면 Easy로 넘긴다.
    - 이유: Clean/Main 통합 Normal 단일 풀.
  - 심볼: `AreaBundleOrchestrator.SelectEasy` — 메서드 (수정)
    - 설명: Easy도 히트맵 최고. 후보 없으면 가중랜덤(isKillHand).
    - 이유: Easy 폴백을 같은 점수 체계로.
  - 심볼: `AreaBundleOrchestrator.TrySelectByHeatmap` — 메서드 (추가)
    - 설명: `maxCandidatesToScore` 샘플을 셔플 후 `HeatmapHandScorer.ScoreBest` 최대 번들을 고른다. predictedAreaScore 슬롯에 heat 합을 넣는다.
    - 이유: Normal/Easy 공통 선택.
  - 심볼: `AreaBundleOrchestrator.SampleCandidates` — 메서드 (수정)
    - 설명: banSmallL 제거. null만 건너뛰고 셔플·상한 절단.
    - 이유: 접대 소형L 밴은 더 이상 선택 게이트가 아님.
  - 심볼: `TrySelectHospitality` / `TrySelectAllClearExact` / `ScoreSurvivors` / `TrySelectCleanChain` / `ApplyDeathReject` / `ToResult` / `CaptureExplain` / `LogGate` / `LogPerf` / `ContainsSmallL` — 메서드 (삭제)
    - 설명: Area·올클·접대·Clean·Death·디버그 게이트 경로를 제거한다.
    - 이유: Phase 40 선택 기준에서 제외.
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `AreaBundlePoolSO.uniqueMinOccupied` — 필드 (추가, `uniqueAreaThreshold` 대체)
    - 설명: Unique dirty 판정용 최소 점유 칸. 기본 40.
    - 이유: dirty를 Area 점수 대신 찬 칸 수로.
  - 심볼: `AreaBundlePoolSO.UniqueMinOccupied` — 프로퍼티 (추가)
    - 설명: `uniqueMinOccupied` 클램프 공개.
    - 이유: Orchestrator dirty 게이트.
  - 심볼: `AreaBundlePoolSO.UniqueAreaThreshold` — 프로퍼티 (삭제)
    - 설명: Area 기반 Unique 게이트 공개를 제거한다.
    - 이유: 점유 칸 게이트로 교체.
  - 심볼: `AreaBundlePoolSO.maxCandidatesToScore` — 필드 기본값 (수정)
    - 설명: 기본 16→64.
    - 이유: Normal 561개 중 히트맵 샘플 폭 확대(히트맵은 MaxArea보다 저렴).
- 파일: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `uniqueMinOccupied` / `maxCandidatesToScore` — 직렬화 (수정)
    - 설명: 40 / 64로 맞춤.
    - 이유: 런타임 튜닝이 코드 기본과 같게.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap.LogDeal` / `ResolveDealStyle` — 메서드 (수정)
    - 설명: Clean/Main·올클·접대 라벨 제거. `heat=` 로 예측 점수 표시.
    - 이유: 단일 Normal·히트맵 선택 반영.
  - 심볼: `BlockSpawnBootstrap.LogHandCompare` — 메서드 (수정)
    - 설명: Area 대신 경로 히트맵 합(rec/act)을 비교한다.
    - 이유: 추천 Explain이 히트맵 경로이므로 같은 척도.
  - 심볼: `TryScoreHeatPath` / `TryScorePlayerHeatPath` / `TryApplyCellsWithHeat` / `FormatHeat` — 메서드 (추가)
    - 설명: 배치 직전 히트맵으로 gain을 누적한 뒤 PlaceAndClear.
    - 이유: HandCompare heat 계산.
  - 심볼: `TryScoreRecommendPath` / `TryScorePlayerPath` / `FormatArea` — 메서드 (삭제)
    - 설명: Area 기반 HandCompare 점수 경로 제거.
    - 이유: 히트맵으로 교체.
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `AreaBundleSelectionGizmo.ResolveModeStyle` — 메서드 (수정)
    - 설명: 라벨을 Unique/Normal/Easy만 표시(Clean·올클·접대 제거).
    - 이유: 티어 단순화 UI 맞춤.

## 2 — 2026-08-13 · 레거시 Area/접대/올클 삭제

**바뀐 것** — 현재 히트맵 스폰에 안 쓰는 Area 점수·접대·올클·완주 솔버·Area 기즈모 프리뷰를 제거한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `drawAreas` / `DrawAreaPartitions` / `DrawAreaPartition` / `DrawAreaSilhouette` — 필드·메서드 (삭제)
    - 설명: 찬/빈 Area 큐브·실루엣 오버레이를 제거한다.
    - 이유: 예전 Area 점수 프리뷰. 현재 선택은 히트맵.
  - 심볼: `DrawModeBanner` — 메서드 (수정)
    - 설명: `찬Area` 대신 `heat`를 표시한다.
    - 이유: 현재 점수 척도 맞춤.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleSelectionResult.cs`
  - 심볼: `HeatScore` — 프로퍼티 (추가, BoardAreaScore/PredictedAreaScore/SequenceCount 삭제)
    - 설명: 손 히트맵 합만 보관한다.
    - 이유: Area 예측 필드 폐기.
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: clean/allClear/areaScore/death/hospitality/survival/cleanShape 관련 필드·프로퍼티 (삭제)
    - 설명: 미사용 게이트·리스트·가중 프로파일을 제거한다. Fill은 Normal+Easy만.
    - 이유: 인스펙터·런타임이 현재 cascade만 노출.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundlePieces.cs`
  - 심볼: `AreaBundlePieces.Build` — 메서드 (이동)
    - 설명: Metrics에서 분리한 번들→offsets 빌더.
    - 이유: Metrics 삭제 후에도 Build 유지.
- 삭제: `AreaBundleMetrics` · `AreaScoreCalculator` · `AreaScoreResult` · `AreaScoreTuning` · `OpportunityDetector` · `HospitalityPiecePolicy` · `PlacementSolver` · `SequenceOutcomeEstimator`
  - 심볼: 각 타입 전체 (삭제)
    - 설명: Area MaxArea·접대·완주 빔 등 구 선택 경로 코드 제거.
    - 이유: 현재 버전 미사용.
- 파일: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateAllClear` / `CreateClean` / `CreateCleanExtras` — 메서드 (삭제)
    - 설명: 올클·Clean 스타터 리스트 생성 제거.
    - 이유: 풀에서 해당 리스트 삭제.
- 파일: `AreaBundleTier` / `ShapeWeightProfile` — enum (수정)
  - 심볼: `AllClear`/`Hospitality`/`Clean` 값 (삭제)
    - 설명: Unique/Normal/Easy · Main/Unique만 남긴다.
    - 이유: 티어·가중 프로파일 단순화.

## 3 — 2026-08-13 · 올클 즉시 채택

**바뀐 것** — 히트맵 평가 중 어느 수에서든 보드가 비는 배치가 있으면 그 패를 즉시 확정한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/HeatmapHandScorer.cs`
  - 심볼: `HeatmapHandScorer.ScoreBest` — 메서드 (수정)
    - 설명: `out bool allCleared` 추가. 어떤 순열에서든 올클이면 즉시 그 경로를 반환한다.
    - 이유: 올클 가능 손을 히트맵 최고보다 우선.
  - 심볼: `HeatmapHandScorer.TryBestPlacement` — 메서드 (수정)
    - 설명: 배치 후보 중 PlaceAndClear 후 점유 0이면 히트맵과 무관하게 즉시 선택.
    - 이유: 1·2·3수 어느 단계에서든 올클 자리를 놓치지 않음.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectByHeatmap` — 메서드 (수정)
    - 설명: 후보 루프에서 `allCleared`면 나머지 샘플을 보지 않고 그 번들 반환.
    - 이유: “나오면 그걸로 정한다”.

## 4 — 2026-08-13 · 올클 상태에선 올클 금지

**바뀐 것** — 시작 보드 점유 0(이미 올클)이면 올클 우선·즉시 채택을 끈다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/HeatmapHandScorer.cs`
  - 심볼: `ScoreBest(..., seekAllClear)` — 파라미터 (추가)
    - 설명: false면 올클 자리 우선·`allCleared` 반환을 하지 않고 히트맵만 쓴다.
    - 이유: 빈 보드에서 연속 올클 패 방지.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectByHeatmap` — 메서드 (수정)
    - 설명: `seekAllClear = CountOccupied(board) > 0`으로 전달.
    - 이유: 올클 상태에서만 올클 게이트 비활성.

## 5 — 2026-08-13 · heat0 칸 −n 페널티 (n=2)

**바뀐 것** — 배치 점수를 `Σheat − n×(heat==0 칸)`으로 바꿔 허공에 걸친 큰 피스를 깎는다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/LineFillHeatmap.cs`
  - 심볼: `ScorePlacement` / `ScoreCells` — 메서드 (추가)
    - 설명: heat 합에서 emptyPenalty×zeroCount를 뺀다.
    - 이유: 밀도 나눗셈 대신 빈 공간 명시 감점.
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `emptyHeatPenalty` / `EmptyHeatPenalty` — 필드·프로퍼티 (추가, 기본 2)
    - 설명: heat==0 칸당 감점 n.
    - 이유: 인스펙터 튜닝.
- 파일: `HeatmapHandScorer` / `AreaBundleOrchestrator` / `BlockSpawnBootstrap` HandCompare — (수정)
  - 심볼: `ScoreBest(..., emptyPenalty)` · 선택·비교 경로 (수정)
    - 설명: 풀의 EmptyHeatPenalty를 점수·HandCompare에 동일 적용.
    - 이유: 추천과 디버그 척도 일치.

## 6 — 2026-08-13 · emptyPen 점수 램프 0→n

**바뀐 것** — `t = 현재점수 / (최대점수/3*2)`, `emptyPen = clamp01(t)×n`. 초반은 페널티 0, 점수 오르면 n까지.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `emptyHeatPenaltyMaxScore` / `ResolveEmptyHeatPenalty` — 필드·메서드 (추가)
    - 설명: 분모용 최대점수(기본 3000)와 t→페널티 변환.
    - 이유: 초반 큰 피스 허용·후반 허공 감점 강화.
- 파일: `Scripts/Domain/Score/ScoreSession.cs`
  - 심볼: `TotalScore` — 프로퍼티 (추가)
    - 설명: 누적 점수 공개.
    - 이유: 스폰 Fill에 전달.
- 파일: `BlockSpawnBootstrap.Fill` / `TurnBootstrap` / `AreaBundleDrawer` / `Orchestrator.Select` — (수정)
  - 심볼: `currentScore` 전달 경로 (추가)
    - 설명: 손 지급 시 세션 총점으로 emptyPen을 계산한다.
    - 이유: 점수 램프가 실제 플레이 점수에 연동.
