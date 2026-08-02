# Phase 4 — 번들 티어 셀렉터 (Relife·Trap·ComboBreak·Easy·Normal)

> **구현:** `block-selection-algorithm` · **Sequence:** [sequence4.md](sequence4.md) · **스펙:** [SPEC.md](SPEC.md) §9.0~9.2·§9.4·§9.6

## 목표

태그별 번들 목록에서 **가중 랜덤 → 랜덤 회전 → 솔버 검증**을 거쳐 번들 1개를 확정하는 셀렉터. 게이트(구간·blame·확률) 판단은 Phase 6 Orchestrator 몫 — 여기서는 "이 보드에서 이 번들이 티어 조건을 만족하는가"만 담당.

## 완료 조건

- [x] `BundleValidation` — Passable / Trap / ComboBreak / Easy / AnyPlaceable 판정 규칙
- [x] `BundleDraw` — 회전 적용된 피스 3개 + 번들 id
- [x] `BundleTierSelector.TryPick` — 가중 추첨(중복 제외) + 검증, probeCount 초과 시 null
- [x] 티어 확률·게이트 튜닝 필드 (`RelifeTurnCount`, `TrapProbability`, `ComboBreakProbability`, `EasyHealthThreshold`, `BundleProbeCount`)
- [x] 검증: Trap(4×4 빈 코너 보드 → trap 번들 통과), ComboBreak(빈 보드 → cb 번들 통과), Relife(Passable 통과)

## 설계 결정

| 결정 | 이유 |
|------|------|
| Easy = Normal 번들 중 `ComboMaintainable == true` 필터 | 스펙 §9.4의 "필터 (§11.2)"는 §11.2(난이도)와 안 맞는 오기로 판단 — 험한 판을 풀어주는 번들 = 클리어까지 가능한 통과 번들 |
| `ComboMaintainable`만 검사 (Easy) | 클리어 가능한 시퀀스 존재 = full sequence 존재를 함의 — 중복 솔버 호출 제거 |
| 검증 실패 번들은 목록에서 제거 후 재추첨 | 같은 번들 반복 검증 방지 — probeCount 안에서 최대한 다른 후보 시도 |
| 회전은 번들당 1회만 배정 | SPEC §15.4 — Draw 시점 회전 고정. 회전 재시도까지 하면 Trap 검증 의미가 흐려짐 |

## 만진 파일

- `Scripts/Domain/BlockSelection/Tiers/BundleValidation.cs` (신규)
- `Scripts/Domain/BlockSelection/Tiers/BundleDraw.cs` (신규)
- `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs` (신규)
- `Scripts/Data/BlockSelectionTuningSO.cs` (수정 — Tier Gates 5필드)

## 범위 밖

게이트 판단·티어 순서(Phase 6), Hospitality/Pressure(Phase 5·6)
