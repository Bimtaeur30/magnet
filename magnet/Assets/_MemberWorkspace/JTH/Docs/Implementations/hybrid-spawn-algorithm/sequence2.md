# sequence2 — Phase 2 변경 기록

## 1 — 2026-08-02 · Relife·Trap·ComboBreak 실시간 전환

**바뀐 것**

- 생성: `Scripts/Domain/HybridSpawn/HybridConstraintGenerator.cs` — 세 티어 공용 "샘플 + 솔버 필터" 골격 (`BundleTierSelector.IsValid` 재사용)
- `HybridSpawnOrchestrator`에 세 티어 게이트·풀 배선 (Phase 1과 동일 커밋 단위로 진행)

**메모**

- 번들 SO 체계는 승계하지 않음 (`BlockBundleSO`·`BlockBundlePoolSO`·기존 번들 에셋은 구 코드와 함께 보존만).
- 검증: 합성 보드에서 Hospitality 10/10 · Trap 8/10 · Pressure 유일해 0.4%(의도된 희귀), 플레이 모드에서 체인 7→1370 승격·턴 정산 로그 확인.
- 알려진 특성: ComboBreak 콤보 불가 증명 비용(~300ms, 극희귀 발동) · Pressure 생성 성공률 ≈ 15%/발동 — phase2.md 참고.
