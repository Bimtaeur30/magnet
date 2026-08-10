# sequence16 — Phase 16 변경 기록

## 1 — 2026-08-08 · Area 개수 패널티 제거

- 수정: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `areaCountPenalty` — 필드 (삭제)
    - 설명: 4-연결 Area 1개당 Total에서 빼던 튜닝 값을 제거한다.
    - 이유: 직사각 개수 패널티와 역할이 겹쳐 중복 감점이라 판단.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.Score(board, tuning)` — 메서드 (수정)
    - 설명: `Total = baseTotal − rectPenalty`만 계산한다. `areaCountPenalty × components.Count` 항을 뺀다.
    - 이유: Area 개수 패널티 제거. 직사각 개수만으로 조각난 판을 감점.
    - 영향: Unique dirty 임계·Normal/Easy Area 최대 선택.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs`
  - 심볼: `AreaScoreResult` 생성자 — (수정)
    - 설명: `areaCount`·`areaCountPenalty` 인자를 제거한다.
    - 이유: 결과에 더 이상 개수 패널티를 실지 않음.
  - 심볼: `AreaCount` / `AreaCountPenalty` — 프로퍼티 (삭제)
    - 설명: Area 개수와 그 패널티 노출을 제거한다.
    - 이유: 점수식에서 해당 항 삭제.

- 에셋: `DefaultAreaBundlePool.asset` — `areaCountPenalty` 직렬화 필드 제거
- 문서: `phases.md` · `phase16.md` · `TUNING_STAGES.md` · `IMPLEMENTATIONS.md` · `INSPECTOR_TOOLTIPS.md`
