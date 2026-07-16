## 1 — 2026-07-16 · Master/BGM/SFX 볼륨 조절

**바뀐 것**
- 수정: `Assets/GameLib/SoundSystem/SoundSystemEvents.cs` — `AudioBus` enum, `SetVolumeEvent` 추가
- 수정: `Assets/GameLib/SoundSystem/SoundManager.cs` — `AudioMixer` 필드 추가, `SetVolumeEvent` 리스너 + 선형→dB 변환 추가
- 수정: `Assets/_MemberWorkspace/PMS/Scripts/Manager/SettingsFuncManager.cs` — 직접 `AudioMixer` 참조 제거, `magnetGameChannel` 이벤트 발행으로 교체. `SetMasterVolume`/`SetBgmVolume`/`SetSfxVolume(float)` 추가
- 수정: `Assets/_MemberWorkspace/PTY/Scenes/PTYScene.unity` — `SoundManager.audioMixer`를 `MainAudioMixer.mixer`에 배선

**메모**
- Play 모드에서 `magnetGameChannel.RaiseEvent(SetVolumeEvent.Init(bus, volume01))`을 Master=0.5/Bgm=0/Sfx=1로 raise해 `AudioMixer.GetFloat`로 각각 -6.02dB/-80dB/0dB 확인. 콘솔 에러 없음.
- 작업 도중 제가 만지지 않은 두 파일이 디스크에서 변경된 걸 발견함(`Assets/EckTechGames/AutoSave/Editor/AutoSaveExtension.cs`의 `using PTY.Scripts.Editor;` 한 줄, `Assets/_Shared/Magnet.Core/Events/SkinEvents.cs`의 `SkinChangedRequestEvent`/`SkinChangedResponseEvent` 추가). 둘 다 이번 구현과 무관 — IDE 미저장 편집이 AutoSave 플러그인으로 flush된 것으로 추정. 사용자 확인 후 **둘 다 현재 상태 그대로 둠**(AutoSaveExtension.cs는 Phase 1에서 이미 되돌린 채 유지, SkinEvents.cs는 건드리지 않음).
- `SettingsFuncManager.magnetGameChannel`을 실제 씬(PMS/KTJ)에 배선하는 건 이번 Phase 범위 밖 — 해당 씬 담당자가 처리해야 함.
