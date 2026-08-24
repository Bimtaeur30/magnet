# line-clear-preview-fx Phase 8 — 꿀벌집 스킨

> **구현:** `line-clear-preview-fx`

## 목표 (완료 기준)

- [x] 꿀 톤 스프라이트 3장을 칸마다 랜덤으로 붙인다 (`RandomizeSprites`)
- [x] 클리어 예고만 짓눌려 일그러진다 (`squash`). 상시 연출 아님. 눌림→복원 루프
- [x] `Honeycomb` SkinDataSO를 JTH 테스트 스킨 리스트에 등록한다
- [x] 클리어 때 칸마다 꿀이 퍼지는 스플랫 (`LineClearEffects` 3슬롯)

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `BlockShatter.shader` `_Squash` | Vert에서 사방 일그러짐+테두리 넘침. Frag에서 육각 UV 왜곡 |
| `BlockShatterHint.squash` | 힌트 클립 → MPB |
| `HoneycombHintSquash.anim` | `squash` 눌림→복원 루프 1.45s. Transform 스케일은 1 유지 |
| `HoneyBurst.shader` | 점성 있는 꿀 방울 파티클 |
| `Honeycomb.asset` | 이름 꿀벌집, 스프라이트 3, 힌트 클립 3, 버스트 3 |

## 범위 밖

- PMS `SkinManager` 인벤토리 리스트 등록
- 배치/클리어 전용 사운드 클립
