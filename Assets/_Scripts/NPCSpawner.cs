using UnityEngine;

public class NpcSpawner : MonoBehaviour
{
    [Header("NPC 종류 설정 (프리팹)")]
    public GameObject[] npcPrefabs;

    [Header("스폰 수량 설정")]
    public int minNpcCount = 3;
    public int maxNpcCount = 7;

    [Header("스폰 위치 설정 (X축 - 좌우 거리)")]
    public float minX = -10f;
    public float maxX = 10f;

    [Header("스폰 위치 설정 (Y축 - 상하 높이)")]
    [Tooltip("플레이어의 가로선(발바닥) Y좌표를 입력하세요.")]
    public float minY = -1f;
    [Tooltip("NPC가 올라갈 수 있는 가장 위쪽 Y좌표를 입력하세요.")]
    public float maxY = 2f;

    void Start()
    {
        SpawnRandomNPCs();
    }

    void SpawnRandomNPCs()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogWarning("스폰할 NPC 프리팹이 등록되지 않았습니다!");
            return;
        }

        int spawnCount = Random.Range(minNpcCount, maxNpcCount + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0, npcPrefabs.Length);
            GameObject selectedPrefab = npcPrefabs[randomIndex];

            // X와 Y 모두 지정해둔 최소/최대 범위 안에서 랜덤으로 뽑습니다.
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);

            // 최종 스폰 위치 지정
            Vector3 spawnPosition = new Vector3(randomX, randomY, transform.position.z);

            // 1. [추가된 부분] Instantiate의 4번째 값으로 'transform'을 넣어주면,
            // 생성된 NPC들이 이 NPC_Spawner 오브젝트의 자식으로 깔끔하게 들어갑니다.
            GameObject spawnedNpc = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, transform);

            // 2. [추가된 부분] 50% 확률로 NPC가 왼쪽 또는 오른쪽을 바라보게 만듭니다.
            bool faceRight = Random.value > 0.5f;
            if (!faceRight)
            {
                // 스케일의 X값을 -1로 곱해서 좌우 반전시킵니다.
                Vector3 scale = spawnedNpc.transform.localScale;
                scale.x *= -1;
                spawnedNpc.transform.localScale = scale;
            }
        }
    }
}