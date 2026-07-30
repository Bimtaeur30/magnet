# 블록 선택 알고리즘 — 구현 기획서

> **slug:** `block-selection-algorithm`  
> **작성:** 2026-07-30  
> **대상 독자:** 구현 개발자 (JTH)  
> **상태:** grill 확정 스펙 → 구현 전 기획서  
> **관련:** `Docs/DESIGN.md` §4.2·§4.9, `random-block-spawn` Phase 6·7

---

## 0. 이 문서로 할 수 있는 것

이 문서만 읽고 아래를 구현할 수 있어야 한다.

1. **보드 + 블록 3개**에 대한 솔버(배치 가능·전체 순서·유일수·콤보 유지) 판정
2. **BoardHealth / BlameScore** 계산
3. **매 턴 3블록**을 어떤 티어·어떤 방식(번들 vs 실시간)으로 고를지 결정
4. 기존 `BlockSupply` / `AbstractDrawer`에 연결

**코드는 이 문서의 의사코드·인터페이스를 따른다.** 수치는 `BlockSelectionTuningSO`에서 튜닝한다.

---

## 1. 한 줄 요약

Block Blast 스타일 **8×8 보드**에서, 매 턴 하단 **3슬롯**에 나올 블록을 **순수 RNG가 아닌** 규칙으로 고른다.

- **판 상태**(`BoardHealth`)와 **유저 최근 실수**(`BlameScore`)를 본다.
- **점수·콤보 수는 직접 입력이 아니다.**
- 대부분은 **미리 만든 3블록 묶음(번들)** 이다.
- **접대(Hospitality)**·**의도적 유일수(Pressure)** 만 **보드 맞춤 실시간 생성**이다.
- **즉사 번들(3개 전부 처음부터 불가)은 없다.** 최소 1개는 놓을 수 있게 한다.
- **Trap**(일부만 넣고 막힘)·**ComboBreak**(살지만 콤보 못 깸)는 **극히 드물게** 나온다.

---

## 2. 게임 규칙 전제 (구현 시 불변)

| 항목 | 값 |
|------|-----|
| 보드 | 8×8 (`BoardGrid`) |
| 턴당 블록 수 | 3 (`BlockSupply.SlotCount`) |
| 플레이어 회전 | 없음. **스폰 시** 0/90/180/270° 중 하나 고정 |
| 중력 | 없음. 라인 클리어만 칸 삭제 |
| 턴 | 3슬롯 전부 소진 후 리필 |
| 게임오버 | 리필 직후(또는 진행 중) **남은 블록을 어디에도 못 놓음** |
| 콤보 | 턴(최대 3블록) 중 1회 이상 라인 클리어 시 유지 (`DESIGN.md` §4.7) |

**솔버는 반드시 라인 클리어를 시뮬레이션한다.**  
“3개를 순서대로 놓되, 매 배치 후 행·열 클리어 → 칸 삭제 → 연쇄”까지 반영해야 Trap / Unique / ComboBreak 판정이 맞다.

---

## 3. 설계 원칙

1. **Death 없음:** 스폰 직후 `hasAnyPlacement >= 1` 항상 보장.
2. **번들 vs 실시간 분리:**
   - 번들: Normal, Trap, ComboBreak (+ Easy는 Normal 필터)
   - 실시간: Hospitality, Pressure (+ 최종 fallback)
3. **억지 블록 금지:** Hospitality / Pressure / Easy 시도 중 `1×1`, `1×2` 등이 **필수**면 해당 티어 **포기** → 아래 스택으로 fallthrough.
4. **쉬운 유일수 티어 없음:** 의도적 유일수는 **Pressure만**. Hospitality가 우연히 unique여도 Pressure가 아님.
5. **Hospitality 품질:** 강한 기회만. **강해도 억지면 무시.** 100% 주지 않음(변덕 확률).
6. **Domain 순수:** 솔버·선택기는 Unity·이벤트·DI 없이 테스트 가능하게.

---

## 4. 용어 사전

