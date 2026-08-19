# line-clear-preview-fx Phase 6 — 스킨 배치/클리어 사운드

> **구현:** `line-clear-preview-fx`

## 목표 (완료 기준)

- [x] `SkinDataSO`에 `PlaceSound` / `LineClearSound` 슬롯 (색 id 배열 아님)
- [x] 놨을 때 스킨 배치음 1발, 비면 기존 place
- [x] 터질 때 스킨 클리어음 1발, 비면 기존 explode
- [x] 다른 AI가 `Docs/SKIN.md`로 스킨 제작 절차를 알 수 있음

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `SkinDataSO.PlaceSound` | 배치 1발 |
| `SkinDataSO.LineClearSound` | 클리어 1발 |
| `BoardPlacementBootstrap` | Place 성공 시 두 소리 Resolve |
| `Docs/SKIN.md` | 스킨 제작 가이드 |

## 범위 밖

- 실제 오디오 클립 제작·SkinData 슬롯 배선 (에셋만 넣으면 재생)
- 클리어 예고(드래그 힌트) 전용 사운드
