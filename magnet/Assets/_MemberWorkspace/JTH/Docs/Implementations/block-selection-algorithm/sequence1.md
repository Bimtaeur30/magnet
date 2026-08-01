# Sequence — Phase 1 (block-selection-algorithm)

> **Phase:** [phase1.md](phase1.md) 와 1:1.

## 1 — 2026-07-31 · 솔버 4파일 (`Simulation/`)

**바뀐 것**

- 생성: `Scripts/Domain/BlockSelection/Simulation/Piece.cs`
- 생성: `Scripts/Domain/BlockSelection/Simulation/ShapeRotator.cs`
- 생성: `Scripts/Domain/BlockSelection/Simulation/BoardSnapshot.cs`
- 생성: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/BlockSelection/Simulation/Piece.cs`
  - 심볼: `Piece` — sealed class (추가)
    - 설명: `ShapeId` + 회전 적용된 `CellOffsets`를 가진 불변 스폰 피스.
    - 이유: 솔버·생성기가 공유하는 최소 데이터 단위. 회전은 생성 시점 고정 (DESIGN §4.2 플레이어 회전 없음).
- 파일: `Scripts/Domain/BlockSelection/Simulation/ShapeRotator.cs`
  - 심볼: `ShapeRotator.Rotate(IReadOnlyList<Vector2Int>, int)` — static 메서드 (추가)
    - 설명: canonical offsets를 90° 단위 시계 방향 회전 후 최소 좌표를 (0,0)으로 정규화.
    - 이유: PTY `BlockShapeSO`는 canonical 1종만 저장 — 스폰·솔버가 4방향을 만들 유일한 지점.
  - 심볼: `ShapeRotator.Normalize(List<Vector2Int>)` — private static (추가)
    - 설명: 회전으로 생긴 음수 좌표를 제거해 pivot 기준을 통일.
    - 이유: `CanPlace` pivot 열거(0..7)가 음수 offsets를 다룰 필요 없게.
- 파일: `Scripts/Domain/BlockSelection/Simulation/BoardSnapshot.cs`
  - 심볼: `BoardSnapshot` — sealed class, `bool[,]` 내부 (추가)
    - 설명: `BoardGrid`의 값 복사본. 시뮬 전용.
    - 이유: 백트래킹이 보드를 변조하므로 실보드 오염 차단.
  - 심볼: `BoardSnapshot(BoardGrid)` — public 생성자 (추가)
    - 설명: 실보드를 읽어 스냅샷 생성. `BoardGrid`는 수정하지 않음.
    - 이유: 처음엔 static `From` 팩토리였으나 C# 기본 관용구에 맞춰 생성자로 정리 (2026-07-31).
  - 심볼: `BoardSnapshot.Clone()` — 메서드 (추가)
    - 설명: 백트래킹 분기마다 통째 복사 (`bool[,]` 얕은 Clone으로 충분).
  - 심볼: `BoardSnapshot.CanPlace(offsets, pivot)` — 메서드 (추가)
    - 설명: bounds + overlap 검사 (`PlacementService.CanPlace`와 동일 의미, 스냅샷 대상).
  - 심볼: `BoardSnapshot.Place(offsets, pivot)` — 메서드 (추가)
    - 설명: 칸 점유 후 꽉 찬 행·열을 동시에 지우고 지운 라인 수 반환.
    - 이유: 솔버 판정(Trap/ComboBreak/unique)은 클리어 반영이 필수 (SPEC §2).
  - 심볼: `BoardSnapshot.ClearFullLines()` — private (추가)
    - 설명: 전체 행·열 1패스 검사·제거. 제거는 칸을 비울 뿐이라 연쇄 불필요.
- 파일: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs`
  - 심볼: `PlacementSolver.HasAnyPlacement(board, pieces)` — static (추가)
    - 설명: 피스 중 1개라도 즉시 배치 가능한가. 스폰 결과 검증용 (Death 없음 보장).
  - 심볼: `PlacementSolver.FullSequenceExists(board, pieces)` — static (추가)
    - 설명: 순서·클리어 포함, 전부 놓는 방법 존재 여부. Trap 판정(`false`)에 사용.
  - 심볼: `PlacementSolver.ComboMaintainable(board, pieces)` — static (추가)
    - 설명: 전부 놓으면서 라운드 중 클리어 ≥1이 가능한 시퀀스 존재 여부. ComboBreak 판정(`false`)에 사용.
  - 심볼: `PlacementSolver.CountFullSequences(board, pieces, cap)` — static (추가)
    - 설명: full sequence 개수. cap 도달 시 조기 종료 — unique 판정은 cap=2.
  - 심볼: `PlacementSolver.Search / TryPlacements / HasAnyPivot / BuildSignatures` — private static (추가)
    - 설명: depth별 미사용 피스 선택 백트래킹. 동일 형태 피스는 depth당 1회만 시도(시그니처 dedupe)해 중복 시퀀스 방지.

**검증**

