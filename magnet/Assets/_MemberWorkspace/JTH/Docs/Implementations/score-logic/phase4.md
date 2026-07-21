# Phase 4 — ComboChangedEvent

## 목표

콤보 값이 바뀔 때 HUD 등이 구독할 수 있도록 `ComboChangedEvent`를 Raise한다.

## 구현 내용

- `MagnetGameEvents.ComboChangedEvent` / `ComboChangedEvent.Init(combo)` 추가 (`ScoreChangedEvent`와 동일 패턴)
- `BoardPlacementBootstrap`
  - 배치 점수 반영 직후: `comboBefore != ComboAfter`이면 Raise
  - `TurnEnded` → `NotifyTurnEnded` 직후: 콤보가 바뀌면(대개 0 리셋) Raise

## 범위 밖

- 콤보 HUD UI 구독·표출 (KTJ/M7 UI)
- Domain `ScoreSession` API 변경

## 코드·에셋 맵

| 파일 | 역할 |
|------|------|
| `Assets/_Shared/Magnet.Core/Events/MagnetGameEvents.cs` | 이벤트 정의 |
| `Assets/_MemberWorkspace/JTH/Scripts/Bootstrap/BoardPlacementBootstrap.cs` | Raise |
