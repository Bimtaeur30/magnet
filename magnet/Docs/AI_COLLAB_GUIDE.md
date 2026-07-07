# AI 협업 가이드 (Unity 팀)



**Cursor · Claude · Codex** 공통. 도구별로 문서를 나누지 않고 이 한 파일만 따른다.



| 대상 | 문서 | 역할 |

|------|------|------|

| **사람** | **이 문서** (`Docs/AI_COLLAB_GUIDE.md`) | 프롬프트, MCP 연결, 구현·Phase·Sequence 워크플로 |

| **AI** | `CLAUDE.md` (프로젝트 루트) | 코딩 규칙·아키텍처·절차 (자동 로드) |



**AI 규칙 자동 로드**



- **Cursor** — `.cursor/rules/main.mdc`가 `CLAUDE.md`를 참조

- **Claude** — 프로젝트 루트에서 열면 `CLAUDE.md` 컨텍스트에 포함

- **Codex** — 동일하게 프로젝트 루트·`CLAUDE.md` 기준 (별도 가이드 불필요)



> 예전 경로 `Docs/Member/[이름]/`, `SEQUENCE.md`, `Sequence/phaseN.md` 는 **사용하지 않음**.



---



## 용어 (3단 계층)



| 용어 | 범위 | 문서 |

|------|------|------|

| **마일스톤 (M0~M10)** | 게임 **전체** 기능 영역 | `Docs/DESIGN.md` §8 |

| **구현** | 개인 기능·Jira·요청 (예: 인벤토리, 블록 좌표) | `IMPLEMENTATIONS.md` |

| **Phase** | 그 구현을 쪼갠 단계 | `Implementations/[slug]/phases.md` |

| **Phase** | 구현을 쪼갠 단계 — **뭘 어떻게 구현하는지 자세히** (예: 구조 → 핵심 → 부가 기능) | `phaseN.md` |
| **Sequence** | 그 Phase에서 **뭐가 바뀌었는지** 순서대로 기록. **Phase와 1:1 파일**, 항목은 한 파일에 쌓음 | `sequenceN.md` |



---



## 폴더 규칙



| 구분 | 경로 |

|------|------|

| 내 코드 | `Assets/MemberWorkspace/[이름]/` |

| 내 구현 인덱스 | `Assets/MemberWorkspace/[이름]/Docs/IMPLEMENTATIONS.md` |

| 구현별 Phase 인덱스 | `.../Implementations/[slug]/phases.md` |

| Phase 계획 | `.../Implementations/[slug]/phaseN.md` |
| Sequence 변경 기록 | `.../Implementations/[slug]/sequenceN.md` (Phase와 1:1) |

| 공용 설계 | `Docs/DESIGN.md` |

| 공용 TODO | `Docs/TODO.md` — 각자 `## [이름]` 섹션만 수정 |

| AI 규칙 | `CLAUDE.md` (프로젝트 루트) |



- AI·에디터는 **항상 프로젝트 루트**(`magnet/`)에서 연다.

- 개인 기록은 `Assets/` 안에 있어 **Unity Project 창**에서도 바로 열 수 있다.

- **남의 `MemberWorkspace` 폴더는 건드리지 않는다.**



### 공용 문서 수정



- `Docs/DESIGN.md` — 팀 합의 후 수정 (게임 규칙·**마일스톤** 표)

- `Docs/TODO.md` — 아직 안 정한 것. 본인 섹션만 편집



---



## 문서 역할



| 문서 | 내용 |

|------|------|

| `DESIGN.md` | 팀 규칙·게임 설계·**마일스톤(M0~M10)** |

| `TODO.md` | 미정 항목·개인 할 일 (Jira 이슈 등) |

| `IMPLEMENTATIONS.md` | 내 **구현** 인덱스 |

| `phases.md` | 한 구현의 **Phase** 인덱스 |

| `phaseN.md` | Phase 계획 — **뭘 어떻게 구현하는지** 상세 |
| `sequenceN.md` | Phase와 **1:1** — **뭐가 바뀌었는지** `## N — 날짜 · 제목` 으로 순서대로 |

| `CLAUDE.md` | AI 행동 규칙 (사람이 매번 읽을 필요 없음) |



---



## 워크플로



**한 번에 Phase 하나만.**



