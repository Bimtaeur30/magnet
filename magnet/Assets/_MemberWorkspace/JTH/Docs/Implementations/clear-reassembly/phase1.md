# Phase 1 — OccupiedCell 전환 + 최내곽 N만 파괴

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 계획됨  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [ ] 부착 성공 시 Domain에 **칸 단위** `OccupiedCell`만 등록 (`PlacedBlock` 멀티칸 목록 제거)
- [ ] `BoardSession` 회전·점유 재구성이 셀 목록 기준으로 동작
- [ ] 클리어 판정: 완성된 N 중 **가장 안쪽(최소 N) 하나만** 선택
- [ ] 파괴 범위: 그 N의 **테두리 칸만** 제거 (바깥 점유 칸은 **아직 그대로** — 재배치는 Phase 2)
- [ ] `SquareClearService` / 관련 결과가 “테두리+바깥 합집합 제거”를 더 이상 하지 않음
- [ ] 배치·회전 Bootstrap이 새 셀 모델로 컴파일·동작 (기존 턴은 클리어 시 테두리만 사라지고 바깥은 잔류)
- [ ] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

### Domain 모델

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `OccupiedCell` | `Domain.Placement` (또는 `Domain`) | `CellId`, 절대좌표 `Position`. 부착 후 유일한 보드 유닛 |
| `BoardSession` | `Domain` | `_placedBlocks` → `_cells`. `AddPlacedBlock` 대신 셰이프 칸마다 `AddCell` / `AddCellsFromShape` |
| `PlacedBlock` | `Domain.Placement` | **삭제** 또는 후보 전용으로 강등(보드 부착 경로에서 미사용) |

부착 흐름:

1. `BlockPlacementService.TryPlace` 성공 시 shape의 절대 칸 목록 산출
2. 칸마다 새 `CellId` 발급 → `BoardSession`에 등록 + `BoardGrid` 점유
3. 이벤트 페이로드는 칸 목록 유지 (`BlockId`가 있으면 “배치 트랜잭션 id” 정도로만 쓰거나 셀 id 목록으로 전환 — 기존 리스너 최소 수정)

### 클리어 (파괴만, 재배치 없음)

| 클래스 | 변경 |
|--------|------|
| `SquareClearDetector` | 완성 N 목록 중 **최소 N(최내곽) 1개만** 반환. 합집합 동시 클리어 제거 |
| `SquareClearService` | 해당 N **테두리 칸만** 세션에서 제거. 바깥 칸 `SetOccupied` 해제 **금지** |
| `ClearDetectionResult` / `ClearedSquareInfo` | “바깥 제거 칸” 필드 제거 또는 비움. `DestroyedCells` = 테두리만 |
| `BoardSession` 제거 API | 셀 id/좌표로 `OccupiedCell` 삭제. offsets 부분 파괴 로직 불필요(이미 칸 단위) |

### 점수 계약 (로직만 준비)

- 이번 웨이브 점수 칸 = 파괴된 테두리 칸 수 (SCRUM-23 본구현 전에도 결과에 칸 수 노출)

## 이 Phase 범위 밖

- 부채꼴 목표 배정·재배치·연쇄 루프 (Phase 2)
- 입력 잠금·이벤트 분리 `CellsRelocated` (Phase 3)
- 칸 View 분해·달팽이 LitMotion (Phase 4)
- `DESIGN`상 재배치 연출 수치 SO

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 세션·점유 | `Scripts/Domain/BoardSession.cs` |
| 기존 배치 블록 | `Scripts/Domain/Placement/PlacedBlock.cs` |
| 클리어 판정 | `Scripts/Domain/Clear/SquareClearDetector.cs` |
| 클리어 서비스 | `Scripts/Domain/Clear/SquareClearService.cs` |
| 턴 Bootstrap | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |

## 메모

- 이 Phase 직후 플레이 감각: 테두리만 사라지고 **바깥이 떠 있는** 상태가 됨. 의도된 중간 단계.
- `block-destruction` Phase 1~4의 “바깥 전부 제거” 경로는 이 Phase에서 Domain 기준으로 폐기.
