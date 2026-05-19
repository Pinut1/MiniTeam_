using UnityEngine;
using System.Collections;

public class NpcSpawner : MonoBehaviour
{
    [Header("프리팹 설정")]
    public GameObject[] boyPrefabs;
    public GameObject[] girlPrefabs;

    [Header("스폰 위치 범위")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -1f;
    public float maxY = 2f;

    [Header("수량 설정")]
    public int minGirlCount = 3;
    public int maxGirlCount = 5;

    void Start()
    {
        SpawnAll();
    }

    void SpawnAll()
    {
        int spawnCount = Random.Range(minGirlCount, maxGirlCount + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), transform.position.z);
            GameObject girl = Instantiate(girlPrefabs[Random.Range(0, girlPrefabs.Length)], spawnPos, Quaternion.identity, transform);

            // 남학생 생성 로직을 즉시 실행
            StartCoroutine(ManagePair(girl.transform));
        }
    }

    private IEnumerator ManagePair(Transform girl)
    {
        // 1. 첫 생성은 즉시 (딜레이 없음)
        GameObject boy = Instantiate(boyPrefabs[Random.Range(0, boyPrefabs.Length)], girl.position + new Vector3(1.5f, 0, 0), Quaternion.identity);
        boy.tag = "NPC";

        // 2. 이후 남학생이 죽었을 때만 3초 딜레이 후 재생성
        while (girl != null)
        {
            if (boy == null)
            {
                yield return new WaitForSeconds(3f);

                if (girl != null)
                {
                    Vector3 spawnPos = girl.position + new Vector3(1.5f, 0, 0);
                    boy = Instantiate(boyPrefabs[Random.Range(0, boyPrefabs.Length)], spawnPos, Quaternion.identity);
                    boy.tag = "NPC";
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}