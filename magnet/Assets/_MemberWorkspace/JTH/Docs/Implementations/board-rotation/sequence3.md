## 1 — 2026-07-13 · 턴 연출 Bootstrap 순차 UniTask

**바뀐 것** — `BoardPlacementBootstrap.TryConfirmPlacement` async화, `PlacedBlocksView` FX API, `BlockDragInput` await 호출

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `_placedBlocksView` — 필드 `PlacedBlocksView` (추가)
    - 설명: Place·Clear·Rotate 연출 UniTask를 호출할 뷰 참조를 Inject로 보관한다.
    - 이유: 연출 소유권을 Bootstrap에 모으려면 뷰 API를 직접 await해야 한다.
    - 영향: `TryConfirmPlacement`가 `PlayPlaceAsync` / `PlayClearAsync` / `PlayRotateAsync`를 순차 await.
  - 심볼: `Awake()` — 메서드 (수정)
    - 설명: `_placedBlocksView` null Assert를 추가한다.
    - 이유: Inject 실패를 조기에 발견한다.
  - 심볼: `TryConfirmPlacement(shape, startPivot, slotIndex, staging)` — 메서드 (수정)
    - 설명: `async UniTask<TurnResolutionResult>`로 바꾸고, Place→Clear→Rotate를 Domain→Raise→await FX 순으로 실행한다. staging `ShapeBlock`을 받아 Place FX에 넘긴다. `TryPlace` 실패 시 staging을 정리한다. Consume은 Rotate FX 완료 후에만 호출한다.
    - 이유: LitMotion이 단계마다 생겨도 await 자리만 채우면 되도록 패턴 ②를 고정한다. Place 스냅은 회전 전 좌표에 맞춘다.
    - 영향: `BlockDragInput.ConfirmPlacementAsync`가 await 호출.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `magnetGameChannel` — 필드 (삭제)
    - 설명: Clear/Rotate 이벤트 구독에 쓰이던 채널 필드를 제거한다.
    - 이유: 연출은 Bootstrap이 직접 호출하므로 이벤트 리스너가 필요 없다.
  - 심볼: `OnEnable()` / `OnDisable()` — 메서드 (삭제)
    - 설명: `SquareCleared`·`BoardRotated` 리스너 등록/해제를 제거한다.
    - 이유: Bootstrap await와 이벤트 연출이 겹치면 회전이 두 번 재생된다.
  - 심볼: `OnSquareCleared` / `OnBoardRotated` — 메서드 (삭제)
    - 설명: 이벤트 기반 Sync·회전 연출 핸들러를 제거한다.
    - 이유: 동일하게 연출 소유권을 Bootstrap으로 이전.
  - 심볼: `PlayPlaceAsync(staging, blockId)` — 메서드 (추가)
    - 설명: 회전 전 `PlacedBlock`으로 Y 스냅(`BlockSnapMotion`) 후 Register/Adopt하고 완료를 UniTask로 반환한다.
    - 이유: Place 단계 await 훅. 스냅 목표는 Domain 회전 전 좌표.
    - 영향: `BoardPlacementBootstrap.TryConfirmPlacement` Place 단계.
  - 심볼: `PlayClearAsync()` — 메서드 (추가)
    - 설명: `SyncWithSession` 후 즉시 완료 UniTask를 반환한다.
    - 이유: 사라짐 LitMotion이 생기면 이 메서드에서 await하면 된다.
    - 영향: Bootstrap Clear 단계.
  - 심볼: `PlayRotateAsync()` — 메서드 (추가)
    - 설명: `AnimateBoardRotation`을 UniTaskCompletionSource로 감싸 완료까지 await 가능하게 한다.
    - 이유: Bootstrap이 Rotate 연출 종료를 기다려 Consume 시점을 맞춘다.
    - 영향: Bootstrap Rotate 단계.
  - 심볼: `AnimateBoardRotation(onComplete)` — 메서드 (유지)
    - 설명: 기존 콜백 기반 회전 연출. `PlayRotateAsync`가 사용한다.
    - 이유: LitMotion 콜백 API를 바꾸지 않고 UniTask 래퍼만 추가.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `_placedBlocksView` — 필드 (삭제)
    - 설명: Input이 직접 Register/Adopt·스냅하던 뷰 참조를 제거한다.
    - 이유: Place 연출·등록은 Bootstrap→View로 이동.
  - 심볼: `OnPointerReleased()` — 메서드 (수정)
    - 설명: `ConfirmPlacementAsync().Forget()`만 호출한다.
    - 이유: 포인터 콜백은 sync 시그니처를 유지하고 본처리는 UniTask로 넘긴다.
  - 심볼: `ConfirmPlacementAsync()` — 메서드 (추가)
    - 설명: `Simulate` 실패 시 선택만 해제. 성공 시 staging을 분리해 `TryConfirmPlacement`를 await하고, `_isPlacing`을 전 연출 동안 유지한다.
    - 이유: 실패 드롭에서 staging을 불필요하게 Take하지 않고, 턴 연출 중 재입력을 막는다.
    - 영향: 부착 확정 진입점.

**메모** — Place 스냅은 회전 전 좌표 → 이후 Rotate FX가 신규 블록도 90° 연출. 기존 “신규는 회전 후 좌표로 스냅”과 달라짐.
---
