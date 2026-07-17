# Sequence 2 — blocked-ring-dim Phase 2

## 1 — 2026-07-17 · 점유 칸 dim Presentation

**바뀐 것** — 수정: `Block.cs`, `OccupiedCellView.cs`, `PlacedBlocksView.cs`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block.baseColor` — 필드 (추가)
    - 설명: Awake 시점 `spriteRenderer.color`를 보관해 dim 해제 시 복원 기준으로 쓴다.
    - 이유: 스킨/기본 tint를 덮어쓰지 않고 RGB만 어둡게 했다가 되돌리기 위함.
  - 심볼: `Block.dimmed` / `Block.dimMultiply` — 필드 (추가)
    - 설명: 현재 dim 여부와 RGB 배수를 보관한다.
    - 이유: `ApplyVisual` 후에도 같은 dim을 다시 칠하기 위해 상태를 Block에 둔다.
  - 심볼: `Block.SetDimmed(bool, float)` — 메서드 (추가)
    - 설명: dim on이면 `baseColor`의 RGB에 multiply를 곱해 적용, off면 `baseColor` 복원.
    - 이유: 「누를 수 없는 UI처럼 검게」를 SpriteRenderer 색만으로 표현.
  - 심볼: `Block.RefreshColor()` — 메서드 (추가)
    - 설명: `dimmed`/`dimMultiply`에 따라 `spriteRenderer.color`를 다시 쓴다.
    - 이유: `SetDimmed`와 `ApplyVisual`이 동일 경로로 색을 맞추게.
  - 심볼: `Block.ApplyVisual(Sprite)` — 메서드 (수정)
    - 설명: 스프라이트 설정 뒤 `RefreshColor()`를 호출한다.
    - 이유: 스킨 교체 시 dim이 풀리지 않게.

- 파일: `Scripts/Presentation/OccupiedCellView.cs`
  - 심볼: `OccupiedCellView._dimmed` / `_dimMultiply` — 필드 (추가)
    - 설명: 칸 View 단위 dim 상태를 보관한다.
    - 이유: Bind 이후 Block이 바뀌거나 스킨이 다시 적용돼도 동일 상태를 재전달.
  - 심볼: `OccupiedCellView.SetDimmed(bool, float)` — 메서드 (추가)
    - 설명: 상태를 저장하고 `_block.SetDimmed`에 전달한다.
    - 이유: `PlacedBlocksView`가 셀 View만 보고 dim을 제어하게.
  - 심볼: `OccupiedCellView.ApplyVisual(Sprite)` — 메서드 (수정)
    - 설명: 스프라이트 적용 후 저장된 dim을 Block에 다시 건다.
    - 이유: `ApplySkinToAll` 경로에서도 dim 유지.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.blockedRingDimMultiply` — SerializeField (추가)
    - 설명: 비활성 링 점유 칸 RGB 배수(기본 0.35). SO 전 Phase 임시 튜닝.
    - 이유: Phase 3 Config SO 전까지 인스펙터에서 강도 조절.
  - 심볼: `PlacedBlocksView.RefreshBlockedRingDim()` — 메서드 (추가)
    - 설명: `BlockedRingDetector.DetectInactiveSquareSizes`로 비활성 half 집합을 만들고, 각 칸 Chebyshev 링이 포함되면 dim.
    - 이유: Domain 판정과 Presentation 적용의 단일 진입점. Phase 3 Bootstrap이 이 메서드를 호출.
    - 영향: Phase 3 `BoardPlacementBootstrap` 예정.
  - 심볼: `PlacedBlocksView.SyncWithSession()` — 메서드 (수정)
    - 설명: 세션 동기화 끝에 `RefreshBlockedRingDim()` 호출.
    - 이유: Bootstrap 배선 전에도 Sync 경로에서 dim이 갱신되게.

**메모** — Place/재조립 직후 자동 refresh는 Phase 3. 회전은 grill상 생략.

---
