# phase12 — 구 스폰 알고리즘 dead code 제거

## 목표

`area-bundle-spawn`으로 교체된 뒤 미배선으로 남아 있던 실험 알고리즘 코드·에셋을 삭제한다.

## 범위

### 삭제
- `Domain/HybridSpawn/**` + `HybridDrawer` + `HybridTuningSO` + `DefaultHybridTuning.asset`
- `BlockBlastAlgorithm` / `BlockBlastSelection` / `BlockBlastDrawer` (카탈로그만 유지)
- `BlockSelection` 오케스트레이터·Health·Blame·Bundles·Generation·Tiers·ShapeRotator + Drawer/Tuning/번들 SO
- `RandomDrawer` (미배선)
- `CellCountWeightTable` / `BlockShapeWeight` / `BlockBundleSO` / `BlockBundlePoolSO`

### 유지 (AreaBundle 의존)
- `BlockBlastCatalog` — 42-ID 오프셋
- `PlacementSimulator` / `PlacementSolver` / `SequenceOutcomeEstimator`
- `UniqueSolution` / `SolutionStep` (`PlacementSolver` 의존)

### 정리
- `BlockSpawnContext`에서 Health/Blame/LastTurnClearedCells 제거

## 완료 기준
- [x] 미배선 구 알고리즘 코드·에셋 삭제
- [x] AreaBundle 배선·컴파일 유지
- [x] IMPLEMENTATIONS 상태 갱신 (코드 보존 → 제거)
