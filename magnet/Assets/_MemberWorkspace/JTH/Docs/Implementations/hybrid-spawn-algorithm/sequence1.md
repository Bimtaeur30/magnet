# sequence1 — Phase 1 변경 기록

## 1 — 2026-08-02 · 병합 골격 + Hospitality·Pressure 42-ID 이식

**바뀐 것**

- 생성: `Scripts/Domain/HybridSpawn/HybridSpawnOrchestrator.cs` · `HybridPiecePool.cs` · `HybridHospitalityGenerator.cs` · `HybridPressureGenerator.cs` · `HybridSelectionResult.cs` · `HybridTier.cs` · `HybridSpawnProbes.cs`
- 생성: `Scripts/Domain/Spawn/HybridDrawer.cs`
- 생성: `Scripts/Data/HybridTuningSO.cs` · `Scripts/Data/CellCountWeightTable.cs`
- 생성: `Scripts/Domain/BlockSelection/Health/IBoardHealthTuning.cs` · `Blame/IBlameTuning.cs` · `Generation/IOpportunityTuning.cs`
- 수정: `BoardHealthCalculator`·`BlameTracker`·`OpportunityScorer` — 튜닝 파라미터를 인터페이스로 (동작 불변)
- 수정: `BlockSelectionTuningSO` — 인터페이스 3종 구현 선언만 추가
- 수정: `BlockBlastAlgorithm` — `RecordExternalRound(triple)` 추가 (특수 티어 히스토리 기록 + 라운드 진행, 트레이트 우회)
- 수정: `Bootstrap/BlockSpawnBootstrap.cs` — `BlockBlastDrawer` → `HybridDrawer`, BoardHealth·BlameTracker 입력 복구(턴 정산 포함), 티어 스타일 로그
- 생성: `ScriptableObjects/HybridSpawn/DefaultHybridTuning.asset` + 씬(New_02_Main) `BlockSpawnBootstrap` 2개(활성·비활성)에 참조 연결

**메모**

- grill 확정 8건은 `phases.md` 참고. 구 `BlockSelection`·`BlockBlastDrawer`·구 SO는 롤백용 보존, 배선만 교체.
- `IsRetrySession`은 스텁(false) — Relife는 game-over/다시 하기 구현 후 개방.
