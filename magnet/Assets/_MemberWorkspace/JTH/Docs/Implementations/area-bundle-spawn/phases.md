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
| 23 | ShapeId 가중 × Area | [phase23.md](phase23.md) | [sequence23.md](sequence23.md) | **완료** |
| 24 | Death 표시 → **디버그 제거** | [phase24.md](phase24.md) | [sequence24.md](sequence24.md) | **완료** (로그·결과 필드 삭제) |
| 25 | Normal/Easy Death 배제(예산 캡) | [phase25.md](phase25.md) | [sequence25.md](sequence25.md) | **완료** |
| 26 | Normal Clean/Main + 체이닝 | [phase26.md](phase26.md) | [sequence26.md](sequence26.md) | **완료** |
| 27 | Unique Unlock 확정 + Unique Shape 가중 | [phase27.md](phase27.md) | [sequence27.md](sequence27.md) | **완료** |
| 28 | 패 선택 Explain 기즈모 | [phase28.md](phase28.md) | [sequence28.md](sequence28.md) | **완료** |
| 29 | 찬 칸 두꺼운 Area 연결 | [phase29.md](phase29.md) | [sequence29.md](sequence29.md) | **완료** |
| 30 | Clean 체이닝 클리어 폴백 | [phase30.md](phase30.md) | [sequence30.md](sequence30.md) | **완료** |
| 31 | Normal Area 라인클리어 필수 | [phase31.md](phase31.md) | [sequence31.md](sequence31.md) | **완료** |
| 32 | 찬 Area — 직교 이웃≥2만 | [phase32.md](phase32.md) | [sequence32.md](sequence32.md) | **완료** |
| 33 | AllClear 이후 Normal 가중랜덤 | [phase33.md](phase33.md) | [sequence33.md](sequence33.md) | **완료** |
| 34 | 모서리 덮개 직사각 패널티 | [phase34.md](phase34.md) | [sequence34.md](sequence34.md) | **완료** |
| 35 | CornerRect 튜닝 단계 CR-1~4 | [phase35.md](phase35.md) | [sequence35.md](sequence35.md) | **완료** |
| 36 | 찬 Area 다리 절단 | [phase36.md](phase36.md) | [sequence36.md](sequence36.md) | **완료** (phase38에서 제거) |
| 37 | MaxArea 랭킹 비용 축소 | [phase37.md](phase37.md) | [sequence37.md](sequence37.md) | **완료** |
| 38 | 찬 Area 직교볼록 홈 절단 + Area 기즈모 | [phase38.md](phase38.md) | [sequence38.md](sequence38.md) | **완료** |
| 39 | Unique 4칸 균형 + 손 최적 배치 기즈모 | [phase39.md](phase39.md) | [sequence39.md](sequence39.md) | **완료** |
| 40 | 라인필 히트맵 스폰 (Unique/Normal 단순화) | [phase40.md](phase40.md) | [sequence40.md](sequence40.md) | **완료** |

## grill 확정 요약

- **cascade:** dirty(`occ≥UniqueMinOccupied`, 기본 **40**) → `p_unique` Unique, 아니면 Normal 히트맵. Unique 실패→Normal→Easy. Easy 실패 시만 가중랜덤(isKillHand).
- **Relife:** Easy **1턴**만 (`IsRetrySession`, 현재 스텁).
- **제거(선택 경로):** 접대·올클 Exact·Clean/Main·Clean체인·Area MaxArea·완주·라인클리어 필수·Death 배제.
- **Normal 점수:** `Σheat − emptyPen×(heat==0)`. `emptyPen = clamp01(score/(max/3*2))×n` (n=2, max 기본 **3000**). 올클 즉시 채택(빈 보드 제외).
- **Unique:** 동적 `UniqueUnlockGenerator` 유지. `uniqueShapeWeights` 4칸 중심.
- **Normal 풀:** Blocks root+Blocks2 병합 **561** ordered (weight=관측 clamp 1–5). Clean 리스트는 미사용 잔존.
- **예산:** `maxCandidatesToScore`=**64**.
- **Area/접대/올클 코드:** Phase40 §2에서 삭제. 선택·기즈모는 히트맵만.
