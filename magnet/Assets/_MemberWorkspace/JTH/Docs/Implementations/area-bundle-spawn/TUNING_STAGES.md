# Area 점수 튜닝 단계

## 확정 (2026-08-02 Round3)

선호: **R3-4 ≫ R3-3 ≫ R3-1 > R3-2** (점수 11 / 8 / 5 / 4)

| 항목 | 값 | 근거 |
|------|-----|------|
| `rectCountPenalty` (k) | **4** | Phase9 이후 살짝 상향 (3→4). 5는 Round3 비선호 |
| `areaCountPenalty` | **4** | 4-연결 Area(찬+빈) 개수 패널티 — 영역 적을수록 Total↑ |
| `emptyTinyPenalty` | **−15** | R3-4 구멍 관대 |
| `filledTinyPenalty` | **−8** | R3-4 |
| 나머지 base | 블렌드 3:2:1 | emptyFull 107 · filledFull 67 … |
| `uniqueAreaThreshold` | **−15** | −25는 Unique 부족 → 약간 완화 |
| `uniqueProbability` | **0.45** | 0.35→조금 더 |

**점수:** `base − 4×rectCount − 4×areaCount`  
~~변(side) 보너스~~ — Phase17에서 제거 (직사각 개수와 역할 중복)

## Clear Priority (Phase 9)

| 항목 | 값 | 근거 |
|------|-----|------|
| `allClearProbability` | **0.75** | grill — Exact 통과 시 지급 변덕 |
| `allClearCooldownTurns` | **1** | 무한 올클 방지 (+ 빈 보드 스킵) |
| `allClearMaxOccupied` | **16** | 이 점유 이하에서만 올클 고정 풀 Exact |
| `allClearBundles` | **12** | Blocks2 대형·고빈도 핸드 고정 |
| ~~`multiClearHardMinLines`~~ | — | **삭제** (Phase 20 → Hospitality) |
| `hospitalityContourMinFill` | **0.7** | 구멍 8이웃 윤곽 채움 하한 |
| `hospitalityProbability` | **0.35** | 접대 후보 있을 때 지급 확률 |
| `outcomeBeamWidth` | **4** | Normal Area용 클리어 추정 (올클·Hospitality Exact는 미사용) |

---

## Round 3 기록

| R3 | 설정 | 점수 |
|---:|------|-----:|
| 1 | k=5 블렌드 | 5 |
| 2 | k=7 블렌드 | 4 |
| 3 | k=3 구멍 가혹 (−50/−25) | 8 |
| **4** | k=3 구멍 관대 (−15/−8) | **11** |

---

## size 블렌드 (확정에 포함)

| 필드 | 값 |
|------|---:|
| emptyFullScore | 107 |
| filledFullScore | 67 |
| emptyTinyMaxSize | 3 |
| filledTinyMaxSize | 2 |

~~sideBonus*~~ — Phase17 제거
