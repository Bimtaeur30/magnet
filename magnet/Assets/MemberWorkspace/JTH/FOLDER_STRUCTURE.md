# JTH Workspace — 폴더·asmdef 합의 (Phase 0)

팀원은 각자 `Assets/MemberWorkspace/[username]/` 아래 동일 패턴을 따른다.

```
MemberWorkspace/[username]/
  Scripts/
    [Username].asmdef          # 예: Magnet.JTH
    Bootstrap/                 # Reflex Installer, 씬 진입
    Events/                    # GameEvent 파생 클래스
    Data/                      # ScriptableObject 정의 (Phase 1+)
    Domain/                    # 순수 로직 (Phase 1+)
    Presentation/              # 뷰·이펙트 (Phase 1+)
  ScriptableObjects/           # SO 에셋
  Resources/                   # ReflexSettings (필수)
  Prefabs/
  Scenes/
```

## asmdef 참조 (공통)

| Assembly | GUID |
|----------|------|
| EventChannel_Assembly | `4f4fe35fbc82e694093dc30123d90eb6` |
| Reflex | `1530a967b84cfb44dbdd5e8a1989764f` |
| UniTask | `f51ebe6a0ceec4240a699833d6309b23` |

## 공용 런타임 코드

- Phase 0 시점: **각자 Workspace에 구현**, JTH `Events/`·`Bootstrap/`를 참조 구현으로 사용.
- `Assets/Shared/` 통합은 팀 합의 후 별도 Phase에서 진행.

## Reflex 부트스트랩

1. `Resources/ReflexSettings.asset` — RootScope 프리팹 등록
2. `Prefabs/RootScope.prefab` — `ContainerScope` + `MagnetProjectInstaller`
3. 씬에 `SceneScope` (`ContainerScope`) + 게임 오브젝트

## 이벤트 채널

- 에셋: `ScriptableObjects/MainEventChannel.asset` (`GameEventChannelSO`)
- DI: `MagnetProjectInstaller`에서 `RegisterValue`
