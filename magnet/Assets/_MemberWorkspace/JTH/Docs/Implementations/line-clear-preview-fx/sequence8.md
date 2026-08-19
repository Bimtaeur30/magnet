# line-clear-preview-fx Sequence 8

> Phase 7 구현 기록 · 물방울 스킨 (랜덤 스프라이트 + 외곽선 일렁임, 파티클 보류)

## 1 — 2026-08-18 · RandomizeSprites + outlineWave + WaterDrop

## 변경 상세

- 파일: `_Shared/Magnet.Core/SO/Skin/SkinDataSO.cs`
  - 심볼: `SkinDataSO.RandomizeSprites` — 프로퍼티 `bool` (추가)
    - 설명: 켜면 `GetSprite`에 넘길 인덱스를 색 id가 아니라 `Sprites` 길이 난수로 고른다.
    - 이유: 물방울 3장은 색 슬롯이 아니라 같은 스킨의 변형이라 `skinId % length`로 고정하면 안 되어서.
  - 심볼: `SkinDataSO.PickVisualIndex(int skinId)` — 메서드 (추가)
    - 설명: `RandomizeSprites`면 `Random.Range(0, Sprites.Length)`를 반환하고, 꺼져 있으면 `ResolveVariationIndex(skinId)`를 반환한다.
    - 이유: 추첨과 색 인덱스 규칙을 호출부가 직접 나누지 않게.
    - 영향: `InGameSkinManager.ResolveVisualIndex`만 호출한다.

- 파일: `Scripts/Domain/Skin/InGameSkinManager.cs`
  - 심볼: `InGameSkinManager._visualIndex` — 필드 `Dictionary<Block, int>` (추가)
    - 설명: 랜덤 스킨에서 칸마다 한 번 고른 스프라이트 인덱스를 보관한다.
    - 이유: 매 프레임/`ApplySkin`마다 다시 추첨하면 칸 그림이 계속 바뀌어서.
  - 심볼: `InGameSkinManager.BlockDestroyedHandler(...)` — 메서드 (수정)
    - 설명: `_blockDict`에 더해 `_visualIndex`에서도 해당 칸을 지운다.
    - 이유: 풀 반환 뒤 딕셔너리에 죽은 칸이 남지 않게.
  - 심볼: `InGameSkinManager.SkinChangedHandler(...)` / `SkinInitializedHandler(...)` — 메서드 (수정)
    - 설명: 장착 스킨을 바꾼 뒤 `_visualIndex`를 비우고 `ApplySkin`한다.
    - 이유: 랜덤 스킨으로 갈아탈 때 이전 인덱스가 새 스프라이트 배열을 가리키지 않게.
  - 심볼: `InGameSkinManager.BlockCreatedHandler(...)` / `ApplySkin()` — 메서드 (수정)
    - 설명: `GetSprite(ResolveVisualIndex(block, skinId))`로 스프라이트를 붙인다.
    - 이유: 색 id와 비주얼 인덱스를 분리해 랜덤 스킨과 기존 7색 스킨을 같은 경로로 처리하려고.
  - 심볼: `InGameSkinManager.ResolveVisualIndex(Block, int)` — 메서드 (추가)
    - 설명: 랜덤 스킨이면 저장된 인덱스를 쓰거나 `PickVisualIndex`로 새로 고르고, 아니면 `skinId`를 그대로 반환한다.
    - 이유: 힌트/게임플레이는 여전히 피스 `skinId`를 쓰므로 비주얼만 칸별로 갈라지게.

