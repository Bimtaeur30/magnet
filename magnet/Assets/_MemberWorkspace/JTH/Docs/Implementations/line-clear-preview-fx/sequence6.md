# line-clear-preview-fx Sequence 6

> Phase 5 구현 기록 · 클리어 조각 파티클

## 1 — 2026-08-16 · 조각 시트 + 7색 버스트 풀

## 변경 상세

- 파일: `Graphics/Vfx/BlockShards.png`
  - 심볼: `BlockShards` — 텍스처 (추가)
    - 설명: 64×64 조각 5장을 가로로 붙인 흰·회색 실루엣 시트. 가장자리는 조금 어둡다.
    - 이유: 파티클이 칸 색으로 곱해 쓰도록 무채색 조각을 직접 그리려고.

- 파일: `Graphics/Materials/DefaultShardBurst.mat`
  - 심볼: `DefaultShardBurst` — 머티리얼 (추가)
    - 설명: URP Particles Unlit, `_BaseMap`/`_MainTex`에 조각 시트, Transparent.
    - 이유: 파티클 렌더러가 알파 있는 조각을 그리게.

- 파일: `Prefabs/Vfx/DefaultShardBurst/DefaultShardBurst_0~6.prefab`
  - 심볼: `ParticleSystem` — 컴포넌트 (추가)
    - 설명: 루프 없이 0초에 5발 버스트. 속도·크기·회전 랜덤, 중력, 알파 페이드, 텍스처 시트 5타일 중 랜덤 1장. startColor만 바리에이션마다 다름.
    - 이유: 칸이 사라진 자리에서 조각이 튀는 것처럼 보이게.
  - 심볼: `PooledParticleEffect` — 컴포넌트 (추가)
    - 설명: 기존 PTY 풀 파티클 래퍼. `Item`·루트 PS·렌더러를 연결한다.
    - 이유: ParticleEffectManager가 Pop/Push 할 수 있게. PTY 코드는 수정하지 않음.

- 파일: `ScriptableObjects/Pool/DefaultShardBurst_0~6.asset`
  - 심볼: `PoolItemSO.prefab` / `initCount` — 필드 (추가)
    - 설명: 각 색 프리팹을 가리키고 initCount 12.
    - 이유: 한 줄 8칸이 동시에 터져도 풀이 모자라지 않게.

- 파일: `GameLib/ObjectPool/PoolManager.asset`
  - 심볼: `PoolManagerSO.itemList` — 필드 (수정)
    - 설명: DefaultShardBurst 0~6 PoolItem을 추가한다.
    - 이유: ParticleEffectManager가 쓰는 공용 풀에 등록해야 Pop이 되게.

- 파일: `_Shared/ScriptableObjects/Skins/Default.asset`
  - 심볼: `SkinDataSO.LineClearEffects` — 프로퍼티 (수정)
    - 설명: 7슬롯에 DefaultShardBurst_0~6을 넣는다.
    - 이유: 칸마다 GetLineClearEffect(spriteIndex)로 해당 젬 색 버스트를 쏘게.

## 2 — 2026-08-16 · 칸 젬 색 매칭

