# Phase 1 — 핵심 체인 이식 (카탈로그·파이프라인·Drawer 교체)

## 목표

BlockBlast! 역공학 핸드오프의 추천 파이프라인을 기존 9-티어 스택 대신 블록 공급 알고리즘으로 사용한다.

## 구현 내용

### 파이프라인 (핸드오프 §3·§5·§11)

1. **base 전략** = 공개 ID 7 (random-no-death)
2. **AlgoFillSortEdgeTrait** (§5.1): round 2부터 90% 확률로 7 → **1370** 교체
3. **1370 근사**: 네이티브 미복원 → 셔플 풀에서 "완주 가능 + 라인 클리어 포함" 조합 우선 탐색, 없으면 완주만 가능한 조합, 그마저 없으면 randomNoDie로 강등
4. **randomNoDie** (§11): ID 2~30 셔플 → 완주 가능 조합 탐색 (100ms + 150조합 예산), 실패 시 [1, random-placeable, 1] fallback
5. **ContinueSameMoreRoundLimitTrait** (§5.2): 최근 2라운드 트리플과 다중집합 2개 이상 겹치면 겹친 슬롯만 재추첨 → actualId **2100**
6. **delCurrentSameBlock** (§11): 직전 트리플과 완전 동일하면 가운데 블록을 produceRandomId로 교체

### 42-ID 블록 카탈로그 (§7)

- 행별 비트마스크 → cell offset 변환 (bit 0 = 왼쪽 셀, row = y)
- 회전형이 각각 별도 ID → **스폰 시 추가 회전 없음** (기존 시스템과의 차이점)
- 풀: no-die 2~30 / produceRandomId 2~42 / 1370 근사 2~42 − 미관측 10종

### 연동

- `BlockBlastDrawer : AbstractDrawer` 신설, `BlockSpawnBootstrap`에서 `BlockSelectionDrawer` 대신 배선
- 솔버는 기존 `PlacementSolver.FullSequenceExists / ComboMaintainable` 재사용 (라인 클리어 시뮬 포함)
- Bootstrap의 구 알고리즘 입력 준비(BoardHealth·BlameTracker·프로브 피스·턴 정산 로그) 제거

## 범위 밖

- 세션 시간·광고 보상·초보자 규제·점수 기반 Trait (관측 500건에서 미발동)
- 미복원 난이도 알고리즘 4001/4007/4010 계열
- 구 `BlockSelection` 코드 삭제 (롤백 대비 보존)
- 500건 데이터와의 자동 분포 검증 (수동 플레이 확인으로 대체)

## 코드·에셋 맵

| 경로 | 역할 |
|------|------|
| `Scripts/Domain/BlockBlast/BlockBlastCatalog.cs` | 42-ID 비트마스크 카탈로그 + 풀 (신규) |
| `Scripts/Domain/BlockBlast/BlockBlastAlgorithm.cs` | 파이프라인 본체 (신규) |
| `Scripts/Domain/BlockBlast/BlockBlastSelection.cs` | 선택 결과 DTO (신규) |
| `Scripts/Domain/Spawn/BlockBlastDrawer.cs` | AbstractDrawer 구현 (신규) |
| `Scripts/Bootstrap/BlockSpawnBootstrap.cs` | Drawer 배선 교체 (수정) |
