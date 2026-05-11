using UnityEngine;

/// <summary>
/// 증거 목록 에셋
/// </summary>
[CreateAssetMenu(fileName = "SpongeEvidenceDatabaseSO", menuName = "Scriptable Objects/SpongeEvidenceDatabaseSO")]
public class SpongeEvidenceDatabaseSO : ScriptableObject
{
    public SpongeEvidenceData[] evidences;

    // 코드에서 ID로 빠르게 찾을 때 사용
    public SpongeEvidenceData GetById(string id)
    {
        return System.Array.Find(evidences, e => e.id == id);
    }
}
