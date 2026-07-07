# Sequence — Phase 2 (block-shape-editor)

> **Phase:** [phase2.md](phase2.md) 와 1:1.

## 1 — 2026-07-07 · 그리드 클릭 토글 + 정규화 오프셋 미리보기

**바뀐 것**

- 수정: `Scripts/Editor/BlockShapeEditorWindow.cs`
  - `activeCells`(`HashSet<Vector2Int>`) 추가, 셀 클릭 토글
  - `GetNormalizedOffsets()` — 최소 코너 기준 정규화
  - `DrawPreview()` — 활성 셀 수·오프셋 목록 표시

**메모**

- Width/Height를 줄이면 그리드 밖으로 벗어난 활성 셀은 자동 삭제됨.

---

## 2 — 2026-07-07 · 그리드 자동 크기 조절 (Grow)

**바뀐 것**

- 수정: `Scripts/Editor/BlockShapeEditorWindow.cs`
  - 고정 `CellSize` 상수 제거 → `MinCellSize`로 대체
  - `DrawGrid()`를 `GUILayout.Button` 자동 레이아웃 대신 `GUILayoutUtility.GetRect(ExpandWidth, ExpandHeight)` + 수동 `Rect` 배치로 변경
  - 셀 크기를 `min(가용 너비/width, 가용 높이/height)`로 계산해 창 크기·비율에 맞춰 자동 조절

**메모**

- 셀이 너무 작아지지 않도록 `MinCellSize = 8f` 하한을 둠.
