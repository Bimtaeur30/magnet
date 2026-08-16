# line-clear-preview-fx Sequence 3

> Phase 2 구현 기록 · 스킨 클립 힌트 + 클리어 이펙트

## 1 — 2026-08-16 · 스냅 클립 루프와 클리어 이펙트

## 변경 상세

- 파일: `_Shared/Magnet.Core/SO/Skin/SkinDataSO.cs`
  - 심볼: `SkinDataSO.HintClips` — 프로퍼티 (추가)
    - 설명: 바리에이션 인덱스에 대응하는 클리어 예고 `AnimationClip` 배열을 보관한다. 슬롯이 비면 그 바리에이션은 클립을 재생하지 않는다.
    - 이유: 알파 숨쉬기 대신 스킨 에셋에 넣은 클립으로 스냅 힌트를 돌리기 위해.
  - 심볼: `SkinDataSO.LineClearEffects` — 프로퍼티 (추가)
    - 설명: 바리에이션 인덱스에 대응하는 칸 이펙트 `PoolItemSO` 배열을 보관한다. 슬롯이 비면 그 바리에이션은 칸 이펙트를 쏘지 않는다.
    - 이유: 실제 클리어 때 스킨 바리에이션에 맞는 이펙트를 쓰기 위해.
  - 심볼: `SkinDataSO.FireCenteredLineClear` — 프로퍼티 (추가)
    - 설명: 켜면 클리어 시 칸마다 이펙트를 쏘지 않고 줄 가운데에 길쭉한 이펙트 1발을 쓴다.
    - 이유: 스킨마다 칸 스파크 / 줄 스트립 연출을 고를 수 있게.
  - 심볼: `SkinDataSO.CenterLineClearEffect` — 프로퍼티 (추가)
    - 설명: 가운데 1발 모드에서 쓸 길쭉한 이펙트 프리팹 풀 아이템을 보관한다.
    - 이유: 칸 이펙트와 다른 에셋이므로 별도 슬롯이 필요해서.
  - 심볼: `SkinDataSO.ResolveVariationIndex(int skinId)` — 메서드 (추가)
    - 설명: `Sprites.Length`로 `skinId`를 양수 나머지로 정규화한다. 스프라이트가 없으면 0을 반환한다.
    - 이유: 클립·이펙트 조회가 스프라이트 바리에이션과 같은 인덱스를 쓰게.
  - 심볼: `SkinDataSO.GetSprite(int skinId)` — 메서드 (추가)
    - 설명: 정규화된 인덱스의 스프라이트를 반환한다. 배열이 비면 null.
    - 이유: `% Length` 반복을 호출부에서 제거.
  - 심볼: `SkinDataSO.GetHintClip(int skinId)` — 메서드 (추가)
    - 설명: 같은 바리에이션 인덱스의 힌트 클립을 반환한다. 배열이 짧거나 슬롯이 비면 null.
    - 이유: 클립이 없는 바리에이션은 스프라이트 통일만 하고 애니메이션을 건너뛰기 위해.
  - 심볼: `SkinDataSO.GetLineClearEffect(int skinId)` — 메서드 (추가)
    - 설명: 같은 바리에이션 인덱스의 칸 이펙트를 반환한다. 없으면 null.
    - 이유: 클리어 FX가 스킨 배열 길이를 직접 다루지 않게.
  - 심볼: `SkinDataSO.GetVariation<T>(T[] items, int skinId)` — 메서드 (추가)
    - 설명: 스프라이트 인덱스로 병렬 배열을 읽고, 인덱스가 배열 밖이면 null을 반환한다. 병렬 배열을 순환하지 않는다.
    - 이유: 클립/이펙트가 스프라이트보다 짧을 때 잘못된 슬롯을 쓰지 않게.

