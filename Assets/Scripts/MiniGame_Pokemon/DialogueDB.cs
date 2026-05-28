using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    // 미니게임별 JSON 파일에서 대사를 로드하는 공용 DB
    // Resources/Dialogues/{fileName}.json 에서 플랫 key-value 형식으로 관리
    public class DialogueDB : MonoBehaviour
    {
        public static DialogueDB Instance { get; private set; }

        private Dictionary<string, string> table = new Dictionary<string, string>();

        void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); // gameObject 전체가 아닌 중복 컴포넌트만 삭제
                return; 
            }
            
            // 만약 자신(Transform + DialogueDB) 외에 다른 컴포넌트가 같이 붙어 있다면
            if (GetComponents<Component>().Length > 2) 
            {
                // 전용 빈 오브젝트를 생성하여 완전히 격리
                GameObject dbHolder = new GameObject("DialogueDB_Global");
                dbHolder.AddComponent<DialogueDB>(); 
                Destroy(this); // 현재의 나는 삭제
                return; // 새로 생성된 DB가 알아서 Instance를 설정하고 DontDestroyOnLoad 됨
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        // 씬 진입 시 해당 미니게임 JSON 로드
        // 예) Load("Pokemon") → Resources/Dialogues/Pokemon.json
        public void Load(string fileName)
        {
            var asset = Resources.Load<TextAsset>($"Dialogues/{fileName}");
            if (asset == null)
            {
                Debug.LogWarning($"[DialogueDB] 파일을 찾을 수 없음: Dialogues/{fileName}");
                return;
            }
            table = ParseFlatJson(asset.text);
            Debug.Log($"[DialogueDB] '{fileName}' 로드 완료 — {table.Count}개 항목");
        }

        // 키로 대사 반환. 키가 없으면 [key] 형태로 경고 표시
        public string Get(string key)
        {
            if (table.TryGetValue(key, out var val)) return val;
            Debug.LogWarning($"[DialogueDB] 키 없음: '{key}'");
            return $"[{key}]";
        }

        // 외부 패키지 없이 플랫 JSON {"key":"value"} 파싱
        static Dictionary<string, string> ParseFlatJson(string json)
        {
            var dict = new Dictionary<string, string>();
            var pattern = new Regex("\"([^\"]+)\"\\s*:\\s*\"((?:[^\\\\\"]|\\\\.)*)\"");
            foreach (Match m in pattern.Matches(json))
                dict[m.Groups[1].Value] = Unescape(m.Groups[2].Value);
            return dict;
        }

        static string Unescape(string s) =>
            s.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\\\", "\\");
    }
}
