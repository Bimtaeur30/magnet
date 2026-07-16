# Phase 2 — Master/BGM/SFX 볼륨 조절

> **구현:** `sound-manager`
> **상태:** 완료
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] `SetVolumeEvent`(버스: `AudioBus.Master`/`Bgm`/`Sfx`, `Volume01` 0~1) 추가
- [x] `SoundManager`가 `AudioMixer`를 `[SerializeField]`로 소유하고 `SetVolumeEvent`를 구독해 선형(0~1)→dB 변환 후 `SetFloat` 적용 — 믹서 제어를 한 곳(`SoundManager`)에 집중
- [x] `Assets/_MemberWorkspace/PMS/Scripts/Manager/SettingsFuncManager.cs` 수정: 직접 들고 있던 `AudioMixer` 필드·`SetMixerVolume` 제거, `magnetGameChannel.RaiseEvent(SetVolumeEvent...)` 호출로 교체. 기존 `ToggleBgm`/`ToggleSfx`(On/Off)는 유지하되 내부적으로 0/1 볼륨 이벤트로 처리. 슬라이더용 `SetMasterVolume`/`SetBgmVolume`/`SetSfxVolume(float)`도 추가
- [x] `PTYScene.unity`의 `SoundManager`에 `MainAudioMixer.mixer` 배선
- [x] Play 모드에서 `SetVolumeEvent` 라운드트립 검증: Master 0.5→-6.02dB, Bgm 0→-80dB, Sfx 1→0dB — 예외 없음

## 구현 내용 (뭘 어떻게)

| 클래스/파일 | 위치 | 변경 |
|---|---|---|
| `SoundSystemEvents.cs` | `Assets/GameLib/SoundSystem/` | `AudioBus` enum(Master/Bgm/Sfx), `SetVolumeEvent`(Bus, Volume01) 추가 |
| `SoundManager.cs` | `Assets/GameLib/SoundSystem/` | `[SerializeField] AudioMixer audioMixer` 추가, `SetVolumeEvent` 리스너(`HandleSetVolumeEvent`) + 선형→dB 변환(`LinearToDecibel`, 0→-80dB 무음 처리) 추가. 파라미터 이름 상수 `MasterVolumeParam`/`BgmVolumeParam`/`SfxVolumeParam`은 기존 `SettingsFuncManager.cs`가 쓰던 이름(`MasterVolume`/`BGMVolume`/`SFXVolume`)과 동일하게 맞춤 |
| `SettingsFuncManager.cs` (PMS) | `Assets/_MemberWorkspace/PMS/Scripts/Manager/` | `AudioMixer audioMixer` 필드·`BgmMixerParameter`/`SfxMixerParameter` 상수·`SetMixerVolume` 헬퍼 제거. `[SerializeField] EventChannelSO magnetGameChannel` 추가. `ToggleBgm`/`ToggleSfx`는 내부적으로 `SetBgmVolume`/`SetSfxVolume` 호출로 변경. `SetMasterVolume`/`SetBgmVolume`/`SetSfxVolume(float volume01)` 신규 공개 메서드 추가(슬라이더 UI용) |
| `PTYScene.unity` | `Assets/_MemberWorkspace/PTY/Scenes/` | `SoundManager`의 `audioMixer` 필드를 `MainAudioMixer.mixer`에 배선 |

### 설계 결정

- 볼륨 조절은 **이벤트로만** 요청한다(`magnetGameChannel.RaiseEvent(SetVolumeEvent...)`) — 팀 규칙("객체 간 통신은 `EventChannelSO`만")을 따름. UI(슬라이더/토글) → 이벤트 → `SoundManager`가 유일하게 `AudioMixer`를 소유하고 적용.
- 무음 기준은 Unity 믹서 최소값인 **-80dB**로 통일(기존 `SettingsFuncManager`의 On/Off 토글이 쓰던 값과 동일).

## 이 Phase 범위 밖

- BGM 페이드 인/아웃 (LitMotion) — Phase 3
- 볼륨 값 영속화(PlayerPrefs 등) — 요청받지 않아 미구현. 지금은 씬 재시작 시 믹서 기본값(0dB)으로 리셋됨
- `SettingsFuncManager.cs`의 `magnetGameChannel` 필드를 실제 씬(PMS `Secene/SampleScene.unity`, KTJ 씬 등)에 배선하는 작업 — 해당 씬 담당자 몫. 이번 Phase는 코드만 정리하고 PTY 소유 `PTYScene.unity`에서 이벤트 라운드트립만 검증함

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 볼륨 이벤트 정의 | `Assets/GameLib/SoundSystem/SoundSystemEvents.cs` |
| 믹서 적용 로직 | `Assets/GameLib/SoundSystem/SoundManager.cs` (`HandleSetVolumeEvent`, `LinearToDecibel`) |
| UI 연동 메서드 | `Assets/_MemberWorkspace/PMS/Scripts/Manager/SettingsFuncManager.cs` |
