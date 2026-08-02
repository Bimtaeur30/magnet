# sequence7 — 직사각 greedy Area 점수

## 1 — 2026-08-02 · 최대면적 직사각 분할로 Area 교체

**바뀐 것**

- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.Score(BoardGrid)` — 메서드 (수정)
    - 설명: 찬/빈 마스크를 각각 greedy 직사각 분할한 뒤 `Total = −components.Count` 와 직사각 목록을 반환한다. `AreaScoreTuning` 인자를 제거했다.
    - 이유: grill 확정 — Area를 사각형만으로 판정하고 개수를 최소화하는 점수로 cascade를 돌리기 위함.
  - 심볼: `AreaScoreCalculator.ScoreTotal(BoardGrid)` — 메서드 (수정)
    - 설명: `Score(board).Total`만 반환. tuning 인자 제거.
    - 이유: 호출부(Orchestrator·Metrics)가 더 이상 SO 튜닝을 넘기지 않음.
  - 심볼: `AreaScoreCalculator.PartitionGreedy` — private static (추가)
    - 설명: 마스크에서 최대면적 직사각을 찾아 깎기를 칸이 없을 때까지 반복하고, 각 직사각을 `sink`에 추가한다.
    - 이유: “가장 큰 직사각부터 만들기” greedy를 찬/빈에 공통 적용.
  - 심볼: `AreaScoreCalculator.TryFindBestRectangle` — private static (추가)
    - 설명: prefix-sum으로 꽉 찬 축정렬 직사각을 전수 검사해 최선 (면적→y↑→x↑→폭↓) 하나를 고른다.
    - 이유: 8×8에서 정확 열거가 싸고, 동률 타이브레이크로 점수를 결정적으로 만듦.
  - 심볼: `AreaScoreCalculator.IsBetter` — private static (추가)
    - 설명: 후보 직사각이 현재 best보다 우선인지 비교한다.
    - 이유: grill Q5 타이브레이크를 한곳에 고정.
  - 심볼: `AreaScoreCalculator.RebuildPrefix` / `RectSum` / `Carve` — private static (추가)
    - 설명: 2D prefix로 O(1) 채움 검사, 선택 직사각 칸을 마스크에서 제거.
    - 이유: 매 후보마다 O(wh) 스캔을 피하고 분할을 진행.
  - 심볼: `ScoreEmpty` / `ScoreFilled` / `SideBonus` / `CountOrthogonalSides` / `Flood` — (삭제)
    - 설명: 4-연결·size·변 보너스 경로 전부 제거.
    - 이유: 새 점수식과 무관하고 혼동을 막기 위함.

- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs`
  - 심볼: `AreaComponentScore` 생성자 `(occupied, x, y, width, height)` — (수정)
    - 설명: 직사각 원점·크기만 보관. `Size = w*h`, `Total = -1`.
    - 이유: 디버그 시 깎인 사각형을 그대로 보고, 개수 점수와 1:1.
  - 심볼: `AreaComponentScore.SideCount` / `BaseScore` / `SideBonus` — (삭제)
    - 설명: 변 보너스·base 필드 제거.
    - 이유: 튜닝식 폐기.

- 파일: `Scripts/Data/AreaScoreTuning.cs` — (삭제)
  - 심볼: `AreaScoreTuning` 전체 — (삭제)
    - 설명: empty/filled/side 튜닝 클래스와 meta 제거.
    - 이유: grill Q8 — size/변 SO 삭제.

- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `areaScore` / `AreaScore` — 필드·프로퍼티 (삭제)
    - 설명: 중첩 `AreaScoreTuning` 직렬화 제거.
    - 이유: 점수식이 상수(−개수)라 SO 불필요.
  - 심볼: `uniqueAreaThreshold` — 필드 기본값 (수정)
    - 설명: 기본값 −20 → **−8**. Tooltip을 −직사각수 기준으로 갱신.
    - 이유: grill Q7 — 새 스케일에서 Unique 게이트 시작점.

- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `Select` 내 `ScoreTotal` 호출 — (수정)
    - 설명: `ScoreTotal(board)`만 호출 (`_pool.AreaScore` 제거).
    - 이유: tuning API 삭제에 맞춤.
  - 심볼: `MaxAreaAfterFullSequence` 호출 — (수정)
    - 설명: tuning 인자 없이 호출.
    - 이유: Metrics 시그니처 변경.

- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleMetrics.cs`
  - 심볼: `MaxAreaAfterFullSequence` / `SearchMaxArea` — (수정)
    - 설명: `AreaScoreTuning` 매개변수 제거, `ScoreTotal(board)` 사용.
    - 이유: Area 점수가 튜닝 불가라 시뮬 경로를 단순화.

- 파일: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `areaScore` YAML 블록 — (삭제), `uniqueAreaThreshold` → −8 — (수정)
    - 설명: 구 튜닝 직렬화 제거, Unique 게이트 −8.
    - 이유: 런타임 에셋이 코드와 일치해야 함.

**조건**

- 찬·빈 각각 겹침 없는 greedy 분할
- 동률: 면적 최대 → y 최소 → x 최소 → 폭 최대
- dirty: `boardArea ≤ -8`
