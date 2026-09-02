# 블록 선택 알고리즘 — 버전·의도 전수 정리

> \\\*\\\*작성:\\\*\\\* 2026-08-31  
> \\\*\\\*목적:\\\*\\\* 자소서용. 채팅 지시 + sequence 기록 + Git 커밋을 맞춰, 시도한 알고리즘을 세대·버전별로 빠짐없이 정리.  
> \\\*\\\*대상 코드:\\\*\\\* Magnet 8×8 라인클리어, 턴당 3슬롯 핸드 공급.  
> \\\*\\\*현재 배선:\\\*\\\* `area-bundle-spawn` Phase 40 (`AreaBundleOrchestrator` + `LineFillHeatmap`).

\---

## 0\. 이 문서가 커버하는 것 / 한계

### 커버

|출처|범위|
|-|-|
|JTH Docs (`IMPLEMENTATIONS.md`, 각 slug의 `phases`/`phaseN`/`sequenceN`, `SPEC.md`, `BLOCKBLAST\\\_ANALYSIS.md`)|가장 완전. 사용자 요청은 sequence 「이유」에 압축되어 있음|
|Git (`origin` = `https://github.com/Bimtaeur30/magnet.git`, 브랜치 `JTH` = `main`)|커밋 메시지·날짜. **PR은 0건** (전부 JTH 직접 푸시)|
|Cursor 로컬 채팅 인덱스|제목·스니펫. 핵심 발화는 아래에 인용|
|이 워크스페이스 Agent transcript|Agent 세션 전문|

### 한계 (정직하게)

* Composer/Ask 채팅 전문을 사이드바처럼 **한 번에 dump**할 수는 없음. 키워드 검색 + 문서 기록이 본체.
* 예전 폴더(`magnet` Unity 루트)와 지금 폴더(git 루트)는 Cursor 인덱스가 겹침. 스크린샷의 `Unique placement world positions`, `Notion content review`도 인덱스에 있음.
* 같은 PC의 **다른 게임**(Headline, Teckdeck) 채팅이 검색에 섞임. 이 문서는 Magnet 스폰만.
* GitHub Issue/PR 없음. 알고리즘 히스토리의 공식 기록은 **커밋 + JTH Docs**.
* `area-bundle` Phase 5·6은 `phaseN.md`가 없고 sequence만 있음. Phase 23\~40은 커밋 메시지에 페이즈 번호가 거의 없음 (`chore: 알고리즘 튜닝` 한 방에 15 phase).

\---

## 1\. 자소서용 한 페이지 — 왜 이렇게 많이 갈아엎었는가

한 문장:

> Block Blast류 8×8 퍼즐의 \\\*\\\*다음 3블록을 고르는 문제\\\*\\\*를, 균등 랜덤 → 보드상태 티어스택 → 원작 역공학 체인 → 하이브리드 → Area 점수 번들 → 라인필 히트맵까지 \\\*\\\*5세대\\\*\\\*로 다시 정의했다. 매 세대의 이유는 “더 똑똑한 모델”이 아니라 \\\*\\\*플레이 체감이 원작과 어긋난다\\\*\\\*는 피드백이었다.

반복된 설계 질문 (전부 채팅/grill에서 본인이 던진 것):

1. **Death를 줄 것인가** — “즉사 패는 없다”로 시작 → Unique는 Death%를 높이려고 작은 블록을 씀 → 최종 Normal은 Death 배제조차 버리고 히트맵만.
2. **유일수(Brilliant escape)란 무엇인가** — 처음엔 솔버 `count==1`. 리스트를 준 적은 없고 “준 자료에서 유일수인 건 분류해 번들 만들어줘”만 있었음. 정적 리스트 대기 → **동적 Unlock**(막힌 1 + 자유 2, 둘로 라인클리어 후 막힌 피스 개방). 큰 블록 유일수는 “자리가 뻔해서 너무 쉽다”.
3. **접대·올클을 따로 줄 것인가** — Hospitality/Momentum/AllClear Exact까지 갔다가 Phase 40에서 **선택 경로 전부 삭제**. 올클은 히트맵 시뮬 중 보드가 비면 즉시 채택하는 부산물만 남김.
4. **원작을 얼마나 복원할 것인가** — 344프레임 사진 분석 + 친구 핸드오프(C++ 1370 미복원 → 근사) → “Health/Blame은 너무 복잡하다” → 스크린샷 번들 풀 → 결국 **줄 채움 근접도**라는 더 단순한 점수로.
5. **복잡도를 죽일 용기** — Phase 16은 잘못된 항을 지움(17에서 복구). Phase 7은 size/변을 버렸다가 8에서 복구. Phase 40은 Area·접대·올클·Clean·Death **수개월치 점수 기계를 삭제**.

