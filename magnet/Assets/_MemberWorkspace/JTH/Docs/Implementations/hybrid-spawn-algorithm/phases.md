# 하이브리드 스폰 알고리즘 — Phase 인덱스

> **구현:** `hybrid-spawn-algorithm` · **Jira:** — · **대체 대상:** `blockblast-handoff-algorithm`(계승) + `block-selection-algorithm`(특수 티어 부활)
> **골격 (grill 확정 2026-08-02):** BlockBlast 핸드오프 체인(7 → 1370 근사 → 반복 억제)이 **기본 공급기**, 그 위에 구 티어 스택의 특수 티어 5종(Relife·Trap·ComboBreak·Hospitality·Pressure)을 게이트로 얹는다.

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 골격 + Hospitality·Pressure 42-ID 이식 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | **완료** |
| 2 | Relife·Trap·ComboBreak 실시간 전환 | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | **완료** |

**grill 확정 사항 (2026-08-02):**

- 병합 골격: 핸드오프 체인을 Normal/Fallback 자리에, 특수 티어를 게이트로 (선택지 a)
- 블록 체계: **42-ID로 통일** — 특수 티어 생성기도 42-ID 풀 샘플링, 스폰 회전 제거 (선택지 a)
- 티어: 5종 유지, **Easy·Fallback 제거** (핸드오프 체인이 대체). Momentum도 제외 — 번들 의존 + 1370의 라인 클리어 선호가 부분 대체
- 번들 SO 체계 **폐기** — 세 번들 티어 모두 "42-ID 풀 샘플링 + 솔버 필터" 실시간 생성
- 특수 티어 출력은 반복 억제 트레이트(2100·delCurrentSameBlock) **우회 + 히스토리 기록**, 생성기 샘플링에서 직전 트리플 회피
- 새 오케스트레이터 + 새 Drawer, Bootstrap 배선만 교체 (구 코드 2벌 롤백용 보존)
- 새 `HybridTuningSO` (생존 필드 + 42-ID 칸 수 가중표), 구 SO 보존
- `IsRetrySession`은 스텁(false) — game-over/다시 하기 구현 후 배선 (Relife 게이트)

**남은 일:** `IsRetrySession` 배선(Relife 개방) · GoodTurn/`MatchesStep` 이벤트 발행(UI 계약 후) — 구 phase7과 동일.
