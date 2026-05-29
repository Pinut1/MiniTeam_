# 코드 리뷰 — 잘 구현된 부분 정리

> 작성일: 2026-05-29  
> 대상 브랜치: feature/pokemon-game, develop

---

## 포켓몬 미니게임 (`Scripts/MiniGame_Pokemon/`)

### 1. DialogueDB — 외부 라이브러리 없이 직접 JSON 파서 구현
**파일:** `DialogueDB.cs:61`

```csharp
static Dictionary<string, string> ParseFlatJson(string json)
{
    var pattern = new Regex("\"([^\"]+)\"\\s*:\\s*\"((?:[^\\\\\"]|\\\\.)*)\"");
    foreach (Match m in pattern.Matches(json))
        dict[m.Groups[1].Value] = Unescape(m.Groups[2].Value);
    return dict;
}
```

- Newtonsoft.Json 같은 외부 패키지 없이 정규식으로 플랫 JSON 파싱
- `Unescape()` 분리로 책임 명확
- 중복 컴포넌트 감지 시 자동으로 전용 오브젝트를 생성해 격리하는 방어 로직 포함

---

### 2. UIPanelSlider — `EnsurePos()` lazy init
**파일:** `UIPanelSlider.cs:26`

```csharp
void EnsurePos()
{
    if (posReady) return;
    if (rt == null) rt = GetComponent<RectTransform>();
    posReady   = true;
    visiblePos = rt.anchoredPosition;
    hiddenPos  = visiblePos + DirVector() * slideDist;
}
```

- 비활성 오브젝트는 `Awake()`가 호출되지 않는 Unity 특성을 정확히 이해하고 첫 사용 시점에 초기화
- `current` 코루틴 참조로 중간에 방향이 바뀌어도 이전 애니메이션 자동 취소
- `SlideIn/SlideOut` 즉시/코루틴 두 가지 API 모두 제공

---

### 3. PokemonGameController — MiniGameManager 없을 때 단독 테스트 분기
**파일:** `PokemonGameController.cs:63`

```csharp
public void OnGameClear()
{
    if (MiniGameManager.Instance != null)
        MiniGameManager.Instance.OnMiniGameClear();
    else
        Debug.Log("[Pokemon] Game Clear! (단독 테스트)");
}
```

- Hub 없이 포켓몬 씬만 단독 실행해도 에러 없이 테스트 가능
- `OnGameFail()`도 동일하게 처리

---

### 4. TrainerTrigger → TrainerPatrol 상속 구조
**파일:** `TrainerTrigger.cs`, `TrainerPatrol.cs`

- `virtual/override`로 `OnTriggerEnter2D` 교체 — 순찰형은 트리거 충돌 무시, 직선 시야 감지로만 배틀 진입
- `OnBattleEnd()` 오버라이드로 순찰 복귀 로직 분리
- `BattleManager`는 `TrainerTrigger` 인터페이스만 알면 되므로 기본형/순찰형 모두 동일하게 처리

---

### 5. HashSet으로 아이템 수집/사용 상태 분리
**파일:** `PokemonGameController.cs:17`

```csharp
private readonly HashSet<MapItemType> collectedItems = new HashSet<MapItemType>();
private readonly HashSet<MapItemType> usedItems      = new HashSet<MapItemType>();
```

- 수집과 사용을 별도 Set으로 관리해 중복 사용 방지
- `HasCollected()` / `HasUsed()` / `UseItem()` 메서드로 외부에 명확한 API 제공

---

## 1942 슈팅 미니게임 (`Scripts/MiniGame_1942/`)

### 1. BossController — `event Action OnBossDefeated` 이벤트 패턴
**파일:** `BossController.cs:38`

```csharp
public event Action OnBossDefeated;
// ...
boss.OnBossDefeated += HandleBossDefeated;
```

- 보스가 `WaveManager`를 직접 참조하지 않고 이벤트로 결과만 알림
- 단방향 의존성 유지 — 보스는 자신이 누구에게 알리는지 모름

---

### 2. 패턴 코루틴 List 관리
**파일:** `BossController.cs:64`

```csharp
private readonly List<Coroutine> patternCoroutines = new();

void StartAllPatterns()
{
    patternCoroutines.Add(StartCoroutine(Pattern1Routine()));
    patternCoroutines.Add(StartCoroutine(Pattern2Routine()));
}

void StopAllPatterns()
{
    foreach (var c in patternCoroutines)
        if (c != null) StopCoroutine(c);
    patternCoroutines.Clear();
}
```

- 패턴이 추가돼도 `StartAllPatterns()` / `StopAllPatterns()` 하나로 일괄 제어
- 2페이즈 전환 시 전체 중단 → 파라미터 교체 → 재시작 흐름이 명확

---

### 3. WaveManager — `autoStart` + `BeginGame()` 분리
**파일:** `WaveManager.cs:51`

```csharp
public bool autoStart = true;

public void BeginGame()
{
    if (waveCoroutine != null) return;
    waveCoroutine = StartCoroutine(RunWaves());
}
```

- 컷씬이 있을 때 `autoStart = false` → 컷씬 끝나면 `BeginGame()` 호출
- Inspector 체크 하나로 테스트/실제 플레이 전환 가능

---

### 4. OnGUI 디버그가 `Debug.isDebugBuild`로만 활성화
**파일:** `ShootingGameController.cs:141`

