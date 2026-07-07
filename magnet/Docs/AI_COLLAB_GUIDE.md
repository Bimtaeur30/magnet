# AI 협업 가이드 (Unity 팀)

**Cursor · Claude · Codex** 공통. 도구별로 문서를 나누지 않고 이 한 파일만 따른다.

| 대상 | 문서 | 역할 |
|------|------|------|
| **사람** | **이 문서** (`Docs/AI_COLLAB_GUIDE.md`) | 프롬프트, MCP 연결, Phase·Sequence 워크플로 |
| **AI** | `CLAUDE.md` (프로젝트 루트) | 코딩 규칙·아키텍처·Phase 절차 (자동 로드) |

**AI 규칙 자동 로드**

- **Cursor** — `.cursor/rules/main.mdc`가 `CLAUDE.md`를 참조
- **Claude** — 프로젝트 루트에서 열면 `CLAUDE.md` 컨텍스트에 포함
- **Codex** — 동일하게 프로젝트 루트·`CLAUDE.md` 기준 (별도 가이드 불필요)

> 예전 경로 `Docs/Member/[이름]/` 는 **사용하지 않음**. 개인 기록은 `Assets/MemberWorkspace/[이름]/Docs/` 에 둔다.

---

## 폴더 규칙

| 구분 | 경로 |
|------|------|
| 내 코드 | `Assets/MemberWorkspace/[이름]/` |
| 내 기록 (인덱스) | `Assets/MemberWorkspace/[이름]/Docs/SEQUENCE.md` |
| 내 기록 (Phase별) | `Assets/MemberWorkspace/[이름]/Docs/Sequence/phaseN.md` |
| 공용 설계 | `Docs/DESIGN.md` |
| 공용 TODO | `Docs/TODO.md` — 각자 `## [이름]` 섹션만 수정 |
| AI 규칙 | `CLAUDE.md` (프로젝트 루트) |

- AI·에디터는 **항상 프로젝트 루트**(`magnet/`)에서 연다.
- 개인 기록은 `Assets/` 안에 있어 **Unity Project 창**에서도 바로 열 수 있다.
- **남의 `MemberWorkspace` 폴더는 건드리지 않는다.**

### 공용 문서 수정

- `Docs/DESIGN.md` — 팀 합의 후 수정 (확정된 규칙·Phase 표, 자주 안 바뀜)
- `Docs/TODO.md` — 아직 안 정한 것 (정해지면 `DESIGN.md`로 옮김). 본인 섹션만 편집

---

## 문서 역할

| 문서 | 내용 |
|------|------|
| `DESIGN.md` | 확정된 팀 규칙·게임 설계·**Phase 표** |
| `TODO.md` | 미정 항목·개인 할 일 |
| `SEQUENCE.md` | 내 **Phase 인덱스** (상태·링크) |
| `Sequence/phaseN.md` | 그 Phase에서 **무엇을 했는지** + **어떤 코드를 볼지** |
| `CLAUDE.md` | AI 행동 규칙 (사람이 매번 읽을 필요 없음) |

---

## Phase 워크플로

`Docs/DESIGN.md` Phase 표 기준. **한 번에 Phase 하나만.**

```
계획 제시 → OK/진행해/허락 → 구현 → phaseN.md 갱신 → 본인 확인 → DESIGN/TODO ✅
```

- Phase 구현 전: AI가 **한국어로 계획**(목표·파일·책임·검증)만 보여준 뒤, **승인 전까지 코드·에셋·씬 수정 금지**
- `「Phase N 시작」`만으로는 승인이 아님. 계획을 본 뒤 **「OK」「진행해」「허락」** 등으로 별도 승인
- 다음 Phase는 **본인이 시킬 때** 시작

---

## Unity MCP 연결

### Unity 쪽

1. Unity에서 **MCP for Unity** 창 열기  
   - 메뉴: `Window → MCP for Unity`  
   - 또는 단축키: **Ctrl+Shift+M** (패키지 버전에 따라 다를 수 있음)
2. **Configure** → 사용 중인 AI 도구(Cursor / Claude 등) **MCP ON**
3. **Start Session**(파란 버튼) → **Connected**(초록) 확인

### AI 도구 쪽

- **Cursor** — Settings → MCP에서 `unityMCP` 토글 확인 (`~/.cursor/mcp.json`)
- **Claude** — 채팅에서 `/mcp` 로 서버 연결 확인
- **Codex** — 프로젝트 MCP 설정에 `unityMCP` 등록 여부 확인

