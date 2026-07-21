# 랜덤 블록 생성 — Phase 인덱스

> **구현:** `random-block-spawn` · **Jira:** [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) · **마일스톤:** M2  
> **v0.7:** **3슬롯**, **BlockBlastPoolSO** (Block Blast 표준 8종). PTY `BlockShapeSourceSO` 인게임 미사용.  
> **스폰 알고리즘** (보드 상태·난이도): DESIGN §4.9 **TBD** — 본 구현 범위 밖.

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | IBlockShape 계약·임시 데이터 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | 완료 |
| 2 | 균등 랜덤 추첨 (순수 로직) | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | 완료 · Phase 6에서 Pool 교체 |
| 3 | 3후보 공급 (`BlockSupply`) | [phase3.md](phase3.md) | [sequence3.md](sequence3.md) | 완료 |
| ~~4~~ | ~~가중치 추첨~~ | — | [sequence4.md](sequence4.md) | **취소** |
| 5 | 4슬롯·턴(핸드 소진) | [phase5.md](phase5.md) | [sequence5.md](sequence5.md) | v0.6 · **3슬롯으로 수정** |
| 6 | **BlockBlastPoolSO + 3슬롯 확정** | [phase6.md](phase6.md) | — | **계획** |

**PTY (SCRUM-25):** BlockShape 에디터 — 인게임 풀과 **분리**. JTH는 `BlockBlastPoolSO`만 사용.
