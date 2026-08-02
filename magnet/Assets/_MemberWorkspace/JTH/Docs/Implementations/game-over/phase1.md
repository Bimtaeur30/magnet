# game-over Phase 1 — 3후보 배치 불가



> **구현:** `game-over` · **Jira:** [SCRUM-22](https://bimtaeur30.atlassian.net/browse/SCRUM-22) · **마일스톤:** M4  

> **DESIGN:** v0.7 §4.6



## 목표 (완료 기준)



- [ ] `PlacementPossibilityChecker` — 3슬롯 × 모든 pivot × (스폰 고정 회전) — `GridPlacementValidator` 재사용

- [ ] 검사 시점: 리필 직후(`TurnStarted`), 슬롯 소모 후(선택적)

- [ ] 하나도 불가 → `GameOverEvent` + `FinalScore`

- [ ] v0.6 `HasCellsOutsideBounds` / `BoundaryViolationEvent` 호출 제거



## 범위 밖



- 게임오버 UI (M7)

- 스폰 알고리즘



## 코드·에셋



- `JTH/Scripts/Domain/` (신규 checker)

- `JTH/Scripts/Bootstrap/BlockSpawnBootstrap.cs` 또는 `BoardPlacementBootstrap.cs`


