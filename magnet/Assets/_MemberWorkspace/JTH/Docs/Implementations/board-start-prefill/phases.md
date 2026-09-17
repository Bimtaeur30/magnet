# Board Start Prefill — Phase 인덱스

> **구현:** `board-start-prefill` · 새 게임 시작 시 8×8 보드를 미리 채운다 ("처음에 비워져 있음" QA 피드백)
> **분석·방침:** `Docs/START_BOARD_PREFILL_ANALYSIS.md`
> **이력:** `block-selection-algorithm` sequence9 #10~#15에서 넣었다 뺀 뒤 2026-09-02 재도입 결정

| Phase | 제목 | 계획 | 변경 기록 | 상태 |
|-------|------|------|-----------|------|
| 1 | 칸-확률 채움 + 번들 구멍 + 퍼펙트/올클 이벤트 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | **완료** |

## 방침 요약

- **채움:** 칸마다 독립 확률(`FillProbability` 기본 0.6). 피스 단위 배치 아님
- **구멍:** Normal 번들(작은 것) 하나를 3피스 모양대로 뚫어 시작 직후 막힘 방지
- **하한:** 구멍 뒤 빈 칸이 `MinEmptyCellsAfterHole` 미만이면 재생성
- **색:** 칸마다 랜덤 (`SkinSession.MaxVariant` 범위)
- **순서:** `PrefillBoard()` → `Fill()` — 첫 손은 채워진 보드 기준으로 뽑힘
- **끄기:** `BoardPrefillConfigSO.Enabled = false`면 빈 보드로 시작 (예전 동작)

## 동반 변경 (퍼펙트/올클리어 이벤트)

같은 커밋에 들어온 별개 연출 트리거. 파일이 겹쳐(`BlockSpawnBootstrap`·`BoardGrid`) 분리 커밋이 안 됨.

- `HandOptimalSolver` — 핸드 3피스로 낼 수 있는 최대 클리어 라인 수 완전탐색
- `MagnetGameEvents.PerfectClearEvent` / `AllClearEvent`
- 보드 점유율 `perfectSolveMinOccupancy`(기본 0.4) 미만이면 탐색 스킵 (히칭 방지)
- 독립 구현 문서가 필요해지면 별도 slug로 분리
