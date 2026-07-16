# Phase 8 — 재배치: 각도 → 수선 복도 폭

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 구현됨 · **선행:** Phase 7  
> **변경 기록:** [sequence8.md](sequence8.md) (1:1)

## 목표 (완료 기준)

- [x] 각도/부채꼴 제거. 원점–원래칸 **직선 수선 반폭(복도)** 으로 후보 판정
- [x] 복도 안 최소 수선거리 슬롯만 이동(안쪽 우선). 막히면 제자리. 슬롯 없으면 보드밖
- [x] `CorridorHalfWidth` SO (기본 0.75 격자). 기즈모는 복도 사각형
- [x] `read_console` 컴파일 에러 0

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 탐색 | `Scripts/Domain/Clear/CellRelocationTargetFinder.cs` |
| 폭 SO | `Scripts/Data/PlacementConfigSO.cs` → `CorridorHalfWidth` |
| 기즈모 | `Scripts/Presentation/CellRelocationTargetGizmo.cs` |
