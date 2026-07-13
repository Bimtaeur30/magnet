# Sequence — Phase 2 (presentation-events)

> **Phase:** [phase2.md](phase2.md) 와 1:1.

## 1 — 2026-07-12 · 블록 파괴 파티클 프리팹 + 텍스처 스왑 지원

**바뀐 것**

- 생성: `Prefabs/Vfx/BlockDebrisParticle.prefab` — `ParticleSystem`(Cone 위쪽 버스트, gravityModifier=1,
  TextureSheetAnimation Grid 4x4 랜덤 정적 프레임, colorOverLifetime 페이드아웃, rotation/size over lifetime) +
  `PooledParticleEffect`
- 생성: `Materials/Vfx/BlockDebrisParticle.mat` (`Sprites/Default`)
- 생성: `Sprites/Vfx/DebrisTestTexture.png` (4x4 색상 구분 테스트 텍스처)
- 수정: `Scripts/Vfx/PooledParticleEffect.cs` — `particleRenderer` 필드 + `SetTexture(Texture)` 추가
  (`MaterialPropertyBlock`으로 `_MainTex`/`_BaseMap` 인스턴스별 오버라이드)
- 수정: `Scripts/Events/PresentationEvents.cs` — `PlayParticleEffectEvent`에 `Texture` 선택 파라미터 추가
- 수정: `Scripts/Vfx/ParticleEffectManager.cs` — 재생 전 `evt.Texture`가 있으면 `SetTexture` 호출

**메모**

- Unity MCP `execute_code`는 메서드 본문으로 실행되므로 `using` 지시문을 쓸 수 없고(전부 컴파일 에러),
  `UnityEngine.Object`처럼 `object`와 이름이 겹치는 타입은 완전한 이름으로 써야 함. `codedom` 컴파일러라
  C# 6 문법만 허용(Roslyn 미설치).
- `ParticleSystem.Burst.repeatInterval`은 0을 허용하지 않음(`ArgumentOutOfRangeException`) — `cycleCount=1`
  이라 실질적으로 안 쓰이지만 1 이상 값을 넣어야 함.
- `startRotation`은 라디안 단위(Unity 공식 예제로 확인, `unity_docs`).
- 시각 검증은 `ParticleSystem.Simulate(t, true, true)` + `manage_camera` 스크린샷으로 진행(0.05/0.35/0.85초) —
  16색 랜덤 타일, 위로 퍼진 뒤 낙하하는 궤적, 후반 페이드아웃 모두 확인. 검증용 씬 인스턴스와 스크린샷은 삭제함.
- **작업 중 발견**: `git status` 확인 결과 `Assets/GameLib/ObjectPool/PoolManager.asset`의 `itemList`가
  비어 있고 기존 "Impact effect"/"Slash Effect" `PoolItemSO` 2개가 삭제된 상태였음. 반면 `PTYScene.unity`에는
  이미 `ParticleEffectManager`+`PoolInitializer`가 실제 `magnetGameChannel`/`PoolManager.asset`에 정확히
  연결되어 배치돼 있었음 — 두 스텁 항목의 `prefab` 참조가 원래 깨져 있었던 점(저장소에 없는 guid)으로 보아
  사용자가 `Tools/PoolManager`로 죽은 스텁을 정리하고 씬 배치까지 진행한 것으로 판단, 되돌리지 않음.
- `refresh_unity` + `read_console` 확인 결과 컴파일 에러·경고 0건.
