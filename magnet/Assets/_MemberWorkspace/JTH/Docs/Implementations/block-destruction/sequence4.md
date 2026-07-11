## 1 — 2026-07-11 · PlacedBlocksView 클리어·회전 Presentation

**바뀐 것** — `PlacedBlocksView.cs`, `ShapeBlock.cs`, `BlockSnapMotion.cs`, `BlockDragInput.cs` 수정

**변경 상세 (왜/무엇)**
- `PlacedBlocksView`: blockId 추적, `SquareClearedEvent`/`BoardRotatedEvent` 리스너
- `ShapeBlock`: `ShowPlaced`, `RemoveCellsAtGrid`, `AnimateRotateClockwise90`
- `BlockDragInput`: 턴 후 Domain 상태(`PlacedBlock`) 기준 스냅 연출
