# sequence22 — Phase 22 변경 기록

## 1 — 2026-08-10 · 접대 피스 allowlist

- 생성: `Scripts/Domain/AreaBundleSpawn/HospitalityPiecePolicy.cs`
  - 심볼: `FitWeight` / `IsAllowed` — 메서드 (추가)
    - 설명: 3칸=0.5, 4–5=1, 1–2·2×2·3×3·6칸=0.
    - 이유: 접대가 너무 쉽게/사각으로 나오지 않게.

- 수정: `Scripts/Domain/AreaBundleSpawn/OpportunityDetector.cs`
  - 심볼: `FindExactFitIds` — 메서드 (수정)
    - 설명: allowlist만 Exact 핏.
  - 심볼: `SumFittingWeight` — 메서드 (추가)
    - 설명: 슬롯 가중 합. `CountFittingSlots` 대체.
  - 심볼: `CompareHoleCoverage` — 메서드 (수정)
    - 설명: 가중 합·구멍별 가중으로 비교.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectHospitality` — 메서드 (수정)
    - 설명: SumFittingWeight 사용.

- 문서: `phase22.md` · `phases.md` · `IMPLEMENTATIONS.md`
