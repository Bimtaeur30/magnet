# Phase 6 — Pressure(유일수) + Orchestrator + UniqueSolution 보관

> **구현:** `block-selection-algorithm` · **Sequence:** [sequence6.md](sequence6.md) · **스펙:** [SPEC.md](SPEC.md) §9·§11·§16.3·부록 A

## 목표

의도적 유일수 생성(유일해 보관 + 정답 매칭 API 포함)과, 티어 스택 전체를 순서대로 시도하는 Orchestrator. 어떤 경우에도 3피스를 반환한다.

## 완료 조건

- [x] `SolutionStep` / `UniqueSolution` — 유일해 스텝(슬롯·피벗·offsets·클리어 수) + `MatchesStep` (엄지척 UI 판정 데이터)
- [x] `PlacementSolver.TryFindUniqueFullSequence` — cap=2 카운트 중 첫 완주 시퀀스 기록 (재탐색 없음)
- [x] `PressureGenerator` — 유일수 + 난이도 하한, 최고 난이도 후보 선택
- [x] `FallbackGenerator` — 1차 통과 가능 조합, 2차 hasAny만
- [x] `SelectionTier` / `BlockSelectionResult` / `BlockSelectionOrchestrator` — 스택 0~7 + 최후 Normal 강제
- [x] Pressure·Fallback 튜닝 필드 8종
- [x] 검증: 강제 유일해 보드(4×4) → count=1, 스텝 replay 일치(recordedClears==replayClears), MatchesStep 정·오답 판정

## 설계 결정

| 결정 | 이유 |
|------|------|
| 유일해는 카운트 탐색 중 **기록기(SequenceRecorder)** 로 확보 | count==1 확인 후 재탐색하면 같은 DFS 2회 — 첫 완주 시 스택 복사로 1회에 해결 |
| 난이도 = bigFinish(마지막 스텝 ≥5칸) + setupClear(앞 스텝 클리어 필요) 2항 | 스펙 §11.2의 tight-pivot 항은 대안 피벗 계산 비용 대비 효과 불명 — 튜닝 SO 가중치로 2항부터 시작 |
| `MatchesStep`은 슬롯 index + 피벗 일치로 판정 | 동일 모양 피스가 다른 슬롯에 있으면 이론상 오답 처리될 수 있으나(솔버 dedup) 희귀 — 단순성 우선 |
| Pressure blame 임계(`BlamePressureThreshold`)는 게이트에 미사용 | 스펙 부록 A 게이트가 zone/health + 확률만 사용 — §13.3 "가중"의 구체 규칙 미정의라 보류 |
| Trap 게이트 통과 시 Pressure 제외(§9.5)는 미구현 | 부록 A 의사코드에 없음 — Trap이 번들을 못 찾고 내려온 경우도 Pressure 시도 허용 |
| 최후 fallback: Normal 번들 hasAny → 그래도 없으면 가중 샘플 강제 반환 | Death 없음 원칙(§3.1)보다 "무조건 3피스 반환"이 우선 — 게임오버 판정은 TurnService 몫 |

## 만진 파일

- `Scripts/Domain/BlockSelection/Solution/SolutionStep.cs` (신규)
- `Scripts/Domain/BlockSelection/Solution/UniqueSolution.cs` (신규)
- `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs` (수정 — 기록기 + `TryFindUniqueFullSequence`)
- `Scripts/Domain/BlockSelection/Generation/PressureGenerator.cs` (신규)
- `Scripts/Domain/BlockSelection/Generation/FallbackGenerator.cs` (신규)
- `Scripts/Domain/BlockSelection/SelectionTier.cs` (신규)
- `Scripts/Domain/BlockSelection/BlockSelectionResult.cs` (신규)
- `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs` (신규)
- `Scripts/Data/BlockSelectionTuningSO.cs` (수정 — Pressure 7필드 + Fallback 1필드)

## 범위 밖

Drawer·Bootstrap 연동(Phase 7), brilliant escape UI(범위 밖 — `IsBrilliantEscapeCandidate` 데이터만)
