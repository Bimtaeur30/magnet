## 1 — 2026-07-15 · Score Domain + ScoreConfigSO

**바뀐 것** — Domain 점수 세션·계산기·튜닝 계약 추가, `ScoreConfigSO`·기본 에셋·문서 추가

- 파일: `Scripts/Domain/Score/IScoreTuning.cs`
  - 심볼: `IScoreTuning` — 인터페이스 (추가)
    - 설명: Domain이 SO에 직접 의존하지 않도록 k·streak 조회 계약을 정의한다.
    - 이유: Bootstrap에서 에셋을 주입해도 Domain은 `IScoreTuning`만 받게 하기 위함.
  - 심볼: `IScoreTuning.GetK(int)` — 메서드 (추가)
    - 설명: 현재 콤보에 해당하는 배율 k를 반환한다.
    - 이유: 클리어 점수식 `k × combo × size × streak`의 k 항.
  - 심볼: `IScoreTuning.GetStreakMultiplier(int)` — 메서드 (추가)
    - 설명: 같은 배치 안 웨이브 순번(1-based)의 연쇄 배율을 반환한다.
    - 이유: 한 배치 다연쇄가 분할 클리어보다 이득이 되도록 하는 보너스 항.

- 파일: `Scripts/Domain/Score/ScoreCalculator.cs`
  - 심볼: `ScoreCalculator._tuning` — 필드 (추가)
    - 설명: k·streak 조회에 쓸 `IScoreTuning`을 보관한다.
    - 이유: 계산기는 상태를 갖지 않고 튜닝만 읽는다.
  - 심볼: `ScoreCalculator.ScoreCalculator(IScoreTuning)` — 생성자 (추가)
    - 설명: null이면 예외, 아니면 `_tuning`에 저장한다.
    - 이유: 튜닝 없이 점수 계산이 호출되지 않게 강제.
  - 심볼: `ScoreCalculator.ComputeWaveScore(int, int, int)` — 메서드 (추가)
    - 설명: `round(k × combo × squareSize × streakMult)`를 int로 반환. 인자 비정상이면 0.
    - 이유: 웨이브 1건 공식을 세션과 분리해 Phase 2에서도 재사용.

- 파일: `Scripts/Domain/Score/PlacementScoreResult.cs`
  - 심볼: `PlacementScoreResult.PlacementScoreResult(...)` — 생성자 (추가)
    - 설명: 배치 반영 결과 DTO 필드를 채운다. waveScores null이면 빈 배열.
    - 이유: Bootstrap이 delta·total·콤보·웨이브별 점수를 한 번에 받게 함.
  - 심볼: `PlacementScoreResult.ScoreDelta` — 프로퍼티 (추가)
    - 설명: 이번 배치로 오른 점수.
    - 이유: 이벤트/디버그에 증분 노출.
  - 심볼: `PlacementScoreResult.TotalScore` — 프로퍼티 (추가)
    - 설명: 반영 후 세션 누적 점수.
    - 이유: `ScoreChangedEvent`에 넣을 값(Phase 2).
  - 심볼: `PlacementScoreResult.ComboAfter` — 프로퍼티 (추가)
    - 설명: 반영 후 콤보.
    - 이유: UI·세이브 연동 준비.
  - 심볼: `PlacementScoreResult.WaveScores` — 프로퍼티 (추가)
    - 설명: 웨이브별 획득 점수 목록.
    - 이유: `SquareClearedEvent.scoreAwarded`에 웨이브 단위로 넣기 위함(Phase 2).
  - 심볼: `PlacementScoreResult.HadClear` — 프로퍼티 (추가)
    - 설명: 이번 배치에 클리어가 있었는지.
    - 이유: 클리어/배치-only 분기 확인.

