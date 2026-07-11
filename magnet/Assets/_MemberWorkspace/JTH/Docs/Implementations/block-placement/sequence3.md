# Sequence — Phase 3 (block-placement)

> **Phase:** [phase3.md](phase3.md) 와 1:1.

## 1 — 2026-07-09 · x축 드래그·감도 램프·Simulate 프리뷰

**바뀐 것**

- 생성: `Scripts/Domain/Placement/DragSensitivityRamp.cs`
- 생성: `Scripts/Input/BlockDragInput.cs`
- 수정: `Scripts/Domain/Placement/BlockPlacementCells.cs` — 중심·pivot 범위·월드↔pivot 변환
- 수정: `Scripts/Data/MagnetInputSO.cs` — `PointerPress`/`PointerRelease` 이벤트
- 수정: `Scripts/Input/Controls.inputactions` — `PointerPress` 액션
- 수정: `Scripts/Input/Controls.cs` — 재생성(수동 동기화)
- 수정: `Scripts/Presentation/BlockPieceView.cs` — `ShowAtWorldCenter`, 색상 오버로드
- 수정: `Scripts/Data/PlacementConfigSO.cs` — 감도 램프·프리뷰 색
- 수정: `ScriptableObjects/DefaultPlacementConfig.asset`
- 수정: `Scripts/Bootstrap/MagnetSceneInstaller.cs` — `previewBlockView`, `boardPlacementBootstrap` 등록
- 수정: `Scenes/Phase0_Bootstrap.unity` — `PreviewBlock`, `BlockDragInput` GO·배선, `BlockSpawnBootstrap` SO 연결
- 수정: `Docs/INSPECTOR_TOOLTIPS.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Placement/DragSensitivityRamp.cs`
  - 심볼: `DragSensitivityRamp` (추가)
    - 설명: Press 시작 포인터 X와의 거리에 따라 포인터 ΔX 배율 증가. `Begin`/`Reset`은 Press/Up 시.
    - 이유: Block Blast식 — 손가락을 많이 움직이지 않아도 블록이 멀리 이동.
  - 심볼: `ApplyPointerDelta` (추가)
    - 설명: `multiplier = min(max, 1 + |currentX - originX| * rampPerUnit)` 후 `pointerDelta * multiplier`.
    - 영향: `BlockDragInput.OnPointerMoved`.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `BlockDragInput` (추가)
    - 설명: `BlockSelectedEvent` 후 **Press+Move**에서만 추적. 키 선택 없이 드래그 무시.
    - 이유: 1/2/3 선택 → 드래그 UX.
  - 심볼: `OnPointerPressed` / `OnPointerMoved` / `OnPointerReleased` (추가)
    - 설명: Press 시 감도·포인터 기준점 Reset. Move 시 즉시 추적(보간 없음)+Simulate 프리뷰. Release 시 Simulate 로그만.
    - 영향: Phase 4 `TryPlace`·`Consume` 연결 예정.

- 파일: `Scripts/Presentation/BlockPieceView.cs`
  - 심볼: `ShowAtWorldCenter` (추가)
    - 설명: 블록 기하 중심 월드 X에 칸을 **즉시** 배치 (LitMotion 없음).
  - 심볼: `Show(..., Color)` (추가)
    - 설명: 프리뷰 고스트 색 분리.

- 파일: `Scripts/Bootstrap/MagnetSceneInstaller.cs`
  - 심볼: `previewBlockView`, `boardPlacementBootstrap` RegisterValue (추가)
    - 이유: 변경된 구조 — 스테이징은 `BlockSpawnBootstrap`, 배치 Domain은 `BoardPlacementBootstrap`, Reflex `[Inject]`.

- 파일: `Scripts/Data/MagnetInputSO.cs`
  - 심볼: `OnPointerPressed` / `OnPointerReleased` (추가)
    - 설명: `PointerPress` Button 콜백.

**검증**

- `read_console` Error 0 (에디터 리컴파일 후).
- Play: 1/2/3 선택 → 스테이징 `(0, stagingY)` 표시.
- Press+드래그: 블록이 포인터를 **즉시** 따라감, 연속 드래그 시 감도 증가.
- Release 후 재 Press: 감도 1부터 재시작.
- 드래그 중 고스트 = `Simulate` 격자 스냅. Release 시 `[BlockDrag] Simulate` 로그. 점유 변화 없음.

**메모**

