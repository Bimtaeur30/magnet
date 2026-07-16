## 1 — 2026-07-16 · 재생 인프라 완성 (믹서 · 프리팹 · 풀 배선)

**바뀐 것**
- 생성: `Assets/GameLib/SoundSystem/MainAudioMixer.mixer` — Master/Bgm/Sfx 그룹, `MasterVolume`/`BGMVolume`/`SFXVolume` 노출 파라미터
- 생성: `Assets/GameLib/SoundSystem/Prefabs/SoundPlayer.prefab` — `AudioSource`+`SoundPlayer`, `sfxGroup`=Sfx, `musicGroup`=Bgm
- 생성: `Assets/GameLib/ObjectPool/Items/SoundPlayer.asset` (PoolItemSO, `initCount`=8)
- 수정: `Assets/GameLib/ObjectPool/PoolManager.asset` — `itemList`에 SoundPlayer 항목 추가 (기존 BlockBlast 항목과 공존)
- 생성: `Assets/GameLib/SoundSystem/Data/TestBgmClip.asset`, `TestSfxClip.asset` — 더미 `SoundClipSO` (audioClip 없음, 파이프라인 검증용)
- 수정: `Assets/_MemberWorkspace/PTY/Scenes/PTYScene.unity` — `SoundManager` GameObject 추가, `poolManagerSO`/`soundItemSO`/`SoundEventChannel` 배선(각각 `PoolManager.asset`/`SoundPlayer.asset`/`MagnetGameChannel.asset`)

**메모**
- AudioMixer 그룹 생성·파라미터 노출은 `UnityEditor.Audio.AudioMixerController`가 internal이라 리플렉션으로 1회성 실행(Editor 스크립트로 남기지 않음). 향후 그룹/파라미터를 더 추가하려면 Unity Audio Mixer 창에서 수동으로 하거나 동일한 리플렉션 방식을 다시 써야 함.
- Play 모드에서 `magnetGameChannel.RaiseEvent(PlaySoundEvent.Init(TestBgmClip))` / `PlaySoundEvent.Init(TestSfxClip, TestSfxClip)`를 실행해 `SoundManager.HandlePlaySoundEvent`가 `IsBgm`을 정확히 분기(콘솔 True/False)하는 것을 확인. 클립에 실제 `audioClip`이 없어 `PlayBgm`/`PlayLoopOrOneShot`은 조기 반환 — 실제 재생·풀 pop/push까지는 이번 Phase에서 검증되지 않음(실제 오디오 리소스 투입 후 확인 필요).
- `Assets/_MemberWorkspace/PMS/Scripts/Manager/SettingsFuncManager.cs`는 이번 Phase에서 수정하지 않음 — Phase 2에서 볼륨 이벤트 연동 시 수정 예정(사용자 사전 승인됨).
- `SoundManager`는 기존 공용 `PoolManagerSO`(`PoolManager.asset`, 파티클 이펙트와 공유)를 그대로 재사용 — 별도 PoolManagerSO를 만들지 않음.
