# sequence18 — Phase 18 변경 기록

## 1 — 2026-08-08 · 직사각 개수 = 찬 칸만

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `CountRectangles(board)` — 메서드 (수정)
    - 설명: occupied 마스크만 greedy 직사각 분할해 개수를 센다. empty 마스크 `PartitionCount`를 제거한다.
    - 이유: 빈 칸 직사각 수는 점수에 쓰지 않기로 함.
    - 영향: `rectCountPenalty` 항·Unique/Normal Area 비교.

- 수정: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `rectCountPenalty` Tooltip — (수정)
    - 설명: "찬 칸 greedy 직사각 1개당 점수에서 빼는 양".
    - 이유: 찬+빈이 아님을 Inspector에 반영.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `areaScore` Tooltip — (수정)
    - 설명: "빈/찬 Area size + 찬 직사각·Area 개수 패널티".
    - 이유: 동일.

- 문서: `phases.md` · `phase18.md` · `IMPLEMENTATIONS.md` · `INSPECTOR_TOOLTIPS.md`