자소서에 쓰기 좋은 역량 태그: 시스템 디자인, 플레이테스트 루프, 역공학, 솔버/탐색 예산, **자신의 설계를 폐기하는 판단**, 튜닝 가능한 노브 vs 하드 규칙.

\---

## 2\. 세대 지도

```text
Gen0  random-block-spawn          2026-07-07 \\\~ 07-21
      균등 랜덤 · 3슬롯 공급 인프라. 알고리즘 TBD.
        │
        │  (게임 자체를 Block Blast로 피벗: 07-27)
        ▼
Gen1  block-selection-algorithm   2026-07-30 SPEC → 08-01 Phase1\\\~9
      BoardHealth + Blame · 9티어 스택 · 344프레임 사진 반영
        │  “원작이랑 패가 너무 다름” / 프리필 넣었다가 본인이 전부 삭제
        ▼
Gen2  blockblast-handoff          2026-08-01\\\~02
      원작 1.3.71 역공학 체인 7 → 1370근사 → 2100. 42-ID. Health 배선 제거
        │  “길고 큰 블록이 안 나온다”
        ▼
Gen3  hybrid-spawn-algorithm      2026-08-02
      BaseChain(핸드오프) + 특수 티어 5종 게이트. 번들 SO 폐기
        │  채팅: “BoardHealth랑 Blame은 너무 복잡한 것 같음”
        ▼
Gen4  area-bundle-spawn           2026-08-02 \\\~ 08-13  Phase 1\\\~40
      Area 점수·번들 풀·Unique Unlock · (중간에 접대/올클/Death/Clean)
      Phase 40: 라인필 히트맵으로 전면 단순화  ← 현재
        │
        └─ Relife (08-16) Easy 패 이어하기 · UniqueCorrectPlacementEvent
```

Git에서 세대가 커밋 메시지에 드러나는 방식:

|커밋|날짜|메시지|실제 내용|
|-|-|-|-|
|`3cf26df`|07-07|feat: Block Spawn System|Gen0 Draw|
|`de4d8db`|07-07|feat: Block 공급 시스템|Gen0 Supply · **가중치 당일 취소**|
|`d4daea2`|07-15|fix: 선택지 4개, 턴 개념 추가|4슬롯 (이후 3으로 복귀)|
|`d1223ab`|07-30|…블럭 선택 알고리즘 기획서 완성|SPEC.md|
|`3e3c0b4`\~`1c91b1d`|08-01|Phase 1\~7 feat|Gen1 본체|
|`405cd4a`|08-01|밸런싱|Gen1 Phase 8\~9 (번호 없음)|
|`5232801`|08-02|fix: 선택 알고리즘 수정|Gen2 핸드오프|
|`d308ff2`|08-02|블록 선택 알고리즘 v3|Gen3 하이브리드|
|`39179ae`|08-02|v4|Gen4 Phase 1\~6|
|`03dfa9e`|08-02|fix: 블럭 선택 알고리즘\*\*(최종)\*\*|**거짓 종점** — 이후 Phase 9\~40 계속|
|`babdabb`|08-13|feat: 블록 선택 알고리즘 완성|Phase 40 히트맵|
|`91b9594`|08-16|feat: Relife 시스템|Easy 이어하기|
|`edaeffa`|08-16|feat; UniqueCorrectPlacementEvent|유일수 정답 배치 이벤트|

\---

## 3\. Gen0 — `random-block-spawn` (인프라, 알고리즘 아님)

**의도:** SCRUM-18. 형태 계약과 3후보 공급만. DESIGN §4.9 스폰 알고리즘은 TBD.

|Phase|날짜|무엇을|본인 의도|결과|
|-|-|-|-|-|
|1|07-07|`IBlockShape` 계약, Presets|PTY 에디터와 인게임 로직 분리|Gen1\~4 형태 기반|
|2|07-07|`BlockDrawer` 균등 추첨, `IRandom`|나중에 Draw 내부만 교체(OCP)|시드 결정론|
|3|07-07|`BlockSupply` 3슬롯, Consume 후 해당 슬롯만 리필|DESIGN “항상 3개”|이후 Fill은 핸드 소진 시|
|**4 취소**|07-07|가중치 SO|**“프로토타입에서 출현 가중치 불필요”** — 당일 구현 후 삭제|균등만 유지. 가중치는 Gen1에서 티어별로 재등장|
|5|07-14|SlotCount=4, 핸드 소진=턴|턴 FSM|v0.7에서 3슬롯로 되돌림|
|6 계획|—|BlockBlastPoolSO 8종|PTY 풀과 원작 8종 불일치|미완. Gen2가 42-ID로 대체|

