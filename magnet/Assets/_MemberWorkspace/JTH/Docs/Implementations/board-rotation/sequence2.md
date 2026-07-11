## 1 — 2026-07-11 · 회전 LitMotion + Consume 순서

**바뀐 것** — `PlacementConfigSO.RotationDuration`, `PlacedBlocksView.AnimateBoardRotation`, `BoardPlacementBootstrap` Consume 위치

**변경 상세 (왜/무엇)**
- `Consume`을 클리어·회전 **이후**로 이동 (DESIGN §3)
- 기존 부착 블록: `BoardRotatedEvent` 수신 시 LitMotion 회전 연출
- 신규 블록: Domain 회전 후 최종 `PlacedBlock` 좌표로 스냅
