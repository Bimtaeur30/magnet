# sequence9 — Phase 9 변경 기록

## 1 — 2026-08-02 · Normal 올클·멀티클리어 우선

**바뀐 것**

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `outcomeBeamWidth` — 필드 (추가)
    - 설명: `SequenceOutcomeEstimator` 빔 폭. 기본 4.
    - 이유: 완주 클리어·올클 여부를 Area 탐색과 별도로 추정하기 위함.
  - 심볼: `OutcomeBeamWidth` — 프로퍼티 (추가)
    - 설명: 빔 폭 조회(최소 1).
    - 이유: Orchestrator가 SO에서 예산 읽음.
  - 심볼: `allClearProbability` — 필드 (추가)
    - 설명: 올클 가능 후보가 있을 때 지급 확률. 기본 0.75.
    - 이유: grill — 변덕 유지, 낙첨 시 올클 제외.
  - 심볼: `AllClearProbability` — 프로퍼티 (추가)
    - 설명: 확률 조회.
    - 이유: Orchestrator 게이트 입력.
  - 심볼: `allClearCooldownTurns` — 필드 (추가)
    - 설명: 올클 패 지급 후 올클 최우선 쉬는 턴 수. 기본 1.
    - 이유: 무한 올클 연쇄 방지 (빈 보드 스킵과 함께).
  - 심볼: `AllClearCooldownTurns` — 프로퍼티 (추가)
    - 설명: 쿨다운 턴 수 조회(음수면 0).
    - 이유: 지급 시 `_allClearCooldownRemaining`에 대입.
  - 심볼: `multiClearMinLines` — 필드 (추가)
    - 설명: 이 줄 수 이상이면 Area 대신 클리어 최대. 기본 3.
    - 이유: grill — 3피스 완주 기준 3줄.
  - 심볼: `MultiClearMinLines` — 프로퍼티 (추가)
    - 설명: 하한 조회(최소 1).
    - 이유: MultiClear 필터 문턱.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleTier.cs`
  - 심볼: `AreaBundleTier.AllClear` — enum 값 (추가)
    - 설명: 올클 우선으로 고른 Normal 패 티어.
    - 이유: 로그·UI에서 Area Normal과 구분.
  - 심볼: `AreaBundleTier.MultiClear` — enum 값 (추가)
    - 설명: ≥N줄 클리어 최대로 고른 패 티어.
    - 이유: 깔짝 Area와 구분.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `ScoredCandidate` — private readonly struct (추가)
    - 설명: 후보 번들 + 클리어·올클·예측 Area·seq를 한 번에 보관.
    - 이유: 올클/멀티/Area 단계를 같은 평가 결과로 재사용.
  - 심볼: `_allClearCooldownRemaining` — 필드 (추가)
    - 설명: 남은 올클 쿨다운 턴.
    - 이유: 지급 후 1턴 올클 스킵.
  - 심볼: `TrySelectNormalPriority` — 메서드 (추가)
    - 설명: 생존 후보 스코어 → (빈보드·쿨다운 아니면) 올클 75% → 낙첨 시 올클 제외 → ≥N줄 클리어 최대 → Area 최대.
    - 이유: grill 확정 Normal 우선순위.
    - 영향: `SelectNormalOrEasy`가 `TrySelectByMaxArea` 대신 이 경로 사용.
  - 심볼: `SelectNormalOrEasy` — 메서드 (수정)
    - 설명: Normal 선택을 `TrySelectNormalPriority`로 교체.
    - 이유: Area-only로는 멀티·올클이 안 나옴.
  - 심볼: `ScoreSurvivors` — 메서드 (추가)
    - 설명: 샘플 후보에 `SequenceOutcomeEstimator` + `MaxAreaAfterFullSequence`를 돌려 완주만 수집.
    - 이유: 클리어/올클과 Area를 동일 후보 집합에서 비교.
  - 심볼: `ToResult` / `FilterBoardEmptied` / `ExcludeBoardEmptied` / `FilterMinClears` / `PickMaxClears` / `PickMaxArea` / `CountOccupied` — 메서드 (추가)
    - 설명: 결과 조립·올클/멀티 필터·최대 선택·빈 보드 판정.
    - 이유: `TrySelectNormalPriority` 가독성·단일 책임.

- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `TierStyle` — 메서드 (수정)
    - 설명: AllClear·MultiClear 로그 라벨/색 추가.
    - 이유: 플레이 중 어느 우선순위가 발동했는지 확인.

- 문서: `phase9.md` · `phases.md` · `IMPLEMENTATIONS.md` · `Docs/INSPECTOR_TOOLTIPS.md` · `TUNING_STAGES.md` (Clear Priority 행)

## 2 — 2026-08-02 · rect k=4 + MultiClear 빨간 로그

- 수정: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `rectCountPenalty` — 필드 기본값 (수정)
    - 설명: 기본 3 → **4**.
    - 이유: 직사각 개수 패널티를 살짝 강화.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `areaScore.rectCountPenalty` — 직렬화 값 (수정)
    - 설명: 에셋 3 → **4**.
    - 이유: 런타임 SO가 코드 기본값과 같게.
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `TierStyle` — 메서드 (수정)
    - 설명: MultiClear 색 `#FF8A65` → `#FF1744` (빨강).
    - 이유: ≥3줄 우선 선택이 콘솔에서 눈에 띄게.
  - 심볼: `LogSelection` — 메서드 (수정)
    - 설명: `reason`도 `<color>` 안에 넣어 본문까지 동일 색.
    - 이유: 멀티클리어 시 reason까지 빨갛게.

## 3 — 2026-08-02 · 멀티클리어 4줄 50% / 5줄+ 100%

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `multiClearMinLines` / `MultiClearMinLines` — 필드·프로퍼티 (삭제)
    - 설명: 단일 문턱(3줄·항상) 제거.
    - 이유: 4/5줄 차등 확률로 교체.
  - 심볼: `multiClearSoftMinLines` / `MultiClearSoftMinLines` — 필드·프로퍼티 (추가)
    - 설명: 소프트 문턱 기본 4.
    - 이유: 4줄부터 멀티클리어 후보.
  - 심볼: `multiClearSoftProbability` / `MultiClearSoftProbability` — 필드·프로퍼티 (추가)
    - 설명: 소프트 문턱일 때 선택 확률 기본 0.5.
    - 이유: 4줄은 50%만 강제.
  - 심볼: `multiClearHardMinLines` / `MultiClearHardMinLines` — 필드·프로퍼티 (추가)
    - 설명: 하드 문턱 기본 5 — 이상이면 확률 1.
    - 이유: 5줄 이상은 무조건 멀티클리어.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: 최선 클리어 ≥hard → 100%, soft~hard-1 → SoftProbability, 낙첨 시 Area로 하향(후보 유지).
    - 이유: 사용자 요청 4@50% / 5@100%.
