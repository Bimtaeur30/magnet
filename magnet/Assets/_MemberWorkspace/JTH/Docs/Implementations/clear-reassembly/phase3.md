# Phase 3 — Bootstrap 턴 루프·입력 잠금·이벤트

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 계획됨 · **선행:** Phase 2  
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 목표 (완료 기준)

- [ ] 턴 순서: `배치 → (클리어 재조립 연쇄 Domain) → 연출 대기(Phase 4) → 회전 → 후보 갱신`
- [ ] Domain이 반환한 `ClearWave[]`를 Bootstrap이 순차 처리
- [ ] 채널: `SquareClearedEvent`(테두리 파괴) + `CellsRelocatedEvent`(웨이브 재배치 목록)
- [ ] 연쇄·연출 구간 **입력 잠금** (드래그·후보 선택 불가). 종료 후 해제
- [ ] 기존 “클리어 1회 후 즉시 회전” 경로를 재조립 연쇄 완료 후로 이동
- [ ] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

| 심볼/클래스 | 책임 |
|-------------|------|
| `BoardPlacementBootstrap` (또는 Turn Bootstrap) | Place 성공 후 `ClearReassemblyService.ResolveAllWaves` → 이벤트 → (P4) 연출 await → Rotate |
| `MagnetGameEvents.SquareClearedEvent` | 파괴 칸·N·점수칸 수 (바깥 제거 필드 제거) |
| `MagnetGameEvents.CellsRelocatedEvent` | `Relocations` 목록 (연출용) |
| 입력 게이트 | 기존 입력 컴포넌트에 `IsInputLocked` 또는 채널 `TurnResolutionStarted/Finished` |

Phase 4 전: 연출 await는 no-op(즉시 완료)여도 Domain·이벤트·잠금 골격은 동작.

## 이 Phase 범위 밖

- 달팽이 LitMotion·칸 View 분해 (Phase 4)
- 점수 HUD (SCRUM-23)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 배치 Bootstrap | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |
| 회전 턴 | `board-rotation` Bootstrap / Turn 관련 |
| 이벤트 | `Scripts/Events/MagnetGameEvents.cs` |
