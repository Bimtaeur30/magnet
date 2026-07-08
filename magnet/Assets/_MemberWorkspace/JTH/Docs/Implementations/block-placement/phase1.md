# Phase 1 — 자석 흡착 시뮬·배치 Domain

> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] 빈 `BoardGrid` + `IBlockShape` + `(pivotX, stagingY)` → **스냅 후 최종 pivot** 반환
- [x] 정착 좌표 계산 중 **기존 점유 칸·Y 경계·자석 `(0,0)`** 에 닿으면 **직전** pivot에서 정지
- [x] 겹침/자석 칸 위 배치 시 `PlacementResult`로 **실패 사유** 반환
- [x] `BoardSession`이 `BoardGrid` + `PlacedBlock` 목록 보유
- [x] `BlockPlacedEvent` 페이로드 확장 (`BlockId`, `SlotIndex`, `ShapeId`, `Pivot`, `CellPositions`)
- [x] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `MagnetSnapSimulator` | `Domain.Placement` | 자석 흡착 규칙으로 **최종 정착 pivot** 계산 (연출은 Phase 4 Presentation). 내부적으로 격자 검사를 위해 1칸 단위 진행 |
| `BlockPlacementService` | `Domain.Placement` | `Simulate` / `TryPlace`. 시작 겹침 검사 → 스냅 → 최종 겹침 검사 → (선택) 점유 등록 |
| `BlockPlacementCells` | `Domain.Placement` | 절대 칸 좌표·겹침·경계 밖 판별 헬퍼 (internal) |
| `PlacementResult` | `Domain.Placement` | 성공/실패·최종 pivot·칸 목록·`HasCellsOutsideBounds` |
| `PlacedBlock` | `Domain.Placement` | 부착된 블록 기록 (ID, ShapeId, pivot, offsets) |
| `BoardSession` | `Domain` | `BoardGrid` + `PlacedBlock` 목록 + ID 할당 |

### 스냅 규칙

- **방향:** `pivotY < 0` → +Y, `pivotY > 0` → -Y, `pivotY == 0` → 이동 없음
- **스테이징:** 보드 아래(Y 경계 밖)에서 시작 가능 — 진입 중인 칸은 Y 경계 검사에서 허용
- **정지 조건:** 다음 칸이 점유·자석·Y 상/하한(`±half`) 초과
- **경계 밖 부착:** 배치는 성공할 수 있음. `HasCellsOutsideBounds`로 SCRUM-22에 전달 (이 Phase에서 이벤트 Raise 없음)

> **메모:** 실제 “부드러운 흡착 이동”은 Presentation에서 final pivot(또는 final world pos)을 구한 뒤 LitMotion으로 연출한다. Domain은 좌표 계산만 담당.

## 이 Phase 범위 밖

- 입력, 뷰, 씬 배선, `Consume`, `BoundaryViolationEvent` Raise
- LitMotion 연출, 후보 UI

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 스냅 시뮬 | `Scripts/Domain/Placement/MagnetSnapSimulator.cs` |
| 배치 서비스 | `Scripts/Domain/Placement/BlockPlacementService.cs` |
| 보드 세션 | `Scripts/Domain/BoardSession.cs` |
| 이벤트 페이로드 | `Scripts/Events/MagnetGameEvents.cs` → `BlockPlacedEvent` |
