# sequence23 — Phase 23 변경 기록

## 1 — 2026-08-10 · ShapeId 가중 × Area

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `shapeWeights` — 필드 (추가)
    - 설명: ShapeId 1~42 float 배열. 인덱스=ID, `[0]` 미사용. 기본 1.
    - 이유: 플레이 피드백으로 피스 빈도를 직접 조절할 노브.
  - 심볼: `GetShapeWeight` — 메서드 (추가)
    - 설명: ID별 가중 반환. 범위 밖·미설정은 1, 음수는 0.
    - 이유: Orchestrator가 안전하게 배수를 읽게.
  - 심볼: `MeanShapeWeight` — 메서드 (추가)
    - 설명: 번들 세 피스 가중 산술평균.
    - 이유: `predictedArea × 평균` 우승 판정용.
  - 심볼: `EnsureShapeWeights` — 메서드 (추가)
    - 설명: 길이 43 배열을 보장하고 빠진 ID는 1로 채움.
    - 이유: 에셋/코드 기본값이 어긋나도 런타임 안전.
  - 심볼: `OnValidate` — 메서드 (추가)
    - 설명: 에디터에서 배열 길이·음수 클램프.
    - 이유: 인스펙터 편집 시 깨진 배열 방지.
  - 심볼: `ResetShapeWeightsToOne` — ContextMenu (추가)
    - 설명: 전 ID 가중을 1로 리셋.
    - 이유: 튜닝 되돌리기.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `PickMaxEffectiveArea` — 메서드 (추가, `PickMaxArea` 대체)
    - 설명: `predicted × MeanShapeWeight` 최댓값 후보 선택.
    - 이유: Phase23 A안 — 전부 1이면 기존 maxArea와 동일.
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: Normal Area 픽을 `PickMaxEffectiveArea`로 바꾸고 reason에 ×w 표기.
    - 이유: 로그로 가중 적용 여부 확인.
  - 심볼: `TrySelectByMaxArea` — 메서드 (수정)
    - 설명: Easy 경로도 effective 최댓값으로 선택.
    - 이유: Normal과 동일 규칙.

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights` — 직렬화 (추가)
    - 설명: ID1–42 전부 1 (인덱스0=0).
    - 이유: 기본 동작 유지한 채 튜닝 시작점.

## 2 — 2026-08-10 · L3(ㄱ) 가중 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: 3칸 ㄱ(L tromino 4방향) 가중 `1 → 0.5`.
    - 이유: 플레이 피드백 — Normal/Easy Area에서 L3 과다.

## 3 — 2026-08-10 · I3(1×3) 가중 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[4,5]` — 직렬화 (수정)
    - 설명: 세로·가로 1×3 가중 `1 → 0.5`.
    - 이유: 플레이 피드백 — I3 과다.

## 4 — 2026-08-10 · 3×3 가중 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[13]` — 직렬화 (수정)
    - 설명: 3×3 정사각 가중 `1 → 0.5`.
    - 이유: 플레이 피드백 — 3×3 과다.

## 5 — 2026-08-10 · 2×3 가중 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[35,36]` — 직렬화 (수정)
    - 설명: 2×3·3×2 직사각 가중 `1 → 0.5`.
    - 이유: 플레이 피드백 — 2×3 과다.

## 6 — 2026-08-10 · I4(1×4) 가중 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[7,17]` — 직렬화 (수정)
    - 설명: 세로·가로 1×4 가중 `1 → 0.5`.
    - 이유: 플레이 피드백 — I4 과다.

## 7 — 2026-08-10 · I5(1×5) 가중 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[11,22]` — 직렬화 (수정)
    - 설명: 가로·세로 1×5 가중 `1 → 0.5`.
    - 이유: 플레이 피드백 — I5 과다.

## 8 — 2026-08-10 · 3×3·2×3 추가 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[13,35,36]` — 직렬화 (수정)
    - 설명: 3×3·2×3·3×2 가중 `0.5 → 0.25` (회전 포함 전부).
    - 이유: 0.5로도 과다 — 번들 평균이라 체감이 약함.

## 9 — 2026-08-10 · 직사각·2×2 → 0.35

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[9,13,35,36]` — 직렬화 (수정)
    - 설명: 2×2·3×3·2×3·3×2 가중을 **0.35**로 통일 (직전 0.25 직사각 포함).
    - 이유: 플레이 피드백 — 직사각·네모 빈도 재조정.

## 10 — 2026-08-10 · I3 추가 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[4,5]` — 직렬화 (수정)
    - 설명: 1×3·3×1 가중 `0.5 → 0.35`.
    - 이유: 플레이 피드백 — I3 과다 지속.

