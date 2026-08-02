# Phase 10 — Area 개수 패널티

## 목표

4-연결 Area(찬+빈) **개수가 적을수록** Total이 높아지도록 `areaCountPenalty × areaCount` 항을 추가한다.

## 결과

- [x] `AreaScoreTuning.areaCountPenalty` (기본 4)
- [x] `AreaScoreCalculator` / `AreaScoreResult`에 AreaCount·AreaCountPenalty 반영
- [x] `DefaultAreaBundlePool` · TUNING_STAGES · Tooltip 동기화

## 식

`Total = baseArea − rectCountPenalty×rectCount − areaCountPenalty×areaCount`
