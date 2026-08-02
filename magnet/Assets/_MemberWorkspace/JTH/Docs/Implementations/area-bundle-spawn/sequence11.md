# sequence11 — Phase 11 변경 기록

## 1 — 2026-08-02 · Blocks2 Normal 재집계

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal()` — 메서드 (수정)
    - 설명: Blocks2(~347장, 유효 319핸드) 집계로 Normal 리스트를 교체. 195개 엔트리(`n001`…). 제외: ShapeId 1, 37+. 포함: freq≥2 또는 large(11–13,21–24,35–36). weight=`clamp(count,1..5)`.
    - 이유: 구 Normal(500샘플·59)은 대형(13/35/36 등)이 거의 없고 원작 Normal 스크린샷과 불일치. 올클·멀티가 잘 안 나오는 원인 중 하나.
    - 영향: `AreaBundlePoolSO.FillStarterBundles` · `AreaBundleOrchestrator` ResolveList 폴백.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `normalBundles` — SerializeField Tooltip (수정)
    - 설명: Tooltip을 Blocks2 재집계 정책 문구로 갱신.
    - 이유: 인스펙터·`INSPECTOR_TOOLTIPS`와 소스 정책 일치.

- 에셋: `DefaultAreaBundlePool.asset` — ContextMenu `Fill Starter Normal+Easy Bundles`로 Normal 195 반영.
- 원본 집계: `Docs/Blocks2_batches/batch*_hands.json` · `normal_rebuild.json`
- 문서: `phase11.md` · `phases.md` · `IMPLEMENTATIONS.md` · `INSPECTOR_TOOLTIPS.md`(해당 시)
