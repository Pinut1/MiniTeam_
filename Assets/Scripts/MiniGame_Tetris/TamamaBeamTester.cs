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
        // 발사 좌표 및 거리 설정
        Vector3 spawnPosition = new Vector3(number, 7, 0);

        // 컨트롤러를 통해 임팩트 발동 요청
        if (TetrisGameController.Instance != null)
        {
            TetrisGameController.Instance.OnTamamaImpactTriggered(true, spawnPosition, Quaternion.identity, (float)number);
        }
        else
        {
            Debug.LogError("오류: TetrisGameController 인스턴스를 찾을 수 없습니다!");
        }
    }
}
