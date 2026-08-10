# sequence21 — Phase 21 변경 기록

## 1 — 2026-08-10 · 구멍·윤곽 Exact 접대

- 수정: `Scripts/Domain/AreaBundleSpawn/OpportunityDetector.cs`
  - 심볼: `HospitalityHole` — 타입 (추가)
  - 심볼: `FindQualifyingHoles(board, minContourFill)` — 메서드 (추가)
    - 설명: 4-연결 빈 영역 → 8이웃 윤곽 채움% ≥ n → Exact 핏 ID.
    - 이유: 접대 = 거의 막힌 구멍에 쏙 들어가는 피스.
  - 심볼: `CountFittingSlots` / `CompareHoleCoverage` — 메서드 (추가)
    - 설명: 번들 슬롯의 구멍 핏 수·높은 구멍 우선 비교.
  - 심볼: `FindClearingPieceIds` / `ContainsOpportunityPiece` — (삭제)
    - 설명: 즉시 클리어 기회 정의 제거.
    - 이유: 의도 불일치.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `hospitalityContourMinFill` / `HospitalityContourMinFill` — (추가)
  - 심볼: `hospitalityProbability` / `HospitalityProbability` — (추가)

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectHospitality` — 메서드 (수정)
    - 설명: 구멍 기반 선정 + CanSurvive + Area 타이브레이크.
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: 접대 후보 후 p=HospitalityProbability, 낙첨 시 Normal.

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `hospitalityContourMinFill=0.7` · `hospitalityProbability=0.35`

- 문서: `phase21.md` · `phases.md` · `IMPLEMENTATIONS.md` · `TUNING_STAGES.md` · `INSPECTOR_TOOLTIPS.md`
