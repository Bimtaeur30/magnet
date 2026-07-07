# AI 협업 가이드 (Unity 팀 · Cursor)

문서 두 개: `.cursor/rules/main.mdc`(Cursor가 `CLAUDE.md` 자동 로드) / **이 문서**(사람이 읽음).

---

## 폴더 규칙

- 내 코드 → `Assets/MemberWorkspace/[이름]/`
- 내 기록 → `Docs/Member/[이름]/SEQUENCE.md` + `Docs/Member/[이름]/Sequence/phaseN.md`
- 공용 문서(`Docs/DESIGN.md`, `Docs/TODO.md`) → 팀 합의로만 수정 (`TODO.md`는 각자 `## [이름]` 섹션만)
- Cursor는 항상 **프로젝트 루트**에서 연다.

## 문서 역할

- `DESIGN.md` — 확정된 팀 규칙·Phase 표 (자주 안 바뀜)
- `TODO.md` — 아직 안 정한 것 (정해지면 DESIGN으로)
- `SEQUENCE.md` — 내 Phase 인덱스
- `Sequence/phaseN.md` — 그 Phase에서 **무엇을 했는지 + 어떤 코드를 볼지**

## Phase 워크플로

`DESIGN.md` 기준 · 한 번에 Phase 하나: **계획 → OK/진행해 → 구현 → phaseN.md 갱신 → 본인 확인 후 ✅**

## Unity MCP 연결 (Cursor)

1. Unity → `Window → MCP for Unity`
2. `Configure All Detected Clients` (최초 1회)
3. Cursor → `Settings → MCP` → `unityMCP` ON
4. Unity MCP **Connected** 확인 후 채팅 시작

디버깅: `read_console`만. 에이전트 **play 금지**.

---

## 프롬프트

`[이름]` `[번호]` `[N-1]` `[기능]` 을 본인 값으로 바꿔서 **해당 블록만** 복사한다.

---

### 1) 작업 시작 (매번)

**언제:** 새 채팅 열 때, 작업 시작할 때

```
나는 [이름]. 내 코드는 Assets/MemberWorkspace/[이름]/,
기록은 Docs/Member/[이름]/SEQUENCE.md + Sequence/phaseN.md.
공용 설계는 @Docs/DESIGN.md 참고.
Unity MCP 사용, 디버깅은 read_console(콘솔 로그)만.
한 번에 한 Phase만. 남의 폴더 건드리지 마.
```

---

### 2) 기능 추가 (코드 X, Phase 설계만)

**언제:** 새 기능 넣기 전, Phase 쪼갤 때

```
[기능] 추가하고 싶어. 코드 짜지 말고 먼저 Phase로 나눠줘.
각 Phase마다 목표/완료 조건/만질 파일 범위까지.
괜찮으면 "Phase 1 시작" 할게.
```

---

### 3) Phase 구현

**언제:** Phase 본격 구현할 때 (계획 → OK → 구현)

```
Phase [번호] 구현해줘. 관련 파일만, 전체 코드 다시 X.
먼저 한국어로 계획(목표/파일/책임/검증)만 보여줘.
내가 OK 하기 전까지 코드/에셋/씬 건들지 마.
OK 한 뒤에 구현하고, 끝나면 Sequence/phase[번호].md랑 SEQUENCE.md를 갱신해줘.
```

---

### 4) 새 대화 이어가기

**언제:** 채팅 새로 열고 이전 Phase 이어갈 때

```
진행 중 프로젝트. 나는 [이름].

@Docs/DESIGN.md
@Docs/Member/[이름]/SEQUENCE.md
@Docs/Member/[이름]/Sequence/phase[N-1].md

지금 Phase [번호] 할 거야. 이어서 도와줘.
```

---

## 규칙 (꼭)

- 새 기능은 항상 Phase로 쪼개서 진행.
- 한 번에 한 Phase만. 다음 Phase는 내가 시킬 때 시작.
- 내 폴더만 건드리기 (`MemberWorkspace/[이름]/`, `Docs/Member/[이름]/`).
- Phase 구현 전: **계획 → 내가 승인 → 구현**.
- Unity MCP로는 **플레이 금지**, 디버깅은 콘솔 로그(`read_console`)만.
- AI가 짠 코드는 내가 이해한 뒤에만 커밋.
- 새 대화 시작할 땐 문서를 몽땅 붙이지 말고, **필요한 부분만** `@파일`로 참조.

## Cursor 팁

- `Ctrl+L` 채팅 · `Ctrl+I` 인라인 편집
- `@DESIGN.md` 로 파일 참조
- MCP 안 보이면 `Settings → MCP` 토글 확인

## 더 보기

`Docs/DESIGN.md` · `Docs/TODO.md` · `Docs/README.md` · `CLAUDE.md`
