# sequence37 — Phase 37 변경 기록

> Phase 계획: [phase37.md](phase37.md)

## 1 — 2026-08-12 · 빔 Area 근사 + MaxArea top-K

**바뀐 것** — Normal/Easy/Hospitality/Clean 체인에서 전 후보 MaxArea DFS를 없애고, 빔 보드 Area로 순위 매긴 뒤 상위 K만 MaxArea한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `maxAreaRefineTopK` — 필드 (추가, 기본 4)
    - 설명: 빔 Area 근사 후 MaxArea로 다시 재는 상위 후보 수. 0이면 정밀화 없음.
    - 이유: 전 후보 MaxArea(16×~15ms)가 Select의 70~90%였음.
  - 심볼: `MaxAreaRefineTopK` — 프로퍼티 (추가)
    - 설명: 음수면 0으로 클램프해 반환.
    - 이유: Orchestrator가 안전하게 읽도록.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `ScoreSurvivors(board, list, profile)` — 메서드 (수정)
    - 설명: 후보마다 OutcomeBeam + `ScoreTotal(FinalBoard)`로 predicted를 넣고, `RefineTopKWithMaxArea` 호출. clear≥1 게이트 유지.
    - 이유: MaxArea 전수 평가 제거가 렉 해결의 본체.
  - 심볼: `RefineTopKWithMaxArea` — 메서드 (추가)
    - 설명: effective Area 상위 K에만 `MaxAreaAfterFullSequence`를 돌려 predicted를 교체.
    - 이유: 정밀 점수는 Death 직전 상위만 필요.
  - 심볼: `TrySelectByMaxArea` — 메서드 (수정)
    - 설명: Easy도 CanSurvive+MaxArea 전수 대신 빔 Area + top-K refine.
    - 이유: Easy 폴백도 같은 병목 가능.
  - 심볼: `TrySelectHospitality` — 메서드 (수정)
    - 설명: 후보 순위는 빔 Area, 우승 1개만 MaxArea refine( K>0일 때).
    - 이유: 접대 스파이크(~158ms)가 MaxArea 전수였음.
  - 심볼: `TryQueueCleanChain` — 메서드 (수정)
    - 설명: afterBest를 MaxArea 보드 대신 현재 패 빔 `FinalBoard`로 잡음.
    - 이유: 체인 예약이 Select에 MaxArea를 한 번 더 붙이던 446ms급 스파이크 제거.
- 파일: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `maxAreaRefineTopK` — 직렬화 값 (추가, 4)
    - 설명: 기본 풀에 top-K=4 기록.
    - 이유: 에디터에서 필드가 0으로 떨어지는 것 방지.
