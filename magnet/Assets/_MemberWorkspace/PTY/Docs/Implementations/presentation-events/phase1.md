# Phase 1 — 파티클 풀 인프라 (연출 이벤트 구조 첫 항목)

> **구현:** `presentation-events` · **Jira:** SCRUM-26
> **상태:** 완료
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] 파티클 이펙트를 풀링해서 재생하는 인프라 구성
- [x] 풀 항목(프리팹/키/초기수) 관리는 커스텀 에디터로 가능해야 함
- [x] 소비자는 직접 참조/주입 없이 이벤트만 raise하면 파티클이 재생되도록

## 사전 조사 — 기존 자산 재사용

`Assets/GameLib/ObjectPool/`에 이미 범용 오브젝트 풀(`IPoolable`, `PoolItemSO`, `PoolManagerSO`, `Pool`)과
`Tools/PoolManager` 커스텀 에디터(UI Toolkit, split view, 항목 추가/삭제/이름변경/프리팹 검증)가 존재하지만
아직 어떤 씬에도 연결되지 않은 상태였다. `Assets/_Shared/Sound/SoundPlayer.cs`+`SoundManager.cs`가 이미 이
풀을 사운드용으로 쓰고 있는 선례이며, `PlaySoundEvent`(`EventChannelSO`)를 구독해 풀에서 꺼내 재생하고
끝나면 `OnSoundFinished` 콜백으로 반환하는 패턴을 그대로 따랐다.

**결론: GameLib.ObjectPool은 수정하지 않고 그대로 재사용.** 새 코드는 파티클 전용 `IPoolable` 구현체와
그걸 이벤트로 구동하는 매니저만 PTY workspace에 추가한다. `Tools/PoolManager` 에디터도 그대로 쓴다
(이미 깔끔한 UI Toolkit 창이라 별도 에디터를 새로 만들지 않음).

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|---|---|---|
| `PooledParticleEffect` | `Scripts/Vfx/` | `AbstractMonoPoolable` 구현체. `Play()`로 재생 시작, UniTask로 `ParticleSystem.IsAlive(true)`를 감시하다 끝나면 `OnEffectFinished` 이벤트 발행. `ResetItem()`에서 정지+클리어+이벤트 해제 (SoundPlayer와 동일 패턴) |
| `PlayParticleEffectEvent` / `PresentationEvents` | `Scripts/Events/` | `PoolItemSO Effect`, `Vector3 Position`, `Quaternion Rotation`을 담는 연출 이벤트. JTH 소유 `MagnetGameEvents.cs`와 별도 파일 |
| `ParticleEffectManager` | `Scripts/Vfx/` | 씬 배치용 MonoBehaviour. `magnetGameChannel`에서 `PlayParticleEffectEvent`를 구독해 `PoolManagerSO.Pop<PooledParticleEffect>`로 꺼내 재생하고, `OnEffectFinished` 수신 시 `Push`로 반환 |

### 소비 방법 (다른 시스템에서 파티클을 재생하려면)

```csharp
magnetGameChannel.RaiseEvent(
    PresentationEvents.PlayParticleEffectEvent.Init(explosionEffectItem, position, rotation));
```

`[Inject]`나 서비스 참조가 필요 없다 — `EventChannelSO`만 있으면 된다 (프로젝트 규칙: 객체 간 통신은
EventChannelSO만).

### 항목 등록 (커스텀 에디터로 관리)

1. Unity 메뉴 `Tools/PoolManager` 실행 (`GameLib/ObjectPool/PoolManager.asset` 대상)
2. `Create` 버튼으로 새 `PoolItemSO` 생성 → 이름 변경, 파티클 프리팹(루트에 `PooledParticleEffect` + `ParticleSystem` 붙은 프리팹) 드래그 등록
3. 프리팹에 `PooledParticleEffect`가 없으면 에디터가 자동으로 프리팹 참조를 비움 (`PoolItemSO.OnValidate`, 기존 동작)

### DI/씬 배치 관련 결정

- `ParticleEffectManager`는 Reflex에 등록하지 않는다 — 아무도 직접 참조/주입할 필요가 없고, 이벤트로만 트리거되기 때문 (SoundManager와 동일한 역할이지만 싱글톤은 사용하지 않음, 프로젝트 DI 규칙 준수)
- `PoolManagerSO.InitializePool(Transform)`은 `ParticleEffectManager`가 아니라 GameLib의 `PoolInitializer`(씬에 배치, `DontDestroyOnLoad`)가 호출하는 것을 전제로 한다 — 중복 초기화 방지

## 이 Phase 범위 밖

- 씬에 `PoolInitializer`/`ParticleEffectManager` 배치, `magnetGameChannel`·`PoolManager.asset` 연결
- 실제 파티클 프리팹 제작, `PoolItemSO` 항목 등록
- `SquareClearedEvent`/`GameOverEvent` 등 기존 게임플레이 이벤트에서 `PlayParticleEffectEvent`를 raise하는 연결 (다음 Phase)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 파티클 풀 아이템 | `Scripts/Vfx/PooledParticleEffect.cs` |
| 연출 이벤트 | `Scripts/Events/PresentationEvents.cs` |
| 씬 매니저 | `Scripts/Vfx/ParticleEffectManager.cs` |
| 재사용한 기존 풀 시스템 (수정 안 함) | `Assets/GameLib/ObjectPool/` |
| 참고한 선례 (사운드) | `Assets/_Shared/Sound/SoundPlayer.cs`, `SoundManager.cs` |
