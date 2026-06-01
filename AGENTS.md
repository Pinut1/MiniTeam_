# MiniTeam_ — AI Agent 공통 가이드

> 이 파일은 Claude, Gemini, Codex 등 여러 AI CLI가 공통으로 참조하는 프로젝트 컨텍스트 문서입니다.

---

## 프로젝트 개요

- **장르**: 미니게임 허브 (Hub 씬에서 각 미니게임 진입)
- **엔진**: Unity 6 (C#), Unity 버전: `6000.3.8f1`
- **씬 전환**: Additive Scene Loading
- **네임스페이스 루트**: `MiniTeam`

---

## 팀원 & 담당

| 이름 | 역할 | 담당 미니게임 | 담당 씬/폴더 |
|---|---|---|---|
| 김기욱 | 메인 PD / 프레젠터 / 개발 | 테트리스 × 케로로 | `MiniGame_Tetris.unity` / `Scripts/MiniGame_Tetris` |
| 김예지 | 시나리오 디렉터 | 눈빛 보내기 × 슈가슈가룬 | `MiniGame_Melon.unity` / `Scripts/MiniGame_EyeContact` |
| 김영욱 | 개발 디렉터 | 1942 × 파워퍼프걸 | `Hub.unity` / `MiniGame_1942.unity` / `Scripts/Core` / `Scripts/MiniGame_1942` |
| 장한나 | 총괄 시나리오 / 서기 | 스폰지밥 × 역전재판 | `MiniGame_Sponge.unity` / `Scripts/MiniGame_Sponge` |
| 차정민 | 아트A 디렉터 | 디지몬 × 포켓몬 (공동) | `Art/Sprites` |
| 황해인 | 아트B 디렉터 | 디지몬 × 포켓몬 (공동) | `Art/UI` |

---

## 절대 규칙 (AI가 반드시 지켜야 할 제약)

- `Hub.unity` → **김영욱만 수정 가능**, AI가 임의로 수정하지 말 것
- `Scripts/Core/` → **김영욱만 수정**, AI가 Core 파일 변경 시 반드시 명시적으로 사용자에게 확인
- `MiniGameManager.cs` → 김영욱만 관리
- 타 팀원 담당 씬/폴더는 수정하지 말 것 (작업자에게 확인 필요)
- 혼자 머지 금지 — PR 생성만 하고 머지는 사람이 직접 수행

---

## 폴더 구조

```
Assets/
├── _Scenes/
│   ├── Hub.unity                     ← 김영욱 전담
│   ├── MiniGame_1942.unity           ← 김영욱
│   ├── MiniGame_Melon.unity          ← 김예지
│   ├── MiniGame_Pokemon.unity        ← 차정민 + 황해인
│   ├── MiniGame_Sponge.unity         ← 장한나
│   └── MiniGame_Tetris.unity         ← 김기욱
├── Scripts/
│   ├── Core/                         ← 김영욱 전담 (IMiniGame, MiniGameManager)
│   ├── MiniGame_1942/                ← 김영욱
│   ├── MiniGame_EyeContact/          ← 김예지 (구: MiniGame_Melon)
│   ├── MiniGame_Pokemon/             ← 차정민 + 황해인
│   ├── MiniGame_Sponge/              ← 장한나
│   └── MiniGame_Tetris/              ← 김기욱
├── Art/
│   ├── Sprites/                      ← 차정민 관리
│   └── UI/                           ← 황해인 관리
└── Audio/
```

---

## 핵심 인터페이스

```csharp
// Scripts/Core/IMiniGame.cs — 모든 미니게임이 반드시 구현
public interface IMiniGame {
    void OnGameClear();
    void OnGameFail();
}
```

```csharp
// Scripts/Core/MiniGameManager.cs — Hub <-> 미니게임 씬 전환 전담
public class MiniGameManager : MonoBehaviour {
    public static MiniGameManager Instance { get; private set; }
    private string currentScene;

    void Awake() => Instance = this;

    public void EnterMiniGame(string sceneName) {
        currentScene = sceneName;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void ExitMiniGame() {
        SceneManager.UnloadSceneAsync(currentScene);
    }
}
```

---

## 코딩 컨벤션

### 네임스페이스
- Core: `MiniTeam.Core`
- 미니게임별: `MiniTeam.{GameName}` (예: `MiniTeam.Shooting1942`)

### 네이밍
| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스 | PascalCase | `EnemySpawner` |
| 메서드 | PascalCase | `TakeHit()` |
| public 변수 | camelCase | `moveSpeed` |
| private 변수 | camelCase | `currentHp` |
| 상수 | UPPER_SNAKE | `MAX_HP` |

### 사용 금지 (레거시 API)

```csharp
// Object.Find 계열
FindObjectOfType<T>()      // ❌
FindObjectsOfType<T>()     // ❌

FindAnyObjectByType<T>()                           // ✅
FindFirstObjectByType<T>()                         // ✅
FindObjectsByType<T>(FindObjectsSortMode.None)     // ✅

// UI 텍스트
Text myText;              // ❌  (using UnityEngine.UI)
TextMeshProUGUI myText;   // ✅  (using TMPro)

// Rigidbody velocity
rb.velocity = ...;        // ❌
rb.linearVelocity = ...;  // ✅
```

### 기타
- `Debug.Log`는 배포 전 반드시 정리
- Inspector 연결 public 변수는 `[Header("설명")]` 필수

---

## 브랜치 전략

```
main           ← 최종 배포, 직접 푸시 금지 (김기욱 승인 필요)
└── develop    ← 팀 통합 브랜치
    ├── feature/hub-system
    ├── feature/1942-shooting
    ├── feature/melon-game
    ├── feature/pokemon-game
    ├── feature/sponge-game
    └── feature/tetris-game
```

- feature → develop 머지: 팀원 1명 리뷰 필수
- feature에서 main으로 직접 머지 금지

---

## 커밋 메시지 컨벤션

```
feat:     새 기능 추가       예) feat: 테트리스 블록 낙하 로직 구현
fix:      버그 수정          예) fix: 씬 전환 시 오브젝트 미삭제 수정
art:      에셋 추가/수정     예) art: 디지몬 캐릭터 스프라이트 추가
docs:     문서/주석 수정     예) docs: MiniGameManager 주석 정리
refactor: 코드 구조 개선     예) refactor: BossPattern 구조 개선
chore:    설정 파일 수정     예) chore: .gitignore Unity 항목 추가
```

---

## 에셋 규칙

- 스프라이트 → `Assets/Art/Sprites/MiniGame_{이름}/`
- UI 에셋 → `Assets/Art/UI/MiniGame_{이름}/`
- 파일명: `chr_agumon_idle_01.png` (타입_이름_상태_번호)
- 용량 큰 파일 (`.psd`, `.wav`, `.anim` 등) → **Git LFS** 관리

---

## AI CLI별 설정 파일

| AI 도구 | 설정 파일 |
|---------|----------|
| Claude Code | `CLAUDE.md` |
| Gemini CLI | `GEMINI.md` |
| OpenAI Codex | `AGENTS.md` (이 파일) |
| 공통 참조 | **이 파일 (`AGENTS.md`)** |
