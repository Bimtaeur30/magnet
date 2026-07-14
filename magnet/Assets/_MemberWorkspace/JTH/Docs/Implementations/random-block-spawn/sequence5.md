## 1 — 2026-07-14 · 4슬롯·TurnState·턴 이벤트

**바뀐 것** — `BlockSupply.cs`, `TurnState.cs`(생성), `BlockSpawnBootstrap.cs`, `MagnetGameEvents.cs`, `MagnetInputSO.cs`, `Controls.inputactions`, `Controls.cs`, `Docs/DESIGN.md`, `Docs/TODO.md`

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Spawn/BlockSupply.cs`
  - 심볼: `BlockSupply.SlotCount` — 상수 (수정) `3` → `4`
    - 설명: 후보 슬롯 배열 길이를 4로 둔다.
    - 이유: 핸드 크기 4개·전부 소진 시 턴 종료·리필 규칙.
    - 영향: `Fill`/`Consume`/`CreateSnapshot`/`AreAllSlotsEmpty`·입력 인덱스 상한.
- 파일: `Scripts/Domain/Turn/TurnState.cs`
  - 심볼: `TurnState.TurnIndex` — 프로퍼티 (추가)
    - 설명: 현재 핸드 턴 번호를 보관한다.
    - 이유: 턴이 게임에 존재하되 플레이 수치와 미연결인 최소 상태.
  - 심볼: `TurnState.BeginFirstTurn()` — 메서드 (추가)
    - 설명: `TurnIndex`를 1로 설정한다.
    - 이유: 시작 `Fill` 직후 첫 턴을 연다.
  - 심볼: `TurnState.AdvanceAfterHandExhausted()` — 메서드 (추가)
    - 설명: `TurnIndex`를 1 증가시킨다.
    - 이유: 4슬롯 소진·리필 후 다음 턴으로 진행.
- 파일: `Assets/_Shared/Magnet.Core/Events/MagnetGameEvents.cs`
  - 심볼: `MagnetGameEvents.TurnStartedEvent` — static 필드 (추가)
    - 설명: 재사용 싱글톤 이벤트 인스턴스.
    - 이유: EventChannelSO Raise용.
  - 심볼: `MagnetGameEvents.TurnEndedEvent` — static 필드 (추가)
    - 설명: 재사용 싱글톤 이벤트 인스턴스.
    - 이유: EventChannelSO Raise용.
  - 심볼: `TurnStartedEvent.TurnIndex` — 프로퍼티 (추가)
    - 설명: 시작된 턴 번호.
    - 이유: 구독자(향후)가 턴을 식별.
  - 심볼: `TurnStartedEvent.Init(int)` — 메서드 (추가)
    - 설명: `TurnIndex` 설정 후 this 반환.
    - 이유: `new` 금지·Init 재사용 패턴.
  - 심볼: `TurnEndedEvent.TurnIndex` — 프로퍼티 (추가)
    - 설명: 종료된 턴 번호.
    - 이유: Fill 직전 어떤 턴이 끝났는지 전달.
  - 심볼: `TurnEndedEvent.Init(int)` — 메서드 (추가)
    - 설명: `TurnIndex` 설정 후 this 반환.
    - 이유: Init 재사용 패턴.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap._turnState` — 필드 (추가)
    - 설명: `TurnState` 인스턴스 보관.
    - 이유: Start/Consume에서 동일 턴 상태 유지.
  - 심볼: `BlockSpawnBootstrap.Turn` — 프로퍼티 (추가)
    - 설명: `_turnState` 노출.
    - 이유: 디버그·향후 소비자.
  - 심볼: `BlockSpawnBootstrap.Start()` — 메서드 (수정)
    - 설명: `Fill` → `BeginFirstTurn` → `RaiseTurnStarted` → `RaiseCandidatesUpdated`.
    - 이유: 시작 시 턴 1과 4후보를 함께 연다.
  - 심볼: `BlockSpawnBootstrap.Consume(int)` — 메서드 (수정)
    - 설명: 전부 비면 `RaiseTurnEnded` → `Fill` → `Advance` → `RaiseTurnStarted` 후 후보 이벤트.
    - 이유: 합의된 턴 종료·리필 순서.
  - 심볼: `BlockSpawnBootstrap.RaiseTurnStarted()` — 메서드 (추가)
    - 설명: `TurnStartedEvent.Init(TurnIndex)` Raise.
    - 이유: 채널로 턴 시작 방송.
  - 심볼: `BlockSpawnBootstrap.RaiseTurnEnded()` — 메서드 (추가)
    - 설명: `TurnEndedEvent.Init(TurnIndex)` Raise.
    - 이유: Fill 전 턴 종료 방송.
- 파일: `Scripts/Data/MagnetInputSO.cs`
  - 심볼: `MagnetInputSO.OnSelectSlot` — Digit4/Numpad4 → 인덱스 3 (수정)
    - 설명: 키 4를 네 번째 슬롯으로 매핑.
    - 이유: SlotCount=4 입력 지원.
- 파일: `Scripts/Input/Controls.inputactions` / `Controls.cs`
  - 심볼: SelectSlot 바인딩 `<Keyboard>/4`, `<Keyboard>/numpad4` (추가)
    - 설명: Input System에서 4키·숫자패드4를 SelectSlot에 연결.
    - 이유: MagnetInputSO가 performed 콜백을 받으려면 바인딩 필요.

**메모** — KTJ `BlockSlotContainer`는 후보 4개일 때 슬롯 배열 길이 미만이면 인덱스 예외 가능. UI는 이번 범위 밖.
---
