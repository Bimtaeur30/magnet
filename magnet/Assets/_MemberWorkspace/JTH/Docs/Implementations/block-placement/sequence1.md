# Sequence — Phase 1 (block-placement)

> **Phase:** [phase1.md](phase1.md) 와 1:1.

## 1 — 2026-07-08 · 자석 흡착 시뮬·배치 Domain

**바뀐 것**

- 생성: `Scripts/Domain/Placement/MagnetSnapSimulator.cs`
- 생성: `Scripts/Domain/Placement/BlockPlacementService.cs`
- 생성: `Scripts/Domain/Placement/BlockPlacementCells.cs`
- 생성: `Scripts/Domain/Placement/PlacementResult.cs`
- 생성: `Scripts/Domain/Placement/PlacementFailureReason.cs`
- 생성: `Scripts/Domain/Placement/PlacedBlock.cs`
- 생성: `Scripts/Domain/BoardSession.cs`
- 수정: `Scripts/Events/MagnetGameEvents.cs` — `BlockPlacedEvent` 페이로드 확장

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Placement/MagnetSnapSimulator.cs`
  - 심볼: `MagnetSnapSimulator` (추가)
    - 설명: 자석 흡착 규칙으로 “최종 정착 pivot”을 계산한다. 내부적으로 한 칸 단위로 진행하지만, 이는 연출이 아니라 격자 규칙 검사를 위한 계산이다.
    - 이유: 입력/뷰 없이도 “흡착 결과”를 재현 가능하게 만들어 Phase 2~4에서 동일 규칙을 재사용하기 위해.
    - 영향: `BlockPlacementService`가 최종 pivot 계산에 사용.
  - 심볼: `Snap(IBlockShape shape, Vector2Int startPivot, BoardGrid grid)` (추가)
    - 설명: `startPivot`에서 시작해 `CanStep(...)`이 false가 될 때까지 pivot을 한 칸씩 이동한 뒤 최종 pivot 반환.
    - 이유: Phase 3에서 “릴리즈 시 즉시 결과”가 필요하므로 단일 API로 고정.
  - 심볼: `GetStepTowardMagnetRow(int pivotY)` (추가)
    - 설명: `pivotY < 0`이면 +Y, `pivotY > 0`이면 -Y, `pivotY == 0`이면 0.
    - 이유: “플레이어는 x만 결정, y는 규칙이 결정”을 코드로 강제.
  - 심볼: `CanStep(...)` (추가)
    - 설명: 다음 pivot에서의 모든 칸이 (1) 자석 `(0,0)`을 밟지 않고 (2) 점유와 겹치지 않으며 (3) 이동 방향의 Y 경계를 넘지 않으면 true.
    - 이유: “경로상 가장 먼저 닿는 블록/경계에서 스냅”을 1칸 단위 보수 검사로 구현.

- 파일: `Scripts/Domain/Placement/BlockPlacementService.cs`
  - 심볼: `BlockPlacementService` (추가)
    - 설명: 배치 규칙의 “판정(시뮬)”과 “상태 반영(부착)” 진입점을 제공한다.
    - 이유: Phase 3(프리뷰/릴리즈)과 Phase 4(부착/이벤트)가 같은 규칙·결과 타입을 공유하도록 만들기 위해.
    - 영향: Phase 3 입력(드래그 릴리즈)·Phase 4 orchestrator가 호출 예정.
  - 심볼: `Simulate(IBlockShape shape, Vector2Int startPivot)` (추가)
    - 설명: 점유 변경 없이 최종 pivot/절대 칸 좌표/경계 밖 여부를 계산해 반환한다.
    - 이유: “미리보기/검증”을 실제 부착과 분리해, 입력 단계에서도 안정적으로 사용할 수 있게 하기 위해.
  - 심볼: `TryPlace(IBlockShape shape, Vector2Int startPivot)` (추가)
    - 설명: 성공 시에만 `BoardGrid` 점유를 반영하고 `PlacedBlock`를 세션에 기록한다.
    - 이유: 실패(겹침/자석)에서도 상태가 변하지 않게 해 디버깅/학습 비용을 줄이기 위해.
  - 심볼: `ApplyPlacement(IBlockShape shape, Vector2Int finalPivot, int blockId)` (추가)
    - 설명: 최종 pivot 기준 절대 칸 좌표 생성 → `SetOccupied` 적용 → `PlacedBlock` 기록.
    - 이유: “판정(BuildResult)”과 “상태 변경”을 분리해 동일 규칙을 유지하기 위해.
  - 심볼: `BuildResult(IBlockShape shape, Vector2Int startPivot)` (추가)
    - 설명: 시작 겹침 검사 → 스냅 → 최종 겹침 검사 → 경계 밖 여부 계산.
    - 이유: 시뮬과 부착이 동일한 판정 루틴을 공유하도록 만들어 규칙 불일치를 방지.

- 파일: `Scripts/Domain/Placement/BlockPlacementCells.cs`
  - 심볼: `BlockPlacementCells` (추가)
    - 설명: offsets→절대 칸 좌표 변환 + 겹침/경계 판정 헬퍼 (internal).
    - 이유: 칸 기준 규칙(자석/점유/경계)을 여러 클래스에서 중복 구현하지 않도록 하기 위해.
    - 영향: `MagnetSnapSimulator`/`BlockPlacementService`가 동일한 판정 로직을 사용.
  - 심볼: `ToAbsolute(IBlockShape shape, Vector2Int pivot)` (추가)
    - 설명: `pivot + offset`을 모두 모아 절대 칸 좌표 리스트를 만든다.
    - 이유: 점유/자석/경계 판정과 이벤트 페이로드(`CellPositions`)의 공통 기반.
  - 심볼: `GetOverlapReason(IReadOnlyList<Vector2Int> cells, BoardGrid grid)` (추가)
    - 설명: 자석 `(0,0)` 포함 시 `OverlapsMagnet`, 점유와 겹치면 `OverlapsOccupied`, 아니면 `None`.
    - 이유: 실패 원인을 타입으로 고정해 표시/분기를 안전하게 만들기 위해.
  - 심볼: `HasAnyCellOutsideBounds(IReadOnlyList<Vector2Int> cells, BoardGrid grid)` (추가)
    - 설명: 절대 칸 중 하나라도 `IsInBounds`를 벗어나면 true.
    - 이유: SCRUM-22(경계 이탈 게임오버)에서 사용할 “판정 데이터”를 먼저 제공.

- 파일: `Scripts/Domain/Placement/PlacementResult.cs`
  - 심볼: `PlacementResult` (추가)
    - 설명: 배치 결과 DTO(성공/실패, 실패 사유, 최종 pivot, 절대 칸 좌표, 경계 밖 여부).
    - 이유: 입력/뷰/이벤트 계층이 “같은 결과 형식”을 공유하도록 해 학습·추적을 쉽게 하기 위해.
  - 심볼: `Succeeded(...)`, `Failed(...)` (추가)
    - 설명: 성공/실패 결과를 생성하는 팩토리 메서드.
    - 이유: 호출부에서 new 인자 실수를 줄이고 의도를 명확히 하기 위해.

- 파일: `Scripts/Domain/Placement/PlacementFailureReason.cs`
  - 심볼: `PlacementFailureReason` (추가)
    - 설명: 실패 사유 enum (`OverlapsOccupied`, `OverlapsMagnet`).
    - 이유: 실패 원인을 문자열 대신 타입으로 고정해서 안정적으로 기록·분기하기 위해.

- 파일: `Scripts/Domain/Placement/PlacedBlock.cs`
  - 심볼: `PlacedBlock` (추가)
    - 설명: “부착된 블록” 기록(블록 ID, ShapeId, pivot, offsets).
    - 이유: Phase 4에서 View 유지, 이후 SCRUM-20/21(폭발/회전)에서 블록 단위 추적이 필요하기 때문.

- 파일: `Scripts/Domain/BoardSession.cs`
  - 심볼: `BoardSession` (추가)
    - 설명: `BoardGrid` 점유 상태 + `PlacedBlock` 목록 + 블록 ID 발급을 보유하는 세션 루트.
    - 이유: `BoardGrid` 단독(SCRUM-17)에서 확장해 “배치로 인해 생성된 블록”을 세션 단위로 남기기 위해.
    - 영향: Phase 2~4에서 Bootstrap이 세션을 만들고, Service가 세션을 수정.
  - 심볼: `AllocateBlockId()` (추가)
    - 설명: 1부터 증가하는 블록 ID 발급.
    - 이유: 이벤트/뷰에서 블록을 안정적으로 식별하기 위한 최소 식별자.
  - 심볼: `RegisterPlacedBlock(PlacedBlock placedBlock)` (추가)
    - 설명: 세션에 부착 블록 기록 추가.
    - 이유: 회전/파괴 단계에서 “현재 존재하는 블록들”을 조회할 수 있게 하기 위해.

- 파일: `Scripts/Events/MagnetGameEvents.cs`
  - 심볼: `BlockPlacedEvent` 페이로드 확장 (수정)
    - 설명: `BlockId`, `SlotIndex`, `ShapeId`, `Pivot`, `CellPositions`를 담도록 확장.
    - 이유: SCRUM-20/21에서 pivot·칸 좌표가 필요하고, Phase 4에서 `Consume(slotIndex)` 연동에도 필요하기 때문.
    - 영향: 기존 `Init(int blockId)` 호출부 없음 (스텁만 존재).

**검증**

- IDE 기준 C# 린트 에러 0.
- Unity 에디터 미연결 상태라 `read_console`로 컴파일 에러 확인은 **미검증**. (에디터 실행 후 확인 필요)

**메모**

- Y 경계 밖 스테이징 진입: 스냅 중 `|y| > half` 는 **이동 방향 반대쪽** 경계에서만 정지. X 경계는 스냅 중 검사하지 않음 (`HasCellsOutsideBounds`로 SCRUM-22에 위임).

---
