# 대사 시스템 (DialogueDB) 사용 가이드

> 작성일: 2026-05-15  
> 담당: 차정민 / 황해인 (포켓몬 파트)  
> 공통 사용 가능 — 모든 미니게임 적용 가능

---

## 개요

미니게임별 대사를 JSON 파일로 관리하는 시스템입니다.  
코드 수정 없이 JSON 파일만 편집해서 대사를 추가/변경할 수 있습니다.

```
Resources/Dialogues/Pokemon.json   ← 포켓몬 미니게임 대사
Resources/Dialogues/Tetris.json    ← 테트리스 미니게임 대사 (예시)
Resources/Dialogues/Sponge.json    ← 스폰지밥 미니게임 대사 (예시)
```

---

## 1. JSON 파일 작성법

### 형식

```json
{
  "키_이름": "대사 내용",
  "키_이름2": "대사 내용2"
}
```

- 키는 영문 소문자 + 언더바 사용 (`menu_pokemon`, `item_pokemonball`)
- 값은 한글/영문 모두 가능
- 줄바꿈: `\n` 사용

### 키 네이밍 규칙

| 접두사 | 용도 | 예시 |
|---|---|---|
| `menu_` | START 메뉴 항목 대사 | `menu_pokemon`, `menu_save` |
| `item_` | 아이템 획득 대사 | `item_pokemonball`, `item_digivice` |
| `npc_` | NPC 대사 | `npc_kuchipach_appear` |
| `escape_` | 탈출 관련 대사 | `escape_no_item` |
| `battle_` | 배틀 관련 대사 | `battle_start`, `battle_win` |

### Pokemon.json 예시 (전체)

```json
{
  "menu_pokemon":   "포켓몬을 가지고 있지 않아!",
  "menu_bag_empty": "가방이 비어있어.",
  "menu_bag_has":   "소중한 물건이 들어있어!",
  "menu_save":      "지금은 저장할 수 없어!",

  "item_pokemonball":  "[ 포켓몬볼을 획득했다! ]",
  "item_strangecandy": "[ 이상한사탕을 획득했다! ]",
  "item_digivice":     "[ 디지바이스를 획득했다! ]",

  "npc_kuchipach_appear": "쿠치파치가 나타났다!",
  "npc_kuchipach_deny":   "쿠치파치: 오? 뭐야? 포켓몬 찾아? 난 포켓몬이 아니야~!",
  "npc_kuchipach_flee":   "쿠치파치가 도망쳤다!",
  "npc_kuchipach_give":   "쿠치파치: 잠깐! 이건 너한테 줄게. 이걸 가져가렴~!",

  "escape_no_item": "오브제 없이는 탈출할 수 없다!"
}
```

---

## 2. 유니티 씬 세팅

### 필수 컴포넌트 (씬당 1회)

씬에 빈 오브젝트를 만들고 아래 컴포넌트를 붙여주세요.

| 컴포넌트 | 역할 |
|---|---|
| `DialogueDB` | JSON 로드 및 대사 조회 |
| `MapDialogueUI` | 화면 하단 대화창 표시 |

`MapDialogueUI` 인스펙터 연결:

| 필드 | 연결 대상 |
|---|---|
| `Panel` | 대화창 루트 오브젝트 |
| `Dialogue Text` | TextMeshProUGUI 텍스트 |
| `Confirm Indicator` | ▼ 아이콘 오브젝트 (없어도 됨) |

### 씬 시작 시 JSON 로드

씬 시작 스크립트(GameController 등)의 `Start()`에 추가:

```csharp
void Start()
{
    DialogueDB.Instance.Load("Pokemon"); // 파일명만 입력 (확장자 제외)
}
```

---

## 3. 코드에서 대사 출력하기

### 기본 사용법

```csharp
// 대사 한 줄 가져오기
string text = DialogueDB.Instance.Get("menu_pokemon");
// → "포켓몬을 가지고 있지 않아!"
```

### 대화창에 출력하기 (코루틴)

```csharp
// Z / Space / Enter 로 닫히는 대화창 표시
IEnumerator ShowDialogue(string key)
{
    string text = DialogueDB.Instance.Get(key);
    yield return StartCoroutine(MapDialogueUI.Instance.Show(text));
    // 여기서부터 플레이어가 키를 누른 이후 실행됨
}
```

### 조건에 따라 다른 대사 출력

```csharp
// 가방이 비었는지에 따라 다른 키 사용
string key = PokemonGameController.Instance.HasItem ? "menu_bag_has" : "menu_bag_empty";
string text = DialogueDB.Instance.Get(key);
```

### 연속 대사 출력

```csharp
IEnumerator ShowSequence()
{
    yield return StartCoroutine(MapDialogueUI.Instance.Show(DialogueDB.Instance.Get("npc_kuchipach_appear")));
    yield return StartCoroutine(MapDialogueUI.Instance.Show(DialogueDB.Instance.Get("npc_kuchipach_deny")));
    yield return StartCoroutine(MapDialogueUI.Instance.Show(DialogueDB.Instance.Get("npc_kuchipach_flee")));
}
```

---

## 4. 공통 코드 레퍼런스

### DialogueDB.cs

```csharp
// 위치: Assets/Scripts/MiniGame_Pokemon/DialogueDB.cs

DialogueDB.Instance.Load("Pokemon");     // JSON 파일 로드
DialogueDB.Instance.Get("key");          // 대사 가져오기
```

### MapDialogueUI.cs

```csharp
// 위치: Assets/Scripts/MiniGame_Pokemon/MapDialogueUI.cs

MapDialogueUI.Instance.IsShowing         // 대화창 표시 중 여부 (bool)
MapDialogueUI.Instance.Show("텍스트")    // 대화창 표시 (코루틴)
```

### StartMenuUI.cs — 버튼 OnClick 연결 목록

| 버튼 | 메서드 |
|---|---|
| 포켓몬 | `StartMenuUI.OnPokemonButton()` |
| 가방 | `StartMenuUI.OnBagButton()` |
| 저장 | `StartMenuUI.OnSaveButton()` |
| 닫기 | `StartMenuUI.OnCloseButton()` |

---

## 5. 새 미니게임에 적용하는 법

1. `Resources/Dialogues/` 에 `{미니게임이름}.json` 파일 생성
2. 씬 시작 시 `DialogueDB.Instance.Load("{미니게임이름}")` 호출
3. `DialogueDB.Instance.Get("키")` 로 대사 사용

> `DialogueDB`는 `DontDestroyOnLoad` 싱글톤이므로 씬이 바뀌어도 유지됩니다.  
> 단, 미니게임이 바뀔 때 `Load()`를 다시 호출하면 해당 게임 JSON으로 교체됩니다.

---

## 주의사항

- JSON 키는 **정확히** 코드와 일치해야 합니다 (대소문자 구분)
- 키가 없으면 `[key_name]` 형태로 화면에 출력되며 콘솔에 경고가 뜹니다
- JSON 파일은 **UTF-8** 인코딩으로 저장해야 한글이 깨지지 않습니다
