# Inspector 직렬화 필드 Tooltip

팀 공용 — `[SerializeField]` 이름만으로 역할이 불명확한 필드에 `[Tooltip]`을 붙이고, 문구를 이 문서에 기록한다.

> 코드 식별자·API 이름은 English, Tooltip 문구는 **한국어**.

---

## 규칙

### Tooltip을 붙이는 경우

| 상황 | 예 |
|------|-----|
| 도메인 용어·약어 | `cellsPerSide`, `stagingYExtraBelow` |
| 일반명·축약 | `config`, `cellsRoot`, `linesRoot` |
| 단위·범위가 이름에 없음 | `cellFill` (0.1~1 비율) |
| 자동 생성·선택적 할당 | `linesRoot`, `cellsRoot` (비우면 런타임 생성) |
| 역할이 코드 맥락 없이는 모호 | `stagingBlockView` |

### Tooltip을 생략하는 경우

| 상황 | 예 |
|------|-----|
| 타입+이름으로 충분 | `BoardConfigSO boardConfig`, `EventChannelSO magnetGameChannel` |
| 일반 Unity 관례·자명한 속성 | `cellSize`, `lineWidth`, `pieceColor`, `cellColor` |

### 구현

- `[Tooltip("…")]`을 `[SerializeField]` **바로 위**에 배치
- 필드 추가·이름 변경·Tooltip 수정 시 **아래 표도 갱신**
- 개인 `sequenceN.md`에는 변경 요약만, **전체 목록은 이 문서가 소스 오브 트루스**

---

## 필드 목록 (멤버별)

### JTH — `Assets/_MemberWorkspace/JTH/`

