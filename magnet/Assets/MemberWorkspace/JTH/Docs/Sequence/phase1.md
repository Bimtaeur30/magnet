# Phase 1 — 보드·자석 축

> **상태:** 구현됨 · 사용자 확인 대기  
> **다음:** Phase 2 — `BlockShapeSO` + 3블록 후보

## 목표 (DESIGN.md)

N×N 격자 렌더, 중앙 자석 축 표시, `BoardConfigSO`

## 이 Phase에서 한 일

- `BoardConfigSO` — N(홀수), cellSize, 일반 칸/자석 축 색
- `BoardCoordinates` — `GridToWorld` / `WorldToGrid`, `IsInBounds`, 자석=(0,0)
- `BoardGrid` — `Dictionary<Vector2Int, bool>` 점유 (Phase 3+ 연동)
- `BoardView` — SO `SerializeField`, `-half..half` 루프로 격자 렌더
- `DefaultBoardConfig.asset` (N=9)
- `Phase0_Bootstrap` 씬 — `Board` 오브젝트, 카메라 Orthographic

## 이 Phase에서 안 한 것

- 블록·흡착·폭발·회전 → Phase 2~6
- `BoardGrid`를 뷰/게임플레이에 연결 → Phase 3 이후

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 보드 설정 SO | `Scripts/Data/BoardConfigSO.cs` |
| 좌표 변환 | `Scripts/Domain/BoardCoordinates.cs` |
| 점유 Dictionary | `Scripts/Domain/BoardGrid.cs` |
| 격자 렌더 | `Scripts/Presentation/BoardView.cs` |
| 기본 설정 에셋 | `ScriptableObjects/DefaultBoardConfig.asset` |
| 씬 | `Scenes/Phase0_Bootstrap.unity` → `Board` |

### 좌표 요약

- 격자: 자석 = `(0, 0)`, N=9 유효 범위 `[-4 .. 4]`
- 월드: `(gx * cellSize, gy * cellSize)`
- 보드 밖: `|gx| > N/2` 또는 `|gy| > N/2` → 경계 이탈 (Phase 4)

## 메모

- SO는 Reflex 없이 `BoardView` Inspector 연결.
- 플레이 시 9×9 격자 + 가운데 금색(자석) 칸 확인.