| 용어 | 의미 |
|------|------|
| **피스(Piece)** | 스폰될 블록 1개 = `shapeId` + 고정 `cellOffsets` + 고정 `rotation` |
| **번들(Bundle)** | 피스 3개의 고정 조합 (순서는 슬롯 0,1,2에 대응) |
| **라운드(Round)** | 이번에 준 3피스를 **모두 소진할 때까지**의 구간 (= 1턴) |
| **full sequence** | 3피스를 어떤 **순서**로, 어떤 **위치·회전**으로든 **전부** 놓는 방법 1가지 |
| **hasAny** | 현재 보드에서 3피스 중 **최소 1개**라도 지금 당장 놓을 수 있는가 |
| **Trap** | `hasAny == true` && `fullSequenceExists == false` → 일부만 넣고 게임오버 |
| **ComboBreak** | `fullSequenceExists` && `comboMaintainable == false` → 살지만 이번 라운드에 라인 클리어 불가 |
| **Unique / Pressure** | `countFullSequences == 1` && difficulty ≥ 하한 |
| **BoardHealth** | 판이 얼마나 “건강한지” (높을수록 여유, 낮을수록 더러움) |
| **BlameScore** | 최근 유저 배치가 판을 망친 정도 (점수 무관) |
| **TooEmpty / Sweet / TooDirty** | BoardHealth 구간. 너무 비움 / 적정 / 너무 참 |

---

## 5. 시스템 전체 흐름

```text
[턴 종료 / 리필 시점]
        │
        ▼
┌───────────────────┐
│ BoardGrid 스냅샷   │
│ BlameTracker 상태  │
│ (Score는 사용 안 함)│
└─────────┬─────────┘
          ▼
┌───────────────────┐
│ BoardHealth 계산   │ → zone: TooEmpty | Sweet | TooDirty
│ BlameScore 읽기    │
└─────────┬─────────┘
          ▼
┌───────────────────┐
│ Tier Priority Stack│  (§9) — 위에서부터 시도, 성공 시 종료
└─────────┬─────────┘
          ▼
┌───────────────────┐
│ 3 × Piece 확정     │  cellOffsets + rotation (+ skin은 BlockSupply)
└─────────┬─────────┘
          ▼
    BlockSupply.Fill()
```

**배치 후 / 턴 종료 후:**

```text
BlameTracker.OnTurnEnded(placementDelta, boardBefore, boardAfter)
```

---

## 6. 블록 카탈로그 (17종)

PTY `BlockShapeSource` 기준 canonical ID. 스폰 시 **회전 4방향** 적용 → 대각선 `Diag2`/`Diag3`는 각 1종만 등록.

| shapeId | 칸 수 | Normal 풀 | 비고 |
|---------|------:|:---------:|------|
| `1x1` | 1 | ❌ | Hospitality(재시작 직후) 전용 |
| `1x2` | 2 | ❌ | 억지 블록 — 특수 티어에서 fallthrough |
| `1x3` | 3 | ✅ | |
| `1x4` | 4 | ✅ | |
| `1x5` | 5 | ✅ | Big Three |
| `2x2` | 4 | ✅ | |
| `3x2` | 6 | ✅ | |
| `3x3` | 9 | ✅ | Big Three |
| `L3` | 3 | ✅ | |
| `L4` | 4 | ✅ | |
| `J4` | 4 | ✅ | |
| `T4` | 4 | ✅ | |
| `S4` | 4 | ✅ | |
| `Z4` | 4 | ✅ | |
| `L3x3` | 5 | ✅ | Big L |
| `Diag2` | 2 | ✅ | 코너 대각 2칸 |
| `Diag3` | 3 | ✅ | 코너 대각 3칸 |

**Normal / Trap / ComboBreak 번들**에는 `1x1`을 넣지 않는다.

---

## 7. 회전 처리

### 7.1 규칙

- 플레이어는 회전 불가.
- **Draw 시점**에 각 피스마다 `rotation ∈ {0, 90, 180, 270}` 랜덤(또는 균등).
- 솔버·배치 검사는 **회전이 적용된 `cellOffsets`** 로 한다.

### 7.2 구현 메모

```text
RotateOffsets(canonicalOffsets, rotation) → IReadOnlyList<Vector2Int>
```

- canonical은 PTY `BlockShapeSO.CellOffsets` (pivot 기준 상대 좌표).
- `PlacementService.CanPlace`는 이미 회전된 offsets를 받는다고 가정.

### 7.3 솔버에서의 탐색

한 피스당:

