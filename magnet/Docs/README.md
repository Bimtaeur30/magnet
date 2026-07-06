# Magnet (MAGNET SQUARE)

슈퍼센트 활동 — **하이퍼 캐주얼 블록 퍼즐** 프로토타입 (Unity 6, 3주)

## 한 줄 소개

보드 중앙의 **자석 축**에 블록을 붙여 **정사각형 테두리**를 완성해 터뜨리고, 매 턴 보드가 **90° 회전**하는 모바일 퍼즐 게임.

## 문서

| 문서 | 설명 |
|------|------|
| [DESIGN.md](./DESIGN.md) | 게임 규칙, 시스템, 기술 아키텍처, **구현 Phase** |
| [TODO.md](./TODO.md) | 팀원별 할 일 (자기 섹션만 수정) |
| [CLAUDE.md](../CLAUDE.md) | 코딩·협업 규칙 (DI, UniTask, LitMotion, 폴더 소유권) |

## 팀 Workspace

| 멤버 | 코드 | 작업 로그 |
|------|------|-----------|
| JTH | `Assets/MemberWorkspace/JTH/` | `Docs/Member/JTH/SEQUENCE.md` |
| KTJ | `Assets/MemberWorkspace/KTJ/` | `Docs/Member/KTJ/SEQUENCE.md` |
| PMS | `Assets/MemberWorkspace/PMS/` | `Docs/Member/PMS/SEQUENCE.md` |
| PTY | `Assets/MemberWorkspace/PTY/` | `Docs/Member/PTY/SEQUENCE.md` |

## 기술 스택

- Unity **6000.3** + URP 2D
- [Reflect](https://github.com/gustavopsantos/reflex) (DI)
- [UniTask](https://github.com/Cysharp/UniTask)
- [LitMotion](https://github.com/annulusgames/LitMotion)
- Unity Input System, Cinemachine

## 시작하기

1. Unity에서 `magnet` 프로젝트 열기
2. `Docs/DESIGN.md` Phase 순서 확인
3. 담당 Phase 하나씩 구현 → 완료 후 표 상태 갱신
4. **타인 `MemberWorkspace` 수정 금지**

## 레퍼런스

- Block Blast! — 블록 퍼즐 기본 골격
- 무한의 계단 — 점수 구간 스킨 해금
