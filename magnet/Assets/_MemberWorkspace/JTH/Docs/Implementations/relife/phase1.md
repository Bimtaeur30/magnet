# relife Phase 1 — Relife 오퍼·수락 이벤트

> **구현:** `relife`  
> **grill:** 2026-08-16

## 목표 (완료 기준)

- [x] 게임오버 + `TotalScore ≥ RelifeMinScore` + 세션 미사용 → Easy 셀 오프셋 3개 미리 뽑기
- [x] `RelifeOfferedEvent` 발행, 이때 `GameOverEvent` 없음
- [x] `RelifeAcceptedEvent` → 뽑아 둔 Easy를 슬롯에 넣고 이어하기
- [x] 조건 불충족·이미 사용 → 기존 `GameOverEvent`
- [x] `ScoreConfigSO.RelifeMinScore` Inspector

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `RelifeSession` | 오퍼 가능 여부, pending 패, 세션 1회 |
| `TurnBootstrap` | 게임오버 분기에서 오퍼/수락 배선 |
| `BlockSpawnBootstrap` | Easy 미리 뽑기, 준비된 오프셋으로 Fill |
| `AreaBundleOrchestrator.SelectEasyHand` | Easy 히트맵(실패 시 가중랜덤) |
| `MagnetGameEvents` | `RelifeOfferedEvent` / `RelifeAcceptedEvent` |
| `ScoreConfigSO.RelifeMinScore` | n |

## 범위 밖

- Relife UI
- `RelifeDeclinedEvent`
- 적/스테이지/목표 점수
- 세션당 2회 이상
