using UnityEngine;

public class TamamaBeam : MonoBehaviour
{
    [Header("빔 구성 요소 연결")]
    public Transform beamBody;
    public Transform beamEnd;

    [Header("연출 세팅")]
    public float destroyTime = 0.5f;

    /// <summary>
    /// 발사 지점으로부터 벽까지의 거리를 받아 빔의 길이와 끝단 위치를 세팅합니다.
    /// </summary>
    /// <param name="distance">발사 지점(블록)에서 왼쪽 벽까지의 거리</param>
    public void Setup(float distance)
    {
        // 1. 기둥 세팅
        // 중심을 기준으로 양쪽으로 늘어나기 때문에 벽과 발사 지점의 절반(-distance / 2) 위치로 옮겨서 스케일을 늘림
        beamBody.localPosition = new Vector3(-distance / 2f, 0, 0);
        beamBody.localScale = new Vector3(distance, 1, 1);

        // 2. 끝단 세팅
        // 정확히 거리만큼 떨어진 왼쪽 벽 (-dsitance) 위치에 폭발 이미지를 배치
        beamEnd.localPosition = new Vector3(-distance, 0, 0);

        // 3. 수명이 다하면 자동 파괴
        Destroy(gameObject, destroyTime);
    }
}
