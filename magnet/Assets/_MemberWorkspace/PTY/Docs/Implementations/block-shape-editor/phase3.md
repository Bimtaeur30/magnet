# Phase 3 — 부가 기능 (SO 저장)

> **구현:** `block-shape-editor` · **Jira:** [SCRUM-25](https://bimtaeur30.atlassian.net/browse/SCRUM-25)
> **상태:** 완료
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 목표 (완료 기준)

- [x] Shape Id 입력 필드
- [x] Save 버튼 — `BlockShapeSO` 에셋 생성 (Shape Id 미입력·활성 셀 없음이면 비활성화)
- [x] Clear 버튼 — 활성 셀·Shape Id 초기화

## 구현 내용 (뭘 어떻게)

`BlockShapeEditorWindow.DrawSaveControls()` / `SaveShape()`:

- `Save` 활성 조건: `shapeId`가 비어있지 않고 `activeCells.Count > 0`
- 저장 절차:
  1. `CreateInstance<BlockShapeSO>()`로 인스턴스 생성
  2. `BlockShapeSO`의 `shapeId`·`cellOffsets`는 private 필드이므로 런타임 API를 추가하지 않고 `SerializedObject`로 직접 값 대입 (에디터 전용 책임을 에디터 쪽에 유지)
  3. `AssetDatabase.GenerateUniqueAssetPath("{SaveFolder}/{shapeId}.asset")`로 중복 이름 충돌 방지 후 `AssetDatabase.CreateAsset` + `SaveAssets`
  4. 생성된 에셋을 `Selection.activeObject`로 선택 + `EditorGUIUtility.PingObject`로 하이라이트
- 저장 폴더: `Assets/_MemberWorkspace/PTY/ScriptableObjects/BlockShapes`
- `Clear`: `activeCells.Clear()` + `shapeId = ""`

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 에디터 창 | `Scripts/Editor/BlockShapeEditorWindow.cs` |
| 저장 대상 SO | `Scripts/Data/BlockShapeSO.cs` |
| 저장 폴더 | `ScriptableObjects/BlockShapes/` |
