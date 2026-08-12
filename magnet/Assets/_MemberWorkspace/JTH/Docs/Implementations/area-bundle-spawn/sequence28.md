# sequence28 — Phase 28 변경 기록

> Phase 계획: [phase28.md](phase28.md)

## 1 — 2026-08-11 · 패 선택 시뮬 배치 기즈모

**바뀐 것** — 패를 고를 때 보드에 블록을 넣어 본 우승/Unique 언락 수를 Scene 기즈모로 그린다(텍스트 없음).

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleExplainStep.cs`
  - 심볼: `AreaBundleExplainStep` — readonly struct (추가)
    - 설명: `PieceSlotIndex`·`Pivot`·`Cells`(절대 격자)를 담는다.
    - 이유: 기즈모가 “직접 넣은 칸”만 그리려면 선택 결과와 분리된 스텝 타입이 필요해서.
  - 심볼: `AreaBundleExplainStep.AreaBundleExplainStep(...)` — 생성자 (추가)
    - 설명: 슬롯·피벗·칸 목록을 불변으로 보관한다.
    - 이유: MaxArea/Unique 경로에서 동일 타입으로 넘기기 위해.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleMetrics.cs`
  - 심볼: `AreaBundleMetrics.TryGetBestSequenceExplain` — 메서드 (추가)
    - 설명: MaxArea 우승 보드·점수와 함께 배치 스텝 리스트를 반환한다.
    - 이유: 선택 직후 “왜 이 패인지”를 같은 시뮬 경로로 재현하기 위해.
  - 심볼: `AreaBundleMetrics.TryGetBoardAfterBestSequence` — 메서드 (수정)
    - 설명: 내부에서 `TryGetBestSequenceExplain`을 호출하고 explain은 버린다.
    - 이유: Clean 체이닝 등 기존 호출부를 유지하면서 경로 추적을 한곳으로 모은다.
  - 심볼: `AreaBundleMetrics.SearchMaxArea` — 메서드 (수정)
    - 설명: 탐색 중 `path`에 스텝을 push/pop하고 최고점일 때 `bestPath`를 복사한다.
    - 이유: 최종 보드만으로는 기즈모에 넣을 중간 배치를 알 수 없어서.
  - 심볼: `AreaBundleMetrics.BuildExplainStep` / `CopyExplainPath` — 메서드 (추가)
    - 설명: offsets+pivot → 절대 칸 스텝 생성, path 복사.
    - 이유: SearchMaxArea가 참조를 공유하지 않게 하기 위해.
