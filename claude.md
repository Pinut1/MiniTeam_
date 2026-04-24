# 유니티 팀프로젝트 CLAUDE.md

## 프로젝트 개요
- 장르: 미니게임 허브 (Hub에서 각 미니게임 진입)
- 엔진: Unity (C#)
- 씬 전환 방식: Additive Scene Loading

---

## 팀원 & 담당

| 이름 | 역할 | 담당 미니게임 | 담당 씬/폴더 |
|---|---|---|---|
| 김기욱 | 메인 PD / 프레젠터 / **개발** | 테트리스 × 케로로 | MiniGame_Tetris.unity / Scripts/MiniGame_Tetris |
| 김예지 | 시나리오 디렉터 | 눈빛 보내기 × 슈가슈가룬 | MiniGame_Melon.unity / Scripts/MiniGame_Melon |
| 김영욱 | 개발 디렉터 | 1942 × 파워퍼프걸 | Hub.unity / MiniGame_1942.unity / Scripts/Core / Scripts/MiniGame_1942 |
| 장한나 | 총괄 시나리오 / 서기 | 스폰지밥 × 역전재판 | MiniGame_Sponge.unity / Scripts/MiniGame_Sponge |
| 차정민 | 아트A 디렉터 | 디지몬 × 포켓몬 (공동) | Art/Sprites |
| 황해인 | 아트B 디렉터 | 디지몬 × 포켓몬 (공동) | Art/UI |

### 프로그래머
- **김영욱** — 개발 디렉터, Core 시스템 전담
- **김기욱** — PD 겸 개발, 테트리스 미니게임 전담

---

## 절대 규칙
- `Hub.unity` → 김영욱만 수정 가능, 수정 전 팀 공지 필수
- `MiniGameManager.cs` → 김영욱만 관리
- `Scripts/Core/` → 김영욱만 수정
- 각자 담당 씬/폴더 외 수정 시 팀 채팅 공지 필수
- 혼자 머지 금지, 최소 1명 리뷰 후 머지

---

## 폴더 구조

```
Assets/
├── _Scenes/
│   ├── Hub.unity                  ← 김영욱 전담
│   ├── MiniGame_1942.unity        ← 김영욱
│   ├── MiniGame_Melon.unity       ← 김예지
│   ├── MiniGame_Pokemon.unity     ← 차정민 + 황해인
│   ├── MiniGame_Sponge.unity      ← 장한나
│   └── MiniGame_Tetris.unity      ← 김기욱
├── Scripts/
│   ├── Core/                      ← 김영욱 전담 (IMiniGame, MiniGameManager)
│   ├── MiniGame_1942/             ← 김영욱
│   ├── MiniGame_Melon/            ← 김예지
│   ├── MiniGame_Pokemon/          ← 차정민 + 황해인
│   ├── MiniGame_Sponge/           ← 장한나
│   └── MiniGame_Tetris/           ← 김기욱
├── Art/
│   ├── Sprites/                   ← 차정민 관리
│   └── UI/                        ← 황해인 관리
└── Audio/
```

---

## 핵심 인터페이스

```csharp
// Scripts/Core/IMiniGame.cs
// 모든 미니게임이 반드시 상속받아 구현해야 함
public interface IMiniGame {
    void OnGameClear();
    void OnGameFail();
}
```

```csharp
// Scripts/Core/MiniGameManager.cs
// Hub <-> 미니게임 씬 전환 전담, 김영욱만 수정
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

## 브랜치 전략

```
main           ← 최종 배포, 직접 푸시 금지 (김기욱 승인 필요)
└── develop    ← 팀 통합 브랜치
    ├── feature/hub-system       ← 김영욱
    ├── feature/1942-shooting    ← 김영욱
    ├── feature/melon-game       ← 김예지
    ├── feature/pokemon-game     ← 차정민 + 황해인
    ├── feature/sponge-game      ← 장한나
    └── feature/tetris-game      ← 김기욱
```

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

## 에셋 업로드 규칙

- 캐릭터/오브젝트 스프라이트 → `Assets/Art/Sprites/`
- UI 에셋 → `Assets/Art/UI/`
- 파일명 규칙: `chr_agumon_idle_01.png` (타입_이름_상태_번호)
- 용량 큰 파일(.psd, .wav 등)은 Git LFS로 관리