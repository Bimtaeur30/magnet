# score-logic Phase 5 — Line clear 점수·콤보

> **구현:** `score-logic` · **Jira:** [SCRUM-23](https://bimtaeur30.atlassian.net/browse/SCRUM-23) · **마일스톤:** M7 (로직)  
> **DESIGN:** v0.7 §4.7

## 목표 (완료 기준)

- [ ] `ScoreCalculator` — **지워진 줄 수**(행+열) × 콤보 × `ScoreConfigSO` (square size 제거)
- [ ] 클리어 없음: 배치 칸 수 점수 (기존 유지)
- [ ] 연쇄 웨이브마다 콤보 +1, 턴 종료 시 클리어 없으면 리셋 (기존 구조 유지)
- [ ] `LineClearedEvent` (또는 기존 이벤트 확장) — `scoreAwarded`, clearedLineCount
- [ ] `SquareClearedEvent` deprecated

## 범위 밖

- HUD UI
- 스폰 알고리즘

## 코드·에셋

- `JTH/Scripts/Domain/Score/ScoreCalculator.cs`
- `JTH/Scripts/Domain/Score/ScoreSession.cs`
- `JTH/Scripts/Bootstrap/BoardPlacementBootstrap.cs`
