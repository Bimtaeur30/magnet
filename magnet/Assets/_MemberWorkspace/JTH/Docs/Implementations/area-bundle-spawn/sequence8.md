# sequence8 — size/변 + 직사각 패널티 합산

## 1 — 2026-08-02 · base Area − k×rectCount

**바뀐 것**

- 파일: `Scripts/Data/AreaScoreTuning.cs` — (재추가)
  - 심볼: size/변 필드들 — (재추가)
    - 설명: 블렌드(3:2:1) 기본값으로 empty/filled/side 튜닝을 다시 둔다.
    - 이유: 구 Area 점수를 본체 점수로 복구하기 위함.
  - 심볼: `rectCountPenalty` — 필드 (추가)
    - 설명: 직사각 1개당 점수에서 빼는 양. 기본 **5**.
    - 이유: grill — 직사각 개수도 점수에 영향을 주게 함.

- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `Score` / `ScoreTotal` — (수정)
    - 설명: 4-연결 flood로 base를 구한 뒤 `CountRectangles`로 개수를 세고 `base − k·count`를 반환. tuning 인자 복구.
    - 이유: 두 신호를 한 점수로 합쳐 게이트·선택에 씀.
  - 심볼: `CountRectangles` / `PartitionCount` — (추가·유지)
    - 설명: 찬·빈 마스크 최대면적 greedy 직사각 개수만 센다 (컴포넌트 리스트에는 넣지 않음).
    - 이유: Components는 flood 디버그용, 개수는 패널티 항 전용.
  - 심볼: `ScoreEmpty` / `ScoreFilled` / `SideBonus` / `Flood` / `CountOrthogonalSides` — (재추가)
    - 설명: Phase 1 size·변 경로 복구.
    - 이유: base Area 산출.

- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs`
  - 심볼: `RectCount` / `BaseArea` / `RectPenalty` — 프로퍼티 (추가)
    - 설명: 합산 내역을 결과에 노출.
    - 이유: 로그·디버그에서 base와 직사각 항을 구분.
  - 심볼: `AreaComponentScore` — (수정)
    - 설명: 다시 flood 컴포넌트(size·side·base·sideBonus) 구조.
    - 이유: 직사각은 개수만 쓰므로 기하 필드는 불필요.

- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `areaScore` / `AreaScore` — (재추가)
  - 심볼: `uniqueAreaThreshold` 기본 — (수정) −8 → **−5**
    - 설명: size/변 스케일에 맞춰 게이트를 이전 플레이 값으로.
    - 이유: 합산 점수는 −직사각수만 쓸 때보다 스케일이 큼.

- 파일: `AreaBundleOrchestrator` / `AreaBundleMetrics` — (수정)
  - 심볼: `ScoreTotal`·`MaxAreaAfterFullSequence` 호출 — tuning 재전달
    - 설명: `_pool.AreaScore`를 다시 넘김.
    - 이유: k·size/변을 SO에서 조정.

- 파일: `DefaultAreaBundlePool.asset` — areaScore 블록·k=5·thresh=−5 복구

**조건**

- `Total = BaseArea − rectCountPenalty × RectCount`
- rect 동률 규칙·greedy는 Phase 7과 동일
