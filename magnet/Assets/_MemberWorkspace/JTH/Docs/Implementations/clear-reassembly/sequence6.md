# sequence6 — Phase 6 변경 기록

> Phase 계획: [phase6.md](phase6.md)

## 1 — 2026-07-16 · 재배치 후보 기즈모

**바뀐 것** — `(0,1)` 점유 후 `(0,2)`처럼 상황별 재배치 가능 칸을 합쳐 Scene 기즈모로 그린다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `CellRelocationTargetFinder.CollectMatching` — 메서드 (추가)
    - 설명: 주어진 서클/부채꼴/안쪽 필터를 통과하는 빈칸을 `HashSet`에 모두 넣는다.
    - 이유: TryFind의 “최종 1칸”과 달리 디버그용 전체 후보가 필요해서.
  - 심볼: `CellRelocationTargetFinder.CollectAllStageCandidates` — 메서드 (추가)
    - 설명: TryFind와 동일한 단계(설정각∩서클 → 설정각 → 90° → 안쪽최근접) 후보를 합친다.
    - 이유: 단계별로 갈 수 있는 칸을 빠짐없이 기즈모에 넣기 위해.
  - 심볼: `CellRelocationTargetFinder.CollectSequentialTargets` — 메서드 (추가)
    - 설명: TryFind → 목표 점유 → 반복으로 연쇄 목표 목록을 만든다.
    - 이유: `(0,1)`이 이미 있으면 `(0,2)`가 되는 상황을 포함해 그리기 위해.
- 파일: `Scripts/Presentation/CellRelocationTargetGizmo.cs`
  - 심볼: `CellRelocationTargetGizmo.boardConfig` / `placementConfig` — 필드 (추가)
    - 설명: 격자 크기·반각 SO를 SerializeField로 받는다.
    - 이유: SO는 Inject 금지 규칙.
  - 심볼: `CellRelocationTargetGizmo.ejector` / `seedOccupied` / `includeAllEjectors` — 필드 (추가)
    - 설명: 출발 칸(기본 `(0,4)`), 시드 점유(기본 `(0,1)`), 전 이젝터 스캔 토글.
    - 이유: 요청한 `(0,1)` 가정과 바깥 이젝터 연쇄를 Inspector에서 조절.
  - 심볼: `CellRelocationTargetGizmo.OnDrawGizmos` — 메서드 (추가)
    - 설명: 빈 보드·시드 점유 각각의 단계 후보와 연쇄 목표를 합쳐 칸/부채꼴/서클을 그린다.
    - 이유: Scene에서 재배치 가능 칸을 한눈에 확인.
  - 심볼: `CellRelocationTargetGizmo.CollectStageUnion` / `CollectSequential` / `CreateGrid` / `DrawCell` / `DrawSectorAndCircle` / `Rotate` — 메서드 (추가)
    - 설명: 그리드 구성·후보 수집·기즈모 도형 그리기를 나눈다.
    - 이유: OnDrawGizmos 본문을 짧게 유지.
