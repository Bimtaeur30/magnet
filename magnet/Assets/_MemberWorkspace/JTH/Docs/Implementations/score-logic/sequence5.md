## 1 — 2026-07-24 · Block Blast 역분석 점수 공식

**바뀐 것** — square/kTier 공식을 `λ(n)×base×combo×tier`로 교체. 세션 base 랜덤·soft-cap·콤보 +1/클리어.

### 변경 상세

- 파일: `Scripts/Data/ScoreConfigSO.cs`
  - 심볼: `ScoreConfigSO.BaseMin` — 프로퍼티 (추가)
    - 설명: 세션 base 랜덤 하한(포함, 기본 30).
    - 이유: Block Blast처럼 판마다 base가 달라지며 범위를 SO로 조절.
  - 심볼: `ScoreConfigSO.BaseMax` — 프로퍼티 (추가)
    - 설명: 세션 base 랜덤 상한(포함, 기본 55).
    - 이유: BaseMin과 쌍으로 균등 추출 구간 정의.
  - 심볼: `ScoreConfigSO.SoftCapClearScoreThreshold` — 프로퍼티 (추가)
    - 설명: 클리어 점수 누적 임계(기본 75000). 이상이면 11+ tier 1.25 고정.
    - 이유: 원작 고콤보 배율 드랍을 재현.
  - 심볼: `ScoreConfigSO.Random` / `ClearBonus` — 프로퍼티 (삭제)
    - 설명: 임시·구 계수 필드 제거.
    - 이유: 새 base/soft-cap 필드로 대체.

- 파일: `Scripts/Domain/Score/ScoreCalculator.cs`
  - 심볼: `ScoreCalculator.LineMultiplier(int)` — 메서드 (추가)
    - 설명: n≤1 → 1, n≥2 → n(n-1).
    - 이유: 5줄=20 등 실측 배수(비계승).
  - 심볼: `ScoreCalculator.ResolveTier(int, bool)` — 메서드 (추가)
    - 설명: 콤보 구간 1/1.5/2.0, softCap 시 11+는 1.25.
    - 이유: 배율 테이블을 순수 함수로 고정.
  - 심볼: `ScoreCalculator.ClearScore(...)` — 메서드 (추가)
    - 설명: λ×base×combo×tier를 반올림한 클리어 점수.
    - 이유: ScoreSession이 공식에만 의존하도록.

