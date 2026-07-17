# Phase 1 — Domain 점수 계산 + ScoreConfigSO

> **구현:** `score-logic` · **Jira:** [SCRUM-23](https://bimtaeur30.atlassian.net/browse/SCRUM-23) · **마일스톤:** M7 (로직)  
> **상태:** 진행 중  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] `ScoreConfigSO`: k 티어·`streakMultipliers` 인스펙터 튜닝 가능, 기본값은 합의 상수
- [x] `ScoreCalculator`: 웨이브별 `round(k×combo×squareSize×streakMult)` (`ScoreConfigSO` 직접 사용)
- [x] `ScoreSession.ApplyPlacement`: 클리어 없음 → `+cellsPlaced`; 클리어 있음 → 웨이브마다 콤보+1 후 점수 합산 (배치 칸 점수·올클 없음)
- [x] `ScoreSession.NotifyTurnEnded`: 턴 중 클리어 0이면 콤보 0
- [x] `ScoreSession.Reset`: 세션 점수·콤보·턴 플래그 초기화
- [x] Bootstrap/이벤트 **미연결**
- [ ] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

### 점수 식 (확정)

- `lines` 개념 → 웨이브별 `squareSize` (3→3, 5→5…)를 **웨이브마다 따로** 적용 (합산 후 한 번 곱하지 않음)
- 웨이브 `i`(1-based), 배치 직전 콤보 `C`:
  - `combo = C + i`
  - `score += round(k(combo) × combo × S_i × streakMult(i))`
- 클리어 없음: `+cellsPlaced`
- 올클리어 보너스: 없음

### k / streak 기본값

| combo | k |
|------:|--:|
| 1–5 | 53.33 |
| 6–12 | 44 |
| 13–59 | 30 |
| 60+ | 24 |

| 웨이브 | streakMult |
|-------:|----------:|
| 1 | 1.00 |
| 2 | 1.35 |
| 3 | 1.60 |
| 4+ | 1.80 (마지막 값 유지) |

### 클래스

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `ScoreCalculator` | `Domain/Score` | 웨이브 1건 점수 계산 (`ScoreConfigSO`) |
| `PlacementScoreResult` | `Domain/Score` | 배치 점수 결과 DTO |
| `ScoreSession` | `Domain/Score` | 누적·콤보·턴 클리어 플래그 |
| `ScoreConfigSO` | `Data` | 티어·배율 SO |

## 이 Phase 범위 밖

- `BoardPlacementBootstrap` / `BlockSpawnBootstrap` 연동
- `ScoreChangedEvent` / `GameOverEvent` / `SubmitScore`
- `DESIGN.md` §4.7 문구 동기화 (Phase 3)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 튜닝 SO | `Scripts/Data/ScoreConfigSO.cs` |
| 세션·계산 | `Scripts/Domain/Score/` |
| 기본 에셋 | `ScriptableObjects/DefaultScoreConfig.asset` |
