# Phase 38 — 찬 Area 직교볼록 홈 절단 + Area 기즈모

## 목표

다리절단을 제거하고, U형 홈처럼 행/열이 두 구간으로 끊긴 찬 Area만 잘라 두 덩어리로 나눈다. 계단형 L(직교볼록)은 한 Area로 유지한다. 분할 결과를 Scene 기즈모로 본다.

## 범위

1. `SplitAtBridges` / 다리절단 경로 삭제
2. 찬 칸: 4연결 → 직교볼록(행·열 run≤1) 검사 → 다중 run 갭에서 균형 축절단 → 재귀
3. 빈 칸: 기존 4연결 유지
4. `AreaScoreCalculator.Partition` 공개 + `AreaPartition`
5. `AreaBundleSelectionGizmo`에 찬 Area 오버레이 (별도 AreaPartitionGizmo 없음)

## 비범위

- depth 임계 튜닝(직교볼록 이분법만)
- 빈 칸 홈 절단
- 점수식 계수 변경

## 수락

- [x] 계단/L(직교볼록) 찬 Area 1개
- [x] U홈 찬 Area 2개 이상
- [x] 다리절단 코드 없음
- [x] 기즈모로 찬/빈 Area 구분 표시
- [x] 최적의 수 기즈모(`AreaBundleSelectionGizmo`)에서 Area 표시
- [x] 컴파일 오류 없음