```text
for rotation in {0, 90, 180, 270}:
  offsets = RotateOffsets(shape, rotation)
  for pivot in all grid positions:
    if CanPlace(offsets, pivot, grid):
      → 배치 시도
```

---

## 8. 솔버 / 시뮬레이션 API (Phase 1 핵심)

### 8.1 공개 API (제안 시그니처)

```csharp
// Domain/BlockSelection/Simulation/

bool HasAnyPlacement(BoardSnapshot board, PieceSet pieces);

bool FullSequenceExists(BoardSnapshot board, PieceSet pieces);

bool ComboMaintainable(BoardSnapshot board, PieceSet pieces, ComboContext combo);

int CountFullSequences(BoardSnapshot board, PieceSet pieces, int cap = 3);
// cap: 2 이상이면 조기 종료 (unique 판정은 1인지만 확인하면 됨)

SimulationResult SimulatePlace(BoardSnapshot board, Piece piece, Vector2Int pivot);
// 내부: 배치 → LineClearDetector 동일 로직 → 연쇄 클리어 until stable
```

`PieceSet` = 길이 3인 `Piece[]` (null 슬롯 없음, 스폰 직후 기준).

### 8.2 BoardSnapshot

실제 `BoardGrid`를 오염시키지 않기 위해 **복사본** 사용.

```text
BoardSnapshot:
  int BoardSize
  bool IsOccupied(x, y)
  Clone() → BoardSnapshot
  ApplyPlacement(offsets, pivot) → new snapshot + clearedLineCount
```

`BoardGrid`에 `Clone()` 또는 `BoardSnapshot.From(BoardGrid)` 추가를 권장.

### 8.3 배치 1수 시뮬 (의사코드)

```text
function SimulatePlace(board, piece, pivot):
  if not CanPlace(piece.offsets, pivot, board):
    return INVALID

  newBoard = board.Clone()
  for cell in piece.offsets at pivot:
    newBoard.SetOccupied(cell, true)

  changedCells = placed cells
  totalClears = 0

  loop:
    result = LineClearDetector.Detect(newBoard, changedCells)
    if result.clearedLines is empty:
      break
    for each line in result.clearedLines:
      remove all cells on that line from newBoard
      totalClears += 1
    changedCells = removed cells  // 연쇄 검사용

  return { board: newBoard, clearCount: totalClears }
```

### 8.4 Full sequence 백트래킹

3피스 **순열** 6가지 × 각 피스의 (rotation, pivot) 탐색.

```text
function CountFullSequences(board, pieces[3], cap):
  count = 0
  for perm in permutations([0,1,2]):
    if DFS(board, perm, 0):
      count += 1
      if count >= cap: return count
  return count

function DFS(board, perm, depth):
  if depth == 3:
    return true  // 3개 모두 배치 성공

  pieceIndex = perm[depth]
  piece = pieces[pieceIndex]

  for rotation in {0,90,180,270}:
    offsets = Rotate(piece.shape, rotation)
    for pivot in all positions:
      sim = SimulatePlace(board, {offsets}, pivot)
      if sim.invalid: continue
      if DFS(sim.board, perm, depth + 1):
        return true
  return false
```

**`FullSequenceExists`** = `CountFullSequences >= 1`  
**`uniqueFullSequence`** = `CountFullSequences == 1` (cap=2로 최적화)

### 8.5 HasAnyPlacement

```text
for each piece in pieces:
  for rotation, pivot:
    if CanPlace: return true
return false
```

스폰 결과 검증: **반드시 true**. false면 해당 후보 폐기.

### 8.6 ComboMaintainable

**의미:** 3피스를 전부 소진하는 **어떤 full sequence**에서든, 라운드 중 **최소 1회** 라인 클리어가 발생하는가.

```text
function ComboMaintainable(board, pieces):
  for each full sequence (perm + placements):
    simBoard = board
    clearedInRound = 0
    for each step in sequence:
      sim = SimulatePlace(simBoard, ...)
      clearedInRound += sim.clearCount
      simBoard = sim.board
    if clearedInRound >= 1:
      return true
  return false
```

`ComboContext` (선택): 현재 콤보 상태는 **스폰 알고리즘 입력이 아님**.  
ComboBreak는 “이번 3피스 라운드에서 클리어 가능 여부”만 본다.

### 8.7 성능 메모

