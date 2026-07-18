# Phase 5 — 첫 Place Y스냅 수정 + Draw 시 회전

> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  
> **상태:** 구현됨 · **선행:** Phase 4  
> **변경 기록:** [sequence5.md](sequence5.md) (1:1)  
> **grill-me:** 2026-07-16 확정

## 목표 (완료 기준)

- [x] 게임 시작 후 첫 Place에서도 Y 스냅이 거리 비례로 자연스럽게 보인다
- [x] `SnapDuration` = 칸 1칸당 시간. `duration = SnapDuration * max(cells, 1)`
- [x] `BlockDrawer.Draw` 시 0/90/180/270 균등 회전된 `IBlockShape` 래퍼를 슬롯에 넣는다 (원본 SO 불변)
- [x] `read_console` 컴파일 에러 0

## 구현 내용

| 클래스 | 변경 |
|--------|------|
| `ShapeBlock` | Y스냅 duration을 이동 칸 수에 비례 |
| `BlockSnapMotion` | ShowAtSnapStart → Animate (transform startY) |
| `PlacementConfigSO` | SnapDuration 의미: 칸당 시간 |
| `RotatedBlockShape` | 시계 90°×N `IBlockShape` 래퍼 |
| `BlockDrawer` | Draw 시 `Next(4)` 쿼터 턴 적용 |

## 이 Phase 범위 밖

- clear-reassembly 재배치 (Phase 5 별도 완료)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| Y 스냅 | `Scripts/Presentation/ShapeBlock.cs`, `BlockSnapMotion.cs` |
| 스폰 회전 | `Scripts/Domain/Spawn/RotatedBlockShape.cs`, `BlockDrawer.cs` |

