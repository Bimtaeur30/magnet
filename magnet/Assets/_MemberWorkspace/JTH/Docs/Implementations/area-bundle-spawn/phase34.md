# Phase 34 — 모서리 덮개 직사각 패널티

## 목표

greedy `rectCount` 패널티를 제거하고, 보드 **네 모서리** 각각에서 시작해 **모든 찬 칸을 덮는** 축정렬 직사각 면적 중 **최솟값**에 새 상수 `cornerRectPenalty`를 곱해 뺀다.

## 범위

1. `AreaScoreTuning.cornerRectPenalty` (기본 1), `rectCountPenalty` 삭제
2. `MinCornerCoverRectArea` — 빈 보드 0
3. `Total = base − cornerRectPenalty×area − areaCountPenalty×areaCount`
4. greedy Partition/CountRectangles 제거
5. `AreaScoreResult` 필드명 CornerRect* 로 교체

## 비범위

- 찬/빈 Area flood 규칙 변경
- 상수 튜닝 플레이테스트

## 수락

- [x] rectCount 경로 없음
- [x] 네 모서리 중 최소 덮개 면적 × cornerRectPenalty
- [x] 빈 보드 corner 패널티 0
- [x] 컴파일 오류 없음
