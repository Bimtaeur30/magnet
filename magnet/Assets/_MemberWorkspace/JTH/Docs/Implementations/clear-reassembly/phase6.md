# Phase 6 — 재배치 후보 칸 기즈모

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 구현됨 · **선행:** Phase 5  
> **변경 기록:** [sequence6.md](sequence6.md) (1:1)

## 목표 (완료 기준)

- [x] Scene 기즈모로 재배치 가능 칸(단계 후보 ∪ 연쇄 TryFind)을 표시한다
- [x] `(0,1)` 시드 점유 시 `(0,2)` 등 다음 목표가 합쳐져 보인다
- [x] `read_console` 컴파일 에러 0

## 구현 내용

| 클래스 | 변경 |
|--------|------|
| `CellRelocationTargetFinder` | `CollectMatching` / `CollectAllStageCandidates` / `CollectSequentialTargets` |
| `CellRelocationTargetGizmo` | 빈 보드·시드·연쇄 후보 합쳐 OnDrawGizmos |

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 후보 수집 | `Scripts/Domain/Clear/CellRelocationTargetFinder.cs` |
| 기즈모 | `Scripts/Presentation/CellRelocationTargetGizmo.cs` |
| 씬 오브젝트 | `Scenes/Phase0_Bootstrap` → `CellRelocationTargetGizmo` |