```

계획 제시 → OK/진행해/허락 → 구현 → sequenceN.md(변경 기록)·phaseN.md → phases.md → IMPLEMENTATIONS.md 갱신 → 본인 확인 → DESIGN/TODO ✅

```



- 구현 전: AI가 **한국어로 계획**(구현·Phase·파일·책임·검증)만 보여준 뒤, **승인 전까지 코드·에셋·씬 수정 금지**

- `「Phase N 시작」`만으로는 승인이 아님. 계획을 본 뒤 **「OK」「진행해」「허락」** 등으로 별도 승인

- 다음 Phase는 **본인이 시킬 때** 시작



---



## Unity MCP 연결



### Unity 쪽



1. Unity에서 **MCP for Unity** 창 열기 — `Window → MCP for Unity` 또는 **Ctrl+Shift+M**

2. **Configure** → AI 도구 MCP **ON**

3. **Start Session** → **Connected** 확인



### AI 도구 쪽



- **Cursor** — Settings → MCP → `unityMCP` (`~/.cursor/mcp.json`)

- **Claude** — `/mcp` 로 서버 확인

- **Codex** — 프로젝트 MCP에 `unityMCP` 등록 여부 확인



### 디버깅 (토큰 절약)



- **`read_console`** 로 컴파일 에러·경고·로그만 확인

- AI **플레이 모드 진입 금지** (사용자가 명시할 때만 예외)

- 런타임 확인: **사용자 플레이** → 콘솔 로그만 읽기



---



## 프롬프트



`[이름]` `[slug]` `[번호]` `[기능]` 을 본인 값으로 바꿔 **해당 블록만** 복사한다.



### 1) 작업 시작



**언제:** 새 채팅을 열 때



```

나는 [이름]. 내 코드는 Assets/MemberWorkspace/[이름]/,

기록은 IMPLEMENTATIONS.md + Implementations/[slug]/phases.md + phaseN.md + sequenceN.md.

공용 설계는 @Docs/DESIGN.md 참고.

Unity MCP, 디버깅은 read_console만.

한 번에 한 Phase만. 남의 폴더 건드리지 마.

```



### 2) 새 구현 · Phase 설계 (코드 X)



**언제:** 새 기능·Jira 이슈를 받았을 때



```

[기능] 구현할 거야. 코드 짜지 말고 먼저 구현을 Phase로 쪼개줘.

각 Phase마다 목표/완료 조건/만질 파일 범위까지.

괜찮으면 "Phase 1 시작" 할게.

```



### 3) Phase 구현



**언제:** Phase 본격 구현할 때 (계획 → OK → 구현)



```

[slug] 구현 Phase [번호] 구현해줘. 관련 파일만.

먼저 한국어로 계획만 보여줘. OK 전까지 코드/에셋/씬 건들지 마.

OK 후 구현하고, sequence[번호].md에 뭐가 바뀌었는지 기록 → phase[번호].md → phases.md → IMPLEMENTATIONS.md 갱신해줘.

```



### 4) 새 대화 이어가기



**언제:** 채팅을 새로 열고 이전 작업을 이어갈 때



```

진행 중 프로젝트. 나는 [이름].



@Docs/DESIGN.md

@Assets/MemberWorkspace/[이름]/Docs/IMPLEMENTATIONS.md

@Assets/MemberWorkspace/[이름]/Docs/Implementations/[slug]/phases.md

@Assets/MemberWorkspace/[이름]/Docs/Implementations/[slug]/phase[N].md

@Assets/MemberWorkspace/[이름]/Docs/Implementations/[slug]/sequence[N].md



[slug] Phase [번호] 이어서 도와줘.

```



> 문서 통째 붙여넣기 X — **필요한 파일만 `@`로 참조**.



---



## 규칙 (꼭)



- 새 기능은 **구현 → Phase**로 쪼개고, Phase마다 **Sequence(변경 기록)** 를 남김

- **한 번에 한 Phase만** — 다음은 본인이 시킬 때

- **내 폴더만** 수정

- 구현 전: **계획 → 승인 → 구현**

- Unity MCP: **플레이 금지**, 디버깅은 **콘솔만**

- AI 코드는 **본인이 이해한 뒤에만** 커밋



---



## 더 보기



- [README.md](./README.md) — 프로젝트 개요·팀 Workspace 표

- [DESIGN.md](./DESIGN.md) — 게임 규칙·마일스톤

- [CLAUDE.md](../CLAUDE.md) — AI 상세 코딩 규칙


