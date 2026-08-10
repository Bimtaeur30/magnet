# sequence20 — Phase 20 변경 기록

## 1 — 2026-08-10 · Hospitality 교체

- 생성: `Scripts/Domain/AreaBundleSpawn/OpportunityDetector.cs`
  - 심볼: `FindClearingPieceIds(board)` — 메서드 (추가)
    - 설명: 42-ID 각각에 대해 보드에 배치 후 `PlaceAndClear>0`이면 기회 피스로 모은다.
    - 이유: “들어가기 좋은 자리”를 즉시 클리어 가능으로 정의.
  - 심볼: `ContainsOpportunityPiece(entry, ids)` — 메서드 (추가)
    - 설명: 번들 3ID 중 기회 ID 포함 여부.
    - 이유: Hospitality 후보 필터.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleTier.cs`
  - 심볼: `MultiClear` → `Hospitality` — enum (수정)
    - 설명: 멀티클리어 티어 이름을 접대로 바꾼다 (동일 ordinal).
    - 이유: Clear Priority 의미 변경.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectHospitality` — 메서드 (추가)
    - 설명: 기회 ID 포함 Normal 번들 샘플 → 완주 불가 스킵 → 예측 Area 최대 선택.
    - 이유: 기회 피스 포함 최적 Normal과 동일 수치. 죽으면 포기.
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: AllClear 다음 Hospitality, 실패 시 ScoreSurvivors(빔) Area만.
    - 이유: 멀티클리어 단계 제거.
  - 심볼: `FilterMinClears` / `PickMaxClears` — (삭제)
    - 설명: 멀티클리어 전용 헬퍼 제거.
    - 이유: 미사용.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `multiClearHardMinLines` / `MultiClearHardMinLines` — (삭제)
    - 설명: 멀티 문턱 튜닝 제거.
    - 이유: Hospitality는 문턱 줄 수 없음.

- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `TierStyle` — (수정)
    - 설명: Hospitality → 로그 "접대".
    - 이유: 티어 이름 변경.

- 문서: `phases.md` · `phase20.md` · `IMPLEMENTATIONS.md` · `TUNING_STAGES.md` · `INSPECTOR_TOOLTIPS.md`
