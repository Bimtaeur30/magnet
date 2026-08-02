# Area 점수 튜닝 단계

## 확정 (2026-08-02 Round3)

선호: **R3-4 ≫ R3-3 ≫ R3-1 > R3-2** (점수 11 / 8 / 5 / 4)

| 항목 | 값 | 근거 |
|------|-----|------|
| `rectCountPenalty` (k) | **4** | Phase9 이후 살짝 상향 (3→4). 5는 Round3 비선호 |
| `areaCountPenalty` | **4** | 4-연결 Area(찬+빈) 개수 패널티 — 영역 적을수록 Total↑ |
| `emptyTinyPenalty` | **−15** | R3-4 구멍 관대 |
| `filledTinyPenalty` | **−8** | R3-4 |
| 나머지 base | 블렌드 3:2:1 | emptyFull 107 · filledFull 67 · side 14/5 … |
| `uniqueAreaThreshold` | **−15** | −25는 Unique 부족 → 약간 완화 |
| `uniqueProbability` | **0.45** | 0.35→조금 더 |

**점수:** `base − 4×rectCount − 4×areaCount`

## Clear Priority (Phase 9)

| 항목 | 값 | 근거 |
|------|-----|------|
| `allClearProbability` | **0.75** | grill — 올클 가능 시 지급 변덕 |
| `allClearCooldownTurns` | **1** | 무한 올클 방지 (+ 빈 보드 스킵) |
| `multiClearHardMinLines` | **6** | 6줄 미만 Clear Priority 무시 → Area |
| ~~`multiClearSoftMinLines`~~ | — | **삭제** (Phase 13) |
| ~~`multiClearSoftProbability`~~ | — | **삭제** (Phase 13) |
| `outcomeBeamWidth` | **4** | 클리어·올클 빔 추정 |

---

## Round 3 기록

| R3 | 설정 | 점수 |
|---:|------|-----:|
| 1 | k=5 블렌드 | 5 |
| 2 | k=7 블렌드 | 4 |
| 3 | k=3 구멍 가혹 (−50/−25) | 8 |
| **4** | k=3 구멍 관대 (−15/−8) | **11** |

---

## size/변 블렌드 (확정에 포함)

| 필드 | 값 |
|------|---:|
| emptyFullScore | 107 |
| filledFullScore | 67 |
| sideBonusAtIdeal | 14 |
| sideBonusPerTwoSides | 5 |
| emptyTinyMaxSize | 3 |
| filledTinyMaxSize | 2 |
| sideBonusIdealMax | 4 |
