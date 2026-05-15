using System;
using UnityEngine;

public class LineClearEventManager : MonoBehaviour
{
    public static LineClearEventManager Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }

    public void ProcessLineClear(int linesCleared)
    {
        if (linesCleared < 3) return;

        if(linesCleared == 3)
        {
            TriggerTripleEffect();
        }

        else if(linesCleared == 4)
        {
            TriggerTetrisEffect();
        }
    }


    private void TriggerTripleEffect()
    {
        Debug.Log("3줄 지움");
    }
    private void TriggerTetrisEffect()
    {
        Debug.Log("4줄 지움");
    }
}
