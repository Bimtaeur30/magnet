# Sequence — Phase 3 (save-system)

> **Phase:** [phase3.md](phase3.md) 와 1:1.

## 1 — 2026-07-13 · 클라우드 의존 제거, 로컬 저장 실제 구현

**바뀐 것**

- 수정: `Scripts/Save/ISaveService.cs` — `ForceSyncAsync()` 제거
- 수정: `Scripts/Save/SaveService.cs` — `ICloudSaveProvider` 의존 제거, 로컬 로드/갱신/즉시저장 실제 로직 구현
- 수정: `Scripts/Save/Local/JsonFileSaveRepository.cs` — `Application.persistentDataPath/save.json` 기반 JSON 로드/저장 실제 구현
- 수정: `Scripts/Events/SaveEvents.cs` — `SaveSyncCompletedEvent`의 `CloudSyncSucceeded` 필드 제거
- 수정: `Scripts/Bootstrap/SaveInstaller.cs` — `CreateCloudProvider()`/플랫폼 분기 제거, 로컬 저장소만 `SaveService`에 주입
- 변경 없음(유지): `Scripts/Save/ICloudSaveProvider.cs`, `Scripts/Save/Cloud/GooglePlayGamesSaveProvider.cs`, `Scripts/Save/Cloud/AppleGameKitSaveProvider.cs`, `Scripts/Save/Cloud/NullCloudSaveProvider.cs`

**메모**

- 로그인 기능을 구현할 수 없어 Android/iOS 게임 센터 클라우드 저장을 붙일 수 없는 상태 → 클라이언트 로컬 저장만 사용하는 방향으로 전환.
- 클라우드 스텁 파일은 삭제하지 않고 남겨둠. 나중에 로그인이 생기면 `SaveInstaller`/`SaveService` 배선만 복원하면 되는 구조.
- `refresh_unity`(compile 요청) + `read_console` 확인 결과 PTY 관련 컴파일 에러·경고 0건.

## 2 — 2026-07-13 · SaveService 초기화 순서 버그 수정

**바뀐 것**

- 수정: `Scripts/Save/SaveService.cs` — `_data` 로드를 `InitializeAsync()`가 아니라 생성자에서 즉시 수행하도록 변경. `InitializeAsync()`는 완료된 `UniTask`만 반환

**메모**

- `SkinManager.Start()`가 `SaveService.InitializeAsync()` 호출 없이 곧바로 `UnlockSkin`을 트리거해 `_data`가 null인 상태로 접근되는 NRE 발생(제보: 콘솔 스택트레이스).
- 로컬 저장은 실제로는 동기 파일 I/O라 "누군가 InitializeAsync를 먼저 호출해줘야 한다"는 순서 의존성 자체가 불필요 — 생성자 로드로 바꿔 `SaveInstaller`가 컨테이너를 구성하는 시점(씬의 다른 `Awake`/`Start`보다 항상 먼저 실행됨)에 `_data`가 채워지도록 해 순서 문제를 원천 차단.
- 별개로 확인된 것: PTYScene에 Reflex `ContainerScope`가 없어 씬 전체 DI가 동작하지 않던 문제와, 씬에 `SaveInstaller`가 누락돼 있던 문제가 함께 있었음(사용자가 별도로 조치).
- `refresh_unity` + `read_console` 확인 결과 컴파일 에러·경고 0건.
