using UnityEngine;

public class NPCMove : MonoBehaviour
{
    public float moveSpeed = 2f;

    // 이동 범위
    public float minX;
    public float maxX;

    float moveDirection = 1f;

    Animator anim;
    SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        ChangeDirection();

        InvokeRepeating(nameof(ChangeDirection), 2f, 2f);
    }

    void Update()
    {
        transform.Translate(Vector2.right * moveDirection * moveSpeed * Time.deltaTime);

        // 범위 제한
        if (transform.position.x <= minX)
        {
            moveDirection = 1;
        }
        else if (transform.position.x >= maxX)
        {
            moveDirection = -1;
        }

        // 방향 뒤집기
        if (moveDirection > 0)
            sr.flipX = false;
        else
            sr.flipX = true;

        // 걷기 애니메이션
        if (anim != null)
        {
            anim.SetBool("Walk", true);
        }
    }

    void ChangeDirection()
    {
        moveDirection = Random.Range(0, 2) == 0 ? -1 : 1;
    }
}