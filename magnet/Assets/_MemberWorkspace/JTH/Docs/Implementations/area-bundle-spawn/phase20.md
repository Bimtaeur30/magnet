# Phase 20 — 멀티클리어 → Hospitality(기회 피스)

## 목표

멀티클리어 Clear Priority를 제거한다. 대신 **지금 놓으면 라인이 지워지는 피스(기회)** 를 포함한 Normal 번들 중, 완주 가능하고 예측 Area가 가장 좋은 조합을 준다. 죽으면(완주 불가) 접대를 포기하고 Area 최대로 간다.

## 구현 내용

- `OpportunityDetector.FindClearingPieceIds` — 카탈로그에서 즉시 클리어>0 배치 가능한 ID
- `TrySelectHospitality` — 기회 ID 포함 번들만, `CanSurvive` + `MaxArea`(Normal과 동일 수치)
- `AreaBundleTier.Hospitality` (구 MultiClear 자리)
- `multiClearHardMinLines` 삭제

## 범위 밖

- 올클 Exact 고정 풀
- Unique
- Normal Area 빔 경로
