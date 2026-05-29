# 미니게임별 AudioManager 작성 가이드

## 개요

각 미니게임은 자신만의 `AudioManager`를 **네임스페이스로 격리**해서 만든다.  
클래스 이름은 모두 `AudioManager`로 통일하되, `namespace`를 다르게 써서 충돌을 방지한다.  
실제 오디오 재생은 `Core.SoundManager`에 위임하므로 AudioManager는 **클립 보관 + 호출 창구** 역할만 한다.

---

## 네임스페이스 규칙

| 미니게임 | 네임스페이스 | 스크립트 위치 |
|---|---|---|
| 1942 (완료) | `MiniTeam.Shooting1942` | `Scripts/MiniGame_1942/AudioManager.cs` |
| 테트리스 | `MiniTeam.Tetris` | `Scripts/MiniGame_Tetris/AudioManager.cs` |
| 포켓몬 | `MiniTeam.Pokemon` | `Scripts/MiniGame_Pokemon/AudioManager.cs` |
| 눈빛보내기 | `MiniTeam.EyeContact` | `Scripts/MiniGame_EyeContact/AudioManager.cs` |
| 스폰지밥 | `MiniTeam.Sponge` | `Scripts/MiniGame_Sponge/AudioManager.cs` |

---

## 복사해서 쓰는 템플릿

아래 코드를 자신의 폴더에 `AudioManager.cs`로 만들고,  
`YOUR_NAMESPACE`와 클립 변수만 교체하면 된다.

```csharp
using MiniTeam.Core;
using UnityEngine;

namespace YOUR_NAMESPACE          // ← 위 표에서 자기 네임스페이스로 교체
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip bgmMain;         // ← 필요한 클립으로 교체/추가

        [Header("효과음")]
        public AudioClip sfxExample;      // ← 필요한 클립으로 교체/추가

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // 아래 메서드는 그대로 복사 (수정 불필요)
        public void PlayBGM(AudioClip clip)                    => SoundManager.Instance?.PlayBGM(clip);
        public void StopBGM()                                  => SoundManager.Instance?.StopBGM();
        public void PlaySFX(AudioClip clip)                    => SoundManager.Instance?.PlaySFX(clip);
        public void PlaySFX(AudioClip clip, float volumeScale) => SoundManager.Instance?.PlaySFX(clip, volumeScale);
    }
}
```

---

## 다른 스크립트에서 호출하는 방법

AudioManager는 `Instance` 싱글톤으로 직접 참조한다.  
**별도 using 없이** 같은 namespace 안에 있으면 바로 사용 가능하다.

```csharp
// 예시: 같은 namespace 안의 다른 스크립트
namespace MiniTeam.Tetris
{
    public class TetrisPlayer : MonoBehaviour
    {
        void OnLineClear()
        {
            // 클립 접근 + 재생을 한 줄로
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxExample);
        }

        void OnGameStart()
        {
            AudioManager.Instance?.PlayBGM(AudioManager.Instance.bgmMain);
        }

        void OnGameEnd()
        {
            AudioManager.Instance?.StopBGM();
        }
    }
}
```

---

## 씬에 배치하는 방법

1. 자신의 씬에 **빈 GameObject** 생성 → 이름: `AudioManager`
2. 해당 오브젝트에 자신의 `AudioManager.cs` 컴포넌트 추가
3. Inspector에서 각 클립 슬롯에 오디오 파일 연결
4. 오디오 파일은 `Assets/Audio/` 하위에 자기 미니게임 폴더를 만들어 관리

```
Assets/Audio/
├── 1942/
├── Tetris/       ← 여기에 .wav / .mp3 넣기
├── Pokemon/
├── EyeContact/
└── Sponge/
```

---

## 주의사항

- `AudioManager`는 **DontDestroyOnLoad 하지 않는다** — 씬이 언로드되면 같이 사라지는 게 정상
- BGM/SFX 볼륨 조절은 `Core.SoundManager`에서 처리하므로 AudioManager에서 건드리지 않는다
- `SoundManager`는 Hub 씬에서 항상 살아있으므로 `SoundManager.Instance`는 null이 아님
