# Phase 1 — 구조 (로컬/클라우드 세이브 뼈대)

> **구현:** `save-system` · **Jira:** [SCRUM-28](https://bimtaeur30.atlassian.net/browse/SCRUM-28)
> **상태:** 완료
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] 베스트 스코어 + 언락 스킨을 담는 `GameSaveData` 모델 정의 (게임이 스테이지 형식이 아니므로 스테이지 개념은 두지 않음, 이후 다른 저장 항목 추가 예정)
- [x] 로컬 저장(`ILocalSaveRepository`)과 클라우드 저장(`ICloudSaveProvider`) 인터페이스 분리
- [x] Android(GPGS) / iOS(Game Center) / 미지원 플랫폼(Null)용 `ICloudSaveProvider` 구현 자리(스텁) 생성
- [x] `ISaveService`/`SaveService`로 로컬·클라우드를 오케스트레이션하는 퍼사드 자리 마련
- [x] Reflex DI로 `ISaveService` 등록하는 `SaveInstaller` 배선
- [x] 저장 관련 이벤트(`BestScoreUpdatedEvent`, `SaveSyncCompletedEvent`) 정의

**이번 Phase는 구조만 잡는다.** 실제 파일 I/O, GPGS/Game Center API 호출, 로컬-클라우드 충돌 병합(합의된 정책: 필드별 더 높은 값 우선) 로직은 전부 `NotImplementedException`으로 남겨 다음 담당자가 채운다.

## 구현 내용 (뭘 어떻게)

| 클래스/인터페이스 | 위치 | 책임 |
|---|---|---|
| `GameSaveData` | `Scripts/Save/` | 저장 데이터 모델. `SchemaVersion`, `BestScore`, `UnlockedSkinIds` |
| `ILocalSaveRepository` | `Scripts/Save/` | 로컬(기기 내) 저장 계약 |
| `ICloudSaveProvider` | `Scripts/Save/` | 클라우드 저장 계약 (로그인 여부, 로드, 저장) |
| `ISaveService` | `Scripts/Save/` | 소비자(UI 등)가 `[Inject]`로 쓰는 퍼사드 계약 |
| `JsonFileSaveRepository` | `Scripts/Save/Local/` | **스텁.** `Application.persistentDataPath`에 JSON으로 읽고/쓰도록 채울 자리 |
| `GooglePlayGamesSaveProvider` | `Scripts/Save/Cloud/` | **스텁 (Android).** 기존 벤더링된 `Assets/GooglePlayGames`(`PlayGamesPlatform`, `ISavedGameClient`)로 채울 자리 |
| `AppleGameKitSaveProvider` | `Scripts/Save/Cloud/` | **스텁 (iOS).** Apple 공식 GameKit Unity 플러그인(미설치, 선행 필요) 연동 자리 |
| `NullCloudSaveProvider` | `Scripts/Save/Cloud/` | **완전 구현.** 에디터 등 미지원 환경 폴백 — 항상 미인증, 로컬 저장만 사용하게 함 |
| `SaveService` | `Scripts/Save/` | **스텁.** 로컬/클라우드 조합, 초기 동기화, 필드별 최고값 우선 병합, 이벤트 발행을 채울 자리 |
| `SaveInstaller` | `Scripts/Bootstrap/` | **완전 구현.** 플랫폼별(`#if UNITY_ANDROID` / `UNITY_IOS`) `ICloudSaveProvider` 선택 후 `ISaveService`를 Reflex에 `RegisterValue` |
| `SaveEvents` (`BestScoreUpdatedEvent`, `SaveSyncCompletedEvent`) | `Scripts/Events/` | **완전 구현.** JTH 소유 `MagnetGameEvents.cs`와 별도 파일, 같은 `magnetGameChannel`에서 raise 가능 |

### asmdef

- `Magnet.PTY.asmdef`에 참조 추가: `EventChannel_Assembly`(`GUID:4f4fe35fbc82e694093dc30123d90eb6`), `UniTask`(`GUID:f51ebe6a0ceec4240a699833d6309b23`) — 기존 `Magnet.Contracts`, `Reflex` 참조는 유지

### 합의된 설계 결정 (구현 시 반영해야 할 정책)

- iOS 연동: Apple 공식 GameKit Unity 플러그인 (Unity 내장 `SocialPlatforms` 아님)
- 저장 범위: 베스트 스코어 + 언락 스킨 (스테이지 개념 없음. 이후 다른 저장 항목은 `GameSaveData`에 필드 추가로 확장 — [Phase 2](phase2.md)에서 플레이 통계·장착 스킨 추가됨)
- 로컬 포맷: JSON 파일 (`persistentDataPath`)
- 충돌 해결: 로컬/클라우드 값이 다르면 필드별로 더 높은 값 우선, 병합 후 양쪽에 재저장

## 이 Phase 범위 밖

- `JsonFileSaveRepository`, `GooglePlayGamesSaveProvider`, `AppleGameKitSaveProvider`, `SaveService`의 실제 로직 구현
- Apple GameKit Unity 패키지 설치
- `SaveInstaller`를 실제 씬(`MagnetSceneInstaller` 등)에 배치하고 `magnetGameChannel` 에셋 연결
- 앱 일시정지/종료 시 flush, 재시도/에러 처리 등 견고성 작업

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 데이터 모델 | `Scripts/Save/GameSaveData.cs` |
| 로컬/클라우드/서비스 인터페이스 | `Scripts/Save/ILocalSaveRepository.cs`, `ICloudSaveProvider.cs`, `ISaveService.cs` |
| 로컬 저장 스텁 | `Scripts/Save/Local/JsonFileSaveRepository.cs` |
| 클라우드 저장 스텁/폴백 | `Scripts/Save/Cloud/GooglePlayGamesSaveProvider.cs`, `AppleGameKitSaveProvider.cs`, `NullCloudSaveProvider.cs` |
| 오케스트레이션 스텁 | `Scripts/Save/SaveService.cs` |
| DI 배선 | `Scripts/Bootstrap/SaveInstaller.cs` |
| 이벤트 | `Scripts/Events/SaveEvents.cs` |
