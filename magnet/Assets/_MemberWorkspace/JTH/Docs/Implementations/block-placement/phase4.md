# Phase 4 — 부착 확정·이벤트·공급 연동

> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence4.md](sequence4.md) (1:1)

## 목표 (완료 기준)

- [x] Pointer Up → `TryPlace` (실패 시 점유·공급 변화 없음)
- [x] **성공:** 프리뷰 X로 순간이동 → Y만 LitMotion 스냅 → `PlacedBlocksView`에 잔존
- [x] **성공:** `BlockPlacedEvent` Raise (`BlockId`, `SlotIndex`, `ShapeId`, `Pivot`, `CellPositions`)
- [x] **성공:** 슬롯 `Consume` — **3슬롯 전부 소진 시에만** `Fill` + `BlockCandidatesUpdatedEvent`
- [x] **성공 + `HasCellsOutsideBounds`:** `BoundaryViolationEvent` Raise
- [x] **실패:** 스테이징 연결 해제 — 같은 번호를 다시 눌러야 스테이징
- [x] 부착·스냅 연출 중 입력 무시 (`_isPlacing`)
- [ ] `read_console` 컴파일 에러 0 (에디터 확인)

## 부착 UX (핵심)

### 손 놓기 (성공)

1. `TryPlace`로 Domain 반영
2. 스테이징 `ShapeBlock` 분리 (`TakeStagingForPlacement`)
3. **X:** `ShowAtSnapStart` — 최종 pivot 열(프리뷰와 동일)로 **순간이동**
4. **Y:** `AnimateSnapY` — LitMotion `OutQuad`, `PlacementConfigSO.snapDuration`
5. 완료 → `PlacedBlocksView.Adopt`

### 손 놓기 (실패)

- `DisconnectSelection` — 스테이징·프리뷰 Clear, 선택 해제
- 후보 슬롯은 그대로 — **1/2/3 재입력** 필요

### 공급

- `Consume(slotIndex)` → 해당 슬롯 `null`
- **3슬롯 모두 `null`일 때만** `Fill()` 재추첨
- 매 `Consume`·초기 `Start` 후 `BlockCandidatesUpdatedEvent` Raise

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `BoardPlacementBootstrap` | `Bootstrap/` | `TryConfirmPlacement` — `TryPlace` → 이벤트 → `Consume` |
| `BlockSpawnBootstrap` | `Bootstrap/` | 슬롯 소모·전부 소진 시 `Fill`·후보 이벤트 |
| `BlockSupply` | `Domain.Spawn` | `Consume` null 처리, `AreAllSlotsEmpty` |
| `BlockDragInput` | `Input/` | Release orchestration, `_isPlacing` 가드 |
| `BlockDragDrawer` | `Input/` | `TakeStagingForPlacement` |
| `BlockSnapMotion` | `Presentation/` | X 순간이동 + Y Lit 진입점 |
| `ShapeBlock` | `Presentation/` | `ShowAtSnapStart`, `AnimateSnapY` |
| `PlacedBlocksView` | `Presentation/` | 부착 피스 부모 컨테이너 |
| `BlockCandidatesUpdatedEvent` | `Events/` | 후보 스냅샷 |
| `PlacementConfigSO` | `Data/` | `snapDuration` |

## 이 Phase 범위 밖

- `GameOverEvent` 본체·후보 3개 전부 배치 불가 (SCRUM-22)
- 폭발·회전 (SCRUM-20/21)
- 하단 후보 UI (M7)

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 부착 orchestration | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |
| Release 입력 | `Scripts/Input/BlockDragInput.cs` |
| Y LitMotion | `Scripts/Presentation/ShapeBlock.cs`, `BlockSnapMotion.cs` |
| 부착 잔존 뷰 | `Scripts/Presentation/PlacedBlocksView.cs` |
| 공급 소모 | `Scripts/Bootstrap/BlockSpawnBootstrap.cs`, `Domain/Spawn/BlockSupply.cs` |
| 씬 배선 | `Scenes/Phase0_Bootstrap.unity` — `Board/PlacedBlocks`, `MagnetSceneInstaller.placedBlocksView` → `RegisterValue` |
