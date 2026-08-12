# sequence36 — Phase 36 변경 기록

> Phase 계획: [phase36.md](phase36.md)

## 1 — 2026-08-12 · 찬 Area 다리 절단

**바뀐 것** — 찬 Area를 4연결한 뒤, 끊으면 양쪽이 각 3칸 이상인 다리 칸에서 쪼갠다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaScoreCalculator.cs`
  - 심볼: `AreaScoreCalculator.BridgeSplitMinPartSize` — const (추가, **4**)
    - 설명: 다리 절단 시 양쪽이 이 크기 이상이어야 분할한다.
    - 이유: 3이면 사진형 L의 세로 팔(끊으면  Stub 3)이 잘못 잘려서 4로 상향.
  - 심볼: `AreaScoreCalculator.AddOccupiedBridgeSplitComponents` — 메서드 (추가, thick/eligible 경로 대체)
    - 설명: 찬 칸 4연결 flood 후 `SplitAtBridges`로 나눈 뒤 점수화한다.
    - 이유: 자격(직교≥2) 필터 대신 다리 절단으로 자연/꼼수를 구분.
  - 심볼: `AreaScoreCalculator.SplitAtBridges` — 메서드 (추가)
    - 설명: 칸을 하나씩 제외해 보고, 큰 부분 ≥2개면 재귀 분할 후 제외 칸을 단독 Area로 둔다.
    - 이유: 폭1 복도 중간을 끊는 구현.
  - 심볼: `AreaScoreCalculator.FloodPartsExcluding` — 메서드 (추가)
    - 설명: 집합에서 한 칸을 뺀 나머지 4연결 성분들을 반환한다.
    - 이유: 관절점 판정용.
  - 심볼: `AddOccupiedThickComponents` / `FloodOccupiedEligible` / `CountOccupiedCardinals` / `OccupiedAreaMinCardinalNeighbors` — (삭제)
    - 설명: 직교≥2 자격 기반 찬 Area 경로 제거.
    - 이유: Phase36 다리 절단으로 대체.
