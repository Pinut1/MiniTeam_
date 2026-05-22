using MiniTeam.Tetris;
using UnityEngine;

public class TamamaBeamTester : MonoBehaviour
{
    [Header("테스트용 빔 프리팹 연결")]
    public GameObject tamamaBeamPrefab;

    void Update()
    {
        // 키보드 숫자 0~9 (키보드 위쪽 숫자키) 입력 감지
        for (int i = 0; i <= 9; i++)
        {
            // KeyCode.Alpha1 은 숫자 1, Alpha9는 숫자 9
            KeyCode key = KeyCode.Alpha0 + i;

            if (Input.GetKeyDown(key))
            {
                FireTestBeam(i);
            }
        }
    }

    private void FireTestBeam(int number)
    {
        if (tamamaBeamPrefab == null)
        {
            Debug.LogError("에러: 인스펙터에 타마마 빔 프리팹을 안 넣었습니다!");
            return;
        }

        // 1. 발사 좌표 세팅: 숫자 입력값에 따라 (1,1,0), (2,1,0)... 으로 설정
        Vector3 spawnPosition = new Vector3(number, 1, 0);

        // 2. 프리팹 생성
        GameObject beamObj = Instantiate(tamamaBeamPrefab, spawnPosition, Quaternion.identity);

        // 3. Setup 함수 호출 (distance에 숫자값 그대로 전달)
        if (beamObj.TryGetComponent(out TamamaBeam beamScript))
        {
            beamScript.Setup(number);
            TetrisGameController.Instance.OnTamamaImpactTriggered(true);
            Debug.Log($"[테스트 빔 발사] 키: {number} | 스폰 위치: {spawnPosition} | 타격 거리: {number}");
        }
        else
        {
            Debug.LogError("에러: 프리팹에 TamamaBeam 스크립트가 없습니다!");
        }
    }
}