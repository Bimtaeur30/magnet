# Phase 2 — 점유 칸 dim Presentation

> **구현:** `blocked-ring-dim` · **마일스톤:** UX (클리어 직관)  
> **상태:** 구현됨 · 확인 대기  
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] `Block` / `OccupiedCellView` dim on·off API
- [x] `PlacedBlocksView.RefreshBlockedRingDim` — 비활성 N 링 점유 칸만 어둡게
- [x] 디밍 강도 SerializeField 임시값 (`blockedRingDimMultiply`)
- [x] 스킨 적용 후에도 dim 상태 유지
- [x] `read_console` 컴파일 에러 0

## 구현 내용

| 클래스 | 변경 |
|--------|------|
| `Block` | `SetDimmed` — RGB 배수 / 기본색 복원. `ApplyVisual` 후 색 재적용 |
| `OccupiedCellView` | dim 상태 보관 후 `Block`에 전달 |
| `PlacedBlocksView` | `RefreshBlockedRingDim` + `SyncWithSession` 끝에서 호출. 배수 SerializeField |

## 이 Phase 범위 밖

- Place / 재조립 경로 자동 refresh (Bootstrap — Phase 3)
- Config SO
- 드래그 프리뷰

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 색 dim | `Scripts/Presentation/Block.cs` |
| View 전달 | `Scripts/Presentation/OccupiedCellView.cs` |
| 링→칸 적용 | `Scripts/Presentation/PlacedBlocksView.cs` |
| 판정 | `Scripts/Domain/Clear/BlockedRingDetector.cs` |
