# line-clear-preview-fx Phase 5 — 클리어 조각 파티클

> **구현:** `line-clear-preview-fx`

## 목표 (완료 기준)

- [x] 직접 그린 조각 스프라이트 4~5장
- [x] 실제 클리어 시 칸은 즉시 사라지고, 칸 중심에서 조각이 튀며 페이드
- [x] Default 스킨 7색 `LineClearEffects`에 연결
- [x] 힌트 검은 균열은 유지

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `Graphics/Vfx/BlockShards.png` | 흰 조각 시트 5장 |
| `Prefabs/Vfx/DefaultShardBurst.prefab` | 버스트 베이스. `_0~6`은 색·풀 Item만 다른 바리언트 |
| `PoolItemSO` + `PoolManager` | 풀 등록 |
| `Default.asset` LineClearEffects | 7슬롯 배선 |
| `LineClearHintEffector.PlayBurstForBlock` | 힌트로 바뀐 색(`_appliedSkinId`)으로 줄 전체 버스트 |

## 범위 밖

- 힌트 금과 파티클 조각 1:1
- PTY ParticleEffectManager 코드 변경
- 빛 삼킴 셰이더