- 8×8, 3피스, 4회전 → brute-force 허용 범위.
- `CountFullSequences`는 cap=2로 조기 종료.
- 번들 후보가 많으면 **셔플 후 상위 K개만** 솔버 돌리기 (튜닝 SO).

---

## 9. 매 턴 티어 우선순위 스택

**위에서 아래로** 시도. 해당 티어에서 **유효한 3피스를 확정**하면 종료.  
실패(후보 0)면 **다음 단계**로 fallthrough.

| 순서 | 티어 | 조건 (요약) | 생성 방식 |
|:----:|------|-------------|-----------|
| 1 | **Trap** | TooDirty + Blame 매우 높음 + `p_trap` | 번들 + 검증 |
| 2 | **ComboBreak** | TooEmpty + Blame 중간 이상 + `p_comboBreak` | 번들 + 검증 |
| 3 | **Hospitality** | opportunity 높음 + `p_hospitality` | 실시간 |
| 4 | **Easy** | Health 나쁨 + Blame 낮음 | Normal 번들 필터 |
| 5 | **Pressure** | TooDirty (Trap 구간 제외) + `p_pressure` | 실시간 |
| 6 | **Normal** | 항상 | 번들 가중 랜덤 |
| 7 | **Fallback** | 6까지 실패 | 실시간 느슨한 조합 → 그래도 실패 시 Normal 번들 강제 |

### 9.1 Trap (순서 1)

**목표:** 유저가 일부만 넣고 게임오버되는 **극희귀** 패턴.

**게이트:**

```text
zone == TooDirty
AND blame >= blameTrapThreshold   // "매우 높음"
AND random() < p_trap             // 권장 시작값: 0.005 ~ 0.01 (0.5~1%)
```

**번들 후보:**

```text
tag == Trap
AND HasAnyPlacement(board, bundle) == true
AND FullSequenceExists(board, bundle) == false
```

후보 중 가중 랜덤 1개. 없으면 fallthrough.

### 9.2 ComboBreak (순서 2)

**게이트:**

```text
zone == TooEmpty
AND blame >= blameComboBreakThreshold
AND random() < p_comboBreak       // 권장: 0.03 ~ 0.05
```

**번들 후보:**

```text
tag == ComboBreak
AND FullSequenceExists == true
AND ComboMaintainable == false
AND HasAnyPlacement == true
```

### 9.3 Hospitality (순서 3)

§10 참고. 실패 시 fallthrough.

### 9.4 Easy (순서 4)

**게이트:**

```text
healthScore < easyHealthThreshold    // 판이 험함
AND blame < easyBlameMax           // 유저 탓 아님
```

**선택:** Normal 태그 번들 중 필터 (§11.2).

### 9.5 Pressure (순서 5)

§11 참고.

**게이트:**

```text
zone == TooDirty OR healthScore < pressureHealthThreshold
AND NOT (Trap 게이트 통과했을 때)  // Trap은 1번에서만
AND random() < p_pressure          // 100% 아님
```

### 9.6 Normal (순서 6)

Normal 태그 번들, 가중 랜덤.

**필터:**

```text
HasAnyPlacement == true
(권장) FullSequenceExists == true  // 일반 플레이는 통과 가능 번들
```

### 9.7 Fallback (순서 7)

```text
1) 실시간: HasAny + FullSequence 만족하는 3개 조합 빠르게 샘플
2) 그래도 없으면: Normal 번들 중 HasAny 만족하는 것 아무거나
```

---

## 10. Hospitality (접대) 실시간 생성

### 10.1 원칙

| 원칙 | 설명 |
|------|------|
| 강한 기회만 | 멀티라인·올클리어·보드가 예쁘게 모인 경우 |
| 강해도 억지면 무시 | 작은 블록 끼워 맞추기·어색한 올클이면 **스킵** |
| 변덕 | `opportunityScore`가 높아도 `p_hospitality` (70~85%) 확률로만 시도 |
| unique는 부수 효과 | 의도하지 않음 |

### 10.2 OpportunityScorer (의사코드)

보드만 보고 0~1 점수.

