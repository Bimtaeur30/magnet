# Phase 3 — x축 드래그 입력·포인터 추적·스냅 프리뷰

> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 목표 (완료 기준)

- [x] `MagnetInputSO` 포인터(마우스·터치) → 월드 X → **블록 시각 중심**이 포인터 X와 일치하도록 pivot 계산
- [x] 드래그 중 **스테이징 Y 고정**, X만 변경
- [x] **추적:** 선택 블록이 포인터 X를 **즉시** 따라감 (보간·LitMotion 없음)
- [x] **감도 램프:** 드래그 누적 거리에 따라 포인터보다 블록이 더 많이 이동 (Block Blast식). Pointer Up 시 감도 Reset
- [x] **프리뷰(시뮬):** `BlockPlacementService.Simulate` 결과를 격자에 **딱딱** 표시
- [x] 형태 폭·중심을 고려한 **X 클램프**
- [x] 키보드 1/2/3 **선택 후** Press+드래그에서만 추적
- [x] Pointer Up → `Simulate` 호출·로그 (부착·이벤트는 Phase 4)
- [x] 마우스·터치 동일 경로 (`Pointer` + `PointerPress`)
- [ ] `read_console` 컴파일 에러 0 (에디터 확인)

## 입력·좌표 (핵심 UX)

### pivot — 블록 중심 = 포인터 (드래그 중)

- `BlockPlacementCells.GetShapeCenterOffset`로 기하 중심 계산.
- 드래그 중: `ShowAtWorldCenter(worldCenterX, stagingY)` — 연속 월드 X.
- Simulate·프리뷰: `WorldCenterXToPivotX` → 격자 pivot.

### 추적 vs 프리뷰

| 레이어 | 느낌 | 구현 |
|--------|------|------|
| **추적** | 포인터와 동일 속도 + 감도 램프 | `StagingBlock` `BlockPieceView.ShowAtWorldCenter` |
| **프리뷰** | 격자 스냅 | `PreviewBlock` `BlockPieceView.Show(pivot, previewColor)` |

### 감도 램프 (`DragSensitivityRamp`)

```
distanceFromOrigin = |currentPointerX - pressOriginX|
multiplier = min(maxMultiplier, 1 + distanceFromOrigin * rampPerUnit)
blockDeltaX = pointerDeltaX * multiplier
// Press 시 origin 저장 · Up 시 Reset
```

- 설정: `PlacementConfigSO.dragSensitivityRampPerUnit`, `dragSensitivityMaxMultiplier`

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `DragSensitivityRamp` | `Domain.Placement` | Press 원점 거리 → 배율. 생성 시 `rampPerUnit`·`maxMultiplier` 주입 (Unity·Input 의존 0) |
| `BlockPlacementCells` | `Domain.Placement` | 중심·pivot X 범위·월드↔pivot |
| `BlockDragInput` | `Input/` | 선택 후 Press+Move 드래그·감도·Simulate·뷰 갱신 |
| `MagnetInputSO` | `Data/` | `OnPointerPressed` / `OnPointerReleased` |
| `BlockPieceView` | `Presentation/` | `ShowAtWorldCenter`, 색상 오버로드 |
| `MagnetSceneInstaller` | `Bootstrap/` | `stagingBlockView`, `previewBlockView`, `boardPlacementBootstrap` Reflex 등록 |
| `BlockSpawnBootstrap` | `Bootstrap/` | 선택 시 초기 스테이징 `(0, stagingY)` (Phase 2 유지) |
| `BoardPlacementBootstrap` | `Bootstrap/` | `BlockPlacementService` — DragInput이 Simulate 호출 |

## 이 Phase 범위 밖

- `TryPlace`·점유·`BlockPlacedEvent`·`Consume` (Phase 4)
- LitMotion 흡착 연출 (Phase 4)

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 드래그 입력 | `Scripts/Input/BlockDragInput.cs` |
| 감도 램프 | `Scripts/Domain/Placement/DragSensitivityRamp.cs` |
| 포인터 입력 SO | `Scripts/Data/MagnetInputSO.cs` |
| 스테이징·프리뷰 뷰 | `Scripts/Presentation/BlockPieceView.cs` |
| 씬 배선 | `Scenes/Phase0_Bootstrap.unity` — `BlockDragInput`, `PreviewBlock`, `MagnetSceneInstaller` |
