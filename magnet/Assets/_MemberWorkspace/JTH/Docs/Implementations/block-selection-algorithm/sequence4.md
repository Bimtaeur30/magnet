# Sequence — Phase 4 (block-selection-algorithm)

> **Phase:** [phase4.md](phase4.md) 와 1:1.

## 1 — 2026-08-01 · 번들 티어 셀렉터

**바뀐 것**

- 생성: `Scripts/Domain/BlockSelection/Tiers/BundleValidation.cs`
- 생성: `Scripts/Domain/BlockSelection/Tiers/BundleDraw.cs`
- 생성: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` — Tier Gates 5필드

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Tiers/BundleValidation.cs`
  - 심볼: `BundleValidation` — enum `Passable / Trap / ComboBreak / Easy / AnyPlaceable` (추가)
    - 설명: 번들이 티어 조건을 만족하는지의 판정 규칙 (SPEC §9 각 티어의 "번들 후보" 조건).
    - 이유: 티어마다 셀렉터 메서드를 복제하지 않고 규칙만 바꿔 재사용.

- 파일: `Scripts/Domain/BlockSelection/Tiers/BundleDraw.cs`
  - 심볼: `BundleDraw` — class (`BundleId`, `Pieces`) (추가)
    - 설명: 추첨 확정 결과 — 회전 적용된 피스 3개 + 로그용 번들 id.
    - 이유: Orchestrator가 `BlockSelectionResult.BundleId`(SPEC §16.3)를 채우려면 피스와 id가 함께 필요.

- 파일: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
  - 심볼: `BundleTierSelector.TryPick(board, bundles, validation, rng, probeCount)` — public static (추가)
    - 설명: 가중 랜덤(중복 제외) → 랜덤 회전 → 솔버 검증. 성공 시 `BundleDraw`, probeCount 초과 시 null (fallthrough).
    - 이유: SPEC §9 "후보 중 가중 랜덤 1개, 없으면 fallthrough" + §8.7 "상위 K개만 솔버" (K=probeCount).
    - 영향: Phase 6 Orchestrator의 Relife/Trap/ComboBreak/Easy/Normal 및 최후 fallback이 호출.
  - 심볼: `BundleTierSelector.ToRotatedPieces(bundle, rng)` — private static (추가)
    - 설명: canonical 3개에 각각 `rng.Next(4)` 회전 (SPEC §15.4). 모양 3개 미만·null이면 무효.
  - 심볼: `BundleTierSelector.Validate(board, pieces, validation)` — private static (추가)
    - 설명: 공통 `HasAnyPlacement` 선검사 후 규칙별 분기. Easy는 `ComboMaintainable`만 검사 (클리어 가능 = full sequence 존재 함의).
    - 이유: 모든 티어가 hasAny 필수 (Death 없음, SPEC §3.1). 중복 솔버 호출 최소화.
  - 심볼: `BundleTierSelector.TakeWeighted(remaining, rng)` — private static (추가)
    - 설명: 가중 추첨 1개 후 목록에서 제거. weight는 최소 1로 클램프.
    - 이유: 실패 번들 재추첨 방지 — probeCount 안에서 서로 다른 후보 시도.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `RelifeTurnCount / TrapProbability / ComboBreakProbability / EasyHealthThreshold / BundleProbeCount` — 프로퍼티 5종 (추가)
    - 설명: 티어 게이트 수치 (기본 2 / 0.008 / 0.04 / 0.45 / 8).
    - 이유: SPEC §9.0~9.4 권장 시작값. 소비자는 Phase 6 Orchestrator.

**검증**

- execute_code:
  - Trap: 8×8에서 4×4 빈 코너만 남긴 보드 + trap 번들 → `trap_bulk` 선택 (hasAny ✅ / fullSequence ❌) ✅
  - ComboBreak: 빈 보드 + cb 번들 → `cb_smallmix` 선택 (fullSequence ✅ / comboMaintainable ❌ — 10칸으로 라인 불가) ✅
  - Relife: 빈 보드 + relife 번들 Passable → `relife_combo` 선택 ✅

**메모**

- Easy의 "필터 (§11.2)" 참조는 오기로 판단 — `ComboMaintainable == true` 번들로 해석 (phase4.md 결정).
