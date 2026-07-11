## 1 — 2026-07-11 · SquareClearDetector Domain

**바뀐 것** — `Domain/Clear/ClearedSquareInfo.cs`, `ClearDetectionResult.cs`, `SquareClearDetector.cs` 생성

**변경 상세 (왜/무엇)**
- `SquareClearDetector`: 자석 중심 홀수 N×N 테두리/내부 판정, 제거 칸 합집합 반환
- `ClearDetectionResult`: 판정 결과 DTO (보드 변경 없음)

**메모** — 자석 `(0,0)` 칸은 판정·점유 대상에서 제외
