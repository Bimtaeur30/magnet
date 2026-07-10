## 1 — 2026-07-11 · Bootstrap 턴 파이프라인 + SquareClearedEvent

**바뀐 것** — `BoardPlacementBootstrap.cs`, `MagnetGameEvents.cs`, `TurnResolutionResult.cs` 수정/생성

**변경 상세 (왜/무엇)**
- 턴 순서: `TryPlace` → `BlockPlacedEvent` → 클리어 → `SquareClearedEvent`(size별) → 90° 회전 → `BoardRotatedEvent` → `Consume`
- `SquareClearedEvent`에 `ClearedCells` 페이로드 추가 (Presentation용)
- `ScoreAwarded = 0` (SCRUM-23)
