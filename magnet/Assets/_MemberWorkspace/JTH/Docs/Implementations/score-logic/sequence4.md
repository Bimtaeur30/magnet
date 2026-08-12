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
## 2 — 2026-08-12 · ComboChangedEvent에 터진 위치

**바뀐 것** — 콤보 변경 이벤트에 월드 위치 벡터를 추가. 클리어 칸 중심(없으면 배치 블록 중심)을 Raise한다.

**변경 상세 (왜/무엇)**  
- 파일: `Assets/_Shared/Magnet.Core/Events/MagnetGameEvents.cs`
  - 심볼: `ComboChangedEvent.WorldPosition` — 프로퍼티 (추가)
    - 설명: Raise 시점의 콤보 터진 월드 좌표.
    - 이유: 콤보 UI·연출이 숫자뿐 아니라 발생 위치에서 띄울 수 있게.
  - 심볼: `ComboChangedEvent.Init(int combo, Vector3 worldPosition)` — 메서드 (수정)
    - 설명: `Combo`와 `WorldPosition`을 설정하고 `this`를 반환한다.
    - 이유: 기존 Init 패턴 유지하면서 위치 payload를 계약에 포함.
- 파일: `Assets/_MemberWorkspace/JTH/Scripts/Bootstrap/TurnBootstrap.cs`
  - 심볼: `TurnBootstrap.RaiseComboChangedIfNeeded` — 메서드 (수정)
    - 설명: `PlacementResult`로 위치를 구한 뒤 `Init(comboAfter, worldPosition)`을 Raise한다.
    - 이유: 구독자가 배치/클리어 맥락의 월드 좌표를 받게.
  - 심볼: `TurnBootstrap.ResolveComboWorldPosition` — 메서드 (추가)
    - 설명: 클리어 칸 AABB 월드 중심을 반환하고, 클리어가 없으면 배치 칸 중심으로 폴백한다.
    - 이유: 「콤보 터진 위치」는 클리어 중심이 맞고, 콤보 리셋(무클리어)에도 의미 있는 좌표를 준다.
  - 심볼: `TurnBootstrap.ExpandBounds` — 메서드 (추가)
    - 설명: 그리드 min/max 경계를 한 칸씩 확장한다.
    - 이유: 클리어·배치 경로에서 동일 AABB 계산을 재사용.
---