\---

## 4\. Gen1 — `block-selection-algorithm` (9티어 · Health/Blame)

**기획서:** `SPEC.md` (2026-07-30 grill).  
**한 줄:** 순수 RNG가 아니다. **판 건강(BoardHealth)** 과 **최근 실수(Blame)** 로 티어를 고른다. **점수·콤보 수는 입력이 아니다.**

### 4.1 설계 원칙 (SPEC, 본인 확정)

* Death 없음: 스폰 직후 최소 1개는 놓을 수 있다.
* 번들(미리 만든 3세트) vs 실시간(Hospitality·Pressure만).
* 억지 블록(1×1, 1×2)이 필수면 그 티어 포기.
* 의도적 유일수는 **Pressure만**. 쉬운 유일수 티어 없음.
* Hospitality: 강한 기회만, 강해도 억지면 무시, 100% 안 줌(변덕).
* Domain 순수 (Unity/이벤트/DI 없이 솔버).

채팅 「Block Blast algorithm summary」에서 본인이 관찰한 유일수:

> 어려운 유일수: 2블록으로 라인 깨서 3번째(특히 3×3) 자리 마련  
> 쉬운 유일수: 라인 연속 클리어하다 보면 막혔던 블록이 들어감  
> 통과하면 \\\*\\\*"Brilliant escape"\\\*\\\*

### 4.2 티어 스택 (Phase 9 최종)

`Relife(0) → Trap(1) → ComboBreak(2) → Hospitality(3) → Momentum(3.5) → Easy(4) → Pressure(5) → Normal(6) → Fallback(7)`

|티어|의미|생성|
|-|-|-|
|Relife|이어하기 구제. 1×1은 여기만|번들|
|Trap|일부만 놓고 막힘 (`hasAny \\\&\\\& ¬fullSequence`)|번들, p≈0.8%|
|ComboBreak|살지만 이번 턴 클리어 불가|번들, p≈4%|
|Hospitality|강한 기회 패|**실시간** 빔서치|
|Momentum|콤보 중 큼직한 사각 (사진 분석 후 신설)|번들|
|Easy|Normal 중 콤보 유지 가능|번들 필터|
|Pressure|유일해 + 난이도 하한|**실시간** 솔버|
|Normal|건강 지향|후기엔 독립 가중 추첨|
|Fallback|아무거나 놓을 수 있는 것|실시간|

### 4.3 Phase별

|Ph|커밋|구현|본인 의도|
|-|-|-|-|
|1|`3e3c0b4`|`PlacementSolver`: HasAny / FullSequence / ComboMaintainable / CountFullSequences. 라인클리어 시뮬 필수|Trap/Unique/ComboBreak의 공통 기반|
|2|`4c39a16`|BoardHealth(fill, dead zone, big slot, freedom) → TooEmpty/Sweet/TooDirty. BlameTracker + GoodTurn|“점수는 입력이 아니다”. 꽉 찬 보드가 TooEmpty로 오판되면 Trap이 영구 폐쇄 → fill 우선 매핑으로 수정|
|3|`bf9a310`|번들 16종 + ShapeSampler. 1×1·1×2 가중 0|1×1은 Relife만|
|4|`eed5f3f`|BundleTierSelector, probe 8회|게이트는 Orchestrator, 여기선 조건만|
|5|`21fd48d`|HospitalityGenerator, OpportunityScorer, 빔폭 4|“강한 기회만. 변덕 0.75”|
|6|`b8d8294`|Pressure + UniqueSolution(엄지척 UI용 MatchesStep) + Orchestrator|“의도적 유일수는 Pressure만”|
|7|`1c91b1d`|Drawer 배선, `\\\[뽑기]`/`\\\[Blame]`/`\\\[BoardHp]` 로그|“왜 이 패가 나왔는지” 턴 정산에 로그. Relife는 `IsRetrySession=false` 스텁|
|8|`405cd4a`|클러스터 Health, Normal은 예측 Health 최고, Snug Fit(쏙 맞춤)×3|**“Block Blast 체감 맞추기 — 피드백 즉시 반영.”** 대각선·1×3 하향. “사방 밀폐가 쏙”|
|9|동일 커밋|아래 15연타|사진 344장 전수 (`BLOCKBLAST\\\_ANALYSIS.md`)|

