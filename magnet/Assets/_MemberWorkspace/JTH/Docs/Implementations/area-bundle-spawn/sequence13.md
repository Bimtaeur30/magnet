# sequence13 — Phase 13 변경 기록

## 1 — 2026-08-02 · Normal freq≥2 + MultiClear 5줄만

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal()` — 메서드 (수정)
    - 설명: Blocks2 집계에서 **Count≥2만** 남긴다 (195→27). large(11–13/21–24/35–36) freq1 예외 제거. weight=`clamp(count,1..5)` 유지.
    - 이유: large freq1 예외가 Normal을 큼지막한 패로 부풀려 체감이 과함.
    - 영향: `AreaBundlePoolSO.FillStarterBundles` · Orchestrator ResolveList 폴백.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: 멀티클리어 후보를 `MultiClearHardMinLines`(기본 5) 이상으로만 필터하고 100% 선택. Soft(4줄·확률) 분기 삭제.
    - 이유: 4줄 Clear Priority가 큰 패를 자주 끌어올림. 5줄 미만은 Area 최대로 넘김.
  - 심볼: (간접) `MultiClearSoftMinLines` / `MultiClearSoftProbability` 참조 — 삭제
    - 설명: soft 문턱·확률 읽기를 제거한다.
    - 이유: SO 필드와 함께 Soft 경로 폐기.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `multiClearSoftMinLines` / `MultiClearSoftMinLines` — 필드·프로퍼티 (삭제)
    - 설명: 4줄 soft 문턱을 제거한다.
    - 이유: MultiClear는 hard만 사용.
  - 심볼: `multiClearSoftProbability` / `MultiClearSoftProbability` — 필드·프로퍼티 (삭제)
    - 설명: soft 발동 확률을 제거한다.
    - 이유: Soft 경로 없음.
  - 심볼: `multiClearHardMinLines` Tooltip — 필드 (수정)
    - 설명: “이 줄 수 이상일 때만 Clear Priority”로 문구 갱신.
    - 이유: Soft 폐기에 맞춰 Inspector 의미 일치.
  - 심볼: `normalBundles` Tooltip — 필드 (수정)
    - 설명: “freq≥2만”으로 갱신.
    - 이유: large freq1 정책 철회.

- 에셋: `DefaultAreaBundlePool.asset` — Fill Starter로 Normal 27 반영.
- 스냅샷: `Docs/Blocks2_batches/normal_rebuild_freq2.json`
- 문서: `phase13.md` · `phases.md` · `TUNING_STAGES.md` · `IMPLEMENTATIONS.md`
