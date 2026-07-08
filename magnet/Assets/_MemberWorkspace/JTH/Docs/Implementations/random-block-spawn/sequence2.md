# Sequence — Phase 2 (random-block-spawn)

> **Phase:** [phase2.md](phase2.md) 와 1:1.

## 1 — 2026-07-07 · 균등 랜덤 추첨 (순수 로직)

**바뀐 것**

- 생성: `Assets/_Shared/Magnet.Contracts/BlockShapes/IBlockShapeSource.cs`
- 생성: `Scripts/Domain/Spawn/IRandom.cs`
- 생성: `Scripts/Domain/Spawn/SystemRandom.cs`
- 생성: `Scripts/Domain/Spawn/PresetShapeSource.cs`
- 생성: `Scripts/Domain/Spawn/BlockDrawer.cs`
- 생성: `Tests/Magnet.JTH.Tests.asmdef` (JTH EditMode 테스트 어셈블리, 신규)
- 생성: `Tests/Spawn/BlockDrawerTests.cs`

**변경 상세 (왜/무엇)**

- 파일: `Magnet.Contracts/BlockShapes/IBlockShapeSource.cs`
  - 심볼: `IBlockShapeSource.Shapes` — 프로퍼티 `IReadOnlyList<IBlockShape> { get; }` (추가)
    - 설명: 추첨 후보가 될 블록 형태 목록을 읽기 전용으로 노출한다.
    - 이유: 추첨 대상 형태 풀의 "출처"를 계약으로 분리. JTH↔PTY 경계 유지(Path A) — JTH는 계약만 소비, Phase 5에서 PTY SO가 구현·등록.
    - 영향: `PresetShapeSource`(구현), `BlockDrawer.Draw()`(소비).
- 파일: `Scripts/Domain/Spawn/IRandom.cs`
  - 심볼: `IRandom.Next(int maxExclusive)` — 메서드 (추가)
    - 설명: 0 이상 `maxExclusive` 미만의 정수 하나를 반환하는 난수 계약.
    - 이유: 난수 생성을 계약 뒤로 숨겨 추첨을 결정론적으로 테스트·재현. 테스트에서 고정 구현 주입.
- 파일: `Scripts/Domain/Spawn/SystemRandom.cs`
  - 심볼: `SystemRandom._random` — 필드 `System.Random` (추가)
    - 설명: 실제 난수를 생성하는 `System.Random` 인스턴스를 보관한다.
    - 이유: 시드 상태를 인스턴스에 보관해 전역 오염 없이 재현.
  - 심볼: `SystemRandom()` — 기본 생성자 (추가)
    - 설명: 시드 없이 `System.Random`을 생성한다(실행마다 다른 순서).
    - 이유: 실사용 시 매번 다른 무작위 순서가 필요한 경로.
  - 심볼: `SystemRandom(int seed)` — 시드 생성자 (추가)
    - 설명: 주어진 `seed`로 `System.Random`을 생성한다.
    - 이유: 같은 시드로 같은 추첨 순서를 재현(테스트·디버깅).
  - 심볼: `SystemRandom.Next(int maxExclusive)` — 메서드 (추가)
    - 설명: `_random.Next(maxExclusive)` 결과를 그대로 반환한다.
    - 이유: `IRandom` 계약을 `System.Random`으로 구현.
- 파일: `Scripts/Domain/Spawn/PresetShapeSource.cs`
  - 심볼: `PresetShapeSource.Shapes` — 프로퍼티 `{ get; }` (추가)
    - 설명: 생성 시 저장한 형태 목록을 반환한다.
    - 이유: `IBlockShapeSource`를 구현해 추첨기가 소비할 수 있게 한다.
  - 심볼: `PresetShapeSource()` — 생성자 (추가)
    - 설명: `BlockShapePresets.All`을 `Shapes`에 저장한다.
    - 이유: SO 없이 코드 프리셋만으로 Phase 2를 완결하기 위한 개발용 소스.
- 파일: `Scripts/Domain/Spawn/BlockDrawer.cs`
  - 심볼: `BlockDrawer._source` — 필드 `IBlockShapeSource` (추가)
    - 설명: 뽑을 형태 풀의 출처를 참조로 보관한다.
    - 이유: `Draw()`가 매번 형태 목록을 얻기 위함.
  - 심볼: `BlockDrawer._random` — 필드 `IRandom` (추가)
    - 설명: 인덱스 선택에 쓸 난수 공급자를 보관한다.
    - 이유: 난수 구현을 주입받아 테스트에서 교체 가능하게.
  - 심볼: `BlockDrawer(IBlockShapeSource source, IRandom random)` — 생성자 (추가)
    - 설명: 전달받은 소스와 난수를 두 필드에 그대로 대입한다.
    - 이유: 생성자 DI로 의존을 외부에서 주입(싱글톤·전역 접근 회피).
  - 심볼: `BlockDrawer.Draw()` — 메서드 (추가)
    - 설명: `_source.Shapes`에서 `_random.Next(Count)`로 인덱스를 골라 형태 1개(`IBlockShape`)를 반환한다.
    - 이유: Phase 2는 균등 추첨. Phase 4에서 이 메서드 내부만 가중치로 교체(소스·공급기 불변, OCP).
    - 영향: Phase 3 `BlockSupply`가 이 `Draw()`로 슬롯을 채울 예정.
