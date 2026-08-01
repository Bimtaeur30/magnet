# Phase 2 — BoardHealth + BlameTracker + TuningSO

> **구현:** `block-selection-algorithm` · **Sequence:** [sequence2.md](sequence2.md) · **스펙:** [SPEC.md](SPEC.md) §12·§13·§17

## 목표

보드 상태 점수(`BoardHealth` → TooEmpty/Sweet/TooDirty)와 유저 최근 배치가 판을 망친 정도(`BlameScore`, 감쇠 포함)를 계산하는 순수 Domain 코드 + 수치 튜닝 SO. 점수·콤보는 입력으로 쓰지 않는다. 티어 셀렉터가 없으므로 이번 Phase는 계산기만 만들고 execute_code로 검증한다.

## 완료 조건

- [x] `HealthZone` / `BoardHealthResult` / `BoardHealthCalculator` — §12 지표 4종(fillRate, deadZoneCount, bigPieceSlots, placementFreedom) + 종합 점수 + 구간 매핑
- [x] `BlameTracker` / `TurnFeedback` — §13.1 증가 4종, §13.2 감쇠, §13.4 GoodTurn 판정
- [x] `BlockSelectionTuningSO` — §12.3 구간 경계, §12.2 가중치·정규화 상한, §13.1~13.3 blame 수치 (Tooltip + `INSPECTOR_TOOLTIPS.md` 갱신)
- [x] `DefaultBlockSelectionTuning.asset` 생성 (권장 초기값)
- [x] 컴파일 에러 0 (`read_console`)
- [x] 검증 시나리오 6종 통과 (execute_code — 임시 스크립트 없음)

## 설계 결정

| 결정 | 이유 |
|------|------|
| `placementFreedom` 테스트 피스는 `Compute` 파라미터로 주입 | 17종 모양의 소스는 PTY `BlockShapeSourceSO`뿐 — Domain에 offsets 중복 하드코딩 방지. 1x1 제외는 호출자(Phase 7 Bootstrap) 책임 |
| `bigPieceSlots`용 3×3·1×5는 계산기 내부 상수 | 자명한 고정 모양이라 주입 불필요. 1×5는 가로+세로 2방향 |
| 구간 매핑을 fill 경계 → 스코어 순으로 재배열 (스펙 리터럴과 다름) | 스펙 §12.3 리터럴 순서면 꽉 찬 보드(fill 0.9, score 낮음)가 TooEmpty로 판정돼 Trap 게이트(TooDirty)가 영원히 안 열림. fill 경계 우선 후 score<0.35 → TooEmpty, score<0.40 → TooDirty |
| `blamePerBigSlotLost`는 슬롯 감소 턴에 1회 flat 가산 | 슬롯당 가산이면 블록 하나로 3×3 피벗 10개+가 사라져 권장값(8~12)과 스케일 불일치. freedomDrop만 ×\|Δ\| (스펙 명시) |
| `LastTurnDelta`는 감쇠 전 이번 턴 원값 | §13.4 GoodTurn 기준이 "이번 턴에 새로 쌓인 blame 원값". 감쇠는 `Total`에만 적용 |
| `FillDirtyFalloff` 튜닝 필드 추가 (기본 0.35) | 스펙 §12.2 공식의 상수 `/0.35`를 하드코딩하지 않고 튜닝 가능하게 |
| dead zone = 빈 칸 4방향 flood-fill 연결 영역 중 크기 1~3 | 보드 벽도 경계라 크기 ≤3인 빈 영역은 정의상 "둘러싸임" |
| 회전 중복 제거는 offsets 정렬 후 문자열 시그니처 | 대칭 모양(1×5 등)의 180° 회전이 순서만 다른 동일 집합이라 순서 무관 비교 필요 |

## 만진 파일

- `Scripts/Domain/BlockSelection/Health/HealthZone.cs` (신규)
- `Scripts/Domain/BlockSelection/Health/BoardHealthResult.cs` (신규)
- `Scripts/Domain/BlockSelection/Health/BoardHealthCalculator.cs` (신규)
- `Scripts/Domain/BlockSelection/Blame/TurnFeedback.cs` (신규)
- `Scripts/Domain/BlockSelection/Blame/BlameTracker.cs` (신규)
- `Scripts/Data/BlockSelectionTuningSO.cs` (신규)
- `ScriptableObjects/BlockSelection/DefaultBlockSelectionTuning.asset` (신규)
- `Docs/INSPECTOR_TOOLTIPS.md` (수정 — TuningSO 필드 22종 표 추가)

## 범위 밖

번들·가중치(Phase 3), 티어 셀렉터(Phase 4), Hospitality/Pressure 생성기(Phase 5·6), Drawer·Bootstrap 연동과 이벤트 발행(Phase 7), 티어 확률(`p_trap` 등)·sampleCount 튜닝 필드(Phase 4~6에서 추가)
