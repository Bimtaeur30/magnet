# Phase 7 — 재배치: 자석 원점 부채꼴 + 보드밖 주차

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 구현됨 · **선행:** Phase 6  
> **변경 기록:** [sequence7.md](sequence7.md) (1:1)  
> **grill-me:** 2026-07-16 확정

## 목표 (완료 기준)

- [x] 원점=`(0,0)`, 축=자석→원래 칸. **±HalfSectorDegrees ∩ 서클**만 (각 확장 없음)
- [x] 선택: 축 각 최소 → 자석 최근접 → 시계
- [x] 후보 없으면 같은 축 **보드 밖 최근접 빈칸** 주차
- [x] 기즈모: 노란 이젝터 칸 제거, `(0,0)` 기준 부채꼴·서클·후보·최종·보드밖
- [x] `read_console` 컴파일 에러 0

## 구현 내용

| 클래스 | 변경 |
|--------|------|
| `CellRelocationTargetFinder` | 원점/축 반전, 단일 설정각, OutsidePark 복구, 순위=각→거리→시계 |
| `CellRelocationTargetGizmo` | 노란 칸 제거, 자석 원점 시각화 |
| `PlacementConfigSO` | HalfSector 툴팁 (90° 확장 문구 삭제) |

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 목표 탐색 | `Scripts/Domain/Clear/CellRelocationTargetFinder.cs` |
| 기즈모 | `Scripts/Presentation/CellRelocationTargetGizmo.cs` |
