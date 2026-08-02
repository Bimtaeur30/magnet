# Sequence — Phase 8 (block-selection-algorithm)

> **Phase:** [phase8.md](phase8.md) 와 1:1.

## 1 — 2026-08-01 · 클러스터 Health 성분 + Normal Health 지향 선택 + 대각선·1x3 빈도 하향

**바뀐 것**

- 수정: `Scripts/Domain/BlockSelection/Health/BoardHealthCalculator.cs`
- 수정: `Scripts/Domain/BlockSelection/Health/BoardHealthResult.cs`
- 수정: `Scripts/Domain/BlockSelection/Simulation/SequenceOutcomeEstimator.cs`
- 수정: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
- 수정: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs`
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
- 에셋: `DefaultBlockSelectionTuning.asset` · `Bundles/normal_diag·normal_corner·normal_mix·normal_bigL.asset`
- 문서: `Docs/INSPECTOR_TOOLTIPS.md` · `Docs/BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Health/BoardHealthResult.cs`
  - 심볼: `BoardHealthResult.ClusterCount / LargestClusterSize` — 프로퍼티 2종 (추가)
    - 설명: 점유 칸의 직교 연결 덩어리 수 / 가장 큰 덩어리 칸 수.
    - 이유: 클러스터 성분의 진단 데이터 노출 — 로그·튜닝 판단에 필요.
    - 영향: `BlockSpawnBootstrap.LogHealthChange` 소비.
  - 심볼: `BoardHealthResult` 생성자 — (수정)
    - 설명: clusterCount·largestClusterSize 파라미터 추가.
    - 이유: readonly struct라 생성 시점 주입.

- 파일: `Scripts/Domain/BlockSelection/Health/BoardHealthCalculator.cs`
  - 심볼: `AnalyzeClusters` — static 메서드 (추가)
    - 설명: 점유 칸을 상하좌우 연결 기준으로 플러드필해 (덩어리 수, 최대 덩어리 크기, 점유 칸 수)를 계산. 대각선 인접은 같은 덩어리로 치지 않음.
    - 이유: 피드백 명세 — "대각선 연결 제외, 통째로 모여 있으면 좋고 그 덩어리가 크면 더 좋게".
  - 심볼: `FloodFillOccupiedRegion` — static 메서드 (추가)
    - 설명: 점유 칸 대상 BFS 플러드필 (기존 `FloodFillEmptyRegion`의 점유 버전).
    - 이유: dead zone용 빈칸 플러드필과 조건이 반대(IsOccupied)라 별도 메서드.
  - 심볼: `ClusterComponent` — static 메서드 (추가)
    - 설명: 응집도 `1 − (덩어리수−1)/(점유수−1)`(한 덩어리=1, 전부 흩어짐=0)와 최대 덩어리 크기 정규화(`/ClusterSizeNormalizeMax`)를 `ClusterCohesionShare` 비율로 합산. 빈 보드는 1(중립).
    - 이유: "모여 있음"과 "그 덩어리 크기"를 각각 0~1로 만들어 다른 성분과 같은 스케일로 합산.
  - 심볼: `ComputeScore` — static 메서드 (수정)
    - 설명: `ClusterWeight × clusterComponent` 항 추가, 시그니처에 클러스터 값 3종 전달.
    - 이유: 클러스터 상태가 healthScore를 올리고 내릴 수 있어야 함 (피드백 명세).
  - 심볼: `Compute` — static 메서드 (수정)
    - 설명: `AnalyzeClusters` 호출 후 결과를 Score 계산·`BoardHealthResult`에 전달.
    - 이유: 성분 추가에 따른 배선.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `ClusterWeight` — 프로퍼티 (추가, 기본 0.2)
    - 설명: healthScore에서 클러스터 성분 가중치.
    - 이유: 성분 비중을 SO에서 튜닝 가능하게 (다른 성분과 동일 패턴).
  - 심볼: `ClusterCohesionShare` — 프로퍼티 (추가, 기본 0.5, Range 0~1)
    - 설명: 클러스터 성분 내 응집도 비중 (나머지는 최대 덩어리 크기 비중).
    - 이유: "모임" vs "덩어리 크기" 체감 배분을 플레이테스트에서 조정 가능하게.
  - 심볼: `ClusterSizeNormalizeMax` — 프로퍼티 (추가, 기본 20)
    - 설명: 최대 덩어리 크기를 0~1로 정규화할 상한 칸 수.
    - 이유: 8×8(64칸)에서 20칸 한 덩어리면 충분히 "잘 모임"으로 만점 처리.
  - 심볼: `FillWeight / DeadZoneWeight / BigSlotWeight / FreedomWeight` — 기본값 (수정: 0.4/0.2/0.2/0.2 → 0.35/0.15/0.15/0.15)
    - 설명: 클러스터 0.2가 들어오며 성분 합 1.0 유지 재배분.
    - 이유: zone 문턱(TooEmptyScoreMax 등)이 절대값 기준이라 총합 스케일 보존 필수.
  - 심볼: `NormalHealthCandidateCount` — 프로퍼티 (추가, 기본 4)
    - 설명: Normal 티어에서 결과 Health를 비교할 통과 후보 번들 수. 1이면 기존 가중 랜덤과 동일.
    - 이유: Health 지향 선택의 강도(후보 폭)와 성능을 SO에서 조정 가능하게.

- 파일: `Scripts/Domain/BlockSelection/Simulation/SequenceOutcomeEstimator.cs`
  - 심볼: `SequenceOutcome.FinalBoard` — 프로퍼티 (추가)
    - 설명: 최선(클리어 최다) 경로 종료 시의 보드. 완주 실패면 null.
    - 이유: Normal 후보의 "플레이 후 Health" 예측에 최종 보드가 필요 — 기존엔 클리어 수만 반환.
  - 심볼: `SequenceOutcome` 생성자 / `Estimate` — (수정)
    - 설명: finalBoard 파라미터 추가, 완주 시 `frontier[0].Board` 전달.
    - 이유: 빔이 이미 들고 있는 보드를 노출만 하면 됨 — 추가 계산 없음.

- 파일: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
  - 심볼: `TryPickCandidates` — static 메서드 (추가)
    - 설명: `TryPick`과 같은 가중 랜덤·검증 루프를 돌되, 첫 성공에서 멈추지 않고 probeCount 안에서 최대 maxCandidates개 통과 후보를 수집해 반환 (없으면 빈 리스트).
    - 이유: Normal 티어가 후보 간 Health 비교를 하려면 복수 후보가 필요. 가중 랜덤 순서 유지로 번들 가중치 존중.
    - 영향: `BlockSelectionOrchestrator.TrySelectHealthiestNormal` 소비.

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `_freedomProbePieces` — 필드 (추가)
    - 설명: 후보 Health 예측 시 배치 자유도 계산에 쓸 프로브 피스 (Bootstrap과 동일 집합).
    - 이유: `BoardHealthCalculator.Compute`가 프로브 피스를 요구 — 예측 Health가 실제 다음 턴 Health와 같은 기준이 되게.
  - 심볼: 생성자 — (수정)
    - 설명: `freedomProbePieces` 파라미터 추가.
    - 이유: 프로브 집합의 출처(1x1 제외 규칙)는 Bootstrap 소관 — Domain은 받기만 함.
  - 심볼: `SelectPieces` — 메서드 (수정)
    - 설명: 티어 6 Normal 분기를 `TrySelectHealthiestNormal` 호출로 교체. null이면 기존과 동일하게 Fallback으로.
    - 이유: Normal을 "BoardHealth가 잘 나오는 Normal"로 (피드백 명세 — 응징은 상위 티어 몫).
  - 심볼: `TrySelectHealthiestNormal` — 메서드 (추가)
    - 설명: 통과 가능 후보 최대 `NormalHealthCandidateCount`개 수집 → 각각 `PredictHealthAfterBestPlay` → 최고 점수 번들 확정. 선택 이유에 후보 수·예측 Health 기록.
    - 이유: 가중 랜덤 유지(후보 수집 순서)하되 최종 결정만 Health 기준 — 무작위성과 건강 지향의 절충.
  - 심볼: `PredictHealthAfterBestPlay` — 메서드 (추가)
    - 설명: `SequenceOutcomeEstimator.Estimate`로 최선 플레이 후 보드를 얻어 `BoardHealthCalculator.Compute`의 Score 반환. 빔이 완주 못 찾으면 float.MinValue.
    - 이유: "이 3피스를 주면 판이 어떻게 되나"를 실제 health 공식으로 평가 — 별도 휴리스틱 이중화 방지.

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `Awake` — 메서드 (수정)
    - 설명: 오케스트레이터 생성 시 `_probePieces` 전달.
    - 이유: 생성자 시그니처 변경 배선.
  - 심볼: `LogHealthChange` — 메서드 (수정)
    - 설명: 덩어리 수·최대 덩어리 크기 변화 사유 2종 추가.
    - 이유: 클러스터 성분이 Hp 증감의 원인일 때 로그로 식별 가능해야 튜닝 가능.

**에셋 변경 (빈도 조정)**

- `DefaultBlockSelectionTuning.asset`
  - Health 가중치 0.35/0.15/0.15/0.15 + Cluster 0.2 (합 1.0), `ClusterCohesionShare 0.5` `ClusterSizeNormalizeMax 20` `NormalHealthCandidateCount 4`
  - 모양 가중치(Normal/Hospitality/Pressure): 1x3 `10/8/12 → 5/5/8` · Diag2 `8/6/10 → 4/3/5` · Diag3 `8/6/10 → 3/2/4`
- 번들: `normal_diag` weight `6 → 2` · `normal_corner`(Diag2 포함) weight `8 → 5`
- 번들 구성: `normal_mix`의 1x3 → T4 · `normal_bigL`의 1x3 → 1x4
  - 결과: Normal 번들 추첨에서 1x3 노출 ~46% → ~26%, 대각선 노출 ~15% → ~8%

**검증**

- `refresh_unity`(force + compile) 후 `read_console` 컴파일 에러 0 ✅

**메모**

- Normal Health 선택 비용: 후보 4개 × (빔 서치 + health 1회) — 리필당 1회라 허용 범위. 렉 시 `NormalHealthCandidateCount` ↓.
- 빔(`OutcomeBeamWidth 4`)이 완주 경로를 못 찾는 후보는 최저점 처리 — Passable 검증(솔버)과 빔의 탐색 범위 차이로 드물게 발생 가능.

## 2 — 2026-08-01 · 쏙 맞춤(Snug Fit) 부스트 — 포켓에 맞는 블록 등장 확률 상승

**바뀐 것**

- 생성: `Scripts/Domain/BlockSelection/Generation/SnugFitScorer.cs`
- 수정: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
- 수정: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs`
- 에셋: `DefaultBlockSelectionTuning.asset`
- 문서: `Docs/INSPECTOR_TOOLTIPS.md` · `Docs/BLOCK_SELECTION_TUNING_GUIDE.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Generation/SnugFitScorer.cs`
  - 심볼: `SnugFitScorer.Directions` — static 필드 (추가)
    - 설명: 둘레 검사용 4방향 오프셋.
    - 이유: 대각선은 "쏙" 체감과 무관 — 직교 인접만 검사 (피드백 명세).
  - 심볼: `SnugFitScorer.BestEnclosureAnyRotation` — static 메서드 (추가)
    - 설명: 4회전 각각 `BestEnclosure`를 구해 최댓값 반환. 1.0 도달 시 조기 종료.
    - 이유: 번들 추첨 시점엔 회전이 미정(추첨 후 랜덤 회전)이라 "어떤 회전으로든 쏙 자리가 있는가"가 기준.
    - 영향: `BlockSelectionOrchestrator.BuildSnugScores` 소비.
  - 심볼: `SnugFitScorer.BestEnclosure` — static 메서드 (추가)
    - 설명: 주어진 방향 그대로 전 pivot 합법 배치를 훑어 최고 둘레 막힘 비율 반환 (합법 배치 없으면 0).
    - 이유: Normal 후보 랭킹은 실제 회전 상태의 피스로 판정해야 함 — 회전 불일치 후보에 보너스를 주면 거짓 쏙.
    - 영향: `BlockSelectionOrchestrator.BestSnugOfPieces` 소비.
  - 심볼: `SnugFitScorer.EnclosureRatio` — static 메서드 (추가)
    - 설명: 배치된 피스 밖 인접 칸 중 벽(보드 밖)·점유 칸 비율. 사방 밀폐 = 1.0, 위만 뚫린 포켓 ≈ 0.75.
    - 이유: "사방 막힘이 제일 좋고, 위만 뚫려도 좌우하 막히면 쏙" — 비율 하나로 두 경우를 연속적으로 표현.
  - 심볼: `SnugFitScorer.IsPieceCell` — static 메서드 (추가)
    - 설명: 검사 칸이 피스 자신의 칸인지 판정.
    - 이유: 피스 내부 인접은 둘레가 아님 — 분모에서 제외.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `SnugEnclosureMin` — 프로퍼티 (추가, 기본 0.7, Range 0~1)
    - 설명: 쏙 판정 최소 둘레 막힘 비율. 미만이면 보너스 0.
    - 이유: 어중간하게 막힌 자리(0.5 등)까지 부스트하면 항상 발동해 의미 상실 — "위만 뚫림(≈0.75)"부터 잡히게 기본 0.7.
  - 심볼: `SnugWeightBoost` — 프로퍼티 (추가, 기본 2)
    - 설명: 쏙 모양 보유 번들의 추첨 가중 배수 증가폭 — 사방 밀폐 시 가중 ×3.
    - 이유: "패에 포함될 확률이 높아져야 함"의 직접 구현부. 0이면 기능 끔.
  - 심볼: `SnugNormalRankBonus` — 프로퍼티 (추가, 기본 0.15)
    - 설명: Normal 후보 랭킹(예측 Health)에 더하는 쏙 보너스 상한.
    - 이유: 추첨 확률만 올리면 후보에 들고도 Health 비교에서 탈락 가능 — 예측 Health가 비슷할 때 쏙 후보가 이기게 마지막 단계도 보정.

