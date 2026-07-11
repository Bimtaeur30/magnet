# Phase 1 — 자석 흡착 시뮬·배치 Domain

> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] `IBlockShape` + `startPivot` → 흡착 규칙으로 **성공 시 최종 pivot** / **실패 시 사유** 반환
- [x] 경로상 **기존 점유·자석 `(0,0)`** 에 닿으면 **직전** pivot에서 스냅(성공)
- [x] 경로상 블록·자석을 **못 만나 Y 경계만** 만나면 `NoSnapTarget` **실패** (경계에 부착하지 않음)
- [x] 시작 위치가 이미 겹치면 `OverlapsOccupied` / `OverlapsMagnet` 실패
- [x] `BoardSession`이 `BoardGrid` + `PlacedBlock` 목록 보유 (`AddPlacedBlock`)
- [x] `BlockPlacedEvent` 페이로드 확장 (`BlockId`, `SlotIndex`, `ShapeId`, `Pivot`, `CellPositions`)
- [ ] `read_console` 컴파일 에러 0 (에디터 연결 시 확인)

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `MagnetSnapSimulator` | `Domain.Placement` | `TrySnap` — 블록/자석 contact → 성공 pivot, Y 경계만 → false |
| `BlockPlacementService` | `Domain.Placement` | `Simulate` / `TryPlace`. 시작 겹침 → TrySnap → (성공 시) 점유·ID |
| `BlockPlacementCells` | `Domain.Placement` | 절대 칸·겹침·경계 밖 헬퍼 |
| `PlacementResult` | `Domain.Placement` | 성공/실패·BlockId·pivot·칸·`HasCellsOutsideBounds` |
| `PlacementFailureReason` | `Domain.Placement` | `OverlapsOccupied`, `OverlapsMagnet`, `NoSnapTarget` |
| `PlacedBlock` | `Domain.Placement` | 부착 기록 |
| `BoardSession` | `Domain` | `AddPlacedBlock`으로 ID+목록 |

### 스냅 규칙 (`DESIGN.md` §4.3)

- **방향:** `pivotY < 0` → +Y, `pivotY > 0` → -Y, `pivotY == 0` → 흡착 경로 없음 → 실패
- **성공:** 다음 칸이 **점유 또는 자석** → 현재 pivot에 스냅
- **실패 (`NoSnapTarget`):** 다음 칸이 **Y 경계 밖**이고, 그 전에 contact가 없음
- **경계 밖 부착:** 블록/자석에 **붙은 뒤** 형태가 커서 칸이 보드 밖으로 나가면 성공 + `HasCellsOutsideBounds` (게임오버는 SCRUM-22)

> **연출:** Domain은 좌표만. 부드러운 이동은 Phase 4 LitMotion.

## 이 Phase 범위 밖

- 입력, 뷰, 씬 배선, `Consume`, `BoundaryViolationEvent` Raise
- LitMotion, 후보 UI

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 스냅 시뮬 | `Scripts/Domain/Placement/MagnetSnapSimulator.cs` |
| 배치 서비스 | `Scripts/Domain/Placement/BlockPlacementService.cs` |
| 보드 세션 | `Scripts/Domain/BoardSession.cs` |
| 이벤트 페이로드 | `Scripts/Events/MagnetGameEvents.cs` → `BlockPlacedEvent` |
