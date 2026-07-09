# Sequence — Phase 2 (block-placement)

> **Phase:** [phase2.md](phase2.md) 와 1:1.

## 1 — 2026-07-09 · 보드 상태 부트스트랩·블록 선택·스테이징 뷰

**바뀐 것**

- 생성: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
- 생성: `Scripts/Input/BlockSelectionInput.cs`
- 생성: `Scripts/Presentation/BlockPieceView.cs`
- 생성: `Scripts/Data/PlacementConfigSO.cs`
- 생성: `ScriptableObjects/DefaultPlacementConfig.asset`
- 수정: `Scripts/Events/MagnetGameEvents.cs` — `BlockSelectedEvent` 추가
- 수정: `Scenes/Phase0_Bootstrap.unity` — `BoardPlacementBootstrap`, `StagingBlock`, `BlockSelectionInput` GO 추가·배선

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `PlacementConfigSO` (추가)
    - 설명: 스테이징 Y 오프셋·피스 색·칸 fill 비율을 에디터에서 조정하는 SO.
    - 이유: 스테이징 위치·연출 색을 코드 상수 대신 데이터로 분리.
  - 심볼: `GetStagingY(int cellsPerSide)` (추가)
    - 설명: `-(cellsPerSide + stagingYExtraBelow)` 반환. 기본 extra=1 → N=9일 때 `y=-5`.
    - 이유: Phase 1 Domain 전제(보드 아래 Y≠0 스테이징)와 `BoardConfigSO.CellsPerSide`를 연결.
    - 영향: `BoardPlacementBootstrap.Awake`가 초기 pivot Y 계산에 사용.

- 파일: `ScriptableObjects/DefaultPlacementConfig.asset`
  - 심볼: 기본 에셋 (추가)
    - 설명: `stagingYExtraBelow=1`, 피스 색·fill 기본값.
    - 이유: 씬·뷰에서 `[SerializeField]`로 연결할 프로토타입 설정.

- 파일: `Scripts/Events/MagnetGameEvents.cs`
  - 심볼: `MagnetGameEvents.BlockSelectedEvent` — static 필드 (추가)
    - 설명: 슬롯 선택 이벤트 재사용 인스턴스.
    - 이유: `EventChannelSO` + `Init()` 패턴 준수 (`new` 금지).
  - 심볼: `BlockSelectedEvent` (추가)
    - 설명: `SlotIndex`, `IBlockShape Shape` 페이로드.
    - 이유: UI 없이 키 입력 → Bootstrap/뷰가 선택 결과를 수신.
    - 영향: `BlockSelectionInput` Raise, `BoardPlacementBootstrap` 구독.

- 파일: `Scripts/Input/BlockSelectionInput.cs`
  - 심볼: `BlockSelectionInput` (추가)
    - 설명: `BlockCandidatesUpdatedEvent`로 후보 스냅샷 유지. `Update`에서 1/2/3 키 감지.
    - 이유: M7 후보 UI와 분리된 최소 프로토타입 입력.
  - 심볼: `OnCandidatesUpdated` (추가)
    - 설명: 이벤트의 `Candidates`를 `_candidates`에 저장.
    - 이유: 키 입력 시점에 유효 슬롯·형태를 조회.
  - 심볼: `TrySelect(int slotIndex)` (추가)
    - 설명: null·범위 검사 후 `BlockSelectedEvent.Init(slotIndex, shape)` Raise.
    - 이유: 잘못된 슬롯·빈 후보에서 이벤트 남발 방지.