## 11 — 2026-08-10 · L3 상향 · 2×2/3×3 0.45

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: 작은 ㄱ(L3 4방향) `0.5 → 0.8`.
    - 이유: 플레이 피드백 — L3 과소.
  - 심볼: `shapeWeights[9,13]` — 직렬화 (수정)
    - 설명: 2×2·3×3 `0.35 → 0.45`.
    - 이유: 플레이 요청 수치.

## 12 — 2026-08-10 · L3 → 0.3 (정정)

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: 작은 ㄱ `0.8 → 0.3`.
    - 이유: 직전 피드백 정정 — 실제로는 L3 과다.

## 13 — 2026-08-10 · L3 → 0.15

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: 작은 ㄱ `0.3 → 0.15`.
    - 이유: 플레이 피드백 — 0.3으로도 과다.

## 14 — 2026-08-10 · L3 → 0.08

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: 작은 ㄱ `0.15 → 0.08`.
    - 이유: 플레이 요청 수치.

## 15 — 2026-08-10 · I2(1×2) → 0.2

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[2,3]` — 직렬화 (수정)
    - 설명: 세로·가로 1×2 가중 `1 → 0.2`.
    - 이유: 플레이 피드백 — I2 과다.

## 16 — 2026-08-10 · 이상 번들 제거 (I4×2+I5, I3+2×2×2)

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal` / `CreateEasy` — 메서드 (수정)
    - 설명: `n020,n043,n279,n317`(1×4×2+1×5), `n042,n262,ez12`(1×3+2×2×2) 엔트리 삭제.
    - 이유: Blocks2 전수 수집이라 이상 조합이 그대로 들어감 — 플레이 피드백으로 제외.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: 동일 bundleId — 직렬화 (삭제)
    - 설명: 런타임 풀에서 위 7개 제거.
    - 이유: StarterData와 에셋 동기화.

## 17 — 2026-08-10 · I2·ㄱ 추가 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[2,3]` — 직렬화 (수정)
    - 설명: 1×2 `0.2 → 0.08`.
    - 이유: 플레이 피드백 — 여전히 과다.
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: 작은 ㄱ `0.02 → 0.01`.
    - 이유: 동일.
  - 심볼: `shapeWeights[8,12,21,23,24,29–34,42]` — 직렬화 (수정)
    - 설명: L4·L5 ㄱ `0.08 → 0.03`.
    - 이유: 동일.

## 18 — 2026-08-10 · 큰 ㄱ 가중 복구

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[8,12,21,23,24,29–34,42]` — 직렬화 (수정)
    - 설명: L4·L5 ㄱ `0.03 → 0.08` (직전 추가 하향 철회).
    - 이유: 큰 ㄱ은 더 낮출 필요 없음.

## 19 — 2026-08-10 · 작은 ㄱ 하드밴

- 수정: `Scripts/Domain/AreaBundleSpawn/HospitalityPiecePolicy.cs`
  - 심볼: `IsSmallL` — 메서드 (추가)
    - 설명: ID 6·15·27·28 판별.
    - 이유: 공통 밴 기준.
- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `ContainsSmallL` / `SampleCandidates` / `PickWeighted` / 접대·올클 루프 — (수정)
    - 설명: 작은 ㄱ이 하나라도 든 번들 후보에서 제외.
    - 이유: 가중만으로는 손이 계속 나옴 — 번들 단위 차단.
- 수정: `Scripts/Domain/AreaBundleSpawn/UniqueUnlockGenerator.cs`
  - 심볼: `BuildUniquePoolIds` — 메서드 (수정)
    - 설명: Unique 풀에서 작은 ㄱ 제외.
    - 이유: Unique 경로로도 나오던 구멍 차단.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: `0`.
    - 이유: 이중 안전장치.

## 20 — 2026-08-10 · T+I3+I3 번들 제거

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal` — 메서드 (수정)
    - 설명: `n177`(10,5,5 = T+1×3+1×3) 삭제.
    - 이유: 플레이 피드백 — 이상한 조합.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `n177` — 직렬화 (삭제)
    - 설명: 동일.
    - 이유: StarterData와 동기화.

