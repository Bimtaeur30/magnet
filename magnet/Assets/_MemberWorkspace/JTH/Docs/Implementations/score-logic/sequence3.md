## 1 — 2026-07-15 · 턴 콤보 리셋 + GameOver·스킨 실점수

**바뀐 것** — `TurnEnded`에서 콤보 리셋, GameOver/스킨 해금 체크에 세션 총점 전달. `DESIGN.md` 점수·턴 문구 동기화. Domain 공식 변경 없음.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.OnEnable` — 메서드 (추가)
    - 설명: `TurnEndedEvent` 리스너를 등록한다.
    - 이유: 핸드 소진 시점에 점수 세션의 턴 종료를 알리기 위함.
  - 심볼: `BoardPlacementBootstrap.OnDisable` — 메서드 (추가)
    - 설명: `TurnEndedEvent` 리스너를 제거한다.
    - 이유: EventChannel 구독 누수 방지.
  - 심볼: `BoardPlacementBootstrap.OnTurnEnded` — 메서드 (추가)
    - 설명: `_scoreSession.NotifyTurnEnded()`를 호출한다.
    - 이유: 턴 중 클리어가 없으면 콤보 0 (합의 규칙).
    - 영향: `BlockSpawnBootstrap.Consume`이 올리는 `TurnEndedEvent`를 소비.
  - 심볼: `BoardPlacementBootstrap.RaiseScoreSkinUnlockCheck` — 메서드 (추가)
    - 설명: `SkinUnlockCheckEvent.Init(Score, totalScore)`를 `magnetGameChannel`에 Raise한다.
    - 이유: 스킨 해금 임계값을 세션 총점으로 판정 (0 하드코딩 제거).
  - 심볼: `BoardPlacementBootstrap.TryConfirmPlacement` — 메서드 (수정)
    - 설명: `ScoreChanged` 직후 `RaiseScoreSkinUnlockCheck(TotalScore)`. 경계 GameOver 시 동일 체크 후 `GameOverEvent.Init(TotalScore)`.
    - 이유: 중도 해금 + 베스트/`SubmitScore`에 실점수 전달 (Phase 2의 0 제거).

- 파일: `Docs/DESIGN.md`
  - 심볼: §3 턴 메모 — 문구 (수정)
    - 설명: `TurnEnded` 시 콤보 리셋이 점수에 연결된다고 명시.
    - 이유: “플레이 수치 미연결” 문구가 Phase 3와 불일치.
  - 심볼: §4.7 점수 — 섹션 (수정)
    - 설명: BB식 웨이브 점수·턴 콤보 리셋·이벤트·풀클 보너스 없음으로 갱신.
    - 이유: SCRUM-23 구현과 설계 문서 동기화.
  - 심볼: §4.8 해금 행 — 문구 (수정)
    - 설명: 세션 누적 점수 + `SkinUnlockCheck`(Score)로 명시.
    - 이유: 베스트와 세션 점수를 혼동하지 않게.
  - 심볼: §3 루프 표 5행 — 문구 (수정)
    - 설명: “점수 = 파괴 테두리 칸” → §4.7 참조.
    - 이유: 옛 칸수 비례 표현 제거.

- 파일: `Docs/Implementations/score-logic/phase3.md` · `sequence3.md` · `phases.md` (추가·수정)
  - 설명: Phase 3 계획·변경 기록·인덱스.
  - 이유: 구현 기록 규칙.

- 파일: `Docs/IMPLEMENTATIONS.md` (수정)
  - 설명: `score-logic`를 phase3 상태로 갱신.
  - 이유: 인덱스 동기화.

**메모** — 후보 전부 배치 불가 GO는 SCRUM-22. `skinChannel` 필드는 기존과 같이 미사용(Raise는 `magnetGameChannel`).
---

## 2 — 2026-07-17 · 클리어(폭발) 점수 k ≈ 2/3

**바뀐 것** — 일반 배치 대비 폭발 점수 갭이 커서 `ScoreConfigSO` 기본 k를 약 2/3(1/3 하향)로 조정. 공식·콤보·streak는 동일.

- 파일: `Scripts/Data/ScoreConfigSO.cs`
  - 심볼: `ScoreConfigSO.kTiers` — 필드 기본값 (수정)
    - 설명: k `80/66/45/36` → `53.33/44/30/24`.
    - 이유: 웨이브 점수 `round(k×combo×squareSize×streak)`만 약 1/3 낮춰 배치 칸 점수와의 체감 차이를 줄임.
    - 영향: `ScoreCalculator.ComputeWaveScore` → `GetK`.

- 파일: `ScriptableObjects/DefaultScoreConfig.asset`
  - 심볼: `kTiers[].k` — 에셋 값 (수정)
    - 설명: 런타임에 쓰는 기본 에셋 k를 위와 동일하게 맞춤.
    - 이유: 코드 기본값만 바꾸면 기존 에셋은 이전 k를 유지함.

- 파일: `Docs/Implementations/score-logic/phase1.md`
  - 심볼: k 기본값 표 (수정)
    - 설명: 문서상 기본 k를 신규 값으로 동기화.
    - 이유: Phase 1 합의 상수 표가 실제 튜닝과 어긋나지 않게.

**메모** — 배치 무클리어 `+cellsPlaced`는 변경 없음. 초기에 1/3로 잘못 넣었다가 2/3 유지로 정정.
---
