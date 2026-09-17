# Phase 1 — 얼음 결정 파편

## 목표 · 완료 조건

- 공용 `ThemeBurst` ice 모드를 쓰지 않는 전용 `IceCrystalBurst` 셰이더를 만든다.
- Ice 8색이 전용 머티리얼·프리팹·PoolItemSO를 통해 각자 재생된다.
- 기존 `SkinDataSO`와 `PoolManagerSO`가 새 PoolItemSO를 참조한다.

## 구현

| 대상 | 책임 |
|---|---|
| `IceCrystalBurst.shader` | 두 겹의 각진 결정, 청록 림광과 백색 스파클을 그린다. |
| `Materials/IceCrystalBurst_0~7.mat` | 기존 색상별 tint를 유지하며 전용 셰이더를 사용한다. |
| `Prefabs/IceCrystalBurst_0~7.prefab` | 기존 검증된 burst 물리 파라미터를 유지하고 전용 머티리얼을 렌더한다. |
| `Pool/IceCrystalBurst_0~7.asset` | 새 프리팹을 ObjectPool에 제공한다. |
| `Ice.asset`, `PoolManager.asset` | 기존 얼음 PoolItemSO 참조를 새 PTY 에셋으로 교체한다. |

## 범위 밖

- 다른 스킨의 파괴 연출 교체
- Block 프리팹, Animation Event, 게임플레이 코드 변경