### 4.4 Phase 9 플레이테스트 발화 (sequence9, 당일)

사진 분석 확정: 대각선 0/344, 중복 흔함, 콤보 중 3×3, 밀도 역상관(빈 보드=대형, 빽빽=얇은 조각).

그다음 **본인 입으로 즉시 수정한 것들:**

|#|발화|조치|
|-|-|-|
|2|“지금 번들이 실게임 패랑 너무 다름”|Normal 번들 13종 삭제 → 슬롯 독립 가중 추첨|
|3|“작은 ㄱ자(L3)가 너무 많이 나옴” + “쏙 맞는 블록이 너무 노골적”|L3 하향, Snug 완화|
|4|“데드존이 너무 쌔게 때림”|BlamePerDeadZone 20→8|
|5|“3x3이 3개 나왔는데 이거 뭐임”|`mom\\\_bigtriple` 삭제|
|6|“계속 네모난 블럭이 나오면서 점수 먹기가 너무 쉬워짐”|Momentum에 멀티라인 게이트, 확률 하향|
|7|“2x2가 3개 — 같은 블럭 3개는 안 나오게”|트리플만 재추첨, 페어는 허용|
|8|“Health가 늘어났으면 Blame도 줄어야 하는 거 아님?”|Health 개선 시 blame 차감|
|9|“배치 자유도 하락 감점이 너무 심함”|freedom drop 벌점 완화|
|10\~14|“처음에 채우는 게 없는 것도 블라스트랑 너무 다름” → “듬성듬성” → “너무 안빽빽” → “작은 블럭을 줘야 하는데 큰 블럭” → “시작하자마자 클리어 돼도 빽빽하게”|프리필 반복 수정|
|**15**|**“그냥 처음에 채우는 거 싹 다 없애줘”**|프리필 **전면 삭제**. 빈 보드 시작 복귀|

이 세대의 핵심 학습: **원작 관찰 → 규칙 추가 → 체감이 노골적이면 바로 후퇴.** 프리필은 원작과 맞추려다 본인이 폐기.

\---

## 5\. Gen2 — `blockblast-handoff-algorithm` (원작 체인 이식)

**자료:** 친구가 준 `BlockBlast\\\_AI\\\_Handoff\\\_5files` (PDF + 검증 500건 + restored\_profile.json). 원작 1.3.71.

**grill (08-01):**

* 주력 1370(C++ `all-combination fill`, native 14009)은 **미복원** → “완주 가능 + 라인 클리어 선호” **근사**.
* 블록 체계 **42-ID** (회전형이 별도 ID → 스폰 회전 없음).
* 관측된 핵심 체인만: 500건 중 1370=430, 7=69, 2100=1. 세션시간·광고·초보자·4001 계열 제외.
* 구 Orchestrator는 **삭제 안 함**, Bootstrap 배선만 교체.

**파이프라인:**

```text
base 7 (random-no-death)
  → round≥2, 90% → 1370 근사
  → 실패 → randomNoDie 셔플
  → \\\[1, random-placeable, 1] fallback
  → 2100 반복 억제 (최근 2라운드 교집합≥2)
  → delCurrentSameBlock (직전 트리플 완전 동일 시 가운데 교체)
```

Health/Blame **전부 제거**.

**즉시 피드백:** “원작 대비 길고 큰 블록이 안 나오고 작은 블록만 나옴.”  
원인: 500건 미관측 10종 제외가 합성 보드 왜곡 + “첫 통과 조합”이 소형 쏠림.  
조치: 대각 3칸(39\~41)만 제외, 칸 수 가중 샘플 120회, 중복 허용.

커밋: `5232801` `fix: 선택 알고리즘 수정`.

\---

## 6\. Gen3 — `hybrid-spawn-algorithm` (체인 + 특수 티어)

**의도 (grill 08-02):** Gen2만 있으면 Trap/Hospitality/Pressure 같은 **의도적 난이도 곡선이 사라진다.** 핸드오프를 Normal 자리에 두고, Gen1 특수 티어를 게이트로 얹는다.

