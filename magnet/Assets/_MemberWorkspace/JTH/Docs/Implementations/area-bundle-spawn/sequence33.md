# sequence33 — Phase 33 변경 기록

> Phase 계획: [phase33.md](phase33.md)

## 1 — 2026-08-11 · 올클 상태 가중랜덤 (수정)

**바뀐 것** — 보드가 비어 있으면(올클 상태) Normal 가중 랜덤으로 준다. 올클 패 지급 “다음 손” 플래그 방식은 쓰지 않는다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator._randomAfterAllClear` — 필드 (삭제)
    - 설명: 올클 패 다음 손용 플래그를 제거한다.
    - 이유: 트리거가 “올클 패 이후”가 아니라 “빈 보드 상태”이기 때문.
  - 심볼: `AreaBundleOrchestrator.TrySelectNormalPriority` — 메서드 (수정)
    - 설명: `occupied == 0`이면 올클/접대/Area 전에 `SelectWeightedRandomNormal`을 반환한다.
    - 이유: 올클 상태(빈 보드)에서 Clean Area가 대형 패를 고정적으로 고르지 않게.
  - 심볼: `AreaBundleOrchestrator.SelectWeightedRandomNormal` — 메서드 (추가/수정)
    - 설명: Normal `PickWeighted`로 한 손을 만들어 반환한다. 로그는 `올클 상태 가중랜덤`.
    - 이유: 빈 보드 전용 랜덤 지급 경로.
  - 심볼: `AreaBundleOrchestrator.Select` — 메서드 (수정)
    - 설명: 플래그 분기 없이 기존 Clean 체이닝·cascade만 유지한다.
    - 이유: 랜덤은 Normal priority의 빈 보드 분기에서만 처리.
