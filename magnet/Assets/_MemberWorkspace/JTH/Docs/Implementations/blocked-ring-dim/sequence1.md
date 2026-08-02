# Sequence 1 — blocked-ring-dim Phase 1

## 1 — 2026-07-17 · Domain 비활성 링 판정 추가

**바뀐 것** — 생성: `Scripts/Domain/Clear/BlockedRingDetector.cs`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Clear/BlockedRingDetector.cs`
  - 심볼: `BlockedRingDetector.DetectInactiveSquareSizes(BoardGrid)` — 메서드 (추가)
    - 설명: `squareHalf = 1…boardHalf` 각 테두리를 순회해 비활성인 링의 N(`2*half+1`)을 오름차순 리스트로 반환한다. `grid == null`이면 빈 목록.
    - 이유: Phase 2 Presentation이 「어떤 링의 점유 칸을 어둡게 할지」를 Domain 결과만으로 소비하게 하기 위함.
    - 영향: Phase 2+ Presenter / Bootstrap refresh가 이 API를 호출 예정.
  - 심볼: `BlockedRingDetector.IsRingInactive(BoardGrid, int)` — 메서드 (추가)
    - 설명: 해당 `squareHalf` 테두리 칸을 모아 빈칸이 있는지 보고, 빈칸 중 하나라도 `IsEmptyCellBlocked`면 true. 빈칸이 없으면 false(완성·비활성 아님).
    - 이유: grill 규칙 「빈칸 존재 + 하나라도 막힘 → 링 비활성」을 링 단위로 고정.
  - 심볼: `BlockedRingDetector.IsEmptyCellBlocked(BoardGrid, Vector2Int)` — 메서드 (추가)
    - 설명: 칸의 상(바깥)·좌·우 이웃이 모두 `IsOccupied`이면 true. 하(중앙)는 검사하지 않음.
    - 이유: 「상좌우가 막히면 그 구멍으로 못 들어온다」는 인접 점유(A안) 정의를 Domain에 그대로 옮김.
  - 심볼: `BlockedRingDetector.GetLocalDirs(Vector2Int, …)` — 메서드 (추가)
    - 설명: 변 칸은 축 방향 하·상·좌·우, 모서리는 하=`(-sign x,-sign y)`, 상=반대, 좌·우=`(-sign x,0)`·`(0,-sign y)` 오프셋을 out으로 준다.
    - 이유: 「하=중앙」과 모서리 A안을 한곳에서 계산해 변/모서리 판정이 어긋나지 않게.
  - 심볼: `BlockedRingDetector.ChebyshevFromMagnet(int, int)` — 메서드 (추가)
    - 설명: `max(|x|,|y|)`로 자석 기준 링 반경을 구한다.
    - 이유: `SquareClearDetector`와 동일하게 테두리 칸만 고르기 위함(해당 클래스 private라 복제).

**메모** — OOB 칸은 `BoardGrid.IsOccupied` false → 막힘으로 치지 않음(grill A 그대로). 영구 테스트 폴더 없음.

---
