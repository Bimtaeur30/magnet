# Phase 3 — 클라우드 의존 제거, 로컬 저장 실제 구현

> **구현:** `save-system` · **Jira:** [SCRUM-28](https://bimtaeur30.atlassian.net/browse/SCRUM-28)
> **상태:** 완료
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 배경

로그인 기능을 구현할 수 없어 Android/iOS 게임 센터(GPGS/Game Center) 계정으로 클라우드 저장을 붙일 수 없는 상태. 클라우드 저장 없이 클라이언트(로컬)에만 저장하도록 방향을 바꿈.

## 목표 (완료 기준)

- [x] `SaveService`가 `ICloudSaveProvider` 없이 `ILocalSaveRepository`만으로 동작하도록 배선 변경
- [x] `JsonFileSaveRepository`에 실제 JSON 파일 I/O 구현 (`Application.persistentDataPath`)
- [x] `SaveService`에 실제 로드/갱신/즉시저장 로직 구현 (Phase 1·2에서 스텁으로 남겨둔 부분)
- [x] 클라우드 전용 API(`ForceSyncAsync`, `SaveSyncCompletedEvent.CloudSyncSucceeded`)는 로컬 전용 흐름에서 불필요하므로 정리
- [x] `GooglePlayGamesSaveProvider`/`AppleGameKitSaveProvider`/`NullCloudSaveProvider`/`ICloudSaveProvider` 파일은 삭제하지 않고 유지 (로그인 기능 추후 구현 시 재사용)

## 구현 내용 (뭘 어떻게)

| 클래스/인터페이스 | 변경 |
|---|---|
| `ISaveService` | `ForceSyncAsync()` 제거 (클라우드 동기화 전용 API라 로컬 전용에선 불필요) |
| `SaveService` | 생성자에서 `ICloudSaveProvider` 제거, `ILocalSaveRepository` + `EventChannelSO`만 사용. `InitializeAsync`에서 로컬 파일 로드(없으면 새 `GameSaveData`). 각 mutator(`SubmitScore`/`UnlockSkin`/`EquipSkin`/`AddPlayTime`/`SubmitExplosionCombo`/`RecordGameOver`)가 값 갱신 후 즉시 로컬 저장. `SubmitScore`/`SubmitExplosionCombo`는 기존값보다 클 때만 갱신(최고값 정책). 매 저장 시 `SaveSyncCompletedEvent` 발행, 베스트 스코어 갱신 시 `BestScoreUpdatedEvent` 발행 |
| `JsonFileSaveRepository` | `Application.persistentDataPath/save.json`에 `JsonUtility`로 실제 로드/저장 구현. 파일 없으면 `Load()`가 `null` 반환 |
| `SaveEvents.SaveSyncCompletedEvent` | `CloudSyncSucceeded` 필드 제거 (클라우드 없으니 단순 "로컬 저장 완료" 신호로 단순화) |
| `SaveInstaller` | `CreateCloudProvider()`/플랫폼 분기 제거. `JsonFileSaveRepository`만 생성해 `SaveService`에 주입 |
| `ICloudSaveProvider`, `Cloud/GooglePlayGamesSaveProvider`, `Cloud/AppleGameKitSaveProvider`, `Cloud/NullCloudSaveProvider` | **변경 없음.** 파일은 유지하되 어디서도 참조하지 않음 (미사용) |

### 합의된 설계 결정

- 클라우드 관련 스텁 파일은 삭제하지 않고 남겨둠 — 나중에 로그인 기능이 생기면 `SaveInstaller`에 플랫폼별 `ICloudSaveProvider` 생성 로직을 복원하고, `SaveService` 생성자에 다시 주입해 `InitializeAsync`에서 로컬/클라우드 병합(필드별 더 높은 값 우선, Phase 1 정책) + 각 mutator에서 클라우드 비동기 push를 추가하면 됨. 수동 동기화가 필요해지면 `ISaveService.ForceSyncAsync()`도 그때 다시 추가.
- `AddPlayTime` 등 빈번히 호출될 수 있는 mutator도 다른 메서드와 동일하게 매 호출마다 즉시 로컬 저장하는 단순한 정책을 그대로 적용함 (throttling 등은 이번 범위 밖, 실제 호출 빈도가 문제가 되면 추후 조정).

## 이 Phase 범위 밖

- 클라우드 로그인/동기화 실제 구현 (로그인 기능 자체가 없어 보류)
- 게임 오버, 콤보 발생, 플레이 타임 틱, 스킨 장착 시점에 `SaveService` 메서드를 실제로 호출하는 소비자 코드 연결
- 앱 일시정지/종료 시 flush, 파일 I/O 실패 시 재시도/에러 처리 등 견고성 작업

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 서비스 인터페이스 | `Scripts/Save/ISaveService.cs` |
| 서비스 실제 구현 | `Scripts/Save/SaveService.cs` |
| 로컬 저장 실제 구현 | `Scripts/Save/Local/JsonFileSaveRepository.cs` |
| DI 배선 | `Scripts/Bootstrap/SaveInstaller.cs` |
| 이벤트 | `Scripts/Events/SaveEvents.cs` |
| 미사용 클라우드 스텁(유지) | `Scripts/Save/ICloudSaveProvider.cs`, `Scripts/Save/Cloud/*.cs` |
