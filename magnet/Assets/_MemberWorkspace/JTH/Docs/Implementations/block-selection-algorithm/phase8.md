# Phase 8 — 플레이테스트 피드백 밸런싱 1차

> **구현:** block-selection-algorithm · **선행:** Phase 7 (전 티어 가동)
> **배경:** Block Blast 체감 맞추기 — 팀 밸런싱 전 1인 반복 피드백 (사용자 지시: 피드백 즉시 반영)

## 목표

1. **대각선(Diag2·Diag3) 블록 빈도 감소** — 번들·모양 가중치 하향.
2. **1x3 빈도 감소** — Normal 번들 11개 중 4개에 포함(추첨의 ~46%)이던 것을 축소.
3. **BoardHealth에 클러스터 성분 추가** — 점유 칸의 직교(대각선 제외) 연결 응집도 + 최대 덩어리 크기. 한 덩어리로 모여 있을수록, 그 덩어리가 클수록 health ↑.
4. **Normal 티어를 Health 지향으로** — 통과 가능 후보 여러 개의 "최선 플레이 후 보드 Health"를 예측해 가장 좋은 번들 선택. (응징은 Trap·Pressure 몫 — Normal은 항상 건강한 판을 준다.)
5. **쏙 맞춤(Snug Fit) 부스트** — 보드에 특정 블록이 "쏙 들어가는" 포켓(둘레가 벽·블록으로 막힌 자리)이 있으면 그 블록이 패에 포함될 확률 상승. 사방 밀폐가 최고, 위만 뚫려도 좌우하가 막히면 쏙으로 침.

## 범위 밖

- Blame 로직 변경 (클러스터는 health에만 반영)
- 번들 풀 신규 추가, Relife 게이트 연동

## 코드·에셋 맵

| 대상 | 변경 |
|------|------|
| `Health/BoardHealthCalculator.cs` | 클러스터 분석 + 성분 합산 |
| `Health/BoardHealthResult.cs` | ClusterCount·LargestClusterSize |
| `Simulation/SequenceOutcomeEstimator.cs` | FinalBoard 반환 |
| `Tiers/BundleTierSelector.cs` | TryPickCandidates (복수 후보) + 가중 배수 |
| `Generation/SnugFitScorer.cs` | 쏙 판정 (둘레 막힘 비율) — 신규 |
| `BlockSelectionOrchestrator.cs` | Normal 티어 Health 비교 선택 |
| `Data/BlockSelectionTuningSO.cs` | ClusterWeight 등 4필드 |
| `DefaultBlockSelectionTuning.asset` | 가중치 재배분 + 모양 가중치 하향 |
| `Bundles/normal_diag·corner·mix·bigL.asset` | 가중치·구성 조정 |