## 변경 상세

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block._placedSprite` — 필드 `Sprite` (추가)
    - 설명: `ApplySkin`으로 깐 원래 젬 스프라이트를 보관한다. 힌트 통일 스프라이트와 분리한다.
    - 이유: 힌트가 드롭 피스 색으로 덮어도, 터질 때는 그 칸의 원래 색을 써야 해서.
  - 심볼: `Block.PlacedSprite` — 프로퍼티 (추가)
    - 설명: `_placedSprite`를 읽는다.
    - 이유: 클리어 버스트가 렌더러의 힌트 스프라이트가 아니라 원래 젬을 고르게.
  - 심볼: `Block.VisualCenter` — 프로퍼티 (추가)
    - 설명: `skinRenderer.bounds.center`를 반환한다. 렌더러가 없으면 transform 위치.
    - 이유: 조각이 칸 비주얼 중심에서 나가게.
  - 심볼: `Block.ApplySkin(Sprite)` — 메서드 (수정)
    - 설명: 인자를 `_placedSprite`에 저장한 뒤, 힌트 중이면 힌트 스프라이트만 그린다.
    - 이유: 힌트 덮어쓰기가 원래 색 레퍼런스를 지우지 않게.
  - 심볼: `Block.ResetItem()` — 메서드 (수정)
    - 설명: 풀 반환 시 `_placedSprite`를 null로 비운다.
    - 이유: 다음 Pop이 이전 칸 색을 들고 있지 않게.

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.presentationChannel` — 필드 `EventChannelSO` (추가)
    - 설명: `PlayParticleEffectEvent`를 올리는 채널. Prefab에 ParticleEffectManager와 같은 SO를 넣는다.
    - 이유: PTY 매니저 코드를 건드리지 않고 기존 파티클 재생 경로를 쓰려고.
  - 심볼: `LineClearHintEffector.PlayBurstForBlock(Block)` — 메서드 (추가)
    - 설명: `FireCenteredLineClear`면 스킵. `PlacedSprite`로 슬롯을 고르고 칸 중심에서 버스트를 쏜다.
    - 이유: 한 줄에 여러 색이 있어도 드롭 피스 SkinId가 아니라 사라지는 칸 색으로 터지게.
    - 영향: `PlacedBlocksView.DestroyCellViews`가 Push 전에 호출한다.
  - 심볼: `LineClearHintEffector.ResolveSpriteIndex(Sprite)` — 메서드 (추가)
    - 설명: `_currentSkin.Sprites`에서 같은 스프라이트 참조의 인덱스를 찾는다. 없으면 0.
    - 이유: LineClearEffects 슬롯이 스킨 스프라이트 배열과 1:1이라서.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.DestroyCellViews(IEnumerable<Vector2Int>)` — 메서드 (수정)
    - 설명: `PushBlock` 전에 `PlayBurstForBlock`을 칸마다 호출한다.
    - 이유: 풀 반환·ResetItem이 스프라이트를 지우기 전에 색을 읽어야 해서.

- 파일: `Scripts/Presentation/LineClearExplosionPresenter.cs`
  - 심볼: `LineClearExplosionPresenter.OnBlockPlaced(BlockPlacedEvent)` — 메서드 (수정)
    - 설명: 칸별 `PlayCellEffects`를 제거한다. `FireCenteredLineClear`일 때만 줄 중심 이펙트를 쏜다.
    - 이유: 칸 버스트는 DestroyCellViews에서 이미 쏘므로 중복·잘못된 SkinId 발사를 막으려고.

- 파일: `Prefabs/Board/Placed Blocks View.prefab`
  - 심볼: `LineClearHintEffector.presentationChannel` — 직렬화 필드 (수정)
    - 설명: presentation EventChannelSO를 할당한다.
    - 이유: PlayBurstForBlock이 이벤트를 올릴 수 있게.

- 파일: `Prefabs/Vfx/DefaultShardBurst/DefaultShardBurst_0~6.prefab`
  - 심볼: `ParticleSystem.main.startColor` — 필드 (수정)
    - 설명: Default 스킨 `Sprites[i]` 아틀라스를 Blit으로 읽어 평균 RGB를 구한 뒤, 그 색의 82%~108% TwoColors로 넣는다. 0=빨강, 1=주황, 2=노랑, 3=초록, 4=보라, 5=하늘, 6=파랑.
    - 이유: 예전에 넣은 임의 팔레트(0=노랑 등)가 실제 젬과 달랐기 때문에.

## 3 — 2026-08-16 · 힌트 색으로 통일 버스트

## 변경 상세

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.PlayBurstForBlock(Block)` — 메서드 (수정)
    - 설명: 힌트가 있으면 `_appliedSkinId`(줄이 변한 색)로 이펙트를 고른다. 힌트가 없을 때만 `PlacedSprite`로 폴백한다.
    - 이유: 칸마다 원래 젬 색을 쓰면 힌트로 한 색이 된 줄에서 다른 색 조각이 나와서.

## 4 — 2026-08-16 · 조각 시트 교체

## 변경 상세

- 파일: `Graphics/Vfx/BlockShards.png`
  - 심볼: `BlockShards` — 텍스처 (수정)
    - 설명: 제공한 5조각 그림을 검은 배경→알파로 바꾼 뒤 256×256 칸 5개(1280×256) 스트립으로 넣는다. 임포트는 RGBA32 비압축.
    - 이유: 기존 320×64 압축 시트가 파티클에서 뭉개져 보여서.

## 5 — 2026-08-16 · 버스트 프리팹 바리언트

## 변경 상세

- 파일: `Prefabs/Vfx/DefaultShardBurst/DefaultShardBurst.prefab`
  - 심볼: `DefaultShardBurst` — 베이스 프리팹 (추가)
    - 설명: ParticleSystem·머티리얼·PooledParticleEffect를 가진 원본. startColor는 흰색 TwoColors, Item은 비움.
    - 이유: 크기·속도·개수는 여기서만 고치면 7색이 같이 따라가게.

- 파일: `Prefabs/Vfx/DefaultShardBurst/DefaultShardBurst_0~6.prefab`
  - 심볼: `PrefabInstance` — 바리언트 (수정)
    - 설명: 독립 프리팹을 베이스 바리언트로 바꾼다. 오버라이드는 이름, startColor, `PooledParticleEffect.Item`만.
    - 이유: 모션을 7번 복제하지 않으려고.
