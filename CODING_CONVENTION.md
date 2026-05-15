# MiniTeam 코딩 컨벤션

## 네임스페이스
- 모든 스크립트는 반드시 네임스페이스 사용
- Core 시스템: `MiniTeam.Core`
- 미니게임별: `MiniTeam.{GameName}` (예: `MiniTeam.Shooting1942`)

## 네이밍
| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스 | PascalCase | `EnemySpawner` |
| 메서드 | PascalCase | `TakeHit()` |
| public 변수 | camelCase | `moveSpeed` |
| private 변수 | camelCase | `currentHp` |
| 상수 | UPPER_SNAKE | `MAX_HP` |

## 파일 구조
```
Assets/
├── Scripts/MiniGame_{이름}/   ← 본인 스크립트만
├── Prefabs/MiniGame_{이름}/   ← 본인 프리팹만
├── Art/Sprites/MiniGame_{이름}/
└── Art/Materials/MiniGame_{이름}/
```

## 사용 금지 (레거시 API)

### Object.Find 계열
```csharp
// ❌ Deprecated
FindObjectOfType<T>()
FindObjectsOfType<T>()

// ✅ 사용할 것
FindAnyObjectByType<T>()   // 아무 인스턴스나 빠르게 찾을 때
FindFirstObjectByType<T>() // 첫 번째 인스턴스가 필요할 때
FindObjectsByType<T>(FindObjectsSortMode.None) // 전체 목록이 필요할 때
```

### UI 텍스트
```csharp
// ❌ Legacy
using UnityEngine.UI;
Text myText;

// ✅ 사용할 것
using TMPro;
TextMeshProUGUI myText;
```

### 기타
```csharp
// ❌ Deprecated
rb.velocity = ...;

// ✅ 사용할 것
rb.linearVelocity = ...;
```

## Git 브랜치 규칙
- 작업 브랜치: `feature/{본인게임}` 에서만 작업
- 머지 순서: `feature → develop` (팀원 확인 후) → `main`
- **절대 feature에서 main으로 직접 머지 금지**

## 기타
- `Debug.Log`는 개발 중에만 사용, 배포 전 정리
- Inspector 연결이 필요한 public 변수는 `[Header("설명")]` 필수
- 씬 이름은 `MiniGame_{이름}` 형식 통일 (예: `MiniGame_1942`)
