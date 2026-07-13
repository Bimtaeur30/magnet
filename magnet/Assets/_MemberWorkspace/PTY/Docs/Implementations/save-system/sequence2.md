# Sequence — Phase 2 (save-system)

> **Phase:** [phase2.md](phase2.md) 와 1:1.

## 1 — 2026-07-12 · 플레이 통계 + 장착 스킨 저장 항목 추가

**바뀐 것**

- 수정: `Scripts/Save/GameSaveData.cs` — `EquippedSkinId`, `TotalPlayTime`, `MaxExplosionCombo`, `GameOverCount` 필드 추가
- 수정: `Scripts/Save/ISaveService.cs` — 위 필드에 대응하는 getter 및 `EquipSkin`, `AddPlayTime`, `SubmitExplosionCombo`, `RecordGameOver` 메서드 추가
- 수정: `Scripts/Save/SaveService.cs` — 새 인터페이스 멤버를 `NotImplementedException` 스텁으로 반영

**메모**

- "획득한 스킨 개수"는 별도 필드 없이 `UnlockedSkinIds.Count`로 계산하기로 함 (사용자 확인).
- "최고 점수"는 Phase 1의 `BestScore`와 동일 항목이라 신규 필드를 만들지 않음.
- 실제 누적/최고값 갱신 로직과 호출 시점(게임 오버, 콤보 발생, 플레이 타임 틱, 스킨 장착) 연결은 범위 밖 — Phase 1과 동일하게 스텁으로만 남김.
- `refresh_unity`(compile 요청) + `read_console` 확인 결과 컴파일 에러·경고 0건.
