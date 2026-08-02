# sequence5 — cascade 변경 기록

## 1 — 2026-08-02 · Unique→Normal→Easy cascade

**바뀐 것**

- `AreaBundleOrchestrator` — Relife 1턴 Easy / dirty·pUnique Unique → Normal → Easy. 킬 패 폐기(Easy만 가중 랜덤)
- `AreaBundlePoolSO` — `easyBundles`, `uniqueProbability`, `relifeEasyTurnCount`. Early 게이트 제거
- `AreaBundleStarterData.CreateEasy` — 보장 1x1 패 + 관측 소형/1x1 + 구 Early (26)
- `AreaBundleTier` — Early 제거, Easy 추가
- `AreaBundleDrawer` / `BlockSpawnBootstrap` — `IsRetrySession` 전달(스텁 false)

**메모**

- Unique 리스트는 샘플 대기(비어 있으면 Unique 스킵 → Normal).
