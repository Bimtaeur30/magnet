# Sequence — Phase 3 (random-block-spawn)

> **Phase:** [phase3.md](phase3.md) 와 1:1.

## 1 — 2026-07-07 · 3후보 공급 (`BlockSupply`)

**바뀐 것**

- 생성: `Scripts/Domain/Spawn/BlockSupply.cs`
- 생성: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
- 수정: `Scripts/Events/MagnetGameEvents.cs`
- 수정: `Scenes/Phase0_Bootstrap.unity` — `BlockSpawnBootstrap` GameObject 추가

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Spawn/BlockSupply.cs`
  - 심볼: `BlockSupply.SlotCount` — 상수 `3` (추가)
    - 설명: 하단 후보 슬롯 개수를 한곳에 고정한다.
    - 이유: `DESIGN.md` §4.2 「항상 3개 후보」 규칙을 매직 넘버 없이 표현.
  - 심볼: `BlockSupply._drawer` — 필드 `BlockDrawer` (추가)
    - 설명: 슬롯을 채울 때마다 참조하는 추첨기.
    - 이유: 추첨 로직(Phase 2)을 재사용하고 공급기는 슬롯 상태만 담당(SRP).
  - 심볼: `BlockSupply._slots` — 필드 `IBlockShape[3]` (추가)
    - 설명: 현재 3슬롯 후보 형태를 보관하는 배열.
    - 이유: `Fill`/`Consume` 후보 상태의 단일 소스.
  - 심볼: `BlockSupply(BlockDrawer drawer)` — 생성자 (추가)
    - 설명: 전달받은 `BlockDrawer`를 `_drawer`에 저장한다.
    - 이유: 생성자 DI로 추첨기 주입(전역·싱글톤 회피).
  - 심볼: `BlockSupply.Candidates` — 프로퍼티 `IReadOnlyList<IBlockShape>` (추가)
    - 설명: `_slots` 배열을 읽기 전용 목록으로 노출한다.
    - 이유: 외부(UI·배치)가 현재 후보 3개를 조회.
  - 심볼: `BlockSupply.GetCandidate(int slotIndex)` — 메서드 (추가)
    - 설명: 지정 슬롯 인덱스의 `IBlockShape`를 반환한다.
    - 이유: 단일 슬롯 조회(선택·배치)용 명시 API.
  - 심볼: `BlockSupply.Fill()` — 메서드 (추가)
    - 설명: 0~2 슬롯을 각각 `_drawer.Draw()`로 채운다.
    - 이유: 게임 시작·전체 리필 시 3후보 초기화.
  - 심볼: `BlockSupply.Consume(int slotIndex)` — 메서드 (추가)
    - 설명: `slotIndex` 슬롯만 `_drawer.Draw()`로 교체한다.
    - 이유: `DESIGN.md` 「1개 사용 시 해당 슬롯만 새 블록으로 교체」.
  - 심볼: `BlockSupply.CreateSnapshot()` — 메서드 (추가)
    - 설명: `_slots` 내용을 길이 3인 새 배열로 복사해 반환한다.
    - 이유: 이벤트 방송 시 외부가 슬롯 배열을 변조하지 못하게 스냅샷 전달.
    - 영향: `BlockSpawnBootstrap.RaiseCandidatesUpdated()`가 스냅샷을 이벤트에 넣음.
- 파일: `Scripts/Events/MagnetGameEvents.cs`
  - 심볼: `MagnetGameEvents.BlockCandidatesUpdatedEvent` — static 필드 (추가)
    - 설명: 후보 갱신 이벤트의 재사용 인스턴스(`GameEvents` 패턴).
    - 이유: `RaiseEvent` 시 `new` 금지 규칙 준수.
  - 심볼: `BlockCandidatesUpdatedEvent.Candidates` — 프로퍼티 (추가)
    - 설명: 갱신된 후보 형태 목록(스냅샷)을 담는다.
    - 이유: UI(M7/M9)·기타 구독자가 후보 3개를 수신.
  - 심볼: `BlockCandidatesUpdatedEvent.Init(IReadOnlyList<IBlockShape>)` — 메서드 (추가)
    - 설명: `Candidates`에 스냅샷을 넣고 `this`를 반환한다.
    - 이유: fluent `RaiseEvent(…Init(…))` 패턴.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap._eventChannel` — 필드 `[Inject] EventChannelSO` (추가)
    - 설명: 후보 갱신 이벤트를 Raise할 채널.
    - 이유: Installer `RegisterValue` 경유 주입(DI 예외 패턴).
  - 심볼: `BlockSpawnBootstrap._shapeSource` — 필드 `[Inject] IBlockShapeSource` (추가)
    - 설명: 추첨 풀(`BlockShapeSourceSO`)을 계약 타입으로 주입받는다.
    - 이유: JTH는 PTY 구체 타입 없이 Path A 소비. 직렬화 불가 → Reflex 예외.
  - 심볼: `BlockSpawnBootstrap._supply` — 필드 `BlockSupply` (추가)
    - 설명: 런타임 3슬롯 공급 상태.
    - 이유: `Start`/`Consume`에서 동일 인스턴스 유지.
  - 심볼: `BlockSpawnBootstrap.Supply` — 프로퍼티 (추가)
    - 설명: `_supply`를 외부(SCRUM-19 배치)에 노출한다.
    - 이유: 배치 로직이 후보·소모 API에 접근.
  - 심볼: `BlockSpawnBootstrap.Start()` — 메서드 (추가)
    - 설명: `BlockDrawer`+`BlockSupply` 생성 → `Fill()` → `RaiseCandidatesUpdated()`.
    - 이유: 씬 진입 시 3후보 초기화·첫 이벤트 방송.
  - 심볼: `BlockSpawnBootstrap.Consume(int slotIndex)` — public 메서드 (추가)
    - 설명: `_supply.Consume` 후 `RaiseCandidatesUpdated()`.
    - 이유: 블록 배치 완료 시 슬롯 소모+UI 갱신 이벤트 한곳에서 처리.
    - 영향: SCRUM-19에서 호출 예정.
  - 심볼: `BlockSpawnBootstrap.RaiseCandidatesUpdated()` — private 메서드 (추가)
    - 설명: 스냅샷으로 `BlockCandidatesUpdatedEvent`를 Raise하고 로그 1줄.
    - 이유: 이벤트 방송 책임을 Bootstrap에만 두고 Domain은 이벤트 모름.
