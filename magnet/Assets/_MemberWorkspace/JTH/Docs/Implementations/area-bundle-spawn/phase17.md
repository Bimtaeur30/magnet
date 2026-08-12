# Phase 17 — 변(side) 보너스 제거 · Area 개수 패널티 복구

## 목표

Phase16에서 잘못 뺀 Area 개수 패널티를 되돌리고, 의도에 맞게 **변 보너스**를 점수식에서 제거한다.

## 구현 내용

- `sideBonusIdealMax` / `sideBonusAtIdeal` / `sideBonusPerTwoSides` 삭제
- `SideBonus` / `CountOrthogonalSides` 및 edge union 헬퍼 삭제
- `AreaComponentScore`에서 `SideCount`/`SideBonus` 제거 (`Total = BaseScore`)
- `areaCountPenalty`·`AreaCount`/`AreaCountPenalty` 복구
- `Total = base − rectPenalty − areaCountPenalty`

## 범위 밖

- size base(empty/filled), 직사각 패널티, Unique/Clear Priority 변경 없음
