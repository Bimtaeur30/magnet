# 연출 이벤트 구조 — Phase 인덱스

> **구현:** `presentation-events` · **Jira:** SCRUM-26

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 파티클 풀 인프라 (기존 GameLib.ObjectPool 재사용) | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | 완료 |
| 2 | 블록 파괴 파티클 (스킨 텍스처 랜덤 조각 + 팝업/중력 낙하) | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | 완료 |

## 관련

- `GameEvents`/`EventChannelSO` 기반 연출(파티클·SFX 등) 트리거 구조의 첫 단계. 파티클 재생만 다루며,
  실제 게임플레이 이벤트(`SquareClearedEvent` 등)에서 언제 어떤 이펙트를 재생할지 매핑하는 것은 다음 Phase.
- `Assets/GameLib/ObjectPool/`(범용 풀 + `Tools/PoolManager` 에디터)과 `Assets/_Shared/Sound/`(선례)를
  그대로 재사용하고 수정하지 않음.
