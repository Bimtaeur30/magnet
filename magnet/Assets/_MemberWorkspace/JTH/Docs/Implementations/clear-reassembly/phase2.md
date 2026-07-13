# Phase 2 — 부채꼴 목표 배정 + 연쇄 웨이브 Domain

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 계획됨 · **선행:** Phase 1  
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [ ] 한 번 폭발 후: 테두리 파괴 → 바깥 셀 목표 배정 → Domain 격자 이동까지 **웨이브 단위 선확정**
- [ ] 목표 규칙: 자석 방향 **전체 90° 부채꼴** ∩ 서클(반지름=자석까지 거리) 안 **미예약 빈칸** 중 **자석에 가장 가까운 칸**
- [ ] 동률: 부채꼴 축에 더 가까운 각 → 그래도 같으면 12시 기준 시계방향 우선
- [ ] 배정 순서: Chebyshev `half+1` 링부터 바깥으로, 링 안은 **12시→시계방향**
- [ ] 배정 즉시 **출발 칸 비움**, 목표 칸 **예약(점유)**
- [ ] 후보: 보드 안 빈칸만. 자석 `(0,0)` 제외. 테두리 메우기 최적화 없음
- [ ] 웨이브 종료 후 다시 최내곽 클리어 검사 → 없으면 종료. 있으면 다음 웨이브 (Domain에서 연쇄 끝까지)
- [ ] 결과: `ClearWave[]` — 각 웨이브에 `DestroyedCells`, `Relocations(cellId, from, to)`, 점수 칸=Destroyed
- [ ] Presentation/물리 Overlap **없음** (Domain 격자 연산만)
- [ ] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `CellRelocationTargetFinder` | `Domain.Clear` | 부채꼴+거리로 목표 빈칸 1개 |
| `CellRelocationOrder` | `Domain.Clear` | 링·시계 정렬 |
| `ClearReassemblyService` | `Domain.Clear` | 웨이브 루프: 최내곽 파괴 → 배정·이동 → 재검사 |
| `ClearWave` / `CellRelocation` | `Domain.Clear` | 불변 결과 DTO |
| `SquareClearService` | `Domain.Clear` | Phase 1 파괴를 재조립 서비스에 통합하거나 위임 |

### 배정 알고리즘 (요약)

1. `half = (N-1)/2` 폭발 후, 점유 칸 중 `Chebyshev > half` 가 이젝터
2. 이젝터를 `(ring asc, clock-from-12 asc)` 정렬
3. 각 이젝터에 대해:
   - 축 = 자석←셀 방향(또는 셀→자석)
   - 빈칸 중: 보드 안 ∧ 미예약 ∧ 각도∈±45° ∧ `dist(cell,ejector) ≤ dist(magnet,ejector)`
   - 그중 자석 거리 최소 1칸 선택 → 예약, 출발 비움, `Relocation` 기록, Domain 좌표를 `to`로 갱신
4. Assert: 후보는 항상 존재 (폭발로 빈칸 생김 전제)

### 궤도 연출 수치

Domain은 `from`/`to`만. 튕김 3칸·공전 반지름 등은 Phase 4 SO.

## 이 Phase 범위 밖

- Bootstrap 입력 잠금·이벤트 Raise (Phase 3)
- LitMotion 달팽이 (Phase 4)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| Phase 1 클리어 | `Scripts/Domain/Clear/*` |
| 세션 셀 | `Scripts/Domain/BoardSession.cs` |
| 좌표 | `Scripts/Domain/BoardCoordinates.cs` |
