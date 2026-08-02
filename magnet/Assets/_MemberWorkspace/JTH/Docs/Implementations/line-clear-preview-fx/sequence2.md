# line-clear-preview-fx Sequence 2

> Phase 1 보완 · Effector + Preview 정렬

## 변경 상세

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block.SortingBandPreview` / `SortingBandPlaced` / `SortingBandStaging` — 상수 (추가)
    - 설명: Preview(1000) &lt; Placed(10000) &lt; Staging(20000) 그리기 밴드.
    - 이유: Preview가 Staging보다 위에 그려지던 sortingOrder 증가 문제 해결.
  - 심볼: `Block.ApplySortingBand(int bandBase)` — 메서드 (추가)
    - 설명: 밴드 기준으로 `SetSortingOrder`를 호출한다.
    - 이유: ShapeBlock·PlacedBlocksView가 역할별 순서를 명시적으로 지정.

- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `ShapeBlock._sortingBand` — 필드 (추가)
    - 설명: Show=Staging, ShowPreview=Preview 밴드를 보관.
    - 이유: 셀 생성 시 올바른 sortingOrder 적용.
  - 심볼: `ShapeBlock.ShowPreview(...)` — 메서드 (수정)
    - 설명: Preview 밴드로 `ShowCells` 후 PreviewAlpha 적용 (더 이상 Show→덮어쓰기만 하지 않음).
    - 이유: Staging 밴드로 먼저 그린 뒤 Preview로 바꾸는 경로를 제거.

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.SetHints(...)` — 메서드 (추가)
    - 설명: 클리어 칸의 placed 블록 + 인자로 받은 프리뷰 칸만 `SetClearHint(true)`.
    - 이유: 클리어 줄만 깜빡이고, 힌트 소유권을 View 쪽으로 모음.
  - 심볼: `LineClearHintEffector.ClearHints()` — 메서드 (추가)
    - 설명: 이전 힌트 전부 off.
    - 이유: 스냅 해제·재적용 시 잔상 방지.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.lineClearHintEffector` — 필드 (추가)
    - 설명: 같은 GO의 Effector 참조 (없으면 AddComponent).
    - 이유: 보드 칸 연출을 View 계층에 둔다.
  - 심볼: `PlacedBlocksView.SetLineClearHints` / `ClearLineClearHints` — 메서드 (추가)
    - 설명: Effector로 위임.
    - 이유: GameBoard 진입점.
  - 심볼: `PlacedBlocksView.ReplaceCell` — 메서드 (수정)
    - 설명: 배치 시 `SortingBandPlaced` 적용.
    - 이유: Staging에서 detach된 블록이 Staging order로 남지 않게.

- 파일: `Scripts/Presentation/GameBoard.cs`
  - 심볼: `GameBoard.SetLineClearHints` / `ClearLineClearHints` — 메서드 (추가)
    - 설명: `_blocksView` Effector API 위임.
    - 이유: Input은 GameBoard만 Inject.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `BlockDragInput._clearHintBlocks` — 필드 (삭제)
    - 설명: Input이 직접 힌트 리스트를 들고 있지 않음.
    - 이유: Effector로 이전.
  - 심볼: `BlockDragInput.UpdateLineClearHints(...)` — 메서드 (수정)
    - 설명: 시뮬 → clearedCells → 프리뷰 중 `offset+pivot ∈ cleared`만 모아 `GameBoard.SetLineClearHints`.
    - 이유: 클리어 줄 밖 프리뷰 칸은 깜빡이지 않음.

## 에셋

- `Prefabs/Board/Placed Blocks View.prefab` — `LineClearHintEffector` 배선
