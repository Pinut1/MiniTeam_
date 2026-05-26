using MiniTeam.Tetris;
using UnityEngine;

public class TamamaBeamTester : MonoBehaviour
{
    void Update()
    {
        // 키보드 숫자 0~9 입력 감지
        for (int i = 0; i <= 9; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;

            if (Input.GetKeyDown(key))
            {
                FireTestBeam(i);
            }
        }
    }

    private void FireTestBeam(int number)
    {
        // 1. 발사 좌표 설정
        Vector3 spawnPosition = new Vector3(number, 1, 0);

        // 2. 컨트롤러를 통해 임팩트 발동 요청 (빔 생성 및 컷씬 포함)
        if (TetrisGameController.Instance != null)
        {
            TetrisGameController.Instance.OnTamamaImpactTriggered(true, spawnPosition, Quaternion.identity, (float)number);
            Debug.Log($"[테스트 빔 요청] 키: {number} | 요청 위치: {spawnPosition} | 타격 거리: {number}");
        }
        else
        {
            Debug.LogError("오류: TetrisGameController 인스턴스를 찾을 수 없습니다!");
        }
    }
}
