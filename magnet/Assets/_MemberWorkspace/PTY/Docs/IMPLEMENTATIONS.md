# PTY — 구현 인덱스

개인 작업은 **구현 → Phase → Sequence** 3단계로 기록한다.

| 구현 (slug) | 제목 | Jira | Phase 인덱스 | 상태 |
|-------------|------|------|--------------|------|
| [block-shape-editor](./Implementations/block-shape-editor/phases.md) | 블록 형태 생성 에디터 | [SCRUM-25](https://bimtaeur30.atlassian.net/browse/SCRUM-25) | phase4 완료 | 구현됨 · 확인 대기 |
| [save-system](./Implementations/save-system/phases.md) | 세이브 시스템 (베스트 스코어/언락 스킨, GPGS·GameCenter 연동 뼈대) | [SCRUM-28](https://bimtaeur30.atlassian.net/browse/SCRUM-28) | phase1 완료 | 구조만 구현됨 · 기능 로직은 스텁 |

## 계층 (용어)

| 용어 | 의미 | 파일 |
|------|------|------|
| **구현** | 기능·Jira 이슈·요청 단위 (예: 인벤토리) | `Implementations/[slug]/` |
| **Phase** | 그 구현을 쪼갠 단계. **뭘 어떻게 구현하는지 자세히** 적음 | `phaseN.md` |
| **Sequence** | 그 Phase에서 **뭐가 바뀌었는지** 순서대로 적는 변경 기록. **Phase와 1:1** 파일 | `sequenceN.md` |

예: 인벤토리 구현 → Phase 1 구조 / Phase 2 아이템 넣기 / Phase 3 부가 기능(단축키·드래그), Phase마다 `sequenceN.md`에 변경 사항 기록.

## 폴더 구조 (새 구현 시작 시)

```
Docs/
  IMPLEMENTATIONS.md
  Implementations/
    [slug]/
      phases.md          # Phase 인덱스
      phase1.md          # Phase 1 계획 (뭘 어떻게)
      sequence1.md       # Phase 1 변경 기록 (1:1)
      phase2.md
      sequence2.md
```

형식 예시: `Assets/MemberWorkspace/JTH/Docs/Implementations/block-coordinates/`

## 파일 형식

- **`phaseN.md`** — 목표(완료 기준) · 구현 내용(클래스·책임·방식 상세) · 범위 밖 · 코드·에셋 맵
- **`sequenceN.md`** — `## N — 날짜 · 제목` 섹션을 순서대로 추가 (파일 분리 X). 각 항목: **바뀐 것**(생성/수정/삭제 파일) · **메모**
