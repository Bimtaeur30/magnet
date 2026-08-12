# sequence34 — Phase 34 변경 기록

> Phase 계획: [phase34.md](phase34.md)

## 1 — 2026-08-11 · 모서리 덮개 직사각 패널티

**바뀐 것** — greedy 직사각 개수 패널티를 없애고, 네 모서리 기준 전 찬칸 덮개 직사각 최소 면적 × `cornerRectPenalty`로 바꾼다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `AreaScoreTuning.rectCountPenalty` — 필드 (삭제)
    - 설명: greedy 직사각 개수 계수 제거.
    - 이유: Phase34에서 모서리 덮개 면적 패널티로 대체.
  - 심볼: `AreaScoreTuning.cornerRectPenalty` — 필드 (추가, 기본 1)
    - 설명: 최소 모서리 덮개 면적에 곱하는 패널티 계수.
    - 이유: 사용자가 새 상수 × 면적 형태의 패널티를 원해서.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.Score` — 메서드 (수정)
    - 설명: `CountRectangles` 대신 `MinCornerCoverRectArea`와 `cornerRectPenalty`로 Total을 만든다.
    - 이유: 새 패널티 정의 반영.
  - 심볼: `AreaScoreCalculator.MinCornerCoverRectArea` — 메서드 (추가)
    - 설명: 찬 칸 bbox로 네 모서리 덮개 면적 `(maxX+1)*(maxY+1)` 등을 계산해 최솟값을 반환. 빈 보드 0.
    - 이유: “모서리에서 시작·전 찬칸 포함·최소 사이즈”를 구현.
  - 심볼: `CountRectangles` / `PartitionCount` / `TryFindBestRectangle` / `IsBetter` / `RebuildPrefix` / `RectSum` / `Carve` — 메서드 (삭제)
    - 설명: greedy 직사각 분할 경로 전부 제거.
    - 이유: 더 이상 점수에 쓰이지 않아서.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs`
  - 심볼: `AreaScoreResult.RectCount` / `RectPenalty` — 프로퍼티 (삭제)
    - 설명: 구 rect 개수·패널티 필드 제거.
    - 이유: API를 새 의미에 맞추기 위해.
  - 심볼: `AreaScoreResult.CornerRectArea` / `CornerRectPenalty` — 프로퍼티 (추가)
    - 설명: 최소 덮개 면적과 그에 대한 패널티 값을 보관.
    - 이유: 디버그·로그에서 새 항을 읽을 수 있게.
- 파일: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `areaScore.cornerRectPenalty` — 직렬화 (추가, 1)
    - 설명: 풀 에셋에 새 계수 기록. `rectCountPenalty` 키 제거.
    - 이유: 런타임 SO가 구 필드를 남기지 않게.