- 파일: `Scripts/Domain/Skin/InGameSkinManager.cs`
  - 심볼: `InGameSkinManager._currentSkin` — 필드 (수정)
    - 설명: `Sprite[]` 대신 `SkinDataSO`를 보관한다.
    - 이유: 힌트/클리어가 같은 스킨 에셋의 클립·이펙트를 쓰므로 스프라이트 배열만으로는 부족해서.
  - 심볼: `InGameSkinManager.BlockCreatedHandler(...)` — 메서드 (수정)
    - 설명: `_currentSkin.GetSprite(evt.SkinId)`로 스킨을 입힌다. 스킨 초기화 전이면 Apply를 건너뛴다.
    - 이유: 초기화 전 `% Length` NRE를 막고, 인덱스 규칙을 SO 헬퍼로 통일.
  - 심볼: `InGameSkinManager.SkinChangedHandler(...)` / `SkinInitializedHandler(...)` — 메서드 (수정)
    - 설명: 이벤트에서 `SkinDataSO` 전체를 `_currentSkin`에 넣는다.
    - 이유: Sprites만이 아니라 클립·이펙트 설정의 출처가 같아야 해서.
  - 심볼: `InGameSkinManager.ApplySkin()` — 메서드 (수정)
    - 설명: `_currentSkin`이 있을 때만 등록된 블록에 `GetSprite`를 적용한다.
    - 이유: null 스킨에서 크래시하지 않게.

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block.hintAnimator` — 필드 (추가)
    - 설명: Skin 자식의 Animator. Playable 그래프의 출력 타깃이다. 비어 있으면 Awake에서 skinRenderer와 같은 GO에서 찾는다.
    - 이유: 스킨 클립을 레거시 Animation이 아니라 Mecanim/Playable로 재생하기 위해.
  - 심볼: `Block._placedSprite` / `_hintSprite` / `_hintActive` / `_playingClip` / `_hintGraph` / `_hintPlayable` — 필드 (추가)
    - 설명: 원래 스프라이트, 스냅 통일 스프라이트, 힌트 활성, 재생 중인 클립과 Playable 그래프를 보관한다.
    - 이유: 스냅 해제 시 원복하고, 같은 클립을 매 프레임 재시작하지 않기 위해.
  - 심볼: `Block._clearHint` / `_clearHintTime` / `_hintBrightMin` 등 펄스 필드 — 필드 (삭제)
    - 설명: 알파·밝기 sine 숨쉬기 상태를 제거한다.
    - 이유: 힌트를 클립 루프로 바꾸므로 펄스 경로가 필요 없어서.
  - 심볼: `Block.ApplySkin(...)` — 메서드 (수정)
    - 설명: `_placedSprite`를 갱신하고, 힌트 중이면 통일 스프라이트를 유지한 채 원본만 바꿔 둔다.
    - 이유: 스냅 중 스킨 변경 이벤트가 와도 힌트 비주얼을 덮어쓰지 않게.
  - 심볼: `Block.SetClearHint(bool, Sprite, AnimationClip)` — 메서드 (수정)
    - 설명: true면 통일 스프라이트를 적용하고 클립을 루프 재생한다. false면 클립을 멈추고 `_placedSprite`로 되돌린다. 같은 스프라이트·클립이면 재시작하지 않는다. 클립이 null이면 스프라이트만 바꾼다.
    - 이유: 스냅 힌트를 스킨 클립 기반으로 바꾸고, 드래그 중 SetHints 반복 호출에 클립이 끊기지 않게.
    - 영향: `LineClearHintEffector`만 호출.
  - 심볼: `Block.PlayHintClip(...)` / `StopHintClip()` / `LoopHintClipIfNeeded()` / `ClearHint()` / `ApplySprite(...)` — 메서드 (추가)
    - 설명: PlayableGraph로 클립을 재생·정지하고, `isLooping`이 꺼진 클립도 길이마다 시간을 되감아 루프한다. 정지 시 그래프를 Destroy한다.
    - 이유: 스냅이 유지되는 동안 힌트가 멈추지 않게, 풀 반환 시 그래프 누수를 막기 위해.
  - 심볼: `Block.Update()` — 메서드 (수정)
    - 설명: 펄스 알파 갱신 대신 `LoopHintClipIfNeeded`만 호출한다.
    - 이유: 숨쉬기 제거 후에도 비루프 클립을 강제로 루프해야 해서.
  - 심볼: `Block.OnDisable()` / `OnDestroy()` — 메서드 (추가)
    - 설명: 비활성·파괴 시 Playable 그래프를 정리한다.
    - 이유: 풀 Push로 꺼진 블록에 그래프가 남지 않게.
  - 심볼: `Block.ResetItem()` — 메서드 (수정)
    - 설명: `SetClearHint(false)`로 힌트를 끄고 `_placedSprite`를 비운다. 펄스 관련 Refresh는 없어졌다.
    - 이유: 풀에서 나올 때 이전 힌트 스프라이트·클립이 남지 않게.
  - 심볼: `Block.RefreshColor()` — 메서드 (수정)
    - 설명: dim·알파만 적용하고 힌트 밝기 배율을 계산하지 않는다.
    - 이유: 펄스 힌트가 사라져서.

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.skinChannel` — 필드 (추가)
    - 설명: 장착 스킨 SO를 받는 채널.
    - 이유: 런타임 Instantiate된 View라 Inject를 쓸 수 없어 이벤트로 스킨을 캐시하기 위해.
  - 심볼: `LineClearHintEffector._currentSkin` / `_appliedSkinId` — 필드 (추가)
    - 설명: 현재 스킨 에셋과, 이번 스냅에 적용 중인 프리뷰 `SkinId`를 보관한다.
    - 이유: `SkinId`가 바뀌면 힌트를 전부 갈아끼우고, 같으면 칸 집합만 증감하기 위해.
  - 심볼: `LineClearHintEffector.SetHints(clearedCells, previewBlocks, skinId)` — 메서드 (수정)
    - 설명: 클리어 칸의 보드 블록과 인자로 받은 프리뷰 블록에 프리뷰 `SkinId`의 스프라이트·클립을 `SetClearHint(true)`로 건다. 스킨이 없거나 칸이 없으면 Clear.
    - 이유: 스테이징은 호출부가 넘기지 않으므로 영향 없고, 반투명 프리뷰와 보드 칸만 통일하기 위해.
  - 심볼: `LineClearHintEffector.ClearHints()` — 메서드 (수정)
    - 설명: 힌트 블록 전부 `SetClearHint(false)` 후 `_appliedSkinId`를 리셋한다.
    - 이유: 스냅 해제 시 보드 칸 스프라이트·클립을 원복하기 위해.
  - 심볼: `LineClearHintEffector.OnEnable()` / `OnDisable()` / `OnSkinChanged` / `OnSkinInitialized` — 메서드 (추가)
    - 설명: 스킨 채널을 구독해 `_currentSkin`을 갱신하고, 비활성 시 힌트를 정리한다.
    - 이유: Effector가 스킨 SO를 직접 들고 있지 않게.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.SetLineClearHints(...)` — 메서드 (수정)
    - 설명: `LineClearPreviewConfigSO` 대신 프리뷰 블록 리스트와 `skinId`를 Effector에 넘긴다.
    - 이유: 펄스 Config가 더 이상 힌트 입력이 아니라서.

- 파일: `Scripts/Presentation/GameBoard.cs`
  - 심볼: `GameBoard.SetLineClearHints(...)` — 메서드 (수정)
    - 설명: 프리뷰 블록과 `skinId`를 View로 위임한다.
    - 이유: Input은 GameBoard만 Inject하므로 시그니처를 여기서 맞춘다.
  - 심볼: `GameBoard.GridToWorldCenter(Vector2Int grid)` — 메서드 (추가)
    - 설명: 칸 원점 `GridToWorld`에 칸 크기 절반을 더해 셀 중심 월드를 반환한다.
    - 이유: 클리어 이펙트를 칸 코너가 아니라 블록 시각 중심에 놓기 위해.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `BlockDragInput.OnPointerReleased()` — 메서드 (수정)
    - 설명: `PlaceBlock`에 `_selectedBlockData.SkinId`를 넘긴다.
    - 이유: 배치 후 클리어 FX가 놓은 피스 바리에이션을 쓰게.
  - 심볼: `BlockDragInput.UpdateLineClearHints(...)` — 메서드 (수정)
    - 설명: `LineClearPreviewConfigSO` 게이트를 제거하고, 클리어 칸에 올라온 프리뷰 블록만 모아 `SetLineClearHints`에 넘긴다.
    - 이유: 힌트는 스킨 클립 기반이고, 스테이징이 아닌 반투명 프리뷰만 영향 받게.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.PlaceBlock(...)` — 메서드 (수정)
    - 설명: `skinId` 인자를 받아 `PlacementResult`에 넣는다.
    - 이유: 클리어 연출이 놓은 피스 바리에이션을 이벤트만으로 알게.

