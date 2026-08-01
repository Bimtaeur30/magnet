# Sequence — Phase 9 (block-selection-algorithm)

> **Phase:** [phase9.md](phase9.md) 와 1:1. 근거 데이터: `Docs/BLOCKBLAST_ANALYSIS.md`

## 1 — 2026-08-01 · 실게임 344프레임 분석 반영 (대각선 제거·중복 번들·Momentum·밀도 바이어스)

**바뀐 것**

- 수정: `Scripts/Domain/BlockSelection/Bundles/BundleTag.cs`
- 수정: `Scripts/Domain/BlockSelection/SelectionTier.cs`
- 수정: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
- 수정: `Scripts/Domain/Spawn/BlockSpawnContext.cs` · `Scripts/Domain/Spawn/BlockSelectionDrawer.cs`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs`
- 생성: 번들 에셋 7종 (`mom_bigsquares`·`mom_squarefeast`·`mom_bigtriple`·`mom_rects`·`normal_twinlines`·`normal_tripleL`·`normal_twinsquare`)
- 에셋: `DefaultBlockSelectionTuning.asset`·`BlockBundlePool.asset`·`normal_corner`·`cb_smallmix`
- 문서: `Docs/BLOCKBLAST_ANALYSIS.md`(관찰·결론) · `Docs/INSPECTOR_TOOLTIPS.md` · `Docs/BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Bundles/BundleTag.cs`
  - 심볼: `BundleTag.Momentum` — enum 값 (추가)
    - 설명: 클리어 흐름 유지용 큼직한 번들 태그.
    - 이유: 실게임 관찰 #4 — 콤보 흐름 중 큼직한 사각 패 지급. 기존 태그와 용도가 달라 분리.
    - 영향: `mom_*` 번들 4종이 이 태그, 오케스트레이터 Momentum 티어가 조회.

- 파일: `Scripts/Domain/BlockSelection/SelectionTier.cs`
  - 심볼: `SelectionTier.Momentum` — enum 값 (추가)
    - 설명: Hospitality와 Easy 사이 티어.
    - 이유: 진단 로그·UI 훅에서 티어 식별 필요.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `MomentumProbability` — 프로퍼티 (추가, 기본 0.6)
    - 설명: 직전 턴 클리어 시 Momentum 시도 확률.
    - 이유: 실게임도 매번은 아님 — 확률 튜닝 여지. 0이면 티어 끔.
  - 심볼: `DenseFillMin / DenseSlimBoost / SparseFillMax / SparseBigBoost` — 프로퍼티 4종 (추가, 기본 0.45/2/0.25/1.5)
    - 설명: 밀도 바이어스 문턱·배수 (빽빽→얇은 블록 번들 ↑, 널널→큰 블록 번들 ↑).
    - 이유: 실게임 관찰 #5 밀도 역상관 재현. 배수 1이면 끔.

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `_shapeTraits` — 필드 `Dictionary<BlockShapeSO,(bool slim,bool big)>` (추가)
    - 설명: 모양별 얇음(1xN)·큼(6칸+) 정적 특성. 생성자에서 1회 캐시.
    - 이유: 밀도 배수 계산을 매 추첨 반복하므로 offsets 재분석 방지.
  - 심볼: `ComputeTraits` — static 메서드 (추가)
    - 설명: offsets의 x 전부 동일 또는 y 전부 동일 → slim, 칸 수 ≥ 6 → big.
    - 이유: "얇은 조각"(빽빽한 판에서 빈틈에 들어감)·"큰 조각"(빈 판에서 시원함)의 판정 기준.
  - 심볼: `_fillRate` — 필드 (추가)
    - 설명: 이번 리필의 보드 채움률. `SelectPieces` 시작 시 health에서 복사.
    - 이유: 밀도 배수가 추첨 콜백(`DensityMultiplier`)에서 보드 상태를 참조해야 함.
  - 심볼: `SelectPieces` — 메서드 (수정)
    - 설명: `lastTurnClearedCells` 파라미터 추가(기본 0). Hospitality와 Easy 사이에 Momentum 게이트 삽입: 직전 턴 클리어 > 0 + zone ≠ TooDirty + 확률 통과 → Momentum 번들 Passable 추첨.
    - 이유: 실게임 관찰 #4. TooDirty 제외는 큰 블록을 줄 공간이 없는 판에 대형 번들을 강제하지 않기 위함.
  - 심볼: `CombinedMultiplier` — 메서드 (추가)
    - 설명: `SnugMultiplier × DensityMultiplier`. Normal 후보 수집·Easy·Momentum 추첨에 주입.
    - 이유: 부스트 2종(쏙·밀도)을 한 콜백으로 — 셀렉터 시그니처 유지.
  - 심볼: `DensityMultiplier` — 메서드 (추가)
    - 설명: fillRate > DenseFillMin이고 얇은 모양 포함 → DenseSlimBoost, fillRate < SparseFillMax이고 큰 모양 포함 → SparseBigBoost, 그 외 1.
    - 이유: 실게임 관찰 #5 — 판 상태에 맞는 크기의 조각이 오게.
  - 심볼: `TrySelectHealthiestNormal` — 메서드 (수정)
    - 설명: 후보 수집 배수를 `SnugMultiplier` → `CombinedMultiplier`로 교체.
    - 이유: Normal 추첨에도 밀도 바이어스 반영.

- 파일: `Scripts/Domain/Spawn/BlockSpawnContext.cs`
  - 심볼: `LastTurnClearedCells` — 프로퍼티 (추가)
    - 설명: 직전 턴 클리어로 사라진 칸 수 (0 = 클리어 없음).
    - 이유: Momentum 게이트 입력. 점수·콤보 대신 보드에서 유도한 값 — "점수는 입력 아님" 설계와 충돌 최소화.

- 파일: `Scripts/Domain/Spawn/BlockSelectionDrawer.cs`
  - 심볼: `Draw` — 메서드 (수정)
    - 설명: `context.LastTurnClearedCells`를 `SelectPieces`에 전달.
    - 이유: 배선.

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `Fill` — 메서드 (수정)
    - 설명: 턴 정산 시 `ComputeLastTurnClearedCells` 호출, 컨텍스트에 전달 (첫 리필은 0).
    - 이유: 클리어 발생 시점(턴 종료)과 리필 시점이 일치하는 유일한 곳.
  - 심볼: `ComputeLastTurnClearedCells` — 메서드 (추가)
    - 설명: `턴 시작 점유 + 직전 3피스 칸 수 − 현재 점유` (음수는 0 클램프). 배치 칸 수는 `_drawer.LastResult.Pieces`에서 합산.
    - 이유: 클리어 칸 수를 이벤트 의존 없이 보드 스냅샷만으로 유도 — 다른 시스템과 결합 없음.
  - 심볼: `CountOccupied` — static 메서드 (추가)
    - 설명: BoardGrid 점유 칸 수.
    - 이유: 위 계산 보조.
  - 심볼: `TierStyle` — 메서드 (수정)
    - 설명: Momentum → ("모멘텀(흐름 유지)", 금색) 케이스 추가.
    - 이유: 강조 로그 식별.

**에셋 변경 (실게임 분석 반영)**

- 모양 가중(Normal/Hosp/Pressure): **Diag2 `0/0/0` · Diag3 `0/0/0`** (344프레임 0건) · 3x3 `6→14` · 3x2 `12→14`
- 풀: `normal_diag` 제외, 신규 7종 등록
- 신규 번들: `mom_bigsquares`[3x3,3x3,2x2]w10 · `mom_squarefeast`[3x3,2x2,3x2]w10 · `mom_bigtriple`[3x3×3]w4 · `mom_rects`[3x2,3x2,1x4]w8 (tag Momentum) / `normal_twinlines`[1x4,1x4,L3]w8 · `normal_tripleL`[L3×3]w6 · `normal_twinsquare`[3x3,3x3,2x2]w6 (tag Normal)
- 구성 교체: `normal_corner` Diag2→L3(중복 L3×2) · `cb_smallmix` Diag2→1x3(중복 1x3×2)

**검증**

- `refresh_unity` 후 컴파일 에러 0 ✅
- 플레이 스모크: 첫 리필 `tier=Normal bundle=normal_twinsquare` — 빈 보드 대형 부스트(SparseBigBoost) 정상 작동 확인 ✅

**메모**

- 보류: 새 게임 프리필 보드(보드 초기화 소관), bag 가뭄 방지, 회전 지정 번들, Hospitality/Fallback 생성 티어의 밀도·쏙 부스트.
- Momentum 체감 과하면 `MomentumProbability` ↓ 또는 `mom_bigtriple` weight ↓.

## 2 — 2026-08-01 · Normal·Easy 티어 독립 추첨 전환 (고정 번들 폐기)

**바뀐 것**

- 수정: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
- 수정: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs`
- 삭제: `normal_*` 번들 에셋 13종 (+meta) · `BlockBundlePool.asset` 재작성 (특수 티어 10종만)
- 에셋: `DefaultBlockSelectionTuning.asset` (`NormalSampleTries` 추가)
- 문서: `BLOCKBLAST_ANALYSIS.md` §5 · `BLOCK_SELECTION_TUNING_GUIDE.md` · `Docs/INSPECTOR_TOOLTIPS.md`

