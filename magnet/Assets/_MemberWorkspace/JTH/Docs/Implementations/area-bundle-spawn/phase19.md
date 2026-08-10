# Phase 19 — 올클 고정 번들 풀 + Exact 검증

## 목표

올클 판정에서 빔을 쓰지 않는다. Blocks2 관측 대형·고빈도 핸드 12개를 고정 풀로 두고,  
점유 칸이 적을 때만 Exact(완주 후 보드 빔)로 통과한 패를 지급한다.

## 구현 내용

- `AreaBundleStarterData.CreateAllClear` — 12 bundles
- `AreaBundlePoolSO.allClearBundles` · `allClearMaxOccupied`(16)
- `AreaBundleMetrics.CanEmptyBoard` — DFS Exact, 첫 성공 시 종료
- `Orchestrator` — 올클은 고정 풀 Exact → 확률; 멀티/Area는 기존 빔 유지

## 선정 기준 (우선)

1. Blocks2 hands에 실제 출현
2. 대형 피스(13/35/36/11/22) 포함
3. 셀 합·관측 빈도 상위
4. Exact 예산 위해 **12개**로 제한

## 범위 밖

- 멀티클리어 빔 제거
- Unique 동적 생성 변경
