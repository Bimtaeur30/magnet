# line-clear-preview-fx Sequence 9

> Phase 8 구현 기록 · 꿀벌집 스킨 (랜덤 3톤 + 짓눌림 셰이더 + 꿀 스플랫)

## 1 — 2026-08-20 · Honeycomb squash + honey splat

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `_Squash` — 프로퍼티/유니폼 (추가)
    - 설명: 0~1. Vert에서 메시를 세로로 납작하게 하고 옆으로 민 뒤, Frag에서 육각 UV를 눌러 일그러뜨린다. 0이면 원본과 같다.
    - 이유: 클리어 예고 때 벌집이 짓눌린 것처럼 보이게 하려고. `_WaterWobble`은 풍선 말랑이라 육각 격자가 안 찌그러진다.
    - 영향: `BlockShatterHint.Apply`가 MPB로 칸마다 넣는다.
  - 심볼: `Vert` `_Squash` 메시 변위 — 셰이더 (수정)
    - 설명: `_Squash`가 켜지면 Y를 줄이고 X를 늘리며 아래가 처지고 가장자리가 살짝 톱니처럼 휜다.
    - 이유: 스케일 클립만으로는 육각 실루엣이 안 눌려 보여서.
  - 심볼: `Frag` `_Squash` UV 변위 — 셰이더 (수정)
    - 설명: UV를 세로로 눌러 샘플하고 사인으로 벽을 꺾는다. 스프라이트 밖으로 나가면 clip 한다.
    - 이유: 메시만 납작하면 텍스처가 같이 줄어 일그러짐이 약해서. 아틀라스 이웃 칸을 안 읽게 clip이 필요해서.

- 파일: `Scripts/Presentation/BlockShatterHint.cs`
  - 심볼: `BlockShatterHint.SquashId` — 필드 `int` (추가)
    - 설명: 셰이더 프로퍼티 `_Squash`의 PropertyToID를 보관한다.
    - 이유: LateUpdate마다 문자열 조회하지 않게.
  - 심볼: `BlockShatterHint.squash` — 필드 `float` (추가)
    - 설명: 힌트 클립이 키프레임하는 짓눌림 세기. 0이면 왜곡 없음.
    - 이유: `_Squash`를 클립에 직접 넣으면 공유 머티리얼이 바뀌어 칸이 같이 눌려서 MPB로 넣게.
  - 심볼: `BlockShatterHint.Apply()` — 메서드 (수정)
    - 설명: MPB에 `_Squash`를 `squash`로 쓴다.
    - 이유: 힌트 클립 값이 다음 드로우에 반영되게.
  - 심볼: `BlockShatterHint.ResetShatter()` — 메서드 (수정)
    - 설명: `squash`를 0으로 되돌린 뒤 `Apply`한다.
    - 이유: 힌트가 꺼진 칸에 짓눌림이 남지 않게.

- 파일: `Graphics/Shaders/HoneyBurst.shader`
  - 심볼: `_Tint` / `_Softness` / `_RimPower` — 프로퍼티 (추가)
    - 설명: 타원 방울 실루엣에 호박색 코어·하이라이트를 칠한다.
    - 이유: 클리어 때 점성 있는 꿀 방울로 보이게. 물/슬라임 버스트 셰이더와 색·하이라이트가 달라서 분리했다.

- 파일: `CodexBridge/Editor/HoneycombSkinBuilder.cs`
  - 심볼: `HoneycombSkinBuilder.Build()` — 메서드 (추가)
    - 설명: 스프라이트 3장, 짓눌림 힌트 클립, 꿀 스플랫 풀 아이템 3개, `Honeycomb` SkinDataSO를 만들고 테스트 리스트·풀에 등록한다.
    - 이유: 물방울/슬라임과 같이 에셋을 한 메뉴로 만들게.

## 에셋

- `Graphics/Sprites/HoneycombBlocks.png` — 3셀. 검정 배경 제거.
- `HoneycombSkin/Animations/HoneycombHintSquash.anim` — `squash` + scale 눌림→복원 루프 (0.64s).
- `HoneycombSkin/Prefabs/HoneyBurst_0..2.prefab` — 칸 중심에서 꿀이 사방으로 퍼진다. `PooledParticleEffect` + `HoneyBurst` 틴트.
- `HoneycombSkin/Pool/HoneyBurst_0..2.asset` — initCount 12.
- `_Shared/ScriptableObjects/Skins/Honeycomb.asset` — `RandomizeSprites` 켜짐, HintClips 3칸 동일 클립, LineClearEffects 3.
- `JTH/ScriptableObjects/test/Skin data list.asset` — Honeycomb 등록. PMS 인벤토리 리스트는 안 건드림.

