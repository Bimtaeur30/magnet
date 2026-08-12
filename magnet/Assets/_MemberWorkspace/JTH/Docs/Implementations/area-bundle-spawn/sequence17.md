# sequence17 — Phase 17 변경 기록

## 1 — 2026-08-08 · 변 보너스 제거 · Area 개수 패널티 복구

- 수정: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `sideBonusIdealMax` / `sideBonusAtIdeal` / `sideBonusPerTwoSides` — 필드 (삭제)
    - 설명: 찬 Area 변 개수 보너스 튜닝을 제거한다.
    - 이유: 직사각 개수 패널티와 겹치는 형태 보정이라 불필요. (Phase16에서 의도했던 제거 대상)
  - 심볼: `areaCountPenalty` — 필드 (재추가)
    - 설명: 4-연결 Area 1개당 Total에서 빼는 값을 다시 둔다 (기본 4).
    - 이유: Phase16에서 잘못 제거함. 면(Area) 개수 패널티는 유지.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `ScoreComponent` — 메서드 (수정)
    - 설명: `CountOrthogonalSides`/`SideBonus` 호출을 제거하고 base만 합산한다.
    - 이유: 변 보너스 삭제.
  - 심볼: `SideBonus` / `CountOrthogonalSides` / `PackEdge` / `UnpackEdge` / `TryUnion` / `Find` — (삭제)
    - 설명: 변 개수 계산·보너스·엣지 유니온 헬퍼를 제거한다.
    - 이유: 호출처 없음.
  - 심볼: `Score` — 메서드 (수정)
    - 설명: `Total = base − rectPenalty − areaCountPenalty×areaCount`로 복구한다.
    - 이유: Area 개수 패널티 유지.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs`
  - 심볼: `AreaComponentScore.SideCount` / `SideBonus` — 프로퍼티 (삭제)
    - 설명: 컴포넌트 결과에서 변 관련 필드를 뺀다. `Total => BaseScore`.
    - 이유: 변 보너스 삭제.
  - 심볼: `AreaCount` / `AreaCountPenalty` — 프로퍼티 (재추가)
    - 설명: Area 개수와 패널티를 결과에 다시 노출한다.
    - 이유: Phase16 오제거 복구.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `areaScore` Tooltip — (수정)
    - 설명: "빈/찬 Area size + 직사각·Area 개수 패널티".
    - 이유: 변 보너스 문구 제거.

- 에셋: `DefaultAreaBundlePool.asset` — side* 필드 제거, `areaCountPenalty: 4` 복구
- 문서: `phases.md` · `phase17.md` · `TUNING_STAGES.md` · `IMPLEMENTATIONS.md` · `INSPECTOR_TOOLTIPS.md`
