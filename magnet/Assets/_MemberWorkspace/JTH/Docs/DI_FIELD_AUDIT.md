# JTH — SerializeField vs [Inject] 감사 기록

AI·구현자가 **씬 MonoBehaviour를 SerializeField로 잘못 배선**한 사례를 기록한다.  
규칙 원본: `.cursor/rules/jth-reflex-di.mdc`, `CLAUDE.md` §DI.

## 판별 요약

| 타입 | 소비자 필드 | Installer |
|------|-------------|-----------|
| ScriptableObject | `[SerializeField]` | RegisterValue 금지 |
| 프리팹 **에셋** | `[SerializeField]` | — |
| 씬 MonoBehaviour | `[Inject]` | `RegisterValue` (Installer만 SerializeField) |
| 같은 GO 컴포넌트 | `GetComponent` / `[RequireComponent]` | — |

## 확정 오류 (수정됨)

| 파일 | 필드 | 잘못 | 수정 | 날짜 |
|------|------|------|------|------|
| `BlockDragInput.cs` | `placedBlocksView` | `[SerializeField]` | `[Inject]` + `MagnetSceneInstaller.RegisterValue` | 2026-07-11 |

**교훈:** `PlacedBlocksView`는 씬 GO 컴포넌트. 프리팹 에셋이 아님. Phase 4에서 AI가 Inspector 배선으로 처리함 → 금지 패턴.

## 경계 케이스 (팀 확인)

| 파일 | 필드 | 현재 | 결론 |
|------|------|------|------|
| `BlockDragDrawer.cs` | `shapeBlockPrefab` | `[SerializeField]` | ✅ 유지 — 프리팹 **에셋** |
| `MagnetSceneInstaller.cs` | `blockSpawnBootstrap` 등 | `[SerializeField]` | ✅ Installer 등록용만 |
| `BoardView.cs` | `linesRoot` | `[SerializeField] Transform` | ✅ 유지 — optional 같은 GO 하위 |
| `Block.cs` | `spriteRenderer` | `[SerializeField]` | ✅ 유지 — 프리팹 내부 컴포넌트 |

## 구현 전 체크리스트

- [ ] 새 필드 타입이 `MonoBehaviour` 파생 씬 서비스인가? → Inject
- [ ] 필드 이름이 `*Prefab`이고 타입이 프리팹 에셋인가? → SerializeField
- [ ] `EventChannelSO` / `*SO`인가? → SerializeField, Inject 금지
- [ ] Installer에만 SerializeField로 등록했는가? 소비자에 같은 참조 없는가?