| 파일 | 필드 | Tooltip |
|------|------|---------|
| `Scripts/Data/BoardConfigSO.cs` | `cellsPerSide` | 자석 중심(0,0)에서 한쪽 끝까지의 칸 수. 전체 한 변 = CellsPerSide × 2 + 1 |
| `Scripts/Data/BlockVisualConfigSO.cs` | `stagingYExtraBelow` | 보드 하단에서 스테이징 영역까지 추가로 내릴 칸 수. stagingY = -(CellsPerSide + 이 값) |
| `Scripts/Data/BlockVisualConfigSO.cs` | `cellFill` | 블록 칸 스프라이트가 격자 칸 대비 차지하는 비율(0.1~1). 1이면 칸과 동일 크기 |
| `Scripts/Data/BlockDragConfigSO.cs` | `sensitivityRampPerUnit` | Press 시작 포인터 X와의 거리(월드 유닛) 1당 블록 이동 배율 증가량. Block Blast식 감도 램프 |
| `Scripts/Data/BlockDragConfigSO.cs` | `sensitivityMaxMultiplier` | 드래그 감도 배율 상한. 1이면 램프 없음 |
| `Scripts/Data/BlockSnapConfigSO.cs` | `duration` | 손 놓은 뒤 Y축 자석 스냅: 칸 1칸 이동에 걸리는 시간(초). 이동 칸 수에 비례 |
| `Scripts/Data/BlockSnapConfigSO.cs` | `ease` | Place 성공 후 Y 스냅 LitMotion 이징 (Ease enum) |
| `Scripts/Data/BoardRotationConfigSO.cs` | `duration` | 폭발 처리 후 보드·블록 90° 회전 LitMotion 시간(초) |
| `Scripts/Data/BoardRotationConfigSO.cs` | `preRotationDelay` | 재조립 연출이 끝난 뒤 회전 시작 전 대기(초) |
| `Scripts/Data/BoardRotationConfigSO.cs` | `ease` | 보드 90° 회전 시 칸 View 이동 LitMotion 이징 (Ease enum) |
| `Scripts/Data/ClearReassemblyRuleConfigSO.cs` | `corridorHalfWidth` | 원점–원래칸 직선 수선 반폭(격자). 복도 안만 후보. 막히면 제자리(보드밖 주차 없음) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `originalCellColor` | 노랑 와이어 — 원래 칸(originalCell) 위치 |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `seedOccupiedColor` | 주황/살몬 채움 — 시드 점유 칸(seedOccupied) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `candidateColor` | 민트 초록 와이어 — 복도 안 이동 가능 후보 칸 |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `chosenColor` | 하늘 파랑 채움 — TryFind가 고른 최종 목표 칸(안쪽만) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `stayColor` | 회색 채움 — 안쪽이 막혀 원래 칸에 제자리 |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `corridorColor` | 흰 선 — 원점~원래칸 수선 복도(CorridorHalfWidth) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `axisColor` | 노란 축 선 — 원점(0,0) → 원래 칸 방향 |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `bounceCells` | 폭발 후 바깥으로 튕기는 거리(칸) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `bounceDuration` | 튕김 LitMotion 시간(초) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `bounceEase` | 튕김(바깥으로 밀려남) LitMotion 이징 (Ease enum) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `landDuration` | 착지(목표 칸 이동) 시간(초) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `landEase` | 착지(목표 칸으로 이동) LitMotion 이징 (Ease enum) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `spinDegreesPerSecond` | 비행 중 자전 각속도(도/초) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `staggerPerRing` | 다음 링 시작 지연(초). 같은 링은 동시 이동. 이전 링 완료를 기다리지 않음 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `duration` | 폭발 테두리 펄스 LitMotion 시간(초) |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `peakScale` | 테두리 기준 크기 대비 최대 배율. 1이면 크기 변화 없음 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `sizeEase` | 펄스 크기 LitMotion 이징. t는 alpha와 동일, Ease만 다름 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `alphaEase` | 펄스 알파 LitMotion 이징. t는 크기와 동일, Ease만 다름 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `maxAlpha` | 펄스 최대 알파(0~1). 기본색 알파에 곱함 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `color` | 폭발 테두리 LineRenderer 색 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `lineWidth` | 폭발 테두리 LineRenderer 두께 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `sortingOrder` | 폭발 테두리 LineRenderer sortingOrder |
| `Scripts/Data/ScoreConfigSO.cs` | `kTiers` | 콤보 구간별 k. maxComboInclusive 오름차순. 마지막 구간이 60+ 등으로 나머지 처리 |
| `Scripts/Data/ScoreConfigSO.cs` | `KTier.maxComboInclusive` | 이 구간에 포함되는 최대 콤보(이상이면 다음 구간). 오름차순이어야 함 |
| `Scripts/Data/ScoreConfigSO.cs` | `KTier.k` | 해당 콤보 구간의 배율 k |
| `Scripts/Data/ScoreConfigSO.cs` | `streakMultipliers` | 같은 배치 안 웨이브 순번(1-based)별 연쇄 배율. 길이 부족 시 마지막 값 사용 |
| `Scripts/Presentation/BoardView.cs` | `config` | 격자 크기·색상 등 보드 시각화 설정 |
| `Scripts/Presentation/BoardView.cs` | `linesRoot` | 격자·자석 축 LineRenderer의 부모 Transform. 비우면 자동 생성 |
| `Scripts/Presentation/BoardView.cs` | `placementConfigPreview` | 폭발 테두리 ContextMenu 프리뷰용 PlacementConfig. 비우면 씬/에셋에서 자동 탐색 |
| `Scripts/Presentation/ShapeBlock.cs` | `blockPrefab` | 블록 칸 1개 프리팹(Block 컴포넌트 + SpriteRenderer). 필요 개수만큼 인스턴스 생성 후 재사용 |
| `Scripts/Presentation/Block.cs` | `spriteMask` | 칸 스킨 클리핑용. SetSortingOrder에서 Custom Range로 인접 마스크와 격리 |
| `Scripts/Input/BlockDragDrawer.cs` | `shapeBlockPrefab` | 스테이징·프리뷰 표시용 ShapeBlock 프리팹. Awake에서 2개 Instantiate |

### KTJ — `Assets/_MemberWorkspace/KTJ/`

_(아직 등록된 Tooltip 없음)_

### PTY — `Assets/_MemberWorkspace/PTY/`

_(아직 등록된 Tooltip 없음)_

### PMS — `Assets/_MemberWorkspace/PMS/`

_(아직 등록된 Tooltip 없음)_

---

## 변경 이력

| 날짜 | 내용 |
|------|------|
| 2026-07-09 | JTH 7개 필드 Tooltip 추가 및 팀 문서 최초 작성 |
| 2026-07-09 | JTH Phase 3 — 감도 램프·프리뷰 뷰 Tooltip 3개 추가 |
| 2026-07-16 | JTH Block.spriteMask Tooltip 추가 (SpriteMask Custom Range 격리) |
