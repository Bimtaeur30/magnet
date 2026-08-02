# sequence1 — Phase 1 변경 기록

## 1 — 2026-08-02 · Area 점수 도메인

**바뀐 것**

- 생성: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
- 생성: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs` (`AreaComponentScore` 포함)
- 생성: `Docs/Implementations/area-bundle-spawn/phases.md` · `phase1.md` · `sequence1.md`

**심볼**

- `AreaScoreCalculator.Score` / `ScoreTotal` / `ScoreEmpty` / `ScoreFilled` / `SideBonus` / `CountOrthogonalSides`
- `AreaScoreResult` · `AreaComponentScore`

**메모**

- 변 = 경계 단위 변을 동일 직선·연속이면 하나로 합친 개수 (직·정사각=4).
- 다음 Phase: 42-ID 번들 풀 + Early/Normal Area 최대 선택. 유일수 샘플은 사용자 제공 대기.
