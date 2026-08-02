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

## grill 확정 요약

- **cascade:** dirty → `p_unique` Unique, 아니면 Normal. Unique 실패→Normal→Easy. **킬 패 없음**(Easy 실패 시 가중 랜덤만).
- **Relife:** Easy **1턴**만 (`IsRetrySession`, 현재 스텁).
- **처음부터 Easy 게이트 없음.**
- **선택:** Unique=seq→death→Area / Normal·Easy=Area 최대.
- **Unique:** 동적 `UniqueUnlockGenerator` (1 불가 + 2 클리어 언락). 번들 폐기. 실패 시 Normal→Easy.
- **Normal:** 500건 빈도≥2 비유일·1x1 제외 (59).
- **Easy:** 보장 1x1 패 + 관측 소형/1x1 + 구 Early (26).
- **Area (Phase 8):** `base − 3×rectCount`, tiny 관대(−15/−8). Unique thresh=**−15**, p=**0.45**. 표: `TUNING_STAGES.md`.
