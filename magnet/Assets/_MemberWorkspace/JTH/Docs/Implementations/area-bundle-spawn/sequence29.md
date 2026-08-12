# sequence29 — Phase 29 변경 기록

> Phase 계획: [phase29.md](phase29.md)

## 1 — 2026-08-11 · 찬 칸 두꺼운 Area 연결

**바뀐 것** — 찬 Area를 “두꺼운 코어 + 한 칸 돌출”으로만 묶고, 가느다란 다리·두 칸 연속 돌출은 분리한다. 빈 Area는 그대로 4연결.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.OccupiedCoreMinNeighbors8` — const (추가)
    - 설명: 찬 코어로 볼 최소 8이웃 찬 개수(3).
    - 이유: 폭1 복도의 칸(이웃 2)을 코어에서 빼 두 덩어리가 다리로 합쳐지지 않게.
  - 심볼: `AreaScoreCalculator.OccupiedAttachMinNeighbors8` — const (추가)
    - 설명: 돌출 부착에 필요한 최소 “이미 덩어리인 8이웃” 수(2).
    - 이유: 한 칸 툭 나온 건 허용하고, 그다음 칸은 거절하기 위해.
  - 심볼: `AreaScoreCalculator.Neighbors8` — 정적 필드 (추가)
    - 설명: 상하좌우+대각 8방향 오프셋.
    - 이유: 코어/부착 판정이 8이웃을 보기 때문.
  - 심볼: `AreaScoreCalculator.Score` — 메서드 (수정)
    - 설명: 빈 칸만 기존 `Flood`로 모은 뒤, 찬 칸은 `AddOccupiedThickComponents`로 모은다.
    - 이유: 두꺼운 연결을 찬 칸에만 적용하기 위해 경로를 나눈다.
  - 심볼: `AreaScoreCalculator.AddOccupiedThickComponents` — 메서드 (추가)
    - 설명: 코어 표시 → 코어 4연결 flood → 1패스 돌출 부착 → 잔여 찬 칸 4연결.
    - 이유: 두꺼운 덩어리와 가느다란 줄을 서로 다른 Area로 점수에 넣기 위해.
  - 심볼: `AreaScoreCalculator.FloodOccupiedCores` — 메서드 (추가)
    - 설명: `isCore`인 찬 칸만 4연결 BFS로 한 컴포넌트를 만든다.
    - 이유: 두꺼운 본체를 먼저 확정해야 돌출 부착 기준이 생긴다.
  - 심볼: `AreaScoreCalculator.AttachOccupiedProtrusions` — 메서드 (추가)
    - 설명: 코어 컴포넌트에 4인접·비코어·8이웃 덩어리≥2인 칸만 한 번에 붙인다.
    - 이유: “한 칸 돌출 OK / 두 칸째 불가”를 1패스로 보장.
  - 심볼: `AreaScoreCalculator.CountNeighborsInComponent` — 메서드 (추가)
    - 설명: 후보 칸의 8이웃 중 이미 컴포넌트에 속한 개수를 센다.
    - 이유: 부착 조건(≥2) 판정용.
  - 심볼: `AreaScoreCalculator.CountOccupiedNeighbors8` — 메서드 (추가)
    - 설명: 보드에서 찬 칸 8이웃 개수를 센다.
    - 이유: 코어(≥3) 판정용.
- 파일: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `AreaScoreTuning.areaCountPenalty` Tooltip — 필드 메타 (수정)
    - 설명: “4-연결(찬+빈)” 문구를 “찬=두꺼운연결·빈=4연결”로 바꾼다.
    - 이유: Inspector가 Phase 29 정의를 반영하게.
- 파일: `Docs/Implementations/area-bundle-spawn/TUNING_STAGES.md`
  - 심볼: `areaCountPenalty` 행 설명 (수정)
    - 설명: 동일하게 찬/빈 연결 규칙을 구분해 적는다.
    - 이유: 튜닝 표와 코드 동작을 맞추기 위해.
