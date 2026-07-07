# Phase 1 — Reflex·EventChannel·asmdef·씬 부트스트랩

> **구현:** `common-bootstrap` · **Jira:** — · **마일스톤:** M0  
> **상태:** 구현됨 · 사용자 최종 확인 전  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

이 파일은 이 Phase에서 **뭘 어떻게 구현하는지**를 적는다. 실제로 바뀐 내용은 `sequence1.md`에 기록.

## 목표 (완료 기준)

Reflect(Reflex) 부트스트랩, EventChannelSO 에셋, asmdef/폴더 합의, 빈 씬 진입.

## 구현 내용 (뭘 어떻게)

| 항목 | 구현 방식 |
|------|-----------|
| asmdef | `Magnet.JTH` — EventChannel / Reflex / UniTask 참조 |
| 이벤트 스텁 | DESIGN 6.4 이벤트 클래스 + 검증용 `Phase0ReadyEvent`. `GameEvents` static + `Init()` 패턴, `new` 금지 |
| Reflex DI | `MainEventChannel`을 `MagnetProjectInstaller`(RootScope)에서 `RegisterValue`로 컨테이너 등록 |
| 에셋 | `MainEventChannel`, `ReflexSettings`, `RootScope` prefab |
| 씬 | `Phase0_Bootstrap` — Camera, Light, SceneScope, Phase0Bootstrap |
| 팀 합의 문서 | `FOLDER_STRUCTURE.md` — 폴더·asmdef 패턴 |

### Reflex 구조 (읽는 순서)

1. `ReflexSettings.asset` → `RootScope.prefab` 참조
2. `RootScope` = `ContainerScope` + 자식 `MagnetProjectInstaller` (`MainEventChannel` 연결)
3. 씬의 `SceneScope` (`ContainerScope`) → 씬 단위 컨테이너
4. `Phase0Bootstrap` — `[Inject] EventChannelSO` 주입, `GameEvents.Phase0ReadyEvent`로 Raise

## 이 Phase 범위 밖

- 보드 격자, 자석 축, `BoardConfigSO` → **block-coordinates** 구현
- 블록·흡착·폭발·회전 등 게임플레이 전부

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| asmdef 참조 | `Scripts/Magnet.JTH.asmdef` |
| 게임 이벤트 정의 | `Scripts/Events/MagnetGameEvents.cs` |
| Reflex 등록 (SO → DI) | `Scripts/Bootstrap/MagnetProjectInstaller.cs` |
| 부트스트랩 검증 | `Scripts/Bootstrap/Phase0Bootstrap.cs` |
| 이벤트 채널 에셋 | `ScriptableObjects/MainEventChannel.asset` |
| Reflex 전역 설정 | `Resources/ReflexSettings.asset` |
| Root 바인딩 (prefab만) | `Prefabs/RootScope.prefab` |
| 진입 씬 | `Scenes/Phase0_Bootstrap.unity` |
| 팀 폴더 규칙 | `FOLDER_STRUCTURE.md` |
