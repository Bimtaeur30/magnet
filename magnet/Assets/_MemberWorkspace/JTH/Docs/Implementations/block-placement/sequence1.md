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
- Domain의 1칸 단위 진행은 **연출이 아니라 최종 정착 좌표 계산**이다. 부드러운 이동은 Phase 4 Presentation(LitMotion)에서 final pivot으로 처리.

---

## 2 — 2026-07-08 · TryPlace 결과 재생성 제거

**바뀐 것**

- 수정: `Scripts/Domain/Placement/BlockPlacementService.cs`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Placement/BlockPlacementService.cs`
  - 심볼: `TryPlace` (수정)
    - 설명: 성공 시 `PlacementResult.Succeeded(...)`로 동일 결과를 다시 만들지 않고, `BuildResult`가 준 `result`를 그대로 반환한다.
    - 이유: 같은 pivot/칸/경계 정보로 새 인스턴스를 만드는 것은 불필요 할당·읽기 비용만 늘린다.
  - 심볼: `ApplyPlacement(...)` (수정)
    - 설명: `ToAbsolute`를 다시 호출하지 않고 `result.CellPositions`를 받아 점유 등록에 사용한다.
    - 이유: 판정 단계에서 이미 만든 절대 칸 좌표를 재사용해 중복 계산을 없애기 위해.

**검증**

- 이번 변경은 동작 동일·할당만 줄인 리팩터. IDE 린트 확인.

**메모**

- (구) BlockId를 Result에 안 넣던 방식 → `## 3`에서 `WithBlockId`로 수정.

---

## 3 — 2026-07-08 · BlockId·세션 API·겹침 규칙 정리

**바뀐 것**

