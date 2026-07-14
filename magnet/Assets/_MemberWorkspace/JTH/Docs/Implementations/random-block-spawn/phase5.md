# Phase 5 — 4슬롯·턴(핸드 소진) 도입

> **구현:** `random-block-spawn` · **Jira:** [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) · **마일스톤:** M2  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence5.md](sequence5.md) (1:1)

## 목표 (완료 기준)

- [x] `BlockSupply.SlotCount = 4`
- [x] 슬롯 소모 시 해당 칸만 `null`, **4개 전부 소진** 시에만 `Fill`
- [x] `TurnState` — 시작 `TurnIndex=1`, 핸드 소진 시 `AdvanceAfterHandExhausted`
- [x] `TurnStartedEvent` / `TurnEndedEvent` Raise (구독자 없음 · 플레이 수치 미연결)
- [x] 키보드 `4` / numpad4 → 슬롯 인덱스 3
- [x] `Docs/DESIGN.md` 3→4후보·턴 정의 동기화
- [ ] KTJ 하단 UI 4슬롯 — **범위 밖** (별도)

## 턴 vs 배치 회전

| 개념 | 트리거 | 플레이 효과 (현재) |
|------|--------|-------------------|
| 배치 후처리 | 블록 1개 부착 성공 | 클리어 → **90° 회전** (기존) |
| 턴 | 4슬롯 전부 소진 | `TurnEnded` → `Fill` → `TurnIndex++` → `TurnStarted`만 |

## 구현 내용

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `BlockSupply` | `Domain.Spawn` | `SlotCount=4` |
| `TurnState` | `Domain.Turn` | `TurnIndex` |
| `BlockSpawnBootstrap` | `Bootstrap` | Start/Consume에서 턴·후보 이벤트 |
| `TurnStartedEvent` / `TurnEndedEvent` | `MagnetGameEvents` | 채널 페이로드 |
| `MagnetInputSO` + `Controls` | `Data` / `Input` | 슬롯 4 키 |

## 이 Phase 범위 밖

- KTJ `BlockSlotContainer` 4슬롯 UI
- 턴 기반 점수·콤보·게임오버 FSM
- 회전 타이밍 변경

## 코드·에셋

| 보려는 것 | 경로 |
|-----------|------|
| 슬롯 수 | `Scripts/Domain/Spawn/BlockSupply.cs` |
| 턴 상태 | `Scripts/Domain/Turn/TurnState.cs` |
| 리필·이벤트 | `Scripts/Bootstrap/BlockSpawnBootstrap.cs` |
| 이벤트 타입 | `Assets/_Shared/Magnet.Core/Events/MagnetGameEvents.cs` |
