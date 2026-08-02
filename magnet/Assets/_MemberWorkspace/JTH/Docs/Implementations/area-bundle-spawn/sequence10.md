# sequence10 — Phase 10 변경 기록

## 1 — 2026-08-02 · Area 개수 패널티

- 수정: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `areaCountPenalty` — 필드 (추가)
    - 설명: 4-연결 Area 1개당 Total에서 빼는 양. 기본 4.
    - 이유: 영역이 쪼개질수록 감점 → 적을수록 점수↑.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs`
  - 심볼: `AreaScoreResult` 생성자 — (수정)
    - 설명: `areaCount`·`areaCountPenalty` 인자 추가.
    - 이유: 진단·로그용 분해.
  - 심볼: `AreaCount` / `AreaCountPenalty` — 프로퍼티 (추가)
    - 설명: 영역 개수와 그 패널티 합.
    - 이유: Total 구성 요소 노출.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `Score` — 메서드 (수정)
    - 설명: `Total = base − rectPenalty − areaCountPenalty×components.Count`.
    - 이유: Phase 10 식.

- 에셋: `DefaultAreaBundlePool.asset` — `areaCountPenalty: 4`
- 문서: `phase10.md` · `phases.md` · `TUNING_STAGES.md` · `INSPECTOR_TOOLTIPS.md`
