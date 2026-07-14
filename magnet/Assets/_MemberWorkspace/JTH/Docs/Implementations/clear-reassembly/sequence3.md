# sequence3 — Phase 3 변경 기록

> Phase 계획: [phase3.md](phase3.md)

## 1 — 2026-07-14 · Bootstrap 턴 루프·입력 잠금·이벤트

**바뀐 것** — Place 후 재조립 연쇄→연출→회전 순서로 바꾸고, 이벤트·입력 잠금을 연결.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `IsTurnResolving` (추가), `TryConfirmPlacement` (수정), `RaiseReassemblyEvents` (추가)
    - 설명: Domain `ResolveAllWaves` → SquareCleared/CellsRelocated Raise → PlayReassembly → Rotate.
    - 이유: 연쇄 종료 전에 회전하지 않고, 연출 중 입력을 잠근다.
- 파일: `Assets/_Shared/Magnet.Core/Events/MagnetGameEvents.cs`
  - 심볼: `CellsRelocatedEvent` / `Init` (추가)
    - 설명: 웨이브별 재배치 cellId·from·to 페이로드.
    - 이유: 클리어와 재배치 이벤트를 분리한다.
- 파일: `Scripts/Domain/Turn/TurnResolutionResult.cs`
  - 심볼: `Reassembly` (추가, ClearResult 대체)
    - 설명: 턴 결과에 재조립 웨이브 결과를 담는다.
    - 이유: 구 ClearDetectionResult 단일 패스와 계약이 달라짐.
- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: 선택/드래그/확정 가드에 `IsTurnResolving` (수정)
    - 설명: 턴 해석 중 입력 무시.
    - 이유: 연출·연쇄 중 입력 잠금.

**메모** — 달팽이 LitMotion은 sequence4.
---
