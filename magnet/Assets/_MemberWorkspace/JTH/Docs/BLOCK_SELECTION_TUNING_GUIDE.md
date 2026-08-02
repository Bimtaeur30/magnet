# 블록 선택 알고리즘 — 플레이테스트 튜닝 가이드

> **대상 독자:** 알고리즘 구현을 모르는 상태에서 플레이하며 난이도·체감을 조정할 팀원
> **수정할 파일:** `Assets/_MemberWorkspace/JTH/ScriptableObjects/BlockSelection/DefaultBlockSelectionTuning.asset` (인스펙터에서 수정)
> **상세 스펙:** [Implementations/block-selection-algorithm/SPEC.md](Implementations/block-selection-algorithm/SPEC.md)

---

## 1. 이 알고리즘이 뭘 하는가 (1분 요약)

매 턴 하단 3슬롯에 나올 블록은 **순수 랜덤이 아니다.** 알고리즘은 매 리필마다 두 가지를 본다.

- **BoardHealth** — 판이 얼마나 건강한지 (0~1). 구간(zone)으로 나뉨:
  - `TooEmpty` = 판이 너무 비어 있음
  - `Sweet` = 적정
  - `TooDirty` = 판이 너무 차거나 더러움 (죽은 틈 많음)
- **BlameScore** — 최근 몇 턴 동안 **유저가 판을 얼마나 망쳤는지** (죽은 틈 생성, 중앙 막기, 큰 블록 자리 없애기 등). 점수·콤보와는 무관.

이 둘을 보고 아래 **티어 스택**을 위에서부터 시도해, 처음 성공한 티어의 3블록을 준다.

| 순서 | 티어 | 언제 발동 | 유저 체감 |
|:---:|------|-----------|-----------|
| 0 | **Relife** | 재시작 직후 첫 N턴 (현재 게이트 닫힘, §6 참고) | 1×1 포함 접대 블록 |
| 1 | **Trap** | 판 더러움 + 유저 잘못 큼 + 극소 확률 | 일부만 놓고 게임오버 유도 (극희귀) |
| 2 | **ComboBreak** | 판 너무 빔 + 유저 잘못 중간↑ + 확률 | 놓을 순 있지만 이번 턴 라인 클리어 불가 |
| 3 | **Hospitality** | 보드에 좋은 기회(한 칸 부족한 줄 등)가 보일 때 + 확률 | 멀티라인·올클 기회를 주는 접대 |
| 3.5 | **Momentum** | 직전 턴 **멀티라인급** 클리어(10칸+) + 확률 | 큼직한 사각 위주 "기분 좋은 패" (흐름 유지 — 실게임 분석 반영) |
| 4 | **Easy** | 판이 험한데 유저 잘못이 아닐 때 | 콤보 유지 가능한 독립 추첨 핸드 |
| 5 | **Pressure** | 판 더러움 + 확률 | **정답 순서가 딱 1개**인 유일수 |
| 6 | **Normal** | 항상 (위 전부 실패 시) | 모양 가중표에서 **슬롯 3개 독립 추첨(중복 허용)**한 통과 후보 중 **플레이 후 BoardHealth가 가장 좋아지는** 핸드 |
| 7 | **Fallback** | 6까지 실패 | 최소한 1개는 놓을 수 있는 조합 강제 |

안전장치: **어떤 경우에도 3개 중 최소 1개는 놓을 수 있는 조합**만 나온다 (즉사 없음).

---

## 2. 지금 무슨 티어가 나왔는지 확인하는 법

플레이 모드에서 매 리필마다 Console에 1줄씩 찍힌다.

```text
[BlockSelect] turn=12 zone=Sweet health=0.62 blame=18.4 tier=Normal bundle=generated
[BlockSelect] turn=40 zone=TooDirty health=0.31 blame=42.1 tier=Pressure bundle=generated
```