## 2 — 2026-08-20 · 사방 일그러짐 + 테두리 넘침

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Vert` `_Squash` 메시 변위 — 셰이더 (수정)
    - 설명: 한 축으로 납작하게 하지 않는다. 가장자리를 각도 로브로 바깥에 밀어 원래 칸 테두리를 조금 넘기고, 중심은 살짝 오므리며 XY를 같은 세기로 휜다. 로브는 `_Time`으로 천천히 돈다.
    - 이유: 세로로만 눌리면 벌집이 납작해지기만 해서. 전체적으로 짓눌리며 꿀이 테두리 밖으로 배어 나오게.
  - 심볼: `Frag` `_Squash` UV 변위 — 셰이더 (수정)
    - 설명: UV를 세로로 스케일하지 않고 XY 모두 같은 세기로 꺾는다. 중심만 살짝 오므리고 밖으로 나간 UV는 saturate 한다.
    - 이유: 축 스케일 clip은 한쪽만 찌그러진 구멍을 만들어서. 아틀라스 이웃은 saturate로 막는다.

- 파일: `HoneycombSkin/Animations/HoneycombHintSquash.anim`
  - 심볼: `m_LocalScale` 커브 — 에셋 (수정)
    - 설명: X/Y 스케일을 1로 고정한다. 짓눌림은 `squash`만 흔든다.
    - 이유: 클립이 X를 늘리고 Y를 줄이면 셰이더와 겹쳐 한 방향으로만 눌려 보여서.
    - 영향: `Block.PlayHintClip`이 이 클립을 재생.

- 파일: `CodexBridge/Editor/HoneycombSkinBuilder.cs`
  - 심볼: `HoneycombSkinBuilder.CreateHintClip()` — 메서드 (수정)
    - 설명: 스케일 키를 1로 두고 `squash` 루프만 남긴다.
    - 이유: 다시 빌드해도 한 방향 압착 클립이 되살아나지 않게.

## 3 — 2026-08-20 · 펄스 느리게 + 넘침 두껍게

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Vert` `_Squash` 메시 변위 — 셰이더 (수정)
    - 설명: 테두리 로브를 더 두껍고 크게 민다(각도 주파수 낮춤, 변위 0.058→0.12, 림 구간을 안쪽까지). `_Time` 속도를 2.6에서 1.15로 낮춘다.
    - 이유: 얇은 돌기가 빠르게 깜빡여 뭉개진 꿀처럼 안 보여서.
  - 심볼: `Frag` `_Squash` UV 변위 — 셰이더 (수정)
    - 설명: 육각 UV 휨도 같은 방향으로 키우고 주파수를 낮춘다.
    - 이유: 실루엣만 커지고 격자 일그러짐이 작으면 안쪽이 안 뭉개져 보여서.

- 파일: `HoneycombSkin/Animations/HoneycombHintSquash.anim`
  - 심볼: `squash` 커브 — 에셋 (수정)
    - 설명: 루프를 0.64초에서 1.45초로 늘린다. 눌림→복원 모양은 유지한다.
    - 이유: 팔딱임 간격이 너무 짧아서.
    - 영향: `Block.PlayHintClip`이 이 클립을 재생.

- 파일: `CodexBridge/Editor/HoneycombSkinBuilder.cs`
  - 심볼: `HoneycombSkinBuilder.CreateHintClip()` — 메서드 (수정)
    - 설명: `squash` 키 시간을 1.45초 루프에 맞춘다.
    - 이유: 다시 빌드해도 빠른 팔딱 클립이 되살아나지 않게.

## 4 — 2026-08-20 · 뭉개짐 한 단계 줄임

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Vert` `_Squash` 메시 변위 — 셰이더 (수정)
    - 설명: 테두리 변위를 0.12에서 0.085로, 중심 오므림과 부가 휨도 같이 줄인다. 두꺼운 로브 주파수는 유지한다.
    - 이유: 넘침을 키운 뒤 너무 짓이겨 보여서 한 단계만 되돌리려고.
  - 심볼: `Frag` `_Squash` UV 변위 — 셰이더 (수정)
    - 설명: 육각 UV 휨을 0.055에서 0.042로 낮춘다.
    - 이유: 실루엣과 격자 뭉개짐을 같이 맞추려고.