- 파일: `Scripts/Domain/Score/ScoreSession.cs`
  - 심볼: `ScoreSession._config` — 필드 (추가)
    - 설명: base 범위·soft-cap 임계를 읽기 위한 SO 참조.
    - 이유: Reset/ApplyPlacement에서 설정 공유.
  - 심볼: `ScoreSession._sessionBase` — 필드 (추가)
    - 설명: 이번 판에 고정된 base.
    - 이유: 클리어 점수의 선형 계수.
  - 심볼: `ScoreSession._clearScoreCumulative` — 필드 (추가)
    - 설명: 배치 제외 클리어 점수 누적.
    - 이유: soft-cap 판정.
  - 심볼: `ScoreSession._softCapped` — 필드 (추가)
    - 설명: 임계 도달 후 영구 감쇠 플래그.
    - 이유: 한 판에서 2.0 복귀 없음.
  - 심볼: `ScoreSession.ScoreSession(ScoreConfigSO)` — 생성자 (수정)
    - 설명: config 보관 후 Reset으로 base 롤.
    - 이유: 세션 수명과 base 수명 일치.
  - 심볼: `ScoreSession.TotalScore` / `Combo` / `SessionBase` / `ClearScoreCumulative` / `IsSoftCapped` — 프로퍼티 (추가)
    - 설명: 세션 상태 조회.
    - 이유: Bootstrap·이벤트·디버그.
  - 심볼: `ScoreSession.ApplyPlacement(...)` — 메서드 (수정)
    - 설명: 배치 칸 가산 + 클리어 시 콤보+1 후 ClearScore 합산, soft-cap 갱신, first/lastDrop으로 콤보 유지·구조.
    - 이유: 역분석 공식·턴 콤보 규칙 반영.
    - 영향: `BoardPlacementBootstrap.ApplyPlacementScore`
  - 심볼: `ScoreSession.Reset()` — 메서드 (추가)
    - 설명: 점수·콤보·soft-cap·턴 플래그 초기화 후 base 재추출.
    - 이유: 재시작 시 새 세션 수치.
  - 심볼: `ScoreSession.RollSessionBase()` — 메서드 (추가)
    - 설명: `[BaseMin, BaseMax]` 균등 정수 추출.
    - 이유: Block Blast식 랜덤 base.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap._scoreSession` 생성 — `new ScoreSession(scoreConfig)` (수정)
    - 설명: SO를 넘겨 세션 생성.
    - 이유: base 롤에 config 필요.
  - 심볼: `BoardPlacementBootstrap.ApplyPlacementScore` — 메서드 (수정)
    - 설명: clearedLineCount·firstDrop·lastDrop을 새 ApplyPlacement에 전달.
    - 이유: square size 목록 API 제거.
  - 심볼: `BoardPlacementBootstrap.PlaceBlock` — 메서드 (수정)
    - 설명: line clear 후 점수·ScoreChanged·ComboChanged만 처리 (회전/재조립 WIP 제거).
    - 이유: 깨진 비동기·deprecated 경로를 제거하고 Domain 점수 연동을 우선.

- 파일: `ScriptableObjects/DefaultScoreConfig.asset`
  - 심볼: 직렬화 필드 BaseMin/BaseMax/SoftCapClearScoreThreshold (수정)
    - 설명: 30 / 55 / 75000.
    - 이유: 권장 기본값.

- 파일: `Docs/DESIGN.md` §4.7 (수정)
  - 설명: 역분석 공식·soft-cap·콤보 규칙으로 갱신.
  - 이유: 팀 DESIGN 동기화.

- 파일: `Docs/INSPECTOR_TOOLTIPS.md` (수정)
  - 설명: ScoreConfigSO Tooltip 표 교체.
  - 이유: 필드 변경 반영.

## 2 — 2026-07-24 · soft-cap(1.25) 제거

**바뀐 것** — 추가 데이터에서 미재현되어 soft-cap 전부 삭제. tier는 11+에서 2.0 고정.

### 변경 상세

- 파일: `Scripts/Data/ScoreConfigSO.cs`
  - 심볼: `ScoreConfigSO.SoftCapClearScoreThreshold` — 프로퍼티 (삭제)
    - 설명: soft-cap 임계 필드 제거.
    - 이유: 1.25 배율 미채택.
- 파일: `Scripts/Domain/Score/ScoreCalculator.cs`
  - 심볼: `ScoreCalculator.ResolveTier(int)` — 메서드 (수정)
    - 설명: softCapped 인자 제거. 11+는 항상 2.0.
    - 이유: soft-cap 폐기.
  - 심볼: `ScoreCalculator.ClearScore(...)` — 메서드 (수정)
    - 설명: softCapped 인자 제거.
    - 이유: ResolveTier 시그니처 정리.
- 파일: `Scripts/Domain/Score/ScoreSession.cs`
  - 심볼: `ScoreSession._clearScoreCumulative` / `_softCapped` / `ClearScoreCumulative` / `IsSoftCapped` — (삭제)
    - 설명: soft-cap 추적 상태 제거.
    - 이유: 더 이상 임계 판정 불필요.
  - 심볼: `ScoreSession.ApplyPlacement` — 메서드 (수정)
    - 설명: ClearScore 호출에서 soft-cap 전달·누적 갱신 제거.
    - 이유: soft-cap 폐기.
- 파일: `ScriptableObjects/DefaultScoreConfig.asset` / `Docs/DESIGN.md` / Tooltip·phase 문서 (수정)
  - 설명: SoftCap 필드·서술 삭제.
  - 이유: 문서·에셋 동기화.

## 3 — 2026-07-24 · 첫 클리어는 UI 콤보 0

**바뀐 것** — 표시 콤보는 체인 두 번째 클리어부터 1. 점수 배수는 체인 클리어 순번(clearIndex) 유지.

### 변경 상세

- 파일: `Scripts/Domain/Score/ScoreSession.cs`
  - 심볼: `ScoreSession._chainClears` — 필드 (추가, `_combo` 대체)
    - 설명: 체인 안 클리어 횟수(1부터). 점수 배수·tier 입력.
    - 이유: UI 콤보와 점수 배수를 분리.
  - 심볼: `ScoreSession.Combo` — 프로퍼티 (수정)
    - 설명: `max(0, _chainClears - 1)`. 첫 클리어 후 0, 다음 클리어부터 1.
    - 이유: 첫 클리어는 콤보로 치지 않음.
  - 심볼: `ScoreSession.ApplyPlacement` — 메서드 (수정)
    - 설명: 클리어 시 `_chainClears++` 후 ClearScore에 clearIndex 전달. 결과에 UI Combo 반환.
    - 이유: 점수 공식은 clearIndex, 이벤트/HUD는 UI 콤보.
- 파일: `Scripts/Domain/Score/ScoreCalculator.cs`
  - 심볼: `ClearScore` / `ResolveTier` 인자명 — (수정)
    - 설명: `comboAfter` → `clearIndexInChain`.
    - 이유: UI 콤보와 혼동 방지.
- 파일: `Docs/DESIGN.md` §4.7 (수정)
  - 설명: 첫 클리어 콤보 0 규칙 명시.
  - 이유: 팀 규칙 동기화.

## 4 — 2026-07-24 · 구조 예외는 콤보≥1일 때만

**바뀐 것** — 2줄+ 첫수 구조는 UI 콤보가 이미 1 이상일 때만. 첫 클리어만 한 체인에는 미적용.

### 변경 상세

- 파일: `Scripts/Domain/Score/ScoreSession.cs`
  - 심볼: `ScoreSession.HasCombo` — 프로퍼티 (추가)
    - 설명: `_chainClears >= 2` (UI 콤보 ≥ 1).
    - 이유: 구조 예외 발동 조건.
  - 심볼: `ScoreSession.SaveCombo` — 프로퍼티 (수정)
    - 설명: `beforeLast && twoLineFirst` 항에 `HasCombo` AND 추가.
    - 이유: 콤보가 아닌 첫 클리어만으로는 예외로 체인을 이어주지 않음.
- 파일: `Docs/DESIGN.md` §4.7 (수정)
  - 설명: 구조 예외 전제(콤보≥1) 명시.
  - 이유: 규칙 동기화.
