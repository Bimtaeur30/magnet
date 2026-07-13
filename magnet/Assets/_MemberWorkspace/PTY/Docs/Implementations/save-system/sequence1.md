# Sequence — Phase 1 (save-system)

> **Phase:** [phase1.md](phase1.md) 와 1:1.

## 1 — 2026-07-12 · 저장 구조 뼈대 최초 구현

**바뀐 것**

- 생성: `Scripts/Save/GameSaveData.cs`
- 생성: `Scripts/Save/ILocalSaveRepository.cs`
- 생성: `Scripts/Save/ICloudSaveProvider.cs`
- 생성: `Scripts/Save/ISaveService.cs`
- 생성: `Scripts/Save/SaveService.cs` (스텁)
- 생성: `Scripts/Save/Local/JsonFileSaveRepository.cs` (스텁)
- 생성: `Scripts/Save/Cloud/GooglePlayGamesSaveProvider.cs` (스텁)
- 생성: `Scripts/Save/Cloud/AppleGameKitSaveProvider.cs` (스텁)
- 생성: `Scripts/Save/Cloud/NullCloudSaveProvider.cs` (완전 구현, 폴백)
- 생성: `Scripts/Bootstrap/SaveInstaller.cs` (완전 구현, DI 배선)
- 생성: `Scripts/Events/SaveEvents.cs` (완전 구현)
- 수정: `Scripts/Magnet.PTY.asmdef` — `EventChannel_Assembly`, `UniTask` 참조 추가

**메모**

- 실제 저장 기능(JSON I/O, GPGS/Game Center 연동, 병합 로직)은 다른 담당자가 이어서 구현할 예정이라 전부 `NotImplementedException` 스텁으로 남김.
- `SaveInstaller`는 아직 어떤 씬에도 배치하지 않음. 씬에 배치하고 `magnetGameChannel` SO를 연결하는 것은 범위 밖.
- `refresh_unity`(compile 요청) + `read_console` 확인 결과 컴파일 에러·경고 0건.

## 2 — 2026-07-12 · 스테이지 개념 제거, 저장 범위를 베스트 스코어 + 언락 스킨으로 축소

**바뀐 것**

- 수정: `Scripts/Save/GameSaveData.cs` — `ClearedStageIndex` 제거, `UnlockedItemIds` → `UnlockedSkinIds`
- 수정: `Scripts/Save/ISaveService.cs` — `ClearedStageIndex` 제거, `SubmitStageCleared` 제거, `UnlockItem` → `UnlockSkin`
- 수정: `Scripts/Save/SaveService.cs` — 위 인터페이스 변경 반영

**메모**

- 게임 자체가 스테이지 형식이 아니라는 확인에 따라 스테이지 관련 필드/메서드를 전부 제거함.
- 언락 대상은 스킨뿐이라 필드명을 `UnlockedSkinIds`/`UnlockSkin(skinId)`로 명확히 함 (기존 `SkinUnlockedEvent.SkinId`와 명명 일치).
- 다른 저장 항목(광고 제거 여부, 튜토리얼 완료 등)은 필요해지면 `GameSaveData`에 필드만 추가하면 되는 구조.
- `refresh_unity` + `read_console` 재확인 — PTY 관련 컴파일 에러 없음(무관한 KTJ `SkinBox_UI.cs` 기존 오류 1건은 그대로 존재, 미수정).
