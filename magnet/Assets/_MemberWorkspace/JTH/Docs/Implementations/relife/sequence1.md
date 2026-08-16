# relife Sequence 1

> Phase 1 구현 기록 · Relife 오퍼·수락 이벤트

## 1 — 2026-08-16 · 게임오버 Easy 패 Relife

## 변경 상세

- 파일: `Scripts/Domain/Turn/RelifeSession.cs`
  - 심볼: `RelifeSession.RelifeSession(int minScore)` — 생성자 (추가)
    - 설명: `minScore`가 음수면 0으로 올려 보관한다.
    - 이유: Inspector에서 잘못된 n이 들어와도 오퍼 조건이 깨지지 않게.
  - 심볼: `RelifeSession.MinScore` — 프로퍼티 (추가)
    - 설명: Relife를 띄울 최소 세션 점수를 반환한다.
    - 이유: 오퍼 판정이 설정값에 묶이게.
  - 심볼: `RelifeSession.Used` — 프로퍼티 (추가)
    - 설명: 이번 세션에서 Relife를 이미 수락했는지 반환한다.
    - 이유: 세션당 1회 제한의 상태.
  - 심볼: `RelifeSession.PendingPieces` — 프로퍼티 (추가)
    - 설명: UI에 보여 주고 수락 시 슬롯에 넣을 Easy 셀 오프셋 3개를 보관한다.
    - 이유: 오퍼와 수락 사이에 같은 패를 유지하기 위해.
  - 심볼: `RelifeSession.CanOffer(int totalScore)` — 메서드 (추가)
    - 설명: 미사용이고 pending이 없고 `totalScore >= MinScore`이면 true.
    - 이유: 게임오버 분기에서 Relife와 즉시 GameOver를 가르기 위해.
  - 심볼: `RelifeSession.Offer(...)` — 메서드 (추가)
    - 설명: 미리 뽑은 오프셋을 pending에 넣는다. Used는 아직 켜지 않는다.
    - 이유: 거절(재시작)이면 세션이 끝나므로, 수락 전에 횟수를 소모하지 않게.
  - 심볼: `RelifeSession.Accept()` — 메서드 (추가)
    - 설명: pending을 꺼내 Used=true로 만들고 반환한다. pending이 없으면 null.
    - 이유: 중복 수락 이벤트가 슬롯을 다시 덮지 않게.

- 파일: `_Shared/Magnet.Core/Events/MagnetGameEvents.cs`
  - 심볼: `MagnetGameEvents.RelifeOfferedEvent` — 필드 (추가)
    - 설명: Relife 오퍼 싱글톤 이벤트를 보관한다.
    - 이유: UI가 미리보기 패를 받기 위해. `new` 이벤트 금지.
  - 심볼: `MagnetGameEvents.RelifeAcceptedEvent` — 필드 (추가)
    - 설명: Relife 수락 싱글톤 이벤트를 보관한다.
    - 이유: UI가 수락만 알리면 게임이 들고 있던 패를 넣게.
  - 심볼: `RelifeOfferedEvent.CellOffsetsList` — 프로퍼티 (추가)
    - 설명: Easy 블록 3개의 셀 오프셋 목록을 담는다.
    - 이유: UI가 “이 블럭들로 시작” 미리보기를 그릴 인수.
  - 심볼: `RelifeOfferedEvent.Init(...)` — 메서드 (추가)
    - 설명: 오프셋 목록을 넣고 같은 인스턴스를 반환한다.
    - 이유: EventChannel `RaiseEvent` 계약.
  - 심볼: `RelifeAcceptedEvent.Init()` — 메서드 (추가)
    - 설명: 페이로드 없이 같은 인스턴스를 반환한다.
    - 이유: 패는 게임이 들고 있으므로 UI는 신호만 보내면 됨.

- 파일: `Scripts/Data/ScoreConfigSO.cs`
  - 심볼: `ScoreConfigSO.RelifeMinScore` — 프로퍼티 (추가)
    - 설명: 게임오버 Relife를 줄 최소 점수 n. 기본 100, 0이면 점수 무관.
    - 이유: 초반 짧은 판에는 오퍼하지 않고 Inspector에서 n을 조절하기 위해.

- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator.SelectEasyHand(...)` — 메서드 (추가)
    - 설명: 일반 cascade를 건너뛰고 Easy 리스트 히트맵(없으면 가중랜덤)으로 손을 고른다.
    - 이유: Relife 미리보기가 Normal/Unique가 아닌 Easy 패여야 해서.

- 파일: `Scripts/Domain/Spawn/AreaBundleDrawer.cs`
  - 심볼: `AreaBundleDrawer.DrawEasy(...)` — 메서드 (추가)
    - 설명: `SelectEasyHand` 결과를 `LastResult`에 넣고 Pieces를 반환한다.
    - 이유: 슬롯 Fill 없이 Easy만 뽑기 위해. 수락 후 LogDeal이 같은 LastResult를 쓰게.

- 파일: `Scripts/Domain/Spawn/BlockSupply.cs`
  - 심볼: `BlockSupply.FillFrom(...)` — 메서드 (추가)
    - 설명: Drawer를 타지 않고 주어진 셀 오프셋 3개에 스킨을 붙여 슬롯을 채운다.
    - 이유: Relife 수락 시 오퍼 때 뽑은 그 패를 그대로 쥐어 주기 위해.

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap.DrawEasy(int currentScore)` — 메서드 (추가)
    - 설명: 현재 보드로 Easy 손을 뽑고 셀 오프셋 복사본을 반환한다. turnIndex는 올리지 않는다.
    - 이유: 오퍼 시점에 슬롯을 바꾸지 않고, UI/pending이 같은 불변 오프셋을 보게.
  - 심볼: `BlockSpawnBootstrap.FillPrepared(...)` — 메서드 (추가)
    - 설명: 준비된 오프셋으로 슬롯을 채우고 `BlockCandidatesUpdatedEvent`를 쏜다.
    - 이유: 수락 후 하단 3슬롯·UI가 Relife 패로 바뀌게.
  - 심볼: `BlockSpawnBootstrap.CopyPieces(...)` — 메서드 (추가)
    - 설명: 피스별 `Vector2Int` 배열을 새로 만들어 복사한다.
    - 이유: 오케스트레이터 내부 리스트를 UI가 바꿔도 pending이 안 바뀌게.

- 파일: `Scripts/Bootstrap/TurnBootstrap.cs`
  - 심볼: `TurnBootstrap._relifeSession` — 필드 (추가)
    - 설명: Relife 사용·pending 상태를 보관한다.
    - 이유: 점수·게임오버 분기가 여기 있어서 오퍼 수명도 같이 둠.
  - 심볼: `TurnBootstrap.Awake()` — 메서드 (수정)
    - 설명: `scoreConfig.RelifeMinScore`로 `RelifeSession`을 만든다.
    - 이유: n을 SO에서 읽기 위해.
  - 심볼: `TurnBootstrap.OnEnable()` / `OnDisable()` — 메서드 (수정)
    - 설명: `RelifeAcceptedEvent` 리스너를 붙이거나 뗀다.
    - 이유: UI 수락 신호를 받기 위해.
  - 심볼: `TurnBootstrap.BlockPlacedHandler(...)` — 메서드 (수정)
    - 설명: 게임오버면 먼저 Relife 오퍼를 시도하고, 못 주면 `RaiseGameOver`한다.
    - 이유: 오퍼 중에는 베스트 저장용 `GameOverEvent`를 아직 안 쏘기 위해.
  - 심볼: `TurnBootstrap.TryOfferRelife()` — 메서드 (추가)
    - 설명: 가능하면 Easy를 뽑아 pending에 넣고 `RelifeOfferedEvent`를 쏜다.
    - 이유: 게임오버 즉시 Easy 미리보기를 UI에 넘기기 위해.
  - 심볼: `TurnBootstrap.RelifeAcceptedHandler(...)` — 메서드 (추가)
    - 설명: pending을 꺼내 `FillPrepared`로 슬롯에 넣는다. pending 없으면 무시.
    - 이유: 수락 시 미리 뽑은 패로 이어하기 위해.
  - 심볼: `TurnBootstrap.RaiseGameOver()` — 메서드 (추가)
    - 설명: 스킨 해금 체크와 `GameOverEvent`를 발행한다. Relife를 못 줄 때만 호출한다.
    - 이유: 오퍼 중에는 게임오버를 미루고, 불가 시에만 기존 세이브·UI 리스너가 동작하게.
