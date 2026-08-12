# sequence31 — Phase 31 변경 기록

> Phase 계획: [phase31.md](phase31.md)

## 1 — 2026-08-11 · Normal Area 라인클리어 필수

**바뀐 것** — Normal Area(·Clean 체이닝 후보)는 완주뿐 아니라 라인클리어 ≥1인 패만 고른다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator.ScoreSurvivors` — 메서드 (수정)
    - 설명: `SequenceFound`만 보던 조건을 `TotalClears < 1`이면 제외로 강화한다.
    - 이유: Normal이 클리어 0 패를 주면 콤보·체감이 깨지므로 Area 최대보다 클리어 가능을 우선 게이트로 둔다.
    - 영향: `TrySelectNormalPriority`·`TryQueueCleanChain` 후보 집합.
  - 심볼: `AreaBundleOrchestrator.TrySelectNormalPriority` — 메서드 (수정)
    - 설명: 후보 0일 때 Gate 로그를 `완주+라인클리어≥1`로 바꾼다.
    - 이유: 실패 원인이 생존만이 아니라 클리어 필터임을 로그로 구분하기 위해.
  - 심볼: `AreaBundleOrchestrator.TryQueueCleanChain` — 메서드 (수정)
    - 설명: 이어질 후보 없음 로그에 클리어 조건을 명시한다.
    - 이유: Phase 31 필터와 메시지를 맞추기 위해.

## 2 — 2026-08-11 · 올클 occ 상한 24→12

**바뀐 것** — 올클 Exact 시도를 `occ≤12`일 때만 한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `AreaBundlePoolSO.allClearMaxOccupied` — 필드 (수정)
    - 설명: 기본·Tooltip 권장값을 24→12로 낮춘다.
    - 이유: 올클 시도를 더 비어 있는 보드로만 제한하기 위해.
- 파일: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `allClearMaxOccupied` — 직렬화 (수정)
    - 설명: 런타임 풀 에셋 값을 12로 맞춘다.
    - 이유: SO 기본값만 바꾸면 기존 에셋이 24로 남을 수 있어서.
