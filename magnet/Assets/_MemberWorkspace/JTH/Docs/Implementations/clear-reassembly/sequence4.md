# sequence4 — Phase 4 변경 기록

> Phase 계획: [phase4.md](phase4.md)

## 1 — 2026-07-14 · 칸 View 분해 + 달팽이 LitMotion

**바뀐 것** — 부착 후 칸 View로 분해하고, 재배치 웨이브를 튕김·공전·착지 연출로 재생.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/OccupiedCellView.cs`
  - 심볼: `Bind` / `PlaySnailAsync` / `AnimateMoveTo` / `OrbitClockwise` (추가)
    - 설명: 칸 1개 View. 3칸 튕김→시계 1바퀴→착지, 비행 중 자전, 착지 시 격자 0° 스냅.
    - 이유: 물리 없이 LitMotion으로 달팽이 연출.
- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlayPlaceAsync(PlacementResult)` / `PlayReassemblyAsync` / `SplitStagingIntoCells` (추가·수정)
    - 설명: 스냅 후 ShapeBlock을 cellId View로 분해, 웨이브별 파괴·스태거 달팽이·회전.
    - 이유: 보드 위는 칸 View만 유지.
- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `DetachActiveBlocks` (추가), `PlacedBlock` API (삭제)
    - 설명: 스냅 후 Block을 분리해 칸 View에 넘긴다.
    - 이유: 부착 후 멀티칸 ShapeBlock을 해체.
- 파일: `Scripts/Presentation/BlockSnapMotion.cs`
  - 심볼: `PlayFromOffsets` (추가), `PlayFromPlaced` (삭제)
    - 설명: offset+pivot 기준 Y 스냅.
    - 이유: PlacedBlock 제거.
- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `BounceCells`/`OrbitDuration`/`SpinDegreesPerSecond`/`StaggerPerCell`/`StaggerPerRing` 등 (추가)
    - 설명: 재조립 연출 수치 SO.
    - 이유: 튕김 3칸·스태거 등을 인스펙터에서 조정.

**메모** — 화면 밖 튕김 허용.
---

## 2 — 2026-07-16 · 링 동시 이동 + SO Header·이징 곡선

**바뀐 것** — 같은 링은 한꺼번에 재배치하고, PlacementConfig에 Header·동작별 AnimationCurve를 추가했다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `snapEase` / `rotationEase` / `bounceEase` / `landEase` — 필드 `AnimationCurve` (추가)
    - 설명: Place 스냅·보드 회전·튕김·착지 각각에 LitMotion용 이징 곡선을 둔다. 기본은 OutQuad 근사.
    - 이유: 하드코딩 `Ease.OutQuad` 대신 인스펙터에서 동작별로 조율.
    - 영향: `ShapeBlock.AnimateSnapYFromOffsets`, `OccupiedCellView` 이동 트윈.
  - 심볼: `SnapEase` / `RotationEase` / `BounceEase` / `LandEase` — 프로퍼티 (추가)
    - 설명: 위 곡선 필드의 읽기 전용 접근자.
    - 이유: Presentation이 SO에서 곡선을 소비.
  - 심볼: `StaggerPerCell` — 프로퍼티 (삭제)
    - 설명: 같은 링 칸 사이 스태거 필드를 제거한다.
    - 이유: 같은 링은 동시 이동이므로 칸 단위 딜레이 불필요.
  - 심볼: `StaggerPerRing` Tooltip (수정)
    - 설명: “같은 링은 동시 이동”을 Tooltip에 명시.
    - 이유: 링 간 딜레이만 남는 계약을 인스펙터에 드러냄.
  - 심볼: `CreateOutQuadCurve` / `EnsureEaseCurve` — 메서드 (추가)
    - 설명: OutQuad 근사 곡선 생성·빈 곡선 복구(OnValidate).
    - 이유: 에셋/직렬화 누락 시 이징이 깨지지 않게.
  - 심볼: Header 그룹 (`Staging / Visual`, `Drag`, `Snap`, `Board Rotation`, `Clear Reassembly`, `Clear Reassembly Motion`) (추가)
    - 설명: 인스펙터 필드를 구간별로 묶는다.
    - 이유: SO 가독성.
- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlayWaveRelocationsAsync` (수정)
    - 설명: 딜레이를 `ringStartDelay`만 사용. `indexInRing * StaggerPerCell` 제거.
    - 이유: 같은 링 칸이 한꺼번에 움직이게.
  - 심볼: `AnimateBoardRotation` (수정)
    - 설명: `AnimateMoveTo`에 `placementConfig.RotationEase` 전달.
    - 이유: 회전 이동도 SO 곡선 사용.
- 파일: `Scripts/Presentation/OccupiedCellView.cs`
  - 심볼: `AnimateMoveTo(..., AnimationCurve ease, ...)` (수정)
    - 설명: `WithEase(ease)`로 보드 회전 칸 이동.
    - 이유: 이징을 SO에서 주입.
  - 심볼: `PlayRelocationAsync` / `TweenPositionWithSpin` (수정)
    - 설명: 튕김·착지에 `BounceEase`/`LandEase` 적용.
    - 이유: 재배치 전 구간의 이징을 분리 조율.
- 파일: `Scripts/Presentation/ShapeBlock.cs`
  - 심볼: `AnimateSnapYFromOffsets` (수정)
    - 설명: `WithEase(placementConfig.SnapEase)`로 Y 스냅.
    - 이유: Place 스냅도 SO 곡선.
- 파일: `ScriptableObjects/DefaultPlacementConfig.asset`
  - 심볼: `snapEase`/`rotationEase`/`bounceEase`/`landEase` (추가), `StaggerPerCell` (삭제)
    - 설명: 기본 OutQuad 근사 곡선 직렬화, 칸 스태거 제거.
    - 이유: 런타임 기본 에셋과 코드 필드 동기화.

**메모** — 조율은 `Assets/_MemberWorkspace/JTH/ScriptableObjects/DefaultPlacementConfig` 인스펙터에서.
---

## 3 — 2026-07-16 · 이징 AnimationCurve → LitMotion Ease enum

**바뀐 것** — SO 이징 필드를 LitMotion `Ease` enum 드롭다운으로 교체.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `SnapEase` / `RotationEase` / `BounceEase` / `LandEase` — 프로퍼티 타입 (수정)
    - 설명: `AnimationCurve` → LitMotion `Ease` enum. 인스펙터 드롭다운.
    - 이유: 프로젝트 표준(LitMotion)과 KTJ UI와 동일 패턴. 커스텀 곡선 불필요.
  - 심볼: `CreateOutQuadCurve` / `EnsureEaseCurve` (삭제)
    - 설명: AnimationCurve 보조 코드 제거.
    - 이유: enum 전환 후 불필요.
- 파일: `Scripts/Presentation/OccupiedCellView.cs`
  - 심볼: `AnimateMoveTo` / `TweenPositionWithSpin` — `Ease` 파라미터 (수정)
    - 설명: `WithEase(Ease)` 호출 유지.
    - 이유: SO enum과 타입 일치.
- 파일: `ScriptableObjects/DefaultPlacementConfig.asset`
  - 심볼: `<SnapEase>` 등 — 직렬화 (수정)
    - 설명: 곡선 YAML → `Ease.OutQuad`(5) 정수.
    - 이유: 에셋·코드 동기화.

**메모** — 커스텀 곡선이 필요하면 `AnimationCurve` + `WithEase(curve)`로 되돌릴 수 있음.
---