- 파일: `Scripts/Presentation/BlockPieceView.cs`
  - 심볼: `BlockPieceView` (추가)
    - 설명: `IBlockShape` offsets + pivot → 칸마다 `SpriteRenderer` 쿼드 배치.
    - 이유: 스테이징·이후 부착 피스 표시의 공통 Presentation 컴포넌트(Phase 4 재사용 예정).
  - 심볼: `Show(IBlockShape shape, Vector2Int pivot)` (추가)
    - 설명: `BoardCoordinates.GridToWorld`로 월드 위치 계산, 필요 칸 수만큼 렌더러 풀링.
    - 이유: 형태마다 칸 수가 달라 동적 생성·재사용.
  - 심볼: `Clear()` (추가)
    - 설명: 모든 칸 렌더러 비활성화.
    - 이유: 선택 해제·부착 후 스테이징 비우기(Phase 4).

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap` (추가)
    - 설명: 런타임 `BoardSession`·`BlockPlacementService` 생성. 선택 이벤트 → 스테이징 뷰 연결.
    - 이유: Domain(Phase 1)과 입력·뷰 사이 orchestration 계층.
  - 심볼: `Awake` — `_session`, `_placementService`, `_stagingPivot` (추가)
    - 설명: `boardConfig.BoardSize`로 세션 생성. staging pivot `(0, GetStagingY(...))`.
    - 이유: Phase 3~4가 동일 세션·서비스 인스턴스를 참조.
  - 심볼: `Session` / `PlacementService` / `BlockSpawn` (추가)
    - 설명: Phase 3 입력·Phase 4 Consume 연동용 public 접근자.
    - 이유: Reflex 없이 `[SerializeField]` Bootstrap 간 참조.
  - 심볼: `OnBlockSelected` (추가)
    - 설명: 선택 슬롯·형태 저장 → `stagingBlockView.Show(shape, stagingPivot)`.
    - 이유: 키 선택 즉시 보드 아래에 피스 표시.

- 파일: `Scenes/Phase0_Bootstrap.unity`
  - 심볼: `BoardPlacementBootstrap` GO + 컴포넌트 (추가)
    - 설명: `magnetGameChannel`, `DefaultBoardConfig`, `DefaultPlacementConfig`, `BlockSpawnBootstrap`, `StagingBlock` BlockPieceView 연결.
  - 심볼: `StagingBlock` GO + `BlockPieceView` (추가)
    - 설명: 스테이징 피스 표시 전용 오브젝트.
  - 심볼: `BlockSelectionInput` GO + 컴포넌트 (추가)
    - 설명: `magnetGameChannel` 연결.

**검증**

- `read_console` Error 0 (Unity 리컴파일 후).
- Play 후 1/2/3 키 → `[BlockSelection]`·`[BoardPlacement] Staging slot` 로그 + 보드 아래(`y=-5`) 피스 표시.
- 드래그·부착 없이 `BoardGrid` 점유 변화 없음.

**메모**

- `BlockPieceView`는 pivot 기준 **절대 격자 좌표**를 월드로 변환 — GO가 `(0,0,0)`에 있어야 BoardView와 정렬.
- `Consume`·`TryPlace`는 Phase 4. Phase 2는 선택·표시만.

---

## 2 — 2026-07-09 · InputSO 패턴 전환

**바뀐 것**

- 생성: `Input/MagnetPlacement.inputactions`
- 생성: `Scripts/Input/MagnetPlacementControls.cs`
- 생성: `Scripts/Data/MagnetInputSO.cs`
- 생성: `ScriptableObjects/DefaultMagnetInput.asset`
- 수정: `Scripts/Input/BlockSelectionInput.cs` — `Update`/`GetKeyDown` 제거, `MagnetInputSO` 구독
- 수정: `Scripts/Magnet.JTH.asmdef` — Unity Input System 참조
- 수정: `Scenes/Phase0_Bootstrap.unity` — `BlockSelectionInput.magnetInput` 배선
- 수정: `FOLDER_STRUCTURE.md` — §입력 (InputSO) 규칙 추가

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Data/MagnetInputSO.cs`
  - 심볼: `MagnetInputSO` (추가)
    - 설명: `MagnetPlacementControls.IMagnetActions` 구현 SO. `OnEnable`에서 map Enable·SetCallbacks.
    - 이유: 팀 InputSO 패턴(콜백·event는 SO, MonoBehaviour는 구독만) 준수.
  - 심볼: `OnSlotSelected` / `OnPointerChange` (추가)
    - 설명: 슬롯 0~2 선택·포인터 스크린 좌표 C# event.
    - 영향: `BlockSelectionInput`, Phase 3 `BlockDragInput` 예정.
  - 심볼: `GetWorldPointerPosition()` (추가)
    - 설명: Orthographic 카메라 기준 스크린→월드 (Phase 3 x축 드래그용).
    - 이유: 포인터→격자 변환을 입력 계층 한곳에 모음.

- 파일: `Scripts/Input/MagnetPlacementControls.cs`
  - 심볼: `MagnetPlacementControls` / `IMagnetActions` (추가)
    - 설명: `MagnetPlacement.inputactions` JSON 래퍼. SelectSlot1~3, Pointer.
    - 이유: Input System 생성 클래스와 동일 역할.

- 파일: `Scripts/Input/BlockSelectionInput.cs`
  - 심볼: `Update` + `Input.GetKeyDown` (삭제)
  - 심볼: `magnetInput.OnSlotSelected` 구독 (추가)
    - 설명: SO event → `TrySelect` → `BlockSelectedEvent` Raise.
    - 이유: 레거시 Input API 금지, InputSO 단일 진입점.

**검증**

- `read_console` Error 0.
- Play 후 1/2/3 — Phase 2와 동일 동작.

**메모**

---

## 3 — 2026-07-09 · Controls.inputactions 기반으로 Input 정리

**바뀬 것**

- 수정: `Scripts/Input/Controls.inputactions` — SelectSlot1~3, Pointer 액션·바인딩 추가 (Test 제거)
- 수정: `Scripts/Input/Controls.cs` — Unity Input System 재생성
- 수정: `Scripts/Data/MagnetInputSO.cs` — `new Controls()` + `Player.SetCallbacks(this)` (수동 래퍼 제거)
- 삭제: `Input/MagnetPlacement.inputactions` (+ meta)

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Data/MagnetInputSO.cs`
  - 심볼: `_controls = new MagnetPlacementControls()` (삭제)
  - 심볼: `_controls = new Controls(); _controls.Player.SetCallbacks(this);` (추가)
    - 이유: 팀 InputSO 패턴 — Unity 생성 `Controls` + `IPlayerActions` 콜백.
  - 심볼: `OnTest` 스텁 (삭제)

- 파일: `Scripts/Input/Controls.inputactions`
  - 심볼: Player/SelectSlot1~3, Pointer (추가)
    - 설명: 1/2/3·키패드·마우스/터치 포인터 바인딩.
    - 영향: `Controls.cs` 재생성 → `MagnetInputSO`가 구현하는 인터페이스와 일치.

**검증**

- `read_console` Error 0 (Input System asmdef 참조 포함).

**메모**

- `Controls.cs`는 **수동 편집 금지** — `.inputactions` 수정 후 Unity가 재생성.

---
