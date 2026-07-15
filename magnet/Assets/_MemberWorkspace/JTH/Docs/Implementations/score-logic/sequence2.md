## 1 — 2026-07-15 · 배치 경로 ScoreSession 연동 + ScoreChanged

**바뀐 것** — `BoardPlacementBootstrap`에 점수 세션을 붙이고, 재조립 직후 `ApplyPlacement`·`ScoreChanged`·웨이브 `scoreAwarded`를 연결. Domain 공식 변경 없음.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.scoreConfig` — 필드 `ScoreConfigSO` (추가)
    - 설명: 인스펙터에서 `DefaultScoreConfig` 등 튜닝 SO를 받는다.
    - 이유: SO·프로젝트 에셋은 `[SerializeField]` (Inject 금지).
  - 심볼: `BoardPlacementBootstrap._scoreSession` — 필드 (추가)
    - 설명: 런타임 세션 점수·콤보 상태를 보관한다.
    - 이유: 배치마다 Domain `ApplyPlacement`를 같은 세션에 누적하기 위함.
  - 심볼: `BoardPlacementBootstrap.ScoreSession` — 프로퍼티 (추가)
    - 설명: `_scoreSession` 읽기 전용 노출.
    - 이유: Phase 3 턴 리셋·GameOver가 같은 세션을 참조할 수 있게.
  - 심볼: `BoardPlacementBootstrap.Awake` — 메서드 (수정)
    - 설명: `scoreConfig` Assert 후 `new ScoreSession(scoreConfig)`로 세션을 생성한다.
    - 이유: BoardSession과 같이 Bootstrap Awake에서 Domain 세션을 소유.
  - 심볼: `BoardPlacementBootstrap.TryConfirmPlacement` — 메서드 (수정)
    - 설명: `ResolveAllWaves` 직후 `ApplyPlacementScore` → `RaiseReassemblyEvents` → `ScoreChangedEvent` 순으로 Raise한 뒤 재조립 FX를 재생한다.
    - 이유: 웨이브 크기를 알아야 점수를 계산하고, `SquareCleared`에 실제 점수를 넣으려면 Raise 전에 반영해야 함.
    - 영향: KTJ `ScoreUIView`가 `ScoreChangedEvent`로 총점을 갱신.
  - 심볼: `BoardPlacementBootstrap.ApplyPlacementScore` — 메서드 (추가)
    - 설명: 웨이브 없으면 `cellsPlaced`만, 있으면 각 `wave.SquareSize` 목록으로 `_scoreSession.ApplyPlacement`를 호출한다.
    - 이유: Bootstrap이 Clear 결과와 Placement 칸 수를 Domain 점수 API에 맞춘다.
  - 심볼: `BoardPlacementBootstrap.RaiseReassemblyEvents` — 메서드 (수정)
    - 설명: `scoreAwarded` 인자를 `wave.ScoreCells`(파괴 칸 수) 대신 `scoreResult.WaveScores[w]`로 전달한다.
    - 이유: Phase 1에서 준비한 웨이브 점수를 클리어 이벤트 페이로드로 노출.

- 파일: `Scenes/Phase0_Bootstrap.unity`
  - 심볼: `BoardPlacementBootstrap.scoreConfig` — 직렬화 참조 (추가)
    - 설명: `DefaultScoreConfig.asset`을 연결한다.
    - 이유: Play 시 Assert·세션 생성이 가능하도록.

- 파일: `Docs/Implementations/score-logic/phase2.md` · `sequence2.md` · `phases.md` (추가·수정)
  - 설명: Phase 2 계획·변경 기록·인덱스.
  - 이유: 구현 기록 규칙.

- 파일: `Docs/IMPLEMENTATIONS.md` (수정)
  - 설명: `score-logic`를 phase2 상태로 갱신.
  - 이유: 인덱스 동기화.

**메모** — `GameOverEvent`/`SkinUnlockCheck`는 여전히 0 (Phase 3). Unity 에디터 재연결 후 `read_console`로 컴파일 재확인.
---
