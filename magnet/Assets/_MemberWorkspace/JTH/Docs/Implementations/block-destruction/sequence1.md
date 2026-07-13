## 1 — 2026-07-11 · SquareClearDetector Domain

**바뀐 것** — `Domain/Clear/ClearedSquareInfo.cs`, `ClearDetectionResult.cs`, `SquareClearDetector.cs` 생성

**변경 상세 (왜/무엇)**
- `SquareClearDetector`: 자석 중심 홀수 N×N 테두리/내부 판정, 제거 칸 합집합 반환
- `ClearDetectionResult`: 판정 결과 DTO (보드 변경 없음)

**메모** — 자석 `(0,0)` 칸은 판정·점유 대상에서 제외

---
## 2 — 2026-07-13 · ClearedCells 칸 중복 제거

**바뀐 것** — `SquareClearDetector.cs` 수정

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Clear/SquareClearDetector.cs`
  - 심볼: `SquareClearDetector.Detect` — 메서드 (수정)
    - 설명: 사각형별 `ClearedCells`를 `cellsToRemove`에 넣을 때 `HashSet.Add`가 성공한 칸만 남긴다. 새 칸이 0개면 해당 `ClearedSquareInfo`는 결과에 넣지 않는다.
    - 이유: 동시 클리어 시 같은 칸이 여러 `ClearedSquareInfo.ClearedCells`에 중복 들어가는 것을 막기 위해.
    - 영향: `ClearDetectionResult.ClearedSquares` / `CellsToRemove` 소비자 (Bootstrap·점수).
