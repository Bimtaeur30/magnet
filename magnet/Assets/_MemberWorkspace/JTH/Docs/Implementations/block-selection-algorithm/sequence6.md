# Sequence — Phase 6 (block-selection-algorithm)

> **Phase:** [phase6.md](phase6.md) 와 1:1.

## 1 — 2026-08-01 · Pressure + Orchestrator + UniqueSolution

**바뀐 것**

- 생성: `Scripts/Domain/BlockSelection/Solution/SolutionStep.cs`
- 생성: `Scripts/Domain/BlockSelection/Solution/UniqueSolution.cs`
- 수정: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs`
- 생성: `Scripts/Domain/BlockSelection/Generation/PressureGenerator.cs`
- 생성: `Scripts/Domain/BlockSelection/Generation/FallbackGenerator.cs`
- 생성: `Scripts/Domain/BlockSelection/SelectionTier.cs`
- 생성: `Scripts/Domain/BlockSelection/BlockSelectionResult.cs`
- 생성: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` — Pressure 7필드 + Fallback 1필드

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Solution/SolutionStep.cs`
  - 심볼: `SolutionStep` — readonly struct (`SlotIndex`, `Pivot`, `CellOffsets`, `ClearedLines`) (추가)
    - 설명: 유일해의 배치 1스텝 (SPEC §11.5).
    - 이유: `ClearedLines`는 난이도 판정(setupClear) 입력을 재탐색 없이 제공.

- 파일: `Scripts/Domain/BlockSelection/Solution/UniqueSolution.cs`
  - 심볼: `UniqueSolution.Steps` — 프로퍼티 (추가)
  - 심볼: `UniqueSolution.MatchesStep(currentStepIndex, placedSlotIndex, placedPivot, out placedCells)` — 메서드 (추가)
    - 설명: 스텝 순서·슬롯·피벗 일치 판정 + 엄지척 UI용 배치 칸 목록 (SPEC §11.5).
    - 이유: 라인 클리어로 보드가 변하므로 순서까지 일치해야 정답 — 스펙 명시.
    - 영향: UI 작업 시 배치 이벤트에서 호출 예정 (Phase 7 메모).

- 파일: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs`
  - 심볼: `PlacementSolver.SequenceRecorder` — private class (추가)
    - 설명: DFS 스택을 따라가다 첫 완주 시퀀스를 `Captured`로 고정 (`??=`로 1회만).
    - 이유: count==1 확인 후 재탐색하면 같은 DFS 2회 — 기록기로 1회에 해결 (phase6.md 결정).
  - 심볼: `PlacementSolver.TryFindUniqueFullSequence(board, pieces)` — public static (추가)
    - 설명: cap=2 카운트 + 기록. count==1이면 `UniqueSolution`, 아니면 null.
  - 심볼: `CountSequences / Search / TryPlacements` — 시그니처 변경 (recorder 파라미터, TryPlacements는 offsets 대신 pieceIndex)
    - 설명: 스텝 기록에 슬롯 index가 필요해 piece 참조 대신 index 전달. 기존 호출 경로는 recorder=null로 동작 불변.

- 파일: `Scripts/Domain/BlockSelection/Generation/PressureGenerator.cs`
  - 심볼: `PressureGenerator.PressureDraw` — class (`Pieces`, `Solution`, `Difficulty`) (추가)
  - 심볼: `PressureGenerator.TryGenerate(board, pool, tuning, rng)` — public static (추가)
    - 설명: 샘플 × `TryFindUniqueFullSequence` → 난이도 하한 통과 후보 중 최고 난이도 선택 (SPEC §11.3 "preference for higher difficulty").
  - 심볼: `PressureGenerator.ComputeDifficulty(solution, tuning)` — private static (추가)
    - 설명: bigFinish(마지막 스텝 ≥ `PressureBigFinishMinCells`칸) + setupClear(앞 스텝 클리어 ≥1) 2항 (SPEC §11.2 단순화, phase6.md 결정).

- 파일: `Scripts/Domain/BlockSelection/Generation/FallbackGenerator.cs`
  - 심볼: `FallbackGenerator.TryGenerate(board, pool, tuning, rng)` — public static (추가)
    - 설명: 샘플링 중 hasAny+fullSequence면 즉시 반환, 아니면 첫 hasAny 조합 유지 후 반환 (SPEC §9.7 1·2차).

- 파일: `Scripts/Domain/BlockSelection/SelectionTier.cs`
  - 심볼: `SelectionTier` — enum 8종 (추가)

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionResult.cs`
  - 심볼: `BlockSelectionResult` — class (추가)
    - 설명: SPEC §16.3 — `Tier`, `BundleId`, `WasGenerated`(=BundleId null), `HealthScore`, `Zone`, `Blame`, `IsBrilliantEscapeCandidate`(=Tier==Pressure), `UniqueSolution`, `Pieces`.
    - 이유: 로그(§20)·UI 훅(§11.4·§11.5) 데이터의 단일 운반체.

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `BlockSelectionOrchestrator(tuning, bundles, rng)` — 생성자 (추가)
    - 설명: `tuning.BlockWeights`에서 티어별 `WeightedShape` 풀 3종 구성.
  - 심볼: `BlockSelectionOrchestrator.SelectPieces(board, health, blame, isRetrySession, turnIndex)` — 메서드 (추가)
    - 설명: 부록 A 스택 — 0 Relife → 1 Trap → 2 ComboBreak → 3 Hospitality(확률) → 4 Easy → 5 Pressure → 6 Normal → 7 Fallback → 최후 Normal 강제. 보드는 진입 시 1회 클론.
    - 이유: health는 Bootstrap이 리필당 1회 계산해 주입 — blame 정산과 공유 (Phase 7).
    - 영향: `BlockSelectionDrawer`(Phase 7)가 호출.
  - 심볼: `ForceNormalAny / TryPickBundle / Roll / FromBundle / FromGenerated` — private (추가)
    - 설명: 최후 강제 선택(Normal hasAny → 가중 샘플)·번들 픽 위임·확률 롤·결과 조립.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `PressureProbability / PressureHealthThreshold / PressureSampleCount / PressureDifficultyMin / PressureBigFinishWeight / PressureSetupClearWeight / PressureBigFinishMinCells` — 프로퍼티 7종 (추가)
    - 설명: §9.5·§11 수치 (0.5 / 0.45 / 40 / 0.5 / 0.5 / 0.5 / 5).
  - 심볼: `FallbackSampleCount` — 프로퍼티 (추가, 기본 40)

**검증**

- execute_code:
  - 강제 유일해 보드(4×4, 홀 3칸 + 커스텀 13칸·16칸 피스): `CountFullSequences=1`, `TryFindUniqueFullSequence` non-null, 스텝 3개 replay 시 recordedClears==replayClears (4/8/8), `MatchesStep` 정답 true·오답 피벗 false ✅
  - 빈 보드 Orchestrator → tier=Normal, hasAny ✅ / 더러운 보드(TooDirty) → 3피스 반환, hasAny ✅

**메모**

- `BlamePressureThreshold`(§13.3)는 부록 A 게이트에 없어 미사용 — "가중" 규칙 확정 시 반영.
- `MatchesStep`은 동일 모양 피스가 다른 슬롯에 있는 희귀 케이스에서 슬롯 불일치로 오답 처리될 수 있음 (phase6.md 결정).
