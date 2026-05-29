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
    public float duration; // ǥ                                          (0                ǥ           )
}

public class CharacterReactionUI : MonoBehaviour
{
    public static CharacterReactionUI Instance;
    [Header("UI     ")]
    public Image characterImage; // ȭ 鿡       ĳ     UI  ̹          Ʈ

    [Header("ǥ              ")]
    public List<ReactionSprite> reactions; //  ν    Ϳ          ǥ       Ʈ

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
        //               ⺻ ǥ           
        ShowReaction(ReactionType.Idle);
       
    }
    
    //  ܺο    ǥ    ٲ          θ   Լ 
    // ܺο ǥ ٲ  θ Լ
    public void ShowReaction(ReactionType type)
    {
        float targetDuration = 0f;

        foreach (var reaction in reactions)
        {
            if (reaction.reactionType == type)
            {
                characterImage.sprite = reaction.characterSprite;
                targetDuration = reaction.duration;
                break;
            }
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
