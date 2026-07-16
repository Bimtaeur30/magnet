# sequence5 — Phase 5 변경 기록

> Phase 계획: [phase5.md](phase5.md)

## 1 — 2026-07-16 · 첫 Place 스냅 startY 고정 + Draw 회전 래퍼

**바뀐 것** — 첫 배치 Y 스냅이 스킵되던 경로를 staging 격자 Y로 시작점을 고정해 고치고, 핸드 Fill 시 형태에 0~270° 랜덤 회전을 준다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `AnimateSnapY` / `AnimateSnapYFromOffsets` (수정)
    - 설명: `stagingGridY + offset.y`로 startY 계산·강제 세팅 후 LitMotion. transform.localPosition.y 읽기 제거.
    - 이유: 첫 Place에서 startY≈targetY로 낙하가 안 보이던 레이스 방어.
    - 영향: `BlockSnapMotion`.
- 파일: `Scripts/Presentation/BlockSnapMotion.cs`
  - 심볼: `Play` / `PlayFromOffsets` (수정)
    - 설명: Animate 호출에 `stagingGridY` 전달.
    - 이유: ShapeBlock API 변경 반영.
- 파일: `Scripts/Domain/Spawn/RotatedBlockShape.cs`
  - 심볼: `RotatedBlockShape` (추가)
    - 설명: 원본 `IBlockShape`의 CellOffsets를 시계 90°×N 회전한 래퍼.
    - 이유: SO 불변 + Domain 배치와 표시 일치.
- 파일: `Scripts/Domain/Spawn/BlockDrawer.cs`
  - 심볼: `Draw` (수정)
    - 설명: 형태 추첨 후 `Next(4)`로 0/90/180/270 적용 (0이면 원본 반환).
    - 이유: 뽑을 때 랜덤 회전 요구.

**메모** — clear-reassembly Phase 5(안쪽 진입)와 별개. §2에서 startY 강제 세팅 롤백.
---
## 2 — 2026-07-16 · Y스냅 시간을 이동 칸 수에 비례

**바뀐 것** — 첫 Place만 초고속으로 보이던 원인을 ‘고정 duration + 긴 이동 거리’로 보고, startY 강제 세팅은 되돌리고 duration을 칸 수 비례로 바꾼다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `AnimateSnapY` / `AnimateSnapYFromOffsets` (수정)
    - 설명: `stagingGridY` 강제 startY 제거(transform 읽기 복구). `duration = SnapDuration * max(cells, 1)`.
    - 이유: 고정 0.12s면 스테이징→자석(첫 배치)처럼 먼 이동만 체감 속도가 폭주한다.
    - 영향: `BlockSnapMotion`.
- 파일: `Scripts/Presentation/BlockSnapMotion.cs`
  - 심볼: `Play` / `PlayFromOffsets` (수정)
    - 설명: Animate에 stagingGridY 전달 제거.
    - 이유: startY 강제 세팅 API 롤백.
- 파일: `Scripts/Data/PlacementConfigSO.cs`, `Docs/INSPECTOR_TOOLTIPS.md`
  - 심볼: `SnapDuration` Tooltip (수정)
    - 설명: ‘총 시간’ → ‘칸 1개당 시간’.
    - 이유: 의미 변경을 Inspector에 반영.

**메모** — Draw 회전 래퍼(§1)는 유지.
---
## 3 — 2026-07-16 · 스테이징 Y 피벗 = 최상단 칸

**바뀐 것** — 긴 세로 블록이 스테이징에서 보드를 침범하지 않도록, 형태 최상단 칸이 staging 행에 오르게 한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Placement/BlockPlacementCells.cs`
  - 심볼: `GetMaxOffsetY` / `GetStagingPivotY` (추가)
    - 설명: `pivotY = stagingGridY - max(offset.y)`.
    - 이유: Domain·표시가 같은 Y 기준으로 맞아야 흡착 시작점이 일치한다.
- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `ShowAtWorldCenter` / `ShowAtSnapStartFromOffsets` (수정)
    - 설명: Y는 중심 대신 최상단 정렬. X는 기하 중심 유지.
    - 이유: 세로로 긴 블록 상단이 보드 안으로 올라가지 않게.
- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `GetCurrentPivot` (수정)
    - 설명: pivot Y를 `GetStagingPivotY`로 계산.
    - 이유: Simulate/Place 시작 행을 표시와 동일하게.

**메모** — X 드래그 중심 정렬은 기존 유지.
---

