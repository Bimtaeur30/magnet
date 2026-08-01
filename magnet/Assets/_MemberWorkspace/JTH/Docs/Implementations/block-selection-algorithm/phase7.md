# Phase 7 — Drawer 연동·GoodTurn/정답 매칭 데이터·로그

> **구현:** `block-selection-algorithm` · **Sequence:** [sequence7.md](sequence7.md) · **스펙:** [SPEC.md](SPEC.md) §16·§20

## 목표

Orchestrator를 실제 스폰 파이프라인(`BlockSupply` → `AbstractDrawer`)에 연결하고, 턴 종료 blame 정산 + 매 리필 진단 로그를 붙인다.

## 완료 조건

- [x] `BlockSelectionDrawer : AbstractDrawer` — `RandomDrawer` 대체, `LastResult` 노출
- [x] `BlockSpawnContext` 확장 — `Health`, `BlameTotal`, `IsRetrySession`, `TurnIndex` (Score는 알고리즘 미사용)
- [x] `BlockSpawnBootstrap` — 튜닝·번들 SO 필드, 프로브 피스(1x1 제외 16종), `BlameTracker` 턴 정산, `[BlockSelect]` 로그, `LastSelection`/`LastTurnFeedback` 노출
- [x] 씬(`New_02_Main`) `BlockSpawnBootstrap` 2개 인스턴스에 SO 연결 + 저장
- [x] 플레이 모드 확인: `[BlockSelect] turn=0 zone=TooEmpty health=0.60 blame=0.0 tier=Normal bundle=normal_zigzag`

## 설계 결정

| 결정 | 이유 |
|------|------|
| 컨텍스트에 `BlameTracker` 객체 대신 값(`BlameTotal` 등)만 전달 | Drawer(Domain)가 필요한 건 blame 수치뿐 — 상태 객체는 Bootstrap이 소유 |
| 턴 정산은 `Fill()` 진입 시점 (직전 Fill의 보드 스냅샷과 비교) | Fill은 lastDrop에만 호출 → "3피스 라운드 종료" 시점과 정확히 일치, `allPiecesPlaced=true` 보장 |
| health 계산은 리필당 1회, blame 정산과 선택기 입력에 공유 | `BoardHealthCalculator.Compute`는 프로브 16종 순회 — 중복 계산 방지 |
| `IsRetrySession`은 일단 상수 false | 재시작이 씬 리로드(PMS `RestartGame`) 방식이라 크로스-씬 상태 필요 — 세션 상태 전달 수단 확정 전까지 Relife 게이트 닫힘 |
| 로그·데이터 노출은 Bootstrap, Domain은 판정만 | 팀 규칙 Domain 순수성 — 이벤트 발행도 이후 UI 작업에서 Bootstrap이 담당 |

## 만진 파일

- `Scripts/Domain/Spawn/BlockSelectionDrawer.cs` (신규)
- `Scripts/Domain/Spawn/BlockSpawnContext.cs` (수정)
- `Scripts/Bootstrap/BlockSpawnBootstrap.cs` (수정)
- `Docs/INSPECTOR_TOOLTIPS.md` (수정 — Phase 3~7 신규 필드 표)
- 씬 `New_02_Main` (SO 참조 연결)

## 남은 일 (다음 작업)

- `IsRetrySession` 연동 (재시작 감지 → Relife 게이트 개방)
- GoodTurn / 정답 매칭(`UniqueSolution.MatchesStep`) 이벤트 발행 — UI 담당과 계약 후
