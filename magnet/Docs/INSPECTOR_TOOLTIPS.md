# Inspector 직렬화 필드 Tooltip

팀 공용 — `[SerializeField]` 이름만으로 역할이 불명확한 필드에 `[Tooltip]`을 붙이고, 문구를 이 문서에 기록한다.

> 코드 식별자·API 이름은 English, Tooltip 문구는 **한국어**.

> **v0.7 (2026-07-21):** `BoardSnapConfigSO`, `BoardRotationConfigSO`, `ExplosionBorderConfigSO`, `BlockedRingDimConfigSO`, 자석 축 관련 Tooltip은 **deprecated** (Block Blast 피벗). 신규 필드는 §JTH v0.7 참고.

---

## 규칙

### Tooltip을 붙이는 경우

| 상황 | 예 |
|------|-----|
| 도메인 용어·약어 | `cellsPerSide`, `stagingYExtraBelow` |
| 일반명·축약 | `config`, `cellsRoot`, `linesRoot` |
| 단위·범위가 이름에 없음 | `cellFill` (0.1~1 비율) |
| 자동 생성·선택적 할당 | `linesRoot`, `cellsRoot` (비우면 런타임 생성) |
| 역할이 코드 맥락 없이는 모호 | `stagingBlockView` |

### Tooltip을 생략하는 경우

| 상황 | 예 |
|------|-----|
| 타입+이름으로 충분 | `BoardConfigSO boardConfig`, `EventChannelSO magnetGameChannel` |
| 일반 Unity 관례·자명한 속성 | `cellSize`, `lineWidth`, `pieceColor`, `cellColor` |

### 구현

- `[Tooltip("…")]`을 `[SerializeField]` **바로 위**에 배치
- 필드 추가·이름 변경·Tooltip 수정 시 **아래 표도 갱신**
- 개인 `sequenceN.md`에는 변경 요약만, **전체 목록은 이 문서가 소스 오브 트루스**

---

## 필드 목록 (멤버별)

### JTH v0.7 (Block Blast — Phase 구현 시 Tooltip 추가)

