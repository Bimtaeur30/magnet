# Relife — Phase 인덱스

> **구현:** `relife` · 게임오버 시 점수 조건 Easy 패 이어하기
> **grill 확정:** 2026-08-16

| Phase | 제목 | 계획 | 변경 기록 | 상태 |
|-------|------|------|-----------|------|
| 1 | Relife 오퍼·수락 이벤트 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | **완료** |

## grill 확정 요약

- **트리거:** 3슬롯 전부 배치 불가 + `TotalScore ≥ n` + 세션 미사용
- **n:** `ScoreConfigSO.RelifeMinScore` (기본 100). 목표 점수 없음
- **오퍼:** 현재 보드로 Easy 셀 오프셋 3개 미리 뽑음. `RelifeOfferedEvent`만. `GameOverEvent` 안 쏨
- **수락:** `RelifeAcceptedEvent` → 뽑아 둔 패를 슬롯에 넣고 이어하기. 이후 리필은 일반 스폰
- **거절:** `RelifeDeclinedEvent` 없음. UI가 `GameOverEvent` 후 재시작
- **횟수:** 세션당 1회 (수락 시 소모)
