# 블록 선택 알고리즘 — Phase 인덱스

> **구현:** `block-selection-algorithm` · **Jira:** — · **마일스톤:** DESIGN §4.9 (스폰 알고리즘 TBD → 본 구현)  
> **스펙:** [SPEC.md](SPEC.md) — grill 확정 설계 (BoardHealth + BlameScore, 티어 스택, 번들 vs 실시간)  
> **전제:** `random-block-spawn`과 독립으로 Domain부터. Phase 7에서 `AbstractDrawer` 교체·연동.

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 솔버 (배치 시뮬 + 4종 판정) | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | **완료** |
| 2 | BoardHealth + BlameTracker + TuningSO | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | **완료** |
| 3 | 블록 가중치 + 번들 SO (Relife 포함) | [phase3.md](phase3.md) | [sequence3.md](sequence3.md) | **완료** |
| 4 | 번들 티어 셀렉터 (Relife·Trap·ComboBreak·Easy·Normal) | [phase4.md](phase4.md) | [sequence4.md](sequence4.md) | **완료** |
| 5 | Hospitality 실시간 생성 | [phase5.md](phase5.md) | [sequence5.md](sequence5.md) | **완료** |
| 6 | Pressure(유일수) + Orchestrator + UniqueSolution 보관 | [phase6.md](phase6.md) | [sequence6.md](sequence6.md) | **완료** |
| 7 | Drawer 연동·GoodTurn/정답 매칭 데이터·로그 | [phase7.md](phase7.md) | [sequence7.md](sequence7.md) | **완료** |

**의존:** 1 → 2 → 3 → 4 → 5 → 6 → 7 (직렬)

**UI 데이터 계약 (SPEC §4.5):** GoodTurn(턴 Blame delta) → Phase 2·7 / 유일수 정답 배치(엄지척) → Phase 6·7.

**남은 일 (전 Phase 완료 후):** `IsRetrySession` 연동(Relife 게이트 개방) · GoodTurn/`MatchesStep` 이벤트 발행(UI 계약 후) — [phase7.md](phase7.md) 참고.