- `refresh_unity` 후 `read_console` 컴파일 에러 0.
- `execute_code` 시나리오 5종 (임시 스크립트 파일 없이):
  1. 빈 보드 + (1x3, 2x2, L3) → `hasAny=T, fullSeq=T, combo=F` ✅ (10칸으로 한 줄 8칸 완성 불가)
  2. row0 7칸 참 + (1x1, 2x2, 1x3) → `combo=T` ✅
  3. Trap 보드(3×3 빈 영역 + 고립 빈칸, 3x3×3개) → `hasAny=T, fullSeq=F, combo=F, count=0` ✅
  4. 빈 보드 `CountFullSequences(cap=2)` → `2` (조기 종료) ✅
  5. L3 90° 회전 → 정규화된 3칸, 음수 없음 ✅

**메모**

- 검증 중 첫 Trap 보드 구성이 잘못됨(행·열 3~6이 처음부터 꽉 참 — 실게임에 존재 불가한 정지 상태). 솔버가 8라인을 정당하게 지운 것. 고립 대각 빈칸을 추가한 유효 보드로 재검증 통과 — **솔버 버그 아님**.
- `ComboMaintainable`은 현재 콤보 상태를 입력받지 않음 — "이번 3피스 라운드에서 클리어 가능한가"만 판정 (SPEC §8.6).

## 2 — 2026-07-31 · 보드 표현 통합 (`BoardSnapshot` 제거)

**바뀐 것**

- 수정: `Scripts/Domain/Board/BoardGrid.cs` — 내부 `HashSet<Vector2Int>` → `bool[,]`, `Clone()` 추가
- 생성: `Scripts/Domain/BlockSelection/Simulation/PlacementSimulator.cs`
- 삭제: `Scripts/Domain/BlockSelection/Simulation/BoardSnapshot.cs`
- 수정: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs` — `BoardSnapshot` → `BoardGrid`
- 수정: `Scripts/Domain/Clear/LineClearDetector.cs` — `IsLineFull` 안의 디버그 잔재 `Debug.Log(x)` 제거

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Board/BoardGrid.cs`
  - 심볼: `BoardGrid._cells` — `bool[,]` (변경, 기존 `_occupied` HashSet 대체)
    - 설명: 8×8 고정 크기 보드라 배열이 복사·조회 모두 유리. 스냅샷과 이중 표현을 없앰.
    - 이유: 보드 상태를 HashSet으로 쓰는 곳은 `BoardGrid`뿐임을 확인 후 통합 (다른 HashSet 사용처는 지역 변수·PTY 에디터 툴).
  - 심볼: `BoardGrid.IsOccupied(Vector2Int)` — (변경)
    - 설명: bounds 검사 후 배열 조회. 보드 밖 좌표는 `false` — HashSet 시절 의미 유지.
    - 이유: `PlacementService.GetOverlap`이 bounds 검사 전에 `IsOccupied`를 호출하므로 보드 밖 좌표가 들어옴.
  - 심볼: `BoardGrid.Clone()` + private 생성자 — (추가)
    - 설명: 솔버 백트래킹 분기용 값 복사. 기존 `BoardSnapshot.Clone()` 역할 흡수.
  - 심볼: `BoardGrid.HasOccupiedCellOutsideBounds()` — (삭제)
    - 이유: 호출처 없는 죽은 코드 + `bool[,]`에선 보드 밖 저장 자체가 불가.
- 파일: `Scripts/Domain/BlockSelection/Simulation/PlacementSimulator.cs`
  - 심볼: `PlacementSimulator.PlaceAndClear(grid, offsets, pivot)` — static (추가)
    - 설명: 칸 점유 → `LineClearDetector.Detect` 재사용 → 클리어된 칸 해제, 지운 라인 수 반환. 넘긴 grid를 직접 변조(시뮬은 `Clone()` 후 호출).
    - 이유: 기존 `BoardSnapshot.Place + ClearFullLines`가 실게임 클리어 로직(`LineClearDetector`)과 별도 구현이었던 중복 제거.
- 파일: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs`
  - 심볼: 공개 API 4종 + private 헬퍼 — 시그니처 `BoardSnapshot` → `BoardGrid` (변경)
    - 설명: 배치 검사는 `PlacementService.CanPlace`, 배치 시뮬은 `PlacementSimulator.PlaceAndClear` 재사용.
    - 이유: 시뮬 전용 중복 코드 제거 — 실게임과 같은 판정 로직 사용 보장.
- 파일: `Scripts/Domain/Clear/LineClearDetector.cs`
  - 심볼: `IsLineFull` 내 `Debug.Log(x)` — (삭제)
    - 이유: 디버그 잔재. 솔버가 `Detect`를 수천 번 호출하는 핫패스라 로그 폭탄 방지.

**검증**

- `refresh_unity` 후 `read_console` 컴파일 에러 0.
- `execute_code` 시나리오 5종 재실행 전부 통과 (빈 보드 T/T/F, nearRow combo=T + 원본 보드 비오염, Trap T/F/F, count cap=2 → 2, `PlaceAndClear` 라인 클리어 1 + 칸 해제 확인).

**메모**

- `SetOccupied`는 보드 밖 좌표 시 예외 발생 (기존 HashSet은 조용히 저장) — 호출처 전수 확인 결과 모두 in-bounds라 fail-fast가 더 안전.
- `BoardGrid`는 팀 공용이 아닌 JTH 소유 Domain이지만, Phase 1의 "기존 코드 수정 없음" 원칙은 이 항목으로 예외 처리됨 (사용자 승인).

## 3 — 2026-07-31 · `Piece` 클래스 제거

**바뀐 것**

- 삭제: `Scripts/Domain/BlockSelection/Simulation/Piece.cs`
- 수정: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs` — `IReadOnlyList<Piece>` → `IReadOnlyList<IReadOnlyList<Vector2Int>>`

