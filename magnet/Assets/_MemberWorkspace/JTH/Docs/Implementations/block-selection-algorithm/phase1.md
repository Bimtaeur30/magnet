# Phase 1 — 솔버 (배치 시뮬레이션 + 4종 판정)

> **구현:** `block-selection-algorithm` · **Sequence:** [sequence1.md](sequence1.md) · **스펙:** [SPEC.md](SPEC.md) §8

## 목표

보드 클론 + 피스 3개에 대해 **라인 클리어를 반영**한 판정 4종을 제공하는 순수 Domain 코드.
이후 모든 티어(Trap/ComboBreak/Pressure/Hospitality)가 이 API 위에서 동작한다.

## 완료 조건

- [x] `BoardGrid` — 내부 `bool[,]` 통합 + `Clone()` (시뮬은 클론 대상, 실보드 비오염)
- [x] `PlacementSimulator` — 배치 → `LineClearDetector` 재사용 클리어 시뮬
- [x] `ShapeRotator` — canonical offsets 0/90/180/270° 회전 + 원점 정규화
- [x] `PlacementSolver` — `HasAnyPlacement` / `FullSequenceExists` / `ComboMaintainable` / `CountFullSequences(cap)`
- [x] 컴파일 에러 0 (`read_console`)
- [x] 검증 시나리오 5종 통과 (execute_code — 임시 스크립트 없음)

## 설계 결정

| 결정 | 이유 |
|------|------|
| `BoardGrid` 내부 `bool[8,8]`로 통합, `BoardSnapshot` 제거 | 보드 표현 단일화 — 시뮬·실게임이 같은 타입/클리어 로직 사용 (sequence #2) |
| 시뮬 배치 검사·클리어는 `PlacementService.CanPlace` + `LineClearDetector.Detect` 재사용 | 판정 로직 중복 제거 |
| 클리어는 배치 직후 꽉 찬 행·열 **동시 1패스** | 제거는 칸을 비울 뿐이라 연쇄 불가 (수학적으로) |
| 회전은 피스 생성 시점 적용, 솔버는 회전 탐색 안 함 | 스폰 시 회전 고정 규칙(DESIGN §4.2)과 일치 |
| 피스 = `IReadOnlyList<Vector2Int>` (별도 `Piece` 클래스 없음) | 스폰 파이프라인(`Drawer`/`ShapeBlockData`)과 동일 표현 — shapeId 사용처 없음 (sequence #3) |
| `CountFullSequences(cap)` 조기 종료 | unique 판정은 cap=2로 충분 |
| 동일 형태 피스의 depth 내 중복 시도 스킵 | 같은 3x3 3개 등에서 시퀀스 중복 카운트 방지 |
| Unity 의존 `Vector2Int`만 | Domain 순수성 (이벤트·DI·SO 없음) |

## 만진 파일

- `Scripts/Domain/BlockSelection/Simulation/ShapeRotator.cs` (신규)
- `Scripts/Domain/BlockSelection/Simulation/PlacementSimulator.cs` (신규)
- `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs` (신규)
- `Scripts/Domain/Board/BoardGrid.cs` (수정 — `bool[,]` + `Clone()`)
- `Scripts/Domain/Clear/LineClearDetector.cs` (수정 — 디버그 로그 제거)
- `Scripts/Domain/Placement/PlacementService.cs` (수정 — `CanPlaceAnywhere` 추가)
- `Scripts/Domain/Turn/TurnService.cs` (수정 — `CanPlaceAnywhere` 사용)

## 범위 밖

BoardHealth, Blame, 번들, 티어 셀렉터, Drawer 연동 (Phase 2~7)
