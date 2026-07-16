# 사운드 매니저 — Phase 인덱스

> **구현:** `sound-manager`

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 재생 인프라 완성 (믹서 · 프리팹 · 풀 배선) | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | 완료 |
| 2 | Master/BGM/SFX 볼륨 조절 | phase2.md (예정) | — | 예정 |
| 3 | BGM 페이드 인/아웃 (LitMotion) | phase3.md (예정) | — | 예정 |

## 관련

- 기존 `Assets/GameLib/SoundSystem/` 코드 골격(`SoundManager`/`SoundPlayer`/`SoundClipSO`/`SoundSystemEvents`)은 이미 있었고, Phase 1은 거기 빠져있던 AudioMixer·프리팹·풀 등록 에셋만 채웠다. 코드 자체는 수정하지 않음.
- Phase 2에서 볼륨 조절을 위해 `SoundManager`가 `AudioMixer`를 직접 소유하도록 하고, `Assets/_MemberWorkspace/PMS/Scripts/Manager/SettingsFuncManager.cs`(다른 멤버 소유 파일)를 최소 수정할 예정 — 사용자 사전 승인됨.
- 실제 BGM/SFX 오디오 리소스는 아직 없음(더미 클립만 존재) — 팀이 리소스를 확보하면 `SoundClipSO` 에셋을 추가로 만들어 채워야 함.