| 항목 | 의미 |
|------|------|
| `turn` | 몇 번째 리필인지 (0부터) |
| `zone` | 현재 BoardHealth 구간 (TooEmpty / Sweet / TooDirty) |
| `health` | BoardHealth 점수 0~1 (높을수록 건강) |
| `blame` | 현재 누적 BlameScore (감쇠 반영) |
| `tier` | 이번 턴 선택된 티어 |
| `bundle` | 번들 ID (Trap·ComboBreak·Relife·Momentum), 독립 추첨·실시간 생성이면 `generated` |

**튜닝 전 반드시 이 로그부터 볼 것.** "어렵다"는 체감이 zone 판정 문제인지, blame이 안 쌓여서인지, 확률이 낮아서인지 로그로 구분된다.

---

## 3. 증상별 — 무슨 수치를 건드려야 하나

가장 많이 쓰게 될 표. 모든 수치는 `DefaultBlockSelectionTuning.asset` 인스펙터에 있고, 각 필드에 툴팁도 달려 있다.

### "게임이 너무 어렵다 / 자꾸 막힌다"

| 조치 | 필드 | 현재값 | 방향 |
|------|------|-------:|------|
| 접대(Hospitality)를 더 자주 | `HospitalityProbability` | 0.75 | ↑ (최대 1) |
| 접대 발동 문턱을 낮춤 | `OpportunityHighThreshold` | 0.7 | ↓ (0.6 정도) |
| 접대 품질 기준 완화 (더 자주 성공) | `HospitalityMinQualityClears` | 2 | ↓ (1) |
| Easy 티어를 더 넓게 | `EasyHealthThreshold` | 0.45 | ↑ |
| Easy 티어의 blame 허용치 완화 | `EasyBlameMax` | 15 | ↑ |
| 유일수(Pressure)를 덜 | `PressureProbability` | 0.5 | ↓ |
| 콤보 끊기를 덜 | `ComboBreakProbability` | 0.04 | ↓ |

### "게임이 너무 쉽다 / 긴장감이 없다"

| 조치 | 필드 | 현재값 | 방향 |
|------|------|-------:|------|
| 접대 빈도 축소 | `HospitalityProbability` | 0.75 | ↓ |
| 유일수를 더 자주 | `PressureProbability` | 0.5 | ↑ |
| 유일수 발동 범위 확대 (더 건강한 판에서도) | `PressureHealthThreshold` | 0.45 | ↑ |
| 콤보 끊기를 더 | `ComboBreakProbability` | 0.04 | ↑ (0.05 이하 권장) |
| Trap을 더 (신중히!) | `TrapProbability` | 0.008 | ↑ (0.01 이하 권장) |

### "유일수(Pressure)가 한 번도 안 나온다"

로그에서 `tier=Pressure`가 안 보일 때. 원인은 보통 셋 중 하나.

1. **게이트를 못 넘음** — `zone=TooDirty`가 잘 안 뜨면 판이 충분히 더러워지기 전에 게임이 끝나는 것. `PressureHealthThreshold` ↑ (0.5~0.55)로 범위를 넓힌다.
2. **확률에서 떨어짐** — `PressureProbability` ↑.
3. **후보 생성 실패** — 유일수 조건(정답 순서 딱 1개 + 난이도 하한)이 까다로움. `PressureDifficultyMin` ↓ (0.3~0.4) 또는 `PressureSampleCount` ↑ (60~80, 성능 주의).

### "콤보가 어이없게 자꾸 끊긴다"

| 조치 | 필드 | 현재값 | 방향 |
|------|------|-------:|------|
| ComboBreak 확률 축소 | `ComboBreakProbability` | 0.04 | ↓ |
| ComboBreak blame 문턱 상향 (더 큰 실수에만) | `BlameComboBreakThreshold` | 25 | ↑ |

### "Trap(함정)이 체감된다 / 억울하게 죽는다"

Trap은 의도적으로 **극희귀**(0.5~1%)여야 한다. 로그에 `tier=Trap`이 자주 보이면:

| 조치 | 필드 | 현재값 | 방향 |
|------|------|-------:|------|
| Trap 확률 축소 | `TrapProbability` | 0.008 | ↓ (0.005 또는 0) |
| Trap blame 문턱 상향 | `BlameTrapThreshold` | 55 | ↑ |

