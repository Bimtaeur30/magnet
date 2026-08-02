# sequence14 — Phase 14 변경 기록

## 1 — 2026-08-02 · MultiClear 6 + Normal 전수 평등 집계

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `multiClearHardMinLines` — 필드 (수정)
    - 설명: 기본값 **6**.
    - 이유: 멀티클리어 Clear Priority 문턱 상향.
  - 심볼: `normalBundles` Tooltip — 필드 (수정)
    - 설명: 스크린샷 전수·필터 없음·관측횟수 가중만.
    - 이유: large 우선/제외 등 편애 정책 폐기.

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal()` — 메서드 (수정)
    - 설명: batch1–7 **전수** 325패. 포함/제외 필터 없음. weight=`clamp(count,1..5)`만.
    - 이유: Phase11 large 우선이 큼직한 패 과다의 원인. 사진에 나온 것을 평등하게 가져와야 함.
    - 영향: Fill Starter · Orchestrator 폴백.

- 에셋: `DefaultAreaBundlePool.asset` Fill
- 스냅샷: `Docs/Blocks2_batches/normal_rebuild_all_equal.json`
- 문서: `phases.md` · `IMPLEMENTATIONS.md` · `INSPECTOR_TOOLTIPS.md` · `TUNING_STAGES.md`

## 2 — 2026-08-02 · 무효 번들 n203 제거 (ID 0 NRE)

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal()` — 메서드 (수정)
    - 설명: `E("n203", 25, 0, 0)` 항목을 목록에서 삭제. Normal **324**패.
    - 이유: ShapeId `0`은 `BlockBlastCatalog` 미사용 슬롯이라 `GetOffsets(0)`이 null → `PlacementService.AnyMatch`에서 NRE.
    - 영향: `DefaultAreaBundlePool` Fill·Orchestrator 스코어링.

- 에셋: `DefaultAreaBundlePool.asset` — `n203` 엔트리 삭제
- 스냅샷: `normal_rebuild_all_equal.json` · `normal_rebuild_no_megarect.json` 동일 무효 행 삭제
- 문서: `phases.md` · `IMPLEMENTATIONS.md` · `phase14.md` 카운트 325→324