연결 후 아래 **「작업 시작」** 프롬프트로 세션을 연다.

### 디버깅 (토큰 절약)

- Unity MCP **`read_console`** 로 컴파일 에러·경고·로그만 확인
- AI가 **플레이 모드 진입 금지** (사용자가 「플레이해서 확인해」라고 할 때만 예외)
- 런타임 확인이 필요하면 **사용자가 플레이** → 이후 콘솔 로그만 읽기

---

## 프롬프트

`[이름]` `[번호]` `[N-1]` `[기능]` 을 본인 값으로 바꿔 **해당 블록만** 복사한다.

### 1) 작업 시작

**언제:** 새 채팅을 열 때, 작업을 시작할 때

```
나는 [이름]. 내 코드는 Assets/MemberWorkspace/[이름]/,
기록은 Assets/MemberWorkspace/[이름]/Docs/SEQUENCE.md + Docs/Sequence/phaseN.md.
공용 설계는 @Docs/DESIGN.md 참고.
Unity MCP 사용, 디버깅은 read_console(콘솔 로그)만.
한 번에 한 Phase만. 남의 폴더 건드리지 마.
```

### 2) 기능 추가 (코드 X, Phase 설계만)

**언제:** 새 기능을 넣기 전, Phase를 쪼갤 때

```
[기능] 추가하고 싶어. 코드 짜지 말고 먼저 Phase로 나눠줘.
각 Phase마다 목표/완료 조건/만질 파일 범위까지.
괜찮으면 "Phase 1 시작" 할게.
```

### 3) Phase 구현

**언제:** Phase 본격 구현할 때 (계획 → OK → 구현)

```
Phase [번호] 구현해줘. 관련 파일만, 전체 코드 다시 X.
먼저 한국어로 계획(목표/파일/책임/검증)만 보여줘.
내가 OK 하기 전까지 코드/에셋/씬 건들지 마.
OK 한 뒤에 구현하고, 끝나면 Sequence/phase[번호].md랑 SEQUENCE.md를 갱신해줘.
왜 그렇게 짰는지 짧게 설명해줘.
```

### 4) 새 대화 이어가기

**언제:** 채팅을 새로 열고 이전 Phase를 이어갈 때

```
진행 중 프로젝트. 나는 [이름].

@Docs/DESIGN.md
@Assets/MemberWorkspace/[이름]/Docs/SEQUENCE.md
@Assets/MemberWorkspace/[이름]/Docs/Sequence/phase[N-1].md

지금 Phase [번호] 할 거야. 이어서 도와줘.
```

> 문서를 통째로 붙이지 말고, **필요한 파일만 `@`로 참조**한다.

---

## 규칙 (꼭)

- 새 기능은 **항상 Phase로 쪼개서** 진행
- **한 번에 한 Phase만** — 다음 Phase는 본인이 시킬 때
- **내 폴더만** 수정 (`MemberWorkspace/[이름]/`, `Docs/Sequence/` 등)
- Phase 구현 전: **계획 → 승인 → 구현**
- Unity MCP: **플레이 금지**, 디버깅은 **콘솔 로그(`read_console`)만**
- AI가 짠 코드는 **본인이 이해한 뒤에만** 커밋
- 새 대화 시작 시 문서 **몽땅 붙여넣기 X** — `@파일` 로 필요한 부분만

---

## 에디터·도구 팁

| 도구 | 팁 |
|------|-----|
| **Cursor** | `Ctrl+L` 채팅 · `Ctrl+I` 인라인 편집 · `@Docs/DESIGN.md` 파일 참조 |
| **Cursor MCP** | Settings → MCP 토글 · `~/.cursor/mcp.json` 에 `unityMCP` |
| **Claude** | 프로젝트 루트 워크스페이스 · `/mcp` 로 MCP 확인 |
| **Codex** | 프로젝트 루트에서 작업 · `CLAUDE.md` 규칙 공유 |

---

## 더 보기

- [README.md](./README.md) — 프로젝트 개요·팀 Workspace 표
- [DESIGN.md](./DESIGN.md) — 게임 규칙·Phase 표
- [CLAUDE.md](../CLAUDE.md) — AI 상세 코딩 규칙 (DI, UniTask, LitMotion, EventChannel 등)