```text
opportunityScore(board):
  s = 0

  // near-complete lines (한 칸 부족한 행/열 수)
  nearLines = count rows/cols with (8 - occupied) == 1
  s += nearLines * w_nearLine

  // 동시에 여러 줄이 한 칸 부족 (멀티라인 잠재)
  s += multiLineSetupBonus(board)

  // all-clear 가능성 (남은 칸이 적고 구조가 단순)
  if fillRate < 0.2 and deadZoneCount == 0:
    s += w_allClearPotential

  // 큰 연속 빈 영역 (3x3, 1x5 들어갈 자리)
  s += bigSlotScore(board)

  // 억지 패널티: 죽은 땅 많으면 감점
  s -= deadZoneCount * w_deadZone

  return clamp01(s)
```

**게이트:** `opportunityScore >= opportunityHighThreshold` (권장 0.65~0.75 시작)

### 10.3 HospitalityGenerator

```text
function TryGenerateHospitality(board, weights, tuning):
  if opportunityScore(board) < HIGH: return FAIL
  if random() > p_hospitality: return FAIL

  candidates = empty list
  repeat sampleCount times:  // 권장 50~200
    combo = Sample3Shapes(allowedPool, weights)  // 1x1 제외 (재시작 직후 예외 §12)
    if UsesForcedAwkwardBlock(combo): continue   // 1x2 등 억지 규칙

    pieces = AssignRotations(combo)
    if not FullSequenceExists(board, pieces): continue

    score = SimulateBestOutcome(board, pieces)
    // score: 총 클리어 라인 수, all-clear 여부, 큰 블록 사용 등

    if score < hospitalityMinQuality: continue   // 억지 올클 차단
    candidates.add({ pieces, score })

  if candidates empty: return FAIL
  return weightedPick(candidates)  // score 높을수록 선택
```

`SimulateBestOutcome`: 6순열 × 배치 중 **가장 많이 클리어**되는 시나리오 점수 (완벽 플레이 가정).

### 10.4 1×1 예외

**재시작 직후 / SessionEase 높음** (§12, 보류)일 때만 `1x1`을 allowedPool에 추가.

---

## 11. Pressure (의도적 유일수) 실시간 생성

### 11.1 목표

`countFullSequences == 1` 이고, **너무 쉬운 unique**는 제외.

### 11.2 Difficulty 점수 (의사코드)

full sequence 1개를 찾았을 때 그 시나리오의 난이도:

```text
difficulty(sequence):
  d = 0
  d += (uses large block like 3x3 in last step) ? w_bigFinish : 0
  d += (first two steps clear >= 1 line) ? w_setupClear : 0
  d += (tight pivot / few alternative pivots in step) ? w_tight : 0
  d -= (single obvious placement) ? penalty_easy : 0
  return d
```

**게이트:** `difficulty >= pressureDifficultyMin`

### 11.3 PressureGenerator

```text
function TryGeneratePressure(board, weights, tuning):
  if not pressureZoneGate(board, blame): return FAIL
  if random() > p_pressure: return FAIL

  candidates = empty
  repeat sampleCount times:
    combo = Sample3Shapes(normalPool, weights)  // 1x1 제외
    if UsesForcedAwkwardBlock(combo): continue

    pieces = AssignRotations(combo)
    if CountFullSequences(board, pieces, cap=2) != 1: continue
    d = DifficultyOfUniqueSequence(board, pieces)
    if d < pressureDifficultyMin: continue
    candidates.add({ pieces, d })

  if candidates empty: return FAIL
  return pick with preference for higher difficulty
```

### 11.4 Brilliant escape

- **트리거:** 플레이어가 **Pressure에서 준 3피스**로 라운드를 **무사히 완료**했을 때.
- Hospitality가 우연히 unique였을 때도 UI 문구는 띄울 **수** 있으나, **의도된 성취는 Pressure만** 카운트.
- 구현: `BlockSelectionResult.Intent == Pressure && turnCompleted` → 이벤트 또는 bool 훅 (UI는 범위 밖).

---

## 12. BoardHealth (복합 점수)

### 12.1 입력 지표

| 지표 | 계산 |
|------|------|
| `fillRate` | 점유 칸 / 64 |
| `deadZoneCount` | 1~3칸 크기의 **둘러싸인 빈 구역** 개수 (flood-fill) |
| `bigPieceSlots` | `3x3`, `1x5`가 들어갈 수 있는 (rotation 포함) 슬롯 수 |
| `placementFreedom` | 표준 테스트 피스 집합(예: 1x1 제외 16종) 각각의 **합법 배치 수** 평균 |

