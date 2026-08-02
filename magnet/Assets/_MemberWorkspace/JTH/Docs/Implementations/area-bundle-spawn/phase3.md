# Phase 3 — 유일수 seq/death + 킬 폴백

## 목표

유일수 경로: 시퀀스 최소 → 데스 최대 → Area. 생존 불가면 리스트 랜덤 킬.

## 결과

- `AreaBundleMetrics.CountDeaths` / `CountSequences` / `MaxAreaAfterFullSequence`
- `AreaBundleOrchestrator.SelectUnique`
- 유일수 리스트 비어 있으면 기본 폴백(샘플 전 임시)