- 파일: `Tests/Spawn/BlockDrawerTests.cs`
  - 심볼: `BlockDrawerTests.FixedRandom` — 중첩 스텁 클래스 (추가)
    - 설명: `Next()`가 생성자에서 받은 고정 인덱스를 항상 반환하는 `IRandom` 스텁.
    - 이유: 난수를 고정해 추첨 결과를 결정론적으로 단정하기 위함.
  - 심볼: `BlockDrawerTests.Draw_ReturnsShapeAtRandomIndex()` — 테스트 (추가)
    - 설명: `FixedRandom(2)`로 뽑으면 `source.Shapes[2]`와 같은 인스턴스가 나오는지 확인.
    - 이유: `Draw()`가 난수 인덱스를 실제로 형태 선택에 쓰는지 검증.
  - 심볼: `BlockDrawerTests.Draw_SameSeed_ProducesSameSequence()` — 테스트 (추가)
    - 설명: 같은 시드의 두 드로어가 20회 연속 같은 형태를 반환하는지 확인.
    - 이유: 시드 결정론(재현성) 보장 검증.
  - 심볼: `BlockDrawerTests.Draw_AlwaysReturnsShapeFromSource()` — 테스트 (추가)
    - 설명: 50회 뽑아 모두 `BlockShapePresets.All` 안의 형태인지 확인.
    - 이유: 인덱스 범위를 벗어난 값을 반환하지 않음을 검증.
  - 심볼: `BlockDrawerTests.Draw_CoversAllShapes_OverManyDraws()` — 테스트 (추가)
    - 설명: 500회 뽑아 등장한 고유 형태 수가 소스 전체 개수와 같은지 확인.
    - 이유: 균등 추첨이 특정 형태를 배제하지 않음을 검증.

**검증**

- `Assets/Refresh` → 컴파일 에러 0 (경고는 Reflex 로그 레벨 1건, 무관).
- (당시) EditMode 테스트 `Magnet.JTH.Tests` 4/4 통과 후 **항목 2에서 삭제** — JTH 정책: 검증용 테스트는 레포에 남기지 않음.

**메모**

- `IBlockShapeSource`를 JTH가 아닌 `Magnet.Contracts`에 둔 것은 Path A(경계 유지) 결정 때문. Contracts는 공용이라 팀 공유 권장.
- `BlockDrawer`는 방어 코드 최소화(빈 소스면 `Next(0)`가 자연 예외). CLAUDE.md Simplicity First.

---

## 2 — 2026-07-07 · 검증용 테스트 삭제 (JTH 정책)

**바뀐 것**

- 삭제: `Tests/Magnet.JTH.Tests.asmdef`, `Tests/Magnet.JTH.Tests.asmdef.meta`
- 삭제: `Tests/Spawn/BlockDrawerTests.cs`, `Tests/Spawn/BlockDrawerTests.cs.meta`
- 삭제: `Tests/Spawn.meta`, `Tests.meta`

**변경 상세 (왜/무엇)**

- 파일: `Tests/Spawn/BlockDrawerTests.cs` (삭제)
  - 심볼: `BlockDrawerTests` 및 하위 테스트·`FixedRandom` 스텁 (삭제)
    - 설명: Phase 2 추첨 로직을 EditMode에서 자동 검증하던 테스트 클래스 전체.
    - 이유: JTH Workspace에는 영구 테스트를 두지 않는다. AI 검증 후 레포를 가볍게 유지(사용자 요청).
- 파일: `Tests/Magnet.JTH.Tests.asmdef` (삭제)
  - 심볼: `Magnet.JTH.Tests` 어셈블리 정의 (삭제)
    - 설명: EditMode 테스트 전용 asmdef.
    - 이유: 테스트 코드 삭제에 맞춰 빈 테스트 어셈블리 제거.

**검증**

- 삭제 후 `read_console` 컴파일 에러 0 확인.

**메모**

- 정책 저장: `.cursor/rules/jth-tests.mdc` — 검증용 테스트는 통과 확인 후 삭제, 사용자가 "테스트 남겨" 요청 전까지 `Tests/` 신설 금지.

---
