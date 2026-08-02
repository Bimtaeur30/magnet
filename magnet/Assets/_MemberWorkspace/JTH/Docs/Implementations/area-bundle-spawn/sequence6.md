# sequence6 — Unique 동적 Unlock 패턴

## 1 — 2026-08-02 · Unique 번들 폐기 → Unlock 생성기

**바뀐 것**

- 생성: `UniqueUnlockGenerator` — 1개 당장 불가 + 2개 배치 가능 + 라인 클리어로 해제, 슬롯=[blocked,u0,u1]
- 수정: `AreaBundleOrchestrator` — Unique 리스트 제거, 동적 생성 실패 시 Normal→Easy
- 수정: `AreaBundlePoolSO` — `uniqueBundles` 제거, `uniqueSampleCount` 추가

**조건**

- blocked: `!CanPlaceAnywhere`
- unlock 둘: 둘 다 `CanPlaceAnywhere`
- unlock 경로에 **클리어 ≥1** 후 blocked 배치 가능
