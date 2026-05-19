using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum ReactionType
{
    Idle,
    Impact_fail,
    Impact_success,
    game_clear,
    game_fail
}

[System.Serializable]
public struct ReactionSprite
{
    public ReactionType reactionType;
    public Sprite characterSprite;
    public float duration; // 표정을 몇 초 동안 유지할 것인지에 대한 변수 (0으로 둘 시 다음 표정까지 유지)
}

public class CharacterReactionUI : MonoBehaviour
{
    public static CharacterReactionUI Instance;
    [Header("UI 연결")]
    public Image characterImage; // 화면에 띄워둔 캐릭터 UI 이미지 컴포넌트

    [Header("표정 데이터 세팅")]
    public List<ReactionSprite> reactions; // 인스펙터에서 등록할 표정 리스트

    private Coroutine resetCoroutine;

    private void Awake()
    {
        if (Instance !=null)
        {
            Destroy(gameObject);
        }
        else
            Instance = this;
    }

    private void Start()
    {
        // 게임 시작 시 기본 표정으로 세팅
        ShowReaction(ReactionType.Idle);
       
    }
    
    // 외부에서 표정 바꿀 때마다 부를 함수
    public void ShowReaction(ReactionType type)
    {
        float targetDuration = 0f;

        foreach (var reaction in reactions)
        {
            characterImage.sprite = reaction.characterSprite;
            targetDuration = reaction.duration;
            break;
        }


        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        if (type != ReactionType.Idle && targetDuration > 0f)
        {
            resetCoroutine = StartCoroutine(ResetToIdleAfterDelay(targetDuration));
        }
    }
     
    private IEnumerator ResetToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowReaction(ReactionType.Idle);
    }
}