- 파일: `Scripts/Domain/Score/ScoreSession.cs`
  - 심볼: `ScoreSession._calculator` — 필드 (추가)
    - 설명: 웨이브 점수 계산기.
    - 이유: ApplyPlacement에서 웨이브 루프에 사용.
  - 심볼: `ScoreSession._clearedThisTurn` — 필드 (추가)
    - 설명: 현재 턴(핸드) 중 클리어가 한 번이라도 있었는지.
    - 이유: `NotifyTurnEnded`에서 콤보 리셋 여부 판단.
  - 심볼: `ScoreSession.ScoreSession(IScoreTuning)` — 생성자 (추가)
    - 설명: `ScoreCalculator`를 만들고 점수를 0에서 시작한다.
    - 이유: 세션당 하나의 누적 상태를 소유.
  - 심볼: `ScoreSession.TotalScore` — 프로퍼티 (추가)
    - 설명: 세션 누적 점수.
    - 이유: 인게임 총점 소스 오브 트루스.
  - 심볼: `ScoreSession.Combo` — 프로퍼티 (추가)
    - 설명: 현재 콤보(웨이브마다 +1된 값).
    - 이유: 다음 배치 계산·턴 리셋 대상.
  - 심볼: `ScoreSession.ClearedThisTurn` — 프로퍼티 (추가)
    - 설명: `_clearedThisTurn` 읽기 전용 노출.
    - 이유: 디버그·테스트용.
  - 심볼: `ScoreSession.ApplyPlacement(int, IReadOnlyList<int>)` — 메서드 (추가)
    - 설명: wave 없으면 `+cellsPlaced`(콤보 불변). 있으면 웨이브마다 콤보+1 후 점수 합산, 턴 클리어 플래그 true.
    - 이유: 합의된 배치/클리어 점수·웨이브당 콤보 규칙 구현.
    - 영향: Phase 2 Bootstrap이 배치 성공 후 호출 예정.
  - 심볼: `ScoreSession.NotifyTurnEnded()` — 메서드 (추가)
    - 설명: 턴 중 클리어 없으면 콤보 0, 플래그 리셋.
    - 이유: 턴(핸드 4) 단위 콤보 리셋 합의.
    - 영향: Phase 3 `BlockSpawnBootstrap` Fill 경로에서 호출 예정.
  - 심볼: `ScoreSession.Reset()` — 메서드 (추가)
    - 설명: 총점·콤보·턴 플래그를 0/false로 초기화.
    - 이유: 새 판/재시작 준비(Phase 3).

- 파일: `Scripts/Data/ScoreConfigSO.cs`
  - 심볼: `ScoreConfigSO.KTier` — 중첩 구조체 (추가)
    - 설명: `maxComboInclusive` + `k` 한 구간.
    - 이유: 인스펙터에서 티어 테이블 편집.
  - 심볼: `ScoreConfigSO.KTier.maxComboInclusive` — 필드 (추가)
    - 설명: 이 구간의 최대 콤보(포함).
    - 이유: 오름차순 구간 탐색용.
  - 심볼: `ScoreConfigSO.KTier.k` — 필드 (추가)
    - 설명: 구간 배율.
    - 이유: BB 추정 k 튜닝.
  - 심볼: `ScoreConfigSO.kTiers` — 필드 (추가)
    - 설명: 기본 5/12/59/9999 → 80/66/45/36.
    - 이유: Exact 공식이 아니므로 SO로 밸런스.
  - 심볼: `ScoreConfigSO.streakMultipliers` — 필드 (추가)
    - 설명: 기본 1.00 / 1.35 / 1.60 / 1.80.
    - 이유: 웨이브 순번 배율을 숫자 2 고정이 아니라 튜닝 가능하게.
  - 심볼: `ScoreConfigSO.GetK(int)` — 메서드 (추가)
    - 설명: `IScoreTuning` 구현. 콤보가 속한 첫 구간의 k 반환.
    - 이유: Domain `ScoreCalculator`가 SO를 직접 몰라도 되게.
  - 심볼: `ScoreConfigSO.GetStreakMultiplier(int)` — 메서드 (추가)
    - 설명: 1-based 인덱스, 배열 밖이면 마지막 값, 비면 1.
    - 이유: 4웨이브 이상에서도 soft-cap 배율 유지.
  - 심볼: `ScoreConfigSO.OnValidate()` — 메서드 (추가)
    - 설명: maxCombo·k·streak를 음수 방지로 클램프.
    - 이유: 인스펙터 오입력으로 점수가 음수가 되지 않게.

