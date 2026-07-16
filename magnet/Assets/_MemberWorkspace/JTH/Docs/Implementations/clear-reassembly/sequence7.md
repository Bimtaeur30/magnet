# sequence7 — Phase 7 변경 기록

> Phase 계획: [phase7.md](phase7.md)

## 1 — 2026-07-16 · 자석 원점 재배치 + 보드밖 주차

**바뀐 것** — 부채꼴을 칸에서 쏘던 방식을 자석 `(0,0)`에서 원래 칸 방향으로 쏘도록 바꾸고, 설정각만 쓰며 실패 시 보드 밖 주차한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryFind` — 메서드 (수정)
    - 설명: 설정각∩서클만 시도하고, 실패 시 `TryAllocateOutsidePark`. ±90°·각도무시 폴백 삭제.
    - 이유: 각을 넓히지 말라는 요구 + 후보 없을 때 보드밖 배치.
  - 심볼: `TryFindInSectorCircle` — 메서드 (추가)
    - 설명: 원점 자석·축=원래 칸 방향. 후보 순위=축 각 → 자석 거리 → 시계.
    - 이유: “축에 붙어 최대한 안쪽으로” 떨어지게.
  - 심볼: `TryAllocateOutsidePark` — 메서드 (추가)
    - 설명: 보드 밖·미점유 칸 중 같은 축 각 최소·거리 최소를 고른다.
    - 이유: 부채꼴 실패 시 경계 밖 최근접 주차.
  - 심볼: `TryGetAxis` / `TryReadCandidate` — 메서드 (추가)
    - 설명: 축·서클·설정각 필터를 공유한다.
    - 이유: Find/Collect 중복 제거.
  - 심볼: `CollectSectorCircleCandidates` — 메서드 (추가)
    - 설명: 설정각∩서클 통과 빈칸을 모두 모은다.
    - 이유: 기즈모용. `CollectAllStageCandidates`/`CollectMatching` 대체.
  - 심볼: `CollectAllStageCandidates` / `CollectMatching` / `TryFindInternal` — 메서드 (삭제)
    - 설명: 이젝터 원점·다단계 각 확장 API 제거.
    - 이유: 새 기하와 불일치.
- 파일: `Scripts/Presentation/CellRelocationTargetGizmo.cs`
  - 심볼: `originalCell` — 필드 (추가), `ejector` / `ejectorColor` / `includeAllEjectors` — 필드 (삭제)
    - 설명: 축 방향용 원래 칸만 두고 노란 이젝터 표시를 없앤다.
    - 이유: 쏘는 주체는 `(0,0)`이므로 노란 칸 불필요.
  - 심볼: `OnDrawGizmos` / `DrawAxis` / `DrawSectorAndCircle` / `DrawChosen` — 메서드 (수정·추가)
    - 설명: 원점 `(0,0)`에서 부채꼴·서클·축·후보·최종(보드밖은 별색)을 그린다.
    - 이유: 새 로직 시각 검증.
- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `HalfSectorDegrees` — 툴팁 (수정)
    - 설명: 90° 확장 문구 삭제, 자석 원점·보드밖 주차 설명.
    - 이유: Inspector/툴팁 문서와 동작 일치.

## 2 — 2026-07-16 · 이상각 슬롯만 이동 (막히면 제자리)

**바뀐 것** — 부채꼴 안 최소 축 각(이상각)이 점유면 옆 각도 칸으로 가지 않고 제자리. 영역 슬롯이 아예 없을 때만 보드밖.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryFind` — 메서드 (수정)
    - 설명: 이상각 빈칸 선택 실패 시 OutsidePark가 아니라 `false`(제자리). 슬롯 부재일 때만 보드밖.
    - 이유: `(0,2)`에서 `(0,1)` 점유 시 `(-1,1)`(45°)로 가지 않음.
  - 심볼: `TryFindIdealAngle` — 메서드 (추가)
    - 설명: 점유 포함 부채꼴∩서클 슬롯 중 최소 축 각을 구한다.
    - 이유: 각도 우선 — 가장 축에 붙은 경로만 유효.
  - 심볼: `TryPickEmptyAtIdealAngle` — 메서드 (추가)
    - 설명: 이상각과 같은 빈칸만 거리→시계로 고른다.
    - 이유: 이상각이 막히면 차선 각도로 폴백하지 않음.
