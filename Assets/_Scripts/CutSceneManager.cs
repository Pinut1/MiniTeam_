using UnityEngine;

public class CutsceneNpcManager : MonoBehaviour
{
    [Header("스폰할 NPC 프리팹")]
    public GameObject pierrePrefab;
    public GameObject vanillaPrefab;

    [Header("스폰 좌표 (직접 입력)")]
    public Vector3 pierreSpawnPosition;
    public Vector3 vanillaSpawnPosition;

    // PlayerLaser 스크립트에서 호출할 함수
    public void SpawnCutsceneNpcs()
    {
        if (pierrePrefab != null)
        {
            Instantiate(pierrePrefab, pierreSpawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("피에르 프리팹이 설정되지 않았습니다.");
        }

        if (vanillaPrefab != null)
        {
            Instantiate(vanillaPrefab, vanillaSpawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("바닐라 프리팹이 설정되지 않았습니다.");
        }
    }
}