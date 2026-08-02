# Phase 1 — Domain 비활성 링 판정

> **구현:** `blocked-ring-dim` · **마일스톤:** UX (클리어 직관)  
> **상태:** 구현됨 · 확인 대기  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] `BoardGrid`만으로 N×N 테두리 링 비활성 여부 판정
- [x] 빈칸 0(완성) → 비활성 아님
- [x] 빈칸 ≥1 이고 그중 하나라도 상·좌·우 점유 막힘 → 링 비활성
- [x] 하 = 중앙, 모서리 하 = `(-sign x, -sign y)`, 좌·우 = 테두리 이웃
- [x] `read_console` 컴파일 에러 0

## 구현 내용

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `BlockedRingDetector` | `Domain.Clear` | `DetectInactiveSquareSizes` — 비활성 N 목록(오름차순) |

규칙(grill 확정):

- 막힘 = 빈칸의 **상·좌·우** 인접이 **점유** (하=중앙은 미사용)
- 모서리: 상=바깥 대각, 좌·우=변 쪽 두 이웃
- 링 비활성: 빈칸 존재 + **하나라도** 막힌 빈칸
- OOB는 점유 아님 → 막힘으로 치지 않음

## 이 Phase 범위 밖

- OccupiedCellView dim / Block 색
- Bootstrap 갱신 배선
- Config SO
- 회전·드래그 프리뷰

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 비활성 링 판정 | `Scripts/Domain/Clear/BlockedRingDetector.cs` |
| 테두리 순회 참고 | `Scripts/Domain/Clear/SquareClearDetector.cs` |
| 점유 조회 | `Scripts/Domain/BoardGrid.cs` |
