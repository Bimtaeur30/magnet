## 1 — 2026-07-11 · BoardSession 제거 + SquareClearService

**바뀐 것** — `SquareClearService.cs` 생성, `BoardSession.cs`·`PlacedBlock.cs`·`BoardGrid.cs` 수정

**변경 상세 (왜/무엇)**
- `SquareClearDetector`: 테두리 클리어 시 **테두리 + N×N 바깥** 칸 제거, **내부(chebyshev < half) 유지**
- `BoardSession.RemoveCells`: 칸 단위 제거, 남은 칸 있으면 offsets 갱신

---
## 2 — 2026-07-11 · DESIGN §4.5 동기화

**바뀐 것** — `Docs/DESIGN.md` §4.5·§3·M5·SCRUM-20, `CLAUDE.md` Phase 규칙

**변경 상세**
- 클리어 시 **테두리 + N×N 바깥** 제거, **내부 유지**
- 풀 클리어는 `isFullClear` 플래그만 (내부 제거 없음)
- 마일스톤 진행 규칙: 「담당자 확인 후」 문구 삭제 → 규칙 변경 시 DESIGN/TODO 동기화
