# Phase 37 — MaxArea 랭킹 비용 축소 (빔 근사 + top-K 정밀화)

## 목표

패 선택 Select가 100~450ms로 렉을 내던 병목 `MaxAreaAfterFullSequence`(후보 전원 DFS)를 줄인다.

## 설계

1. **전 후보:** `SequenceOutcomeEstimator` 완주 빔 → `FinalBoard`에 `ScoreTotal` = predicted Area 근사
2. **상위 K만:** `maxAreaRefineTopK`(기본 4)에 대해 MaxArea 정밀화
3. **Clean 체인 afterBest:** MaxArea 보드 대신 빔 `FinalBoard`
4. **Hospitality / Easy:** 동일 패턴 (접대는 우승 1개만 MaxArea refine)

## 수용 기준

- Normal Select에서 `ScoreSurvivors.MaxArea` 전후보 합이 사라지거나 `MaxAreaRefine topK`만 남음
- Select total이 대개 **~50ms 이하** 수준으로 체감 개선 (보드·후보에 따라 변동)
- 라인클리어≥1 · Death 배제 · Clean 체인 게이트 동작 유지
