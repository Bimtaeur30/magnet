# JTH — 구현 인덱스

개인 작업은 **구현 → Phase → Sequence** 3단계로 기록한다.  
팀 로드맵은 `Docs/DESIGN.md` **마일스톤(M0~M10)** — 개인 Phase와 **다른 개념**.

→ 구현 선택 → `phases.md` → `phaseN.md`(계획) + `sequenceN.md`(변경 기록) → 표의 코드 경로 확인.

| 구현 (slug) | 제목 | Jira | Phase 인덱스 | 상태 |
|-------------|------|------|--------------|------|
| [common-bootstrap](./Implementations/common-bootstrap/phases.md) | 공통 기반 (Reflex·이벤트) | — | phase1 완료 | 구현됨 · 확인 대기 |
| [block-coordinates](./Implementations/block-coordinates/phases.md) | 블록 좌표·보드 격자 | [SCRUM-17](https://bimtaeur30.atlassian.net/browse/SCRUM-17) | phase1 완료 | 구현됨 · 확인 대기 |
| random-block-spawn | 랜덤 블록 생성 | [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) | — | 미착수 |
| block-placement | 블록 배치·흡착 | [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) | — | 미착수 |
| game-over | 게임 오버 판정 | [SCRUM-22](https://bimtaeur30.atlassian.net/browse/SCRUM-22) | — | 미착수 |
| block-destruction | 블록 파괴 판정 | [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) | — | 미착수 |
| board-rotation | 회전·턴 흐름 | [SCRUM-21](https://bimtaeur30.atlassian.net/browse/SCRUM-21) | — | 미착수 |
| score-logic | 점수 관리 (로직) | [SCRUM-23](https://bimtaeur30.atlassian.net/browse/SCRUM-23) | — | 미착수 |

**UI / HUD / 인벤토리 / 메뉴는 JTH 담당·Jira 범위 밖.**

## 계층 (용어)

| 용어 | 의미 | 파일 | 예 |
|------|------|------|-----|
| **구현** | 기능·Jira 이슈·요청 단위 | `Implementations/[slug]/` | 인벤토리, 블록 좌표 |
| **Phase** | 그 구현을 쪼갠 단계. **뭘 어떻게 구현하는지 자세히** 적음 | `phaseN.md` | 1) 구조 → 2) 핵심 기능 → 3) 부가 기능 |
| **Sequence** | 그 Phase에서 **뭐가 바뀌었는지** 순서대로 적는 변경 기록. **Phase와 1:1** 파일 | `sequenceN.md` | `## 1 — 최초 구현`, `## 2 — 버그 수정` … |

## 새 AI 세션

`IMPLEMENTATIONS.md` + **진행 중 구현**의 `phases.md` + 해당 **`phaseN.md`·`sequenceN.md`** 만 읽는다 (전체 히스토리 X).

## 파일 형식

**`phaseN.md`** (Phase 계획 — 뭘 어떻게)

- 목표 (완료 기준) · 구현 내용 (클래스·책임·방식 상세) · 범위 밖 · 코드·에셋 맵

**`sequenceN.md`** (변경 기록 — Phase와 1:1, 안에 여러 항목)

```markdown
## 1 — 날짜 · 제목
**바뀐 것** — 생성/수정/삭제 파일 목록
**메모** — 함정·비자명한 결정만
---
## 2 — 날짜 · 제목
…
```
