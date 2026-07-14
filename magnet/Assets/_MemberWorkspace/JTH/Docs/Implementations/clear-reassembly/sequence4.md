# sequence4 — Phase 4 변경 기록

> Phase 계획: [phase4.md](phase4.md)

## 1 — 2026-07-14 · 칸 View 분해 + 달팽이 LitMotion

**바뀐 것** — 부착 후 칸 View로 분해하고, 재배치 웨이브를 튕김·공전·착지 연출로 재생.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/OccupiedCellView.cs`
  - 심볼: `Bind` / `PlaySnailAsync` / `AnimateMoveTo` / `OrbitClockwise` (추가)
    - 설명: 칸 1개 View. 3칸 튕김→시계 1바퀴→착지, 비행 중 자전, 착지 시 격자 0° 스냅.
    - 이유: 물리 없이 LitMotion으로 달팽이 연출.
- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlayPlaceAsync(PlacementResult)` / `PlayReassemblyAsync` / `SplitStagingIntoCells` (추가·수정)
    - 설명: 스냅 후 ShapeBlock을 cellId View로 분해, 웨이브별 파괴·스태거 달팽이·회전.
    - 이유: 보드 위는 칸 View만 유지.
- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `DetachActiveBlocks` (추가), `PlacedBlock` API (삭제)
    - 설명: 스냅 후 Block을 분리해 칸 View에 넘긴다.
    - 이유: 부착 후 멀티칸 ShapeBlock을 해체.
- 파일: `Scripts/Presentation/BlockSnapMotion.cs`
  - 심볼: `PlayFromOffsets` (추가), `PlayFromPlaced` (삭제)
    - 설명: offset+pivot 기준 Y 스냅.
    - 이유: PlacedBlock 제거.
- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `BounceCells`/`OrbitDuration`/`SpinDegreesPerSecond`/`StaggerPerCell`/`StaggerPerRing` 등 (추가)
    - 설명: 재조립 연출 수치 SO.
    - 이유: 튕김 3칸·스태거 등을 인스펙터에서 조정.

**메모** — 화면 밖 튕김 허용.
---
