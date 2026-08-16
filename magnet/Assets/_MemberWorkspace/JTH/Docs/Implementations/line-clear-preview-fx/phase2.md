# line-clear-preview-fx Phase 2 — 스킨 클립 힌트 + 클리어 이펙트

> **구현:** `line-clear-preview-fx`

## 목표 (완료 기준)

- [x] 스킨 바리에이션에 클립·칸 이펙트(옵션) + 가운데 1발 bool/슬롯
- [x] 스냅 시 클리어 줄의 보드 칸 + 반투명 프리뷰만 프리뷰 `SkinId`로 스프라이트 통일, 클립 루프
- [x] 스테이징 블록은 힌트 영향 없음
- [x] 스냅 해제 시 보드 칸 원복, 배치 시 통일 유지
- [x] 알파 숨쉬기 제거
- [x] 실제 클리어 때만 이펙트. 모든 칸 동시 / 줄마다 가운데 1발(가로 0°·세로 90°)

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `SkinDataSO` | 바리에이션 클립·칸 이펙트, 가운데 1발 설정 |
| `Block.SetClearHint` | 스프라이트 오버라이드 + Playable 클립 루프 |
| `LineClearHintEffector` | 스냅 힌트 소유, 스킨 채널 캐시 |
| `LineClearExplosionPresenter` | 배치 클리어 이펙트 Raise |
| `PlacementResult.SkinId` | 놓은 피스 바리에이션을 클리어 FX에 전달 |
| `Block` Skin `Animator` | Playable 출력 타깃 |

## 범위 밖

- 스킨별 클립/이펙트 에셋 제작 (인스펙터에 넣으면 재생)
- 기존 알파 숨쉬기 Config 튜닝 (코드 경로에서 제거)
