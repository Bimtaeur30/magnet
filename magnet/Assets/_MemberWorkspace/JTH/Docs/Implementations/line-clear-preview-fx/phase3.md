# line-clear-preview-fx Phase 3 — 기본 스킨 쩌적 힌트

> **구현:** `line-clear-preview-fx`

## 목표 (완료 기준)

- [x] 보로노이 조각 셰이더로 칸이 불규칙하게 금이 가고 살짝 벌어짐
- [x] 힌트 클립이 `shatter`를 한 번 올리고 갈라진 채로 멈춤 (뾰잉뾰잉·루프 제거)
- [x] 아틀라스 스프라이트·SpriteMask·풀 반환에서 깨지지 않음
- [x] 기본 스킨 7색 HintClips에 쩌적 클립 연결

## 구현 내용

| 클래스/에셋 | 책임 |
|-------------|------|
| `BlockShatter.shader` | UV 보로노이 금·조각 이동. `_Shatter` 0이면 원본 |
| `BlockShatterHint` | 클립의 `shatter`를 MPB로 넣고 스프라이트 UV 렉트 전달 |
| `Block` | 힌트 시작/종료·스프라이트 교체 때 쩌적 적용/원복 |
| `DefaultHintShatter.anim` | 0.9초 루프 `shatter` 펄스 |

## 범위 밖

- 놓을 때 클리어 파티클
- 3D 함몰·속 재질