**변경 상세 (왜/무엇)**

- 배경: 사용자 피드백 "지금 번들이 실게임 패랑 너무 다름". 344프레임 관찰상 실게임 핸드는 [J4,1x4,1x4]·[2x2,S4,1x3]·[S4,1x5,J4] 등 **자유 조합 + 중복**으로, 수작업 고정 번들 15종으로는 조합 공간을 재현 불가. 슬롯 3개 독립 가중 추첨이 관찰 데이터를 가장 잘 설명하는 생성 모델.

- 파일: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
  - 심볼: `IsValid` — public static 메서드 (추가)
    - 설명: 핸드(3피스) 단위 검증 공개 래퍼 (내부 `Validate` 위임).
    - 이유: 번들 없이 샘플한 핸드도 동일 솔버 검증(Passable/Easy/AnyPlaceable)을 타야 함.
  - 심볼: `TryPickCandidates` — 메서드 (삭제)
    - 이유: phase8에서 Normal Health 비교용으로 추가했으나 독립 추첨 전환으로 유일 호출처 소멸.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `NormalSampleTries` — 프로퍼티 (추가, 기본 12)
    - 설명: Normal·Easy 핸드 샘플 시도 예산 (검증 실패분 포함).
    - 이유: 검증 통과율이 보드 상태에 따라 달라 상한 필요 (성능 가드).

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `_normalEntries` — 필드 (추가)
    - 설명: (shape SO, offsets, normalWeight) 튜플 목록. SO 참조 보존.
    - 이유: 쏙·밀도 배수는 SO 키 딕셔너리(`_snugByShape`·`_shapeTraits`) 조회가 필요한데 기존 `WeightedShape`는 offsets만 가짐.
  - 심볼: `_dynamicNormalPool` — 필드 (추가) · `BuildDynamicNormalPool` — 메서드 (추가)
    - 설명: 매 리필마다 모양별 유효 가중 = normalWeight × 쏙 배수 × 밀도 배수로 풀 재구성 (`SelectPieces` 시작 시 1회).
    - 이유: 부스트를 번들 단위 → **모양 단위**로 이동 — 포켓에 맞는 그 모양이 직접 자주 나옴 (기존엔 그 모양이 든 번들 전체가 부스트).
  - 심볼: `ShapeMultiplier` — 메서드 (추가)
    - 설명: 모양 1개의 쏙 × 밀도 배수.
  - 심볼: `SampleValidHands` — 메서드 (추가)
    - 설명: `ShapeSampler.Sample3Rotated`(중복 허용)로 핸드를 뽑아 `IsValid` 통과분만 최대 N개 수집. 예산 `NormalSampleTries`.
  - 심볼: `TrySelectHealthiestNormal` — 메서드 (수정)
    - 설명: 후보 수집을 번들 추첨 → `SampleValidHands(Passable)`로 교체. 랭킹(예측 Health + 쏙 보너스)·결과는 `FromGenerated`(bundleId 없음 → 로그 `generated`).
  - 심볼: Easy 티어 블록 — (수정)
    - 설명: `TryPickBundle(Normal, Easy검증)` → `SampleValidHands(Easy, 1개)`.
  - 심볼: `ForceNormalAny` — 메서드 (수정)
    - 설명: Normal 번들 의존 제거 — `SampleValidHands(AnyPlaceable, 1개)` → 실패 시 기존 무검증 강제 샘플.
  - 참고: 번들 단위 `CombinedMultiplier`(쏙×밀도)는 Momentum 티어 전용으로 유지.

