# Phase 2 — 균등 랜덤 추첨 (순수 로직)

> **구현:** `random-block-spawn` · **Jira:** [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) · **마일스톤:** M2
> **상태:** 구현됨 · 확인 대기
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] 형태 후보 풀에서 **1개를 균등 확률로** 뽑는 순수 C# 로직 (Unity/UI 의존 0)
- [x] `IBlockShapeSource` 계약을 `Magnet.Contracts`에 정의 (Phase 5에서 PTY SO가 구현)
- [x] 개발용 `PresetShapeSource`가 `BlockShapePresets.All`을 소스로 제공
- [x] `BlockDrawer`가 `IBlockShapeSource` + `IRandom`으로 형태 1개 반환
- [x] **시드 고정 시 결정론적** — 같은 시드 → 같은 순서 (`IRandom` + `SystemRandom(int seed)`)
- [x] `read_console` 컴파일 에러 없음

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `IBlockShapeSource` | `Magnet.Contracts.BlockShapes` | 추첨 대상 형태 풀(`Shapes`) 제공 계약 |
| `IRandom` | JTH `Domain.Spawn` | 난수 공급 계약 (`Next(maxExclusive)`) — 시드 재현·구현 교체용 |
| `SystemRandom` | JTH `Domain.Spawn` | `System.Random` 기반 `IRandom`, 시드 생성자 지원 |
| `PresetShapeSource` | JTH `Domain.Spawn` | `BlockShapePresets.All`을 `IBlockShapeSource`로 노출 (개발용) |
| `BlockDrawer` | JTH `Domain.Spawn` | 소스 + `IRandom`으로 형태 1개 추첨 (균등) |

### 설계 근거

- **`IRandom` 분리:** 추첨 순서를 시드로 재현하고, 난수 구현을 주입 가능하게 하기 위해 계약 뒤로 숨김.
- **`IBlockShapeSource`를 Contracts에:** JTH↔PTY 경계 유지(Path A). JTH는 계약만 소비, Phase 5에서 PTY가 `BlockShapeSO` 기반 소스를 구현·등록.
- **`BlockDrawer` 균등:** Phase 4에서 `Draw()` 내부만 가중치 방식으로 교체 (소스·공급기 불변, OCP).

## 이 Phase 범위 밖

- 3후보 슬롯 관리·후보 갱신 이벤트 (Phase 3, `BlockSupply`)
- 가중치 추첨 (Phase 4)
- PTY `BlockShapeSO` → `IBlockShapeSource` 실연동 (Phase 5, PTY 협의)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 소스 계약 | `Assets/_Shared/Magnet.Contracts/BlockShapes/IBlockShapeSource.cs` |
| 난수 계약·구현 | `Scripts/Domain/Spawn/IRandom.cs`, `SystemRandom.cs` |
| 개발용 소스 | `Scripts/Domain/Spawn/PresetShapeSource.cs` |
| 추첨기 | `Scripts/Domain/Spawn/BlockDrawer.cs` |
