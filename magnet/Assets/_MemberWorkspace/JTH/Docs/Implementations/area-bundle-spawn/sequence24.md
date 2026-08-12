# sequence24 — Phase 24 변경 기록

## 1 — 2026-08-10 · Death Area 패널티

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `deathAreaPenalty` — 필드 (추가)
    - 설명: Normal/Easy effective에서 death당 뺄 점수 k. 기본 8.
    - 이유: grill B안 — 덜 위험한 손을 Area와 같은 점수축에서 비교.
  - 심볼: `DeathAreaPenalty` — 프로퍼티 (추가)
    - 설명: k 노출. 음수는 0.
    - 이유: Orchestrator가 SO에서 읽게.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `ScoredCandidate.DeathCount` — 프로퍼티/생성자 (추가)
    - 설명: 후보별 CountDeaths 보관.
    - 이유: 우승 판정·로그·SelectionResult에 전달.
  - 심볼: `EffectiveAreaScore` — 메서드 (추가)
    - 설명: `predicted × mean(w) − k × death`.
    - 이유: Normal/Easy 공통 점수식.
  - 심볼: `ScoreSurvivors` — 메서드 (수정)
    - 설명: 후보마다 `CountDeaths` 계산해 ScoredCandidate에 넣음.
    - 이유: Normal Area 우승에 death 반영.
  - 심볼: `PickMaxEffectiveArea` — 메서드 (수정)
    - 설명: EffectiveAreaScore 최댓값 선택.
    - 이유: shapeWeight만 쓰던 Phase23을 death까지 확장.
  - 심볼: `TrySelectByMaxArea` — 메서드 (수정)
    - 설명: Easy도 동일 effective, result.DeathCount 채움.
    - 이유: Easy 폴백도 함정 손 억제.
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: reason에 `-k×d` 표기.
    - 이유: 로그로 death 영향 확인.
  - 심볼: `ToResult` — 메서드 (수정)
    - 설명: deathCount=0 고정 제거 → pick.DeathCount 전달.
    - 이유: Bootstrap 로그·결과 정합.

## 4 — 2026-08-10 · Death API·필드 삭제

- 삭제: `Scripts/Domain/AreaBundleSpawn/AreaBundleMetrics.cs`
  - 심볼: `CountDeaths` / `CountDeathsRecursive` / `RemainingPieces` — 메서드 (삭제)
    - 설명: death 전수 탐색 API 제거.
    - 이유: 미사용·부하 큰 dead code.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleSelectionResult.cs`
  - 심볼: `DeathCount` / 생성자 `deathCount` — (삭제)
    - 설명: 결과에서 death 필드 제거.
    - 이유: 항상 0 스텁만 남던 필드.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleSelectionResult` 생성 호출 — (수정)
    - 설명: `deathCount` 인자 전부 제거.
    - 이유: 생성자 시그니처 변경.
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `LogSelection` — 메서드 (수정)
    - 설명: 로그에서 `death=` 제거.
    - 이유: 필드 삭제에 맞춤.

## 5 — 2026-08-10 · CountSequences 핫패스 제거

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `ScoreSurvivors` / `TrySelectHospitality` / `TrySelectAllClearExact` / `TrySelectByMaxArea` — 메서드 (수정)
    - 설명: 후보 루프에서 `CountSequences` 제거. 우승 손에만 1회 계산.
    - 이유: 선택에 안 쓰는 전수 시퀀스 카운트가 렉 유발. 접대 제외↑으로 Normal 경로가 더 자주 탐.
  - 심볼: `ToResult` — 메서드 (수정)
    - 설명: board 받아 우승 손 seq만 계산.
    - 이유: 로그용 SequenceCount 유지·비용 최소화.

## 8 — 2026-08-10 · Death 분모 표시

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleMetrics.cs`
  - 심볼: `CountDeathPercent` — 메서드 (수정)
    - 설명: `out int branches` 분모 반환.
    - 이유: 로그에 분모 표기.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleSelectionResult.cs`
  - 심볼: `DeathBranches` — 프로퍼티 (추가)
    - 설명: 검사한 중간 갈래 수.
    - 이유: 분모 보관.
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `LogSelection` — 메서드 (수정)
    - 설명: `death=42%/123` 형태.
    - 이유: 퍼센트와 분모 동시 표시.

## 9 — 2026-08-10 · Death 디버그 제거

- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `LogSelection` — 메서드 (수정)
    - 설명: 빨간 `death=` 로그 제거.
    - 이유: 디버그 표시 불필요.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleSelectionResult.cs`
  - 심볼: `DeathPercent` / `DeathBranches` — 프로퍼티 (삭제)
    - 설명: 결과에서 Death 필드 제거.
    - 이유: 로그 전용 필드였음. 배제는 Orchestrator 내부만.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: Unique/접대/올클/Easy킬 결과 생성 — (수정)
    - 설명: 표시용 `CountDeathPercent` 호출 제거.
    - 이유: 불필요 비용·디버그 제거.
  - 심볼: `PickAreaWithDeathReject` / `ToResult` — 메서드 (수정)
    - 설명: reason의 deathSkip/Budget/Fallback 표기 제거. 결과로 death 미전달.
    - 이유: 선택 배제만 유지.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleMetrics.cs`
  - 심볼: `CountDeathPercent` 무제한 오버로드 — 메서드 (삭제)
    - 설명: 로그용 오버로드 제거. 예산 있는 API만 유지.
    - 이유: 배제 경로만 사용.