- 파일: `Scenes/Phase0_Bootstrap.unity`
  - 심볼: `BlockSpawnBootstrap` GameObject + `BlockSpawnBootstrap` 컴포넌트 (추가)
    - 설명: 씬 루트에 부트스트랩 오브젝트 배치.
    - 이유: `Start`에서 Reflex 주입 후 3후보 초기화가 실행되도록.

**검증**

- `Assets/Refresh` 후 `read_console` 컴파일 에러 0.
- 플레이 시 Console: `[BlockSpawn] BlockCandidatesUpdatedEvent raised (3 slots)` (사용자 플레이 시).

**메모**

- 형태 풀은 PTY `BlockShapeSource.asset`(10종). `PresetShapeSource`는 Phase 3에서 미사용.
- PTY `BlockShapeSourceInstaller`는 이전 항목(Phase 3 착수 전)에서 `RootScope`에 이미 배선됨.

---

## 2 — 2026-07-07 · EventChannelSO Inject 제거·필드명 정리

**바뀐 것**

- 수정: `Scripts/Bootstrap/Phase0Bootstrap.cs`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
- 수정: `Scripts/Bootstrap/MagnetProjectInstaller.cs`
- 수정: `Prefabs/RootScope.prefab`
- 수정: `Scenes/Phase0_Bootstrap.unity`
- 생성: `.cursor/rules/jth-event-channel.mdc`
- 수정: `CLAUDE.md`, `.cursor/rules/jth-csharp.mdc`, `FOLDER_STRUCTURE.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Bootstrap/Phase0Bootstrap.cs`
  - 심볼: `Phase0Bootstrap._eventChannel` — `[Inject] EventChannelSO` 필드 (삭제)
    - 설명: Reflex로 이벤트 채널을 주입받던 필드.
    - 이유: `EventChannelSO`는 SerializeField 대상 — Inject 패턴 제거.
  - 심볼: `Phase0Bootstrap.mainEventChannelSO` — `[SerializeField] EventChannelSO` (추가)
    - 설명: `MainEventChannel.asset`을 Inspector에서 직접 연결하는 채널 필드.
    - 이유: SO는 `[SerializeField]` 규칙 준수. 필드명에 `EventChannelSO`가 드러나게 명명.
  - 심볼: `Phase0Bootstrap.Start()` — `Debug.Assert(mainEventChannelSO != null)` (추가)
    - 설명: 채널 미할당 시 개발 빌드에서 즉시 실패.
    - 이유: `CLAUDE.md` 방어 규칙(미할당 SerializeField).
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap._eventChannel` — `[Inject] EventChannelSO` (삭제)
    - 설명: 동일 — Inject 제거.
    - 이유: EventChannelSO SerializeField 전용.
  - 심볼: `BlockSpawnBootstrap.mainEventChannelSO` — `[SerializeField] EventChannelSO` (추가)
    - 설명: 후보 갱신 Raise에 쓰는 채널.
    - 이유: Phase0Bootstrap과 동일 에셋을 씬에서 직렬화 연결.
  - 심볼: `BlockSpawnBootstrap._shapeSource` → `BlockSpawnBootstrap._blockShapeSource` (이름 변경)
    - 설명: Inject 대상은 `IBlockShapeSource`만 유지, 필드명 명확화.
    - 이유: EventChannel과 혼동 방지.
  - 심볼: `BlockSpawnBootstrap.RaiseCandidatesUpdated()` — `mainEventChannelSO.RaiseEvent(...)` (수정)
    - 설명: Inject 필드 대신 SerializeField 채널로 Raise.
    - 이유: Inject 제거에 따른 호출부 변경.
- 파일: `Scripts/Bootstrap/MagnetProjectInstaller.cs`
  - 심볼: `MagnetProjectInstaller.mainEventChannel` + `RegisterValue` (삭제)
    - 설명: Installer에서 EventChannel 등록하던 코드 제거.
    - 이유: EventChannel은 소비자 SerializeField — Installer 경유 Inject 금지.
- 파일: `Prefabs/RootScope.prefab`, `Scenes/Phase0_Bootstrap.unity`
  - 심볼: Bootstrap 컴포넌트 `mainEventChannelSO` 슬롯 (추가/배선)
    - 설명: `MainEventChannel.asset` 참조를 각 Bootstrap에 연결.
    - 이유: SerializeField 배선.

**검증**

- `read_console` 컴파일 에러 0.

**메모**

- 규칙: `jth-event-channel.mdc` — **모든 SO Inject 금지**, 메인 채널 필드명 `magnetGameChannel`.

---

## 3 — 2026-07-07 · 이벤트 채널 필드명 `magnetGameChannel`

**바뀐 것**

- 수정: `Phase0Bootstrap.cs`, `BlockSpawnBootstrap.cs` — `mainEventChannelSO` → `magnetGameChannel`
- 수정: `Phase0_Bootstrap.unity` — 직렬화 키 `magnetGameChannel`
- 수정: `CLAUDE.md`, `jth-event-channel.mdc`, `jth-csharp.mdc`, `FOLDER_STRUCTURE.md`

**변경 상세 (왜/무엇)**

- 심볼: `magnetGameChannel` — `[SerializeField] EventChannelSO` (이름 확정)
  - 설명: 메인 게임 이벤트 채널 SO 참조 필드.
  - 이유: SO는 전부 SerializeField(`CLAUDE.md`). 필드명은 팀 합의 `magnetGameChannel` — `eventChannel`/`mainEventChannelSO` 등 금지.

---
