# Phase 2 — 블록 파괴 파티클 (스킨 텍스처 랜덤 조각 + 팝업/중력 낙하)

> **구현:** `presentation-events` · **Jira:** SCRUM-26
> **상태:** 완료 (프리팹·텍스처 스왑 지원까지. 실제 스킨 연결은 범위 밖)
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] 부숴지는 블록의 스킨 텍스처에서 랜덤한 부분을 보여주는 파티클
- [x] 위로 튀어오른 뒤 중력을 받아 자연스럽게 떨어지는 움직임
- [x] 어떤 스킨 텍스처든 재생 시점에 갈아끼울 수 있는 구조 (텍스처 하드코딩 X)
- [x] `manage_camera` 스크린샷 + `ParticleSystem.Simulate`로 시각 검증 (초반 버스트/중반 낙하/후반 페이드아웃)

## 사전 확인 — 실제 블록 스프라이트 연결은 범위 밖

`JTH.Scripts.Presentation.Block`(SpriteRenderer)에는 현재 적용된 스프라이트를 읽는 public API가 없고,
이 파일은 JTH 소유라 이번 작업에서 직접 손대지 않기로 사용자와 합의함. 따라서 이번 Phase는
**임의의 `Texture`를 받아 재생하는 범용 파티클**을 만드는 데 집중하고, "실제 파괴된 블록의 스킨을
가져와서 넘기는" 연결은 JTH 협업이 필요한 다음 단계로 남긴다.

## 구현 내용 (뭘 어떻게)

| 대상 | 내용 |
|---|---|
| `BlockDebrisParticle.prefab` (`Prefabs/Vfx/`) | `ParticleSystem` + `ParticleSystemRenderer` + `PooledParticleEffect`. Shape=Cone(위쪽, angle 25°)로 퍼지며 튀어오르고, `main.gravityModifier=1`로 낙하. `TextureSheetAnimation`(Grid 4x4=16타일, `startFrame`을 0~15 랜덤, `frameOverTime`은 상수 0)로 파티클마다 텍스처의 랜덤한 한 조각만 정적으로 표시. `colorOverLifetime`로 끝에서 페이드아웃, `rotationOverLifetime`/`sizeOverLifetime`로 회전·수축 추가 |
| `BlockDebrisParticle.mat` (`Materials/Vfx/`) | `Sprites/Default` 셰이더(프로젝트의 기존 `SpriteRenderer` 시각과 동일 계열, URP 2D에서 검증된 조합) |
| `DebrisTestTexture.png` (`Sprites/Vfx/`) | 16개 타일이 각각 다른 색인 4x4 테스트 텍스처. "랜덤 조각 표시"가 실제로 보이는지 확인하기 위한 자리표시자 — 실제 스킨 아트로 교체 예정 |
| `PooledParticleEffect.SetTexture(Texture)` | `MaterialPropertyBlock`으로 `_MainTex`/`_BaseMap`을 인스턴스별로 덮어써 머티리얼 애셋을 공유하면서도 스킨마다 다른 텍스처를 표시. **아틀라스에 패킹된 Sprite는 지원 안 함** — 독립 텍스처여야 함 |
| `PlayParticleEffectEvent.Init(effect, position, rotation, texture = null)` | `texture` 파라미터 추가 (기본값 null → 프리팹에 이미 설정된 텍스처 그대로 사용, `SoundEvents.PlaySoundEvent`의 선택적 파라미터 관례와 동일) |
| `ParticleEffectManager.HandlePlayParticleEffect` | `evt.Texture`가 있으면 재생 전 `pooled.SetTexture(evt.Texture)` 호출 |

### 시각 검증 방법

Unity MCP로 씬에 `BlockDebrisParticle` 인스턴스를 만들고 `ParticleSystem.Simulate(t, true, true)`로
시간을 강제 진행시키며 스크린샷 촬영 (0.05s: 버스트 직후 16색 타일 확인 / 0.35s: 위로 퍼지고 낙하 시작 /
0.85s: 대부분 소멸 + 페이드아웃 확인). 검증 후 씬의 임시 인스턴스와 스크린샷은 삭제함.

## 진행 중 발견한 사항 (참고용, 이번 Phase에서 손대지 않음)

작업 중 `Assets/GameLib/ObjectPool/PoolManager.asset`의 `itemList`가 비어 있고(기존 "Impact effect"/
"Slash Effect" 항목 2개 삭제됨), 반대로 `PTYScene.unity`에는 이미 `ParticleEffectManager` +
`PoolInitializer` GameObject가 실제 `magnetGameChannel`(JTH `MagnetGameChannel.asset`)과
`PoolManager.asset`에 정확히 연결되어 배치돼 있었다. 두 항목의 `prefab` 참조가 원래 깨져 있었던 점
(guid가 저장소 어디에도 없음)으로 미루어, 사용자가 `Tools/PoolManager` 에디터로 죽은 스텁 항목을 정리하고
씬 배치까지 이미 진행한 것으로 보여 되돌리지 않았다. `BlockDebrisParticle`을 `PoolItemSO`로 등록하는
작업은 사용자 확인 후 진행.

## 이 Phase 범위 밖

- `BlockDebrisParticle`을 `PoolItemSO`로 등록해 `PoolManager.asset`의 `itemList`에 추가 (`Tools/PoolManager` 사용)
- `Block`/`ShapeBlock`(JTH 소유)에서 실제 적용된 스킨 텍스처를 읽어 `PlayParticleEffectEvent`로 넘기는 연결
- `SquareClearedEvent` 등에서 셀 파괴 시 이 이벤트를 raise하는 게임플레이 연결
- 실제 스킨 아트 텍스처로 `DebrisTestTexture.png` 교체

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 파티클 프리팹 | `Prefabs/Vfx/BlockDebrisParticle.prefab` |
| 머티리얼 | `Materials/Vfx/BlockDebrisParticle.mat` |
| 테스트 텍스처 | `Sprites/Vfx/DebrisTestTexture.png` |
| 텍스처 스왑 API | `Scripts/Vfx/PooledParticleEffect.cs` |
| 이벤트 확장 | `Scripts/Events/PresentationEvents.cs` |
| 텍스처 적용 지점 | `Scripts/Vfx/ParticleEffectManager.cs` |
