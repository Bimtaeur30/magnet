# Phase 3 — 갱신 배선 + 튜닝 SO

> **구현:** `blocked-ring-dim` · **마일스톤:** UX (클리어 직관)  
> **상태:** 구현됨 · 확인 대기  
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 목표 (완료 기준)

- [x] Place 성공 연출 직후 `RefreshBlockedRingDim`
- [x] 각 클리어·재조립 웨이브 연출 직후 refresh
- [x] Rotate 경로에서는 refresh 호출 없음
- [x] `BlockedRingDimConfigSO` + `PlacementConfig` 연결, 기본 에셋 할당
- [x] `read_console` 컴파일 에러 0

## 구현 내용

| 클래스/에셋 | 변경 |
|-------------|------|
| `BlockedRingDimConfigSO` | `dimMultiply` 튜닝 |
| `PlacementConfigSO` | `blockedRingDim` 참조 |
| `BoardPlacementBootstrap` | Place 후·웨이브 후 refresh |
| `PlacedBlocksView` | 배수 SO에서 읽기 (로컬 SerializeField 제거) |
| `DefaultBlockedRingDimConfig` | 기본 0.35 |
| `DefaultPlacementConfig` | SO 할당 |

## 이 Phase 범위 밖

- 드래그 프리뷰 가정 배치 dim
- 튜토리얼·HUD

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 갱신 시점 | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |
| 배수 SO | `Scripts/Data/BlockedRingDimConfigSO.cs` |
| dim 적용 | `Scripts/Presentation/PlacedBlocksView.cs` |