* 42-ID 통일, 스폰 회전 없음.
* 티어 5종만: Relife · Trap · ComboBreak · Hospitality · Pressure. **Easy·Fallback·Momentum 제거** (체인이 대체).
* 번들 SO **폐기** → 전부 “42-ID 샘플 + 솔버 필터” 실시간.
* 특수 티어는 2100 반복억제를 **우회**하되 히스토리에는 기록.

커밋: `d308ff2` `블록 선택 알고리즘 v3`.

**전환 이유 (채팅 Easy Cascade Spawn):**

> “BoardHealth랑 Blame 같은 건 내가 볼 때 너무 복잡한 것 같음.”

이 한 줄이 Gen4의 출발점. Health 구간으로 게이트를 여는 대신, **고정 번들 리스트 + Area 점수**로 가겠다는 선언.

\---

## 7\. Gen4 — `area-bundle-spawn` (현재까지 40 Phase)

하이브리드를 통째로 내렸다. Unique / Normal / Easy cascade. Unique는 리스트를 받은 적이 없어서 동적 생성.

채팅에 반복된 확인:

> 유일수 전용 리스트는 준 적 없다.  
> 있었던 건 「준 자료에서 유일수인 건 따로 분류해서 번들 만들어줘」뿐.  
> `uniqueBundles`는 비워 두고 샘플 대기 → Phase 6에서 폐기하고 `UniqueUnlockGenerator`.

### 7.1 Phase 1\~20 — 점수식과 풀을 세우다

|Ph|날짜|커밋(묶음)|메커니즘|의도 / 왜|
|-|-|-|-|-|
|1|08-02|`39179ae` v4|Area = 4연결 size + 변 보너스|grill Area 점수 도메인 먼저|
|2|08-02|v4|Early20 / Normal40, MaxArea 선택|유일수 샘플 **사용자 제공 대기**|
|3|08-02|v4|Unique: seq 최소 → Death 최대 → Area. 생존 불가 시 랜덤 킬|유일수는 “덜 풀리고 더 위험한 손”|
|4|08-02|v4|`AreaBundleDrawer` 배선. Hybrid는 롤백용 보존|런타임 교체|
|5|08-02|v4 (phase md 없음)|Unique→Normal→Easy cascade. Early 삭제. Easy 26. 킬패는 Easy 가중랜덤만|Relife=Easy 1턴. dirty면 Unique|
|6|08-02|v4 (phase md 없음)|**Unique 번들 폐기 → Unlock 생성기** (막힌1+자유2, 클리어로 해제)|리스트 없이 보드 상태 기반 유일수|
|7|08-02|`cfa6c01`|Area = −(찬+빈 greedy 직사각 수). size/변 **폐기**|“사각형만으로 판정, 개수 최소화”|
|8|08-02|동일|size/변 **복구** + rect 패널티 k=5|7이 신호를 너무 버림|
|9|08-02|`ab38cb5`|Normal 우선: 올클 75% → 멀티 4줄50%/5줄100% → Area|“깔짝깔짝 Area만으론 시원한 폭발이 안 나온다.” **본인 요청 4@50 / 5@100**|
|10|08-02|동일|areaCountPenalty=4|영역이 쪼개질수록 감점|
|11|08-02|동일|Normal 59→195 (Blocks2 스크린샷 \~347장)|구 풀에 대형이 없어 올클이 안 나옴|
|12|08-02|동일|Hybrid/Health/Blame **코드 삭제** (Catalog·Simulator만 잔존)|AreaBundle이 Health를 안 씀|
|13|08-02|동일|Normal 195→27 (freq≥2만). 멀티는 5줄만|large freq1 예외가 “큼지막한 패”로 부풂. 4줄 우선이 큰 패를 끌어올림|
|14|08-02|동일|Normal **324 전수 평등**. 멀티 6줄. n203(id 0) 제거|“사진에 나온 것을 평등하게”. Phase13 freq 필터 철회|
|15|08-05|`4c6d196`|유일해 API·UniqueSolution 삭제|UI MatchesStep 미배선, 소비처 없음|
|16|08-08|`6048de6`|areaCount 패널티 **제거**|“rect와 역할이 겹친다” — **의도 오류**|
|17|08-08|동일|변 보너스 삭제, areaCount **복구**|16이 잘못된 항을 지움. 지울 것은 side bonus였다|
|18|08-08|동일|rect 패널티 = **찬 칸만**|빈 칸 직사각은 노이즈|
|19|08-10|`d46fa01`+docs|올클 고정 12번들 + Exact DFS (빔 폐기)|빔이 올클을 놓침(false negative). occ≤16만 Exact|
|20|08-10|동일|MultiClear → Hospitality(지금 놓으면 즉시 클리어)|6줄 멀티가 대형 패·인위적 폭발을 유발|

