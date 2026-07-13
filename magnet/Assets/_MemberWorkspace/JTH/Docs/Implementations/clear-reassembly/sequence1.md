# sequence1 — Phase 1 변경 기록

> Phase 계획: [phase1.md](phase1.md)

## 1 — 2026-07-14 · OccupiedCell 전환 + 최내곽 테두리만 파괴

**바뀐 것** — Domain 보드 유닛을 칸 단위로 교체하고, 클리어는 최내곽 N 테두리만 제거하도록 변경.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Placement/OccupiedCell.cs`
  - 심볼: `OccupiedCell` 생성자/`CellId`/`Position`/`SetPosition` (추가)
    - 설명: 부착 후 보드의 유일한 유닛(칸)을 표현한다.
    - 이유: 멀티칸 `PlacedBlock` 연결 개념을 제거하고 칸 단위 규칙에 맞춘다.
- 파일: `Scripts/Domain/BoardSession.cs`
  - 심볼: `AddCells` / `TryGetCell` / `RemoveCellsAt` / `MoveCell` / `RotateAllClockwise90` (추가·수정)
    - 설명: 셀 목록·격자 점유를 cellId 기준으로 관리한다.
    - 이유: 폭발·재배치·회전이 모두 칸 단위로 동작해야 한다.
  - 심볼: `AddPlacedBlock` / `PlacedBlocks` / `PlacedBlock` 경로 (삭제)
    - 설명: 멀티칸 배치 블록 저장을 제거한다.
    - 이유: 부착 이후 블록 개념 없음.
- 파일: `Scripts/Domain/Placement/PlacedBlock.cs` (삭제)
  - 심볼: 클래스 전체 (삭제)
    - 설명: 보드 부착 엔티티로 더 이상 쓰이지 않는다.
    - 이유: `OccupiedCell`로 대체.
- 파일: `Scripts/Domain/Clear/SquareClearDetector.cs`
  - 심볼: `TryDetectInnermost` (추가), `Detect` (수정)
    - 설명: 완성 N 중 최소 N 하나만 고르고 파괴 칸은 테두리만 반환한다.
    - 이유: 동시 합집합 클리어·바깥 제거 규칙을 폐기한다.
- 파일: `Scripts/Domain/Placement/BlockPlacementService.cs` / `PlacementResult.cs`
  - 심볼: `TryPlace` → `AddCells`, `PlacementResult.CellIds`/`WithCellIds` (추가)
    - 설명: 부착 성공 시 칸 id 목록을 반환한다.
    - 이유: Presentation이 칸 View로 분해·추적한다.

**메모** — 재배치·연쇄는 sequence2.
---
