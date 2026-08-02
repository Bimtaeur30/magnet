# sequence2 — Phase 2 변경 기록

## 1 — 2026-08-02 · 번들 풀 + Early/Normal Area 최대

**바뀐 것**

- 생성: `Scripts/Data/AreaBundleEntry.cs` · `AreaBundlePoolSO.cs` · `AreaBundleStarterData.cs`
- 생성: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs` (Early/Normal 경로)
- 생성: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset` (early 20 · normal 40)

**심볼**

- `AreaBundleEntry` · `AreaBundlePoolSO` · `AreaBundleStarterData.CreateEarly/CreateNormal`
- `AreaBundleOrchestrator.SelectByMaxArea`
