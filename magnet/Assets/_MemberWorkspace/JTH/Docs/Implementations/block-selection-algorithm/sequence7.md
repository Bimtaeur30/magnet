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

## 2 — 2026-08-01 · 색상 강조 진단 로그 3종 (뽑기·Blame·BoardHp)

**바뀐 것**

- 수정: `Scripts/Domain/BlockSelection/Blame/TurnFeedback.cs`
- 수정: `Scripts/Domain/BlockSelection/Blame/BlameTracker.cs`
- 수정: `Scripts/Domain/BlockSelection/BlockSelectionResult.cs`
- 수정: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Blame/TurnFeedback.cs`
  - 심볼: `TurnFeedback.NewDeadZones / CenterCellsGained / BigSlotLost / FreedomDrop / DecayLoss` — 프로퍼티 5종 (추가)
    - 설명: 이번 턴 blame 증감의 성분 분해값 (새 dead zone 수, 중앙 점유 칸, 큰 슬롯 상실 여부, 자유도 하락량, 감쇠 손실량).
    - 이유: "Blame이 왜 올랐는지/내렸는지" 로그에 사유를 찍으려면 합산 delta만으로는 부족 — 계산 주체(BlameTracker)가 성분을 노출해야 함.
    - 영향: `BlockSpawnBootstrap.LogBlameChange`가 소비.
  - 심볼: `TurnFeedback` 생성자 — (수정)
    - 설명: 분해 필드 5종을 파라미터로 받도록 확장.
    - 이유: readonly struct라 생성 시점에 전부 주입.

- 파일: `Scripts/Domain/BlockSelection/Blame/BlameTracker.cs`
  - 심볼: `BlameTracker.OnTurnEnded` — 메서드 (수정)
    - 설명: 기존 delta 합산은 동일. 성분값(newDeadZones·centerCellsGained·bigSlotLost·freedomDrop)을 지역변수로 보존하고, 감쇠 손실 `decayLoss = Total × (1 − decayRate)`를 Total 갱신 전에 계산해 `TurnFeedback`에 전달. 음수 성분(dead zone 감소·자유도 상승)은 0으로 클램프해 "가산 기여분"만 노출.
    - 이유: blame 수치 로직 무변경으로 사유 데이터만 추출 — 로그가 실제 계산과 어긋나지 않게 계산 지점에서 직접 뽑음.

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionResult.cs`
  - 심볼: `BlockSelectionResult.SelectionReason` — 프로퍼티 (추가)
    - 설명: "선택 이유: …" 1줄 + "상위 티어 경과: …" 1줄로 구성된 여러 줄 진단 문자열.
    - 이유: 어떤 티어가 왜 나왔는지는 오케스트레이터만 정확히 앎(확률 굴림·번들 검증 실패 등) — 결과에 실어 소비 측(Bootstrap 로그)에 전달.
  - 심볼: `BlockSelectionResult` 생성자 — (수정)
    - 설명: `selectionReason` 파라미터 추가.
    - 이유: 불변 결과 객체라 생성 시점 주입.

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `BlockSelectionOrchestrator._trace` — 필드 `List<string>` (추가)
    - 설명: `SelectPieces` 1회 동안 스킵·실패한 티어의 경과 메모. 호출 시작 시 Clear.
    - 이유: 최종 티어까지 내려온 경로("왜 상위 티어가 안 나왔나")를 로그에 담기 위함. 리필당 1회라 GC 부담 미미.
  - 심볼: `BlockSelectionOrchestrator.SelectPieces` — 메서드 (수정)
    - 설명: 각 티어 게이트를 else-if 사슬로 세분화해 스킵 사유(존 불일치 / blame 문턱 미달 / 확률 굴림 실패 / 번들·생성 실패)를 `_trace`에 기록. 확률 굴림은 앞 조건 통과 시에만 수행 — 판정 순서·결과 분포는 기존과 동일.
    - 이유: "왜 그런 뽑기가 나왔는지" 로그의 정확성 — Bootstrap에서 문턱값으로 역추정하면 게이트 통과 후 번들 실패 케이스를 구분 못 함.
  - 심볼: `BlockSelectionOrchestrator.ForceNormalAny` — 메서드 (수정)
    - 설명: 두 최후 수단 분기에 각각 이유 문자열 전달.
    - 이유: Fallback 로그에서 "번들 강제"와 "3피스 강제 샘플"을 구분.
  - 심볼: `BlockSelectionOrchestrator.FromBundle / FromGenerated` — 메서드 (수정, static → instance)
    - 설명: `selectedReason` 파라미터 추가, `ComposeReason` 결과를 결과 객체에 주입.
    - 이유: `_trace` 접근이 필요해 instance로 전환.
  - 심볼: `BlockSelectionOrchestrator.ComposeReason` — 메서드 (추가)
    - 설명: 선택 이유 1줄 + `_trace`를 " · "로 이어 붙인 상위 티어 경과 1줄을 합성.
    - 이유: 로그 포맷 조립을 한 곳에 고정.

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap.Fill` — 메서드 (수정)
    - 설명: 턴 정산 직전 `blameBefore` 캡처, 정산 직후 `LogBlameChange`·`LogHealthChange` 호출 (첫 리필은 정산이 없어 로그 없음).
    - 이유: Blame·Hp 증감이 확정되는 유일한 시점(턴 정산)에 맞춰 로그 — "맞는 타이밍" 요구.
  - 심볼: `BlockSpawnBootstrap.LogSelection` — 메서드 (수정)
    - 설명: 기존 `[BlockSelect]` 1줄 로그 유지 + `[뽑기]` 강조 로그 추가 — 티어 한글명·색상·bold 헤드라인, 아랫줄(비강조)에 `SelectionReason`.
    - 이유: 기존 로그 외에 색상 강조 로그 요청 — 기존 소비자(SPEC §20 형식) 보존.
  - 심볼: `BlockSpawnBootstrap.TierStyle` — 메서드 (추가)
    - 설명: 티어 → (한글 라벨, hex 색) 매핑. Trap 빨강 / Pressure 주황 / Hospitality 초록 / Normal 흰색 등.
    - 이유: 콘솔에서 티어를 색으로 즉시 식별.
  - 심볼: `BlockSpawnBootstrap.LogBlameChange` — 메서드 (추가)
    - 설명: `[Blame] before → after (±net)` 헤드라인(증가 빨강·감소 초록) + 아랫줄에 성분별 사유(새 dead zone·중앙 점유·큰 슬롯 상실·자유도 하락·감쇠).
    - 이유: Blame 로그에는 Blame 사유만 — 요청 명세.
  - 심볼: `BlockSpawnBootstrap.LogHealthChange` — 메서드 (추가)
    - 설명: `[BoardHp] before → after (±delta)` 헤드라인(하락 빨강·상승 초록) + zone 변화 + 아랫줄에 채움률·dead zone·큰 슬롯·자유도 변화.
    - 이유: Hp 로그에는 Hp 사유만 — 요청 명세.

**검증**

- `refresh_unity` 후 `read_console` 컴파일 에러 0 ✅

**메모**

- 강조 로그는 Unity 콘솔 rich text (`<color>`·`<b>`) 사용 — 리스트 뷰에선 첫 줄, 선택 시 사유 줄까지 표시.