Phase 16→17은 자소서에 쓰기 좋은 사례: **가설이 틀렸음을 인정하고 항을 되돌림.**

### 7.2 Phase 21\~39 — 접대·피스 빈도·Area 정의 전쟁

|Ph|날짜|메커니즘|본인 의도 (sequence/채팅)|
|-|-|-|-|
|21|08-10|접대 = **거의 막힌 구멍에 Exact로 쏙** (윤곽 채움≥70%)|Phase20 “즉시 클리어”는 **의도 불일치**. 원하는 건 구제 핏|
|22|08-10|접대 allowlist (1–2칸·2×2·3×3·6칸 금지, 3칸 가중 절반)|“접대가 너무 쉽게/사각으로”|
|23|08-10|`shapeWeights` × Area. **당일 28회 튜닝**|L3/I3/I4/I5/3×3/2×3 과다. 가중만으론 약함 → 번들 하드밴·접대 FitWeight=0. L3를 올렸다가 **“실제로는 과다” 즉시 정정**. 작은 ㄱ 계열 억제 (L3만이 아니라 L4·L5도)|
|24|08-10|Death를 점수 항으로 넣었다가 **표시/API 삭제**|전수 CountSequences가 렉. 디버그 불필요|
|25|08-10|Death% 배제(상위 8손, 30%, 분모 예산 48)|함정 손이 Area 1등. 예산 초과면 통과(렉 방지). 올클 occ 16→24 (중반에 안 뜸)|
|26|08-10|Normal Clean/Main 2모드 + Clean 체이닝 40%|판이 깨끗하면 올클 친화, 더러우면 생존 튜닝. 최적 수 연속 선뽑|
|27|08-11|Unique 가중 + **강A 우선**(둘 놓기 전 하나로는 언락 불가)|Main/Clean과 Unique 분포 분리. 쉬운 단독 언락을 뒤로|
|28|08-11|패 선택 Explain 기즈모 (채움은 와이어만)|“지금 패가 뭔지 / 어디에 넣었는지”. 실블록처럼 보이면 오해|
|29|08-11|찬 Area = 두꺼운 코어 + 1칸 돌출|폭1 다리로 두 덩어리가 한 Area가 되는 꼼수|
|30|08-11|Clean 체인: 현재 보드에서 클리어≥1 아니면 폐기|플레이어가 최적 수를 안 두면 클리어 0 패가 나옴|
|31|08-11|Normal은 **라인클리어≥1 필수**. 올클 occ 24→12|클리어 0 Normal이 콤보 체감을 깨뜨림|
|32|08-11|찬 Area = 직교 이웃≥2만 (돌출 전부 분리)|“튀어나온 것도 Area에서 제외” — 29의 1칸 허용 철회|
|33|08-11|**빈 보드**면 Normal 가중랜덤 (올클 패 다음이 아님)|빈 보드 Clean Area가 대형 패를 고정|
|34|08-11|greedy 개수 패널티 삭제 → **모서리 최소 덮개 면적** × k|본인이 원하는 상수×면적 형태|
|35|08-11|CornerRect Stage 0.5/1/2/4를 에이전트가 바꿔 가며 플레이 비교|체감으로 k 확정|
|36|08-12|직교≥2 폐기, **다리(bridge) 절단** (양쪽≥4)|짧은 L까지 쪼개지던 문제 vs 폭1 다리 꼼수|
|37|08-12|전 후보 MaxArea DFS → 빔 근사 + top-4 정밀 (100\~450ms→\~50ms)|Select의 70\~90%가 MaxArea. 접대 스파이크 \~158ms|
|38|08-12|다리절단 **삭제**, 직교볼록 홈 절단. MaxNotchDepth 2→1→**0**|두꺼운 U 바닥은 다리로 안 잘림. “U홈은 두 Area, 계단 L은 유지”. 얕은 홈(F0)도 한 Area로 남으면 안 됨|
|39|08-13|Unique **4칸 테트로 중심** 가중. live 기즈모 당일 추가→삭제|채팅: **“유일수가 너무 쉬움. 큰 블럭이라 놓을 곳이 뻔함. 작은 블럭으로 Death%를 높여야 함.”** 소형만=해가 많고 대형만=뻔함 → 4칸 균형|

