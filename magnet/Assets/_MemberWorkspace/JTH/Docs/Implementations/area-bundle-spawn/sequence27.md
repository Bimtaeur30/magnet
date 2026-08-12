# sequence27 — Unique Unlock 확정 + Unique Shape 가중

## 1 — 2026-08-11 · Unique 전용 가중 + 강A 우선

- 수정: `Scripts/Data/ShapeWeightProfile.cs`
  - 심볼: `ShapeWeightProfile.Unique` — enum 값 (추가)
    - 설명: Unique 언락 추첨용 가중 프로파일 구분자를 둔다.
    - 이유: Main/Clean과 Unique 피스 분포를 분리 튜닝하기 위해.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `uniqueShapeWeights` — 필드 (추가)
    - 설명: ShapeId 1~42 Unique 추첨 가중 배열을 직렬화한다.
    - 이유: Unique 폴더 관측 빈도로 “평소 Unique에 나오는 피스”를 뽑기 위해.
  - 심볼: `GetShapeWeight` — 메서드 (수정)
    - 설명: `ShapeWeightProfile.Unique`일 때 `uniqueShapeWeights`를 반환한다.
    - 이유: Orchestrator/Generator가 Unique 전용 가중을 조회할 수 있게.
  - 심볼: `EnsureUniqueShapeWeights` — 메서드 (추가)
    - 설명: 배열 길이를 43으로 맞추고 빈 칸은 `DefaultUniqueShapeWeight`로 채운다.
    - 이유: 에셋 미설정·부분 배열에서도 Unique 가중이 유효하게.
  - 심볼: `DefaultUniqueShapeWeight` — 메서드 (추가)
    - 설명: Unique 폴더 46장 시각 라벨 빈도를 기본 가중으로 돌려준다(0=제외).
    - 이유: 초소형·미관측 대각을 막고 관측된 I/L/사각을 자주 뽑기 위해.
  - 심볼: `ResetUniqueShapeWeights` — ContextMenu (추가)
    - 설명: Unique 가중을 폴더 빈도 기본값으로 리셋한다.
    - 이유: 튜닝 중 원복이 필요해서.
  - 심볼: `OnValidate` — 메서드 (수정)
    - 설명: Unique 배열 Ensure·Clamp를 추가한다.
    - 이유: Inspector에서 음수/길이 오류를 막기 위해.

- 수정: `Scripts/Domain/AreaBundleSpawn/UniqueUnlockGenerator.cs`
  - 심볼: `TryGenerate(board, rng, sampleCount, shapeWeight)` — 메서드 (수정)
    - 설명: 가중 랜덤으로 트리플을 뽑고, 막힌1+자유2+클리어 언락을 검사한다. 단독 언락 불가(강A)를 우선 반환하고, 없으면 weak 후보를 반환한다. 중복 ID 허용.
    - 이유: 원본 Unique 핵심(둘 놓기 전 하나 불가)을 유지하면서 쉬운 단독 언락을 샘플 내에서 뒤로 밀기 위해.
  - 심볼: `AloneUnlocks` — 메서드 (추가)
    - 설명: unlock 피스 하나를 어디에 놓아도(클리어 포함) blocked가 열리는지 검사한다.
    - 이유: 강A 우선 판정에 필요.
  - 심볼: `BuildWeightedPoolIds` / `BuildPrefixSums` / `PickWeighted` — 메서드 (추가)
    - 설명: weight>0 ID만 모아 CDF로 가중 추첨한다.
    - 이유: Unique 전용 가중 샘플링.
  - 심볼: `BuildUniquePoolIds` / 균등 `PoolIds` — (삭제)
    - 설명: 고정 균등 풀을 제거한다.
    - 이유: 가중 풀로 대체.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectUniqueDynamic` — 메서드 (수정)
    - 설명: `GetShapeWeight(id, Unique)`를 Generator에 넘긴다.
    - 이유: Unique 추첨이 SO Unique 가중을 쓰게.

- 수정: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `uniqueShapeWeights` — 직렬화 (추가)
    - 설명: Unique 폴더 빈도 시드 배열을 넣는다.
    - 이유: 런타임 기본값이 에디터 에셋에도 반영되게.

- 문서: `phase27.md` · `phases.md` · `TUNING_STAGES.md` · `INSPECTOR_TOOLTIPS.md` · `IMPLEMENTATIONS.md`
