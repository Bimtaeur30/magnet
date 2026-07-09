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
| `Scripts/Data/PlacementConfigSO.cs` | `stagingYExtraBelow` | 보드 하단에서 스테이징 영역까지 추가로 내릴 칸 수. stagingY = -(CellsPerSide + 이 값) |
| `Scripts/Data/PlacementConfigSO.cs` | `cellFill` | 블록 칸 스프라이트가 격자 칸 대비 차지하는 비율(0.1~1). 1이면 칸과 동일 크기 |
| `Scripts/Presentation/BoardView.cs` | `config` | 격자 크기·색상 등 보드 시각화 설정 |
| `Scripts/Presentation/BoardView.cs` | `linesRoot` | 격자·자석 축 LineRenderer의 부모 Transform. 비우면 자동 생성 |
| `Scripts/Presentation/BlockPieceView.cs` | `cellsRoot` | 블록 피스 칸 SpriteRenderer의 부모 Transform. 비우면 자동 생성 |
| `Scripts/Presentation/BlockPieceView.cs` | `cellPrefab` | 블록 칸 1개 프리팹(SpriteRenderer 포함). 필요 개수만큼 인스턴스 생성 후 재사용 |
| `Scripts/Bootstrap/MagnetSceneInstaller.cs` | `stagingBlockView` | 슬롯 선택 시 스테이징 영역에 미리보기로 표시할 BlockPieceView |

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
