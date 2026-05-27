using UnityEngine;
using System.Collections.Generic;

public class DialogueDB : MonoBehaviour
{
    public static DialogueDB Instance;
    private Dictionary<string, string> dialogueDict = new Dictionary<string, string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Load(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/" + fileName);
        if (jsonFile == null)
        {
            Debug.LogError("파일을 찾을 수 없습니다: " + fileName);
            return;
        }

        // --- JSON 데이터 파싱 (파일 구조 변경 없이 읽기) ---
        string json = jsonFile.text;
        // { } 제거하고 콤마로 분리
        string cleanJson = json.Replace("{", "").Replace("}", "").Replace("\"", "");
        string[] pairs = cleanJson.Split(',');

        dialogueDict.Clear();
        foreach (string pair in pairs)
        {
            string[] kv = pair.Split(':');
            if (kv.Length >= 2)
            {
                dialogueDict[kv[0].Trim()] = kv[1].Trim();
            }
        }
        Debug.Log(fileName + " 로드 완료! 대사 개수: " + dialogueDict.Count);
    }

    public string Get(string key)
    {
        if (dialogueDict.ContainsKey(key)) return dialogueDict[key];
        Debug.LogWarning("키를 찾을 수 없음: " + key);
        return "[" + key + "]";
    }
}