### 12.2 종합 점수

```text
healthScore =
    w_fill   * FillComponent(fillRate)      // 너무 비면 낮음, 너무 차면 낮음
  + w_dead   * (1 - normalize(deadZoneCount))
  + w_big    * normalize(bigPieceSlots)
  + w_free   * normalize(placementFreedom)

FillComponent(r):
  // Sweet spot ≈ 0.15~0.45 (Reddit 메타 ~25%)
  if r < 0.12: return r / 0.12 * 0.5      // TooEmpty 쪽
  if r > 0.55: return max(0, 1 - (r-0.55)/0.35)  // TooDirty 쪽
  return 1.0                                 // Sweet
```

### 12.3 구간 매핑

```text
if fillRate < tooEmptyFillMax OR healthScore < tooEmptyScoreMax:
  zone = TooEmpty
else if fillRate > tooDirtyFillMin OR healthScore < tooDirtyScoreMax:
  zone = TooDirty
else:
  zone = Sweet
```

**시작값 (튜닝 SO):**

| 키 | 권장 초기값 |
|----|------------|
| `tooEmptyFillMax` | 0.12 |
| `tooDirtyFillMin` | 0.55 |
| `tooEmptyScoreMax` | 0.35 |
| `tooDirtyScoreMax` | 0.40 |

---

## 13. BlameScore

### 13.1 올리는 이벤트 (턴 종료 시)

| 이벤트 | blame 증가 |
|--------|-----------|
| 새 dead zone 생성 (1~3칸) | +`blamePerDeadZone` (15~25) |
| 중앙 영역(3,3)-(4,4) 점유 증가 | +`blamePerCenterCell` (3~5) |
| bigPieceSlots 감소 (3x3 or 1x5 자리 막음) | +`blamePerBigSlotLost` (8~12) |
| placementFreedom 급감 (이전 대비 Δ) | +`blamePerFreedomDrop` × |Δ| |

**올리지 않음:** 점수, 콤보, 플레이 시간.

### 13.2 감쇠

```text
매 턴 종료:
  blame = blame * decayRate + turnBlameDelta
  // decayRate 권장 0.65~0.75
```

### 13.3 임계값 (시작값)

| 용도 | 키 | 권장 |
|------|-----|------|
| ComboBreak | `blameComboBreakThreshold` | 25 |
| Pressure 가중 | `blamePressureThreshold` | 35 |
| Trap | `blameTrapThreshold` | 55 |
| Easy (낮아야 함) | `easyBlameMax` | 15 |

---

## 14. 블록 가중치

### 14.1 목적

- Normal: 자연스러운 분포
- Hospitality / Pressure: **큰·긴 블록** 우선
- **억지 블록** 필요 시 해당 티어 포기

### 14.2 예시 테이블 (튜닝 SO)

| shapeId | Normal | Hospitality | Pressure |
|---------|-------:|------------:|---------:|
| 1x1 | 0 | 0* | 0 |
| 1x2 | 0 | 0 | 0 |
| 1x3 | 10 | 8 | 12 |
| 1x4 | 12 | 15 | 14 |
| 1x5 | 8 | 20 | 10 |
| 2x2 | 15 | 12 | 15 |
| 3x2 | 12 | 18 | 12 |
| 3x3 | 6 | 15 | 18 |
| L3~Z4, L3x3 | 10 each | 12 each | 12 each |
| Diag2, Diag3 | 8 | 6 | 10 |

\* 재시작 직후 Hospitality만 1x1 허용 시 가중 > 0

### 14.3 억지 블록 판정

```text
UsesForcedAwkwardBlock(combo):
  if combo contains 1x1 or 1x2: return true
  // 확장: 1x3만 3개인 조합 등도 튜닝으로 추가 가능
  return false
```

---

## 15. 번들 데이터

### 15.1 BlockBundleSO (제안)

```text
BlockBundleSO:
  string bundleId
  BundleTag tag          // Normal | Trap | ComboBreak
  string[] shapeIds[3]   // 슬롯 순서
  int weight             // Normal 가중치 (Trap/ComboBreak는 1이어도 됨)
```

### 15.2 BlockBundlePoolSO

```text
BlockBundlePoolSO:
  BlockBundleSO[] allBundles
  GetByTag(tag) → list
```

