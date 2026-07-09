# null 조건 연산자 (`?.` / `??` / `??=`) 가이드

JTH 워크스페이스에서 `if (x != null)` 패턴을 점검하고, 가능한 경우 null 조건·null 병합 연산자로 바꾼 기록.

**적용일:** 2026-07-09

## 규칙 요약

| 패턴 | 권장 변환 | 예 |
|------|-----------|-----|
| `if (obj != null) obj.Method()` | `obj?.Method()` | 이벤트 해제, 단일 호출 |
| `if (obj != null) obj.Event -= h` | `obj?.Event -= h` | `OnDisable` 구독 해제 |
| `if (cache != null) return cache` | `return cache ??= Create()` | 지연 생성 캐시 |
| `if (x == null) x = value` | `x ??= value` | lazy 초기화 |
| `if (a == null) a = b` (대체값) | `a ?? b` | Shader 폴백 등 |
| `if (obj != null) return` (이미 있음) | **유지** 또는 `if (obj == null) { 생성 }` | lazy root 생성 |
| `Debug.Assert(x != null)` | **유지** | 개발자 설정 오류 검증 |
| `if (x == null) return` (조기 종료) | **유지** | 런타임 입력·복구 흐름 |

`?.`는 **멤버 접근·호출**에만 쓴다. “null이 아니면 return” 같은 **흐름 분기**에는 맞지 않는다.

## JTH 코드 감사 결과

### 변환 완료

| 파일 | 이전 | 이후 | 연산자 |
|------|------|------|--------|
| `BlockSelectionInput.cs` · `OnDisable` | `if (magnetInput != null) { … -= … }` | `magnetInput?.OnSlotSelected -= OnSlotSelected` | `?.` |
| `BlockSelectionInput.cs` · `OnDisable` | `if (magnetGameChannel != null) { RemoveListener }` | `magnetGameChannel?.RemoveListener<…>(…)` | `?.` |
| `BoardView.cs` · `GetLineMaterial` | `if (_sharedLineMaterial != null) return …` + shader null 체크 | `_sharedLineMaterial ??= new Material(… ?? …); return …` | `??=` · `??` |
| `MagnetInputSO.cs` · `MainCam` | `if (_mainCam == null) _mainCam = Camera.main` | `_mainCam ??= Camera.main` | `??=` |

### 변환하지 않음 (이유)

| 파일 | 패턴 | 이유 |
|------|------|------|
| `BoardView.cs` · `EnsureLinesRoot` | `if (linesRoot != null) return` | `if (linesRoot == null) { 생성 }` — `?.` 부적합, early return을 생성 블록으로 정리 |
| `BlockPieceView.cs` · `EnsureCellsRoot` | `if (cellsRoot != null) return` | 위와 동일 |
| `BlockSelectionInput.cs` · `TrySelect` | `if (_candidates == null …)` · `if (shape == null)` | 런타임 입력 검증 — 명시적 분기 유지 |
| `MagnetInputSO.cs` · `OnEnable` | `if (_controls == null) { new + SetCallbacks }` | 생성 시 **여러 문장** 필요. `??=`만으로는 `SetCallbacks` 중복 호출 위험 |
| `MagnetInputSO.cs` · `GetWorldPointerPosition` | `if (MainCam == null) return …` | null일 때 **다른 값 반환** — `?.`로 대체 불가 |
| 전역 | `Debug.Assert(x != null, …)` | `CLAUDE.md` 방어 코드 규칙 — Assert 유지 |
| `Controls.cs` (생성) | `if (instance == null \|\| …)` | Input System 생성 코드 — 수정 금지 |

### 이미 적용되어 있던 예

- `MagnetInputSO.OnDisable`: `_controls?.Disable()`
- `MagnetInputSO` 이벤트 발행: `OnSlotSelected?.Invoke(…)` · `OnPointerChange?.Invoke(…)`
- `BasePresenter` (HwanLib): `View?.RootCanvas`

## 변환 예시

```csharp
// Before
if (magnetInput != null)
    magnetInput.OnSlotSelected -= OnSlotSelected;

// After
magnetInput?.OnSlotSelected -= OnSlotSelected;
```

```csharp
// Before
if (_sharedLineMaterial != null)
    return _sharedLineMaterial;
_sharedLineMaterial = new Material(shader);
return _sharedLineMaterial;

// After
_sharedLineMaterial ??= new Material(
    Shader.Find("Sprites/Default")
    ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
return _sharedLineMaterial;
```

```csharp
// Before — != null early return
if (linesRoot != null)
    return;
var root = new GameObject("Lines");
// ...

// After — 생성 블록으로 정리 (?. 아님, 동등 분기)
if (linesRoot == null)
{
    var root = new GameObject("Lines");
    // ...
}
```

## 범위

- **대상:** `Assets/_MemberWorkspace/JTH/Scripts/**/*.cs`
- **제외:** 타 멤버 워크스페이스, `HwanLib`, `TextMesh Pro`, 기타 서드파티
