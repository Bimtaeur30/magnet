# sequence7 — Phase 7 변경 기록

## 1 — 2026-08-02 · Grid↔뷰 동기화·프리뷰 피벗 수정

- 수정: `Scripts/Domain/Placement/PlacementService.cs`
  - 심볼: `TryGetBoardPivot` — 메서드 (수정)
    - 설명: last 피벗 폴백 시 `CanPlace(cellOffsets, last, grid)`가 참일 때만 유지.
    - 이유: 불가 피벗을 sticky로 남겨 프리뷰/배치가 어긋나던 문제 방지.

- 수정: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `OnPointerReleased` — 메서드 (수정)
    - 설명: `_lastBoardPivot`이 있어도 `CanPlace` 실패면 배치하지 않고 선택만 해제.
    - 이유: 마지막 이동 프레임과 손 떼기 사이/폴백으로 생긴 불가 배치 차단.

- 수정: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlaceStagingBlock` — 메서드 (수정)
    - 설명: `Add` 대신 `ReplaceCell` — 기존 칸 잔상은 풀 반환 후 교체.
    - 이유: `ArgumentException` duplicate key 제거.
  - 심볼: `ReplaceCell` — 메서드 (추가)
    - 설명: 칸 잔상 제거 후 새 Block 등록.
    - 이유: PlaceStagingBlock 교체 경로.
  - 심볼: `DestroyCellViews` — 메서드 (수정)
    - 설명: 인자 `IEnumerable<Vector2Int>`, null이면 no-op.
    - 이유: `as IReadOnlyList` null로 클리어 뷰가 스킵되던 위험 제거.
  - 심볼: `ReturnBlocks` / `PushBlock` — 메서드 (추가)
    - 설명: 미배치 블록·잔상 공통 풀 반환.
    - 이유: Place 거절 시 고아 Block 방지.

- 수정: `Scripts/Presentation/GameBoard.cs`
  - 심볼: `AddBlock` — 메서드 (수정)
    - 설명: 뷰 `PlaceStagingBlock` 후 Grid `SetOccupied`.
    - 이유: 뷰 실패 전에 Grid만 차는 불일치 방지.
  - 심볼: `ReturnUnplacedBlocks` — 메서드 (추가)
    - 설명: 뷰 `ReturnBlocks` 위임.
    - 이유: Bootstrap이 거절된 detached를 풀로 돌림.
  - 심볼: `RemoveCellsAt` — 메서드 (수정)
    - 설명: null 가드 + `DestroyCellViews(gridPositions)` 직접 전달.
    - 이유: 잘못된 cast로 뷰 미삭제 방지.

- 수정: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `PlaceBlock` — 메서드 (수정)
    - 설명: 자유 칸 검사 → AddBlock → Consume → 클리어. 실패 시 ReturnUnplacedBlocks.
    - 이유: Consume이 배치보다 앞서 슬롯이 사라지던 문제 + 예외 후 상태 꼬임 완화.
  - 심볼: `IsPlacementFree` — 메서드 (추가)
    - 설명: 절대 좌표가 보드 안·비점유인지 확인.
    - 이유: PlaceBlock 가드.
