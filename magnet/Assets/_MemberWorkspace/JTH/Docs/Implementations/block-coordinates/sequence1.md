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

## 2 — 2026-07-07 · BoardView LineRenderer 전환

**바뀐 것**

- 수정: `Scripts/Presentation/BoardView.cs` — 칸마다 GO+SpriteRenderer(81개) 제거 → `LineRenderer` 격자선 + 자석 축 윤곽 (`Grid`·`MagnetAxis` 자식 2개)

**메모**

- 보드 배경은 고정 라인만. 블록 피스는 Phase 2+에서 별도 프리팹/풀로 스폰·부착 예정.

## 3 — 2026-07-07 · LineRenderer GO 분리 (DisallowMultipleComponent)

**바뀐 것**

- 수정: `Scripts/Presentation/BoardView.cs` — `Grid`/`MagnetAxis` 컨테이너에 `LineRenderer`를 중복 `AddComponent`하던 방식 제거. 선·루프마다 자식 GO(`Line`/`Loop`)를 만들고 GO당 `LineRenderer` 1개씩 부착.

**메모**

- `LineRenderer`는 GO당 1개만 허용 → 두 번째 `AddComponent`가 null 반환·NRE 원인이었음.
- Material static 공유는 유지. `Start()` 1회 빌드이므로 추가 캐싱 없음.
