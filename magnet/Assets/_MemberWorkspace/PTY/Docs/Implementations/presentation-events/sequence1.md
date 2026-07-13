# Sequence — Phase 1 (presentation-events)

> **Phase:** [phase1.md](phase1.md) 와 1:1.

## 1 — 2026-07-12 · 파티클 풀 인프라 (기존 GameLib.ObjectPool 재사용)

**바뀐 것**

- 생성: `Scripts/Vfx/PooledParticleEffect.cs` — `AbstractMonoPoolable` 구현체, UniTask로 재생 완료 감시 후 `OnEffectFinished` 발행
- 생성: `Scripts/Events/PresentationEvents.cs` — `PlayParticleEffectEvent`(`PoolItemSO`, `Vector3`, `Quaternion`)
- 생성: `Scripts/Vfx/ParticleEffectManager.cs` — `magnetGameChannel` 구독, 풀에서 꺼내 재생 후 자동 반환
- 수정: `Scripts/Magnet.PTY.asmdef` — `ObjectPool.Runtime.Assembly`(`GUID:193cd8bd6f0c1454bab4694e30833173`) 참조 추가

**메모**

- 처음엔 Reflex `IParticlePoolService`를 새로 설계했으나, 코드 조사 중 `Assets/GameLib/ObjectPool/`에 이미 완성된 범용 풀(+`Tools/PoolManager` 커스텀 에디터)이 있고 `Assets/_Shared/Sound/SoundPlayer.cs`+`SoundManager.cs`가 이미 이벤트 채널로 이 풀을 구동하는 선례임을 확인 → 사용자 확인 후 GameLib 재사용 + 이벤트 기반(`PlayParticleEffectEvent`) 설계로 전환. Reflex `[Inject]` 서비스보다 프로젝트 규칙("객체 간 통신은 EventChannelSO만")에 더 맞고, SoundManager 패턴과도 일관됨.
- `SoundManager`는 `MonoSingleton<SoundManager>`를 쓰지만, 이는 프로젝트 DI 규칙(싱글톤 금지) 이전의 기존 코드로 판단해 그대로 따르지 않음. `ParticleEffectManager`는 싱글톤 없이 이벤트로만 트리거되는 순수 리스너로 구현.
- `PoolManagerSO.InitializePool`은 GameLib의 `PoolInitializer`가 씬에서 한 번만 호출하는 것을 전제로 하며, `ParticleEffectManager`에서는 중복 호출하지 않음.
- 씬 배치(`PoolInitializer`/`ParticleEffectManager` GameObject, `magnetGameChannel`/`PoolManager.asset` 연결)와 실제 파티클 프리팹/`PoolItemSO` 등록은 범위 밖으로 남김.
- `refresh_unity`(전체 compile 요청) + `read_console` 확인 결과 컴파일 에러·경고 0건.