- 에셋:
  - 삭제 13종: normal_big·bigL·bigthree·corner·diag·lines·long·mix·tetro·zigzag (기존 10) + twinlines·tripleL·twinsquare (entry 1에서 신설한 Normal 태그 3종 — 독립 추첨이 중복 핸드를 자연 생성하므로 불필요).
  - `BlockBundlePool.asset`: trap 2·cb 2·relife 2·mom 4 = 10종만 등록.

**검증**

- 컴파일 에러 0 ✅ · 플레이 스모크: `turn=0 tier=Normal bundle=generated` — 독립 추첨 경로 정상 ✅

**메모**

- 이제 Normal 손패 구성은 `BlockWeights`의 normalWeight가 직접 결정 — 특정 모양 빈도 조절은 번들 편집이 아니라 가중표 수정.
- `BundleTag.Normal` enum 값은 유지 (직렬화 호환 — 태그 int 0).

## 3 — 2026-08-01 · L3 빈도·쏙 부스트 완화 (플레이 피드백)

**바뀐 것**

- 에셋: `DefaultBlockSelectionTuning.asset`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` (기본값·툴팁만, 로직 없음)
- 문서: `Docs/INSPECTOR_TOOLTIPS.md` · `BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 피드백: "작은 ㄱ자(L3)가 너무 많이 나옴" + "쏙 맞는 블록 포함이 너무 노골적".
- 원인: 두 문제가 결합 — L3는 3칸 코너라 중반 보드에서 둘레 막힘 ≥ 0.7 자리가 거의 항상 존재 → 쏙 부스트(×3)를 상시 수령 + Normal 랭킹 가산점 0.15로 후보 경쟁까지 우승 → 체감 빈도 폭증·노골적.
- 조치 (데이터):
  - L3 가중: normal 10 → **6**, hospitality 12 → **10** (pressure 12 유지).
  - `SnugEnclosureMin` 0.7 → **0.8**: L3 기준 둘레 8칸 중 7칸 이상 막힌 진짜 포켓만 쏙 판정 (개방 코너 자리 제외).
  - `SnugWeightBoost` 2 → **0.6**: 사방 밀폐 시 가중 ×3 → ×1.6.
  - `SnugNormalRankBonus` 0.15 → **0.06**: 예측 Health가 비슷할 때만 갈리는 타이브레이커 수준.
