# board-rotation — Phase 3

## 목표 (완료 기준)

- `TryConfirmPlacement`가 Place → Clear → Rotate를 **Domain → Raise → await FX** 순으로 비동기 실행한다.
- Place 스냅은 **회전 전** Session 좌표 기준. Consume은 **전 연출 종료 후**(정상 회전 턴만).
- `PlacedBlocksView`는 Clear/Rotate 이벤트 연출 구독을 제거하고, Bootstrap이 호출하는 UniTask API만 제공한다.
- Clear 사라짐 LitMotion은 아직 없어도 `PlayClearAsync` 자리에 바로 끼울 수 있다.

## 구현 내용

### `BoardPlacementBootstrap.TryConfirmPlacement`

1. `TryPlace` → `BlockPlaced` Raise → `PlayPlaceAsync`
2. `DetectAndApply` → `SquareCleared` Raise(들) → `PlayClearAsync`
3. bounds 위반 시 `BoundaryViolation` Raise 후 return (Rotate·Consume 없음)
4. `RotateClockwise` → `BoardRotated` Raise → `PlayRotateAsync`
5. `Consume(slotIndex)`

Domain 서비스는 sync 유지. 연출 await만 Bootstrap이 순차 수행.

### `PlacedBlocksView`

- `PlayPlaceAsync` / `PlayClearAsync` / `PlayRotateAsync`
- `SquareCleared` / `BoardRotated` 리스너 제거 (연출 이중 재생 방지)

### `BlockDragInput`

- 실패는 `Simulate`로 걸러 staging을 분리하지 않음
- 성공 시 staging을 인자로 넘겨 `await TryConfirmPlacement`
- `_isPlacing`은 전 턴 연출 동안 true

## 범위 밖

- Clear 사라짐 LitMotion 본구현
- Domain 메서드 UniTask화
- 새 Event 타입

## 코드·에셋 맵

| 심볼 | 경로 |
|------|------|
| `BoardPlacementBootstrap.TryConfirmPlacement` | `Scripts/Bootstrap/` |
| `PlacedBlocksView.PlayPlaceAsync` 등 | `Scripts/Presentation/` |
| `BlockDragInput.ConfirmPlacementAsync` | `Scripts/Input/` |
