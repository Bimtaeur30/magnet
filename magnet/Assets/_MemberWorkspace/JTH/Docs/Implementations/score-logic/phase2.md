# Phase 2 — 배치 경로 연동 + ScoreChanged

> **구현:** `score-logic` · **Jira:** [SCRUM-23](https://bimtaeur30.atlassian.net/browse/SCRUM-23) · **마일스톤:** M7 (로직)  
> **상태:** 구현됨 · 컴파일 확인 대기  
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] 성공 배치 후 `ScoreSession.ApplyPlacement` 호출 (클리어 없음 → `+cellsPlaced`, 있음 → 웨이브 `SquareSize`)
- [x] `ScoreChangedEvent.Init(totalScore)` Raise
- [x] `SquareClearedEvent.scoreAwarded` = 웨이브 실제 점수 (`WaveScores[w]`)
- [x] Domain 점수 공식 변경 없음
- [ ] `read_console` 컴파일 에러 0 (Unity MCP 미연결 시 재확인)

## 구현 내용 (뭘 어떻게)

### 호출 순서

`TryPlace` 성공 → `BlockPlaced` → Place FX → `ResolveAllWaves`  
→ **`ApplyPlacementScore` → `RaiseReassemblyEvents`(웨이브 점수) → `ScoreChanged`**  
→ Reassembly FX → (경계면 GameOver·점수 0 유지) → Rotate → Consume

### 클래스

| 심볼 | 위치 | 책임 |
|------|------|------|
| `BoardPlacementBootstrap.scoreConfig` | SerializeField SO | `DefaultScoreConfig` 주입 |
| `BoardPlacementBootstrap._scoreSession` | 런타임 | 세션 누적·콤보 |
| `ApplyPlacementScore` | Bootstrap | 재조립 웨이브 → `ApplyPlacement` |
| `RaiseReassemblyEvents(..., scoreResult)` | Bootstrap | `scoreAwarded`에 `WaveScores` |

## 이 Phase 범위 밖

- `NotifyTurnEnded` / 턴 콤보 리셋 (`BlockSpawnBootstrap`)
- `GameOverEvent` 최종 점수·베스트·`SkinUnlockCheck` 실점수
- HUD UI / `DESIGN.md` §4.7 동기화

## 코드·에셋 맵

| 보려는 것 | 경로 |
|-----------|------|
| 배치·점수 Raise | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |
| Domain (변경 없음) | `Scripts/Domain/Score/` |
| 튜닝 에셋 | `ScriptableObjects/DefaultScoreConfig.asset` |
| 씬 배선 | `Scenes/Phase0_Bootstrap.unity` — `scoreConfig` |
