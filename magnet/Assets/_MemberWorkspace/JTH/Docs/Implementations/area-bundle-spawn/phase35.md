# Phase 35 — CornerRect 튜닝 단계 1~4

## 목표

`cornerRectPenalty`를 Stage 1~4(0.5/1/2/4)로 나눠, **에이전트가 에셋 값을 바꿔** 가며 플레이 평가한다.

## 범위

1. 단계 표·현재 Stage 표시 (`TUNING_STAGES.md`)
2. 에이전트가 `DefaultAreaBundlePool.areaScore.cornerRectPenalty` 직접 수정
3. Inspector ContextMenu 튜닝 UI는 쓰지 않음

## 비범위

- 우승 값 확정(평가 후)

## 수락

- [x] Stage 1=0.5 적용(현재)
- [x] 평가표에 1~4·점수 칸
- [x] ContextMenu CR 전환 제거