- 수정: `Scripts/Domain/Placement/PlacementResult.cs`
- 수정: `Scripts/Domain/BoardSession.cs`
- 수정: `Scripts/Domain/Placement/BlockPlacementService.cs`
- 수정: `Scripts/Domain/Placement/MagnetSnapSimulator.cs`
- 수정: `Scripts/Domain/Placement/BlockPlacementCells.cs`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Placement/PlacementResult.cs`
  - 심볼: `BlockId` 프로퍼티 (추가), `WithBlockId(int)` (추가)
    - 설명: 성공 배치 후 발급된 블록 ID를 결과에 실어 반환한다. `Simulate`는 `BlockId == 0`.
    - 이유: `TryPlace` 호출부가 `Session.PlacedBlocks` Last를 뒤지지 않고도 이벤트 Raise에 쓸 ID를 바로 얻게 하기 위해.
  - 심볼: `Succeeded(..., int blockId = 0)` (시그니처 확장)
    - 설명: 기본은 0(시뮬). 부착 확정 후 `WithBlockId`로 ID를 채운다.
    - 이유: 판정 결과와 ID 발급 시점을 분리하면서도 반환 타입은 하나로 유지.
- 파일: `Scripts/Domain/BoardSession.cs`
  - 심볼: `AllocateBlockId` / `RegisterPlacedBlock` (삭제)
  - 심볼: `AddPlacedBlock(shapeId, pivot, cellOffsets)` (추가)
    - 설명: ID 발급 + `PlacedBlock` 목록 등록을 한 메서드에서 처리하고 발급 ID를 반환한다.
    - 이유: ID만 발급하고 등록을 빠진 상태를 불가능하게 만들기 위해.
- 파일: `Scripts/Domain/Placement/BlockPlacementService.cs`
  - 심볼: `TryPlace` (수정)
    - 설명: 점유 반영 → `AddPlacedBlock` → `result.WithBlockId(blockId)` 반환.
    - 이유: BlockId를 Result에 실어 Phase 4 이벤트와 맞추기 위해.
  - 심볼: `ApplyPlacement` (삭제)
    - 설명: 점유 루프를 `TryPlace`에 인라인, 블록 기록은 `BoardSession.AddPlacedBlock`에 위임.
    - 이유: 얇은 private 래퍼가 세션 API와 책임이 겹쳐 제거.
  - 심볼: `BuildResult` — final `GetOverlapReason` 호출 (삭제)
    - 설명: 최종 겹침 재검사를 제거. Snap이 이미 점유·자석에서 멈춤.
    - 이유: 시작 겹침만 통과하면 final은 동일 규칙상 안전 — 죽은 검사 제거.
- 파일: `Scripts/Domain/Placement/MagnetSnapSimulator.cs` / `BlockPlacementCells.cs`
  - 심볼: `CanStep` → `BlockPlacementCells.HasOverlap(shape, nextPivot, grid)` (수정)
  - 심볼: `GetOverlapReason(IBlockShape, Vector2Int, BoardGrid)` (추가)
    - 설명: 자석·점유 겹침 규칙을 Cells 한곳에만 두고 Snap이 재사용. Y 경계만 Snap 쪽에 남김.
    - 이유: 규칙이 두 파일에 복제되면 나중에 자석 규칙 변경 시 어긋나기 쉬움.
  - 심볼: `GetOverlapReason(IReadOnlyList<...>)` (삭제)
    - 설명: shape+pivot 경로로 통일해 미사용 오버로드 제거.

**검증**

- IDE 린트 확인 예정. Unity `read_console`은 에디터 연결 시.

**메모**

- `WithBlockId`는 새 `PlacementResult`를 만든다. 이전 “동일 Succeeded 재생성”과는 다름 — **추가된 필드(BlockId)를 채우기 위한** 복사.

---

## 4 — 2026-07-08 · 흡착 규칙 정정 (경계 ≠ 스냅 대상)

**바뀐 것**

- 수정: `Scripts/Domain/Placement/MagnetSnapSimulator.cs`
- 수정: `Scripts/Domain/Placement/BlockPlacementService.cs`
- 수정: `Scripts/Domain/Placement/PlacementFailureReason.cs`
- 수정: `Docs/DESIGN.md` (§2.1·§3·§4.3·§4.6, 변경 이력 0.6)
- 수정: `Docs/Implementations/block-placement/phase1.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Placement/PlacementFailureReason.cs`
  - 심볼: `NoSnapTarget` (추가)
    - 설명: 흡착 경로에서 블록·자석을 못 만나 Y 경계에만 닿았을 때의 실패 사유.
    - 이유: “그 x에 놓을 수 없음”을 점유/자석 겹침과 구분하기 위해.
- 파일: `Scripts/Domain/Placement/MagnetSnapSimulator.cs`
  - 심볼: `Snap(...)` → `TrySnap(..., out Vector2Int finalPivot)` (교체)
    - 설명: contact(점유·자석)면 `true` + 직전 pivot. Y 경계만이면 `false` (경계 pivot은 out에 남기되 호출부는 사용하지 않음).
    - 이유: 구 API는 경계에서도 “성공 pivot”을 돌려 빈 보드에서 반대편 테두리에 붙는 잘못된 규칙을 강제했음.
  - 심볼: `EvaluateNext` (추가)
    - 설명: next의 Y 경계 초과 → `Boundary`, 겹침 → `Contact`, 아니면 `None`으로 한 칸 진행.
    - 이유: 경계와 contact를 같은 “정지”로 취급하지 않기 위해.
  - 심볼: `stepY == 0` 분기 (수정)
    - 설명: 자석 행에 이미 있으면 흡착 경로 없음 → `false`.
    - 이유: 스테이징은 Y≠0에서 시작한다는 전제와 맞춤.
- 파일: `Scripts/Domain/Placement/BlockPlacementService.cs`
  - 심볼: `BuildResult` (수정)
    - 설명: `TrySnap` 실패 시 `PlacementResult.Failed(NoSnapTarget)`. 성공 시에만 절대 칸·`HasCellsOutsideBounds` 계산.
    - 이유: 경계-only는 부착하지 않음. `HasCellsOutsideBounds`는 **블록/자석에 붙은 뒤** 형태가 삐져나온 경우(SCRUM-22)만.
- 파일: `Docs/DESIGN.md`
  - 심볼: §2.1 / §3 표 / §4.3 / §4.6 (수정)
    - 설명: “블록 또는 경계에 스냅” 삭제. 스냅 대상 = 블록·자석. 경계만 = 배치 불가. 게임오버 조건과 구분.
    - 이유: 팀 설계 문서가 실제 게임 규칙과 어긋나 있어 동시 갱신.

**검증**

- 빈 보드 + 1×1 `x≠0` 스테이징 → `NoSnapTarget`.
- 빈 보드 + 1×1 `x=0` → 자석 contact → 최종 `(0, -1)`(아래에서 시작 시) 성공.
- IDE 린트. Unity 콘솔은 에디터 연결 시.

**메모**

- “배치 불가(그 x)” ≠ “즉시 게임오버”. 3후보×모든 x가 전부 실패일 때 게임오버는 SCRUM-22.

---