| 파일 | 필드 | Tooltip |
|------|------|---------|
| `Scripts/Data/BoardConfigSO.cs` | `boardSize` | 8×8 격자 한 변 칸 수. Block Blast 기본 8 |
| `Scripts/Data/BlockBlastPoolSO.cs` | `shapes` | Block Blast 표준 polyomino 목록 (1x1~Z4). 인게임 추첨 풀 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `TooEmptyFillMax` | fillRate가 이 값 미만이면 TooEmpty 구간 (권장 0.12) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `TooDirtyFillMin` | fillRate가 이 값 초과면 TooDirty 구간 (권장 0.55) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `TooEmptyScoreMax` | fill 구간 판정 후, healthScore가 이 값 미만이면 TooEmpty (권장 0.35) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `TooDirtyScoreMax` | fill 구간 판정 후, healthScore가 이 값 미만이면 TooDirty (권장 0.40) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `FillDirtyFalloff` | TooDirtyFillMin 초과 시 fill 성분이 0까지 떨어지는 fillRate 폭. 0.35면 fill 0.90에서 0 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `FillWeight` | healthScore에서 fillRate 성분 가중치 (성분 합 1 권장) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `DeadZoneWeight` | healthScore에서 dead zone(고립 빈칸 1~3) 성분 가중치 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BigSlotWeight` | healthScore에서 큰 블록(3x3·1x5) 슬롯 성분 가중치 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `FreedomWeight` | healthScore에서 배치 자유도(테스트 피스 평균 합법 배치 수) 성분 가중치 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `ClusterWeight` | healthScore에서 클러스터(점유 칸 직교 연결 응집도·최대 덩어리 크기) 성분 가중치 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `DeadZoneNormalizeMax` | dead zone 개수를 0~1로 정규화할 상한. 이 개수 이상이면 성분 0 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BigSlotNormalizeMax` | 큰 블록 슬롯 수 정규화 상한. 빈 8×8 보드 = 100 (3x3 36 + 1x5 가로·세로 64) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `FreedomNormalizeMax` | 배치 자유도(피스당 평균 합법 배치 수, 회전 포함) 정규화 상한. 빈 보드 기준 ≈100 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `ClusterCohesionShare` | 클러스터 성분에서 응집도(한 덩어리로 모임) 비중. 나머지는 최대 덩어리 크기 비중 (권장 0.5) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `ClusterSizeNormalizeMax` | 최대 덩어리 크기를 0~1로 정규화할 상한 칸 수. 이 이상 모여 있으면 크기 성분 만점 (권장 20) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlamePerDeadZone` | 턴 종료 시 새 dead zone 1개당 blame 증가량. 1~3칸 포켓은 흔한 플레이라 과하면 응징 남발 (권장 5~12) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlamePerCenterCell` | 중앙 2×2 영역 새 점유 칸 1개당 blame 증가량 (권장 3~5) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlamePerBigSlotLost` | 큰 블록(3x3·1x5) 슬롯 수가 줄어든 턴에 1회 가산되는 blame (권장 8~12) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlamePerFreedomDrop` | 배치 자유도 감소량 1당 blame 증가량. 클리어 없는 턴은 자유도가 자연 하락(10~30)하므로 높으면 평범한 플레이도 벌점 (권장 0.1~0.2) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlameHealthGainRelief` | healthScore 증가 1.0당 blame 차감량. 판을 개선한 턴은 실수 벌점을 상쇄 — +0.1 개선이면 -6 (권장 40~80, 0이면 끔) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlameDecayRate` | 매 턴 종료 시 누적 blame에 곱하는 감쇠율 (권장 0.65~0.75) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlameComboBreakThreshold` | ComboBreak 티어 게이트: blame이 이 값 이상 (권장 25) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlamePressureThreshold` | Pressure 가중 게이트: blame이 이 값 이상 (권장 35) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BlameTrapThreshold` | Trap 티어 게이트: blame이 이 값 이상 (권장 55) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `EasyBlameMax` | Easy 티어 게이트: blame이 이 값 미만이어야 함 (유저 탓 아님, 권장 15) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `GoodTurnBlameDeltaMax` | GoodTurn 판정: 3피스 전부 배치 + 이번 턴 blame delta가 이 값 이하 (권장 5) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `blockWeights` | 모양별 티어 추첨 가중치 테이블 (SPEC §14.2). 1x1·1x2는 전 티어 0 권장 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `RelifeTurnCount` | Relife(재시작 접대) 티어가 적용되는 재시작 세션 첫 턴 수 (권장 1~2) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `TrapProbability` | Trap 티어 발동 확률 (게이트 통과 후, 권장 0.005~0.01) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `ComboBreakProbability` | ComboBreak 티어 발동 확률 (게이트 통과 후, 권장 0.03~0.05) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `EasyHealthThreshold` | Easy 티어 게이트: healthScore가 이 값 미만이면 판이 험함으로 판정 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `BundleProbeCount` | 티어 하나가 번들 검증(솔버)을 시도할 최대 번들 수. 초과 시 fallthrough |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `NormalHealthCandidateCount` | Normal 티어에서 결과 BoardHealth를 비교할 통과 후보 핸드 수. 1이면 단순 가중 랜덤과 동일 (권장 3~5) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `NormalSampleTries` | Normal·Easy 티어 독립 추첨 핸드의 최대 샘플 시도 횟수. 검증 실패분 포함 (권장 10~16) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `MomentumProbability` | Momentum(큼직한 기분 좋은 패) 티어 시도 확률. 높으면 클리어→큰 사각→또 클리어 양성 루프로 점수가 쉬워짐 (권장 0.3~0.5, 0이면 끔) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `MomentumMinClearedCells` | Momentum 발동에 필요한 직전 턴 최소 클리어 칸 수. 한 줄 = 8칸이므로 10이면 멀티라인급 턴에서만 발동 (권장 9~16) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `DenseFillMin` | fillRate가 이 값 초과(빽빽)면 얇은 블록 부스트 + 큰 블록 감점 적용 (권장 0.38~0.45) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `DenseSlimBoost` | 빽빽한 보드에서 얇은 블록 포함 번들에 곱하는 배수 (권장 1.5~2.5, 1이면 끔) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `DenseBigPenalty` | 빽빽한 보드에서 큰 블록(6칸+)에 곱하는 배수 (0~1). 꽉 찬 판에 3x3·3x2가 쏟아지는 것 방지 (권장 0.3~0.6, 1이면 끔) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `SparseFillMax` | fillRate가 이 값 미만(널널)이면 큰 블록(6칸 이상) 포함 번들의 추첨 가중 배수 적용 (권장 0.25) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `SparseBigBoost` | 널널한 보드에서 큰 블록 포함 번들에 곱하는 배수 (권장 1.3~2, 1이면 끔) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `SnugEnclosureMin` | 쏙 판정 최소 둘레 막힘 비율. 위만 뚫린 포켓 ≈ 0.75, 사방 밀폐 = 1.0. 이 미만이면 보너스 없음. 낮으면 작은 조각이 상시 부스트돼 노골적 (권장 0.8) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `SnugWeightBoost` | 쏙 맞는 모양의 추첨 가중 증가폭. 사방 밀폐 시 가중 ×(1+이 값). 크면 노골적 (권장 0.5~1, 0이면 끔) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `SnugNormalRankBonus` | Normal 후보 랭킹에 더하는 쏙 보너스 상한 (healthScore 스케일). 예측 Health가 비슷할 때만 갈리는 수준 권장 (권장 0.05~0.1) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `HospitalityProbability` | opportunity 게이트 통과 후 Hospitality를 실제로 시도할 확률 (변덕, 권장 0.7~0.85) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OpportunityHighThreshold` | opportunityScore가 이 값 이상이어야 Hospitality 시도 (권장 0.65~0.75) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `HospitalitySampleCount` | Hospitality 후보 3피스 조합 샘플 횟수 (권장 50~200) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `HospitalityMinQualityClears` | Hospitality 후보 최소 품질: 완벽 플레이 시 총 클리어 라인 수 하한 (억지 올클 차단) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OpportunityNearLineWeight` | 한 칸 부족한 행·열 1개당 opportunityScore 가산 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OpportunityMultiLineBonus` | 한 칸 부족한 행·열이 2개 이상일 때 추가 가산 (멀티라인 잠재) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OpportunityAllClearWeight` | 올클리어 잠재 가산: fillRate가 하한 이하 + dead zone 0일 때 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OpportunityAllClearFillMax` | 올클리어 잠재로 판정하는 fillRate 상한 (권장 0.2) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OpportunityBigSlotWeight` | 큰 블록 슬롯 성분 가중치: 정규화된 bigPieceSlots × 이 값 가산 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OpportunityDeadZonePenalty` | dead zone 1개당 opportunityScore 감점 (억지 패널티). 과하면 포켓 있을 때 접대가 안 나옴 (권장 0.05~0.1) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `OutcomeBeamWidth` | 최선 결과 추정(빔 서치) 폭. 클수록 정확하지만 느림 (권장 4~8) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `PressureProbability` | Pressure 게이트 통과 후 실제로 시도할 확률 (100% 아님) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `PressureHealthThreshold` | TooDirty가 아니어도 healthScore가 이 값 미만이면 Pressure 게이트 통과 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `PressureSampleCount` | Pressure 후보 3피스 조합 샘플 횟수 (유일수 판정은 비싸므로 보수적으로) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `PressureDifficultyMin` | 유일해 난이도가 이 값 미만이면 버림 (너무 쉬운 unique 제외) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `PressureBigFinishWeight` | 난이도 가산: 유일해의 마지막 스텝이 큰 블록일 때 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `PressureSetupClearWeight` | 난이도 가산: 유일해의 앞 두 스텝에서 라인 클리어가 필요할 때 |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `PressureBigFinishMinCells` | '큰 블록'으로 치는 최소 칸 수 (1x5·3x3·L3x3 = 5칸 이상) |
| `Scripts/Data/BlockSelectionTuningSO.cs` | `FallbackSampleCount` | Fallback 실시간 조합 샘플 횟수 |
| `Scripts/Data/BlockShapeWeight.cs` | `shape` | 가중치를 적용할 블록 모양 |
| `Scripts/Data/BlockShapeWeight.cs` | `normalWeight` | Normal 티어(번들 외 실시간 Fallback 포함) 가중치. 0이면 제외 |
| `Scripts/Data/BlockShapeWeight.cs` | `hospitalityWeight` | Hospitality(접대) 실시간 생성 가중치. 큰·긴 블록일수록 높게 |
| `Scripts/Data/BlockShapeWeight.cs` | `pressureWeight` | Pressure(의도적 유일수) 실시간 생성 가중치 |
| `Scripts/Data/BlockBundleSO.cs` | `bundleId` | 로그·디버그용 번들 식별자 (예: normal_big) |
| `Scripts/Data/BlockBundleSO.cs` | `tag` | 번들 용도 태그. 티어 스택이 태그별로 후보를 거른다 |
| `Scripts/Data/BlockBundleSO.cs` | `shapes` | 슬롯 0,1,2에 대응하는 블록 모양 3개. 1x1은 Relife 태그에서만 허용 |
| `Scripts/Data/BlockBundleSO.cs` | `weight` | 같은 태그 안에서의 가중 랜덤 추첨 가중치 (Trap/ComboBreak는 1이어도 됨) |
| `Scripts/Data/BlockBundlePoolSO.cs` | `allBundles` | 전체 번들 목록. 태그 무관하게 전부 등록 |
| `Scripts/Bootstrap/BlockSpawnBootstrap.cs` | `selectionTuningSO` | 블록 선택 알고리즘 수치 튜닝 SO |
| `Scripts/Bootstrap/BlockSpawnBootstrap.cs` | `bundlePoolSO` | 티어별 번들 모음 SO |

