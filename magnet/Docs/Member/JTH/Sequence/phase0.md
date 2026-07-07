# Phase 0 — 공통 기반

> **상태:** 구현됨 · 사용자 최종 확인 전 (`DESIGN.md` ✅는 선반영)  
> **다음:** Phase 1 — 보드 N×N + 자석 축 + `BoardConfigSO` (미착수)

## 목표 (DESIGN.md)

Reflect(Reflex) 부트스트랩, EventChannelSO 에셋, asmdef/폴더 합의, 빈 씬 진입.

## 이 Phase에서 한 일

### 사전 준비 (Phase 0 착수 전)

- `Docs/DESIGN.md` — 게임 규칙, Phase 표, 기술 스택, Jira 매핑
- `Docs/README.md`, `Docs/TODO.md` — 문서·할 일 정리
- JTH 역할 확정: 코어 게임플레이 리드, Phase 0–6
- Jira MCP (`Atlassian-MCP-Server`) 설정

### 구현 (JTH Workspace)

- **asmdef** — `Magnet.JTH` (EventChannel / Reflex / UniTask 참조)
- **이벤트 스텁** — DESIGN 6.4 이벤트 클래스 + 검증용 `Phase0ReadyEvent`
- **Reflex DI** — `MainEventChannel`을 RootScope에서 컨테이너에 등록
- **에셋** — `MainEventChannel`, `ReflexSettings`, `RootScope` prefab
- **씬** — `Phase0_Bootstrap` (Camera, Light, SceneScope, Phase0Bootstrap)
- **팀 합의 문서** — `FOLDER_STRUCTURE.md` (폴더·asmdef 패턴)

### 이 Phase에서 안 한 것

- 보드 격자, 자석 축, `BoardConfigSO` → **Phase 1**
- 블록·흡착·폭발·회전 등 게임플레이 전부

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| asmdef 참조 | `Assets/MemberWorkspace/JTH/Scripts/Magnet.JTH.asmdef` |
| 게임 이벤트 정의 | `Scripts/Events/MagnetGameEvents.cs` |
| Reflex 등록 (SO → DI) | `Scripts/Bootstrap/MagnetProjectInstaller.cs` |
| Phase 0 동작 확인용 | `Scripts/Bootstrap/Phase0Bootstrap.cs` |
| 이벤트 채널 에셋 | `ScriptableObjects/MainEventChannel.asset` |
| Reflex 전역 설정 | `Resources/ReflexSettings.asset` |
| Root 바인딩 (prefab만) | `Prefabs/RootScope.prefab` |
| 진입 씬 | `Scenes/Phase0_Bootstrap.unity` |
| 팀 폴더 규칙 | `FOLDER_STRUCTURE.md` |

### Reflex 구조 (읽는 순서)

1. `ReflexSettings.asset` → `RootScope.prefab` 참조
2. `RootScope` = `ContainerScope` + 자식 `MagnetProjectInstaller` (`MainEventChannel` 연결)
3. 씬의 `SceneScope` (`ContainerScope`) → 씬 단위 컨테이너
4. `Phase0Bootstrap` — `[Inject] EventChannelSO` 주입, `GameEvents.Phase0ReadyEvent`로 Raise

### 이벤트 패턴

- `GameEvents` static 클래스에 인스턴스 미리 생성
- 데이터 필요 시 `GameEvents.BlockPlacedEvent.Init(id)` → `RaiseEvent`
- `new GameEvent()` 금지

## 메모

- 패키지 이름은 **Reflex** (`com.gustavopsantos.reflex`). CLAUDE.md 구 지칭 "Reflect"와 동일.
- `RootScope`는 **씬에 두지 않음**. prefab + ReflexSettings 등록만.
- MCP 메뉴로 `Assets/` 루트에 잘못 생긴 `ReflexSettings.asset`, `RootScope.prefab` 있으면 **삭제** (JTH 쪽 사용).
- Phase 0 구현 시 워크플로 미준수: 계획 승인 없이 구현, 에이전트가 play 모드로 검증함 → 이후 Phase부터 `CLAUDE.md` 워크플로 따름.