- 파일: `Scripts/Domain/Placement/PlacementResult.cs`
  - 심볼: `PlacementResult.SkinId` — 프로퍼티 (추가)
    - 설명: 이번에 놓은 피스의 스킨 바리에이션 id.
    - 이유: `BlockPlacedEvent` 소비자가 클리어 FX에 쓸 통일 id를 받게.
  - 심볼: `PlacementResult` 생성자 — 생성자 (수정)
    - 설명: `skinId`를 받아 프로퍼티에 저장한다.
    - 이유: 기존 결과 객체에 배치 스킨을 실어 나르기 위해.

- 파일: `Scripts/Presentation/LineClearExplosionPresenter.cs`
  - 심볼: `LineClearExplosionPresenter.inGameChannel` / `presentationChannel` / `skinChannel` — 필드 (추가)
    - 설명: 배치 이벤트, 파티클 Raise, 장착 스킨 캐시용 채널.
    - 이유: SO는 SerializeField, 파티클은 기존 PresentationChannel 계약.
  - 심볼: `LineClearExplosionPresenter._gameBoard` — 필드 (추가)
    - 설명: 칸·줄 중심 월드 변환용 GameBoard. 씬 객체라 Inject.
    - 이유: Input과 같은 GO에서 보드 좌표를 얻기 위해.
  - 심볼: `LineClearExplosionPresenter._currentSkin` — 필드 (추가)
    - 설명: 장착 중인 `SkinDataSO`.
    - 이유: 클리어 때 bool·이펙트 슬롯을 읽기 위해.
  - 심볼: `LineClearExplosionPresenter.OnBlockPlaced(...)` — 메서드 (추가)
    - 설명: 클리어가 있으면 가운데 모드면 줄마다 1발, 아니면 통일 `SkinId` 칸 이펙트를 모든 클리어 칸에 동시에 쏜다.
    - 이유: 이펙트는 실제 터질 때만, 배치 피스 id로 통일 유지.
  - 심볼: `LineClearExplosionPresenter.PlayCellEffects(...)` — 메서드 (추가)
    - 설명: `GetLineClearEffect(skinId)`를 교차 칸 중복 없이 각 셀 중심에 identity 회전으로 Raise한다. 이펙트가 없으면 return.
    - 이유: 가로+세로 교차점에서 이펙트가 두 번 나가지 않게.
  - 심볼: `LineClearExplosionPresenter.PlayCenteredEffects(...)` — 메서드 (추가)
    - 설명: `CenterLineClearEffect`를 줄마다 중심에 쏜다. 가로는 0°, 세로는 Z 90°.
    - 이유: 길쭉한 이펙트가 줄 방향과 맞게.
  - 심볼: `LineClearExplosionPresenter.ResolveLineCenter(...)` — 메서드 (추가)
    - 설명: 줄의 첫 칸·끝 칸 중심의 중점을 반환한다.
    - 이유: 8칸 줄의 기하 중심에 1발 놓기 위해.
  - 심볼: `LineClearExplosionPresenter.PlayEffect(...)` — 메서드 (추가)
    - 설명: `PlayParticleEffectEvent.Init`을 PresentationChannel에 Raise한다.
    - 이유: PTY `ParticleEffectManager`가 풀에서 재생하는 기존 경로를 재사용.

## 에셋

- `Prefabs/Instantiate/Block.prefab` — Skin 자식에 Animator, `hintAnimator` 배선
- `Prefabs/Board/Placed Blocks View.prefab` — `LineClearHintEffector.skinChannel`
- `Prefabs/Input.prefab` — `LineClearExplosionPresenter` + inGame/presentation/skin 채널