### "zone 판정이 이상하다" (거의 항상 TooEmpty / TooDirty만 뜸)

zone은 `fillRate`(점유율)와 `healthScore` 두 조건으로 판정한다.

```text
fillRate < TooEmptyFillMax(0.12)  또는 healthScore < TooEmptyScoreMax(0.35) → TooEmpty
fillRate > TooDirtyFillMin(0.55) 또는 healthScore < TooDirtyScoreMax(0.40) → TooDirty
그 외 → Sweet
```

- 초반에 `TooEmpty`가 계속 뜨는 건 정상 (빈 보드는 원래 TooEmpty).
- `Sweet`가 거의 안 나오면 `TooEmptyFillMax` ↓ / `TooDirtyFillMin` ↑ 로 Sweet 구간을 넓힌다.

### "blame이 안 쌓인다 / 너무 빨리 쌓인다"

로그의 `blame` 값이 임계값(25/35/55) 근처까지 안 가면 Trap·ComboBreak·Pressure 게이트가 영영 안 열린다.

| 조치 | 필드 | 현재값 | 방향 |
|------|------|-------:|------|
| 실수당 blame 증가량 | `BlamePerDeadZone` | 8 | ↑↓ (죽은 틈 1개당 — 과하면 응징 남발) |
| | `BlamePerCenterCell` | 4 | ↑↓ (중앙 2×2 점유당) |
| | `BlamePerBigSlotLost` | 10 | ↑↓ (큰 블록 자리 잃은 턴당) |
| | `BlamePerFreedomDrop` | 0.15 | ↑↓ (배치 자유도 감소 1당 — 클리어 없는 턴도 자연 하락하므로 낮게) |
| blame이 오래 남게/빨리 사라지게 | `BlameDecayRate` | 0.7 | ↑ = 오래 남음, ↓ = 빨리 사라짐 (매 턴 곱해짐) |
| 판 개선 시 blame 보상 | `BlameHealthGainRelief` | 60 | ↑↓ (healthScore +0.1 개선당 blame -6 차감. 0이면 끔) |

### "특정 블록이 너무 자주/안 나온다"

`DefaultBlockSelectionTuning.asset`의 **Block Weights** 리스트에서 모양별로 티어별 가중치를 조정한다 (Normal / Hospitality / Pressure 각각).

**phase9부터 Normal·Easy 티어는 이 가중표에서 슬롯 3개를 독립 추첨한다 (중복 허용 — 실게임과 동일).** 즉 normalWeight가 손패 구성을 직접 결정하며, 같은 모양이 한 패에 2개(페어)까지 나올 수 있다. **같은 모양 3개(트리플)는 샘플러 차원에서 금지** (`ShapeSampler` — 번들 포함 전 경로).

- 값은 상대 가중치 (합이 100일 필요 없음). 0이면 해당 티어에서 절대 안 나옴.
- **`1x1`·`1x2`는 전 티어 0 유지** — 1×1은 Relife 번들 전용, 1×2는 억지 블록이라 설계상 금지.
- **`Diag2`·`Diag3`도 전 티어 0 유지** — 실게임 344프레임 분석에서 대각선 조각 0건 (오리지널에 없는 모양). 번들에서도 전부 제외됨.
- ⚠️ **가중치는 씬 시작 시 캐시된다** — 플레이 모드 중 바꿔도 반영 안 됨. 플레이를 껐다 다시 시작할 것. (그 외 확률·임계값 수치는 플레이 중 변경 즉시 다음 리필부터 반영됨.)

번들(고정 3블록 조합)은 이제 **특수 티어 전용**이다 (Trap `trap_*` · ComboBreak `cb_*` · Relife `relife_*` · Momentum `mom_*`). 추가·수정하려면 `ScriptableObjects/BlockSelection/Bundles/`의 에셋을 편집하고, 새로 만들면 `BlockBundlePool.asset`에 등록한다. `normal_*` 번들 13종은 phase9에서 독립 추첨으로 대체되며 삭제됨.