- 파일: `Scripts/Presentation/BlockShatterHint.cs`
  - 심볼: `BlockShatterHint.OutlineWaveId` — 필드 `int` (추가)
    - 설명: 셰이더 프로퍼티 `_OutlineWave`의 PropertyToID를 보관한다.
    - 이유: LateUpdate마다 문자열 조회하지 않게.
  - 심볼: `BlockShatterHint.outlineWave` — 필드 `float` (추가)
    - 설명: 힌트 클립이 키프레임하는 외곽선 일렁임 세기. 0이면 왜곡 없음.
    - 이유: `_OutlineWave`를 클립에 직접 넣으면 공유 머티리얼이 바뀌어 칸이 같이 흔들려서 MPB로 넣게.
  - 심볼: `BlockShatterHint.Apply()` — 메서드 (수정)
    - 설명: MPB에 `_OutlineWave`를 `outlineWave`로 쓴다.
    - 이유: 힌트 클립 값이 다음 드로우에 반영되게.
  - 심볼: `BlockShatterHint.ResetShatter()` — 메서드 (수정)
    - 설명: `outlineWave`를 0으로 되돌린 뒤 `Apply`한다.
    - 이유: 힌트가 꺼진 칸에 일렁임이 남지 않게.

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.SetHints(...)` — 메서드 (수정)
    - 설명: `RandomizeSprites`면 힌트 스프라이트 통일에 `null`을 넘겨 칸의 기존 스프라이트를 유지한다. 클립은 기존처럼 `GetHintClip(skinId)`이다.
    - 이유: 통일하면 랜덤 3장이 피스 색 id 한 장으로 덮여서.

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `_OutlineWave` — 프로퍼티/유니폼 (추가)
    - 설명: 0~1. Frag에서 스프라이트 UV 원형 림만 각도 사인으로 살짝 민다. 버텍스 메시는 건드리지 않는다.
    - 이유: 힌트 때만 외곽선이 물결치게 하려고. `_WaterWobble`은 쿼드 전체가 말랑거려 물방울과 다르다.

## 에셋

- `Graphics/Sprites/WaterDropBlocks.png` — 3셀. 외곽 투명 패딩.
- `Graphics/Animations/WaterDropHintWave.anim` — `outlineWave` 0→1 (0.25s), 루프 없음. 파동은 셰이더 `_Time`.
- `_Shared/ScriptableObjects/Skins/WaterDrop.asset` — `RandomizeSprites` 켜짐, HintClips 3칸 동일 클립, `LineClearEffects` 빈 배열.
- `JTH/ScriptableObjects/test/Skin data list.asset` — WaterDrop 등록. PMS 인벤토리 리스트는 안 건드림.

## 2 — 2026-08-19 · 물방울 힌트 일렁임 재생 수정

## 변경 상세

- 파일: `Graphics/Animations/WaterDropHintWave.anim`
  - 심볼: `WaterDropHintWave` 클립 설정 — 에셋 (수정)
    - 설명: `outlineWave`를 클립 전체에서 1로 두고 `loopTime`을 켠다. 런타임 바인딩 해시를 `AnimationUtility`로 다시 쓴다.
    - 이유: 0.25초 비루프면 애니메이터가 기본값 0으로 돌아가 파동이 안 보이고, YAML `attribute: 0`이면 Playable이 필드를 안 넣어서. 루프 시작이 0이면 매 주기 일렁임이 꺼져서 1로 고정.
    - 영향: `Block.PlayHintClip`이 이 클립을 재생.

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Vert` `_OutlineWave` 실루엣 변위 — 셰이더 (수정)
    - 설명: `_OutlineWave`가 켜지면 버텍스를 중심에서 바깥으로 각도 사인만큼 민다. `_Time`으로 계속 출렁인다.
    - 이유: Tight 메시에서 Frag UV만 밀면 실루엣이 안 움직여 일렁임이 안 보여서.
  - 심볼: `Frag` `_OutlineWave` UV 변위 — 셰이더 (수정)
    - 설명: 림 UV 밀림을 0.04에서 0.07로 키운다.
    - 이유: 내부 물결도 힌트 중에 보이게.

- 파일: `Docs/Implementations/line-clear-preview-fx/phase7.md`
  - 심볼: `WaterDropHintWave.anim` 행 — 문서 (수정)
    - 설명: 루프 유지와 Vert 실루엣 파동으로 고친다.
    - 이유: 완료 기준이 실제 재생과 맞게.

## 3 — 2026-08-19 · 물방울 일렁임 약하게

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Vert` `_OutlineWave` 실루엣 변위 — 셰이더 (수정)
    - 설명: 변위를 0.07에서 0.022로, 각속도·시간 속도를 낮추고 2차 파를 0.3으로 줄인다.
    - 이유: 힌트 중 외곽이 너무 출렁여서 약한 물결만 남기려고.
  - 심볼: `Frag` `_OutlineWave` UV 변위 — 셰이더 (수정)
    - 설명: UV 밀림을 0.07에서 0.025로, 파동 속도도 Vert와 같이 낮춘다.
    - 이유: 내부 왜곡이 실루엣과 같이 과해 보이지 않게.

## 4 — 2026-08-19 · 물방울 일렁임 조금 키움

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Vert` `_OutlineWave` 실루엣 변위 — 셰이더 (수정)
    - 설명: 변위를 0.022에서 0.034로 올린다. 파동 속도는 유지한다.
    - 이유: 한 단계 줄인 값이 너무 작아서, 과하지 않게만 키우려고.
  - 심볼: `Frag` `_OutlineWave` UV 변위 — 셰이더 (수정)
    - 설명: UV 밀림을 0.025에서 0.036으로 올린다.
    - 이유: 실루엣과 내부 물결 크기를 같이 맞추려고.

## 5 — 2026-08-19 · 물방울 클리어 버스트

## 변경 상세

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.PlayBurstForBlock(...)` — 메서드 (수정)
    - 설명: `RandomizeSprites`면 `_appliedSkinId` 대신 `PlacedSprite`로 이펙트 슬롯을 고른다.
    - 이유: 물방울은 칸마다 스프라이트가 달라서 피스 색 id로 고르면 다른 방울 버스트가 나가서.

- 파일: `WaterDropSkin/Prefabs/WaterDropBurst_0..2.prefab`
  - 심볼: `WaterDropBurst_*` — 프리팹 (추가)
    - 설명: 칸 중심에서 물방울 파티클을 한 번 터뜨린다. `PooledParticleEffect` + `WaterBalloonBurst` 셰이더 틴트.
    - 이유: 클리어 때 칸이 사라지며 물이 튀는 연출이 필요해서.

- 파일: `WaterDropSkin/Pool/WaterDropBurst_0..2.asset`
  - 심볼: `WaterDropBurst_*` — `PoolItemSO` (추가)
    - 설명: 버스트 프리팹을 풀 아이템으로 등록한다. initCount 12.
    - 이유: `PlayParticleEffectEvent`가 PoolItemSO만 받기 때문에.
    - 영향: `PoolManager.asset` itemList, `WaterDrop.LineClearEffects`.

- 파일: `_Shared/ScriptableObjects/Skins/WaterDrop.asset`
  - 심볼: `SkinDataSO.LineClearEffects` — 직렬화 배열 (수정)
    - 설명: 스프라이트 3장과 같은 인덱스로 `WaterDropBurst_0..2`를 넣는다.
    - 이유: Phase 7에서 비워 둔 클리어 파티클 슬롯을 채우려고.

