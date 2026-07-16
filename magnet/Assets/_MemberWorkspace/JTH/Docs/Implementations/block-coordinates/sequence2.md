# Sequence — Phase 2 (block-coordinates)

> **Phase:** [phase2.md](phase2.md) 와 1:1.

## 1 — 2026-07-16 · Board Transform 기준 보드 공간

**바뀐 것**

- 수정: `Scripts/Domain/BoardCoordinates.cs` — board-local 의미 주석
- 수정: `Scripts/Presentation/BoardView.cs` — Transform이 보드 원점임을 문서화
- 수정: `Scripts/Bootstrap/MagnetSceneInstaller.cs` — `BoardView` 등록
- 수정: `Scripts/Input/BlockDragInput.cs` — 포인터 → board-local 변환
- 수정: `Scenes/Phase0_Bootstrap.unity` — `BlockDragInput`를 `Board` 자식, Installer에 `boardView` 배선

**메모**

- `PlacedBlocks`는 이미 `Board` 자식이라 부착 칸은 계층만으로 따라감. 스테이징/입력만 맞춰 줌.
- `BoardCoordinates`에 origin을 넣지 않음 — callers가 `localPosition`으로 쓰고 있어 이중 오프셋이 됨.

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BoardCoordinates.cs`
  - 심볼: `BoardCoordinates` 클래스 요약 주석 (수정)
    - 설명: 격자 ↔ 보드 로컬이며 월드 배치는 Board Transform 담당이라고 명시.
    - 이유: `GridToWorld` 이름을 World로 오해해 origin을 넣으면 안 된다는 계약을 문서화.
  - 심볼: `BoardCoordinates.GridToWorld` 요약 주석 (수정)
    - 설명: 반환값이 board-local / localPosition용임을 명시.
    - 이유: Presentation이 이미 localPosition에 넣고 있어 의미가 로컬임을 고정.
  - 심볼: `BoardCoordinates.WorldToGrid` 요약 주석 (수정)
    - 설명: 인자 `world`가 board-local임을 명시.
    - 이유: 실제 월드 좌표를 넣으면 보드를 옮긴 뒤 격자 변환이 틀어짐.

- 파일: `Scripts/Presentation/BoardView.cs`
  - 심볼: `BoardView` 클래스 요약 주석 (수정)
    - 설명: 이 Transform이 보드 로컬 공간의 월드 원점이라고 명시.
    - 이유: 격자선·자식 뷰가 이 Transform을 따라가도록 설계 의도를 고정.

- 파일: `Scripts/Bootstrap/MagnetSceneInstaller.cs`
  - 심볼: `MagnetSceneInstaller.boardView` — 필드 (추가)
    - 설명: 씬의 `BoardView` 참조를 Installer에 보관한다.
    - 이유: 씬 MonoBehaviour는 SerializeField로 Installer에만 배선하고 소비자는 Inject.
  - 심볼: `MagnetSceneInstaller.InstallBindings` (수정)
    - 설명: `boardView` Assert 후 `RegisterValue(boardView)` 추가.
    - 이유: `BlockDragInput`이 Board Transform으로 포인터를 변환하려면 DI로 `BoardView`가 필요.
    - 영향: `BlockDragInput`이 `[Inject] BoardView` 소비.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `BlockDragInput._boardView` — 필드 (추가)
    - 설명: 주입된 `BoardView`로 보드 Transform에 접근한다.
    - 이유: 월드 포인터를 보드 로컬로 바꿀 기준 Transform이 필요.
  - 심볼: `BlockDragInput.Awake` (수정)
    - 설명: `_boardView` null Assert 추가.
    - 이유: Inject 누락을 즉시 발견.
  - 심볼: `BlockDragInput.BeginDrag` (수정)
    - 설명: 포인터 X를 `GetPointerBoardLocalX()`로 받아 clamp·감도 램프에 사용.
    - 이유: 보드를 옮긴 뒤에도 드래그 시작 위치가 격자 기준으로 맞아야 함.
  - 심볼: `BlockDragInput.OnPointerMoved` (수정)
    - 설명: 이동 델타도 board-local X 기준으로 감도 램프·클램프.
    - 이유: 월드 X를 쓰면 보드 origin만큼 pivot이 어긋남.
  - 심볼: `BlockDragInput.GetPointerBoardLocalX` — 메서드 (추가)
    - 설명: `magnetInput` 월드 포인터를 `_boardView.transform.InverseTransformPoint`로 보드 로컬 X로 변환.
    - 이유: World↔board-local 변환을 입력 경계 한곳에 모아 Domain/좌표 유틸은 로컬만 유지.
    - 영향: `BeginDrag` / `OnPointerMoved`.

- 파일: `Scenes/Phase0_Bootstrap.unity`
  - 심볼: `BlockDragInput` Transform 부모 (수정)
    - 설명: 루트 → `Board` 자식. Installer `boardView`에 Board 컴포넌트 연결.
    - 이유: 스테이징·프리뷰 ShapeBlock이 Board와 함께 움직이도록 계층으로 묶음.
---