### "프레임 드랍 / 리필 순간 렉"

솔버(시뮬레이션) 비용을 줄인다. 품질은 다소 떨어진다.

| 필드 | 현재값 | 방향 |
|------|-------:|------|
| `HospitalitySampleCount` | 60 | ↓ |
| `PressureSampleCount` | 40 | ↓ |
| `FallbackSampleCount` | 40 | ↓ |
| `OutcomeBeamWidth` | 4 | ↓ (최소 2) |
| `BundleProbeCount` | 8 | ↓ (특수 티어 번들 검증 수) |
| `NormalHealthCandidateCount` | 4 | ↓ (1이면 Health 비교 없이 단순 가중 추첨) |
| `NormalSampleTries` | 12 | ↓ (Normal·Easy 핸드 샘플 시도 예산) |

---

## 4. 전체 파라미터 레퍼런스

인스펙터 헤더 순서대로. 각 필드에 툴팁이 있으므로 여기서는 "이걸 올리면 어떻게 되는가"만 적는다.

### Health Zone / Health Weights / Health Normalize

판 상태 진단 기준. **웬만하면 건드리지 말 것** — zone 판정이 바뀌면 모든 티어의 발동 조건이 같이 흔들린다.

| 필드 | 기본 | 올리면 |
|------|-----:|--------|
| `TooEmptyFillMax` | 0.12 | TooEmpty 판정 범위 넓어짐 → ComboBreak 기회 증가 |
| `TooDirtyFillMin` | 0.55 | TooDirty 판정 범위 좁아짐 → Trap·Pressure 기회 감소 |
| `TooEmptyScoreMax` / `TooDirtyScoreMax` | 0.35 / 0.40 | healthScore 기반 보조 판정 문턱 |
| `FillDirtyFalloff` | 0.35 | 판이 찰 때 health 하락이 완만해짐 |
| `FillWeight` / `DeadZoneWeight` / `BigSlotWeight` / `FreedomWeight` / `ClusterWeight` | 0.35/0.15/0.15/0.15/0.2 | healthScore 성분 비중 (합 1 권장) |
| `DeadZoneNormalizeMax` | 6 | 죽은 틈 개수 정규화 상한 |
| `BigSlotNormalizeMax` / `FreedomNormalizeMax` | 100 / 100 | 정규화 상한 (빈 보드 기준값, 고정 권장) |
| `ClusterCohesionShare` | 0.5 | 클러스터 성분에서 "한 덩어리로 모임" 비중 (나머지는 최대 덩어리 크기) |
| `ClusterSizeNormalizeMax` | 20 | 최대 덩어리가 이 칸 수 이상이면 크기 성분 만점 |

### Blame

유저 실수 추적. §3 "blame이 안 쌓인다" 참고.

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `BlamePerDeadZone` | 8 | 새 죽은 틈(1~3칸 고립 빈칸) 1개당 — 흔한 실수라 낮게 (2개 연속이어도 Pressure 문턱 미달) |
| `BlamePerCenterCell` | 4 | 중앙 2×2 새 점유 칸 1개당 |
| `BlamePerBigSlotLost` | 10 | 3×3·1×5 자리를 잃은 턴에 1회 |
| `BlamePerFreedomDrop` | 0.15 | 배치 자유도 감소분 × 이 값 — 클리어 없는 턴도 10~30 자연 하락하므로 낮게 |
| `BlameHealthGainRelief` | 60 | healthScore가 오른 턴은 증가분 × 이 값만큼 blame **차감** (판 개선 보상) |
| `BlameDecayRate` | 0.7 | 매 턴 `blame = max(0, blame × 이 값 + 이번 턴 delta - Health 개선 차감)` |

### Blame Thresholds

| 필드 | 기본 | 게이트 |
|------|-----:|--------|
| `BlameComboBreakThreshold` | 25 | ComboBreak: blame ≥ 이 값 |
| `BlamePressureThreshold` | 35 | Pressure 가중용 (현재 참고값) |
| `BlameTrapThreshold` | 55 | Trap: blame ≥ 이 값 |
| `EasyBlameMax` | 15 | Easy: blame **<** 이 값 (유저 탓 아닐 때만) |
| `GoodTurnBlameDeltaMax` | 5 | GoodTurn 판정(UI 피드백용): 이번 턴 delta ≤ 이 값 |

