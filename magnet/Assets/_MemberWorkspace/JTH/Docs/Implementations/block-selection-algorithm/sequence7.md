# Sequence — Phase 7 (block-selection-algorithm)

> **Phase:** [phase7.md](phase7.md) 와 1:1.

## 1 — 2026-08-01 · Drawer 연동·Bootstrap·로그

**바뀐 것**

- 생성: `Scripts/Domain/Spawn/BlockSelectionDrawer.cs`
- 수정: `Scripts/Domain/Spawn/BlockSpawnContext.cs`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
- 수정: `Docs/INSPECTOR_TOOLTIPS.md` — Phase 3~7 신규 SO 필드 표
- 씬: `New_02_Main` — `BlockSpawnBootstrap` 2개 인스턴스(활성 1·비활성 1)에 튜닝·번들 풀 SO 연결 후 저장

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Spawn/BlockSelectionDrawer.cs`
  - 심볼: `BlockSelectionDrawer : AbstractDrawer` — class (추가)
    - 설명: `Draw`에서 `BlockSelectionOrchestrator.SelectPieces` 호출, 결과 피스 반환.
    - 이유: 기존 `BlockSupply` 파이프라인(SPEC §16.1)에 무변경 삽입 — `RandomDrawer` 자리 교체.
  - 심볼: `BlockSelectionDrawer.LastResult` — 프로퍼티 (추가)
    - 설명: 직전 리필의 `BlockSelectionResult`.
    - 이유: 로그·UI 훅 데이터 노출 — Domain은 이벤트 발행 안 함 (팀 규칙).

- 파일: `Scripts/Domain/Spawn/BlockSpawnContext.cs`
  - 심볼: `Health / BlameTotal / IsRetrySession / TurnIndex` — 프로퍼티 4종 (추가)
    - 설명: 리필 1회분 알고리즘 입력. `Score`는 알고리즘에서 읽지 않음 (SPEC §16.2 — 필드 유지).
    - 이유: 스펙의 `BlameTracker`/SO 전달 대신 값만 전달 — Drawer가 필요한 건 수치뿐, 상태·에셋 소유는 Bootstrap (phase7.md 결정).

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `ExcludedProbeShapeId` — const string "1x1" (추가)
    - 설명: 배치 자유도 프로브에서 제외할 모양 (SPEC §12.1 — 1x1 제외 16종).
  - 심볼: `selectionTuningSO / bundlePoolSO` — [SerializeField] SO 2종 (추가, Awake Assert)
    - 이유: DI 규칙 — 프로젝트 에셋 SO는 SerializeField (Inject 금지).
  - 심볼: `_drawer / _blameTracker / _probePieces / _turnIndex / _turnStartBoard / _turnStartHealth / _isRetrySession` — private 필드 (추가)
    - 설명: 선택기 상태. `_isRetrySession`은 상수 false — 재시작이 씬 리로드 방식이라 크로스-씬 상태 확정 전까지 Relife 게이트 닫힘 (phase7.md 결정).
  - 심볼: `LastSelection / LastTurnFeedback` — public 프로퍼티 (추가)
    - 설명: 직전 선택 결과 / 직전 턴 blame 판정 (GoodTurn·유일해 매칭의 UI 소비 지점).
  - 심볼: `Awake` — 수정
    - 설명: `RandomDrawer` → `BlockSelectionDrawer(Orchestrator)`. 프로브 피스·BlameTracker 구성.
  - 심볼: `Fill` — 수정
    - 설명: health 계산(리필당 1회) → 직전 턴 blame 정산(`OnTurnEnded`, 첫 리필 제외) → 스냅샷 갱신 → 컨텍스트 조립 → `_supply.Fill` → `[BlockSelect]` 로그 → turnIndex 증가.
    - 이유: Fill은 lastDrop에만 호출되므로 "3피스 라운드 종료" 시점과 일치 — `allPiecesPlaced=true` 보장 (phase7.md 결정).
  - 심볼: `LogSelection` — private (추가)
    - 설명: SPEC §20 형식 1줄 로그 (turn·zone·health·blame·tier·bundle).
  - 심볼: `BuildProbePieces` — private (추가)
    - 설명: `shapeSourceSO.Shapes`에서 1x1 제외 offsets 목록 구성 — Domain은 목록 출처를 모름 (phase2 메모의 약속 이행).

**검증**

- `read_console` 컴파일 에러 0.
- 플레이 모드: `[BlockSelect] turn=0 zone=TooEmpty health=0.60 blame=0.0 tier=Normal bundle=normal_zigzag` — 예외 없음 ✅
- 첫 플레이 시 씬에 `BlockSpawnBootstrap`이 2개(비활성 백업 존재) → 미연결 인스턴스에서 NRE → 두 인스턴스 모두 SO 연결 후 재확인 ✅

**메모**

- 남은 일: `IsRetrySession` 연동(Relife 게이트 개방), GoodTurn·`MatchesStep` 이벤트 발행(UI 계약 후).
- Pressure 발동 빈도·빔 폭·sampleCount는 실플레이 프로파일링 후 튜닝 권장 (`BlockSelectionTuningSO`).
