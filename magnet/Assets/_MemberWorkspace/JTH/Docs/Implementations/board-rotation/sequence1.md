## 1 — 2026-07-11 · GridRotation + BoardRotationService

**바뀐 것** — `Domain/Rotation/GridRotation.cs`, `BoardRotationService.cs`, `BoardSession.RotateAllClockwise90` 생성/추가

**변경 상세 (왜/무엇)**
- `(x,y) → (y,-x)` 시계방향 90° (자석 원점)
- 모든 PlacedBlock pivot/offset 회전 후 격자 점유 재구성
