# sequence4 — Phase 4 변경 기록

## 1 — 2026-08-02 · Bootstrap 배선

**바뀐 것**

- 생성: `Scripts/Domain/Spawn/AreaBundleDrawer.cs`
- 생성: `AreaBundleTier.cs` · `AreaBundleSelectionResult.cs`
- 수정: `Bootstrap/BlockSpawnBootstrap.cs` — Hybrid/Blame 제거, AreaBundle 경로
- 씬 `BlockSpawnBootstrap` ×2 → `areaBundlePoolSO = DefaultAreaBundlePool`

**메모**

- 구 `HybridDrawer`·`HybridSpawnOrchestrator`·`HybridTuningSO`는 롤백용 보존(배선만 교체).