- 추적(스테이징 피스)과 프리뷰(고스트) 분리: `StagingBlock` / `PreviewBlock`.
- `BoardPlacementBootstrap`은 세션·서비스만; 스테이징 표시는 Phase 2 이후 `BlockSpawnBootstrap` 담당.

---

## 2 — 2026-07-09 · 감도 램프 기준을 Press 원점 거리로 변경

**바뀐 것**

- 수정: `Scripts/Domain/Placement/DragSensitivityRamp.cs`
- 수정: `Scripts/Input/BlockDragInput.cs`
- 수정: `Scripts/Data/PlacementConfigSO.cs` — Tooltip 문구
- 수정: `Docs/INSPECTOR_TOOLTIPS.md`, `phase3.md`

**변경 상세 (왜/무엇)**

- 누적 `pointerDelta` 합산 방식은 같은 Press 안에서 계속 움직일수록 배율이 무한히 커짐.
- `Begin(pressOriginX)` 후 `|currentX - originX|`로 배율 계산 — 원점에서 멀수록 감도 상승, 제자리 왕복 시 배율은 origin 기준 거리에만 의존.

---

## 3 — 2026-07-09 · 선택 즉시 스테이징 표시

**바뀐 것**

- 수정: `Scripts/Input/BlockDragInput.cs` — `OnBlockSelected`에서 `ShowAtWorldCenter` 호출
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs` — 스테이징 뷰 책임 제거

**변경 상세 (왜/무엇)**

- 1/2/3 선택 시 스테이징이 안 보이고 클릭해야만 보이던 문제.
- Phase 3 이후 스테이징 연출은 `BlockDragInput` 단일 소유; 선택 직후 `(0, stagingY)` 기준 즉시 표시.

---

## 4 — 2026-07-10 · Debug.Assert 배치 정리

**바뀐 것**

- 수정: `Scripts/Presentation/BlockPieceView.cs` — SerializeField·prefab 검증을 `Awake` 1회로, `Show`/`ShowAtWorldCenter` 반복 Assert 제거, `Show` 오버로드 복원
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs` — `magnetGameChannel` Assert 중복 제거 (`Awake`만)

**변경 상세 (왜/무엇)**

- Inspector 미할당은 라이프사이클 1회 검증. 런타임 메서드마다 `boardConfig`/`shape` 등 Assert는 불필요.
- `cellPrefab` SpriteRenderer 검증도 `Awake`에서 1회.

---

## 5 — 2026-07-10 · DragSensitivityRamp 생성자화·BlockDragInput Assert 정리

**바뀐 것**

