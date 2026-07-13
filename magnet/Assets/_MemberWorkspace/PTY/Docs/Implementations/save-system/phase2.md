# Phase 2 — 저장 항목 확장 (플레이 통계 + 장착 스킨)

> **구현:** `save-system` · **Jira:** [SCRUM-28](https://bimtaeur30.atlassian.net/browse/SCRUM-28)
> **상태:** 완료
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] `GameSaveData`에 총 플레이 타임, 최대 연속 폭발 콤보 횟수, 게임 오버 횟수, 장착 중인 스킨 필드 추가
- [x] `ISaveService`에 위 항목의 getter + 갱신 메서드 자리 추가
- [x] `SaveService` 스텁에 새 멤버 반영 (실제 로직은 Phase 1과 동일하게 `NotImplementedException`으로 유지)

## 구현 내용 (뭘 어떻게)

| 클래스/인터페이스 | 변경 |
|---|---|
| `GameSaveData` | 필드 추가: `EquippedSkinId`(string), `TotalPlayTime`(float, 초), `MaxExplosionCombo`(int), `GameOverCount`(int) |
| `ISaveService` | getter 추가: `EquippedSkinId`, `TotalPlayTime`, `MaxExplosionCombo`, `GameOverCount` / 메서드 추가: `EquipSkin(string)`, `AddPlayTime(float)`, `SubmitExplosionCombo(int)`, `RecordGameOver()` |
| `SaveService` | 위 인터페이스 변경분을 스텁(`NotImplementedException`)으로 반영 |

### 합의된 설계 결정

- "획득한 스킨 개수"는 별도 필드로 저장하지 않고 기존 `UnlockedSkinIds.Count`로 계산해서 사용 (중복 상태 방지)
- "최고 점수"는 이미 Phase 1의 `BestScore`가 그 역할이라 별도 필드를 추가하지 않음
- `MaxExplosionCombo`는 `SubmitScore`와 동일한 패턴으로 세션에서 달성한 값 중 최고치만 갱신하는 방식을 전제로 메서드명을 `SubmitExplosionCombo`로 정함 (실제 "최고값만 반영" 로직은 `SaveService` 구현 시 채울 것)
- `GameOverCount`, `TotalPlayTime`은 누적값이라 `RecordGameOver()`(+1), `AddPlayTime(seconds)`(누적 더하기) 형태의 메서드로 정의

## 이 Phase 범위 밖

- `SaveService`의 실제 누적/최고값 갱신 로직 구현
- 게임 오버, 콤보 발생, 플레이 타임 틱, 스킨 장착 시점에 위 메서드를 실제로 호출하는 소비자 코드 연결
- 로컬-클라우드 병합 시 `TotalPlayTime`/`GameOverCount`(합산이 맞을지 최고값이 맞을지) 등 필드별 병합 정책 세부 확정 — 현재 Phase 1 정책(필드별 더 높은 값 우선)을 그대로 따를지는 다음 담당자가 실제 구현 시점에 재확인 필요

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 데이터 모델 | `Scripts/Save/GameSaveData.cs` |
| 서비스 인터페이스 | `Scripts/Save/ISaveService.cs` |
| 서비스 스텁 | `Scripts/Save/SaveService.cs` |
