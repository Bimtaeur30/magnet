# Phase 3 — 블록 가중치 + 번들 SO (Relife 포함)

> **구현:** `block-selection-algorithm` · **Sequence:** [sequence3.md](sequence3.md) · **스펙:** [SPEC.md](SPEC.md) §14·§15

## 목표

실시간 생성(Hospitality/Pressure/Fallback)용 **모양별 티어 가중치**와, 티어 스택이 소비할 **번들 데이터(SO)** 를 만든다. 가중 샘플러는 순수 Domain.

## 완료 조건

- [x] `BundleTag` — Normal / Trap / ComboBreak / Relife
- [x] `BlockBundleSO` / `BlockBundlePoolSO` — 번들 1개 / 태그별 캐시 조회
- [x] `BlockShapeWeight` + `BlockSelectionTuningSO.BlockWeights` — 모양 17종 × 티어 3종 가중치
- [x] `WeightedShape` / `ShapeSampler` — 가중 랜덤 3피스 + 랜덤 회전 (Domain)
- [x] 번들 에셋 16종 + `BlockBundlePool.asset` 생성 (Normal 10 · Trap 2 · ComboBreak 2 · Relife 2)
- [x] 컴파일 에러 0

## 설계 결정

| 결정 | 이유 |
|------|------|
| 번들 shapes는 `string shapeId[3]`(스펙 리터럴) 대신 `BlockShapeSO[3]` 직접 참조 | shapeId → offsets 조회 단계 제거, 에디터에서 드래그 앤 드롭, 오타 불가 |
| 가중치 테이블은 별도 SO가 아닌 `BlockSelectionTuningSO` 내 리스트 | SPEC §17 SO 목록에 가중치 전용 SO 없음 — 튜닝 수치의 일부 |
| 억지 블록(1x1·1x2)은 `UsesForcedAwkwardBlock` 함수 대신 **가중치 0으로 배제** | 샘플러가 0 가중치를 아예 안 뽑으므로 별도 필터가 불필요 — 죽은 코드 방지 |
| 샘플링은 중복 허용 | Block Blast는 같은 모양 3개도 정상 (`trap_oversize` 등 번들도 중복 사용) |

## 만진 파일

- `Scripts/Domain/BlockSelection/Bundles/BundleTag.cs` (신규)
- `Scripts/Domain/BlockSelection/Bundles/WeightedShape.cs` (신규)
- `Scripts/Domain/BlockSelection/Bundles/ShapeSampler.cs` (신규)
- `Scripts/Data/BlockBundleSO.cs` (신규)
- `Scripts/Data/BlockBundlePoolSO.cs` (신규)
- `Scripts/Data/BlockShapeWeight.cs` (신규)
- `Scripts/Data/BlockSelectionTuningSO.cs` (수정 — `blockWeights` 추가)
- `ScriptableObjects/BlockSelection/Bundles/*.asset` 16종 + `BlockBundlePool.asset` (신규)

## 범위 밖

티어 셀렉터(Phase 4), 생성기(Phase 5·6), Drawer 연동(Phase 7)
