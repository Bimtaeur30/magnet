# 원작 시작 보드 프리필 분석 + 구현 방침

> **날짜:** 2026-08-18 분석 · 2026-09-03 재도입·구현
> **상태:** 구현됨 (`board-start-prefill`). 새 게임 시작 시 8×8 보드를 미리 채운다.
> **코드:** `Domain/Board/BoardPrefillGenerator.cs` · `Data/BoardPrefillConfigSO.cs` ·
> `Presentation/GameBoard.PrefillCells` · `Presentation/PlacedBlocksView.SpawnCells` ·
> `Bootstrap/BlockSpawnBootstrap.PrefillBoard`
> **자료:** `Docs/Blocks2_batches/_blocks_root_up/` 점수 0 프레임 (954, 1240, 1516)

## 뭘 봤나

원작 Block Blast가 **점수 0인 새 게임에서 판을 미리 채워 두는 방식**을 스크린샷으로 역추적했다. 핸드 스폰과 별개다.

## 관측 (점수 0 시작 3판)

| 프레임 | 채움 | 색 | 완성 줄 | 빈 칸 | 첫 손 |
|--------|------|----|---------|-------|--------|
| 954 | 33/64 (52%) | 녹/빨/주 | 없음 | 구멍 여러 개 | Z4, L4, J4 |
| 1516 | 33/64 (52%) | 빨/녹 | 없음 | 중앙 큰 빈 칸 | T4, L4, J4 |
| 1240 | 42/64 (66%) | 보/노 | 오른쪽이 거의 가득 | 세로 복도 | T, S/Z, T |

세 판 배치가 전부 다르다 → 고정 프리셋이 아니라 **매판 생성**. 채움률 대략 **50~65%**.

## 구현 방침 (2026-09-03 확정)

채택한 방식은 **칸 단위 확률 채움 + 번들 모양 구멍 뚫기**다:

1. 빈 8×8의 **칸마다 독립 확률**(`FillProbability`, 기본 0.6)로 채운다.
2. Normal 번들 중 큼지막하지 않은 것 하나를 골라, 그 3피스 모양대로 **구멍을 뚫어** 시작 직후 막히지 않게 한다 (`HoleClusterRadius`로 흩음).
3. 구멍 뚫고도 빈 칸이 `MinEmptyCellsAfterHole` 미만이면 재생성 (`MaxGenerateAttempts`회).
4. 색(`skinId`)은 칸마다 랜덤(`SkinSession.MaxVariant` 범위). 피스 단위 색 아님.
5. 완성 줄이 생겨도 그대로 둔다.
6. **첫 손 3개는 이 채워진 보드를 기준으로** 뽑힌다 (`PrefillBoard()` → `Fill()` 순서).

> **이전 분석의 "결론(피스 랜덤 배치 + 피스 단위 색)"·"아닌 것" 항목은 폐기.**
> 원작 픽셀 완벽 재현이 목적이 아니라 "빈 보드로 시작하지 않는다"가 목적이라,
> 튜닝 노브가 단순하고 결과가 예측 가능한 칸-확률 방식으로 간다. 색 덩어리 느낌이
> 필요해지면 그때 피스 단위 채움/색으로 확장한다.

## 튜닝

`DefaultBoardPrefillConfig.asset`에서 조정. `Seed >= 0`이면 매판 고정(QA·디버그 재현용), `-1`이면 랜덤.
`Enabled = false`면 예전처럼 빈 보드로 시작.