- 코드 기본값·툴팁을 에셋과 동기화 (신규 에셋 생성 대비).

**검증**

- 데이터·툴팁만 변경 — 로직 무변, 린트 0. (수치는 플레이 중 변경 즉시 반영되나 blockWeights는 씬 재시작 필요 — 가이드 §3 참고)

## 4 — 2026-08-01 · 데드존 처벌 완화 (플레이 피드백)

**바뀐 것**

- 에셋: `DefaultBlockSelectionTuning.asset`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` (기본값·툴팁만, 로직 없음)
- 문서: `Docs/INSPECTOR_TOOLTIPS.md` · `BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 피드백: "데드존이 너무 쌔게 때림".
- 원인: 데드존(1~3칸 고립 빈칸)이 3경로로 작용 — ① blame +20/개 (2개면 +40 → Pressure 문턱 35 즉시 돌파, Trap 55 근접), ② Hospitality 기회 점수 -0.15/개 (포켓 있으면 접대 실종), ③ healthScore -0.025/개 (경미). 1~3칸 포켓은 정상 플레이에서도 흔해 즉벌이 과함.
- 조치 (데이터):
  - `BlamePerDeadZone` 20 → **8**: 한 턴 2개 실수(+16)로도 ComboBreak 문턱(25) 미달. 상습(4~5개 누적)이어야 Pressure권.
  - `OpportunityDeadZonePenalty` 0.15 → **0.08**: 포켓 1~2개 있어도 접대 기회 살아 있음.
  - healthScore 성분(`DeadZoneWeight` 0.15)은 유지 — 판정 자체는 그대로, 처벌 강도만 완화.
- 코드 기본값·툴팁 동기화.

**검증**

- 데이터·툴팁만 변경 — 로직 무변, 린트 0. 수치 2종은 플레이 중 변경 즉시 다음 턴부터 반영.

## 5 — 2026-08-01 · mom_bigtriple 삭제 (플레이 피드백)

**바뀐 것**

- 삭제: `Bundles/mom_bigtriple.asset` (+meta) — [3x3,3x3,3x3] w4
- 에셋: `BlockBundlePool.asset` 등록 해제 (Momentum 번들 4종 → 3종)
- 문서: `BLOCK_SELECTION_TUNING_GUIDE.md` Momentum 절

**변경 상세 (왜/무엇)**

- 피드백: "3x3이 3개 나왔는데 이거 뭐임".
- 원인: 클리어 직후 Momentum 60% 발동 × 번들 가중 4/32(≈12.5%) → 클리어 턴의 ~7.5%가 3x3 트리플. 실게임 콤보 스트릭에서 관찰된 패턴이긴 하나 우리 체감상 과함.
- Momentum 잔여 3종(bigsquares·squarefeast·rects)은 유지 — 3x3 ×2까지가 상한.

## 6 — 2026-08-01 · 사각 러시 완화: Momentum 멀티라인 게이트 (플레이 피드백)

**바뀐 것**

- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` · `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
- 에셋: `DefaultBlockSelectionTuning.asset`
- 문서: `BLOCK_SELECTION_TUNING_GUIDE.md` · `Docs/INSPECTOR_TOOLTIPS.md`

**변경 상세 (왜/무엇)**

