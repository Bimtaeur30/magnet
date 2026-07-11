# JTH Workspace — 폴더·asmdef 합의 (M0 공통 기반)

팀원은 각자 `Assets/_MemberWorkspace/[username]/` 아래 동일 패턴을 따른다.

```
_MemberWorkspace/[username]/
  Docs/
    IMPLEMENTATIONS.md           # 구현 인덱스
    Implementations/
      [slug]/                    # 예: block-coordinates, inventory
        phases.md                # Phase 인덱스
        phase1.md                # Phase 1 계획 (뭘 어떻게 구현)
        sequence1.md             # Phase 1 변경 기록 (1:1)
        phase2.md
        sequence2.md
  Scripts/
    [Username].asmdef            # 예: Magnet.JTH
    Bootstrap/
    Events/
    Data/                        # BoardConfigSO, MagnetInputSO 등
    Domain/
    Input/                       # Input MonoBehaviour (SO 구독만)
    Presentation/
  Input/                         # Controls.inputactions (Input System, Unity 코드 생성)
  ScriptableObjects/
  Resources/                     # ReflexSettings (필수)
  Prefabs/
  Scenes/
```

## asmdef 참조 (공통)

| Assembly | GUID |
|----------|------|
| **Magnet.Contracts** (공용 계약) | `eb415e56e20154449aab8a7efaff0147` |
| EventChannel_Assembly | `4f4fe35fbc82e694093dc30123d90eb6` |
| Reflex | `1530a967b84cfb44dbdd5e8a1989764f` |
| UniTask | `f51ebe6a0ceec4240a699833d6309b23` |
| Unity Input System | `75469ad4d38634e559750d17036d5f7c` |

## 공용 런타임 코드

- **멤버 간 계약** (인터페이스·순수 데이터): `Assets/Shared/Magnet.Contracts/`
  - 타 멤버 Workspace를 참조하지 않음. SO 구현체는 각자 Workspace에서 `Magnet.Contracts`만 참조.
- M0 시점: **각자 Workspace에 구현**, JTH `Events/`·`Bootstrap/`를 참조 구현으로 사용.
- 추가 공용 코드는 `Assets/Shared/` 아래 합의 후 확장.

## Reflex 부트스트랩

1. `Resources/ReflexSettings.asset` — RootScope 프리팹 등록
2. `Prefabs/RootScope.prefab` — `ContainerScope` + `MagnetProjectInstaller`
3. 씬에 `SceneScope` (`ContainerScope`) + 게임 오브젝트

**SO ↔ Reflex:** **모든 SO·프로젝트 에셋**(프리팹 등)은 소비 `MonoBehaviour`에 `[SerializeField]` — **`[Inject]`·Installer `RegisterValue` 금지**. **씬 MonoBehaviour / 다른 GO**는 Installer `RegisterValue` + 소비자 **`[Inject]`** (`jth-reflex-di.mdc`). 크로스-asmdef **인터페이스**도 `[Inject]`.

## 이벤트 채널

- 에셋: `ScriptableObjects/MainEventChannel.asset`
- 필드명: `[SerializeField] EventChannelSO magnetGameChannel` (각 소비 컴포넌트)

## 입력 (InputSO)

- **모든 게임플레이 입력**은 `InputSO`(`ScriptableObject`) 한 곳에서 Input System 콜백을 받고 **C# event**로 노출한다.
- MonoBehaviour(`Scripts/Input/`)는 `Update`·`Input.GetKeyDown` **금지** — `[SerializeField] MagnetInputSO` 구독만.
- 패턴: `Controls.inputactions` → Unity 생성 `Controls.cs` → `MagnetInputSO : Controls.IPlayerActions` → 소비자 구독.
- 에셋: `ScriptableObjects/DefaultMagnetInput.asset` · 액션 맵: `Scripts/Input/Controls.inputactions`
- SO는 `[SerializeField]` 연결 (`[Inject]` 금지, 다른 SO와 동일).
