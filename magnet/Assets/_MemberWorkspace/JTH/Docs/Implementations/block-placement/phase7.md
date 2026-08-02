# Phase 7 — 배치·프리뷰·뷰 딕셔너리 동기화 수정

## 목표

`PlacedBlocksView` 중복 키 예외와 프리뷰 미표시가 Grid↔뷰 불일치·피벗 폴백·Consume 순서에서 나오던 문제를 막는다.

## 결과

- [x] `TryGetBoardPivot` 마지막 피벗 폴백에 `CanPlace` 재검증
- [x] 릴리즈 시 `CanPlace` 재검증 후 배치
- [x] `AddBlock`: 뷰 등록 → Grid 점유 순서
- [x] `PlaceStagingBlock`: 잔상 칸은 교체(Replace)
- [x] `Consume`은 `AddBlock` 성공 후
- [x] `DestroyCellViews`/`RemoveCellsAt`: `IEnumerable`·null 안전
