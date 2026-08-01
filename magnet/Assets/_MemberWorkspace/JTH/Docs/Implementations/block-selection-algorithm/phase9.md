# Phase 9 — Block Blast 사진 분석 반영 (실게임 역설계)

> **구현:** block-selection-algorithm · **선행:** Phase 8
> **근거:** `Docs/BLOCKBLAST_ANALYSIS.md` — 실게임 스크린샷 344장 전수 분석 (10 에이전트 병렬)

## 목표

1. **대각선 완전 제거** — 실게임 344프레임에서 대각선 조각 0건. Diag2·Diag3 전 티어 가중 0 + 번들에서 제외.
2. **3x3·3x2 상향** — 실게임에서 3x3이 매우 흔함 (중복 ×2·×3 포함).
3. **중복 허용 번들** — 실게임은 한 핸드에 같은 모양 2~3개가 일상. 중복 번들 신설.
4. **Momentum 티어 신설** — 실게임은 클리어 흐름 중 큼직한 사각 패를 계속 지급 ("기분 좋은 패"의 정체). 직전 턴 클리어 시 확률적으로 큼직한 번들.
5. **밀도 바이어스** — 실게임은 빈 보드→대형, 빽빽→얇은 조각. fillRate 기반 번들 가중 배수.

6. **Normal·Easy 독립 추첨 전환** (entry 2) — 고정 번들로는 실게임의 자유 조합을 재현 불가. 모양 가중표에서 슬롯 3개 독립 추첨(중복 허용)으로 교체, `normal_*` 번들 13종 삭제. 번들은 특수 티어(Trap·ComboBreak·Relife·Momentum) 전용.

7. **~~새 게임 프리필 보드~~** (entry 10~15) — 구현 후 사용자 결정으로 **전체 제거** (entry 15). 빈 보드 시작 유지. 파생 기능인 빽빽할 때 큰 블록 감점(`DenseBigPenalty`)은 존치.

## 범위 밖

- 백(bag) 가뭄 방지, 회전 지정 번들 (후속 후보)

## 코드·에셋 맵

| 대상 | 변경 |
|------|------|
| `Bundles/BundleTag.cs` | Momentum 태그 |
| `SelectionTier.cs` | Momentum 티어 |
| `BlockSelectionOrchestrator.cs` | Momentum 게이트 + 밀도 배수 + 모양 특성 캐시 |
| `Spawn/BlockSpawnContext.cs` · `BlockSelectionDrawer.cs` | LastTurnClearedCells 전달 |
| `Bootstrap/BlockSpawnBootstrap.cs` | 클리어 칸 수 계산 |
| `Data/BlockSelectionTuningSO.cs` | Momentum·Density 5필드 |
| 신규 번들 7종 | mom_bigsquares·mom_squarefeast·mom_bigtriple·mom_rects·normal_twinlines·normal_tripleL·normal_twinsquare |
| 수정 에셋 | 튜닝(가중치)·풀(diag 제외)·normal_corner·cb_smallmix |
| `Domain/Board/BoardPrefillGenerator.cs` (신규) | 프리필 배치 생성 (라인 완성·dead zone 거부) |
| `Presentation/PlacedBlocksView.cs` · `GameBoard.cs` | `CreatePrefillBlocks` · `PrefillPiece` — 풀 Pop + 스킨 이벤트 + 배치 |
| `Prefabs/Board/Placed Blocks View.prefab` | blockItemSO 참조 추가 |