- 피드백: "계속 네모난 블럭이 나오면서 점수 먹기가 너무 쉬워짐".
- 원인: 3중 결합 — ① Momentum 게이트가 "클리어 1칸 이상 + 60%"라 잘 풀리는 판에선 거의 매 턴 발동 → 큰 사각 → 또 클리어 (양성 루프), ② 클리어 직후 빈 보드에 SparseBigBoost ×1.5로 Normal에서도 대형 부스트 중첩, ③ 가중표 상위가 전부 사각(2x2 15·3x2 14·3x3 14 = 추첨의 ~33%).
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `MomentumMinClearedCells` — 프로퍼티 (추가, 기본 10)
    - 설명: Momentum 발동에 필요한 직전 턴 최소 클리어 칸 수. 한 줄 = 8칸 → 10이면 멀티라인급 턴에서만.
    - 이유: 실게임의 "콤보 스트릭 중 큰 패"는 폭발 구간 전용 — 단일 클리어마다 주는 건 과보상.
  - 심볼: `MomentumProbability` — 기본값 0.6 → 0.4 (수정).
- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `SelectPieces` — Momentum 게이트 (수정)
    - 설명: `lastTurnClearedCells <= 0` → `< _tuning.MomentumMinClearedCells`. trace에 칸 수·문턱 표기.
- 에셋: MomentumProbability 0.4 · MomentumMinClearedCells 10 · SparseBigBoost 1.5 → 1.3 · normalWeight 2x2 15→12, 3x2 14→12, 3x3 14→12.

**검증**

- 컴파일 에러 0. blockWeights 변경분은 씬 재시작 필요.

## 7 — 2026-08-01 · 같은 모양 트리플 금지 (플레이 피드백)

**바뀐 것**

- 수정: `Scripts/Domain/BlockSelection/Bundles/ShapeSampler.cs`
- 문서: `BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 피드백: "2x2가 3개 나옴 — 같은 블럭 3개는 안 나오게".
- 원인: 독립 추첨은 중복 무제한 — 낮은 확률(모양당 ~0.1%/핸드)이지만 리필이 잦아 체감 발생. 번들 쪽 트리플은 이미 전부 삭제된 상태.
- 파일: `Scripts/Domain/BlockSelection/Bundles/ShapeSampler.cs`
  - 심볼: `Sample3Rotated` — 메서드 (수정)
    - 설명: 앞 두 슬롯이 같은 모양일 때 마지막 슬롯이 또 같으면 재추첨 (최대 `TripleRejectRetries`=8회, 초과 시 허용 — 풀이 사실상 1종인 극단 케이스 가드). 페어(×2)는 그대로 허용.
    - 이유: 실게임도 페어는 일상·트리플은 희귀. 모든 생성 경로(Normal·Easy·Pressure·Hospitality·Fallback·최후 수단)가 이 샘플러를 공유하므로 단일 지점 수정으로 전체 커버.
  - 심볼: `TripleRejectRetries` — 상수 (추가, 8)

**검증**

- 컴파일 에러 0.

## 8 — 2026-08-01 · Health 개선 시 blame 차감 (플레이 피드백)

**바뀐 것**

- 수정: `Scripts/Domain/BlockSelection/Blame/BlameTracker.cs` · `Scripts/Domain/BlockSelection/Blame/TurnFeedback.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` · `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
- 에셋: `DefaultBlockSelectionTuning.asset`
- 문서: `Docs/INSPECTOR_TOOLTIPS.md` · `BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 피드백: "Health가 늘어났으면 Blame도 어느 정도 줄어들어야 하는 거 아님?".
- 기존: blame은 실수(데드존·중앙 점유 등)로만 증가, 감소는 시간 감쇠(×0.7)뿐 — 판을 적극 개선해도 보상 없음.
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `BlameHealthGainRelief` — 프로퍼티 (추가, 기본 30)
    - 설명: healthScore 증가 1.0당 blame 차감량 (+0.1 개선 = -3).
- 파일: `Scripts/Domain/BlockSelection/Blame/BlameTracker.cs`
  - 심볼: `OnTurnEnded` — 메서드 (수정)
    - 설명: `healthGainRelief = max(0, healthAfter.Score - healthBefore.Score) × BlameHealthGainRelief`. 누적식이 `Total = max(0, Total × decay + delta - relief)`로 변경 (0 미만 방지 클램프 추가).
    - 이유: 실수 벌점(delta)과 별개 축으로 "잘한 플레이" 능동 보상 — GoodTurn 판정(delta 기준)은 불변.
- 파일: `Scripts/Domain/BlockSelection/Blame/TurnFeedback.cs`
  - 심볼: `HealthGainRelief` — 프로퍼티 (추가) + 생성자 파라미터 추가.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `LogBlameChange` — 메서드 (수정): 사유에 "Health 개선 보상 → -x" 추가.

**검증**

- 컴파일 에러 0.

## 9 — 2026-08-01 · 자유도 벌점 완화 + Health 보상 강화 (플레이 피드백)

**바뀐 것**

- 에셋: `DefaultBlockSelectionTuning.asset`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` (기본값·툴팁만, 로직 없음)
- 문서: `Docs/INSPECTOR_TOOLTIPS.md` · `BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 피드백: "배치 자유도 하락 감점이 너무 심함" + "Health 개선 보상이 너무 약함".
- 원인: 배치 자유도는 클리어 없는 턴이면 3피스(~12칸) 배치만으로 10~30 자연 하락 → ×0.5 = +5~15 blame. 평범한 플레이 자체가 벌점이라 GoodTurn(delta ≤ 5)도 거의 불가능. Health 보상(×30)은 통상 개선폭(+0.05~0.15)에서 -1.5~-4.5로 자유도 벌점에 묻힘.
- 조치 (데이터):
  - `BlamePerFreedomDrop` 0.5 → **0.15**: 자연 하락 30이어도 +4.5 — GoodTurn 가능권.
  - `BlameHealthGainRelief` 30 → **60**: +0.1 개선 = -6, 자유도 벌점과 균형.
- 코드 기본값·툴팁 동기화.

**검증**

- 데이터·툴팁만 변경 — 로직 무변. 두 값 모두 플레이 중 변경 즉시 다음 턴부터 반영.

## 10 — 2026-08-01 · 새 게임 프리필 보드 (Block Blast 재현)

**바뀐 것**

- 신규: `Scripts/Domain/Board/BoardPrefillGenerator.cs`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs` · `Scripts/Presentation/PlacedBlocksView.cs` · `Scripts/Presentation/GameBoard.cs` · `Scripts/Data/BlockSelectionTuningSO.cs`
- 에셋: `DefaultBlockSelectionTuning.asset` (Prefill 3필드) · `Prefabs/Board/Placed Blocks View.prefab` (blockItemSO 참조)

