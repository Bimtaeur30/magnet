# Area-번들 스폰 알고리즘 — Phase 인덱스

> **구현:** `area-bundle-spawn` · **대체 대상:** `hybrid-spawn-algorithm` (Health/Blame·특수 티어·1370 기본 체인)
> **grill 확정:** 2026-08-02

| Phase | 제목 | 계획 | Sequence | 상태 |
|-------|------|------|----------|------|
| 1 | Area 점수 도메인 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | **완료** |
| 2 | 42-ID 번들 풀 + Early/Normal Area 최대 | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | **완료** |
| 3 | 유일수 seq/death + 킬 폴백 | [phase3.md](phase3.md) | [sequence3.md](sequence3.md) | **완료** |
| 4 | Drawer·Bootstrap 배선 | [phase4.md](phase4.md) | [sequence4.md](sequence4.md) | **완료** |
| 5 | Unique→Normal→Easy cascade + Easy 리스트 | — | [sequence5.md](sequence5.md) | **완료** |
| 6 | Unique 동적 Unlock (번들 폐기) | — | [sequence6.md](sequence6.md) | **완료** |
| 7 | 직사각 greedy Area 점수 | [phase7.md](phase7.md) | [sequence7.md](sequence7.md) | **완료** |
| 8 | size/변 + 직사각 개수 합산 | [phase8.md](phase8.md) | [sequence8.md](sequence8.md) | **완료** |
| 9 | Normal 올클·멀티클리어 우선 | [phase9.md](phase9.md) | [sequence9.md](sequence9.md) | **완료** |
| 10 | Area 개수 패널티 | [phase10.md](phase10.md) | [sequence10.md](sequence10.md) | **완료** |
| 11 | Blocks2 Normal 번들 재구축 | [phase11.md](phase11.md) | [sequence11.md](sequence11.md) | **완료** |
| 12 | 구 스폰 알고리즘 dead code 제거 | [phase12.md](phase12.md) | [sequence12.md](sequence12.md) | **완료** |
| 13 | Normal freq≥2 · MultiClear 5줄만 | [phase13.md](phase13.md) | [sequence13.md](sequence13.md) | **완료** |
| 14 | MultiClear 6 · Normal 전수 평등 집계 | [phase14.md](phase14.md) | [sequence14.md](sequence14.md) | **완료** |
| 15 | PlacementSolver dead API·유일해 타입 제거 | [phase15.md](phase15.md) | [sequence15.md](sequence15.md) | **완료** |
| 16 | Area 개수 패널티 제거 | [phase16.md](phase16.md) | [sequence16.md](sequence16.md) | **완료** (의도 오류 → phase17에서 복구) |
| 17 | 변 보너스 제거 · Area 개수 패널티 복구 | [phase17.md](phase17.md) | [sequence17.md](sequence17.md) | **완료** |
| 18 | 직사각 패널티 — 찬 칸만 | [phase18.md](phase18.md) | [sequence18.md](sequence18.md) | **완료** |
| 19 | 올클 고정 번들 + Exact | [phase19.md](phase19.md) | [sequence19.md](sequence19.md) | **완료** |
| 20 | 멀티클리어 → Hospitality | [phase20.md](phase20.md) | [sequence20.md](sequence20.md) | **완료** (기회=즉시클리어 → phase21에서 구멍으로 교체) |
| 21 | Hospitality 구멍·윤곽 Exact | [phase21.md](phase21.md) | [sequence21.md](sequence21.md) | **완료** |
| 22 | 접대 피스 allowlist | [phase22.md](phase22.md) | [sequence22.md](sequence22.md) | **완료** |

## grill 확정 요약

- **cascade:** dirty → `p_unique` Unique, 아니면 Normal. Unique 실패→Normal→Easy. **킬 패 없음**(Easy 실패 시 가중 랜덤만).
- **Relife:** Easy **1턴**만 (`IsRetrySession`, 현재 스텁).
- **처음부터 Easy 게이트 없음.**
- **선택:** Unique=seq→death→Area / Normal=올클(75%·쿨다운1·빈보드스킵)→멀티(**5줄+만**)→Area / Easy=Area 최대.
- **Unique:** 동적 `UniqueUnlockGenerator` (1 불가 + 2 클리어 언락). 번들 폐기. 실패 시 Normal→Easy.
- **Normal:** Blocks2 스크린샷 **전수** (**324**, ID0 인식실패 1건 제외). 필터·모양 편애 없음. weight=관측횟수만.
- **Easy:** 보장 1x1 패 + 관측 소형/1x1 + 구 Early (26).
- **Area (Phase 8→17):** `base − 4×rectCount − 4×areaCount` (변 보너스 없음). tiny 관대(−15/−8). Unique thresh=**−15**, p=**0.45**. 표: `TUNING_STAGES.md`.
- **Clear Priority (Phase 9→22):** 올클 Exact(`occ≤16`, p=0.75) · **Hospitality**=윤곽≥70% 구멍 Exact(4–5칸=1 · 3칸=½ · 1–2·2×2·3×3·6제외, p=0.35) · Normal Area=빔 · Easy.
