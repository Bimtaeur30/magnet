# score-logic Sequence 6

> Phase 5 이후 핫픽스 · 콤보 0 이벤트 생략 + GameOver 총점

## 1 — 2026-08-19 · 콤보 0 미발행 · GameOver 점수

## 변경 상세

- 파일: `Scripts/Bootstrap/TurnBootstrap.cs`
  - 심볼: `TurnBootstrap.RaiseComboChangedIfNeeded(...)` — 메서드 (수정)
    - 설명: `comboAfter == 0`이면 Raise하지 않는다. 값이 안 바뀐 경우도 기존처럼 생략한다.
    - 이유: 콤보 0은 UI에 띄울 값이 아니라서, 체인 끊김 때 "0" 팝업이 나가지 않게.
    - 영향: `ComboUIView` / `Combo_UI`가 `ComboChangedEvent`만 구독.
  - 심볼: `TurnBootstrap.RaiseGameOver()` — 메서드 (수정)
    - 설명: `GameOverEvent.Init`에 `_currentStage` 대신 `_scoreSession.TotalScore`를 넣는다. 스테이지 스킨 해금 체크는 `_currentStage`를 그대로 쓴다.
    - 이유: GameOver UI `ScoreTxt`가 `FinalStage`를 점수로 표시하고, Relife 거절 경로도 이미 총점을 넣어서. 스테이지를 넣으면 0이나 적 인덱스가 점수처럼 보임.
    - 영향: `GameOverUIView.HandleGameOverEvent`, `SaveBridge.OnGameOver`의 `SubmitStage`.

- 파일: `Docs/DESIGN.md` §4.7 · 이벤트 표 (수정)
  - 심볼: 콤보·게임오버 문구 — 문서 (수정)
    - 설명: `ComboChangedEvent`는 콤보 ≥ 1일 때만, GameOver 페이로드는 세션 총점이라고 적는다.
    - 이유: 런타임 계약과 설계 문구가 어긋나지 않게.
