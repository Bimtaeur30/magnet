# sequence30 — Phase 30 변경 기록

> Phase 계획: [phase30.md](phase30.md)

## 1 — 2026-08-11 · Clean 체이닝 클리어 폴백

**바뀐 것** — 미리 뽑아 둔 Clean 다음 패가 실제 보드에서 라인클리어를 못 하면, 없었던 것처럼 일반 뽑기로 돌린다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator.Select` — 메서드 (수정)
    - 설명: `_queuedCleanChain`을 꺼낸 뒤 `CanLineClearOnBoard`가 true일 때만 바로 반환하고, false면 로그 후 기존 Unique/Normal/Easy 경로로 내려간다.
    - 이유: 플레이어가 최적 수를 안 두면 예약 보드와 달라져 클리어 0 패가 나올 수 있어서, 그때는 현재 보드에 맞는 뽑기가 필요해서.
  - 심볼: `AreaBundleOrchestrator.CanLineClearOnBoard` — 메서드 (추가)
    - 설명: `SequenceOutcomeEstimator.Estimate`로 완주 후보의 `TotalClears ≥ 1`인지 본다.
    - 이유: 올클/접대와 같은 빔 추정으로 “라인클리어 가능”을 판정하기 위해.
    - 영향: Clean 체이닝 지급 게이트에서만 사용.
