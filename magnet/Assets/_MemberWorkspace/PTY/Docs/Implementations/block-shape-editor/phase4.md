# Phase 4 — 블록 아이콘 자동 생성

> **구현:** `block-shape-editor` · **Jira:** [SCRUM-25](https://bimtaeur30.atlassian.net/browse/SCRUM-25)
> **상태:** 완료
> **변경 기록:** [sequence4.md](sequence4.md) (1:1)

## 목표 (완료 기준)

- [x] `BlockShapeSO`에 아이콘 텍스처 필드 추가
- [x] `BlockShapeEditorWindow`에서 버튼 한 번으로 프로젝트 내 모든 `BlockShapeSO`의 아이콘을 카메라 촬영으로 일괄 생성·할당
- [x] 도형 전체가 정사각 아이콘 안에 잘리지 않고 보이도록 카메라 자동 프레이밍
- [x] `ShapeBlock.prefab`의 스킨(색상/스프라이트)이 바뀌면 아이콘을 자동으로 다시 생성

## 구현 내용 (뭘 어떻게)

### `BlockShapeSO`

- `[SerializeField] private Texture2D icon;` + `public Texture2D Icon => icon;` 추가 (다른 필드와 동일하게 private + 읽기 전용 프로퍼티, 에디터는 `SerializedObject`로 대입)

### `BlockShapeIconGenerator` (신규, 에디터 전용 정적 클래스)

- `EditorSceneManager.NewPreviewScene()`로 메인 씬과 격리된 임시 씬 생성
- 카메라 1개(직교, 배경 투명 `SolidColor` + alpha 0) + JTH `ShapeBlock.prefab` 인스턴스 1개를 **순회 내내 재사용**
  - `UniversalAdditionalCameraData`는 별도로 붙이지 않음 — URP가 `Camera.Render()` 시 내부적으로 `GetUniversalAdditionalCameraData()`로 없으면 자동 생성하므로 불필요한 패키지 참조를 피함
  - `ShapeBlock`은 `[SerializeField]`로 `boardConfig`/`placementConfig`/`blockPrefab`/`systemChannel`/`skinColors`/`skinSprites`가 이미 프리팹에 값이 채워져 있어 DI 없이 그대로 인스턴스화 가능
  - `AssetDatabase.FindAssets("t:BlockShapeSO")`로 프로젝트 전체의 `BlockShapeSO`를 찾아 순회하며 `ShapeBlock.ShowCells(Vector2Int.zero, shapeSo.CellOffsets, 0)`만 반복 호출 (오브젝트 재생성 없음)
  - 스킨(색·스프라이트)은 `ShapeBlock` 내부 `ApplyResolvedSkin()`이 최초 1회만 resolve하고 이후 `_skinResolved` 플래그로 고정하는 기존 동작을 그대로 활용 — 모든 아이콘이 동일한 색/스프라이트로 통일됨 (요청: "현재 설정한 색과 스킨으로")
- 매 도형마다 활성 `SpriteRenderer`들의 world bounds를 계산해 카메라 위치·직교 크기를 자동 프레이밍(여백 20%)
- `RenderTexture`(256×256, ARGB32, 투명) → `Camera.Render()` → `ReadPixels` → `Texture2D` → `EncodeToPNG()` → `File.WriteAllBytes`
- 저장 경로: `Assets/_MemberWorkspace/PTY/Sprites/BlockIcons/{ShapeId}_Icon.png`
- `AssetDatabase.ImportAsset` 후 `TextureImporter.alphaIsTransparency = true` 설정 → 재로드한 `Texture2D`를 `SerializedObject`로 `BlockShapeSO.icon`에 대입
- `finally` 블록에서 임시 카메라·ShapeBlock 인스턴스 파괴 + `EditorSceneManager.ClosePreviewScene`으로 정리

### `BlockShapeEditorWindow`

- `DrawIconGenerationControls()` 추가 — "아이콘 일괄 생성" 버튼 하나만 배치, 클릭 시 `BlockShapeIconGenerator.GenerateAllIcons()` 호출

### `BlockShapeIconAutoRegenerator` (신규, `AssetPostprocessor`)

- `ShapeBlock.prefab`(현재 유일한 스킨 정의처)이 재임포트될 때마다 `EditorApplication.delayCall`로 `BlockShapeIconGenerator.GenerateAllIcons()`를 자동 호출해 모든 아이콘을 최신 스킨으로 갱신

## 범위 밖

- 개별 SO 단위 아이콘 재생성(현재는 항상 전체 일괄 생성)
- 스킨을 아이콘마다 다르게 지정하는 기능(고정된 단일 스킨 사용)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 아이콘 필드 | `Scripts/Data/BlockShapeSO.cs` |
| 아이콘 생성 로직 | `Scripts/Editor/BlockShapeIconGenerator.cs` |
| 버튼 진입점 | `Scripts/Editor/BlockShapeEditorWindow.cs` |
| 스킨 변경 시 자동 재생성 | `Scripts/Editor/BlockShapeIconAutoRegenerator.cs` |
| 생성된 아이콘 PNG | `Sprites/BlockIcons/` |
| 촬영 대상 프리팹(참조만, 수정 없음) | `Assets/_MemberWorkspace/JTH/Prefabs/ShapeBlock.prefab` |
