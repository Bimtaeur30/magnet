# Sequence — Phase 1 (block-coordinates)

> **Phase:** [phase1.md](phase1.md) 와 1:1. 이 Phase에서 **뭐가 바뀌었는지** 순서대로 적는다.  
> 새 작업마다 `## N — 제목` 섹션을 아래에 추가 (파일 분리 X).

## 1 — 2026-07-06 · 최초 구현

**바뀐 것**

- 생성: `Scripts/Data/BoardConfigSO.cs` — N(홀수)·cellSize·칸 색·자석 축 색
- 생성: `Scripts/Domain/BoardCoordinates.cs` — `GridToWorld` / `WorldToGrid` / `IsInBounds`
- 생성: `Scripts/Domain/BoardGrid.cs` — `Dictionary<Vector2Int, bool>` 점유
- 생성: `Scripts/Presentation/BoardView.cs` — 격자 렌더
- 생성: `ScriptableObjects/DefaultBoardConfig.asset` (N=9)
- 수정: `Scenes/Phase0_Bootstrap.unity` — `Board` 오브젝트 추가, 카메라 Orthographic

**메모**

- SO는 Reflex 없이 `BoardView` Inspector 연결.
- 플레이 시 9×9 격자 + 가운데 금색(자석) 칸 확인.
