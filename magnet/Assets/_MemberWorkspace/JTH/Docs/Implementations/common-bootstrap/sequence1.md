# Sequence — Phase 1 (common-bootstrap)

> **Phase:** [phase1.md](phase1.md) 와 1:1. 이 Phase에서 **뭐가 바뀌었는지** 순서대로 적는다.  
> 새 작업마다 `## N — 제목` 섹션을 아래에 추가 (파일 분리 X).

## 1 — 2026-07-06 · 최초 구현

**바뀐 것**

- 생성: `Scripts/Magnet.JTH.asmdef` — EventChannel / Reflex / UniTask 참조
- 생성: `Scripts/Events/MagnetGameEvents.cs` — 이벤트 스텁 + `Phase0ReadyEvent`
- 생성: `Scripts/Bootstrap/MagnetProjectInstaller.cs` — Reflex `RegisterValue`
- 생성: `Scripts/Bootstrap/Phase0Bootstrap.cs` — DI·이벤트 채널 검증
- 생성: `ScriptableObjects/MainEventChannel.asset`, `Resources/ReflexSettings.asset`, `Prefabs/RootScope.prefab`
- 생성: `Scenes/Phase0_Bootstrap.unity`
- 생성: `FOLDER_STRUCTURE.md` — 팀 폴더·asmdef 합의

**메모**

- 패키지 이름은 **Reflex** (`com.gustavopsantos.reflex`). CLAUDE.md 구 지칭 "Reflect"와 동일.
- `RootScope`는 **씬에 두지 않음**. prefab + ReflexSettings 등록만.
- MCP 메뉴로 `Assets/` 루트에 잘못 생긴 `ReflexSettings.asset`, `RootScope.prefab` 있으면 **삭제** (JTH 쪽 사용).
- 이 구현은 워크플로 미준수(계획 승인 없이 구현, play 모드 검증) → 이후 `CLAUDE.md` 워크플로 따름.
