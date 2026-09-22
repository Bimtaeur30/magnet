# 입체 텍스트 셰이더
## 적용
1. Hierarchy에서 TMP 텍스트 오브젝트를 선택한다.
2. Tools > KTJ > Text > Create Extruded Blue Material 또는 Create Extruded Yellow Material.
3. 전용 머티리얼이 Shaders 폴더에 생성되고 선택한 텍스트에 적용된다. 원래 폰트 아틀라스와 SDF 메트릭을 복사한다.
4. TMP Vertex Color는 흰색을 기준으로 한다. 기존 Vertex Gradient도 결과에 곱해진다.
5. Inspector에서 머티리얼 값을 조절한다. 원본 폰트 및 화면 크기가 없으므로 참조와 픽셀 단위 동일함을 보장하지 않는다.

| 속성 | 역할 |
|---|---|
| Face Color / Face Bottom Tint | 앞면 색 / 아래쪽 곱셈 틴트 |
| Gradient Local Y Min Max | 텍스트 로컬 좌표에서 그라데이션 범위. 생성 시 현재 글자 bounds 사용 |
| Outline Color / Thickness | 파란 외곽선 색과 두께 |
| Border Offset X/Y | 측면과 그림자 전체 이동량. Y 음수는 아래 |
| Extrusion / Total Shadow Offset | 전체 이동량 중 단단한 측면의 비율 |
| Extrusion Near Face / Side Color | 측면 시작 / 끝 색 |
| Edge Width / Strength / Light Direction | 가장자리 조명 폭, 강도, 방향 |
| Border Color / Dilate / Softness | 그림자 색, 팽창, 부드러움 |

## 제약
- SDF 폰트 전용, Unity 6 TMP 정점 레이아웃 기준.
- 16회 측면 샘플을 포함해 픽셀당 22회 아틀라스 샘플. 많은 텍스트에 적용하기 전 기기 GPU 측정 필요.
- 아주 깊은 돌출은 샘플 경계가 드러날 수 있다. 글자 아틀라스 padding 밖 효과는 표현할 수 없다.
- 잘림이 생기면 폰트를 더 넉넉한 SDF padding으로 생성하고 효과 깊이를 줄인다. Border Dilate는 Outline Thickness 이상을 시작값으로 사용한다.
- 여러 줄 또는 런타임 크기/정렬 변경 시 Gradient Local Y Min Max도 다시 맞춘다.
- 머티리얼 메뉴는 주 텍스트만 적용한다. 다른 폰트를 사용하는 fallback/material submesh는 각 아틀라스별 전용 머티리얼이 필요하다.
- 씬/프리팹에는 자동 적용하지 않았다. 사용자가 메뉴를 실행할 때 선택한 컴포넌트를 변경한다.

## 검증
Unity Editor에서 셰이더 supported=True, 오류 없음 확인.
UI clip rect / alpha clip 네 가지 조합의 SetPass(0)=True.
머티리얼 생성 메뉴 어셈블리 로드 확인. 실제 화면 외형, RectMask2D 동작, 모바일 빌드 및 참조 이미지 일치도는 미검증.
