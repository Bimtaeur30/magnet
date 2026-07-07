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

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Data/BoardConfigSO.cs`
  - 심볼: `BoardConfigSO` (추가)
  - 이유: 보드 크기/셀 크기/색을 코드 상수로 고정하지 않고 에디터에서 조정 가능하게 만들기 위해.
- 파일: `Scripts/Domain/BoardCoordinates.cs`
  - 심볼: `BoardCoordinates.GridToWorld`, `WorldToGrid`, `IsInBounds` (추가)
  - 이유: “격자 ↔ 월드 변환/경계 판단”을 한 곳으로 모아, 이후 흡착/폭발/회전 로직이 동일 좌표계에 의존하도록 하기 위해.
- 파일: `Scripts/Domain/BoardGrid.cs`
  - 심볼: `BoardGrid.IsOccupied`, `SetOccupied` (추가)
  - 이유: 이후 부착/충돌 판정에서 점유 상태를 단일 소스로 관리하기 위해.

## 2 — 2026-07-07 · BoardView LineRenderer 전환

**바뀐 것**

- 수정: `Scripts/Presentation/BoardView.cs` — 칸마다 GO+SpriteRenderer(81개) 제거 → `LineRenderer` 격자선 + 자석 축 윤곽 (`Grid`·`MagnetAxis` 자식 2개)

**메모**

- 보드 배경은 고정 라인만. 블록 피스는 Phase 2+에서 별도 프리팹/풀로 스폰·부착 예정.

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Presentation/BoardView.cs`
  - 심볼: `BoardView.BuildBoardLines` 및 `LineRenderer` 기반 렌더 (수정)
  - 이유: 칸마다 GO를 생성하는 방식(예: 9×9 → 81개)이 불필요하게 무겁고, 보드는 “고정 격자”만 표시하면 충분하기 때문.

## 3 — 2026-07-07 · LineRenderer GO 분리 (DisallowMultipleComponent)

**바뀐 것**

- 수정: `Scripts/Presentation/BoardView.cs` — `Grid`/`MagnetAxis` 컨테이너에 `LineRenderer`를 중복 `AddComponent`하던 방식 제거. 선·루프마다 자식 GO(`Line`/`Loop`)를 만들고 GO당 `LineRenderer` 1개씩 부착.

**메모**

- `LineRenderer`는 GO당 1개만 허용 → 두 번째 `AddComponent`가 null 반환·NRE 원인이었음.
- Material static 공유는 유지. `Start()` 1회 빌드이므로 추가 캐싱 없음.

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Presentation/BoardView.cs`
  - 심볼: `AddLineSegment`, `AddLineLoop` 구현 방식 (수정)
  - 이유: `LineRenderer`는 동일 GO에 중복 추가가 불가하므로, 선/루프마다 자식 GO를 만들어 1개씩 부착하도록 구조를 바꾸기 위해.
