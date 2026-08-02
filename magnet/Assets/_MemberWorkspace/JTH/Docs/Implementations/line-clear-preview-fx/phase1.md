# line-clear-preview-fx Phase 1 — 프리뷰 라인클리어 힌트

> **구현:** `line-clear-preview-fx`

## 목표 (완료 기준)

- [x] 보드 스냅 프리뷰 시 가상 배치로 클리어 라인 검출 (실보드 미변경)
- [x] 클리어될 **줄 칸만** (보드 블록 + 그 줄 위 프리뷰 칸) 밝기·알파 숨쉬기
- [x] Preview &lt; Placed &lt; Staging sortingOrder 밴드
- [x] `LineClearHintEffector` on `PlacedBlocksView`
- [x] Config SO로 밝기/알파/주기 조절

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `LineClearPreviewDetector` | Clone 보드 가상 배치 → `LineClearDetector` |
| `LineClearPreviewConfigSO` | 밝기·알파·펄스 튜닝 |
| `Block.ApplySortingBand` / `SetClearHint` | 그리기 밴드 + 틴트 숨쉬기 |
| `LineClearHintEffector` | 힌트 on/off 소유 |
| `PlacedBlocksView` / `GameBoard` | Effector 위임 |
| `BlockDragInput` | 시뮬 → 클리어 칸 + 해당 프리뷰만 전달 |

## 범위 밖

- HDR Emission / bloom 셰이더
- 스파크 파티클
- 별도 네모 광원 스트립
- 실제 클리어 폭발 VFX
