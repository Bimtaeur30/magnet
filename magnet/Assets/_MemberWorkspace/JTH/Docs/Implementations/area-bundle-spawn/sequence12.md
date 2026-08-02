# sequence12 — Phase 12 변경 기록

## 1 — 2026-08-02 · 구 스폰 알고리즘 dead code 제거

- 수정: `Scripts/Domain/Spawn/BlockSpawnContext.cs`
  - 심볼: `Health` — 프로퍼티 `BoardHealthResult` (삭제)
    - 설명: 구 Health 존 결과를 Drawer에 넘기던 필드를 제거한다.
    - 이유: AreaBundle은 Health/Blame 입력을 쓰지 않으며 타입도 삭제됨.
  - 심볼: `BlameTotal` — 프로퍼티 `float` (삭제)
    - 설명: 구 Blame 누적값을 Drawer에 넘기던 필드를 제거한다.
    - 이유: BlameTracker·하이브리드 게이트가 없어져 소비처가 없음.
  - 심볼: `LastTurnClearedCells` — 프로퍼티 `int` (삭제)
    - 설명: Momentum 게이트용 직전 클리어 칸 수를 제거한다.
    - 이유: 구 BlockSelection Momentum만 사용했고 현재 cascade에 없음.

- 삭제: `Scripts/Domain/HybridSpawn/**` (폴더 전체)
  - 심볼: `HybridSpawnOrchestrator.Select(...)` — 메서드 (삭제)
    - 설명: Relife→Trap→ComboBreak→Hospitality→Pressure→BaseChain 게이트 선택을 제거한다.
    - 이유: AreaBundle cascade로 교체되어 Bootstrap 미배선.
  - 심볼: `HybridPiecePool` / `HybridHospitalityGenerator` / `HybridPressureGenerator` / `HybridConstraintGenerator` / `HybridSpawnProbes` / `HybridSelectionResult` / `HybridTier` — 타입 (삭제)
    - 설명: 하이브리드 전용 풀·생성기·DTO·enum을 제거한다.
    - 이유: Orchestrator와 함께만 쓰이던 실험 코드.

- 삭제: `Scripts/Domain/Spawn/HybridDrawer.cs`
  - 심볼: `HybridDrawer.Draw(BlockSpawnContext, int)` — 메서드 (삭제)
    - 설명: HybridOrchestrator 결과를 AbstractDrawer로 노출하던 경로를 제거한다.
    - 이유: `AreaBundleDrawer`가 대체.

- 삭제: `Scripts/Data/HybridTuningSO.cs` · `CellCountWeightTable.cs` · `ScriptableObjects/HybridSpawn/DefaultHybridTuning.asset`
  - 심볼: `HybridTuningSO` / `CellCountWeightTable` — 타입 (삭제)
    - 설명: 하이브리드 게이트·칸수 가중 튜닝 SO/헬퍼와 에셋을 제거한다.
    - 이유: 소비자 코드 삭제.

- 삭제: `Scripts/Domain/BlockBlast/BlockBlastAlgorithm.cs` · `BlockBlastSelection.cs` · `Scripts/Domain/Spawn/BlockBlastDrawer.cs`
  - 심볼: `BlockBlastAlgorithm.Select(BoardGrid)` — 메서드 (삭제)
    - 설명: 7→1370→2100 핸드오프 체인 선택을 제거한다.
    - 이유: AreaBundle이 카탈로그만 재사용; 알고리즘 본체는 미배선.
  - 심볼: `BlockBlastSelection` / `BlockBlastDrawer.Draw(...)` — 타입·메서드 (삭제)
    - 설명: 핸드오프 결과 DTO와 Drawer를 제거한다.
    - 이유: Algorithm과 함께만 사용.

- 삭제: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs` · `BlockSelectionResult.cs` · `SelectionTier.cs`
  - 심볼: `BlockSelectionOrchestrator.Select(...)` — 메서드 (삭제)
    - 설명: Health/Blame 기반 티어 스택 선택을 제거한다.
    - 이유: handoff→hybrid→area-bundle로 이미 교체됨.
  - 심볼: `BlockSelectionResult` / `SelectionTier` — 타입 (삭제)
    - 설명: 구 선택 결과·티어 enum을 제거한다.
    - 이유: Orchestrator 전용.

- 삭제: `Scripts/Domain/BlockSelection/Health/**` · `Blame/**` · `Bundles/**` · `Generation/**` · `Tiers/**` · `Simulation/ShapeRotator.cs`
  - 심볼: `BoardHealthCalculator.Compute` / `BlameTracker` / `BundleTierSelector` / `HospitalityGenerator` / `PressureGenerator` / `ShapeRotator.Rotate` 등 — 타입·메서드 (삭제)
    - 설명: 구 티어 스택의 헬스·블레임·번들·생성기·회전 유틸을 제거한다.
    - 이유: AreaBundle이 쓰지 않음. Simulation 솔버·Estimator만 유지.

- 삭제: `Scripts/Domain/Spawn/BlockSelectionDrawer.cs` · `RandomDrawer.cs`
  - 심볼: `BlockSelectionDrawer.Draw` / `RandomDrawer.Draw` — 메서드 (삭제)
    - 설명: 구 티어 Drawer와 균등 랜덤 Drawer를 제거한다.
    - 이유: Bootstrap이 `AreaBundleDrawer`만 사용.

- 삭제: `Scripts/Data/BlockSelectionTuningSO.cs` · `BlockBundleSO.cs` · `BlockBundlePoolSO.cs` · `BlockShapeWeight.cs` · `ScriptableObjects/BlockSelection/**`
  - 심볼: 해당 SO·가중치·번들 에셋 — 타입·에셋 (삭제)
    - 설명: 구 알고리즘 Inspector 튜닝·번들 풀 에셋을 제거한다.
    - 이유: 스크립트 타입 삭제에 따른 고아 에셋 정리.

- 유지: `BlockBlastCatalog` · `PlacementSimulator` · `PlacementSolver` · `SequenceOutcomeEstimator` · `UniqueSolution` · `SolutionStep`
  - 설명: AreaBundleMetrics/Orchestrator/UniqueUnlockGenerator가 42-ID·완주 탐색에 계속 사용.
  - 이유: 공유 유틸이므로 네임스페이스만 구 경로에 남아 있어도 삭제하지 않음.
