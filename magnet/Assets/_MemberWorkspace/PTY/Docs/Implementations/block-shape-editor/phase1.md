# Phase 1 — 구조

> **구현:** `block-shape-editor` · **Jira:** [SCRUM-25](https://bimtaeur30.atlassian.net/browse/SCRUM-25)
> **상태:** 완료
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] PTY 워크스페이스 최소 골격 (`Scripts/`, asmdef) 생성
- [x] `IBlockShape`를 구현하는 `BlockShapeSO` 정의
- [x] 그리드 크기(가로/세로) 입력 + 빈 그리드를 그리는 에디터 창 뼈대 (`BlockShapeEditorWindow`)

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `BlockShapeSO` | `Scripts/Data/` | `IBlockShape` 구현. `shapeId`, `cellOffsets`(`List<Vector2Int>`)만 직렬화. `[CreateAssetMenu]`로 수동 생성도 가능 |
| `BlockShapeEditorWindow` | `Scripts/Editor/` | `EditorWindow`. Phase 1은 그리드 크기 입력(1~20 clamp) + 빈 그리드 렌더링까지만 |

### asmdef

- `Magnet.PTY` — 런타임. `Magnet.Contracts`(`GUID:eb415e56e20154449aab8a7efaff0147`)만 참조
- `Magnet.PTY.Editor` — `includePlatforms: Editor`. `Magnet.PTY` + `Magnet.Contracts` 참조

### 좌표 규칙 (Phase 2에서 적용 예정)

- JTH 컨벤션과 동일: 활성 셀 중 최소 코너를 피벗 `(0,0)`으로 정규화

## 이 Phase 범위 밖

- 그리드 셀 클릭 토글·활성 상태 관리 (Phase 2)
- ShapeId 입력, SO 에셋 저장, 초기화 버튼 (Phase 3)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 공용 계약 | `Assets/Shared/Magnet.Contracts/BlockShapes/IBlockShape.cs` |
| SO 정의 | `Scripts/Data/BlockShapeSO.cs` |
| 에디터 창 | `Scripts/Editor/BlockShapeEditorWindow.cs` |
| 저장 대상 폴더 | `ScriptableObjects/BlockShapes/` |