**변경 상세 (왜/무엇)**

- 피드백: "맨 먼저 채워지는 게 없는 것도 블록 블라스트랑 너무 다름". 분석 결론 6: 실게임 새 게임은 45~60% 프리필 보드로 시작.
- 파일: `Scripts/Domain/Board/BoardPrefillGenerator.cs`
  - 심볼: `Generate` — 정적 메서드 (신규)
    - 설명: 내부 작업 그리드에서 무작위 조각(회전 4방향 포함)을 목표 채움률까지 배치. 라인 완성 배치·dead zone(≤3칸 고립 빈칸) 생성 배치는 거부(되돌림). 반환은 조각별 절대 셀 목록 — 조각 단위 스킨용.
    - 이유: 프리필이 시작부터 클리어되거나 벌점 요인(dead zone)을 만들면 안 됨.
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `PrefillFillMin`(0.4) · `PrefillFillMax`(0.5) · `PrefillMaxAttempts`(200) — 프로퍼티 (추가). Max ≤ 0이면 프리필 끔.
- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `blockItemSO` — 필드 (추가, ShapeBlock과 동일 풀 아이템 에셋)
  - 심볼: `CreatePrefillBlocks` — 메서드 (신규): 풀 Pop + `BlockCreatedEvent` 발행(스킨 등록) 후 Block 목록 반환.
- 파일: `Scripts/Presentation/GameBoard.cs`
  - 심볼: `PrefillPiece` — 메서드 (신규): `CreatePrefillBlocks` + 기존 `AddBlock`(점유+배치) 재사용.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `PrefillBoard` — 메서드 (신규): `Start()`에서 첫 `Fill()` 직전 실행. min~max 사이 무작위 목표 채움률 → 생성 → 조각별 `SkinSession.DrawSkinIds` 스킨 → `PrefillPiece`. `[Prefill]` 로그 1줄.
  - 심볼: `_skinSession` — 필드 (추가): 프리필 스킨 추첨에 재사용 (기존 인라인 생성을 필드로 승격).
  - 이유: 풀·스킨 참조를 부트스트랩에 새로 직렬화하지 않고 JTH 소유 프리팹(`Placed Blocks View`)에만 추가 — 타 워크스페이스 씬(KTJ) 수정 불필요.

**검증**

- 컴파일 에러 0. 첫 Fill의 health 계산은 프리필 이후 보드 기준으로 동작 (첫 턴부터 밀도 바이어스 반영).

## 11 — 2026-08-01 · 프리필 산포 방식 전환 (스크린샷 피드백)

**바뀐 것**

- 수정: `Scripts/Domain/Board/BoardPrefillGenerator.cs` · `Scripts/Bootstrap/BlockSpawnBootstrap.cs` (호출부) · `Scripts/Data/BlockSelectionTuningSO.cs` (기본값·툴팁)
- 에셋: `DefaultBlockSelectionTuning.asset` (PrefillFillMin 0.4→0.3 · Max 0.5→0.4)

**변경 상세 (왜/무엇)**

