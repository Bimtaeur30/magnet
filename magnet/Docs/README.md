# Magnet (MAGNET SQUARE)

슈퍼센트 활동 — **하이퍼 캐주얼 블록 퍼즐** 프로토타입 (Unity 6, 3주)

## 한 줄 소개

보드 중앙의 **자석 축**에 블록을 붙여 **정사각형 테두리**를 완성해 터뜨리고, 매 턴 보드가 **90° 회전**하는 모바일 퍼즐 게임.

## 문서

| 문서 | 설명 |
|------|------|
| [AI_COLLAB_GUIDE.md](./AI_COLLAB_GUIDE.md) | **팀원용** — Cursor·Claude·Codex 공통 프롬프트, MCP, 구현·Phase·Sequence |
| [DESIGN.md](./DESIGN.md) | 게임 규칙, 시스템, 기술 아키텍처, **마일스톤(M0~M10)**, Jira 매핑 |
| [INSPECTOR_TOOLTIPS.md](./INSPECTOR_TOOLTIPS.md) | `[SerializeField]` Tooltip 규칙·멤버별 필드 목록 |
| [TODO.md](./TODO.md) | 팀원별 할 일 (자기 섹션만 수정) |
| [CLAUDE.md](../CLAUDE.md) | AI 코딩 규칙 (Cursor가 `.cursor/rules/main.mdc`로 자동 로드) |

## 외부 연동

| 도구 | 설정 |
|------|------|
| Unity MCP | `~/.cursor/mcp.json` → `unityMCP` |
| Jira (SCRUM) | `~/.cursor/mcp.json` → `Atlassian-MCP-Server` · [Backlog](https://bimtaeur30.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog) |

## 팀 Workspace

| 멤버 | 코드 | 작업 기록 (Unity `Assets/` 안) |
|------|------|--------------------------------|
| JTH | `Assets/_MemberWorkspace/JTH/` | `Docs/IMPLEMENTATIONS.md` · `Docs/Implementations/[slug]/` |
| KTJ | `Assets/_MemberWorkspace/KTJ/` | `Docs/IMPLEMENTATIONS.md` · `Docs/Implementations/` |
| PMS | `Assets/_MemberWorkspace/PMS/` | `Docs/IMPLEMENTATIONS.md` · `Docs/Implementations/` |
| PTY | `Assets/_MemberWorkspace/PTY/` | `Docs/IMPLEMENTATIONS.md` · `Docs/Implementations/` |

팀 공용 설계: `Docs/DESIGN.md` · AI 규칙: `CLAUDE.md` (프로젝트 루트)

> 개인 작업 기록은 예전 `Docs/Member/[이름]/` 가 아니라 **각자 `_MemberWorkspace/[이름]/Docs/`** 에 둡니다. (`Docs/Member` 폴더는 삭제됨)

## 기술 스택

- Unity **6000.3** + URP 2D
- [Reflect](https://github.com/gustavopsantos/reflex) (DI)
- [UniTask](https://github.com/Cysharp/UniTask)
- [LitMotion](https://github.com/annulusgames/LitMotion)
- Unity Input System, Cinemachine

## 시작하기

1. Unity에서 `magnet` 프로젝트 열기
2. `Docs/DESIGN.md` 마일스톤·게임 규칙 확인
3. Jira·`IMPLEMENTATIONS.md` 기준으로 구현 → Phase → Sequence 단위 작업
4. **타인 `_MemberWorkspace` 수정 금지**

## 레퍼런스

- Block Blast! — 블록 퍼즐 기본 골격
- 무한의 계단 — 점수 구간 스킨 해금
