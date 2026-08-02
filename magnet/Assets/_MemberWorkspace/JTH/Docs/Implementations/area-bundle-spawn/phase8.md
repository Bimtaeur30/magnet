# Phase 8 — size/변 + 직사각 개수 합산

## 목표

구 4-연결 Area 점수(size·변)를 복구하고, Phase 7 직사각 greedy 개수를  
`−k × rectCount` 항으로 합산한다. (`k = rectCountPenalty`, 기본 5)

## 계획

1. `AreaScoreTuning` 복구 + `rectCountPenalty`
2. `AreaScoreCalculator` — flood base + greedy rectCount → `Total = base − k·count`
3. 풀 에셋 블렌드 수치 + k=5, `uniqueAreaThreshold=-5`

## 비범위

- cascade 규칙 변경
- 직사각 분할 알고리즘 변경
