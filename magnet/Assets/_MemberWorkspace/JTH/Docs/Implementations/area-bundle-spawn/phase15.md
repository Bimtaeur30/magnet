# Phase 15 — PlacementSolver dead API 제거

## 목표

AreaBundle이 쓰지 않는 구 Pressure/유일해 기록 API와 `Solution` 타입을 제거한다.

## 구현 내용

- `HasAnyPlacement` / `ComboMaintainable` / `TryFindUniqueFullSequence` / `SequenceRecorder` 삭제
- `UniqueSolution` / `SolutionStep` 및 `Solution/` 폴더 삭제
- `CountSequences`에서 `requireClear`·recorder 경로 제거 (완주 카운트만)

## 범위 밖

- `FullSequenceExists` / `CountFullSequences` / `PlacementSimulator` / `SequenceOutcomeEstimator` 유지
- AreaBundle cascade·튜닝 변경 없음

## 코드·에셋 맵

| 경로 | 변경 |
|------|------|
| `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs` | 미사용 API·recorder 제거 |
| `Scripts/Domain/BlockSelection/Solution/**` | 삭제 |
