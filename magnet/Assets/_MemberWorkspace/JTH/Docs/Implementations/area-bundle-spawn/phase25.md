# Phase 25 — Normal/Easy Death 배제 (예산 캡)

## 목표

Normal·Easy Area 선택에서 Death%가 높은 손을 배제하되, 분모(검사 갈래)가 큰 후보는 검사하지 않고 통과시켜 렉을 막는다.

## 규칙

1. effective Area 내림차순 정렬
2. 상위부터 최대 `deathRejectMaxTries`(기본 **5**) 검사
3. `death% > deathRejectPercent`(기본 **50**) **이고** 분모 ≤ `deathBranchBudget`(기본 **32**)로 계산이 끝난 경우만 배제
4. 분모가 예산 초과 → 검사 중단 → **통과(허용)**
5. 시도 전부 배제 → **원래 1등** 채택
6. **미적용:** 접대 · 올클 Exact · Unique · Easy 킬 폴백(가중랜덤)

## SO

| 필드 | 기본 |
|------|------|
| `deathRejectPercent` | 30 |
| `deathRejectMaxTries` | 8 |
| `deathBranchBudget` | 48 (0=무제한) |
