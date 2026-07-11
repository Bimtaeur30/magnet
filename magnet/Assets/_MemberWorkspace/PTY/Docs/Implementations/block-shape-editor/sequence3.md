# Sequence — Phase 3 (block-shape-editor)

> **Phase:** [phase3.md](phase3.md) 와 1:1.

## 1 — 2026-07-07 · Shape Id 입력 + SO 저장 + 초기화

**바뀐 것**

- 수정: `Scripts/Editor/BlockShapeEditorWindow.cs`
  - `shapeId` 필드 + `DrawSaveControls()` 추가 (Save/Clear 버튼)
  - `SaveShape()` — `SerializedObject`로 `BlockShapeSO` 값 대입 후 `AssetDatabase.CreateAsset`로 `ScriptableObjects/BlockShapes/`에 저장

**메모**

- `BlockShapeSO`에 public setter를 추가하지 않고 에디터 스크립트에서 `SerializedObject`로만 값을 채움 — 런타임 API 표면을 늘리지 않기 위함.
- 저장 시 같은 Shape Id가 있으면 `GenerateUniqueAssetPath`로 자동으로 다른 파일명이 붙음 (덮어쓰기 없음).

---

## 2 — 2026-07-07 · 기존 BlockShapeSO 불러오기 + Active Cells 카운트 제거

**바뀐 것**

- 수정: `Scripts/Editor/BlockShapeEditorWindow.cs`
  - `shapeToLoad` 필드 + `DrawLoadControls()`/`LoadShape()` 추가 — `ObjectField`로 `BlockShapeSO` 선택 후 `Load` 버튼으로 `activeCells`·`shapeId`·`width`/`height`를 불러온 에셋 기준으로 복원
  - `DrawPreview()`에서 "Active Cells" 개수 라벨 제거 (오프셋 목록만 표시)

**메모**

- `CellOffsets`는 이미 최소 코너 기준 `(0,0)` 정규화된 값이므로 그대로 `activeCells`에 넣으면 됨.
- 로드 시 `width`/`height`는 불러온 형태의 최대 x/y+1로 자동 확장 (1~20 clamp).
