# Phase 2 — 핵심 기능 (그리드 클릭 → 형태 생성)

> **구현:** `block-shape-editor` · **Jira:** [SCRUM-25](https://bimtaeur30.atlassian.net/browse/SCRUM-25)
> **상태:** 완료
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] 그리드 셀 클릭 시 활성/비활성 토글
- [x] 활성 셀을 최소 코너 기준 `(0,0)`으로 정규화한 `CellOffsets` 계산
- [x] 정규화된 오프셋 미리보기 표시
- [x] 그리드가 에디터 창 크기에 맞춰 셀 크기를 자동 조절 (Expand/Grow)

## 구현 내용 (뭘 어떻게)

`BlockShapeEditorWindow` (`Scripts/Editor/BlockShapeEditorWindow.cs`)에 추가:

- `HashSet<Vector2Int> activeCells` — 클릭한 셀의 그리드 절대 좌표 저장
- `DrawGrid()` — `GUILayoutUtility.GetRect(ExpandWidth, ExpandHeight)`로 남은 공간을 확보한 뒤, `cellSize = min(rect.width/width, rect.height/height)`로 셀 크기를 계산. `GUI.Button`을 셀별 `Rect`에 수동 배치해 클릭 시 `activeCells` 토글
- Width/Height 변경 시 범위를 벗어난 셀은 `activeCells`에서 자동 제거
- `GetNormalizedOffsets()` — 활성 셀 중 최소 x/y를 구해 `(0,0)` 기준으로 이동시킨 목록 반환 (JTH `BlockShapePresets` 피벗 컨벤션과 동일)
- `DrawPreview()` — 활성 셀 개수 + 정규화된 오프셋 목록을 라벨로 표시 (저장 전 확인용)

## 이 Phase 범위 밖

- ShapeId 입력, `BlockShapeSO` 에셋 저장, 초기화 버튼 (Phase 3)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 에디터 창 | `Scripts/Editor/BlockShapeEditorWindow.cs` |