- 피드백: "사진처럼 듬성듬성 많이 해줘야지 지금 너무 이상함. 실제 블럭을 배치하지 않아도 상관 없는데" + 실게임 스크린샷 1장.
- 원인: entry 10은 실제 게임 조각(3x3 포함)을 인접 제한 없이 빽빽이 채워 한 덩어리로 뭉침 — 실게임 프리필은 소형 덩어리들이 떨어져 흩어진 형태.
- 파일: `Scripts/Domain/Board/BoardPrefillGenerator.cs`
  - 심볼: `Clumps` — 정적 배열 (신규): 프리필 전용 소형 덩어리 7종 (2x2 가중 ×2 · 3x2 · 1x2 · 1x3 · L3 · 1x1). 게임 조각과 분리.
  - 심볼: `TouchesExisting` — 메서드 (신규): 기존 점유와 상하좌우 인접하는 배치 거부 — 덩어리 간 간격 보장 (듬성듬성).
  - 심볼: `Generate` — 시그니처 변경: `pieces` 파라미터 제거 (내장 Clumps 사용).
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `PrefillFillMin` 0.4 → **0.3** · `PrefillFillMax` 0.5 → **0.4**: 산포 방식은 인접 금지로 고밀도 도달 불가 + 스크린샷 실측 ~35%.

**검증**

- execute_code로 시드 고정 3샘플 ASCII 출력 — 분리된 소형 덩어리 산포 확인 (결과 31~41%).
- 플레이 스모크: `[Prefill] target=36% result=36% pieces=10` (평균 2.3칸/덩어리).

## 12 — 2026-08-01 · 프리필 밀도 상향 (인접 확률 허용)

**바뀐 것**

- 수정: `Scripts/Domain/Board/BoardPrefillGenerator.cs` · `Scripts/Bootstrap/BlockSpawnBootstrap.cs` (호출부) · `Scripts/Data/BlockSelectionTuningSO.cs`
- 에셋: `DefaultBlockSelectionTuning.asset` (Min 0.3→0.4 · Max 0.4→0.5 · AdjacencyChance 0.4 신규)

**변경 상세 (왜/무엇)**

- 피드백: "너무 안빽빽한데" — entry 11의 전면 인접 금지는 최대 ~40%가 한계 + 균일한 간격이라 성김.
- 파일: `Scripts/Domain/Board/BoardPrefillGenerator.cs`
  - 심볼: `Generate` — 시그니처 변경: `adjacencyChance` 파라미터 추가. 기존 덩어리와 붙는 배치를 전면 거부 대신 **이 확률로 허용** — 큰 뭉치와 간격이 공존.
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `PrefillAdjacencyChance` — 프로퍼티 (추가, 0.4): 0 = 전부 분리(듬성듬성 최대), 1 = 제한 없음(한 덩어리).
  - 심볼: `PrefillFillMin` 0.3 → **0.4** · `PrefillFillMax` 0.4 → **0.5** (실게임 관찰 45~60% 복귀).

**검증**

- 시드 고정 4샘플 ASCII — 결과 44~50%, 부분 병합 + 간격 공존 확인.
- 플레이 스모크: `[Prefill] target=49% result=48% pieces=12`.

## 13 — 2026-08-01 · 프리필 역방향(포켓 파내기) 전환 + 빽빽할 때 큰 블록 감점

**바뀐 것**