### 15.3 초기 번들 예시 (제작 시 채울 것)

**Normal (체감용):**

| bundleId | shapes |
|----------|--------|
| `normal_big` | 2x2, 3x2, 3x2 |
| `normal_mix` | L4, 2x2, 1x3 |
| `normal_long` | 1x4, 1x3, 2x2 |

**Trap (패턴 예시, 실제로는 보드 검증 필수):**

| bundleId | shapes | 의도 |
|----------|--------|------|
| `trap_oversize` | 3x3, 3x3, L3x3 | 큰 블록만 줘서 순서 함정 |

**ComboBreak (예시):**

| bundleId | shapes | 의도 |
|----------|--------|------|
| `cb_noclear` | 1x3, 1x3, 2x2 | 넣을 순 있으나 클리어 어려움 |

번들은 **에디터에서 수십 개** 늘리면서 플레이테스트로 조정.

### 15.4 Draw 시 번들 → 피스 변환

```text
for i in 0..2:
  canonical = shapeIds[i]
  rotation = RandomRotation()
  pieces[i] = { shapeId, RotateOffsets(canonical, rotation) }
```

---

## 16. 기존 코드 연동

### 16.1 현재 구조

| 파일 | 역할 |
|------|------|
| `AbstractDrawer.Draw(context, 3)` | 3개 `cellOffsets` 반환 |
| `BlockSupply.Fill(context)` | Drawer + Skin |
| `BlockSpawnContext` | `BlockShapeSourceSO`, `BoardGrid`, `Score` |
| `BlockSpawnBootstrap` | 턴 종료 시 `Fill` 호출 |
| `TurnService.IsGameOver` | 단일 피스 배치 가능 여부만 (순서 무시) |

### 16.2 변경 계획 (Phase 7)

1. `BlockSelectionDrawer : AbstractDrawer` 추가.
2. `BlockSpawnContext` 확장:
   - `BlameTracker Blame`
   - `BlockSelectionTuningSO Tuning`
   - `BlockBundlePoolSO Bundles`
   - `Score`는 **알고리즘에서 읽지 않음** (필드 유지해도 됨).
3. `BlockSpawnBootstrap`: `RandomDrawer` → `BlockSelectionDrawer`.
4. `BoardPlacementBootstrap` 또는 `TurnBootstrap`: 턴 종료 시 `BlameTracker.OnTurnEnded`.

### 16.3 BlockSelectionResult (로깅·디버그)

```text
BlockSelectionResult:
  Tier selectedTier
  string bundleId?          // 번들이면
  bool wasGenerated         // 실시간이면 true
  float healthScore
  HealthZone zone
  float blame
  bool isBrilliantEscapeCandidate  // Pressure intent
```

---

## 17. ScriptableObject 목록

| SO | 용도 |
|----|------|
| `BlockSelectionTuningSO` | 구간 경계, 확률, blame, 가중치, sampleCount |
| `BlockBundleSO` | 번들 1개 |
| `BlockBundlePoolSO` | 번들 모음 |
| `BlockShapeSourceSO` | shapeId → offsets (기존 PTY/JTH) |

---

## 18. 구현 Phase 맵

| Phase | 산출물 |
|-------|--------|
| **1** | 솔버 API (`Simulation/`) |
| **2** | BoardHealth, BlameTracker, TuningSO |
| **3** | BlockWeight, Bundle SO |
| **4** | BundleTierSelector (Trap, ComboBreak, Easy, Normal) |
| **5** | HospitalityGenerator |
| **6** | PressureGenerator + Orchestrator (전체 스택) |
| **7** | AbstractDrawer 연동, Bootstrap, 로그 |

의존: `1 → 2 → 3 → 4 → 5 → 6 → 7`

---

## 19. 검증 시나리오 (수동·로그)

| # | 시나리오 | 기대 |
|---|----------|------|
| 1 | 빈 보드 + Normal 번들 | `hasAny` true, `fullSequence` true |
| 2 | 거의 찬 보드 + Pressure 시도 | unique 후보 또는 fallthrough |
| 3 | TooEmpty + blame 높음 | 가끔 ComboBreak 번들 |
| 4 | TooDirty + blame 극高 + p 통과 | 극히 가끔 Trap |
| 5 | 예쁜 near-line 보드 | Hospitality 후보 score 높음 |
| 6 | 억지 1x2 필요 조합 | Hospitality/Pressure FAIL → Normal |
| 7 | 모든 티어 실패 | Fallback이 `hasAny` 만족 |

