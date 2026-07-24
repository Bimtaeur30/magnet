# score-logic Phase 5 — Line clear 점수·콤보

> **구현:** `score-logic` · **Jira:** [SCRUM-23](https://bimtaeur30.atlassian.net/browse/SCRUM-23) · **마일스톤:** M7 (로직)  
> **DESIGN:** v0.7 §4.7 (Block Blast 역분석 공식)

## 목표 (완료 기준)

- [x] `ScoreCalculator` — `λ(n)×base×combo×tier` (`λ=n(n-1)` for n≥2)
- [x] 세션 base — `ScoreConfigSO` min/max 랜덤, `ScoreSession`에 고정
- [x] 클리어 사건당 콤보 +1 (다줄 동일), 배치 칸 점수 항상 가산
- [x] 턴 첫/끝 드롭 플래그로 콤보 유지·2줄+ 구조 예외
- [x] `BoardPlacementBootstrap` — `new ScoreSession(scoreConfig)` + ApplyPlacement 연동
- [x] soft-cap(1.25) **미채택** — 추가 데이터에서 미재현
- [x] UI 콤보: 첫 클리어=0, 두 번째 클리어부터 1 (`Combo = clearIndex - 1`)

## 범위 밖

- HUD UI
- LineClearedEvent 페이로드 확장 (별도)
- 스폰 알고리즘

## 코드·에셋

- `JTH/Scripts/Domain/Score/ScoreCalculator.cs`
- `JTH/Scripts/Domain/Score/ScoreSession.cs`
- `JTH/Scripts/Data/ScoreConfigSO.cs`
- `JTH/ScriptableObjects/DefaultScoreConfig.asset`
- `JTH/Scripts/Bootstrap/BoardPlacementBootstrap.cs`
