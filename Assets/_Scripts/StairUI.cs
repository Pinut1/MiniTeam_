using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StairUI : MonoBehaviour
{
    public GameObject upButton;

    public Image fadePanel;
    public float fadeSpeed = 2f;

    void Start()
    {
        upButton.SetActive(false);

        Color color = fadePanel.color;
        color.a = 0;
        fadePanel.color = color;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            upButton.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            upButton.SetActive(false);
        }
    }

    // 버튼에서 호출할 함수
    public void MoveFloor()
    {
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        Color color = fadePanel.color;

        // 화면 검게
        while (color.a < 1)
        {
            color.a += Time.deltaTime * fadeSpeed;
            fadePanel.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // 여기서 층 이동 처리 가능

        // 다시 밝게
        while (color.a > 0)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            fadePanel.color = color;
            yield return null;
        }
    }
}