**변경 상세 (왜/무엇)**

- 심볼: `Piece` — (삭제)
  - 이유: `ShapeId`의 사용처가 실제로는 없음을 확인. 스폰 파이프라인(`Drawer` → `ShapeBlockData` → `ShapeBlock`)은 offsets만으로 동작하고, 가중치 추첨은 `Piece` 생성 전 SO 단계에서 끝나며, 유일해 매칭도 offsets 비교/슬롯 인덱스로 충분. offsets 하나만 남은 껍데기라 기존 파이프라인과 같은 `IReadOnlyList<Vector2Int>` 표현으로 통일.
- 심볼: `PlacementSolver` 공개 API 4종 + private 헬퍼 — 시그니처 변경
  - 설명: 피스 = `IReadOnlyList<Vector2Int>`(cellOffsets). 동일 형태 dedupe 시그니처는 원래 offsets 기반이라 로직 변화 없음.

**검증**

- `refresh_unity` 후 `read_console` 컴파일 에러 0.
- `execute_code` 시나리오 4종(빈 보드 T/T/F, nearRow combo=T + 비오염, Trap T/F/F, count cap=2 → 2) 재실행 통과.

## 4 — 2026-07-31 · `ShapeRotator.Rotate` 파라미터 변경

**바뀐 것**

- 수정: `Scripts/Domain/BlockSelection/Simulation/ShapeRotator.cs` — `Rotate(offsets, rotationDegrees)` → `Rotate(offsets, rotationCount)`

**변경 상세 (왜/무엇)**

- 심볼: `ShapeRotator.Rotate(IReadOnlyList<Vector2Int>, int rotationCount)` — 시그니처 변경
  - 설명: 도(90/180/270) 대신 회전 횟수(0~3)를 받고 내부에서 `% 4`. 호출부가 어차피 횟수를 뽑으므로 도 변환 왕복 제거, 90의 배수가 아닌 입력의 애매함도 제거.

**검증**

- 컴파일 에러 0. `execute_code`로 L3 회전 r=0~5 확인 — r=4·5가 r=0·1과 동일(wrap), 음수 좌표 없음.

## 5 — 2026-07-31 · 전 피벗 스캔 `PlacementService`로 통일

**바뀐 것**

- 수정: `Scripts/Domain/Placement/PlacementService.cs` — `CanPlaceAnywhere` 추가
- 수정: `Scripts/Domain/Turn/TurnService.cs` — `IsGameOver`가 `CanPlaceAnywhere` 사용
- 수정: `Scripts/Domain/BlockSelection/Simulation/PlacementSolver.cs` — private `HasAnyPivot` 삭제

**변경 상세 (왜/무엇)**

- 심볼: `PlacementService.CanPlaceAnywhere(cellOffsets, grid)` — static (추가)
  - 설명: 모양 하나가 보드 어딘가에 놓일 수 있는지 전 피벗 스캔.
  - 이유: 동일한 이중 루프가 `TurnService.IsGameOver`와 `PlacementSolver.HasAnyPivot`에 중복 — 1수 배치 규칙의 주인인 `PlacementService`로 끌어올림.
- 심볼: `TurnService.IsGameOver` — (변경)
  - 설명: 후보별 피벗 이중 루프 제거, `CanPlaceAnywhere` 호출로 대체.
- 심볼: `PlacementSolver.HasAnyPivot` — (삭제)
  - 설명: `HasAnyPlacement`가 `CanPlaceAnywhere`를 직접 사용.

**검증**

- 컴파일 에러 0. `execute_code` 4종: 솔버 기존 판정 유지(빈 보드 T/T/F), `CanPlaceAnywhere` 거의 찬 보드에서 1x3=F·1x1(구멍)=T, 꽉 찬 보드 3x3 hasAny=F, `IsGameOver` 거의 찬 보드+3x3만=T·빈 보드=F.

**메모**

- 남은 역할 구분: `PlacementService` = 1수 배치 규칙(검사·스냅), `PlacementSimulator` = 시뮬 전용 1수 적용(배치+클리어, 클론 위에서만), `PlacementSolver` = 다피스 시퀀스 판정. `PlacementSimulator`를 `PlacementService`로 합치지 않은 건 실보드에 직접 호출하면 Presentation(GameBoard)과 어긋나기 때문 — Simulation 네임스페이스에 둬서 시뮬 전용임을 표시.
