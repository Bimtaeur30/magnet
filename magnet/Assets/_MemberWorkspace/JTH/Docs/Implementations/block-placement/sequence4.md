# Sequence — Phase 4 (block-placement)

> **Phase:** [phase4.md](phase4.md) 와 1:1.

## 1 — 2026-07-11 · 부착 확정·X순간이동+Y Lit·배치 실패 시 선택 해제·3슬롯 소진 후 Fill

**바뀐 것**

- 생성: `Scripts/Presentation/PlacedBlocksView.cs`
- 생성: `Scripts/Presentation/BlockSnapMotion.cs`
- 수정: `Scripts/Bootstrap/BoardPlacementBootstrap.cs` — `TryConfirmPlacement`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs` — `Consume(slotIndex)`, 전부 소진 시 `Fill`, `BlockCandidatesUpdatedEvent`
- 수정: `Scripts/Domain/Spawn/BlockSupply.cs` — `AreAllSlotsEmpty`, `Consume` 범위 검사
- 수정: `Scripts/Input/BlockDragInput.cs` — Release `TryPlace`·스냅 연출·실패 시 `DisconnectSelection`
- 수정: `Scripts/Input/BlockDragDrawer.cs` — `TakeStagingForPlacement`
- 수정: `Scripts/Presentation/ShapeBlock.cs` — `ShowAtSnapStart`, `AnimateSnapY`, `CancelMotions`
- 수정: `Scripts/Events/MagnetGameEvents.cs` — `BlockCandidatesUpdatedEvent`
- 수정: `Scripts/Data/PlacementConfigSO.cs` — `snapDuration`
- 수정: `ScriptableObjects/DefaultPlacementConfig.asset`
- 수정: `Scripts/Magnet.JTH.asmdef` — LitMotion 참조
- 수정: `Scenes/Phase0_Bootstrap.unity` — `PlacedBlocks` GO, `BlockDragInput.placedBlocksView`
- 수정: `Docs/INSPECTOR_TOOLTIPS.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `TryConfirmPlacement(shape, startPivot, slotIndex)` (추가)
    - 설명: `TryPlace` 성공 시 `BlockPlacedEvent`·`BoundaryViolationEvent`(필요 시)·`BlockSpawn.Consume` 순서 실행.
    - 이유: 입력 계층과 Domain·이벤트·공급 연결을 Bootstrap 한곳에 모음.
    - 영향: `BlockDragInput.OnPointerReleased`.

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `Consume(int slotIndex)` (수정)
    - 설명: 슬롯을 `null`로 소모. **3슬롯 전부 비었을 때만** `Fill()`. 매번 `BlockCandidatesUpdatedEvent` Raise.
    - 이유: 사용자 지정 — 개별 재추첨이 아니라 한 세트(3개)를 모두 쓴 뒤 새 세트.
  - 심볼: `Start` (수정)
    - 설명: 초기 `Fill` 후 후보 이벤트 방송 추가.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `OnPointerReleased` (수정)
    - 설명: `TryConfirmPlacement` 호출. 실패 → `DisconnectSelection`(재선택 필요). 성공 → 스테이징 분리 후 `BlockSnapMotion.Play`.
    - 이유: 실패 시 스테이징 재표시가 아닌 연결 끊기 UX.
  - 심볼: `_isPlacing` (추가)
    - 설명: Y Lit 중 입력·선택 무시.

- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `ShowAtSnapStart` (추가)
    - 설명: 각 칸 X = 최종 pivot 열, Y = 스테이징 높이 — 프리뷰 X로 순간이동.
  - 심볼: `AnimateSnapY` (추가)
    - 설명: 칸별 local Y만 LitMotion(`OutQuad`, `snapDuration`).

- 파일: `Scripts/Presentation/BlockSnapMotion.cs`
  - 심볼: `Play(...)` (추가)
    - 설명: `ShowAtSnapStart` → `AnimateSnapY` 오케스트레이션.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `Adopt(ShapeBlock, name)` (추가)
    - 설명: 스냅 완료 피스를 보드 자식으로 유지.

- 파일: `Scripts/Events/MagnetGameEvents.cs`
  - 심볼: `BlockCandidatesUpdatedEvent` (추가)
    - 설명: `CreateSnapshot()` 배열을 후보 갱신 페이로드로 전달.

**검증**

- `read_console` Error 0 (에디터 리컴파일 후).
- Play: 1/2/3 선택 → 드래그 → Release(유효): X 즉시 프리뷰 열, Y 짧게 Lit → 보드 잔존, 점유 반영.
- Release(무효): 스테이징 사라짐, 같은 번호 재입력 전까지 표시 없음.
- 3블록 연속 부착 후에만 새 3후보 `Fill` + `BlockCandidatesUpdatedEvent`.
- `HasCellsOutsideBounds` 시 `BoundaryViolationEvent` Raise.

**메모**

- `TakeStagingForPlacement` 후 `DisconnectSelection`은 새 빈 스테이징만 Clear — 애니메이션 중 피스는 분리된 인스턴스.
- LitMotion은 `Magnet.JTH.asmdef`에 KTJ와 동일 GUID 참조 추가.

---

## 2 — 2026-07-11 · PlacedBlocksView Inject 수정·줄바꿈 정리·DI 문서

**바뀐 것**

- 수정: `Scripts/Input/BlockDragInput.cs` — `placedBlocksView` `[SerializeField]` → `[Inject]`; 이중 줄바꿈 정리
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs` — 이중 줄바꿈 정리
- 수정: `Scripts/Bootstrap/MagnetSceneInstaller.cs` — `placedBlocksView` RegisterValue
- 수정: `Scenes/Phase0_Bootstrap.unity` — 배선을 Installer로 이동
- 수정: `.cursor/rules/jth-reflex-di.mdc`, `Docs/AI_RULES_REFERENCE.md`, `CLAUDE.md`
- 생성: `Docs/DI_FIELD_AUDIT.md`

**변경 상세 (왜/무엇)**

- `PlacedBlocksView`는 씬 MonoBehaviour — 소비자 SerializeField 금지 (`jth-reflex-di.mdc`).
- `BlockDragInput`·`BlockSpawnBootstrap`에 빈 줄이 매 라인마다 끼어 포맷이 깨져 있었음.

---