```csharp
void OnGUI()
{
    if (!Debug.isDebugBuild) return;
    // 보스 스킵, 무적모드, 필살기 충전 등
}
```

- 릴리즈 빌드에서 자동으로 꺼짐
- 무적모드, 보스 스킵, 2페이즈 강제 진입, 필살기 충전까지 테스트 도구가 잘 갖춰짐

---

### 5. 게임오버 이어하기 — `Time.timeScale=0` + `WaitForSecondsRealtime`
**파일:** `ShootingGameController.cs:31`

```csharp
Time.timeScale = 0f;
// ...
yield return new WaitForSecondsRealtime(0.6f);
// ...
var anim = coinSpinObj.GetComponent<Animator>();
if (anim != null) anim.updateMode = AnimatorUpdateMode.UnscaledTime;
```

- 게임오버 시 `timeScale=0`으로 게임 정지
- 이어하기 연출은 `WaitForSecondsRealtime`으로 실시간 대기
- 코인 애니메이션도 `UnscaledTime`으로 처리해 일관성 유지

---

## 스폰지밥 × 역전재판 (`Scripts/MiniGame_Sponge/`)

### 1. `OnStateChanged` 이벤트로 완전한 상태 머신 구현
**파일:** `SpongeGameManager.cs:39`

```csharp
public static event System.Action<GameState> OnStateChanged;

public void ChangeState(GameState newState)
{
    if (currentState == newState) return;
    currentState = newState;
    OnStateChanged?.Invoke(newState);
}
```

- 모든 상태 전환이 반드시 `ChangeState()`를 통해서만 이루어짐
- `SpongeGameController`는 이벤트만 구독하면 되므로 GameManager를 직접 참조할 필요 없음
- 같은 상태로의 중복 전환 방지 포함

---

### 2. `ConsumeConditionMet()` — 소비형 플래그 패턴
**파일:** `SpongeGameManager.cs:192`

```csharp
public bool ConsumeConditionMet()
{
    bool result = conditionJustMet;
    conditionJustMet = false;
    return result;
}
```

- "방금 조건이 충족됐어?" 를 한 번만 물어볼 수 있게 읽는 순간 자동 초기화
- 중복 트리거 방지를 bool 리셋 하나로 해결

---

### 3. `SpongeEvidenceDatabaseSO` — ScriptableObject로 증거 데이터 분리
**파일:** `SpongeEvidenceDatabaseSO.cs`

```csharp
[CreateAssetMenu(fileName = "SpongeEvidenceDatabaseSO", menuName = "Scriptable Objects/SpongeEvidenceDatabaseSO")]
public class SpongeEvidenceDatabaseSO : ScriptableObject
{
    public SpongeEvidenceData[] evidences;
    public SpongeEvidenceData GetById(string id) =>
        System.Array.Find(evidences, e => e.id == id);
}
```

- 증거 목록을 에셋으로 관리해 코드 수정 없이 Inspector에서 추가/수정 가능
- `GetById()`로 ID 기반 검색 API 제공

---

### 4. `SpongeTrialScriptData` — 재판 흐름을 데이터 구조에 그대로 반영
**파일:** `SpongeTrialScriptData.cs`

```csharp
public class SpongeTrialScriptData
{
    public SpongeDialogueLine[]  openingLines;
    public SpongeTestimonyLine[] testimonyLines;
    public SpongeDialogueLine[]  beforeRetestimonyLines;
    public SpongeTestimonyLine[] retestimonyLines;
    public SpongeDialogueLine[]  pressDialogueLines;
    public SpongeDialogueLine[]  evidenceDialogueLines;
    public SpongeDialogueLine[]  endingLines;
}
```

- opening → testimony → retestimony → ending 재판 흐름이 클래스 필드 순서에 그대로 반영
- 대사/증언 타입을 별도 클래스로 분리해 직렬화 가능

---

## 테트리스 × 케로로 (`Scripts/MiniGame_Tetris/`)

### 1. `TamamaImpactData` 구조체로 연출 데이터 패키징
**파일:** `TetrisCutsceneManager.cs:12`

```csharp
public struct TamamaImpactData
{
    public bool isHorizontal;
    public Vector3 spawnPos;
    public Quaternion rot;
    public float distance;
    public int hitCount;
    public Vector3 impactPoint;
}
```

- 연출에 필요한 6개 파라미터를 구조체로 묶어 메서드 시그니처 단순화
- 호출부에서 데이터를 조립하고 연출 매니저에게 통째로 위임하는 명확한 역할 분리

---

### 2. `isCutscenePlaying` 플래그로 입력 차단 일원화
**파일:** `TetrisGameController.cs:12`

- 컷씬/임팩트 연출 중 모든 입력 자동 차단
- 오프닝/임팩트/엔딩 세 군데 콜백에서 `false`로 복구하는 패턴이 일관됨
- 연출 도중 새 테트로미노 스폰 방지도 동일 플래그 하나로 처리

---

### 3. `TetrisCutsceneManager` — DialogueDB 없을 때 동적 생성 방어 코드
**파일:** `TetrisCutsceneManager.cs:176`

```csharp
if (DialogueDB.Instance == null)
{
    GameObject dbObj = new GameObject("DialogueDB");
    dbObj.AddComponent<DialogueDB>();
}
```

- 씬 단독 테스트 시 Hub 없어도 에러 없이 실행 가능
- 오프닝/엔딩 컷씬 모두 동일하게 적용
