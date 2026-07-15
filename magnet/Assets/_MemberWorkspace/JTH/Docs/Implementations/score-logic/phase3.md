# Phase 3 — 턴 콤보 리셋 + GameOver·베스트·스킨 체크

> **구현:** `score-logic` · **Jira:** [SCRUM-23](https://bimtaeur30.atlassian.net/browse/SCRUM-23) · **마일스톤:** M7 (로직)  
> **상태:** 구현됨 · 컴파일 확인됨  
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 목표 (완료 기준)

- [x] `TurnEndedEvent` → `ScoreSession.NotifyTurnEnded` (턴 중 클리어 0이면 콤보 0)
- [x] 경계 GameOver → `GameOverEvent.Init(TotalScore)` (베스트는 PTY `SaveBridge.SubmitScore`)
- [x] `SkinUnlockCheckEvent.Init(Score, TotalScore)` — 배치 점수 반영 후 + GameOver 시
- [x] `Docs/DESIGN.md` §4.7·턴 메모 동기화
- [x] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

### 흐름

```
핸드 전부 소진 → TurnEnded
  → BoardPlacementBootstrap.OnTurnEnded → NotifyTurnEnded()

배치 성공 → ApplyPlacement → ScoreChanged(TotalScore)
  → SkinUnlockCheck(Score, TotalScore)

경계 GameOver → SkinUnlockCheck(Score, TotalScore)
  → GameOverEvent(TotalScore)
```

### 심볼

| 심볼 | 책임 |
|------|------|
| `OnEnable` / `OnDisable` | `TurnEndedEvent` 구독/해제 |
| `OnTurnEnded` | `NotifyTurnEnded` |
| `RaiseScoreSkinUnlockCheck` | Score 타입 해금 체크 Raise |
| `TryConfirmPlacement` GameOver 분기 | `FinalScore` = 세션 총점 |

## 이 Phase 범위 밖

- 후보 4개 전부 배치 불가 GameOver (`game-over` / SCRUM-22)
- HUD UI / `ScoreSession.Reset` 재시작
- PTY·PMS·KTJ 코드

## 코드·에셋 맵

| 보려는 것 | 경로 |
|-----------|------|
| 턴·GO·스킨 연동 | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |
| Domain (변경 없음) | `Scripts/Domain/Score/ScoreSession.cs` |
| 설계 동기화 | `Docs/DESIGN.md` §4.7 |