### Tier Gates

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `RelifeTurnCount` | 2 | 재시작 후 Relife가 적용되는 턴 수 |
| `TrapProbability` | 0.008 | Trap 게이트 통과 후 발동 확률 (0.5~1% 유지 권장) |
| `ComboBreakProbability` | 0.04 | ComboBreak 발동 확률 |
| `EasyHealthThreshold` | 0.45 | healthScore < 이 값이면 "판이 험함" → Easy 후보 |
| `BundleProbeCount` | 8 | 특수 티어(Trap·ComboBreak·Relife·Momentum)당 솔버 검증할 최대 번들 수 (성능) |
| `NormalHealthCandidateCount` | 4 | Normal 티어가 "플레이 후 Health"를 비교할 후보 핸드 수. 1 = 순수 가중 추첨, 클수록 판이 건강하게 유지되지만 리필이 무거워짐 |
| `NormalSampleTries` | 12 | Normal·Easy 독립 추첨의 최대 샘플 시도 (검증 실패분 포함) |

### Momentum (흐름 유지 — 기분 좋은 패)

직전 턴에 라인 클리어가 있었으면 확률적으로 큼직한 사각 번들(`mom_*` 3종: `mom_bigsquares`[3x3,3x3,2x2]·`mom_squarefeast`[3x3,2x2,3x2]·`mom_rects`[3x2,3x2,1x4])을 준다. 실게임 344프레임 분석에서 확인된 "콤보 중 큼직한 패" 패턴의 재현 (`Docs/BLOCKBLAST_ANALYSIS.md` §5). ~~`mom_bigtriple`[3x3×3]~~은 체감 과해서 삭제됨.

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `MomentumProbability` | 0.4 | 게이트 통과 후 Momentum 시도 확률. **0이면 끔.** 높으면 클리어→큰 사각→또 클리어 양성 루프로 점수가 쉬워짐 |
| `MomentumMinClearedCells` | 10 | 발동에 필요한 직전 턴 클리어 칸 수. 한 줄 = 8칸 → 10이면 멀티라인급 턴에서만. 낮추면 단일 클리어에도 발동 |

### Density Bias (밀도 바이어스)

보드가 빽빽하면 얇은 블록(1xN)이, 널널하면 큰 블록(6칸+)이 더 자주 나온다 (실게임의 밀도 역상관 재현). Normal·Easy에선 모양 단위 가중 배수, Momentum에선 번들 단위 배수로 적용.

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `DenseFillMin` / `DenseSlimBoost` | 0.45 / 2 | fillRate > 0.45면 얇은 블록 가중 ×2 |
| `SparseFillMax` / `SparseBigBoost` | 0.25 / 1.3 | fillRate < 0.25면 큰 블록 가중 ×1.3 — Momentum과 겹치면 사각 러시가 되므로 낮게 유지 |

### Snug Fit (쏙 맞춤)

보드에 특정 블록이 "쏙 들어가는" 자리(둘레가 벽·블록으로 막힌 포켓)가 있으면 그 블록이 더 자주 나온다 (Normal·Easy는 모양 단위, Momentum은 번들 단위).

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `SnugEnclosureMin` | 0.8 | 쏙 판정 문턱 — 둘레 막힘 비율(사방 밀폐 = 1.0, 위만 뚫림 ≈ 0.75)이 이 미만이면 보너스 없음. **낮추면 작은 조각(L3 등)이 상시 부스트돼 노골적** |
| `SnugWeightBoost` | 0.6 | 추첨 가중 증가폭 — 사방 밀폐로 쏙 맞는 모양은 가중 ×1.6. **0이면 기능 끔.** 크면 노골적 |
| `SnugNormalRankBonus` | 0.06 | Normal 최종 선택(예측 Health 비교)에서 쏙 후보에 주는 가산점 — 예측 Health가 비슷할 때만 갈리는 수준 |

