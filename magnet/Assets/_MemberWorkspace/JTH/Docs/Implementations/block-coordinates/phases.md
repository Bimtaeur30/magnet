# 블록 좌표·보드 격자 — Phase 인덱스

> **구현:** `block-coordinates` · **Jira:** [SCRUM-17](https://bimtaeur30.atlassian.net/browse/SCRUM-17) · **마일스톤:** M1  
> **v0.7:** 8×8 고정, **자석 축 `(0,0)` 폐기**. v0.6 phase1~2는 magnet 중심 좌표 — Phase 3에서 마이그레이션.

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | BoardConfigSO·좌표·격자 렌더 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | v0.6 · deprecated 방향 |
| 2 | Board Transform을 보드 공간 원점으로 | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | v0.6 · 유지 |
| 3 | **Block Blast 8×8·자석 제거** | [phase3.md](phase3.md) | — | **계획** |