Phase 23은 “같은 범주까지 억제”의 원형: 작은 ㄱ만이 아니라 큰 ㄱ·일자 구멍의 I4까지 접대에서 막음.

Phase 29→32→36→38은 **Area가 무엇인가**를 네 번 재정의한 기록이다. 자소서에서 “한 점수를 네 모델로 검증했다”고 쓸 수 있다.

### 7.3 Phase 40 — 현재 알고리즘 (커밋 `babdabb` 08-13 “완성”)

**의도:** Area MaxArea + 접대 + 올클 Exact + Clean + Death = 복잡·느림·튜닝 불가.  
Normal 점수를 **라인 채움 근접도(히트맵)** 로 바꾸고 cascade를 3단으로 자른다.

채팅 「Combo event position vector」에서 히트맵 폴백도 확정: Normal 완주 가능 손이 0이면 Easy 히트맵, 그마저 없으면 가중랜덤 킬 폴백.

#### Cascade (코드 `AreaBundleOrchestrator.Select`)

```text
Relife 세션이고 초반 턴 → Easy 강제
occupied ≥ 40 이고 p\\\_unique 당첨 → UniqueUnlockGenerator
  실패/스킵 → Normal 히트맵 (풀 셔플 후 최대 64개 점수)
    후보 0 → Easy 히트맵
      후보 0 → Easy 가중랜덤 (isKillHand)
```

**제거:** 접대, 올클 Exact 풀, Clean/Main, 체이닝, Area MaxArea, Death 배제, shapeWeights 선택, 라인클리어 필수 게이트.

#### 라인필 히트맵

행·열마다 `score = n − empty`. 완전 찬/빈 줄은 0.  
그 줄의 **빈 칸 중, 옆이 찬 칸**에만 heat를 더함. 가로+세로 합산.

손 점수: 3! 순열 × 매 수 직전 히트맵 재계산 × 각 피스 greedy 최고 자리.  
`gain = Σheat − emptyPen × (heat==0인 칸 수)`.

emptyPen은 점수에 비례해 0→2로 램프 (maxScore 기본 3000).  
**초반엔 큰 피스·허공 허용, 후반엔 허공 감점.**

올클: 점유>0인 보드에서 시뮬 중 occ=0이 되면 히트맵 무시하고 **즉시 채택**. 빈 보드에서는 연속 올클 방지로 금지.

Unique: dirty = **occ≥40** (Area 점수 게이트 폐기). 생성기는 Phase 27·39 유지, `uniqueShapeWeights` 4칸 중심.

#### Phase 40 직후 (새 slug, 선택 본체는 유지)

|커밋|내용|
|-|-|
|`91b9594` 08-16|Relife: 게임오버 직전 점수≥n이면 Easy 패 오퍼, 수락 시 그 패로 이어하기 (세션 1회)|
|`edaeffa` 08-16|유일수 정답 위치에 놓으면 월드좌표 이벤트 (`UniqueCorrectPlacementEvent`) — UI용. 선택 알고리즘 변경 없음|

08-16 이후 스폰 Domain 커밋 없음.

\---

## 8\. 되풀이된 실패 패턴 (자소서 ‘학습’)

1. **원작을 규칙을 늘려 맞추면 노골적이 된다** — Snug Fit, Momentum 3×3 트리플, 프리필, 접대 Exact.
2. **필터를 조이면 풀이 왜곡된다** — Normal freq≥2(27개) vs 전수 평등(324) vs Blocks2 병합 561.
3. **점수 항을 겹쳐 넣으면 무엇이 이겼는지 모른다** — size/변/rect/areaCount/corner/Death. Phase 16은 잘못된 항을 지움.
4. **탐색은 비싸다** — MaxArea 전 후보, CountSequences 전수, 올클 빔. 예산 캡 → 근사 → 결국 히트맵으로 대체.
5. **유일수는 데이터가 아니라 정의 문제** — 리스트 없음. “막힌 1 + 자유 2”로 조작적 정의를 만들고, 큰 피스면 너무 쉬워서 4칸으로 재가중.
6. **최종은 더 단순한 모델** — 9티어 Health도, 1370 근사도, Area 직사각도 남기지 않고 **줄에 얼마나 가까운가**만 남김.

\---

## 9\. 현재 코드 맵 (Phase 40)