- 파일: `Scripts/Domain/AreaBundleSpawn/UniqueUnlockGenerator.cs`
  - 심볼: `UniqueUnlockGenerator.Result.ExplainSteps` — 프로퍼티 (추가)
    - 설명: 언락 성공 시 시뮬 배치 스텝을 보관한다.
    - 이유: Unique 패도 “넣은 칸” 기즈모가 필요해서.
  - 심볼: `UniqueUnlockGenerator.TryUnlockWithLineClear` / `TryOrder` — 메서드 (수정)
    - 설명: 성공 시 언락 2수 + 막힌 피스 1배치를 Result 슬롯 인덱스로 기록한다.
    - 이유: Unique 판정의 근거가 그 배치이기 때문.
  - 심볼: `UniqueUnlockGenerator.TryFindAnyPlacement` / `BuildExplainStep` — 메서드 (추가)
    - 설명: 언락 후 blocked 한 칸 배치를 찾고 스텝을 만든다.
    - 이유: Unique가 “막힌 피스가 열린다”는 것까지 보이게.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleSelectionResult.cs`
  - 심볼: `AreaBundleSelectionResult.ExplainSteps` — 프로퍼티 (추가)
    - 설명: 선택 시점 시뮬 배치 스텝 목록.
    - 이유: Bootstrap/기즈모가 LastSelection에서 읽게.
  - 심볼: `AreaBundleSelectionResult.AreaBundleSelectionResult(...)` — 생성자 (수정)
    - 설명: optional `explainSteps` 인자를 받아 없으면 빈 배열.
    - 이유: kill 랜덤 등 배치 근거가 없는 경로 호환.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator.CaptureExplain` — 메서드 (추가)
    - 설명: `TryGetBestSequenceExplain`로 MaxArea 스텝을 가져온다.
    - 이유: Normal/Easy/Hospitality/AllClear 결과 생성부를 한 헬퍼로 맞춤.
  - 심볼: `AreaBundleOrchestrator.ToResult` / `TrySelectHospitality` / `TrySelectAllClearExact` / `TrySelectUniqueDynamic` — 메서드 (수정)
    - 설명: 결과 생성 시 `ExplainSteps`를 채운다(Unique는 generator 스텝).
    - 이유: 기즈모가 티어별로 같은 필드를 보게.
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `AreaBundleSelectionGizmo.boardConfig` / `step0Color` / `step1Color` / `step2Color` / `uniqueBlockedColor` / `drawFilled` — 필드 (추가)
    - 설명: 칸 크기 SO와 스텝·Unique blocked 색, 채움 토글.
    - 이유: SO는 SerializeField, 색은 Inspector 튜닝.
  - 심볼: `AreaBundleSelectionGizmo._spawnBootstrap` / `_gameBoard` — Inject 필드 (추가)
    - 설명: LastSelection과 GridToWorld 소스.
    - 이유: 씬 MonoBehaviour는 Inject 규칙.
  - 심볼: `AreaBundleSelectionGizmo.Awake` — 메서드 (추가)
    - 설명: SO·Inject null Assert.
    - 이유: 배선 누락 조기 발견.
  - 심볼: `AreaBundleSelectionGizmo.OnDrawGizmos` — 메서드 (추가)
    - 설명: Play 중 ExplainSteps 칸을 스텝 색으로 DrawCube/WireCube. 텍스트 없음.
    - 이유: 디버그 요청이 “기즈모만”이어서.
  - 심볼: `AreaBundleSelectionGizmo.ResolveStepColor` — 메서드 (추가)
    - 설명: Unique blocked 슬롯은 빨강, 그 외 스텝 인덱스 색.
    - 이유: Unique와 Area 패를 구분하기 쉽게.
- 씬: `Scenes/Phase0_Bootstrap.unity`
  - 심볼: `AreaBundleSelectionGizmo` GameObject (추가)
    - 설명: 컴포넌트 + DefaultBoardConfig 배선.
    - 이유: Play 시 Scene 뷰에서 바로 보이게.

## 2 — 2026-08-11 · 현재 패 형태 미리보기

**바뀐 것** — 보드 오른쪽에 방금 뽑은 3슬롯 피스 형태도 기즈모로 그린다. 보드 배치 칸과 슬롯 색을 맞춤.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `AreaBundleSelectionGizmo.piece0Color` / `piece1Color` / `piece2Color` — 필드 (수정)
    - 설명: 예전 stepNColor를 슬롯 기준으로 개명. 패·보드 배치가 같은 색을 씀.
    - 이유: “지금 패가 뭔지”와 “어디에 넣었는지”를 색으로 대응시키기 위해.
  - 심볼: `AreaBundleSelectionGizmo.drawHandPreview` / `handGapCells` — 필드 (추가)
    - 설명: 패 미리보기 on/off와 슬롯 세로 간격(칸).
    - 이유: Inspector에서 가독성 조절.
  - 심볼: `AreaBundleSelectionGizmo.DrawHandPreview` — 메서드 (추가)
    - 설명: `LastSelection.Pieces`를 보드 오른쪽 열에 슬롯별 색으로 그린다.
    - 이유: UI 슬롯과 별도로 Scene에서 현재 패 형태를 바로 보게.
  - 심볼: `AreaBundleSelectionGizmo.DrawExplainSteps` — 메서드 (추가)
    - 설명: 시뮬 배치 칸을 `PieceSlotIndex` 색으로 그린다(본문 분리).
    - 이유: OnDrawGizmos에서 패 미리보기와 배치 오버레이를 분리.
  - 심볼: `AreaBundleSelectionGizmo.HandColumnOrigin` / `GetBounds` / `ResolvePieceColor` — 메서드 (추가·수정)
    - 설명: 미리보기 원점·바운드 정규화·슬롯/Unique 색 결정.
    - 이유: 패 형태를 겹치지 않게 쌓고 보드 칸과 색을 일치.
  - 심볼: `AreaBundleSelectionGizmo.ResolveStepColor` — 메서드 (삭제)
    - 설명: `ResolvePieceColor`로 대체.
    - 이유: 슬롯 기준 색으로 통일.