- 파일: `Scripts/Domain/BlockSelection/Tiers/BundleTierSelector.cs`
  - 심볼: `TryPick / TryPickCandidates` — 메서드 (수정)
    - 설명: 선택 파라미터 `Func<BlockBundleSO, float> weightMultiplier` 추가 (기본 null = 기존 동작).
    - 이유: 번들 SO의 정적 weight에 보드 상태 의존 배수를 곱하려면 추첨기가 외부 판단을 받아야 함 — 셀렉터는 보드 분석을 몰라야 해서 함수 주입.
  - 심볼: `TakeWeighted` — 메서드 (수정)
    - 설명: 합산·추첨 모두 `EffectiveWeight` 사용.
    - 이유: 배수 반영 지점 일원화.
  - 심볼: `EffectiveWeight` — static 메서드 (추가)
    - 설명: `max(1, round(weight × multiplier))`. multiplier null이면 기존 `max(1, weight)`.
    - 이유: int 가중 추첨 유지하며 배수 적용, 0 가중 방지.

- 파일: `Scripts/Domain/BlockSelection/BlockSelectionOrchestrator.cs`
  - 심볼: `_snugByShape` — 필드 `Dictionary<BlockShapeSO, float>` (추가)
    - 설명: 이번 리필 기준 모양별 쏙 점수 캐시. `SelectPieces` 시작 시 갱신.
    - 이유: 같은 모양이 여러 번들에 있으므로 모양당 1회만 계산 (17모양 × 4회전 × 64pivot — 리필당 1회 허용 범위).
  - 심볼: `SelectPieces` — 메서드 (수정)
    - 설명: 시작부에 `BuildSnugScores(snapshot)` 호출. Easy 티어 `TryPickBundle`에 `SnugMultiplier` 전달.
    - 이유: 쏙 점수는 스냅샷 기준 — 이후 티어 전부가 같은 판단 공유. Easy도 Normal 번들 풀을 쓰므로 동일 부스트.
  - 심볼: `BuildSnugScores` — 메서드 (추가)
    - 설명: `_tuning.BlockWeights`의 모양 전부에 `BestEnclosureAnyRotation` 계산해 캐시.
    - 이유: 번들이 참조하는 모양 SO와 같은 인스턴스 키로 조회하기 위해 튜닝 테이블을 모양 목록의 출처로 사용.
  - 심볼: `SnugMultiplier` — 메서드 (추가)
    - 설명: 번들 3모양 중 최고 쏙 점수로 `1 + SnugWeightBoost × NormalizedSnug` 배수 반환.
    - 이유: "쏙 블록이 든 패"의 추첨 확률 상승 — 피드백의 직접 요구.
    - 영향: Normal `TryPickCandidates`·Easy `TryPickBundle`에 주입.
  - 심볼: `NormalizedSnug` — 메서드 (추가)
    - 설명: `SnugEnclosureMin` 미만 0, 1.0(사방 밀폐)에서 1로 선형 정규화.
    - 이유: 문턱 이하 무시 + "밀폐일수록 더 좋게"의 연속 스케일.
  - 심볼: `BestSnugOfPieces` — static 메서드 (추가)
    - 설명: 후보 3피스(실제 회전 상태) 중 최고 `BestEnclosure`.
    - 이유: 랭킹 보너스는 실제 손에 들어올 회전 기준이어야 정직 — 회전 불일치면 자연히 낮게 나옴.
  - 심볼: `TrySelectHealthiestNormal` — 메서드 (수정)
    - 설명: 후보 수집에 `SnugMultiplier` 적용, 랭킹을 `예측Health + SnugNormalRankBonus × NormalizedSnug(실제 피스)`로 확장. 선택 이유에 쏙 보너스 표기.
    - 이유: 수집(확률)과 최종 선택(랭킹) 두 단계 모두 보정해야 체감으로 이어짐.
  - 심볼: `TryPickBundle` — 메서드 (수정)
    - 설명: 선택 파라미터 `weightMultiplier` 추가해 셀렉터로 전달.
    - 이유: Easy 티어 배선.

**에셋 변경**

- `DefaultBlockSelectionTuning.asset`: `SnugEnclosureMin 0.7` · `SnugWeightBoost 2` · `SnugNormalRankBonus 0.15`

**검증**

- `refresh_unity`(force + compile) 후 `read_console` 컴파일 에러 0 ✅

**메모**

- Trap·ComboBreak·Relife에는 부스트 미적용 (의도된 응징·접대 로직 왜곡 방지). Hospitality·Fallback(모양 가중 실시간 생성)도 미적용 — 체감 부족하면 `ShapeSampler` 가중에 쏙 배수 적용이 다음 후보.
- 번들에 쏙 모양이 있어도 랜덤 회전이 포켓과 안 맞을 수 있음 — Normal은 랭킹(실제 회전 기준)이 걸러주고, Easy는 확률만 오름.
