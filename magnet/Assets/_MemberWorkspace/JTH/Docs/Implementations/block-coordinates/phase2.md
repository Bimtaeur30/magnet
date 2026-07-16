# Phase 2 — Board Transform을 보드 공간 원점으로

> **구현:** `block-coordinates` · **Jira:** [SCRUM-17](https://bimtaeur30.atlassian.net/browse/SCRUM-17) · **마일스톤:** M1  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence2.md](sequence2.md)

## 목표 (완료 기준)

- `Board` Transform을 옮기면 격자·부착 칸·스테이징/프리뷰가 같이 움직인다
- 포인터 드래그 pivot이 보드 위치 기준으로 맞는다
- `BoardCoordinates`는 보드 로컬만 담당 (origin 주입 없음)

## 구현 내용

| 레이어 | 클래스 | 방식 |
|--------|--------|------|
| Domain | `BoardCoordinates` | 주석만 — 반환값은 board-local / `localPosition`용 |
| Presentation | `BoardView` | 이 Transform = 보드 공간의 월드 원점 |
| Bootstrap | `MagnetSceneInstaller` | `BoardView` RegisterValue |
| Input | `BlockDragInput` | 포인터 World → `board.InverseTransformPoint` → board-local X |
| Scene | `Phase0_Bootstrap` | `BlockDragInput`를 `Board` 자식으로 |

### 좌표 규칙

- 격자·보드 로컬: 자석 = `(0,0)`, `gx * cellSize`
- 월드 배치: `Board` Transform
- 입력 경계에서만 World ↔ board-local 변환

## 범위 밖

- `GridToWorld` 메서드 이름 리네임
- 기즈모(`CellRelocationTargetGizmo`) board transform 연동