## 3 — 2026-08-11 · 기즈모 혼동 정리

**바뀐 것** — 보드 위 시뮬은 와이어만, 패 미리보기는 UI처럼 보드 아래 가로·Candidates 기준. Awake 전 NRE 가드.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `AreaBundleSelectionGizmo.DrawExplainSteps` — 메서드 (수정)
    - 설명: 채움 큐브 제거, `explainWireScale` 와이어만.
    - 이유: 채움 기즈모가 보드 실블록처럼 보여 “이상한 블록”으로 오해됨.
  - 심볼: `AreaBundleSelectionGizmo.DrawHandPreview` / `DrawHandFromCandidates` / `HandRowOrigin` — 메서드 (수정·추가)
    - 설명: `Candidates` 오프셋으로 보드 아래 가로 3슬롯 배치.
    - 이유: Game UI 슬롯 순서·형태와 맞추고, 오른쪽 세로 배치로 인한 혼동 제거.
  - 심볼: `AreaBundleSelectionGizmo.drawHandFilled` / `handBelowCells` / `explainWireScale` — 필드 (추가·개명)
    - 설명: 패 채움 토글, 보드 아래 간격, 와이어 크기.
    - 이유: 패/보드 기즈모 역할을 분리해 Inspector에서 조절.
  - 심볼: `AreaBundleSelectionGizmo.OnDrawGizmos` — 메서드 (수정)
    - 설명: `GameBoard.Grid == null`이면 early return.
    - 이유: Awake 전 OnDrawGizmos NRE 방지.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap.LastSelection` — 프로퍼티 (수정)
    - 설명: `_drawer?.LastResult` null-conditional.
    - 이유: drawer 생성 전 기즈모가 LastSelection을 읽어도 NRE 안 나게.

## 4 — 2026-08-11 · 티어/Clean·Main 모드 표시

**바뀐 것** — 결과에 Profile을 넣고, 패 아래 색 띠+라벨로 올클/Clean노말/Main노말/이지 등을 표시.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleSelectionResult.cs`
  - 심볼: `AreaBundleSelectionResult.Profile` — 프로퍼티 (추가)
    - 설명: Normal Clean/Main(및 Unique 가중) 프로파일.
    - 이유: Tier=Normal만으로는 Clean/Main 구분이 안 돼서.
  - 심볼: `AreaBundleSelectionResult.AreaBundleSelectionResult(...)` — 생성자 (수정)
    - 설명: `profile` 인자 추가(기본 Main).
    - 이유: Orchestrator가 선택 시 모드를 결과에 심기 위해.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator.ToResult` — 메서드 (수정)
    - 설명: `ShapeWeightProfile profile`을 받아 결과에 넣음.
    - 이유: PickAreaWithDeathReject의 Clean/Main을 기즈모·로그가 쓰게.
  - 심볼: `AreaBundleOrchestrator.TrySelectUniqueDynamic` — 메서드 (수정)
    - 설명: `profile: Unique`로 결과 생성.
    - 이유: Unique도 Profile 필드를 일관되게 채움.
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `AreaBundleSelectionGizmo.DrawModeBanner` / `ResolveModeStyle` — 메서드 (추가)
    - 설명: 패 아래 티어 색 띠 + Handles.Label(올클리어/Normal-Clean 등).
    - 이유: Scene에서 모드가 한눈에 안 보인다는 피드백.
  - 심볼: `AreaBundleSelectionGizmo.tier*` / `drawModeLabel` / `modeBarHeightCells` — 필드 (추가)
    - 설명: 티어별 색·라벨 토글·띠 높이.
    - 이유: Inspector 튜닝.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap.TierStyle` — 메서드 (수정)
    - 설명: Normal-Clean / Normal-Main 로그 라벨 분리.
    - 이유: 콘솔과 기즈모 표기를 맞춤.
