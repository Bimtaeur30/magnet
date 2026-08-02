# Phase 7 — 직사각 greedy Area 점수

## 목표

4-연결 flood + size/변 점수식을 폐기하고, 찬·빈 마스크를 **최대면적 직사각 반복 제거**로 분할한 뒤  
`점수 = −(찬 직사각 수 + 빈 직사각 수)` 로 cascade 게이트·번들 선택에 쓴다.

## 계획

1. `AreaScoreCalculator` — prefix-sum 기반 최대면적 직사각 greedy (동률: y↑→x↑→폭↓)
2. `AreaComponentScore` — 직사각 기하(x,y,w,h)만 기록, 개당 −1
3. `AreaScoreTuning` 삭제 · `AreaBundlePoolSO.areaScore` 제거
4. `uniqueAreaThreshold` 기본/에셋 **−8**

## 비범위

- Unique/Normal/Easy cascade 규칙 변경
- 정확 최소 직사각 분할
