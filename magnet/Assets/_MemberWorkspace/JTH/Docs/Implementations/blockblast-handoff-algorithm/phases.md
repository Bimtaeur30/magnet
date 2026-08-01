# BlockBlast 핸드오프 알고리즘 — Phase 인덱스

> **구현:** `blockblast-handoff-algorithm` · **Jira:** — · **대체 대상:** `block-selection-algorithm` (9-티어 스택)  
> **근거 자료:** BlockBlast! 1.3.71 역공학 인계본 (외부 폴더 `BlockBlast_AI_Handoff_5files` — PDF 보고서 + 검증 500건 + restored_profile.json)  
> **핵심 한계:** 주력 알고리즘 1370(all-combination fill, native 14009)은 C++ 미복원 → **근사 구현** (grill로 사용자 확정).

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 핵심 체인 이식 (카탈로그·파이프라인·Drawer 교체) | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | **완료** |

**grill 확정 사항 (2026-08-01):**

- 1370 미복원 → "완주 가능 + 라인 클리어 선호" 조합 탐색으로 **근사** (선택지 a)
- 블록 체계는 핸드오프 **42-ID 카탈로그**를 알고리즘 내부 도메인으로 채택, 회전형이 별도 ID라 스폰 회전 없음 (선택지 a)
- Trait는 관측 데이터(500건의 actualId가 1370/7/2100뿐)를 설명하는 **핵심 체인만** 구현 — 세션시간·광고·초보자 규제·미복원 난이도(4001 계열) 제외 (선택지 a)
- 기존 `BlockSelectionOrchestrator` 계열 코드는 **삭제하지 않고 보존** (롤백용) — Bootstrap 배선만 교체

**관측 근거 (핸드오프 §8):** 500건 중 algoActualId 1370=430 · 7=69 · 2100=1. 미출현 ID [11,12,13,21,22,23,24,39,40,41]은 1370 근사 풀에서 제외.
