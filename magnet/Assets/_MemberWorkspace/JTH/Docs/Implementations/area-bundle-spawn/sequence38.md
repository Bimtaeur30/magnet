# sequence38 — Phase 38 변경 기록

> Phase 계획: [phase38.md](phase38.md)

## 1 — 2026-08-12 · 직교볼록 홈 절단 + Area 기즈모

**바뀐 것** — 찬 Area 다리절단을 제거하고, 행·열 다중 run(홈)에서 축정렬 절단으로 직교볼록 조각으로 나눈다. Partition 결과를 Scene 기즈모로 표시한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreResult.cs`
  - 심볼: `AreaPartition` — 구조체 (추가)
    - 설명: 한 Area의 occupied 여부와 칸 목록을 담는다.
    - 이유: 점수·기즈모가 동일 분할을 공유.
  - 심볼: `AreaPartition.Occupied` / `Cells` / `Size` — 프로퍼티 (추가)
    - 설명: 찬/빈 구분·칸·크기를 노출한다.
    - 이유: 기즈모 색/라벨과 점수 size 입력.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.Partition` — 메서드 (추가)
    - 설명: 빈=4연결, 찬=4연결 후 직교볼록 분할한 `AreaPartition` 목록을 반환한다.
    - 이유: Score와 기즈모의 단일 분할 진입점.
  - 심볼: `AreaScoreCalculator.Score` — 메서드 (수정)
    - 설명: `Partition` 결과를 점수화한다 (다리절단 경로 제거).
    - 이유: 새 분할 규칙으로 AreaCount·base를 맞춤.
  - 심볼: `AreaScoreCalculator.SplitOccupiedOrtho` — 메서드 (추가)
    - 설명: 직교볼록이 될 때까지 균형 cut으로 재귀 분할한다.
    - 이유: U홈은 두 Area, 계단 L은 유지.
  - 심볼: `AreaScoreCalculator.IsOrthoConvex` — 메서드 (추가)
    - 설명: 모든 행·열 run이 1개 이하인지 판정한다.
    - 이유: 「얕은 계단 OK / 깊은 U NG」의 격자 동치.
  - 심볼: `AreaScoreCalculator.TryFindBalancedCut` — 메서드 (추가)
    - 설명: 다중 run 갭 후보 중 절단 후 min(left,right) 최대인 축 cut을 고른다.
    - 이유: 홈 입구에서 두 덩어리로 균형 있게 가름.
  - 심볼: `AreaScoreCalculator.ConsiderGapsAlongLine` / `CountRuns` / `BuildRuns` / `FloodWithin` — 메서드 (추가)
    - 설명: run 추출·갭 평가·절단 후 부분 재연결 flood.
    - 이유: 분할 구현 단위.
  - 심볼: `BridgeSplitMinPartSize` / `AddOccupiedBridgeSplitComponents` / `SplitAtBridges` / `FloodPartsExcluding` — (삭제)
    - 설명: 관절점 다리절단 경로를 제거한다.
    - 이유: 두꺼운 U 바닥은 다리절단으로 안 잘리고, 요구와 다른 모델.
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `AreaBundleSelectionGizmo.drawAreas` / `drawOccupiedAreas` / `drawEmptyAreas` — 필드 (추가)
    - 설명: 보드 Area 오버레이 on/off (기본: 찬 Area만).
    - 이유: 최적의 수 기즈모에서 Area를 같이 보게.
  - 심볼: `AreaBundleSelectionGizmo.DrawAreaPartitions` / `DrawAreaPartition` / `ColorForAreaIndex` — 메서드 (추가)
    - 설명: `Partition` 결과를 보드 칸에 색·라벨로 그린다.
    - 이유: 별도 기즈모 없이 선택 Explain과 동일 뷰에서 검증.
  - 심볼: `AreaBundleSelectionGizmo.OnDrawGizmos` — 메서드 (수정)
    - 설명: selection 유무와 무관하게 Area를 먼저 그리고, 패/Explain은 selection이 있을 때만 그린다.
    - 이유: Area는 보드 상태만으로도 보이게.
- 파일: `Scripts/Presentation/AreaPartitionGizmo.cs` / Debug prefab — (삭제)
  - 심볼: 단독 Area 기즈모 경로 (삭제)
    - 설명: 선택 기즈모로 기능을 옮겨 제거한다.
    - 이유: 최적의 수 기즈모에서 Area를 보려는 요구에 맞춤.
---
## 2 — 2026-08-12 · MaxNotchDepth=1 관용

**바뀐 것** — 행·열 다중 run이어도 홈 깊이 ≤1이면 자르지 않는다. 깊이 2+만 축절단.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.MaxNotchDepth` — const (추가, **1**)
    - 설명: 이 깊이 이하 홈은 절단 후보에서 제외한다.
    - 이유: 한 칸 파임·짧은 요철은 한 Area로 유지.
  - 심볼: `AreaScoreCalculator.TryFindDeepBalancedCut` — 메서드 (추가, 기존 TryFindBalancedCut 대체)
    - 설명: 깊이 &gt; MaxNotchDepth 갭만 모아 균형 cut을 고른다.
    - 이유: 얕은 홈 관용과 깊은 U 절단을 분리.
  - 심볼: `AreaScoreCalculator.MeasureGapDepth` / `CountEmptyGapExtent` — 메서드 (추가)
    - 설명: 갭 열이 비어 있는 연속 줄 수(현재 줄 포함)를 잰다.
    - 이유: 홈 깊이 임계 판정.
  - 심볼: `AreaScoreCalculator.SplitOccupiedOrtho` — 메서드 (수정)
    - 설명: 깊은 cut이 없으면 분할하지 않고 한 Area로 남긴다.
    - 이유: MaxNotchDepth 관용 적용.
---
## 4 — 2026-08-12 · MaxNotchDepth 2→1

**바뀐 것** — 홈 관용 깊이를 다시 1로 되돌린다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.MaxNotchDepth` — const (수정, **2→1**)
    - 설명: 깊이 1 이하 홈만 절단하지 않는다.
    - 이유: 관용 2가 과해서 1로 복귀.
---
## 5 — 2026-08-12 · MaxNotchDepth 1→0

**바뀐 것** — 다중 run 갭이면 깊이 관용 없이 절단한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.MaxNotchDepth` — const (수정, **1→0**)
    - 설명: 깊이 1 홈도 자른다. (`depth > 0`)
    - 이유: F0처럼 얕은 홈이 한 Area로 남는 문제.
---