---

## 20. 로깅 (read_console)

매 리필마다 1줄:

```text
[BlockSelect] turn=12 zone=Sweet health=0.62 blame=18.4 tier=Normal bundle=normal_mix
[BlockSelect] turn=40 zone=TooDirty health=0.31 blame=42.1 tier=Pressure generated difficulty=0.71
```

---

## 21. 보류 / 미포함

| 항목 | 설명 |
|------|------|
| **SessionEase** | 빠른 게임오버 후 다음 세션 쉬움 — grill에서 미확정 |
| **1×1 재시작 접대** | Hospitality에서 `SessionEase` 연동 시 구현 |
| **Brilliant escape UI** | 이벤트 훅만. 문구·연출은 UI 담당 |
| **점수 기반 난이도** | 본 알고리즘 입력 아님 |
| **BlockBlastPoolSO** | `random-block-spawn` Phase 6. 없으면 `BlockShapeSourceSO` 직접 사용 |

---

## 22. 체크리스트 (구현 완료 정의)

- [ ] 솔버가 라인 클리어·연쇄를 반영한다
- [ ] 스폰 결과는 항상 `hasAny >= 1`
- [ ] Trap / ComboBreak는 게이트 + 확률 + 검증을 모두 통과할 때만
- [ ] Hospitality / Pressure는 억지 블록 시 포기한다
- [ ] Pressure만 의도적 unique다
- [ ] `Score` / 콤보가 선택기 입력에 쓰이지 않는다
- [ ] `read_console`로 턴마다 tier·health·blame을 확인할 수 있다

---

## 부록 A — 전체 의사코드 (Orchestrator)

```text
function SelectPieces(board, blame, tuning, bundles, rng):
  snapshot = BoardSnapshot.From(board)
  health = BoardHealthCalculator.Compute(snapshot)
  zone = health.zone

  // 1 Trap
  if zone == TooDirty and blame >= tuning.blameTrapThreshold:
    if rng() < tuning.p_trap:
      b = PickTrapBundle(snapshot, bundles, rng)
      if b != null: return Result(Trap, b)

  // 2 ComboBreak
  if zone == TooEmpty and blame >= tuning.blameComboBreakThreshold:
    if rng() < tuning.p_comboBreak:
      b = PickComboBreakBundle(snapshot, bundles, rng)
      if b != null: return Result(ComboBreak, b)

  // 3 Hospitality
  if rng() < tuning.p_hospitality:
    p = HospitalityGenerator.Try(snapshot, tuning, rng)
    if p != null: return Result(Hospitality, p)

  // 4 Easy
  if health.score < tuning.easyHealthThreshold and blame < tuning.easyBlameMax:
    b = PickEasyBundle(snapshot, bundles, tuning, rng)
    if b != null: return Result(Easy, b)

  // 5 Pressure
  if zone == TooDirty or health.score < tuning.pressureHealthThreshold:
    if rng() < tuning.p_pressure:
      p = PressureGenerator.Try(snapshot, tuning, rng)
      if p != null: return Result(Pressure, p)

  // 6 Normal
  b = PickNormalBundle(snapshot, bundles, tuning, rng)
  if b != null: return Result(Normal, b)

  // 7 Fallback
  p = FallbackGenerator.Try(snapshot, tuning, rng)
  if p != null: return Result(Fallback, p)

  return ForceNormalAny(snapshot, bundles, rng)
```

---

## 부록 B — 관련 파일 경로

```
Assets/_MemberWorkspace/JTH/
  Scripts/Domain/BlockSelection/     ← 신규 (솔버·메트릭·생성기)
  Scripts/Domain/Spawn/              ← BlockSpawnContext, Drawer
  Scripts/Data/BlockSelectionTuningSO.cs
  ScriptableObjects/BlockSelection/
  Docs/Implementations/block-selection-algorithm/
    SPEC.md          ← 이 문서
    phases.md        ← Phase 인덱스 (구현 시작 시 작성)
```

PTY 블록 정의: `Assets/_MemberWorkspace/PTY/ScriptableObjects/BlockShapes/` (17종)
