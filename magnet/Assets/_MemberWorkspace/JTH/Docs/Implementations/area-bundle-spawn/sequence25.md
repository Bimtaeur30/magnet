# Sequence 25 — Normal/Easy Death 배제

## 1 — 2026-08-10 · Death 배제 + 분모 예산

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `deathRejectPercent` — 필드 (추가)
    - 설명: Death% 배제 임계(기본 50).
    - 이유: 플레이 중 임계 튜닝.
  - 심볼: `deathRejectMaxTries` — 필드 (추가)
    - 설명: 상위 후보 Death 검사 횟수(기본 5).
    - 이유: 전부 배제 시 1등으로 폴백하기 전 시도 한도.
  - 심볼: `deathBranchBudget` — 필드 (추가)
    - 설명: Death 분모 상한(기본 32). 0=무제한.
    - 이유: 분모 큰 손에서 FullSequenceExists 폭발 방지.
  - 심볼: `DeathRejectPercent` / `DeathRejectMaxTries` / `DeathBranchBudget` — 프로퍼티 (추가)
    - 설명: 오케스트레이터 읽기용.
    - 이유: SO 필드 캡슐화.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleMetrics.cs`
  - 심볼: `CountDeathPercent` — 메서드 (수정·오버로드)
    - 설명: `branchBudget`·`budgetExceeded` 지원. budget≤0이면 무제한.
    - 이유: 선택 시 예산 캡, 로그용은 무제한 유지.
  - 심볼: `AccumulateDeaths` — 메서드 (수정)
    - 설명: 분모가 예산을 넘으면 즉시 중단·`budgetExceeded`.
    - 이유: 비싼 갈래를 도중에 끊기.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `PickAreaWithDeathReject` — 메서드 (추가)
    - 설명: effective 상위부터 Death 배제, 실패 시 1등 폴백.
    - 이유: Normal/Easy Area 공통 선택.
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: `PickMaxEffectiveArea` 대신 `PickAreaWithDeathReject`.
    - 이유: Normal Area에 Death 배제 적용.
  - 심볼: `TrySelectByMaxArea` — 메서드 (수정)
    - 설명: 생존 후보 리스트 후 `PickAreaWithDeathReject`.
    - 이유: Easy Area에도 동일 규칙.
  - 심볼: `ToResult` — 메서드 (수정)
    - 설명: 이미 계산된 death%/분모 전달(재계산 없음).
    - 이유: 배제 루프와 로그 공유.
  - 심볼: `PickMaxEffectiveArea` — 메서드 (삭제)
    - 설명: 정렬·선택이 `PickAreaWithDeathReject`로 통합.
    - 이유: 중복 제거.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `deathRejectPercent` / `deathRejectMaxTries` / `deathBranchBudget` — 직렬화 (추가)
    - 설명: 50 / 5 / 32.
    - 이유: 기본 튜닝 반영.
- 수정: `Docs/INSPECTOR_TOOLTIPS.md`
  - 심볼: deathReject* 툴팁 행 — (추가)
    - 설명: Inspector 툴팁 동기화.
    - 이유: 프로젝트 툴팁 규칙.

## 2 — 2026-08-10 · Death 조임 + 올클 occ 완화

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `deathRejectPercent` — 필드 (수정)
    - 설명: `50 → 30`.
    - 이유: Death 배제 강화.
  - 심볼: `deathRejectMaxTries` — 필드 (수정)
    - 설명: `5 → 8`.
    - 이유: 폴백 전 대체 후보 더 탐색.
  - 심볼: `deathBranchBudget` — 필드 (수정)
    - 설명: `32 → 48`.
    - 이유: 조인 임계에 맞춰 검사 완료율↑.
  - 심볼: `allClearMaxOccupied` — 필드 (수정)
    - 설명: `16 → 24`.
    - 이유: occ>16이면 올클 시도 자체가 스킵되어 중반에 거의 안 뜸.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: deathReject* / `allClearMaxOccupied` — 직렬화 (수정)
    - 설명: 30 / 8 / 48 / 24.
    - 이유: SO 기본과 동기화.
