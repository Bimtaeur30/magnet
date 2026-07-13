# Sequence — Phase 4 (block-shape-editor)

> **Phase:** [phase4.md](phase4.md) 와 1:1.

## 1 — 2026-07-11 · 블록 아이콘 자동 생성 기능 추가

**바뀐 것**

- 수정: `Scripts/Data/BlockShapeSO.cs`
  - `icon`(`Texture2D`) 필드 + `Icon` 읽기 전용 프로퍼티 추가
- 생성: `Scripts/Editor/BlockShapeIconGenerator.cs`
  - 임시 프리뷰 씬 + 카메라 1개 + JTH `ShapeBlock.prefab` 인스턴스 1개를 재사용해 프로젝트 내 모든 `BlockShapeSO`를 순회 촬영
  - `Sprites/BlockIcons/{ShapeId}_Icon.png`로 저장 후 재임포트, `SerializedObject`로 각 SO의 `icon`에 대입
- 수정: `Scripts/Editor/BlockShapeEditorWindow.cs`
  - `DrawIconGenerationControls()` + "아이콘 일괄 생성" 버튼 추가 (`OnGUI` 마지막에 호출)

**메모**

- 카메라·ShapeBlock 인스턴스를 도형마다 새로 만들지 않고 재사용 — `ShowCells()` 재호출만으로 셀 배치가 갱신되는 기존 구조를 그대로 활용해 효율화.
- `UniversalAdditionalCameraData`를 명시적으로 `AddComponent`하려다 `UnityEngine.Rendering.Universal` 참조 문제가 발생해 제거함. URP는 `Camera.Render()` 실행 시 `CameraExtensions.GetUniversalAdditionalCameraData()`로 없으면 자동 생성하므로 명시적 추가가 불필요.
- 스킨(색상/스프라이트)은 `ShapeBlock`의 `ApplyResolvedSkin()`이 최초 1회만 resolve 후 고정하는 기존 동작 덕분에 별도 로직 없이 모든 아이콘이 동일한 스킨으로 통일됨.
- JTH 워크스페이스의 `ShapeBlock.prefab`은 참조(로드·인스턴스화)만 하고 수정하지 않음 — Workspace 규칙 준수.
- 아이콘 PNG는 데이터(SO)와 분리해 `Sprites/BlockIcons/`에 저장(사용자 확인 반영).

---

## 2 — 2026-07-11 · 파일명 오류 수정 + 카메라 프레이밍 버그 수정 + 스킨 변경 시 자동 재생성

**바뀐 것**

- 수정: `Scripts/Editor/BlockShapeIconGenerator.cs`
  - `SanitizeFileName()` 추가 — `ShapeId`에 Windows 파일명 금지 문자(`*` 등)가 있으면 `_`로 치환 후 PNG 경로 생성 (`4*1` shapeId에서 `IOException: ERROR_INVALID_NAME` 발생하던 문제)
  - `FrameCamera()` — `camera.aspect`가 `targetTexture` 대입 전이라 Game 뷰 비율(스테일 값)을 참조하던 버그 수정. 렌더 타겟이 항상 정사각형(`IconSize x IconSize`)임을 활용해 `Mathf.Max(extents.x, extents.y)`로 계산하도록 단순화 — 가로로 긴 도형(예: `4x1`)이 정사각 아이콘 안에서 잘리던 문제 해결
- 생성: `Scripts/Editor/BlockShapeIconAutoRegenerator.cs`
  - `AssetPostprocessor.OnPostprocessAllAssets`로 `ShapeBlock.prefab`(현재 유일한 스킨 정의처: `skinColors`/`skinSprites`) 재임포트를 감지 → `EditorApplication.delayCall`로 `BlockShapeIconGenerator.GenerateAllIcons()` 자동 호출

**메모**

- `ShapeId`는 자유 입력 텍스트라 파일명으로 그대로 쓰면 안전하지 않음을 재확인 — 저장 시점(`SaveShape`)이 아니라 아이콘 파일명 생성 시점에서 sanitize.
- 프로젝트에 별도 `IBlockSkin` SO 구현체가 아직 없어(`Magnet.Contracts.BlockSkins.IBlockSkin` 인터페이스만 존재), "스킨 변경"의 실체는 현재 `ShapeBlock.prefab` 인스펙터 값 편집뿐 — 그 프리팹의 재임포트를 트리거로 삼음. 추후 별도 스킨 SO 시스템이 생기면 이 트리거 대상을 교체해야 함.
- `OnPostprocessAllAssets` 콜백 내부에서 바로 `AssetDatabase` 작업을 하지 않고 `delayCall`로 미뤄 임포트 파이프라인 재진입 문제를 피함.
