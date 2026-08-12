# Area 점수 튜닝 단계

## 확정 (2026-08-02 Round3)

선호: **R3-4 ≫ R3-3 ≫ R3-1 > R3-2** (점수 11 / 8 / 5 / 4)

| 항목 | 값 | 근거 |
|------|-----|------|
| `cornerRectPenalty` | **0.6** (평가 확정 후보) | 네 모서리 덮개 직사각 최소면적 × 이 값. Stage 후보였던 1=0.5 · 2=1 · 3=2 · 4=4 |
| `areaCountPenalty` | **8** | Area 개수 패널티(찬=4연결+깊은홈절단·빈=4연결) — 영역 적을수록 Total↑ |
| `emptyTinyPenalty` | **−15** | R3-4 구멍 관대 |
| `filledTinyPenalty` | **−12** | tiny 찬 Area (−8→−12) |
| 나머지 base | **filledFull=20** | emptyFull 107 · filledFull **20** · tiny −12 |
| `uniqueAreaThreshold` | **−15** | −25는 Unique 부족 → 약간 완화 |
| `uniqueProbability` | **0.45** | 0.35→조금 더 |

**점수:** `base − cornerRectPenalty×minCornerCoverArea − 8×areaCount`  
~~greedy rectCount~~ — Phase34에서 제거  

## CornerRect Round (평가 중)

에이전트에게 **1 / 2 / 3 / 4** 라고 하면 `DefaultAreaBundlePool.cornerRectPenalty`를 아래 표 값으로 바꾼다. 각 단계 플레이 후 점수·메모 기입.

| Stage | `cornerRectPenalty` | 체감 | 점수 | 메모 |
|------:|--------------------:|------|-----:|------|
| 1 | **0.5** | 약함 | | |
| 2 | **1** | 기본 | | |
| 3 | **2** | 중간 | | |
| 4 | **4** | 강함 | | |
| **(현재)** | **0.6** | Stage1~2 사이 확정 후보 | | |

선호 순: _(플레이 후 기입)_

## Clear Priority (Phase 9)

| 항목 | 값 | 근거 |
|------|-----|------|
| `allClearProbability` | **0.75** | grill — Exact 통과 시 지급 변덕 |
| `allClearCooldownTurns` | **1** | 무한 올클 방지 (+ 빈 보드 스킵) |
| `allClearMaxOccupied` | **16** | 이 점유 이하에서만 올클 고정 풀 Exact (12→16 복구) |
| `allClearBundles` | **11** | Blocks2 대형·고빈도 핸드 고정 (ac09 제거 유지) |
| ~~`multiClearHardMinLines`~~ | — | **삭제** (Phase 20 → Hospitality) |
| `hospitalityContourMinFill` | **0.35** | 구멍 8이웃 윤곽 채움 하한 (0.5→추가 하향) |
| `hospitalityProbability` | **0.35** | 접대 후보 있을 때 지급 확률 |
| `hospitalityThreeCellProbability` | **0.5** | 접대 확정 후 핏이 3칸뿐이면 추가 통과 확률 |
| `outcomeBeamWidth` | **4** | Normal/Easy/Hospitality Area·클리어 추정 |
| `maxAreaRefineTopK` | **4** | 빔 Area 근사 후 MaxArea 정밀화 상위 수 (0=없음) |
| `shapeWeights[1..42]` | **대부분 1** | Normal/Easy: `predictedArea × mean(w)`. 접대·올클 미적용 |
| └ L3 `6,15,27,28` | Main **하드밴+w0** · 그 외 허용 | Unique w=5.5 · 접대 Fit 허용 · Clean/Easy/올클 허용 |
| └ L4·L5 ㄱ `8,12,21,23,24,29–34,42` | **0.08** + 접대 제외 | 큰 ㄱ 추가하향 철회(0.03→0.08) |
| └ I2 `2,3` | **0.08** | 1×2 추가 하향 |
| └ I3 `4,5` | **0.35** | 1×3·3×1 과다 지속 → 추가 하향 |
| └ 2×2 `9` | **0.45** | 재조정 |
| └ 3×3 `13` | **0.15** | 과다 → 0.45에서 추가 하향 (접대 이미 제외) |
| └ 2×3 `35,36` | **0.35** | 2×3·3×2 재조정 |
| └ I4 `7,17` | **0.2** + 접대 제외 | 1×4 과다 |
| └ I5 `11,22` | **0.2** | 1×5 과다 → 0.5에서 추가 하향 (접대 제외 안 함) |
| `deathRejectPercent` | **30** | 예산 내 완주 시 Death% 초과면 배제 |
| `deathRejectMaxTries` | **8** | 상위부터 시도, 전부 배제 시 1등 |
| `deathBranchBudget` | **48** | 분모 초과 시 검사 중단·통과 |
| `survivalAreaMax` | **-15** | ≤이면 Main 가중, >이면 Clean 가중 (0→−15 · Main 감소) |
| `cleanChainProbability` | **0.4** | Clean 지급 후 최적 보드 다음 패 예약 |
| `shapeWeights` (Main) | **생존 튜닝** | I2/I3/I5·사각·ㄱ 등 (기존) |
| `cleanShapeWeights` | **전부 1** | Clean Normal Area (작은 ㄱ 포함) |
| `uniqueShapeWeights` | **4칸 중심** · 1~2칸↓ · 5칸+≈0 · 작은 ㄱ=5.5 | Death%↑·해 경로↓ (소형=다해 / 대형=뻔함 회피) |

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
| filledFullScore | 0 |
| emptyTinyMaxSize | 3 |
| filledTinyMaxSize | 2 |

~~sideBonus*~~ — Phase17 제거
