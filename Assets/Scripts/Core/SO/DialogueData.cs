using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("대사 목록")]
    [TextArea(3, 5)]
    public string[] sentences;
}
