# 랜덤 블록 생성 — Phase 인덱스

> **구현:** `random-block-spawn` · **Jira:** [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) · **마일스톤 참고:** M2

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | IBlockShape 계약·임시 데이터 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | 완료 |
| 2 | 균등 랜덤 추첨 (순수 로직) | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | 완료 |
| 3 | 3후보 공급 (`BlockSupply`) | [phase3.md](phase3.md) | [sequence3.md](sequence3.md) | 완료 |
| ~~4~~ | ~~가중치 추첨~~ | — | [sequence4.md](sequence4.md) | **취소** (불필요) |

**PTY (팀 부재 시 JTH 대리 구현):** `BlockShapeSourceSO` + `BlockShapeSourceInstaller` — `IBlockShapeSource` 등록·`RootScope` 배선 완료.

**범위 밖 (SCRUM-25 / PTY):** BlockShapeSO 에셋·블록 생성 에디터