### Hospitality (접대)

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `HospitalityProbability` | 0.75 | 기회가 보여도 이 확률로만 시도 (변덕 — 100% 주지 않음) |
| `OpportunityHighThreshold` | 0.7 | 보드 기회 점수(0~1)가 이 값 이상이어야 시도 |
| `HospitalitySampleCount` | 60 | 후보 조합 샘플 수 (품질↔성능) |
| `HospitalityMinQualityClears` | 2 | 완벽 플레이 시 총 클리어 라인이 이 값 미만이면 버림 (억지 접대 차단) |
| `OpportunityNearLineWeight` | 0.25 | 한 칸 부족한 행·열 1개당 기회 점수 가산 |
| `OpportunityMultiLineBonus` | 0.15 | 한 칸 부족 줄이 2개 이상일 때 보너스 |
| `OpportunityAllClearWeight` / `OpportunityAllClearFillMax` | 0.2 / 0.2 | 올클리어 잠재 가산 / 그 판정 fillRate 상한 |
| `OpportunityBigSlotWeight` | 0.15 | 큰 블록 들어갈 자리 가산 |
| `OpportunityDeadZonePenalty` | 0.08 | 죽은 틈 1개당 감점 — 과하면 포켓 있을 때 접대 안 나옴 |
| `OutcomeBeamWidth` | 4 | 최선 결과 추정 탐색 폭 (성능) |

### Pressure (유일수)

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `PressureProbability` | 0.5 | 게이트 통과 후 시도 확률 |
| `PressureHealthThreshold` | 0.45 | TooDirty가 아니어도 healthScore < 이 값이면 게이트 통과 |
| `PressureSampleCount` | 40 | 후보 샘플 수 (유일수 판정은 비쌈 — 성능 주의) |
| `PressureDifficultyMin` | 0.5 | 유일해 난이도 하한 (너무 쉬운 유일수 제외) |
| `PressureBigFinishWeight` | 0.5 | 난이도 가산: 마지막 스텝이 큰 블록 |
| `PressureSetupClearWeight` | 0.5 | 난이도 가산: 앞 두 스텝에 클리어 필요 |
| `PressureBigFinishMinCells` | 5 | "큰 블록" 최소 칸 수 |

### Fallback

| 필드 | 기본 | 의미 |
|------|-----:|------|
| `FallbackSampleCount` | 40 | 최후 실시간 조합 샘플 수 |

---

## 5. 튜닝 워크플로 권장

1. **플레이 → Console의 `[BlockSelect]` 로그 확인** — 체감 문제가 어느 티어/수치 때문인지 먼저 특정.
2. **한 번에 한 수치만** 조정. 확률·임계값은 플레이 모드 중 바꿔도 다음 리필부터 바로 반영된다 (Block Weights만 예외 — 재시작 필요).
3. 티어 확률(`TrapProbability` 등)은 표본이 커야 체감 — 최소 수십 턴 플레이 후 판단.
4. 확정된 값은 그대로 SO에 저장하고 커밋하면 끝 (코드 수정 불필요).

---

## 6. 알아둘 제약

- **Relife 티어는 현재 발동 안 함** — 재시작이 씬 리로드 방식이라 `IsRetrySession`이 상수 `false`. 연동 전까지 `RelifeTurnCount`를 바꿔도 효과 없음.
- **점수·콤보 수는 알고리즘 입력이 아니다.** "고득점이면 어렵게" 같은 조정은 이 SO로 불가능 (설계상 의도).
- Trap·ComboBreak는 조건이 맞아도 **번들 검증(솔버)을 통과해야** 나온다. 확률을 올려도 로그에 안 보이면 번들 풀(`Bundles/` 폴더)에 조건을 만족시킬 번들이 부족한 것 — 번들 추가가 답일 수 있다.
- 관련 코드: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs` (티어 스택), `Scripts/Data/BlockSelectionTuningSO.cs` (수치 정의), `Scripts/Bootstrap/BlockSpawnBootstrap.cs` (로그).
