# Phase 3 — x축 드래그 입력·포인터 추적·스냅 프리뷰

> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  
> **상태:** 미착수  
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 목표 (완료 기준)

- [ ] `MagnetInputSO` 포인터(마우스·터치) → 월드 X → **블록 시각 중심**이 포인터 X와 일치하도록 pivot 계산
- [ ] 드래그 중 **스테이징 Y 고정**, X만 변경
- [ ] **추적(연출):** 선택 블록이 포인터를 **자연스럽게** 따라감 (부드러운 X 이동)
- [ ] **프리뷰(시뮬):** `BlockPlacementService.Simulate` 결과를 격자에 **딱딱** 표시 (칸 스냅, 보간 없음)
- [ ] 형태 폭·중심을 고려한 **X 클램프** (보드 밖으로 칸이 나가지 않도록)
- [ ] Pointer Up → `Simulate`/`TryPlace` 호출 준비 (실제 부착·이벤트는 Phase 4)
- [ ] **터치** 입력 경로 동작 (`Controls` Pointer — `<Mouse>/position` + `<Touchscreen>/primaryTouch/position`, Phase 2 `MagnetInputSO` 밑작업)
- [ ] `read_console` 컴파일 에러 0

## 입력·좌표 (핵심 UX)

### pivot `(0,0)`이 아니라 **블록 중심 = 포인터**

- Phase 2는 staging pivot `x=0`(형태 정의 pivot) 기준. Phase 3부터 **포인터 X ↔ 블록 시각 중심**을 맞춘다.
- `IBlockShape.CellOffsets`로 **격자 기준 중심**(offsets의 기하 중심)을 구하고, 포인터 월드 X에 중심이 오도록 **역산 pivot**을 계산한다.
- **홀수·짝수 폭 구분 없음** — 중심만 포인터와 일치하면 됨. pivot이 반칸 어긋나도 중심 정렬이 우선.

```
포인터 worldX
  → (선택) 연출용: 블록 뷰 중심을 worldX에 부드럽게 추적
  → 격자용: centerGridX = Round(worldX / cellSize)
  → pivot.x = centerGridX - shapeCenterOffsetX   // int 스냅 규칙은 구현 시 offsets 기준으로 확정
  → pivot.y = stagingY (고정)
```

### 추적 vs 프리뷰 (연출 분리)

| 레이어 | 느낌 | 구현 |
|--------|------|------|
| **추적** | 자연스럽게 따라옴 | `BlockPieceView` — 포인터 X에 중심 맞춰 **연속** 이동 (LitMotion 등, Y= staging 고정) |
| **프리뷰** | 딱딱, 격자 스냅 | `Simulate(shape, pivot)` 결과 칸 — **격자 좌표 그대로** 표시, 보간 없음 (고스트/가이드) |

- Domain `Simulate`는 Phase 1과 동일 — **최종 정착 pivot만** 계산. 프리뷰 UI는 Presentation.
- 릴리즈 전까지 추적 피스 + (선택) 프리뷰 고스트를 동시에 쓸 수 있음.

### 모바일 (스마트폰)

- 별도 터치 API **추가하지 않음**. `MagnetInputSO.OnPointerChange` / `GetWorldPointerPosition()` 단일 경로.
- `Controls.inputactions` Pointer에 **Touchscreen** 바인딩 이미 있음 (Phase 2). Phase 3는 드래그 로직만 **입력 소스 무관**하게 연결.
- 에디터: 마우스. 빌드: 터치 — 동일 코드 경로로 검증.

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `BlockDragInput` | `Input/` | `MagnetInputSO` 포인터 구독. 드래그 상태·pivot 계산(중심 정렬)·Up 시 Domain 호출 |
| `BlockShapeBounds` (또는 `BlockPlacementCells` 확장) | `Domain.Placement` | offsets → 기하 중심·min/max X (클램프·pivot 역산용). Unity·Input 의존 0 |
| `BlockPieceView` | `Presentation/` | `ShowAtWorldCenter` 또는 pivot+연출 분리 — **추적**은 연속 X, **프리뷰**는 격자 스냅 |
| `PlacementPreviewView` (선택) | `Presentation/` | `Simulate` 결과 칸만 딱딱 표시. 없으면 `BlockPieceView` 이중 모드 |
| `BoardPlacementBootstrap` | `Bootstrap/` | DragInput·Service·StagingView 연결 |

### 입력

- `MagnetInputSO` — `[SerializeField]`, `Update`/`GetKeyDown` 금지 (`FOLDER_STRUCTURE.md` §입력).
- 드래그: Pointer pressed → 이동 중 추적+프리뷰 갱신 → released → Phase 4에서 `TryPlace`.

## 이 Phase 범위 밖

- `TryPlace` 점유 반영·`BlockPlacedEvent`·`Consume` (Phase 4)
- 부착 후 보드 잔존 피스, LitMotion **흡착** 연출 (Phase 4)
- 게임오버·토스트
- 하단 후보 UI (M7)

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 포인터 입력 SO | `Scripts/Data/MagnetInputSO.cs` |
| Input Actions | `Scripts/Input/Controls.inputactions` |
| 스테이징 뷰 | `Scripts/Presentation/BlockPieceView.cs` |
| 배치 시뮬 | `Scripts/Domain/Placement/BlockPlacementService.cs` |
| 좌표 유틸 | `Scripts/Domain/BoardCoordinates.cs` |
| 부트스트랩 | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |

## Phase 2와의 관계

- Phase 2: 키 1/2/3 선택 → staging `(0, stagingY)`에 **pivot 기준** 표시.
- Phase 3: 선택 후 포인터 드래그 → **중심 추적** + **Simulate 프리뷰(딱딱)**. 초기 X는 첫 포인터 또는 보드 중앙 등 — 구현 시 확정.
