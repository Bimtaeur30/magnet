# sequence32 — Phase 32 변경 기록

> Phase 계획: [phase32.md](phase32.md)

## 1 — 2026-08-11 · 찬 Area 직교 이웃 ≥2

**바뀐 것** — 찬 Area에서 대각·돌출 부착을 없애고, 상하좌우 찬 이웃 ≥2인 칸만 한 덩어리로 묶는다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.OccupiedAreaMinCardinalNeighbors` — const (추가, 기존 8이웃 상수 대체)
    - 설명: 찬 Area 자격에 필요한 최소 직교 찬 이웃 수(2).
    - 이유: 한 칸 툭 나온 끝(이웃 1)을 본체에서 빼기 위해.
  - 심볼: `AreaScoreCalculator.Neighbors8` — 정적 필드 (삭제)
    - 설명: 8방향 오프셋 배열을 제거한다.
    - 이유: 대각을 더 이상 쓰지 않아서.
  - 심볼: `AreaScoreCalculator.AddOccupiedThickComponents` — 메서드 (수정)
    - 설명: 직교 이웃≥2인 `eligible`만 flood하고, 돌출 부착 호출을 뺀다.
    - 이유: “튀어나온 것도 Area에서 제외” 요구를 반영.
  - 심볼: `AreaScoreCalculator.FloodOccupiedEligible` — 메서드 (추가, `FloodOccupiedCores` 대체)
    - 설명: `eligible` 찬 칸만 4연결 BFS로 묶는다.
    - 이유: 자격 마스크 이름이 코어/돌출 모델과 달라서 교체.
  - 심볼: `AreaScoreCalculator.CountOccupiedCardinals` — 메서드 (추가)
    - 설명: 상하좌우 찬 이웃 개수를 센다.
    - 이유: 자격 판정용.
  - 심볼: `AreaScoreCalculator.AttachOccupiedProtrusions` / `CountNeighborsInComponent` / `CountOccupiedNeighbors8` — 메서드 (삭제)
    - 설명: 8이웃 기반 돌출 부착·코어 판정 경로를 제거한다.
    - 이유: Phase 32 규칙과 충돌해서.
- 파일: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `AreaScoreTuning.areaCountPenalty` Tooltip — 필드 메타 (수정)
    - 설명: 찬 Area 설명을 “직교이웃≥2”로 바꾼다.
    - 이유: Inspector·코드 정의를 맞추기 위해.