- 수정: `Scripts/Domain/Placement/DragSensitivityRamp.cs`
- 수정: `Scripts/Input/BlockDragInput.cs`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Placement/DragSensitivityRamp.cs`
  - 심볼: `DragSensitivityRamp(float rampPerWorldUnit, float maxMultiplier)` (추가)
    - 설명: `rampPerWorldUnit`·`maxMultiplier`를 생성 시 주입해 필드로 보관.
    - 이유: `ApplyPointerDelta` 호출마다 config 인자를 넘기지 않도록.
  - 심볼: `ApplyPointerDelta(pointerDeltaX, currentPointerWorldX)` (수정)
    - 설명: 생성자에서 받은 `_rampPerWorldUnit`·`_maxMultiplier` 사용. 시그니처 2인자.
    - 영향: `BlockDragInput.OnPointerMoved`.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `Awake` (수정)
    - 설명: `placementConfig`·`boardConfig`만 `Debug.Assert`. `_sensitivityRamp` 생성·`_stagingGridY` 계산을 `Awake` 1회로 이동.
    - 이유: 초기화 시점에 없으면 안 되는 SO만 Assert — 이벤트 구독용 `magnetInput`·`magnetGameChannel`은 Assert 제거 (`OnDisable` null-safe 해제).
  - 심볼: `OnEnable` (수정)
    - 설명: Assert 없이 이벤트 구독만.

**메모**

- Assert 기준: `Awake`/`Start`에서 즉시 역참조하는 SerializeField SO만. 런타임 핸들러·구독 해제는 null 체크로 충분.

---

## 6 — 2026-07-10 · BlockPieceView → ShapeBlock 프리팹 구조 전환

**바뀐 것**

- 생성: `Assets/_Shared/Magnet.Contracts/BlockSkins/IBlockSkin.cs`
- 생성: `Scripts/Presentation/Block.cs`
- 생성: `Scripts/Presentation/ShapeBlock.cs`
- 생성: `Prefabs/Block.prefab` (구 `BlockCell.prefab` 대체)
- 생성: `Prefabs/ShapeBlock.prefab`
- 삭제: `Scripts/Presentation/BlockPieceView.cs`
- 삭제: `Prefabs/BlockCell.prefab`
- 수정: `Scripts/Input/BlockDragInput.cs`
- 수정: `Scenes/Phase0_Bootstrap.unity`
- 수정: `Docs/INSPECTOR_TOOLTIPS.md`

**변경 상세 (왜/무엇)**

- 파일: `Assets/_Shared/Magnet.Contracts/BlockSkins/IBlockSkin.cs`
  - 심볼: `IBlockSkin.SkinId` — 프로퍼티 (추가)
    - 설명: 스킨 식별자.
    - 이유: `IBlockShape`와 동일한 계약 패턴으로 추후 `SkinSO` 교체 가능.
  - 심볼: `IBlockSkin.Colors` — 프로퍼티 (추가)
    - 설명: 블록 피스에 적용 가능한 색상 풀.
    - 이유: 시스템 채널 이벤트로 장착 스킨 전달 시 사용.
  - 심볼: `IBlockSkin.Sprites` — 프로퍼티 (추가)
    - 설명: 블록 피스에 적용 가능한 스프라이트 풀.
    - 이유: 동일.

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block.ApplyVisual(Color, Sprite)` — 메서드 (추가)
    - 설명: `SpriteRenderer`에 색·스프라이트 적용.
    - 이유: 칸 단위 비주얼 캡슐화.
  - 심볼: `Block.SetActive` / `SetLocalPosition` / `SetLocalScale` / `SetSortingOrder` — 메서드 (추가)
    - 설명: Transform·Renderer 상태 변경.
    - 이유: `ShapeBlock`이 `SpriteRenderer` 직접 접근 방지.

- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `ShapeBlock.blockPrefab` — 필드 `Block` (추가)
    - 설명: 칸 1개 프리팹 참조.
    - 이유: `BlockPieceView.cellPrefab` 역할 이전.
  - 심볼: `ShapeBlock.systemChannel` — 필드 `EventChannelSO` (추가)
    - 설명: `_Shared/System Channel` 직렬화만. 리스너 미연결.
    - 이유: 추후 스킨 시스템 이벤트 수신 준비.
  - 심볼: `ShapeBlock.skinColors` / `skinSprites` — 필드 (추가)
    - 설명: 임시 인라인 스킨 풀. 피스당 Color·Sprite 각 1개 랜덤.
    - 이유: `SkinSO`·시스템 이벤트 전까지 로컬 테스트.
  - 심볼: `ShapeBlock.Show` / `ShowAtWorldCenter` / `Clear` — 메서드 (추가)
    - 설명: `BlockPieceView` 동일 API. Block Instantiate·재사용.
    - 영향: `BlockDragInput`.
  - 심볼: `ShapeBlock.ApplySkin(IBlockSkin)` — 메서드 (추가)
    - 설명: 계약 기반 스킨 적용. 피스당 1회 resolve.
    - 이유: 시스템 채널 연동 시 사용.
  - 심볼: `ShapeBlock.ShareSkinWith(ShapeBlock)` — 메서드 (추가)
    - 설명: 스테이징 resolve 스킨을 프리뷰에 복사.
    - 이유: staging·preview 색 불일치 방지.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `shapeBlockPrefab` — 필드 (추가)
    - 설명: ShapeBlock 프리팹. Awake에서 staging·preview 2개 Instantiate.
    - 이유: Inject/씬 GO 배선 대신 프리팹 소유 패턴.
  - 심볼: `_stagingBlockView` / `_previewBlockView` — 필드 (삭제)
    - 설명: `BlockPieceView` 참조 제거.
  - 심볼: `Awake` (수정)
    - 설명: `shapeBlockPrefab` Assert + Instantiate.
    - 영향: 씬에 StagingBlock/PreviewBlock GO 불필요.

**메모**

- `systemChannel`은 Assert만 — 이벤트 구독은 스킨 시스템 Phase에서 추가.
- 드래그 중 `Show` 반복 호출 시 스킨 재랜덤 방지: `_skinResolved` 캐시, `Clear()` 시 리셋.

---
