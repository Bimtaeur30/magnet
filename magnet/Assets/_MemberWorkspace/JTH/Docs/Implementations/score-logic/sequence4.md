## 1 — 2026-07-18 · ComboChangedEvent Raise

**바뀐 것** — `MagnetGameEvents`에 콤보 변경 이벤트 추가. 배치·턴 종료 시 값이 바뀔 때만 Raise.

**변경 상세 (왜/무엇)**  
- 파일: `Assets/_Shared/Magnet.Core/Events/MagnetGameEvents.cs`
  - 심볼: `MagnetGameEvents.ComboChangedEvent` — 정적 필드 (추가)
    - 설명: 재사용 싱글톤 `ComboChangedEvent` 인스턴스를 보관한다.
    - 이유: 다른 Magnet 이벤트와 동일하게 `RaiseEvent(MagnetGameEvents.X.Init(...))` 계약을 맞춘다.
  - 심볼: `ComboChangedEvent` — 클래스 (추가)
    - 설명: `GameEvent` 파생. 현재 콤보 값을 담는다.
    - 이유: 점수와 별도로 콤보 UI·연출이 구독할 수 있게 한다.
  - 심볼: `ComboChangedEvent.Combo` — 프로퍼티 (추가)
    - 설명: Raise 시점의 콤보 정수.
    - 이유: 구독자가 표시·연출에 쓸 payload.
  - 심볼: `ComboChangedEvent.Init(int combo)` — 메서드 (추가)
    - 설명: `Combo`를 설정하고 `this`를 반환한다.
    - 이유: EventChannelSO 재사용 인스턴스 Init 패턴. `new` 금지.
- 파일: `Assets/_MemberWorkspace/JTH/Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.RaiseComboChangedIfNeeded` — 메서드 (추가)
    - 설명: `comboBefore != comboAfter`일 때만 `ComboChangedEvent.Init(comboAfter)`를 Raise한다.
    - 이유: 값이 안 바뀐 배치(클리어 없음)에서 불필요한 이벤트를 막는다.
  - 심볼: `BoardPlacementBootstrap.TryConfirmPlacement` — 메서드 (수정)
    - 설명: `ApplyPlacementScore` 전 `comboBefore`를 잡고, `ScoreChanged` 직후 `RaiseComboChangedIfNeeded`를 호출한다.
    - 이유: 클리어로 콤보가 오른 뒤 HUD가 즉시 갱신되도록.
    - 영향: 콤보 UI가 `ComboChangedEvent`를 구독하면 배치 후 갱신 가능.
  - 심볼: `BoardPlacementBootstrap.OnTurnEnded` — 메서드 (수정)
    - 설명: `NotifyTurnEnded` 전후 콤보를 비교해 바뀌면 Raise한다.
    - 이유: 턴 중 클리어 없을 때 콤보 0 리셋을 UI에 알린다.

**메모** — 웨이브마다 개별 Raise하지 않음. 배치 1회 반영 후 최종 콤보만 쏨 (`ScoreChanged`와 동일 단위).
---