- 파일: `ScriptableObjects/DefaultScoreConfig.asset` (추가)
  - 설명: 위 기본 k·streak 값이 들어간 `ScoreConfigSO` 에셋.
  - 이유: Phase 2에서 Bootstrap에 바로 연결 가능.

- 파일: `Docs/Implementations/score-logic/phases.md` · `phase1.md` (추가)
  - 설명: Phase 인덱스·Phase 1 계획.
  - 이유: 구현 기록 규칙.

- 파일: `Docs/IMPLEMENTATIONS.md` (수정)
  - 설명: `score-logic`를 phase1 진행 중으로 갱신.
  - 이유: 인덱스 동기화.

- 파일: `Docs/INSPECTOR_TOOLTIPS.md` (수정)
  - 설명: `ScoreConfigSO` 필드 Tooltip 표 추가.
  - 이유: 팀 Tooltip 소스 오브 트루스.

**메모** — Bootstrap·이벤트 미연결. Unity 에디터가 재연결되면 `read_console`로 컴파일 재확인.
---
## 2 — 2026-07-15 · IScoreTuning 제거

**바뀐 것** — Domain이 `ScoreConfigSO`를 직접 받도록 단순화. `IScoreTuning` 삭제.

- 파일: `Scripts/Domain/Score/IScoreTuning.cs` (삭제)
  - 심볼: `IScoreTuning` / `GetK` / `GetStreakMultiplier` — 인터페이스·메서드 (삭제)
    - 설명: Domain–SO 분리용 계약을 제거한다.
    - 이유: SO가 JTH 소유·동일 asmdef라 인터페이스로 결합을 막을 실익이 없음.

- 파일: `Scripts/Domain/Score/ScoreCalculator.cs`
  - 심볼: `ScoreCalculator._config` — 필드 (수정)
    - 설명: `IScoreTuning` → `ScoreConfigSO`로 타입 변경.
    - 이유: 튜닝 구현체가 SO 하나뿐.
  - 심볼: `ScoreCalculator.ScoreCalculator(ScoreConfigSO)` — 생성자 (수정)
    - 설명: `ScoreConfigSO`를 받아 `_config`에 보관. null이면 예외.
    - 이유: 계산기가 같은 JTH SO를 직접 읽게.
  - 심볼: `ScoreCalculator.ComputeWaveScore` — 메서드 (수정)
    - 설명: `_config.GetK` / `GetStreakMultiplier` 호출로 변경 (동작 동일).
    - 이유: 인터페이스 경유 제거.

- 파일: `Scripts/Domain/Score/ScoreSession.cs`
  - 심볼: `ScoreSession.ScoreSession(ScoreConfigSO)` — 생성자 (수정)
    - 설명: 인자를 `ScoreConfigSO`로 받고 `ScoreCalculator`에 전달.
    - 이유: 세션 생성 API를 SO 직접 주입으로 맞춤.

- 파일: `Scripts/Data/ScoreConfigSO.cs`
  - 심볼: `ScoreConfigSO` — 클래스 (수정)
    - 설명: `IScoreTuning` 구현 선언 제거. `GetK`/`GetStreakMultiplier`는 public 메서드로 유지.
    - 이유: 인터페이스 없이 Domain이 SO 메서드를 직접 호출.

- 파일: `Docs/Implementations/score-logic/phase1.md` (수정)
  - 설명: 완료 기준·클래스 표에서 `IScoreTuning` 문구 삭제.
  - 이유: 계획 문서와 코드 일치.

**메모** — 공식·기본 상수 변경 없음.
---
