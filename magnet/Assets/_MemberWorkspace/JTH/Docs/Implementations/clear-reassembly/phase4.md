# Phase 4 — Presentation 칸 View + 달팽이 LitMotion

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 계획됨 · **선행:** Phase 3  
> **변경 기록:** [sequence4.md](sequence4.md) (1:1)

## 목표 (완료 기준)

- [ ] 흡착 착지 완료 시 멀티칸 View를 **칸당 1 View**로 분리. 이후 보드 위는 셀 View만
- [ ] `SquareCleared` → 테두리 칸 View 제거(이펙트 훅 가능)
- [ ] `CellsRelocated` → 칸마다 LitMotion: **튕김 3칸 → 고정 반지름 시계 360° → 목표 착지**
- [ ] 비행 중 **자전**(공전 속도와 무관, SO). 착지 시 **보드 격자 90°** 스냅
- [ ] 궤도 반지름: `max(시작거리, 목표거리, 유지 안쪽 최대반지+여유)` 월드 거리 — 안쪽 유지 칸과 비접촉
- [ ] 스태거: 같은 링은 **동시** 시작, 다음 링은 이전 링 **시작 후** `StaggerPerRing`(완료 대기 X)
- [ ] 화면 밖 튕김 OK (이펙트)
- [ ] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

| 클래스/SO | 책임 |
|-----------|------|
| 칸 View 팩토리/풀 | 부착 시 셀 View 생성, `cellId` 매핑 |
| `ClearReassemblyPresenter` | 웨이브 연출 UniTask. 스태거·LitMotion |
| `ClearReassemblyMotionConfigSO` | 튕김 칸수(기본 3), 공전 시간, 자전 각속도, 스태거, 궤도 여유 |

물리 Rigidbody / OverlapCircle **사용 안 함**. Domain 결과가 진실.

## 이 Phase 범위 밖

- 폭발 VFX·SFX 폴리시 (M10)
- 점수 HUD

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 기존 배치 Presentation | `Scripts/Presentation/` (placement/clear 관련) |
| 회전 LitMotion | board-rotation Presentation |
| 이벤트 구독 | clear Presentation / Bootstrap |
