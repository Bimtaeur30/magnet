# line-clear-preview-fx Phase 7 — 물방울 스킨

> **구현:** `line-clear-preview-fx`

## 목표 (완료 기준)

- [x] 물방울 스프라이트 3장을 칸마다 랜덤으로 붙인다 (`RandomizeSprites`)
- [x] 클리어 예고만 외곽선이 일렁인다 (`outlineWave`). 상시 연출·메시 전체 말랑 아님
- [x] `WaterDrop` SkinDataSO를 JTH 테스트 스킨 리스트에 등록한다
- [x] 클리어 때 칸마다 물방울 버스트 (`LineClearEffects` 3슬롯)

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `SkinDataSO.RandomizeSprites` / `PickVisualIndex` | 색 id 대신 칸마다 스프라이트 추첨 |
| `InGameSkinManager` | 추첨 인덱스를 칸별로 보관 |
| `BlockShatter.shader` `_OutlineWave` | 림 UV만 왜곡. `_WaterWobble`과 별개 |
| `BlockShatterHint.outlineWave` | 힌트 클립 → MPB |
| `WaterDropHintWave.anim` | `outlineWave` 1 유지 루프. 실루엣 파동은 Vert `_Time` |
| `WaterDrop.asset` | 이름 물방울, 스프라이트 3, 힌트 클립 3, 버스트 3 |

## 범위 밖

- PMS `SkinManager` 인벤토리 리스트 등록
- 배치/클리어 전용 사운드 클립