- 재작성: `Scripts/Domain/Board/BoardPrefillGenerator.cs`
- 수정: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs` · `Scripts/Data/BlockSelectionTuningSO.cs`
- 에셋: `DefaultBlockSelectionTuning.asset` (PrefillFill 0.45/0.55 · DenseFillMin 0.45→0.38 · DenseBigPenalty 0.45 신규)

**변경 상세 (왜/무엇)**

- 피드백: "이렇게 채워놨으면 작은 블럭을 줘야 하는데 너무 큰 블럭을 줌" + "원래(실게임)랑 비교하면 빠진 곳이 이어져 있고 많음".
- 원인 1: 덩어리 산포(entry 12)는 빈 곳이 1칸 폭 실뱀 통로로 남음 — 실게임 프리필은 채움이 통짜, 빈 곳은 조각이 들어갈 네모 포켓 몇 개.
- 원인 2: DenseFillMin 0.45가 프리필 실측(44~50%)과 경계가 겹쳐 얇은 블록 부스트가 미발동 + 큰 블록엔 감점 자체가 없었음.
- 파일: `Scripts/Domain/Board/BoardPrefillGenerator.cs` (전면 재작성)
  - 심볼: `Generate` — **가득 찬 보드에서 빈 포켓을 파내는 역방향**으로 전환. `Pockets`(2x2·3x2 가중 ×2 · 3x3 · 1x4 — 전부 4칸+라 dead zone 원천 차단)를 목표 빈칸 수까지 캐빙. `adjacencyChance`는 이제 "빈 포켓끼리 합침 허용 확률".
  - 심볼: `BreakCompleteLines` — 메서드 (신규): 캐빙 후 남은 완성 행/열을 2x2 캐빙으로 깬다 (스폰 즉시 클리어 방지). 이 추가 캐빙 때문에 실측이 목표보다 3~5%p 낮음 → 기본 목표 0.45~0.55로 보정.
  - 심볼: `PartitionFilled` — 메서드 (신규): 채움 셀을 BFS로 3~5칸 연결 그룹으로 분할 — 그룹 단위 스킨(여러 조각을 쌓은 듯한 색 다양성).
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `DenseBigPenalty` — 프로퍼티 (추가, 0.45): 빽빽(fillRate > DenseFillMin)할 때 큰 블록(6칸+)에 곱하는 배수. Normal·Easy 모양 추첨과 번들 추첨(`DensityMultiplier`) 공통.
  - 심볼: `DenseFillMin` 0.45 → **0.38**: 프리필 시작 밀도부터 발동.
- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `ShapeMultiplier` · `DensityMultiplier` — 수정: 빽빽 분기에서 slim 부스트와 big 감점을 **동시 적용** (기존은 slim 부스트 단독).

**검증**

- 시드 고정 3샘플 ASCII — 결과 45~47%, 통짜 채움 + 네모 포켓 확인.
- 플레이 스모크: `[Prefill] target=48% result=47% pieces=9`, turn=0 zone=Sweet health=0.71 — 첫 턴부터 밀도 바이어스 발동 (0.47 > 0.38).

## 14 — 2026-08-01 · 프리필 라인 깨기 제거 (밀도 우선)

**바뀐 것**

- 수정: `Scripts/Domain/Board/BoardPrefillGenerator.cs` · `Scripts/Data/BlockSelectionTuningSO.cs` (툴팁·기본값)
- 에셋: `DefaultBlockSelectionTuning.asset` (PrefillAdjacencyChance 0.4→0.3)

**변경 상세 (왜/무엇)**

- 피드백: "시작하자마자 클리어 되도 상관 없으니까 빽빽하게 — 위 사진이랑 같은 밀도로". 사진 실측 ~45%인데 라인 깨기 캐빙이 3~5%p를 깎아 실측 40~42%가 잦았음.
- 파일: `Scripts/Domain/Board/BoardPrefillGenerator.cs`
  - 심볼: `BreakCompleteLines` · `TryFindCompleteLine` — 메서드 (삭제): 완성 라인을 그대로 둠 — 첫 배치 때 클리어 허용 (사용자 확인). 결과 채움률이 목표에 근접 (45~55%).
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `PrefillAdjacencyChance` 0.4 → **0.3**: 빈 포켓 분리를 강화 — 사진처럼 또렷한 구멍 몇 개.

**검증**

- 시드 고정 4샘플 ASCII — 결과 45~55% (완성 라인 존치 확인).
- 플레이 스모크: `[Prefill] target=50% result=45% pieces=9`.

## 15 — 2026-08-01 · 프리필 전체 제거 (사용자 결정)

**바뀐 것**

- 삭제: `Scripts/Domain/Board/BoardPrefillGenerator.cs`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs` · `Scripts/Presentation/GameBoard.cs` · `Scripts/Presentation/PlacedBlocksView.cs` · `Scripts/Data/BlockSelectionTuningSO.cs`
- 에셋: `DefaultBlockSelectionTuning.asset` (Prefill 4필드 제거) · `Prefabs/Board/Placed Blocks View.prefab` (blockItemSO 참조 제거)

**변경 상세 (왜/무엇)**

- 피드백: "그냥 처음에 채우는 거 싹 다 없애줘" — entry 10~14의 프리필 기능 전면 철회. 빈 보드 시작으로 복귀.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `PrefillBoard` — 메서드 (삭제) · `_skinSession` 필드 (삭제, 인라인 생성 복귀) · `Start()`는 `Fill()`만 호출.
- 파일: `Scripts/Presentation/GameBoard.cs` — 심볼: `PrefillPiece` (삭제).
- 파일: `Scripts/Presentation/PlacedBlocksView.cs` — 심볼: `CreatePrefillBlocks` · `blockItemSO` 필드 (삭제).
- 파일: `Scripts/Data/BlockSelectionTuningSO.cs` — 심볼: `PrefillFillMin` · `PrefillFillMax` · `PrefillAdjacencyChance` · `PrefillMaxAttempts` (삭제).
- 유지: `DenseBigPenalty` · `DenseFillMin`(0.38) — 프리필과 무관한 밀도 바이어스 기능이라 존치 (꽉 찬 판에서 큰 블록 억제).

**검증**

- 컴파일 에러 0.
