# Sequence — Phase 5 (block-selection-algorithm)

> **Phase:** [phase5.md](phase5.md) 와 1:1.

## 1 — 2026-08-01 · Hospitality 실시간 생성

**바뀐 것**

- 생성: `Scripts/Domain/BlockSelection/Generation/OpportunityScorer.cs`
- 생성: `Scripts/Domain/BlockSelection/Simulation/SequenceOutcomeEstimator.cs`
- 생성: `Scripts/Domain/BlockSelection/Generation/HospitalityGenerator.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` — Hospitality 9필드

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Generation/OpportunityScorer.cs`
  - 심볼: `OpportunityScorer.Score(board, health, tuning)` — public static (추가)
    - 설명: near-line 가산 + 멀티라인 보너스 + 올클 잠재 + 큰 슬롯 성분 − dead zone 패널티 → clamp01 (SPEC §10.2).
    - 이유: 접대 게이트 입력. fillRate·deadZone·bigSlots는 `BoardHealthResult` 재사용 — 리필당 이미 계산된 값 (phase5.md 결정).
  - 심볼: `OpportunityScorer.CountNearCompleteLines(board)` — private static (추가)
    - 설명: 빈 칸이 정확히 1개인 행·열 수.
    - 이유: §10.2 nearLines 정의 — health에 없는 유일한 지표라 직접 순회.

- 파일: `Scripts/Domain/BlockSelection/Simulation/SequenceOutcomeEstimator.cs`
  - 심볼: `SequenceOutcomeEstimator.SequenceOutcome` — readonly struct (`SequenceFound`, `TotalClears`, `BoardEmptied`) (추가)
    - 설명: 최선 시나리오 추정 결과.
  - 심볼: `SequenceOutcomeEstimator.Estimate(board, pieces, beamWidth)` — public static (추가)
    - 설명: 깊이 3 빔 서치 — 각 깊이에서 (미사용 피스 × 전 피벗) 확장 후 클리어 desc·점유 asc 상위 beamWidth 유지.
    - 이유: SPEC §10.3 SimulateBestOutcome의 전수 탐색(6순열 × 위치³)은 빈 보드에서 조합 폭발 — 빔으로 상한 고정 (phase5.md 결정). 완주 경로 발견 = full sequence 존재 증명이라 별도 솔버 호출도 대체.
  - 심볼: `SequenceOutcomeEstimator.ExpandState / CountOccupied` — private static (추가)
    - 설명: 상태 확장(비트마스크 used + 보드 클론)과 점유 수 계산(동률 타이브레이크·올클 판정).

- 파일: `Scripts/Domain/BlockSelection/Generation/HospitalityGenerator.cs`
  - 심볼: `HospitalityGenerator.MaxCandidates` — const int 8 (추가)
    - 설명: 후보가 이만큼 모이면 샘플링 조기 종료.
    - 이유: 품질 가중 추첨에 충분 — sampleCount 전부 돌 필요 없음.
  - 심볼: `HospitalityGenerator.TryGenerate(board, health, pool, tuning, rng)` — public static (추가)
    - 설명: opportunity 게이트 → `ShapeSampler` 샘플 → 빔 추정 → 품질(총 클리어 + 올클 보너스 2) 하한 → 품질 가중 추첨. 실패 시 null.
    - 이유: SPEC §10.3. 변덕 확률(`p_hospitality`)은 Orchestrator 몫으로 분리 — 게이트와 생성의 관심사 분리.
    - 영향: Phase 6 Orchestrator 스택 순서 3에서 호출.
  - 심볼: `HospitalityGenerator.PickWeightedByQuality(candidates, rng)` — private static (추가)
    - 설명: quality 비례 가중 추첨 ("score 높을수록 선택", §10.3).

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `HospitalityProbability / OpportunityHighThreshold / HospitalitySampleCount / HospitalityMinQualityClears` — 프로퍼티 4종 (추가)
    - 설명: 변덕 확률(0.75) / 기회 게이트(0.7) / 샘플 수(60) / 품질 하한(2라인).
  - 심볼: `OpportunityNearLineWeight / OpportunityMultiLineBonus / OpportunityAllClearWeight / OpportunityAllClearFillMax / OpportunityDeadZonePenalty` — 프로퍼티 5종 (추가)
    - 설명: opportunityScore 성분 가중 (0.25 / 0.15 / 0.2 / 0.2 / 0.15).
  - 심볼: `OpportunityBigSlotWeight / OutcomeBeamWidth` — 프로퍼티 2종 (추가)
    - 설명: 큰 슬롯 성분 가중(0.15) / 빔 폭(4).

**검증**

- execute_code: 행 0~2를 7칸씩 채운 near-line 보드 → opportunityScore 0.97 (게이트 0.7 통과), `TryGenerate` 후보 생성 성공 ✅

**메모**

- 빔이 놓친 조합은 후보 탈락(false negative) 가능 — 샘플링 기반이라 허용, 폭은 `OutcomeBeamWidth`로 튜닝.
