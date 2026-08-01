# Phase 2 — Relife·Trap·ComboBreak 실시간 전환

## 목표

번들 SO 기반이던 세 티어를 "42-ID 풀 샘플링 + 솔버 필터" 실시간 생성으로 전환해 5개 특수 티어를 완성한다.

## 구현 내용

- `HybridConstraintGenerator` — 세 티어 공용 골격: 풀에서 트리플 샘플 → `BundleTierSelector.IsValid`(기존 검증 재사용) 통과 첫 조합 반환, 예산 소진 시 null(fallthrough).
  - **Relife:** `Passable` (HasAny + FullSequence). 풀 = {1} ∪ fillPool — **1x1(ID 1)은 이 풀에서만**.
  - **Trap:** `Trap` (HasAny + ¬FullSequence). 대형 위주 가중.
  - **ComboBreak:** `ComboBreak` (FullSequence + ¬ComboMaintainable). 소·중형 위주 가중.
- 생성 트리플은 오케스트레이터에서 히스토리 기록(트레이트 우회) + 직전 트리플 회피 입력 갱신 — Phase 1 골격에 이미 배선됨.

## 검증 (에디터 execute_code, 합성 보드)

- Hospitality: near-line 4개 보드에서 10/10 생성 (opportunity 1.0)
- Trap: TooDirty 보드에서 8/10 생성
- Pressure: 불규칙 보드 500샘플 중 유일해 2건 (0.4%) — 의도된 희귀성, 구 시스템과 동일 기준
- 플레이 모드: round 1 → 7, round 2+ → 1370 근사, 턴 정산(GoodTurn·blame) 로그 정상

## 알려진 특성

- **ComboBreak 검증 비용:** TooEmpty 보드에서 "콤보 불가 증명"은 탐색 전수라 히치성(에디터 기준 ~300ms). 구 번들 시스템과 동일 특성 + 발동 극희귀(TooEmpty + blame≥25 + 4%)라 유지. 문제 시 `ComboBreakSampleTries` 하향.
- **Pressure 발동률:** 게이트 통과 후에도 생성 성공률이 낮음(유일해 0.4%/샘플 × 40샘플 ≈ 15%). 체감 빈도를 올리려면 `PressureSampleCount` 상향(비용 주의).

## 코드·에셋 맵

| 경로 | 역할 |
|------|------|
| `Scripts/Domain/HybridSpawn/HybridConstraintGenerator.cs` | Relife·Trap·ComboBreak 공용 생성기 (신규) |
| `Scripts/Data/HybridTuningSO.cs` | 세 티어 게이트·샘플 예산·칸 수 가중표 (Phase 1에 포함) |
