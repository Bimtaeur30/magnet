# Sequence — Phase 3 (block-selection-algorithm)

> **Phase:** [phase3.md](phase3.md) 와 1:1.

## 1 — 2026-08-01 · 블록 가중치 + 번들 SO

**바뀐 것**

- 생성: `Scripts/Domain/BlockSelection/Bundles/BundleTag.cs`
- 생성: `Scripts/Domain/BlockSelection/Bundles/WeightedShape.cs`
- 생성: `Scripts/Domain/BlockSelection/Bundles/ShapeSampler.cs`
- 생성: `Scripts/Data/BlockBundleSO.cs`
- 생성: `Scripts/Data/BlockBundlePoolSO.cs`
- 생성: `Scripts/Data/BlockShapeWeight.cs`
- 수정: `Scripts/Data/BlockSelectionTuningSO.cs` — `blockWeights` 리스트 추가
- 생성: `ScriptableObjects/BlockSelection/Bundles/` 번들 에셋 16종 + `ScriptableObjects/BlockSelection/BlockBundlePool.asset`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Bundles/BundleTag.cs`
  - 심볼: `BundleTag` — enum `Normal / Trap / ComboBreak / Relife` (추가)
    - 설명: 번들 용도 태그.
    - 이유: 티어 스택(SPEC §9)이 태그별로 후보를 걸러야 함. SO와 Domain이 공유하므로 Domain에 배치.
    - 영향: `BlockBundleSO.tag`, `BlockBundlePoolSO.GetByTag`, Phase 6 Orchestrator가 소비.

- 파일: `Scripts/Domain/BlockSelection/Bundles/WeightedShape.cs`
  - 심볼: `WeightedShape` — readonly struct (`CellOffsets`, `Weight`) (추가)
    - 설명: 실시간 생성용 추첨 항목. canonical offsets + 해당 티어 가중치 1개.
    - 이유: 생성기(Domain)가 SO 리스트를 직접 알 필요 없이 티어별 평탄화된 풀만 받게 (Domain 순수성).
    - 영향: Phase 5·6 생성기와 Orchestrator가 티어별 리스트로 구성.

- 파일: `Scripts/Domain/BlockSelection/Bundles/ShapeSampler.cs`
  - 심볼: `ShapeSampler.PieceCount` — const int 3 (추가)
    - 설명: 한 번에 뽑는 피스 수.
  - 심볼: `ShapeSampler.Sample3Rotated(pool, rng)` — public static (추가)
    - 설명: 중복 허용 가중 추첨 3회 + 각 피스 랜덤 회전(`ShapeRotator`). 유효 가중치 없으면 null.
    - 이유: Hospitality/Pressure/Fallback 샘플링 공통 루틴 (SPEC §10.3·§11.3). 가중치 0(1x1·1x2)은 애초에 안 뽑혀 억지 블록 필터를 대체.
  - 심볼: `ShapeSampler.PickWeighted(pool, totalWeight, rng)` — private static (추가)
    - 설명: 누적 가중 추첨 1회. 부동소수 오차 시 마지막 유효 항목 반환.
    - 이유: 룰렛 휠 표준 구현 — 오차로 null 반환하는 경계 제거.

- 파일: `Scripts/Data/BlockBundleSO.cs`
  - 심볼: `BlockBundleSO` — ScriptableObject, CreateAssetMenu "Magnet/Block Bundle" (추가)
    - 설명: 피스 3개 고정 조합 (SPEC §15.1). `bundleId`(로그용) / `tag` / `shapes`(BlockShapeSO 3개) / `weight`.
    - 이유: 번들은 에디터에서 수십 개 늘리며 플레이테스트로 조정하는 데이터 (SPEC §15.3). shapes는 스펙 리터럴(string shapeId) 대신 SO 직접 참조 — 조회 단계·오타 제거 (phase3.md 결정).

- 파일: `Scripts/Data/BlockBundlePoolSO.cs`
  - 심볼: `BlockBundlePoolSO.AllBundles` — 프로퍼티 (추가)
  - 심볼: `BlockBundlePoolSO.GetByTag(tag)` — 메서드 (추가)
    - 설명: 태그별 번들 목록. 최초 호출 시 Dictionary 캐시 구축, `OnValidate`에서 무효화.
    - 이유: 매 리필 태그 조회가 일어나므로 리스트 순회 반복 제거 (SPEC §15.2).

- 파일: `Scripts/Data/BlockShapeWeight.cs`
  - 심볼: `BlockShapeWeight` — [Serializable] class (`shape`, `normalWeight`, `hospitalityWeight`, `pressureWeight`) (추가)
    - 설명: 모양 1종의 티어별 가중치 (SPEC §14.2 표의 행 1개).
    - 이유: 티어마다 별도 테이블 대신 행 단위 편집 — 인스펙터에서 모양별로 한 줄씩 관리.

- 파일: `Scripts/Data/BlockSelectionTuningSO.cs`
  - 심볼: `BlockSelectionTuningSO.BlockWeights` — `IReadOnlyList<BlockShapeWeight>` (추가)
    - 설명: 모양 17종 가중치 테이블.
    - 이유: SPEC §17 — 가중치 전용 SO 없이 튜닝 SO의 일부.
    - 영향: Orchestrator 생성자가 티어별 `WeightedShape` 풀 구성.

- 에셋: `ScriptableObjects/BlockSelection/Bundles/` (16종) + `BlockBundlePool.asset`
  - Normal 10 (`normal_big`~`normal_diag`, weight 6~12), Trap 2 (`trap_oversize`, `trap_bulk`), ComboBreak 2 (`cb_noclear`, `cb_smallmix`), Relife 2 (`relife_gentle`, `relife_combo` — 1x1 포함).
  - `DefaultBlockSelectionTuning.asset`에 가중치 17행 기입 (1x1·1x2 = 전 티어 0, SPEC §14.2 예시 값).

**검증**

- `read_console` 컴파일 에러 0.
- 에셋 생성 execute_code 결과: bundles=16, pool ok, tuning weights=17.

**메모**

- `UsesForcedAwkwardBlock`(SPEC §14.3)은 별도 함수로 만들지 않음 — 가중치 0 배제가 동일 효과 (phase3.md 결정).