```
AreaBundleOrchestrator.Select
  UniqueUnlockGenerator          (occ≥40, uniqueShapeWeights)
  HeatmapHandScorer.ScoreBest    (3! × LineFillHeatmap)
  AreaBundlePoolSO               (Normal 561, Easy, uniqueMinOccupied=40,
                                  maxCandidatesToScore=64, emptyHeatPenalty 램프)
BlockSpawnBootstrap              Drawer 배선, Relife Easy, HandCompare(heat)
BlockBlastCatalog                42-ID (핸드오프에서 살아남은 공유 부품)
PlacementSimulator               PlaceAndClear (솔버 잔존)
```

삭제된 세대의 코드는 Phase 12에서 대부분 제거. Docs의 Gen1 SPEC / ANALYSIS / 구 phases는 **의도 기록**으로만 남음.

\---

## 10\. 채팅에서 뽑은 본인 발화 (문서에 안 적힌 것 위주)

|대화|발화|반영|
|-|-|-|
|Easy Cascade Spawn|Health/Blame이 너무 복잡하다|Gen4 Area 번들 골격|
|Easy Cascade Spawn|준 자료의 유일수는 따로 분류해 번들|uniqueBundles 슬롯 → 이후 동적 Unlock|
|Block Blast algorithm summary|Brilliant escape / 어려운 vs 쉬운 유일수|Gen1 Pressure, 이후 Unlock 강A|
|Phase 9 sequence|작은 ㄱ, 쏙, 데드존, 3×3 트리플, 프리필 삭제|Gen1 밸런싱|
|이상한 번들 제거|3칸이 너무 많이 나옴. 유일수는 나중에|가중·밴, Unique 후순위|
|Combo event position vector|유일수가 큰 블록이라 너무 쉽다. 작은 블록으로 Death%|Phase 39 4칸 가중|
|Combo event position vector|히트맵 완주 0이면?|Easy 히트맵 → 킬 폴백|
|Unique placement world pos|유일수 정답 위치 UI|UniqueCorrectPlacementEvent|
|패 지급 이유 기즈모|왜 이 패인지 Scene에서|Explain 기즈모 / Profile|

\---

## 11\. 자소서 초안 조각 (사실만, 과장 없이)

**문제.** Block Blast류에서 “다음 3블록”은 난이도·재미·세션 길이의 전부다. 순수 랜덤은 막히거나 시시해지고, 원작은 비공개다.

**행동.**

* 배치 솔버(완주·콤보·유일해)를 Domain 순수 함수로 먼저 만들었다.
* 원작을 두 갈래로 봤다: (1) 인게임 344프레임 전수, (2) 1.3.71 핸드오프 500건 + 미복원 1370 근사.
* 보드 건강/실수 기반 9티어를 직접 설계해 넣었다가, 본인이 “너무 복잡하다”고 폐기했다.
* 스크린샷에서 핸드를 집계해 번들 풀을 재구축하고, Area 점수를 네 모델(size+변, 직사각 개수, 모서리 덮개, 직교볼록 홈)로 갈아끼웠다.
* Unique는 외부 리스트 없이 “막힌 피스 + 클리어로 여는 두 피스”로 정의했다.
* 성능(MaxArea 수백 ms)과 튜닝 실패가 겹치자, 접대·올클·Death·Clean을 선택 경로에서 제거하고 라인필 히트맵으로 단순화했다.

**결과.** 현재 런타임은 Unique(점유≥40) / Normal 히트맵 / Easy 폴백 3단. 42-ID 카탈로그와 시뮬레이터는 공유 부품으로 남겼다. 구현 기록은 phase 40개 + 폐기된 3세대 Docs로 남아 있다.

쓰지 말 것: “완벽한 원작 복원”, “PR 리뷰를 이끌었다”(PR 0건), 팀 전체 알고리즘(본인 JTH 워크스페이스).

\---

## 12\. 관련 문서 인덱스

* `IMPLEMENTATIONS.md` — slug 상태
* `Implementations/block-selection-algorithm/SPEC.md` — Gen1 기획
* `BLOCKBLAST\\\_ANALYSIS.md` — 344프레임
* `BLOCK\\\_SELECTION\\\_TUNING\\\_GUIDE.md` — Gen1 튜닝 (현행 아님)
* `Implementations/area-bundle-spawn/phases.md` + `TUNING\\\_STAGES.md`
* `Implementations/relife/` — Easy 이어하기
* Git: `https://github.com/Bimtaeur30/magnet.git` 브랜치 `JTH`

