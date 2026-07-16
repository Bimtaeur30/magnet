# Phase 1 — 재생 인프라 완성 (믹서 · 프리팹 · 풀 배선)

> **구현:** `sound-manager`
> **상태:** 완료
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 배경

`Assets/GameLib/SoundSystem/`에 `SoundManager`/`SoundPlayer`/`SoundClipSO`/`SoundSystemEvents` 코드 골격은 이미 있었으나, AudioMixer 에셋·SoundPlayer 프리팹·PoolItemSO/PoolManagerSO 등록이 전부 없어 이벤트를 던져도 아무 소리도 나지 않는 상태였다. 이번 Phase는 재생 파이프라인을 실제로 동작하게 채우는 것이 목표.

## 목표 (완료 기준)

- [x] `AudioMixer` 에셋 생성: Master → Bgm, Sfx 그룹. 노출 파라미터 `MasterVolume`/`BGMVolume`/`SFXVolume` (PMS `SettingsFuncManager.cs`가 이미 참조하는 이름과 일치시킴)
- [x] `SoundPlayer` 프리팹 생성, `sfxGroup`/`musicGroup`을 Sfx/Bgm 그룹에 연결
- [x] `PoolItemSO`(SoundPlayer) 생성 후 기존 공용 `PoolManagerSO`(`PoolManager.asset`)에 등록 — 파티클 풀과 같은 매니저를 재사용(별도 PoolManagerSO 만들지 않음)
- [x] `PTYScene.unity`에 `SoundManager` GameObject 배치, `poolManagerSO`/`soundItemSO`/`SoundEventChannel` 필드를 각각 `PoolManager.asset`/`SoundPlayer.asset`/`MagnetGameChannel.asset`(공용 `magnetGameChannel`)에 배선
- [x] 테스트용 더미 `SoundClipSO` 2개(`TestBgmClip`/`TestSfxClip`, `audioClip` 없음) 생성 후 Play 모드에서 `PlaySoundEvent` 라운드트립 검증 — 콘솔에 `IsBgm` True/False 출력 확인, 예외 없음

## 구현 내용 (뭘 어떻게)

| 에셋/오브젝트 | 위치 | 설명 |
|---|---|---|
| `MainAudioMixer.mixer` | `Assets/GameLib/SoundSystem/` | Master 그룹 산하 Bgm/Sfx 자식 그룹. 각 그룹 Volume을 `MasterVolume`/`BGMVolume`/`SFXVolume`로 노출 |
| `SoundPlayer.prefab` | `Assets/GameLib/SoundSystem/Prefabs/` | `AudioSource`+`SoundPlayer` 컴포넌트. `sfxGroup`→Sfx, `musicGroup`→Bgm 배선 |
| `SoundPlayer.asset` (PoolItemSO) | `Assets/GameLib/ObjectPool/Items/` | `prefab`=SoundPlayer.prefab, `initCount`=8 |
| `PoolManager.asset` | `Assets/GameLib/ObjectPool/` | 기존 자산 — `itemList`에 SoundPlayer 항목만 추가(수정) |
| `TestBgmClip.asset`, `TestSfxClip.asset` | `Assets/GameLib/SoundSystem/Data/` | 더미 `SoundClipSO` — `audioClip` 없음, 파이프라인 검증 전용. 실제 오디오 리소스는 팀이 추후 채움 |
| `SoundManager` GameObject | `PTYScene.unity` | `poolManagerSO`/`soundItemSO`/`SoundEventChannel` 배선 완료. 기존 `PoolInitializer`가 같은 `PoolManager.asset`을 초기화하므로 별도 배선 불필요 |

### AudioMixer 생성 방식 (참고)

`UnityEditor.Audio.AudioMixerController`가 `internal` 클래스라 일반 스크립트에서 직접 참조 불가. 리플렉션으로 `CreateMixerControllerAtPath`/`CreateNewGroup`/`AddChildToParent`/`AddExposedParameter`를 호출해 1회성으로 생성함(스크립트로 남기지 않음). 이후 그룹/노출 파라미터를 추가로 바꿔야 하면 Unity 에디터의 Audio Mixer 창에서 수동으로 하거나 동일한 리플렉션 스니펫을 재사용해야 함.

## 이 Phase 범위 밖

- 실제 BGM/SFX 오디오 리소스 연결 (`TestBgmClip`/`TestSfxClip`은 더미)
- Master/BGM/SFX 볼륨 조절 API·이벤트 — [Phase 2](phase2.md)
- BGM 페이드 인/아웃 (LitMotion) — Phase 3
- `SettingsFuncManager.cs`(PMS) 수정 — Phase 2에서 볼륨 이벤트 연동 시 처리
- JTH `Phase0_Bootstrap.unity`, KTJ 씬 등 다른 멤버 씬 통합 — 각 씬 담당자가 `SoundManager`+`PoolInitializer`를 직접 배치해야 함 (본 Phase는 PTY 소유 `PTYScene.unity`에서만 검증)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 매니저/플레이어/이벤트 (기존 코드, 미수정) | `Assets/GameLib/SoundSystem/SoundManager.cs`, `SoundPlayer.cs`, `SoundClipSo.cs`, `SoundSystemEvents.cs` |
| 새 에셋 | `Assets/GameLib/SoundSystem/MainAudioMixer.mixer`, `Prefabs/SoundPlayer.prefab`, `Data/TestBgmClip.asset`, `Data/TestSfxClip.asset` |
| 풀 등록 | `Assets/GameLib/ObjectPool/Items/SoundPlayer.asset`, `Assets/GameLib/ObjectPool/PoolManager.asset` |
| 씬 배선 | `Assets/_MemberWorkspace/PTY/Scenes/PTYScene.unity` (`SoundManager` GameObject) |
