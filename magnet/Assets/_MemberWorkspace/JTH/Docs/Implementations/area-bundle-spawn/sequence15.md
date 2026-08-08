# sequence15 — Phase 15 변경 기록

## 1 — 2026-08-05 · PlacementSolver dead API·유일해 타입 제거

- 수정: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs`
  - 심볼: `PlacementSolver.SequenceRecorder` — nested class (삭제)
    - 설명: DFS 경로 `Stack`과 첫 완주 `Captured` 스냅샷을 담던 기록기를 제거한다.
    - 이유: 유일해 경로 보관·`MatchesStep` UI가 미배선이라 소비처가 없다.
  - 심볼: `PlacementSolver.HasAnyPlacement(board, pieces)` — 메서드 (삭제)
    - 설명: 3피스 중 즉시 배치 가능 여부만 검사하던 API를 제거한다.
    - 이유: AreaBundle·현재 런타임에서 호출처 없음 (구 Trap 선검사용).
  - 심볼: `PlacementSolver.ComboMaintainable(board, pieces)` — 메서드 (삭제)
    - 설명: 완주 중 최소 1회 클리어가 있는 시퀀스 존재 여부를 제거한다.
    - 이유: 구 ComboBreak 판정용. 현재 cascade 미사용.
  - 심볼: `PlacementSolver.TryFindUniqueFullSequence(board, pieces)` — 메서드 (삭제)
    - 설명: cap=2 카운트 + 첫 완주 스텝 기록으로 `UniqueSolution`을 만들던 API를 제거한다.
    - 이유: Pressure/엄지척 매칭 미배선. AreaBundle은 개수·death·outcome만 사용.
  - 심볼: `PlacementSolver.CountSequences(..., requireClear, recorder)` — 메서드 (수정)
    - 설명: `requireClear`·`recorder`·`clearsSoFar` 인자를 없애고 완주 개수만 센다.
    - 이유: 남은 공개 API가 `FullSequenceExists`/`CountFullSequences`뿐이라 클리어 조건·기록이 불필요.
    - 영향: `AreaBundleMetrics.CountSequences` / `CanSurvive`.

- 삭제: `Scripts/Domain/BlockSelection/Solution/UniqueSolution.cs`
  - 심볼: `UniqueSolution.Steps` — 프로퍼티 (삭제)
    - 설명: 유일해 스텝 목록 보관 필드를 타입과 함께 제거한다.
    - 이유: `TryFindUniqueFullSequence` 전용 소비자.
  - 심볼: `UniqueSolution.MatchesStep(...)` — 메서드 (삭제)
    - 설명: 플레이어 배치가 정답 스텝과 일치하는지 판정하던 API를 제거한다.
    - 이유: UI 계약 미배선·호출처 없음.

- 삭제: `Scripts/Domain/BlockSelection/Solution/SolutionStep.cs`
  - 심볼: `SolutionStep` — readonly struct (`SlotIndex`, `Pivot`, `CellOffsets`, `ClearedLines`) (삭제)
    - 설명: 유일해 한 스텝 DTO를 제거한다.
    - 이유: `UniqueSolution`/`SequenceRecorder`만 참조.

- 삭제: `Scripts/Domain/BlockSelection/Solution/` 폴더 · `Solution.meta`
  - 설명: 빈 Solution 네임스페이스 폴더와 Unity meta를 제거한다.
  - 이유: 스크립트 전부 삭제 후 고아 폴더.

- 유지: `FullSequenceExists` · `CountFullSequences` · `PlacementSimulator` · `SequenceOutcomeEstimator`
  - 설명: AreaBundleMetrics/Orchestrator/UniqueUnlockGenerator가 계속 사용.
  - 이유: 현재 스폰 cascade의 공유 유틸.

- 문서: `phases.md` · `IMPLEMENTATIONS.md` · `phase15.md`