## 21 — 2026-08-10 · I2+I3+2×3 번들 제거

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal` — 메서드 (수정)
    - 설명: `n144`(3,35,5 = 2×1+3×2+3×1) 삭제.
    - 이유: 플레이 피드백.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `n144` — 직렬화 (삭제)
    - 설명: 동일.
    - 이유: StarterData와 동기화.

## 16 — 2026-08-10 · L3 접대 제외 + Area 0.02

- 수정: `Scripts/Domain/AreaBundleSpawn/HospitalityPiecePolicy.cs`
  - 심볼: `FitWeight` — 메서드 (수정)
    - 설명: ID 6·15·27·28(작은 ㄱ) FitWeight=0 → 접대 Exact 제외.
    - 이유: shapeWeights 0.08로도 과다 — 접대가 shapeWeights를 안 씀.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[6,15,27,28]` — 직렬화 (수정)
    - 설명: 작은 ㄱ `0.08 → 0.02`.
    - 이유: Normal/Easy Area에서도 추가 억제.

## 17 — 2026-08-10 · L4·L5 ㄱ 전부 억제

- 수정: `Scripts/Domain/AreaBundleSpawn/HospitalityPiecePolicy.cs`
  - 심볼: `FitWeight` — 메서드 (수정)
    - 설명: L4(`8,29–34,42`)·큰 L5(`12,21,23,24`)도 FitWeight=0.
    - 이유: 작은 L3만 막아도 큰 ㄱ이 접대/Area에서 그대로 나와 “ㄱ 계속” 체감.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[8,12,21,23,24,29–34,42]` — 직렬화 (수정)
    - 설명: L4·L5 ㄱ Area 가중 `1 → 0.08`.
    - 이유: Normal/Easy에서도 큰 ㄱ 억제.

## 18 — 2026-08-10 · I4 억제

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[7,17]` — 직렬화 (수정)
    - 설명: 1×4·4×1 `0.5 → 0.2`.
    - 이유: 플레이 피드백 — I4 과다.
- 수정: `Scripts/Domain/AreaBundleSpawn/HospitalityPiecePolicy.cs`
  - 심볼: `FitWeight` — 메서드 (수정)
    - 설명: ID 7·17 FitWeight=0 → 접대 Exact 제외.
    - 이유: 일자 구멍에 I4 Exact가 자주 걸려 shapeWeights만으로 부족.

## 19 — 2026-08-10 · 3×3 추가 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[13]` — 직렬화 (수정)
    - 설명: 3×3 `0.45 → 0.15`.
    - 이유: 플레이 피드백 — 3×3 과다 (접대는 이미 제외).

## 22 — 2026-08-10 · I5+T+I5 번들 제거

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal` — 메서드 (수정)
    - 설명: `n145`(22,25,22 = 1×5+T+1×5) 삭제.
    - 이유: 플레이 피드백.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `n145` — 직렬화 (삭제)
    - 설명: 동일.
    - 이유: StarterData와 동기화.

## 24 — 2026-08-10 · I5 Area 가중만 하향

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[11,22]` — 직렬화 (수정)
    - 설명: I5 `0.5 → 0.2`.
    - 이유: 플레이 피드백 — 1×5 과다. 접대 Exact 제외는 하지 않음.

## 25 — 2026-08-10 · 접대 윤곽 하한 추가 완화

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `hospitalityContourMinFill` — 필드 (수정)
    - 설명: `0.5 → 0.35`.
    - 이유: 적용 계수 추가 하향.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `hospitalityContourMinFill` — 직렬화 (수정)
    - 설명: `0.5 → 0.35`.
    - 이유: SO 동기화.

## 26 — 2026-08-10 · shapeWeights 리셋(작은 ㄱ만 0)

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights[1..42]` — 직렬화 (수정)
    - 설명: 대부분 `1`. ID 6·15·27·28만 `0` 유지.
    - 이유: 최근 커밋 체감 복구. 작은 ㄱ만 억제. **→ 27에서 튜닝값 복구**

## 27 — 2026-08-10 · shapeWeights 튜닝값 복구

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `shapeWeights` — 직렬화 (수정)
    - 설명: 리셋 직전 튜닝값으로 복구 (I2/I3/I4/I5·사각·ㄱ 등).
    - 이유: 2버전 Normal 설계 전에 임의 리셋한 것 되돌림.

## 28 — 2026-08-10 · 3×3+I4+I4 번들 제거

- 수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateNormal` — 메서드 (수정)
    - 설명: `n009`(13,17,17)·`n197`(13,7,7) 삭제.
    - 이유: 플레이 피드백 — 3×3+1×4+1×4.
- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `n009` / `n197` — 직렬화 (삭제)
    - 설명: 동일.
    - 이유: StarterData와 동기화.
