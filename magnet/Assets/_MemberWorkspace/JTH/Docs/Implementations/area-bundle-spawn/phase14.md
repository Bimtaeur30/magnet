# phase14 — MultiClear 6 · Normal 전수 평등 집계

## 목표
- 멀티클리어 문턱 **6**
- Blocks2 사진 패를 **빼거나 넣지 않고** 전부 동일 규칙으로 Normal에 넣음

## 정책
- batch1–7 전수 (344샷 → unique 325 → **무효 ID0 1건 제외 324**)
- **필터 없음** (large 우선·mega 제외·ShapeId 1/37+ 제외·모양 보너스 전부 폐기)
- `weight = clamp(관측횟수, 1..5)` 만

## 결과
- Normal **324**
- `multiClearHardMinLines = 6`
