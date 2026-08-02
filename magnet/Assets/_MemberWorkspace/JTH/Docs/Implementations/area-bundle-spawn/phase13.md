# phase13 — Normal freq≥2 · MultiClear 5줄만

## 목표
- Normal 풀에서 large freq=1 예외 제거 → **freq≥2만**
- Clear Priority 멀티클리어: **5줄 이상만** (4줄 soft 경로 삭제)

## 변경
- `AreaBundleStarterData.CreateNormal` — 195 → **27**
- `AreaBundleOrchestrator.TrySelectNormalPriority` — soft 제거, `hardMin`만
- `AreaBundlePoolSO` — `multiClearSoft*` 필드 삭제
- `DefaultAreaBundlePool` Fill Starter로 동기화

## 완료 기준
- [x] CreateNormal freq≥2
- [x] 4줄 멀티클리어 미발동
- [x] 문서·툴팁 동기화
