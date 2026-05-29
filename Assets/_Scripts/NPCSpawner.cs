using UnityEngine;
using System.Collections;

public class NpcSpawner : MonoBehaviour
{
    [Header("오프닝 제어")]
    public bool isSpawningPaused = false; // 이 값이 true면 스폰을 멈춤

    [Header("프리팹 설정")]
    public GameObject[] boyPrefabs;
    public GameObject[] girlPrefabs;

    [Header("스폰 위치 범위")]
    public float minX = -14f;
    public float maxX = 35f;
    public float minY = 0f;
    public float maxY = 0.5f;

    [Header("수량 설정")]
    public int minGirlCount = 7;
    public int maxGirlCount = 11;

    [Header("리스폰 딜레이 시간 설정")]
    public float normalDelay = 3f; // 하트를 다 모았을 때의 기본 대기 시간
    public float fastDelay = 0.5f; // 하트가 부족할 때의 빠른 대기 시간

    [Header("하트 매니저 연결")]
    // 인스펙터 창에서 하이어라키의 HeartManager 오브젝트를 끌어다 넣으세요.
    public HeartUIManager heartManager;

    // ★★★ [새로 추가] 바닥 Y 위치 설정 ★★★
    [Header("바닥 레벨 설정")]
    [Tooltip("남자 NPC가 무조건 생성되어야 하는 바닥의 Y 위치 값")]
    public float floorY = 0f;

    IEnumerator Start()
    {
        // 오프닝 중이면 끝날 때까지 스폰 대기
        yield return new WaitUntil(() => !isSpawningPaused);
        
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
        // 1. 첫 생성은 즉시 (★첫 생성 위치도 바닥으로 고정★)
        Vector3 firstSpawnPos = new Vector3(girl.position.x + 1.5f, floorY, transform.position.z);
        GameObject boy = Instantiate(boyPrefabs[Random.Range(0, boyPrefabs.Length)], firstSpawnPos, Quaternion.identity);
        boy.tag = "NPC";

        // 2. 남자 NPC가 사라졌을 때 조건에 따라 재생성
        while (girl != null)
        {
            if (boy == null)
            {
                bool isHeartFull = false;

                // 연결된 HeartUIManager가 있는지 확인
                if (heartManager != null)
                {
                    // ★주의: 아래 코드는 HeartUIManager 스크립트에 있는 실제 변수명으로 변경해야 합니다!★
                    // 예시: heartManager.currentHearts >= 7 (현재 하트가 7개 이상인지 확인)

                    // isHeartFull = heartManager.currentHearts >= 7; 
                }
                else
                {
                    Debug.LogWarning("NpcSpawner에 HeartUIManager가 연결되지 않았습니다!");
                }

                // 하트 상태에 따른 딜레이
                float waitTime = isHeartFull ? normalDelay : fastDelay;

                yield return new WaitForSeconds(waitTime);

                // 기다린 후에도 다시 한 번 오프닝 중인지 확인
                yield return new WaitUntil(() => !isSpawningPaused);

                if (girl != null)
                {
                    // 화면 밖 스폰 X좌표 계산
                    float spawnX = girl.position.x + 1.5f;
                    if (Camera.main != null)
                    {
                        float camX = Camera.main.transform.position.x;
                        // 화면 반너비 + 약간의 여유(2f)
                        float camHalfWidth = (Camera.main.orthographicSize * Camera.main.aspect) + 2f;
                        
                        // 스폰 위치가 화면 안에 있다면 화면 밖으로 밀어냄
                        if (Mathf.Abs(spawnX - camX) < camHalfWidth)
                        {
                            float rightOut = camX + camHalfWidth;
                            float leftOut = camX - camHalfWidth;

                            // 화면 오른쪽 밖이 맵 범위 내라면 오른쪽 스폰, 안되면 왼쪽 스폰
                            if (rightOut <= maxX)
                            {
                                spawnX = rightOut;
                            }
                            else if (leftOut >= minX)
                            {
                                spawnX = leftOut;
                            }
                        }
                    }

                    // 바닥에 소년 스폰
                    Vector3 spawnPos = new Vector3(spawnX, floorY, transform.position.z);
                    boy = Instantiate(boyPrefabs[Random.Range(0, boyPrefabs.Length)], spawnPos, Quaternion.identity);
                    boy.tag = "NPC";
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}