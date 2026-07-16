# Phase 5 — 재배치: 안쪽 진입 + 부채꼴 폴백 축소

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 구현됨 · **선행:** Phase 4  
> **변경 기록:** [sequence5.md](sequence5.md) (1:1)  
> **grill-me:** 2026-07-16 확정

## 목표 (완료 기준)

- [x] 폭발로 비워진 칸(최내곽 N 내부 포함)으로 바깥 이젝터가 들어올 수 있다
- [x] 안쪽 잔존 칸(ring ≤ half에 남아 있는 점유)은 고정. 이젝터는 ring > half만
- [x] `PlacementConfigSO.HalfSectorDegrees`로 부채꼴 반각 설정 (±설정 → 실패 시 최대 ±90°)
- [x] 180°·반대편·보드밖 주차 폴백 제거. 부채꼴 실패 시 안쪽·자석 최근접 빈칸만
- [x] `read_console` 컴파일 에러 0

## 구현 내용

| 클래스 | 변경 |
|--------|------|
| `PlacementConfigSO` | `HalfSectorDegrees` (1~90, 기본 45) |
| `CellRelocationTargetFinder` | 보존 제외·OutsidePark·180°/비안쪽 폴백 제거. 설정각→90°→안쪽 최근접 |
| `ClearReassemblyService` | `ResolveAllWaves(session, halfSectorDegrees)`. 안쪽 목표 OutsidePark 리다이렉트 삭제 |
| `BoardPlacementBootstrap` | SO 값을 Service에 전달 |

## 이 Phase 범위 밖

- 최초 배치 낙하 모션
- 핸드 스폰 시 90/180/270 랜덤 회전

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 목표 탐색 | `Scripts/Domain/Clear/CellRelocationTargetFinder.cs` |
| 웨이브 | `Scripts/Domain/Clear/ClearReassemblyService.cs` |
| 각도 SO | `Scripts/Data/PlacementConfigSO.cs` |