### JTH — `Assets/_MemberWorkspace/JTH/` (v0.6 · 일부 deprecated)

| 파일 | 필드 | Tooltip |
|------|------|---------|
| `Scripts/Data/BoardConfigSO.cs` | `cellsPerSide` | *(deprecated v0.7)* 자석 중심(0,0)에서 한쪽 끝까지의 칸 수 |
| `Scripts/Data/BlockVisualConfigSO.cs` | `stagingYExtraBelow` | 보드 하단에서 스테이징 영역까지 추가로 내릴 칸 수. stagingY = -(CellsPerSide + 이 값) |
| `Scripts/Data/BlockVisualConfigSO.cs` | `cellFill` | 블록 칸 스프라이트가 격자 칸 대비 차지하는 비율(0.1~1). 1이면 칸과 동일 크기 |
| `Scripts/Data/BlockVisualConfigSO.cs` | `previewAlpha` | 드래그 중 보드 격자 프리뷰(고스트) 블록 알파(0~1). 스테이징에는 적용되지 않음 |
| `Scripts/Data/BlockDragConfigSO.cs` | `sensitivityRampPerUnit` | Press 시작 포인터 X와의 거리(월드 유닛) 1당 블록 이동 배율 증가량. Block Blast식 감도 램프 |
| `Scripts/Data/BlockDragConfigSO.cs` | `sensitivityMaxMultiplier` | 드래그 감도 배율 상한. 1이면 램프 없음 |
| `Scripts/Data/BlockSnapConfigSO.cs` | `duration` | 손 놓은 뒤 Y축 자석 스냅: 칸 1칸 이동에 걸리는 시간(초). 이동 칸 수에 비례 |
| `Scripts/Data/BlockSnapConfigSO.cs` | `ease` | Place 성공 후 Y 스냅 LitMotion 이징 (Ease enum) |
| `Scripts/Data/BoardRotationConfigSO.cs` | `duration` | 폭발 처리 후 보드·블록 90° 회전 LitMotion 시간(초) |
| `Scripts/Data/BoardRotationConfigSO.cs` | `preRotationDelay` | 재조립 연출이 끝난 뒤 회전 시작 전 대기(초) |
| `Scripts/Data/BoardRotationConfigSO.cs` | `ease` | 보드 90° 회전 시 칸 View 이동 LitMotion 이징 (Ease enum) |
| `Scripts/Data/ClearReassemblyRuleConfigSO.cs` | `corridorHalfWidth` | 원점–원래칸 직선 수선 반폭(격자). 복도 안만 후보. 막히면 제자리(보드밖 주차 없음) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `originalCellColor` | 노랑 와이어 — 원래 칸(originalCell) 위치 |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `seedOccupiedColor` | 주황/살몬 채움 — 시드 점유 칸(seedOccupied) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `candidateColor` | 민트 초록 와이어 — 복도 안 이동 가능 후보 칸 |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `chosenColor` | 하늘 파랑 채움 — TryFind가 고른 최종 목표 칸(안쪽만) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `stayColor` | 회색 채움 — 안쪽이 막혀 원래 칸에 제자리 |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `corridorColor` | 흰 선 — 원점~원래칸 수선 복도(CorridorHalfWidth) |
| `Scripts/Presentation/CellRelocationTargetGizmo.cs` | `axisColor` | 노란 축 선 — 원점(0,0) → 원래 칸 방향 |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `bounceCells` | 폭발 후 바깥으로 튕기는 거리(칸) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `bounceDuration` | 튕김 LitMotion 시간(초) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `bounceEase` | 튕김(바깥으로 밀려남) LitMotion 이징 (Ease enum) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `landDuration` | 착지(목표 칸 이동) 시간(초) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `landEase` | 착지(목표 칸으로 이동) LitMotion 이징 (Ease enum) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `spinDegreesPerSecond` | 비행 중 자전 각속도(도/초) |
| `Scripts/Data/ClearReassemblyMotionConfigSO.cs` | `staggerPerRing` | 다음 링 시작 지연(초). 같은 링은 동시 이동. 이전 링 완료를 기다리지 않음 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `duration` | 폭발 테두리 펄스 LitMotion 시간(초) |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `peakScale` | 테두리 기준 크기 대비 최대 배율. 1이면 크기 변화 없음 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `sizeEase` | 펄스 크기 LitMotion 이징. t는 alpha와 동일, Ease만 다름 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `alphaEase` | 펄스 알파 LitMotion 이징. t는 크기와 동일, Ease만 다름 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `maxAlpha` | 펄스 최대 알파(0~1). 기본색 알파에 곱함 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `color` | 폭발 테두리 LineRenderer 색 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `lineWidth` | 폭발 테두리 LineRenderer 두께 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `sortingOrder` | 폭발 테두리 LineRenderer sortingOrder |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `shakeAmplitude` | 클리어 시 Cinemachine Impulse 카메라 쉐이크 좌우 진폭(월드 유닛). 0이면 쉐이크 없음 |
| `Scripts/Data/ExplosionBorderConfigSO.cs` | `shakeDuration` | 클리어 시 카메라 쉐이크 지속 시간(초). 짧게 유지 |
| `Scripts/Data/ScoreConfigSO.cs` | `BaseMin` | 세션 base 랜덤 하한(포함). ScoreSession 시작·Reset 시 한 번 추출 |
| `Scripts/Data/ScoreConfigSO.cs` | `BaseMax` | 세션 base 랜덤 상한(포함) |
| `Scripts/Presentation/BoardView.cs` | `config` | 격자 크기·색상 등 보드 시각화 설정 |
| `Scripts/Presentation/BoardView.cs` | `linesRoot` | 격자·자석 축 LineRenderer의 부모 Transform. 비우면 자동 생성 |
| `Scripts/Presentation/BoardView.cs` | `placementConfigPreview` | 폭발 테두리 ContextMenu 프리뷰용 PlacementConfig. 비우면 씬/에셋에서 자동 탐색 |
| `Scripts/Presentation/ShapeBlock.cs` | `blockPrefab` | 블록 칸 1개 프리팹(Block 컴포넌트 + SpriteRenderer). 필요 개수만큼 인스턴스 생성 후 재사용 |
| `Scripts/Presentation/Block.cs` | `spriteMask` | 칸 스킨 클리핑용. SetSortingOrder에서 Custom Range로 인접 마스크와 격리 |
| `Scripts/Data/BlockedRingDimConfigSO.cs` | `dimMultiply` | 비활성(막힌) 테두리 링 점유 칸 RGB 배수. 1=변화 없음 |
| `Scripts/Input/BlockDragDrawer.cs` | `shapeBlockPrefab` | 스테이징·프리뷰 표시용 ShapeBlock 프리팹. Awake에서 2개 Instantiate |

### KTJ — `Assets/_MemberWorkspace/KTJ/`

_(아직 등록된 Tooltip 없음)_

### PTY — `Assets/_MemberWorkspace/PTY/`

_(아직 등록된 Tooltip 없음)_

### PMS — `Assets/_MemberWorkspace/PMS/`

_(아직 등록된 Tooltip 없음)_

---

## 변경 이력

| 날짜 | 내용 |
|------|------|
| 2026-07-09 | JTH 7개 필드 Tooltip 추가 및 팀 문서 최초 작성 |
| 2026-07-09 | JTH Phase 3 — 감도 램프·프리뷰 뷰 Tooltip 3개 추가 |
| 2026-07-16 | JTH Block.spriteMask Tooltip 추가 (SpriteMask Custom Range 격리) |
| 2026-07-21 | v0.7 Block Blast 피벗 — deprecated 안내, BlockBlastPoolSO·boardSize 예정 |
| 2026-07-24 | JTH ScoreConfigSO — BaseMin/BaseMax (구 kTiers·SoftCap 제거) |
