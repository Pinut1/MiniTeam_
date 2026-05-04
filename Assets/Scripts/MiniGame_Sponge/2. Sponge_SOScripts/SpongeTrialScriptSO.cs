using UnityEngine;

/// <summary>
/// 대사 + 증언 통합 에셋
/// </summary>
[CreateAssetMenu(fileName = "SpongeTrialScriptSO", menuName = "Scriptable Objects/SpongeTrialScriptSO")]
public class SpongeTrialScriptSO : ScriptableObject
{
    [Header("오프닝 대사")]
    public SpongeDialogueLine[] openingLines;

    [Header("증언 목록(심문 대상)")]
    public SpongeTestimonyLine[] testimonyLines;

    [Header("엔딩 대사")]
    public SpongeDialogueLine[] endingLines;
}
