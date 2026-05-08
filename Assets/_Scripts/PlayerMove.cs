using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float walkSpeed = 2f;   // 최소 속도
    public float runSpeed = 8f;    // 최대 속도
    public float maxDistance = 10f; // 이 거리 이상이면 최고 속도

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        float distance = mousePos.x - transform.position.x;
        float absDistance = Mathf.Abs(distance);

        float direction = 0f;

        if (absDistance > 0.1f)
        {
            direction = Mathf.Sign(distance);
        }

        // 거리 기반 속도 보간 (핵심)
        float t = Mathf.Clamp01(absDistance / maxDistance);
        float speed = Mathf.Lerp(walkSpeed, runSpeed, t);

        // 이동
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // 방향 전환
        if (direction != 0)
        {
            sr.flipX = (direction < 0);
        }
    }
}