# Phase 5 — Hospitality 실시간 생성

> **구현:** `block-selection-algorithm` · **Sequence:** [sequence5.md](sequence5.md) · **스펙:** [SPEC.md](SPEC.md) §10

## 목표

보드가 "예쁘게 모인" 강한 기회일 때만, 완벽 플레이 시 클리어가 충분한 3피스 조합을 실시간 생성. 억지 조합·억지 올클은 버린다.

## 완료 조건

- [x] `OpportunityScorer` — near-line·멀티라인·올클 잠재·큰 슬롯·dead zone 패널티 → 0~1
- [x] `SequenceOutcomeEstimator` — 빔 서치로 최대 클리어 시나리오 추정 (SimulateBestOutcome)
- [x] `HospitalityGenerator.TryGenerate` — 기회 게이트 → 샘플링 → 품질 하한 → 품질 가중 추첨
- [x] Hospitality 튜닝 필드 9종 (`HospitalityProbability`, `OpportunityHighThreshold`, sampleCount, 품질 하한, opportunity 가중 5종, `OutcomeBeamWidth`)
- [x] 검증: near-line 보드(행 3개 × 7칸 채움) → opportunityScore 0.97, 후보 생성 성공

## 설계 결정

| 결정 | 이유 |
|------|------|
| SimulateBestOutcome은 전수 탐색 대신 **빔 서치** (기본 폭 4) | 빈 보드에서 6순열 × 위치 전수는 조합 폭발(수십만 시뮬) — 클리어 많은 순 상위 상태만 유지 |
| `FullSequenceExists` 별도 호출 없이 빔 완주 여부로 대체 | 빔이 완주 경로를 찾으면 존재 증명 — 솔버 중복 호출 제거. 빔이 놓친 조합은 후보 탈락(샘플링이라 허용) |
| 올클리어는 품질에 +2라인 보너스 | 라인 수만으로는 올클 가치 미반영 — §10.2 all-clear 우대와 일치 |
| fillRate·deadZone·bigSlots는 `BoardHealthResult` 재사용 | Orchestrator가 매 리필 이미 계산 — 중복 순회 제거. near-line만 자체 계산 |
| 후보 8개 모이면 샘플링 조기 종료 | 품질 가중 추첨에 충분 — sampleCount 전부 돌 필요 없음 |

## 만진 파일

- `Scripts/Domain/BlockSelection/Generation/OpportunityScorer.cs` (신규)
- `Scripts/Domain/BlockSelection/Simulation/SequenceOutcomeEstimator.cs` (신규)
- `Scripts/Domain/BlockSelection/Generation/HospitalityGenerator.cs` (신규)
- `Scripts/Data/BlockSelectionTuningSO.cs` (수정 — Hospitality 9필드)

## 범위 밖

변덕 확률 판정(Orchestrator, Phase 6), Pressure(Phase 6)
